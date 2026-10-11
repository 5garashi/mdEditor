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
    private readonly MarkdownPipeline _markdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
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
    private string _theme = LoadTheme();

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    public MainWindow(string? initialPath)
    {
        InitializeComponent();
        _initialPath = initialPath;
        ApplyTheme();
        SourceInitialized += (_, _) => ApplyTheme();
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        Closed += (_, _) => SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            SendPreview();
        };

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
            "alt", "checked", "class", "colspan", "disabled", "href", "rowspan", "src", "style",
            "title", "type"
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
            ShowStartupError(Strings.Tr("Microsoft Edge WebView2 Runtime が必要です。"), ex);
            return;
        }
        catch (Exception ex) when (ex is COMException or IOException or UnauthorizedAccessException
                                       or InvalidOperationException or ArgumentException)
        {
            ShowStartupError(Strings.Tr("画面の表示機能（WebView2）を初期化できませんでした。"), ex);
            return;
        }

        EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
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
                if (!string.IsNullOrWhiteSpace(_initialPath))
                {
                    await LoadDocumentAsync(_initialPath);
                }
                else
                {
                    SendDocumentInfo();
                    SendPreview();
                }
                break;
            case "setTheme":
                _theme = NormalizeTheme(root.GetProperty("theme").GetString());
                SaveTheme();
                ApplyTheme();
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

        if (!e.Uri.StartsWith("https://mdeditor.local/", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
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
            var (text, encoding) = DecodeDocument(await File.ReadAllBytesAsync(fullPath));
            _filePath = fullPath;
            _text = text;
            _encoding = encoding;
            _changeVersion++;
            _isDirty = false;
            SendDocumentInfo();
            SendToPage(new { type = "setText", text = _text });
            SendPreview();
        }
        catch (FileNotFoundException ex)
        {
            ShowFileError("ファイルが見つかりません", ex);
        }
        catch (DirectoryNotFoundException ex)
        {
            ShowFileError("フォルダーが見つかりません", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("ファイルを読み取れません", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを読み取れません", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルのパスを確認してください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("このファイルのパス形式には対応していません", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません", ex);
        }
    }

    private async Task<bool> SaveDocumentAsync()
    {
        if (string.IsNullOrWhiteSpace(_filePath))
        {
            return await SaveDocumentAsAsync();
        }

        try
        {
            return await WriteDocumentAsync(_filePath);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("ファイルを保存できません", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを保存できません", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルのパスを確認してください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("このファイルのパス形式には対応していません", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません", ex);
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
            ShowFileError("ファイルを保存できません", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("ファイルを保存できません", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("ファイルのパスを確認してください", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("このファイルのパス形式には対応していません", ex);
        }
        catch (SecurityException ex)
        {
            ShowFileError("ファイルへのアクセスが許可されていません", ex);
        }

        return false;
    }

    // BOMs win; otherwise strict UTF-8, falling back to the system ANSI code page
    // (Shift_JIS on Japanese Windows) for older files.
    private static (string Text, Encoding Encoding) DecodeDocument(byte[] bytes)
    {
        ReadOnlySpan<byte> data = bytes;
        if (data.StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF]))
        {
            return (Encoding.UTF8.GetString(data[3..]), new UTF8Encoding(true));
        }
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xFE]))
        {
            return (Encoding.Unicode.GetString(data[2..]), Encoding.Unicode);
        }
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFE, 0xFF]))
        {
            return (Encoding.BigEndianUnicode.GetString(data[2..]), Encoding.BigEndianUnicode);
        }

        try
        {
            var strictUtf8 = new UTF8Encoding(false, throwOnInvalidBytes: true);
            return (strictUtf8.GetString(bytes), Utf8NoBom);
        }
        catch (DecoderFallbackException)
        {
            var legacy = Encoding.GetEncoding(
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
            return (legacy.GetString(bytes), legacy);
        }
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
        SendDocumentInfo();
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
            // US Letter where the region uses US customary units, A4 elsewhere.
            var useLetter = !System.Globalization.RegionInfo.CurrentRegion.IsMetric;
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
                saved = await EditorWebView.CoreWebView2.PrintToPdfAsync(
                    dialog.FileName,
                    settings);
            }
            finally
            {
                ApplyTheme();
                await EditorWebView.CoreWebView2.ExecuteScriptAsync(
                    "document.body.classList.remove('printing-preview')");
                SendPreview();
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
            ShowFileError("PDFを書き込めません", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("PDFを書き込めません", ex);
        }
        catch (ArgumentException ex)
        {
            ShowFileError("PDFの保存先を確認してください", ex);
        }
        catch (COMException ex)
        {
            ShowFileError("PDFを書き出せません", ex);
        }
        catch (InvalidOperationException ex)
        {
            ShowFileError("PDFの書き出しを開始できません", ex);
        }
        catch (NotSupportedException ex)
        {
            ShowFileError("この環境ではPDFを書き出せません", ex);
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

    private static string LoadTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(SettingsKeyPath);
        return NormalizeTheme(key?.GetValue("Theme") as string);
    }

    private void SaveTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(SettingsKeyPath);
            key.SetValue("Theme", _theme);
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
            isDirty = _isDirty
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

            renderer.Write("<p>");
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
                $"{Strings.Tr("リンクを開けませんでした。")}\n\n{ex.Message}",
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                $"{Strings.Tr("リンクを開けませんでした。")}\n\n{ex.Message}",
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
