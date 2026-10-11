/*
  ファイル名 / File: MainWindow.xaml.cs
  概要 / Description: Loads, saves, renders, and safely previews Markdown documents
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Ganss.Xss;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;

namespace MdEditor;

public partial class MainWindow : Window
{
    private readonly string? _initialPath;
    private readonly string? _initialAnchor;
    private readonly MarkdownPipeline _markdownPipeline =
        // GitHub-style heading ids keep Japanese text, so links like [x](#セットアップ) work.
        new MarkdownPipelineBuilder()
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseAdvancedExtensions()
            .Build();
    private readonly HtmlSanitizer _htmlSanitizer = new();
    private readonly DispatcherTimer _previewTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(200)
    };
    private const string DocumentHost = "document.mdeditor.local";
    private static readonly UTF8Encoding Utf8NoBom = new(false);
    private string _text = string.Empty;
    private string? _filePath;
    // Encoding the document was read with, so saving does not silently convert it.
    private Encoding _encoding = Utf8NoBom;
    // Incremented on every edit; a save only clears the dirty flag if no edit
    // arrived while it was writing.
    private long _changeVersion;
    private bool _isDirty;
    private int _previewRequestId;
    private bool _pageReady;
    private bool _allowClose;
    private bool _closePromptActive;
    private bool _pdfExportActive;
    private bool _windowFullscreen;
    private TaskCompletionSource? _previewRendered;
    private WindowState _windowedState;
    private WindowStyle _windowedStyle;
    private ResizeMode _windowedResizeMode;
    private Rect _windowedBounds;

    private const int DwmwaUseImmersiveDarkMode = 20;
    private const string SettingsKeyPath = @"Software\mdEditor";
    // "auto", "light", or "dark". Kept in the registry so it applies before the page paints.
    private string _theme = NormalizeTheme(LoadSetting("Theme"));
    // PDF paper: "auto" (A4, or Letter where the region uses US customary units), "a4", or "letter".
    private string _paper = NormalizePaper(LoadSetting("PaperSize"));
    // How _encoding was decided: "detected" (BOM or valid UTF-8), "guessed"
    // (system code page fallback), or "chosen" by the user.
    private string _encodingSource = "detected";
    // True when some bytes could not be read in _encoding and became U+FFFD.
    private bool _hasUndecodableBytes;
    // Crash recovery: unsaved text is copied to LocalAppData (never over the
    // document itself) and offered back at the next start if the app ended abnormally.
    private static readonly string RecoveryFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mdEditor", "Recovery");
    private static readonly long ProcessStartTicks = Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks;
    private readonly string _recoveryFile = Path.Combine(RecoveryFolder, $"recovery-{Environment.ProcessId}.json");
    private readonly DispatcherTimer _backupTimer = new() { Interval = TimeSpan.FromSeconds(15) };
    private bool _autoBackup = LoadSetting("AutoBackup") != "off";
    private long _backupVersion = -1;

    private sealed record RecoveryRecord(
        int ProcessId, long ProcessStartTicks, string? Path, string Encoding, string Text, DateTime SavedAt);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public MainWindow(string? initialPath, string? initialAnchor = null)
    {
        InitializeComponent();
        _initialPath = initialPath;
        _initialAnchor = initialAnchor;
        ApplyTheme();
        SourceInitialized += (_, _) => ApplyTheme();
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        Closed += (_, _) => SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            SendPreview();
        };
        _backupTimer.Tick += (_, _) => WriteRecoveryCopy();
        _backupTimer.Start();
        // A normal exit (saved, or changes deliberately discarded) leaves no copy behind.
        Closed += (_, _) => DeleteRecoveryCopy();

        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedTags.UnionWith(
        [
            "a", "blockquote", "br", "code", "del", "div", "em", "h1", "h2", "h3", "h4", "h5",
            "h6", "hr", "img", "input", "li", "ol", "p", "pre", "span", "strong", "table", "tbody",
            "td", "th", "thead", "tr", "ul"
        ]);
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedAttributes.UnionWith(
        [
            "alt", "checked", "class", "colspan", "data-line", "disabled", "href", "id", "rowspan",
            "src", "style", "title", "type"
        ]);
        // Markdig writes table alignment as style="text-align: ..."; nothing else is kept.
        _htmlSanitizer.AllowedCssProperties.Clear();
        _htmlSanitizer.AllowedCssProperties.Add("text-align");
        _htmlSanitizer.AllowedSchemes.Clear();
        _htmlSanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
        // Task-list items are the only inputs Markdig emits; keep them read-only checkboxes.
        _htmlSanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is AngleSharp.Dom.IElement { TagName: "INPUT" } input)
            {
                if (!string.Equals(input.GetAttribute("type"), "checkbox", StringComparison.OrdinalIgnoreCase))
                {
                    input.Remove();
                }
                else
                {
                    input.SetAttribute("disabled", "disabled");
                }
            }
        };
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // The default data folder sits beside the exe, which is read-only for
            // installed (MSIX or Program Files) copies, so keep it in LocalAppData.
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "mdEditor",
                "WebView2");
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await EditorWebView.EnsureCoreWebView2Async(environment);
        }
        catch (WebView2RuntimeNotFoundException ex)
        {
            ShowStartupError(Strings.Tr("Microsoft Edge WebView2 Runtime が必要です。Microsoft のサイトから「WebView2 Runtime」をインストールしてから、もう一度起動してください。"), ex);
            return;
        }
        catch (Exception ex) when (ex is COMException or IOException or UnauthorizedAccessException
                                       or InvalidOperationException or ArgumentException)
        {
            ShowStartupError(Strings.Tr("画面の表示機能（WebView2）を初期化できませんでした。PC を再起動してから、もう一度起動してください。"), ex);
            return;
        }

        EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        // Zoom (Ctrl + wheel, Ctrl + plus/minus, or the settings dialog) is kept between launches.
        EditorWebView.ZoomFactor = LoadZoom();
        EditorWebView.ZoomFactorChanged += (_, _) => OnZoomChanged();
        ApplyTheme();
        EditorWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "mdeditor.local",
            Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            CoreWebView2HostResourceAccessKind.DenyCors);
        EditorWebView.CoreWebView2.WebMessageReceived += WebMessageReceived;
        EditorWebView.CoreWebView2.NavigationStarting += NavigationStarting;
        EditorWebView.CoreWebView2.AddWebResourceRequestedFilter(
            $"https://{DocumentHost}/*", CoreWebView2WebResourceContext.Image);
        EditorWebView.CoreWebView2.WebResourceRequested += DocumentResourceRequested;
        EditorWebView.Source = new Uri("https://mdeditor.local/index.html");
    }

    private async void WebMessageReceived(
        object? sender,
        CoreWebView2WebMessageReceivedEventArgs e)
    {
        using var message = JsonDocument.Parse(e.WebMessageAsJson);
        var root = message.RootElement;
        var type = root.GetProperty("type").GetString();

        switch (type)
        {
            case "ready":
                _pageReady = true;
                Strings.Language = Strings.ForcedLanguage
                    ?? (root.TryGetProperty("lang", out var lang) && lang.GetString() == "en"
                        ? "en"
                        : "ja");
                if (Strings.ForcedLanguage is not null)
                {
                    SendToPage(new { type = "language", lang = Strings.ForcedLanguage });
                }
                SendToPage(new { type = "theme", theme = _theme });
                SendPaper();
                SendZoom();
                SendToPage(new { type = "autoBackup", enabled = _autoBackup });
                if (await OfferRecoveryAsync())
                {
                    // The recovered document is shown instead of the file passed at start.
                }
                else if (!string.IsNullOrWhiteSpace(_initialPath))
                {
                    await LoadDocumentAsync(_initialPath);
                    if (!string.IsNullOrEmpty(_initialAnchor))
                    {
                        SendToPage(new { type = "scrollToAnchor", name = _initialAnchor });
                    }
                }
                else
                {
                    SendDocumentInfo();
                    SendPreview();
                }
                break;
            case "setTheme":
                _theme = NormalizeTheme(root.GetProperty("theme").GetString());
                SaveSetting("Theme", _theme);
                ApplyTheme();
                break;
            case "setAutoBackup":
                _autoBackup = root.GetProperty("enabled").GetBoolean();
                SaveSetting("AutoBackup", _autoBackup ? "on" : "off");
                if (_autoBackup)
                {
                    WriteRecoveryCopy();
                }
                else
                {
                    DeleteRecoveryCopy();
                }
                break;
            case "zoom":
                ChangeZoom(root.GetProperty("action").GetString());
                break;
            case "setPaper":
                _paper = NormalizePaper(root.GetProperty("paper").GetString());
                SaveSetting("PaperSize", _paper);
                SendPaper();
                break;
            case "reopenWithEncoding":
                await ReopenWithEncodingAsync(root.GetProperty("encoding").GetString());
                break;
            case "setSaveEncoding":
                SetSaveEncoding(root.GetProperty("encoding").GetString());
                break;
            case "setLanguage":
                Strings.Language = root.GetProperty("lang").GetString() == "en" ? "en" : "ja";
                SendDocumentInfo();
                break;
            case "dropFile":
                await HandleDropAsync(e);
                break;
            case "change":
                _text = root.GetProperty("text").GetString() ?? string.Empty;
                _changeVersion++;
                _isDirty = true;
                SendDocumentInfo();
                _previewTimer.Stop();
                _previewTimer.Start();
                break;
            case "open":
                await OpenDocumentAsync();
                break;
            case "save":
                await SaveDocumentAsync();
                break;
            case "saveAs":
                await SaveDocumentAsAsync();
                break;
            case "externalLink":
                OpenExternalLink(root.GetProperty("url").GetString());
                break;
            case "documentLink":
                OpenDocumentLink(root.GetProperty("href").GetString());
                break;
            case "toggleWindowFullscreen":
                SetWindowFullscreen(!_windowFullscreen);
                break;
            case "exitWindowFullscreen":
                SetWindowFullscreen(false);
                break;
            case "previewRendered":
                // Only the render that the PDF export asked for counts.
                if (root.TryGetProperty("requestId", out var requestId)
                    && requestId.ValueKind == JsonValueKind.Number
                    && requestId.GetInt32() == _previewRequestId)
                {
                    _previewRendered?.TrySetResult();
                }
                break;
            case "exportPdf":
                await ExportPdfAsync();
                break;
        }
    }

    private void NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri)
            && uri.Scheme is "https" or "http"
            && !string.Equals(uri.Host, "mdeditor.local", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            OpenExternalLink(uri.AbsoluteUri);
            return;
        }

        // Only the app page itself may load; any other address (such as a relative
        // link resolved against mdeditor.local) would leave an error page with no way back.
        if (!e.Uri.StartsWith("https://mdeditor.local/index.html", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
        }
    }

    // A relative link in the document (other.md, ./LICENSE, docs/, other.md#heading):
    // Markdown and text open in a new mdEditor window, safe documents and media in
    // their usual app, and anything else is only shown in File Explorer.
    private void OpenDocumentLink(string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return;
        }

        if (_filePath is null)
        {
            MessageBox.Show(
                Strings.Tr("保存していない文書では、ほかのファイルへのリンクを開けません。先に文書を保存してください。"),
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var fragmentIndex = href.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? href[(fragmentIndex + 1)..] : string.Empty;
        var pathPart = fragmentIndex >= 0 ? href[..fragmentIndex] : href;
        var queryIndex = pathPart.IndexOf('?');
        if (queryIndex >= 0)
        {
            pathPart = pathPart[..queryIndex];
        }

        string fullPath;
        try
        {
            var relative = Uri.UnescapeDataString(pathPart).Replace('/', Path.DirectorySeparatorChar);
            fullPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(_filePath)!,
                relative.TrimStart(Path.DirectorySeparatorChar)));
            fragment = Uri.UnescapeDataString(fragment);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            ShowLinkNotFound(pathPart);
            return;
        }

        if (string.Equals(fullPath, Path.GetFullPath(_filePath), StringComparison.OrdinalIgnoreCase))
        {
            if (fragment.Length > 0)
            {
                SendToPage(new { type = "scrollToAnchor", name = fragment });
            }

            return;
        }

        if (Directory.Exists(fullPath))
        {
            StartExplorer($"\"{fullPath}\"");
            return;
        }

        if (!File.Exists(fullPath))
        {
            ShowLinkNotFound(fullPath);
            return;
        }

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        if (extension is "" or ".md" or ".markdown" or ".txt" or ".text")
        {
            // Markdown and plain text (such as LICENSE) open in a new mdEditor
            // window, so this document stays open where it was.
            OpenInNewWindow(fullPath, fragment);
        }
        else if (SafeToOpenExtensions.Contains(extension))
        {
            StartShell(fullPath);
        }
        else
        {
            // Programs, scripts, and unknown types are never run from a link.
            StartExplorer($"/select,\"{fullPath}\"");
        }
    }

    // Documents and media that open in their usual app from a link. Nothing here
    // can run code by being opened (no executables, scripts, or macro documents).
    private static readonly HashSet<string> SafeToOpenExtensions =
    [
        ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".svg", ".ico",
        ".csv", ".tsv", ".json", ".xml", ".yaml", ".yml", ".log", ".docx", ".xlsx", ".pptx",
        ".mp3", ".wav", ".mp4", ".webm"
    ];

    private void OpenInNewWindow(string path, string anchor)
    {
        var exe = Environment.ProcessPath;
        if (exe is null)
        {
            return;
        }

        var start = new ProcessStartInfo(exe) { UseShellExecute = false };
        start.ArgumentList.Add(path);
        if (anchor.Length > 0)
        {
            start.ArgumentList.Add("--anchor");
            start.ArgumentList.Add(anchor);
        }

        if (Strings.ForcedLanguage is not null)
        {
            start.ArgumentList.Add("--lang");
            start.ArgumentList.Add(Strings.ForcedLanguage);
        }

        try
        {
            Process.Start(start);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ShowLinkError(ex);
        }
    }

    private static void StartShell(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ShowLinkError(ex);
        }
    }

    private static void ShowLinkError(Exception ex)
    {
        MessageBox.Show(
            $"{Strings.Tr("リンクを開けませんでした。リンク先の場所を確かめて、ブラウザーやエクスプローラーから開いてください。")}\n\n{ex.Message}",
            "mdEditor",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void ShowLinkNotFound(string path)
    {
        MessageBox.Show(
            string.Format(Strings.Tr("リンク先のファイルが見つかりません。文書の中のリンクの書き方と、ファイルの場所を確かめてください。\n\n{0}"), path),
            "mdEditor",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private static void StartExplorer(string arguments)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", arguments) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ShowLinkError(ex);
        }
    }

    private async Task HandleDropAsync(CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (!TryGetDroppedMarkdownPath(e, out var filePath))
        {
            MessageBox.Show(
                Strings.Tr("Markdown ファイル（.md または .markdown）を1つだけドロップしてください。"),
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (await ConfirmSaveChangesAsync())
        {
            await LoadDocumentAsync(filePath);
        }
    }

    private static bool TryGetDroppedMarkdownPath(
        CoreWebView2WebMessageReceivedEventArgs e,
        out string filePath)
    {
        filePath = string.Empty;
        var files = e.AdditionalObjects;
        if (files is null
            || files.Count != 1
            || files[0] is not CoreWebView2File file)
        {
            return false;
        }

        var extension = Path.GetExtension(file.Path);
        if (!string.Equals(extension, ".md", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(extension, ".markdown", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        filePath = file.Path;
        return true;
    }

    private async Task OpenDocumentAsync()
    {
        if (!await ConfirmSaveChangesAsync())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = Strings.Tr("Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|すべてのファイル (*.*)|*.*"),
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) == true)
        {
            await LoadDocumentAsync(dialog.FileName);
        }
    }

    private async Task LoadDocumentAsync(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var decoded = DecodeDocument(await File.ReadAllBytesAsync(fullPath));
            _filePath = fullPath;
            ApplyDecodedDocument(decoded);
        }
        catch (FileNotFoundException ex)
        {
            ShowFileError("ファイルが見つかりません。移動・名前の変更・削除がされていないか確かめて、もう一度開いてください", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            ShowFileError("フォルダーが見つかりません。フォルダーがあるか確かめて、別の場所を選んでください", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("ファイルを読み取れません。ほかのアプリで開いている場合は閉じてから、もう一度開いてください", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを読み取れません。ほかのアプリで開いている場合は閉じてから、もう一度開いてください", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルの場所の指定が正しくありません。場所と名前を確かめて、もう一度選んでください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("この形式のファイルの場所には対応していません。PC のドライブか共有フォルダーにあるファイルを選んでください", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません。読み書きできる場所（ドキュメントなど）を選ぶか、ファイルの権限を確かめてください", ex);
        }
    }

    private async Task<bool> SaveDocumentAsync()
    {
        if (string.IsNullOrWhiteSpace(_filePath))
        {
            return await SaveDocumentAsAsync();
        }

        // Overwriting a file that was not read cleanly would replace the unreadable
        // parts of the original with U+FFFD for good.
        if (_hasUndecodableBytes && MessageBox.Show(
                Strings.Tr("この文書には、選んだ文字コードで読めなかった文字（�）が含まれています。上書きすると、元のファイルのその部分は失われます。上書き保存しますか？"),
                "mdEditor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return false;
        }

        try
        {
            return await WriteDocumentAsync(_filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルの場所の指定が正しくありません。場所と名前を確かめて、もう一度選んでください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("この形式のファイルの場所には対応していません。PC のドライブか共有フォルダーにあるファイルを選んでください", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません。読み書きできる場所（ドキュメントなど）を選ぶか、ファイルの権限を確かめてください", ex);
        }

        return false;
    }

    private async Task<bool> SaveDocumentAsAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = Strings.Tr("Markdown ファイル (*.md)|*.md|すべてのファイル (*.*)|*.*"),
            DefaultExt = ".md",
            AddExtension = true,
            FileName = _filePath is null ? Strings.Tr("新しい文書.md") : Path.GetFileName(_filePath)
        };
        if (dialog.ShowDialog(this) != true)
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(dialog.FileName);
            if (!await WriteDocumentAsync(fullPath))
            {
                return false;
            }

            _filePath = fullPath;
            SendDocumentInfo();
            SendPreview();
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルの場所の指定が正しくありません。場所と名前を確かめて、もう一度選んでください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("この形式のファイルの場所には対応していません。PC のドライブか共有フォルダーにあるファイルを選んでください", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません。読み書きできる場所（ドキュメントなど）を選ぶか、ファイルの権限を確かめてください", ex);
        }

        return false;
    }

    // BOMs win; otherwise strict UTF-8, falling back to the system ANSI code page
    // (Shift_JIS on Japanese Windows) for older files.
    private sealed record DecodedDocument(string Text, Encoding Encoding, string Source, bool Lossy);

    private static DecodedDocument DecodeDocument(byte[] bytes)
    {
        ReadOnlySpan<byte> data = bytes;
        if (data.StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]))
        {
            return Decode(bytes, new UTF8Encoding(true), "detected");
        }
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xFE]))
        {
            return Decode(bytes, Encoding.Unicode, "detected");
        }
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFE, 0xFF]))
        {
            return Decode(bytes, Encoding.BigEndianUnicode, "detected");
        }

        var utf8 = Decode(bytes, Utf8NoBom, "detected");
        if (!utf8.Lossy)
        {
            return utf8;
        }

        // Not UTF-8: guess the system code page and tell the user it was a guess.
        var legacy = Encoding.GetEncoding(
            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
        return Decode(bytes, legacy, "guessed");
    }

    // Decodes strictly first so bytes that do not fit the encoding are reported
    // (Lossy) instead of silently turning into "?" and being saved that way.
    private static DecodedDocument Decode(byte[] bytes, Encoding encoding, string source)
    {
        ReadOnlySpan<byte> data = bytes;
        var preamble = encoding.GetPreamble();
        if (preamble.Length > 0 && data.StartsWith(preamble))
        {
            data = data[preamble.Length..];
        }

        var strict = (Encoding)encoding.Clone();
        strict.DecoderFallback = DecoderFallback.ExceptionFallback;
        try
        {
            return new DecodedDocument(strict.GetString(data), encoding, source, false);
        }
        catch (DecoderFallbackException)
        {
            var replacing = (Encoding)encoding.Clone();
            replacing.DecoderFallback = new DecoderReplacementFallback("�");
            return new DecodedDocument(replacing.GetString(data), encoding, source, true);
        }
    }

    private static Encoding? EncodingFromId(string? id) => id switch
    {
        "utf-8" => Utf8NoBom,
        "utf-8-bom" => new UTF8Encoding(true),
        "utf-16le" => Encoding.Unicode,
        "utf-16be" => Encoding.BigEndianUnicode,
        "shift_jis" => Encoding.GetEncoding(932),
        "euc-jp" => Encoding.GetEncoding(51932),
        "iso-2022-jp" => Encoding.GetEncoding(50220),
        "windows-1252" => Encoding.GetEncoding(1252),
        _ => null
    };

    private static string EncodingId(Encoding encoding) => encoding.CodePage switch
    {
        65001 => encoding.GetPreamble().Length > 0 ? "utf-8-bom" : "utf-8",
        1200 => "utf-16le",
        1201 => "utf-16be",
        932 => "shift_jis",
        51932 or 20932 => "euc-jp",
        50220 or 50221 or 50222 => "iso-2022-jp",
        1252 => "windows-1252",
        _ => encoding.WebName
    };

    private static double LoadZoom() =>
        double.TryParse(LoadSetting("Zoom"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var zoom) && zoom >= 0.25 && zoom <= 5
            ? zoom
            : 1.0;

    private void OnZoomChanged()
    {
        SaveSetting("Zoom", EditorWebView.ZoomFactor.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        SendZoom();
    }

    private void SendZoom()
    {
        if (_pageReady)
        {
            SendToPage(new { type = "zoom", factor = EditorWebView.ZoomFactor });
        }
    }

    private static readonly double[] ZoomSteps = [0.5, 0.67, 0.75, 0.8, 0.9, 1, 1.1, 1.25, 1.5, 1.75, 2, 2.5, 3];

    private void ChangeZoom(string? action)
    {
        var current = EditorWebView.ZoomFactor;
        EditorWebView.ZoomFactor = action switch
        {
            "in" => ZoomSteps.FirstOrDefault(step => step > current + 0.001, ZoomSteps[^1]),
            "out" => ZoomSteps.LastOrDefault(step => step < current - 0.001, ZoomSteps[0]),
            _ => 1.0
        };
        // Setting ZoomFactor from code does not raise ZoomFactorChanged.
        OnZoomChanged();
    }

    private void WriteRecoveryCopy()
    {
        if (!_autoBackup || !_isDirty || _backupVersion == _changeVersion)
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(RecoveryFolder);
            var record = new RecoveryRecord(
                Environment.ProcessId, ProcessStartTicks, _filePath, EncodingId(_encoding), _text, DateTime.Now);
            var tempPath = _recoveryFile + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(record), Utf8NoBom);
            File.Move(tempPath, _recoveryFile, overwrite: true);
            _backupVersion = _changeVersion;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            // Recovery is best effort; the document itself is unaffected.
        }
    }

    private void DeleteRecoveryCopy()
    {
        _backupVersion = -1;
        try
        {
            File.Delete(_recoveryFile);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            // A leftover copy is offered at the next start and can be discarded then.
        }
    }

    // Copies left by an mdEditor that is no longer running mean it ended
    // abnormally; copies from another open window are left alone.
    private async Task<bool> OfferRecoveryAsync()
    {
        if (!Directory.Exists(RecoveryFolder))
        {
            return false;
        }

        var candidates = new List<(string File, RecoveryRecord Record)>();
        foreach (var file in Directory.GetFiles(RecoveryFolder, "recovery-*.json"))
        {
            if (string.Equals(file, _recoveryFile, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var record = JsonSerializer.Deserialize<RecoveryRecord>(await File.ReadAllTextAsync(file));
                if (record is not null && !IsStillRunning(record))
                {
                    candidates.Add((file, record));
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                // An unreadable copy is skipped.
            }
        }

        foreach (var (file, record) in candidates.OrderByDescending(item => item.Record.SavedAt))
        {
            var name = record.Path is null ? Strings.Tr("新しい文書.md") : Path.GetFileName(record.Path);
            var answer = MessageBox.Show(
                string.Format(
                    Strings.Tr("前回、保存されないまま終了した文書があります。\n\n文書: {0}\n最後のバックアップ: {1}\n\n復元しますか？\n「いいえ」を選ぶとバックアップを削除します。「キャンセル」を選ぶと、次回の起動時にもう一度確認します。"),
                    name,
                    record.SavedAt.ToString("g")),
                "mdEditor",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (answer == MessageBoxResult.Cancel)
            {
                continue;
            }

            TryDelete(file);
            if (answer != MessageBoxResult.Yes)
            {
                continue;
            }

            _filePath = record.Path;
            _encoding = EncodingFromId(record.Encoding) ?? Utf8NoBom;
            _encodingSource = "detected";
            _hasUndecodableBytes = false;
            _text = record.Text;
            _changeVersion++;
            _isDirty = true;
            SendDocumentInfo();
            SendToPage(new { type = "setText", text = _text });
            SendPreview();
            SendEncodingNotice();
            WriteRecoveryCopy();
            return true;
        }

        return false;
    }

    private static bool IsStillRunning(RecoveryRecord record)
    {
        try
        {
            using var process = Process.GetProcessById(record.ProcessId);
            return Math.Abs(process.StartTime.ToUniversalTime().Ticks - record.ProcessStartTicks)
                < TimeSpan.FromSeconds(2).Ticks;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or Win32Exception)
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            // Left in place; it is offered again next time.
        }
    }

    private void ApplyDecodedDocument(DecodedDocument decoded)
    {
        // A freshly opened document has nothing to recover.
        DeleteRecoveryCopy();
        _text = decoded.Text;
        _encoding = decoded.Encoding;
        _encodingSource = decoded.Source;
        _hasUndecodableBytes = decoded.Lossy;
        _changeVersion++;
        _isDirty = false;
        SendDocumentInfo();
        SendToPage(new { type = "setText", text = _text });
        SendPreview();
        SendEncodingNotice();
    }

    // A guessed or lossy encoding is announced under the toolbar so the user can
    // pick another one before saving over the original file.
    private void SendEncodingNotice()
    {
        var kind = _hasUndecodableBytes ? "lossy" : _encodingSource == "guessed" ? "guessed" : null;
        SendToPage(new { type = "encodingNotice", kind, encoding = EncodingId(_encoding) });
    }

    private async Task ReopenWithEncodingAsync(string? id)
    {
        var encoding = EncodingFromId(id);
        if (encoding is null || _filePath is null)
        {
            return;
        }

        if (_isDirty && MessageBox.Show(
                Strings.Tr("未保存の変更を破棄して、選んだ文字コードで開き直しますか？"),
                "mdEditor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            ApplyDecodedDocument(Decode(await File.ReadAllBytesAsync(_filePath), encoding, "chosen"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            ShowFileError("ファイルを読み取れません。ほかのアプリで開いている場合は閉じてから、もう一度開いてください", ex);
        }
    }

    // Changes only how the next save is written; the text is unchanged.
    private void SetSaveEncoding(string? id)
    {
        var encoding = EncodingFromId(id);
        if (encoding is null || EncodingId(encoding) == EncodingId(_encoding))
        {
            return;
        }

        _encoding = encoding;
        _encodingSource = "chosen";
        _changeVersion++;
        _isDirty = true;
        SendDocumentInfo();
        SendEncodingNotice();
    }

    // Writes to a temporary file beside the target and then swaps it in, so a
    // failure part-way through never leaves a truncated document.
    private async Task<bool> WriteDocumentAsync(string path)
    {
        var version = _changeVersion;
        var text = _text;
        byte[] bytes;
        try
        {
            bytes = EncodeDocument(text, _encoding);
        }
        catch (EncoderFallbackException)
        {
            var answer = MessageBox.Show(
                string.Format(
                    Strings.Tr("この文書には {0} で保存できない文字があります。UTF-8 で保存しますか？"),
                    _encoding.WebName),
                "mdEditor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            _encoding = Utf8NoBom;
            bytes = EncodeDocument(text, _encoding);
        }

        var directory = Path.GetDirectoryName(path) ?? ".";
        var tempPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                             tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                             4096, FileOptions.WriteThrough | FileOptions.Asynchronous))
            {
                await stream.WriteAsync(bytes);
            }

            if (File.Exists(path))
            {
                try
                {
                    File.Replace(tempPath, path, null, ignoreMetadataErrors: true);
                }
                catch (IOException)
                {
                    // Some network shares do not support ReplaceFile.
                    File.Copy(tempPath, path, overwrite: true);
                }
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        if (_changeVersion == version)
        {
            _isDirty = false;
        }
        // What is on disk now is exactly what the editor shows.
        _hasUndecodableBytes = false;
        if (!_isDirty)
        {
            DeleteRecoveryCopy();
        }
        if (_encodingSource == "guessed")
        {
            _encodingSource = "detected";
        }
        SendDocumentInfo();
        SendEncodingNotice();
        return true;
    }

    private static byte[] EncodeDocument(string text, Encoding encoding)
    {
        var strict = (Encoding)encoding.Clone();
        strict.EncoderFallback = EncoderFallback.ExceptionFallback;
        return [.. encoding.GetPreamble(), .. strict.GetBytes(text)];
    }

    private static readonly Dictionary<string, string> ImageContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".webp"] = "image/webp",
            [".avif"] = "image/avif",
            [".bmp"] = "image/bmp",
            [".ico"] = "image/x-icon",
            [".svg"] = "image/svg+xml"
        };

    // Relative images (images/a.png) are requested from DocumentHost and served
    // here from the document's folder: image files only, never above that folder,
    // and only for <img> loads (the filter's resource context), not for scripts.
    private async void DocumentResourceRequested(
        object? sender,
        CoreWebView2WebResourceRequestedEventArgs e)
    {
        var core = EditorWebView.CoreWebView2;
        var folder = _filePath is null ? null : Path.GetDirectoryName(_filePath);
        if (core is null || folder is null || !Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri))
        {
            return;
        }

        using var deferral = e.GetDeferral();
        try
        {
            var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var relative = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/');
            var path = Path.GetFullPath(Path.Combine(root, relative));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                || !ImageContentTypes.TryGetValue(Path.GetExtension(path), out var contentType)
                || !File.Exists(path))
            {
                e.Response = core.Environment.CreateWebResourceResponse(null, 404, "Not Found", "");
                return;
            }

            var bytes = await File.ReadAllBytesAsync(path);
            e.Response = core.Environment.CreateWebResourceResponse(
                new MemoryStream(bytes), 200, "OK", $"Content-Type: {contentType}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                       or ArgumentException or NotSupportedException
                                       or SecurityException)
        {
            e.Response = core.Environment.CreateWebResourceResponse(null, 404, "Not Found", "");
        }
    }

    private async Task ExportPdfAsync()
    {
        if (_pdfExportActive)
        {
            return;
        }

        _pdfExportActive = true;
        try
        {
            SendToPage(new
            {
                type = "pdfExportStatus",
                message = Strings.Tr("PDFの保存先を選択してください")
            });
            var defaultName = _filePath is null
                ? Strings.Tr("新しい文書.pdf")
                : Path.ChangeExtension(_filePath, ".pdf");
            var dialog = new SaveFileDialog
            {
                Filter = Strings.Tr("PDF ファイル (*.pdf)|*.pdf"),
                DefaultExt = ".pdf",
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = Path.GetFileName(defaultName)
            };
            Activate();
            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            _previewTimer.Stop();
            var settings = EditorWebView.CoreWebView2.Environment.CreatePrintSettings();
            settings.MediaSize = CoreWebView2PrintMediaSize.Custom;
            // The paper button chooses A4 or Letter; "auto" follows the region.
            var useLetter = (_paper == "auto" ? RegionPaper() : _paper) == "letter";
            settings.PageWidth = useLetter ? 8.5 : 8.27;
            settings.PageHeight = useLetter ? 11 : 11.69;
            settings.Orientation = CoreWebView2PrintOrientation.Portrait;
            settings.ColorMode = CoreWebView2PrintColorMode.Color;
            settings.MarginTop = 0.59;
            settings.MarginBottom = 0.59;
            settings.MarginLeft = 0.59;
            settings.MarginRight = 0.59;
            settings.ShouldPrintBackgrounds = true;
            settings.ShouldPrintHeaderAndFooter = false;

            bool saved;
            string? tempPath = null;
            try
            {
                // Diagrams and math are drawn by the page, so wait until it reports the preview is complete.
                var rendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                _previewRendered = rendered;
                // PDFs always use the day palette, so diagrams are redrawn in the light theme.
                SendPreview(notifyRendered: true, forPrint: true);
                var finished = await Task.WhenAny(rendered.Task, Task.Delay(TimeSpan.FromSeconds(15)))
                               == rendered.Task;
                _previewRendered = null;
                if (!finished && MessageBox.Show(
                        Strings.Tr("図や数式の描画が15秒以内に終わりませんでした。このままPDFに書き出しますか？"),
                        Strings.Tr("PDFの書き出し"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
                await EditorWebView.CoreWebView2.ExecuteScriptAsync(
                    "document.body.classList.add('printing-preview')");
                // WebView2 fills the PDF page margins with the control's default
                // background (cream, or dark green in dark mode), not with CSS.
                EditorWebView.DefaultBackgroundColor = System.Drawing.Color.White;
                // Print beside the target first so a failure never damages an existing PDF.
                tempPath = Path.Combine(
                    Path.GetDirectoryName(Path.GetFullPath(dialog.FileName))!,
                    $".mdEditor-{Guid.NewGuid():N}.tmp");
                saved = await EditorWebView.CoreWebView2.PrintToPdfAsync(
                    tempPath,
                    settings);
                if (saved)
                {
                    File.Move(tempPath, dialog.FileName, overwrite: true);
                }
            }
            finally
            {
                if (tempPath is not null && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }

                // Restoring the page must not hide the real error or leave the export flag stuck.
                try
                {
                    ApplyTheme();
                    await EditorWebView.CoreWebView2.ExecuteScriptAsync(
                        "document.body.classList.remove('printing-preview')");
                    SendPreview();
                }
                catch (Exception ex) when (ex is COMException or InvalidOperationException or ObjectDisposedException)
                {
                }
            }

            if (!saved)
            {
                MessageBox.Show(
                    Strings.Tr("PDFを書き出せませんでした。ほかのPDF書き出しが完了してから、もう一度お試しください。"),
                    Strings.Tr("PDFの書き出し"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    $"{Strings.Tr("PDFを保存しました。")}\n\n{dialog.FileName}",
                    Strings.Tr("PDFの書き出し"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("PDFを書き込めません。同じ名前の PDF をほかのアプリで開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("PDFを書き込めません。同じ名前の PDF をほかのアプリで開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("PDFの保存先の指定が正しくありません。保存先と名前を確かめて、もう一度選んでください", ex);
        }
        catch (COMException ex)
        {
            ShowFileError("PDFを書き出せません。少し待ってから、もう一度お試しください", ex);
        }
        catch (InvalidOperationException ex)
        {
            ShowFileError("PDFの書き出しを開始できません。mdEditor を再起動してから、もう一度お試しください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("この環境ではPDFを書き出せません。WebView2 Runtime を最新にしてから、もう一度お試しください", ex);
        }
        finally
        {
            _pdfExportActive = false;
            SendDocumentInfo();
        }
    }

    private static bool IsSystemDarkTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int value && value == 0;
    }

    private static string NormalizeTheme(string? theme) =>
        theme is "light" or "dark" ? theme : "auto";

    private static string NormalizePaper(string? paper) =>
        paper is "a4" or "letter" ? paper : "auto";

    private static string RegionPaper() =>
        System.Globalization.RegionInfo.CurrentRegion.IsMetric ? "a4" : "letter";

    private void SendPaper()
    {
        if (_pageReady)
        {
            SendToPage(new { type = "paper", paper = _paper, autoPaper = RegionPaper() });
        }
    }

    private static string? LoadSetting(string name)
    {
        using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
        return key?.GetValue(name) as string;
    }

    private static void SaveSetting(string name, string value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
            key.SetValue(name, value);
        }
        catch (UnauthorizedAccessException)
        {
            // The choice still applies to this session.
        }
        catch (IOException)
        {
            // The choice still applies to this session.
        }
        catch (SecurityException)
        {
            // The choice still applies to this session.
        }
    }

    // The page's prefers-color-scheme comes from the WebView2 profile setting, so
    // the CSS night palette, logos, and Mermaid theme switch with it. The title bar
    // and the background shown before the page paints follow the same choice.
    private void ApplyTheme()
    {
        if (EditorWebView.CoreWebView2 is { } core)
        {
            core.Profile.PreferredColorScheme = _theme switch
            {
                "light" => CoreWebView2PreferredColorScheme.Light,
                "dark" => CoreWebView2PreferredColorScheme.Dark,
                _ => CoreWebView2PreferredColorScheme.Auto
            };
        }

        var dark = _theme == "dark" || (_theme == "auto" && IsSystemDarkTheme());
        // Night paper #1A2212 or day cream #FFF3D6, matching the page background.
        var background = dark ? Color.FromRgb(0x1A, 0x22, 0x12) : Color.FromRgb(0xFF, 0xF3, 0xD6);
        Background = new SolidColorBrush(background);
        EditorWebView.DefaultBackgroundColor =
            System.Drawing.Color.FromArgb(background.R, background.G, background.B);

        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
        {
            var useDarkMode = dark ? 1 : 0;
            DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkMode, ref useDarkMode, sizeof(int));
        }
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
        {
            Dispatcher.BeginInvoke(ApplyTheme);
        }
    }

    private void SetWindowFullscreen(bool isFullscreen)
    {
        var wasFullscreen = _windowFullscreen;
        if (wasFullscreen == isFullscreen)
        {
            return;
        }

        if (!wasFullscreen && isFullscreen)
        {
            _windowedState = WindowState;
            _windowedStyle = WindowStyle;
            _windowedResizeMode = ResizeMode;
            var bounds = WindowState == WindowState.Normal
                ? new Rect(Left, Top, Width, Height)
                : RestoreBounds;
            _windowedBounds = new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);

            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else if (wasFullscreen && !isFullscreen)
        {
            WindowState = WindowState.Normal;
            WindowStyle = _windowedStyle;
            ResizeMode = _windowedResizeMode;
            Left = _windowedBounds.Left;
            Top = _windowedBounds.Top;
            Width = _windowedBounds.Width;
            Height = _windowedBounds.Height;
            WindowState = _windowedState;
        }

        _windowFullscreen = isFullscreen;
    }

    private async Task<bool> ConfirmSaveChangesAsync()
    {
        if (!_isDirty)
        {
            return true;
        }

        var result = MessageBox.Show(
            Strings.Tr("変更を保存しますか？"),
            Strings.Tr("未保存の変更"),
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);
        return result switch
        {
            MessageBoxResult.Yes => await SaveDocumentAsync(),
            MessageBoxResult.No => true,
            _ => false
        };
    }

    private async void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_allowClose || !_isDirty)
        {
            return;
        }

        e.Cancel = true;
        if (_closePromptActive)
        {
            return;
        }

        _closePromptActive = true;
        // Let the Closing event finish first: WPF refuses to show a dialog owned by a
        // window that is in the middle of closing (the Save As dialog for a new document).
        await Task.Yield();
        if (await ConfirmSaveChangesAsync())
        {
            _allowClose = true;
            Close();
        }
        else
        {
            _closePromptActive = false;
        }
    }

    private void SendDocumentInfo()
    {
        if (!_pageReady)
        {
            return;
        }

        SendToPage(new
        {
            type = "documentInfo",
            name = _filePath is null ? Strings.Tr("新しい文書.md") : Path.GetFileName(_filePath),
            isDirty = _isDirty,
            encoding = EncodingId(_encoding),
            encodingSource = _encodingSource,
            canReopen = _filePath is not null
        });
    }

    private void SendPreview(bool notifyRendered = false, bool forPrint = false)
    {
        if (!_pageReady)
        {
            return;
        }

        SendToPage(new
        {
            type = "preview",
            html = _htmlSanitizer.Sanitize(RenderMarkdown(_text)),
            notifyRendered,
            forPrint,
            requestId = notifyRendered ? ++_previewRequestId : 0
        });
    }

    // Raw HTML is parsed only so comments can be recognized: comments are notes in
    // the source (such as file headers) and are dropped, and all other HTML is
    // escaped and shown as text, as before.
    private string RenderMarkdown(string text)
    {
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        _markdownPipeline.Setup(renderer);
        renderer.ObjectRenderers.ReplaceOrAdd<HtmlBlockRenderer>(new EscapedHtmlBlockRenderer());
        renderer.ObjectRenderers.ReplaceOrAdd<HtmlInlineRenderer>(new EscapedHtmlInlineRenderer());
        var document = Markdown.Parse(text, _markdownPipeline);
        ResolveRelativeImages(document);
        // Source line of each block, used by the page to keep the editor and preview
        // scrolled to the same place.
        foreach (var block in document.Descendants<Block>())
        {
            block.GetAttributes().AddProperty(
                "data-line", block.Line.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    // Points relative image paths at the document folder host (served by
    // DocumentResourceRequested). "../" cannot climb above that folder.
    private void ResolveRelativeImages(MarkdownDocument document)
    {
        if (_filePath is null)
        {
            return;
        }

        var baseUri = new Uri($"https://{DocumentHost}/");
        foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage))
        {
            var url = image.Url;
            if (string.IsNullOrWhiteSpace(url)
                || url.StartsWith('/')
                || url.StartsWith('#')
                || Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                continue;
            }

            if (Uri.TryCreate(baseUri, url.Replace('\\', '/'), out var resolved))
            {
                image.Url = resolved.AbsoluteUri;
            }
        }
    }

    private sealed class EscapedHtmlBlockRenderer : HtmlObjectRenderer<HtmlBlock>
    {
        protected override void Write(HtmlRenderer renderer, HtmlBlock obj)
        {
            if (obj.Type == HtmlBlockType.Comment)
            {
                return;
            }

            renderer.Write("<p").WriteAttributes(obj).Write(">");
            renderer.WriteLeafRawLines(obj, true, true);
            renderer.WriteLine("</p>");
        }
    }

    private sealed class EscapedHtmlInlineRenderer : HtmlObjectRenderer<HtmlInline>
    {
        protected override void Write(HtmlRenderer renderer, HtmlInline obj)
        {
            if (!obj.Tag.StartsWith("<!--", StringComparison.Ordinal))
            {
                renderer.WriteEscape(obj.Tag);
            }
        }
    }

    private void SendToPage(object message)
    {
        EditorWebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message));
    }

    private void OpenExternalLink(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("https" or "http" or "mailto"))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Win32Exception ex)
        {
            MessageBox.Show(
                $"{Strings.Tr("リンクを開けませんでした。リンク先の場所を確かめて、ブラウザーやエクスプローラーから開いてください。")}\n\n{ex.Message}",
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                $"{Strings.Tr("リンクを開けませんでした。リンク先の場所を確かめて、ブラウザーやエクスプローラーから開いてください。")}\n\n{ex.Message}",
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ShowFileError(string title, Exception ex)
    {
        var stop = Strings.Language == "en" ? "." : "。";
        MessageBox.Show(
            $"{Strings.Tr(title)}{stop}\n\n{ex.Message}",
            "mdEditor",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void ShowStartupError(string message, Exception ex)
    {
        MessageBox.Show(
            $"{message}\n\n{ex.Message}",
            Strings.Tr("mdEditor を起動できません"),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Close();
    }
}
