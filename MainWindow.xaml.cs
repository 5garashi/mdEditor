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
using System.Net.Http;
using System.Reflection;
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
    // After opening an HTML file the text is Markdown and not saved yet: images
    // still resolve from the HTML file's folder, and Save As suggests a .md beside it.
    private string? _importFolder;
    private string? _importName;
    // Where Save As starts for an imported document, and the images taken out
    // of a Word file (in a temporary folder until the first save copies them).
    private string? _suggestFolder;
    private string? _importAssets;
    // An image opened as a document: copied beside the Markdown file when it is
    // first saved in another folder, so the picture still shows.
    private string? _importImageFile;
    // Internet images: the setting (ask, always, never), consent for this
    // document, how many the last render held back, and the notice state.
    private string _externalImages = NormalizeExternal(LoadSetting("ExternalImages"));
    private bool _externalAllowedHere;
    private int _blockedImages;
    private int _noticedBlocked = -1;
    private string? _importNotice;
    // Every fetch from the internet (pages and document images) goes through this
    // client (Identity 13-3): https only (.NET never follows https to http),
    // a time and a size limit, no cookies, credentials, or Referer, and never
    // an address on this PC or the local network, checked at connect time so
    // names that resolve to such addresses are refused too.
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 5,
        UseCookies = false,
        Credentials = null,
        PreAuthenticate = false,
        ConnectCallback = ConnectToPublicAddressAsync
    })
    {
        Timeout = TimeSpan.FromSeconds(20)
    };
    private const long MaxPageBytes = 10 * 1024 * 1024;

    private static async ValueTask<Stream> ConnectToPublicAddressAsync(
        SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var addresses = await System.Net.Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
        var target = addresses.FirstOrDefault(IsPublicAddress)
            ?? throw new HttpRequestException(Strings.Tr("この PC や社内ネットワークのアドレスは開けません。"));
        var socket = new System.Net.Sockets.Socket(target.AddressFamily, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(target, context.DnsEndPoint.Port, cancellationToken);
            return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }

    // Public internet addresses only: not loopback, private, link-local,
    // carrier-grade NAT, unique-local, multicast, or unspecified.
    private static bool IsPublicAddress(System.Net.IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (System.Net.IPAddress.IsLoopback(address))
        {
            return false;
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            return !(b[0] == 0 || b[0] == 10 || b[0] == 127 || b[0] >= 224
                     || (b[0] == 100 && b[1] >= 64 && b[1] <= 127)
                     || (b[0] == 169 && b[1] == 254)
                     || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                     || (b[0] == 192 && b[1] == 168));
        }

        var bytes = address.GetAddressBytes();
        return !(address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast
                 || address.Equals(System.Net.IPAddress.IPv6None)
                 || (bytes[0] & 0xFE) == 0xFC);
    }

    // Names that are never on the internet: a quick refusal before any lookup.
    private static bool IsLocalName(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)
        || host.EndsWith(".lan", StringComparison.OrdinalIgnoreCase)
        || (!host.Contains('.') && !host.Contains(':'));
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
    private readonly WindowFullscreen _fullscreen = new();
    private TaskCompletionSource? _previewRendered;
    private CoreWebView2Environment? _environment;
    // The preview in its own window, and the last scroll position sent to it.
    private PreviewWindow? _previewWindow;
    private bool _closingApp;
    private double _lastSyncLine;
    private string? _lastSyncEdge = "top";

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
        Closed += (_, _) =>
        {
            _closingApp = true;
            _previewWindow?.Close();
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
            "alt", "checked", "class", "colspan", "data-external-src", "data-line", "disabled", "href", "id", "rowspan",
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
            _environment = environment;
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
        // Every image request passes through the app: document images and, with
        // consent, internet images (fetched by the app's own client, Identity 13-3).
        EditorWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
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
                SendToPage(new { type = "appInfo", version = AppVersion });
                SendToPage(new { type = "externalImages", value = _externalImages });
                if (await OfferRecoveryAsync())
                {
                    // The recovered document is shown instead of the file passed at start.
                }
                else if (_initialPath?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true)
                {
                    await OpenUrlAsync(_initialPath);
                }
                else if (!string.IsNullOrWhiteSpace(_initialPath))
                {
                    await LoadDocumentAsync(_initialPath);
                    if (!string.IsNullOrEmpty(_initialAnchor))
                    {
                        SendScrollToAnchor(_initialAnchor);
                    }
                }
                else
                {
                    SendDocumentInfo();
                    SendPreview();
                }

                if (LoadSetting("PreviewDetached") == "on")
                {
                    await OpenPreviewWindowAsync();
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
                UpdatePreviewWindowTitle();
                break;
            case "dropFile":
                await HandleDropAsync(e);
                break;
            case "toc":
                SendToPage(new { type = "tocText", text = TableOfContents.Build(_text, _markdownPipeline) });
                break;
            case "openSample":
                await OpenSampleAsync(root.GetProperty("text").GetString(),
                    root.TryGetProperty("name", out var sampleName) ? sampleName.GetString() : null);
                break;
            case "dropImages":
                await InsertImagesAsync(
                    (e.AdditionalObjects ?? []).OfType<CoreWebView2File>().Select(file => file.Path).Where(IsImagePath).ToList(), null);
                break;
            case "pasteImage":
                var pastedType = root.TryGetProperty("mime", out var mime) ? mime.GetString() : null;
                await InsertImagesAsync([], Convert.FromBase64String(root.GetProperty("data").GetString() ?? string.Empty),
                    pastedType switch { "image/jpeg" => ".jpg", "image/gif" => ".gif", "image/bmp" => ".bmp", "image/webp" => ".webp", _ => ".png" });
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
                await SaveDocumentAsAsync(
                    allowExports: true,
                    embedSource: root.TryGetProperty("embedSource", out var embedOnSave) && embedOnSave.ValueKind == JsonValueKind.True);
                break;
            case "externalLink":
                OpenExternalLink(root.GetProperty("url").GetString());
                break;
            case "documentLink":
                OpenDocumentLink(root.GetProperty("href").GetString());
                break;
            case "toggleWindowFullscreen":
                _fullscreen.Set(this, !_fullscreen.IsOn);
                break;
            case "exitWindowFullscreen":
                _fullscreen.Set(this, false);
                break;
            case "detachPreview":
                await OpenPreviewWindowAsync();
                break;
            case "attachPreview":
                _previewWindow?.Close();
                break;
            case "syncLine":
                _lastSyncLine = root.GetProperty("line").GetDouble();
                _lastSyncEdge = root.TryGetProperty("edge", out var edge) ? edge.GetString() : null;
                SendToPreviewWindow(new { type = "syncLine", line = _lastSyncLine, edge = _lastSyncEdge });
                break;
            case "allowExternalImages":
                // "Load images" in the notice: for this document only.
                _externalAllowedHere = true;
                _noticedBlocked = -1;
                SendPreview();
                break;
            case "setExternalImages":
                _externalImages = NormalizeExternal(root.GetProperty("value").GetString());
                SaveSetting("ExternalImages", _externalImages);
                _noticedBlocked = -1;
                SendPreview();
                break;
            case "openUrl":
                await OpenUrlAsync(root.GetProperty("url").GetString());
                break;
            case "popupCommand":
                SendToPreviewWindow(new { type = "runCommand", id = root.GetProperty("id").GetString() });
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
            case "exportPng":
                await ExportPngAsync();
                break;
            case "exportDocx":
                await ExportDocxAsync();
                break;
            case "exportHtml":
                await ExportHtmlAsync(root.TryGetProperty("embedSource", out var embed) && embed.ValueKind == JsonValueKind.True);
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

        // Links resolve from the open file's folder, or from the HTML file's folder
        // for a document just opened from HTML.
        var baseFolder = _filePath is null ? _importFolder : Path.GetDirectoryName(_filePath);
        if (baseFolder is null)
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
                baseFolder,
                relative.TrimStart(Path.DirectorySeparatorChar)));
            fragment = Uri.UnescapeDataString(fragment);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            ShowLinkNotFound(pathPart);
            return;
        }

        if (_filePath is not null && string.Equals(fullPath, Path.GetFullPath(_filePath), StringComparison.OrdinalIgnoreCase))
        {
            if (fragment.Length > 0)
            {
                SendScrollToAnchor(fragment);
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
        if (extension is "" or ".md" or ".markdown" or ".txt" or ".text" or ".html" or ".htm")
        {
            // Markdown, HTML (opened as Markdown), and plain text (such as LICENSE) open in a new mdEditor
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
                Strings.Tr("開けるファイル（.md、.txt、.html、.docx、.eml、.csv、.tsv、.json、.xml、.ipynb、画像）を1つだけドロップしてください。画像を編集画面にドロップすると、文書に入れます。"),
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

        var extension = Path.GetExtension(file.Path).ToLowerInvariant();
        if (extension is not (".md" or ".markdown" or ".txt" or ".html" or ".htm" or ".docx" or ".eml" or ".csv" or ".tsv" or ".json" or ".xml" or ".ipynb")
            && !IsImagePath(file.Path))
        {
            return false;
        }

        filePath = file.Path;
        return true;
    }

    private const string OpenFilter =
        "開けるファイル|*.md;*.markdown;*.txt;*.html;*.htm;*.docx;*.eml;*.csv;*.tsv;*.json;*.xml;*.ipynb;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp"
        + "|Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|テキスト ファイル (*.txt)|*.txt|HTML ファイル (*.html;*.htm)|*.html;*.htm"
        + "|Word 文書 (*.docx)|*.docx|メール (*.eml)|*.eml|CSV / TSV (*.csv;*.tsv)|*.csv;*.tsv|JSON / XML (*.json;*.xml)|*.json;*.xml|Jupyter Notebook (*.ipynb)|*.ipynb"
        + "|画像 (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp|すべてのファイル (*.*)|*.*";

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    private static bool IsImagePath(string path) => ImageExtensions.Contains(Path.GetExtension(path));

    private async Task OpenDocumentAsync()
    {
        if (!await ConfirmSaveChangesAsync())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = Strings.Tr(OpenFilter),
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
            if (string.Equals(Path.GetExtension(fullPath), ".docx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ImportDocx(fullPath);
                }
                catch (Exception ex) when (ex is InvalidDataException or InvalidOperationException or FormatException
                                               or System.Xml.XmlException or NotSupportedException or KeyNotFoundException)
                {
                    ShowFileError("Word の文書を読み込めません。Word で開けるか確かめて、.docx 形式で保存し直してから、もう一度開いてください", ex);
                }

                return;
            }

            if (string.Equals(Path.GetExtension(fullPath), ".eml", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ImportMail(fullPath);
                }
                catch (Exception ex) when (ex is FormatException or MimeKit.ParseException or InvalidOperationException or NotSupportedException)
                {
                    ShowFileError("メールを読み込めません。メールアプリから .eml 形式で保存し直してから、もう一度開いてください", ex);
                }

                return;
            }

            if (IsImagePath(fullPath))
            {
                ImportImage(fullPath);
                return;
            }

            var decoded = DecodeDocument(await File.ReadAllBytesAsync(fullPath));
            if (TableFiles.IsTablePath(fullPath))
            {
                ImportTable(fullPath, decoded);
                return;
            }

            if (string.Equals(Path.GetExtension(fullPath), ".ipynb", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ImportNotebook(fullPath, decoded);
                }
                catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException)
                {
                    ShowFileError("Jupyter Notebook を読み込めません。Jupyter で開けるか確かめて、もう一度開いてください", ex);
                }

                return;
            }

            if (DataFiles.IsDataPath(fullPath))
            {
                ImportData(fullPath, decoded);
                return;
            }

            // A page that only forwards to another local HTML file (such as a
            // "latest report" page) opens that file, as a browser would; up to 3 steps.
            for (var step = 0; step < 3 && IsHtmlPath(fullPath)
                 && HtmlImport.RefreshTarget(decoded.Text) is { } target; step++)
            {
                string next;
                try
                {
                    next = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(fullPath)!,
                        Uri.UnescapeDataString(target.Split('#', '?')[0]).Replace('/', Path.DirectorySeparatorChar)));
                }
                catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
                {
                    break;
                }

                if (!IsHtmlPath(next) || !File.Exists(next))
                {
                    break;
                }

                fullPath = next;
                decoded = DecodeDocument(await File.ReadAllBytesAsync(fullPath));
            }

            if (IsHtmlPath(fullPath))
            {
                ImportHtml(fullPath, decoded);
                return;
            }

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

    // Save As lists every format (like Open): Markdown and text save the document
    // itself (the text as it is); PDF, HTML, PNG, and Word export a copy and the
    // document stays as it was. Save of a new document offers the text formats only.
    // Types after the first two, in the order of the Save As list.
    private static readonly string[] ExportTypes = ["plain", ".pdf", ".html", ".png", ".docx", ".csv", ".tsv", ".json", ".ipynb"];

    private const string SaveAsFilter =
        "Markdown ファイル (*.md)|*.md|テキスト（そのまま）(*.txt)|*.txt|テキスト（記号なし）(*.txt)|*.txt"
        + "|PDF (*.pdf)|*.pdf|HTML (*.html)|*.html|PNG 画像 (*.png)|*.png|Word 文書 (*.docx)|*.docx"
        + "|CSV（文書の中の表）(*.csv)|*.csv|TSV（文書の中の表）(*.tsv)|*.tsv|JSON（文書の中の表）(*.json)|*.json|Jupyter Notebook (*.ipynb)|*.ipynb";

    private async Task<bool> SaveDocumentAsAsync(bool allowExports = false, bool embedSource = false)
    {
        var dialog = new SaveFileDialog
        {
            Filter = Strings.Tr(allowExports
                ? SaveAsFilter
                : "Markdown ファイル (*.md)|*.md|テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*"),
            FilterIndex = string.Equals(Path.GetExtension(_filePath), ".txt", StringComparison.OrdinalIgnoreCase) ? 2 : 1,
            DefaultExt = ".md",
            AddExtension = true,
            FileName = _filePath is null ? _importName ?? Strings.Tr("新しい文書.md") : Path.GetFileName(_filePath),
            InitialDirectory = _filePath is null ? _suggestFolder ?? string.Empty : string.Empty
        };
        if (dialog.ShowDialog(this) != true)
        {
            return false;
        }

        if (allowExports && dialog.FilterIndex >= 3 && dialog.FilterIndex - 3 < ExportTypes.Length)
        {
            // The chosen type decides the format, even when another extension was typed.
            var type = ExportTypes[dialog.FilterIndex - 3];
            var extension = type == "plain" ? ".txt" : type;
            var typed = Path.GetExtension(dialog.FileName).ToLowerInvariant();
            var target = typed == extension
                ? dialog.FileName
                : typed is ".md" or ".txt" || ExportTypes.Contains(typed)
                    ? Path.ChangeExtension(dialog.FileName, extension)
                    : dialog.FileName + extension;
            switch (type)
            {
                case "plain": await ExportPlainTextAsync(target); break;
                case ".pdf": await ExportPdfAsync(target); break;
                case ".html": await ExportHtmlAsync(embedSource, target); break;
                case ".png": await ExportPngAsync(target); break;
                case ".docx": await ExportDocxAsync(target); break;
                case ".ipynb": await ExportNotebookAsync(target); break;
                default: await ExportTablesAsync(target, type); break;
            }

            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(dialog.FileName);
            if (!await WriteDocumentAsync(fullPath))
            {
                return false;
            }

            CopyImportedImages(Path.GetDirectoryName(fullPath)!);
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

    // From the VERSION file, through <Version> in mdEditor.csproj.
    private static string AppVersion { get; } =
        typeof(MainWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(MainWindow).Assembly.GetName().Version?.ToString(3)
        ?? string.Empty;

    private static double LoadZoom(string name = "Zoom") =>
        double.TryParse(LoadSetting(name), System.Globalization.NumberStyles.Float,
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

    private static bool IsHtmlPath(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".html" or ".htm";

    // An HTML file opens as Markdown: the original kept by Save as HTML when it
    // is there, otherwise a conversion. The HTML file itself is never written;
    // saving asks for a .md file name.
    private void ImportHtml(string path, DecodedDocument decoded)
    {
        var folder = Path.GetDirectoryName(path)!;
        // Images and links missing beside the file resolve from the page's own
        // address (<base>, canonical, or og:url), as a browser would.
        var (markdown, exact) = HtmlImport.ToMarkdown(decoded.Text, null, relative => ExistsBeside(folder, relative));
        ApplyImported(markdown, folder, Path.GetFileNameWithoutExtension(path) + ".md", false, Strings.Tr(exact
            ? "mdEditor で保存した HTML から、元の Markdown を開きました。保存すると Markdown ファイルになります（HTML ファイルは変わりません）。"
            : "HTML を Markdown に変換して開きました。色や配置、図の絵は元に戻りません。保存すると Markdown ファイルになります（HTML ファイルは変わりません）。"));
    }

    // A Word document opens as Markdown through Mammoth (Word to clean HTML) and
    // the HTML conversion. Its images go to a temporary folder, shown in the
    // preview, and are copied beside the Markdown file when it is first saved;
    // the Word file itself is never written.
    private void ImportDocx(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var imagesName = name + "-images";
        var assets = Path.Combine(Path.GetTempPath(), "mdEditor-import-" + Guid.NewGuid().ToString("N"));
        var count = 0;
        var converter = new Mammoth.DocumentConverter().ImageConverter(image =>
        {
            var extension = image.ContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                "image/svg+xml" => ".svg",
                _ => ".png"
            };
            var file = $"image{++count}{extension}";
            Directory.CreateDirectory(Path.Combine(assets, imagesName));
            using (var source = image.GetStream())
            using (var target = File.Create(Path.Combine(assets, imagesName, file)))
            {
                source.CopyTo(target);
            }

            return new Dictionary<string, string>
            {
                ["src"] = $"{Uri.EscapeDataString(imagesName)}/{file}",
                ["alt"] = image.AltText ?? string.Empty
            };
        });
        var html = converter.ConvertToHtml(path).Value;
        var (markdown, _) = HtmlImport.ToMarkdown(html, null, _ => true);
        ApplyImported(markdown, assets, name + ".md", false, Strings.Tr(
            "Word の文書を Markdown に変換して開きました。見出し・リスト・表・リンク・画像は戻り、色や配置は戻りません。保存すると Markdown ファイルになります（Word の文書は変わりません）。"));
        _suggestFolder = Path.GetDirectoryName(path);
        _importAssets = count > 0 ? Path.Combine(assets, imagesName) : null;
    }

    // A mail opens as Markdown (see MailImport); its pictures are handled like a
    // Word document's, and the .eml file itself is never written.
    private void ImportMail(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var imagesName = name + "-images";
        var assets = Path.Combine(Path.GetTempPath(), "mdEditor-import-" + Guid.NewGuid().ToString("N"));
        var result = MailImport.ToMarkdown(path, assets, imagesName, Strings.Language == "ja");
        ApplyImported(result.Markdown, assets, name + ".md", false, Strings.Tr(
            "メールを Markdown に変換して開きました。件名・差出人・宛先・日付と本文を読み込み、添付ファイルは名前だけを載せました。保存すると Markdown ファイルになります（メールのファイルは変わりません）。"));
        _suggestFolder = Path.GetDirectoryName(path);
        _importAssets = result.Images > 0 ? Path.Combine(assets, imagesName) : null;
    }

    // A CSV or TSV file opens as a Markdown table, the first row as the header.
    private void ImportTable(string path, DecodedDocument decoded)
    {
        var separator = string.Equals(Path.GetExtension(path), ".tsv", StringComparison.OrdinalIgnoreCase) ? '\t' : ',';
        ApplyImported(TableFiles.ToMarkdown(decoded.Text, separator), Path.GetDirectoryName(path)!,
            Path.GetFileNameWithoutExtension(path) + ".md", false, Strings.Tr(
                "表のファイルを Markdown の表にして開きました。1 行目を見出しの行にしています。保存すると Markdown ファイルになります（元のファイルは変わりません）。"));
    }

    // An image opens as a new document that shows it, ready for a description
    // below. The image stays where it is; the document refers to it by name.
    private void ImportImage(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var markdown = $"![{EscapeAlt(name)}]({EscapeImagePath(Path.GetFileName(path))})\n\n";
        ApplyImported(markdown, Path.GetDirectoryName(path)!, name + ".md", false, Strings.Tr(
            "画像を開きました。画像の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、画像もそこに写します。"));
        _importImageFile = path;
    }

    // JSON (a list of records becomes a table; other JSON a code block) and XML
    // (a code block) open as Markdown; the original file is never written.
    // A Jupyter Notebook opens as Markdown (see NotebookFormat); pictures in it
    // are handled like a Word document's, and the notebook itself is never written.
    private void ImportNotebook(string path, DecodedDocument decoded)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var imagesName = name + "-images";
        var assets = Path.Combine(Path.GetTempPath(), "mdEditor-import-" + Guid.NewGuid().ToString("N"));
        var result = NotebookFormat.ToMarkdown(decoded.Text, assets, imagesName);
        ApplyImported(result.Markdown, result.Images > 0 ? assets : Path.GetDirectoryName(path)!, name + ".md", false, Strings.Tr(
            "Jupyter Notebook を Markdown に変換して開きました。コードは言語付きのコードの枠、実行結果は「output」の枠・画像・表になります。保存すると Markdown ファイルになります（ノートブックは変わりません）。"));
        _suggestFolder = Path.GetDirectoryName(path);
        _importAssets = result.Images > 0 ? Path.Combine(assets, imagesName) : null;
    }

    private void ImportData(string path, DecodedDocument decoded)
    {
        var isJson = string.Equals(Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase);
        var (markdown, kind) = isJson ? DataFiles.JsonToMarkdown(decoded.Text) : DataFiles.XmlToMarkdown(decoded.Text);
        var notice = kind switch
        {
            "table" => "JSON の一覧を Markdown の表にして開きました。項目の名前を見出しの行にしています。",
            "invalid" => isJson
                ? "JSON として読めない所があるので、中身をそのままコードの枠に入れて開きました。"
                : "XML として読めない所があるので、中身をそのままコードの枠に入れて開きました。",
            _ => isJson
                ? "JSON をコードの枠に入れて開きました（表にできる形ではないため）。前後に説明を書き足せます。"
                : "XML をコードの枠に入れて開きました。前後に説明を書き足せます。"
        };
        ApplyImported(markdown, Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + ".md", false,
            Strings.Tr(notice) + Strings.Tr("保存すると Markdown ファイルになります（元のファイルは変わりません）。"));
    }

    private static string EscapeAlt(string text) => text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");

    // A relative path as a Markdown link target: each part escaped (spaces,
    // parentheses, and non-ASCII stay safe), separated by "/".
    private static string EscapeImagePath(string relative) =>
        string.Join("/", relative.Split('/', '\\').Select(part => Uri.EscapeDataString(part)));

    // Images dropped on the editor or pasted are put in a "name-images" folder
    // beside the document (or used where they are, when already inside the
    // document's folder) and inserted at the cursor. A new document is saved first.
    private async Task InsertImagesAsync(IReadOnlyList<string> files, byte[]? pasted, string pastedExtension = ".png")
    {
        if (_filePath is null)
        {
            var answer = MessageBox.Show(
                Strings.Tr("画像を入れるには、先に文書を保存します（画像は文書の横の「名前-images」フォルダーに置きます）。保存先を選びますか？"),
                "mdEditor", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (answer != MessageBoxResult.OK || !await SaveDocumentAsAsync() || _filePath is null)
            {
                return;
            }
        }

        try
        {
            var documentFolder = Path.GetDirectoryName(_filePath)!;
            var imagesName = Path.GetFileNameWithoutExtension(_filePath) + "-images";
            var imagesFolder = Path.Combine(documentFolder, imagesName);
            var inserted = new List<string>();

            string Place(string fileName, Action<string> write)
            {
                Directory.CreateDirectory(imagesFolder);
                var stem = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var target = Path.Combine(imagesFolder, fileName);
                for (var n = 2; File.Exists(target); n++)
                {
                    target = Path.Combine(imagesFolder, $"{stem}-{n}{extension}");
                }

                write(target);
                return target;
            }

            foreach (var file in files)
            {
                var full = Path.GetFullPath(file);
                var inside = full.StartsWith(documentFolder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                var target = inside ? full : Place(Path.GetFileName(full), path => File.Copy(full, path));
                inserted.Add($"![{EscapeAlt(Path.GetFileNameWithoutExtension(full))}]({EscapeImagePath(Path.GetRelativePath(documentFolder, target))})");
            }

            if (pasted is not null)
            {
                var target = Place($"image-{DateTime.Now:yyyyMMdd-HHmmss}{pastedExtension}",path => File.WriteAllBytes(path, pasted));
                inserted.Add($"![]({EscapeImagePath(Path.GetRelativePath(documentFolder, target))})");
            }

            if (inserted.Count > 0)
            {
                SendToPage(new { type = "insertText", text = string.Join("\n\n", inserted) + "\n" });
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            ShowFileError("画像を写せません。文書のフォルダーに書き込めるか確かめて、もう一度お試しください", ex);
        }
    }

    // The first save of a document opened from Word puts its images beside the file.
    private void CopyImportedImages(string folder)
    {
        if (_importImageFile is { } image)
        {
            _importImageFile = null;
            var beside = Path.Combine(folder, Path.GetFileName(image));
            if (File.Exists(image) && !File.Exists(beside))
            {
                File.Copy(image, beside);
            }
        }

        if (_importAssets is not { } source || !Directory.Exists(source))
        {
            return;
        }

        var target = Path.Combine(folder, Path.GetFileName(source));
        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source))
        {
            var destination = Path.Combine(target, Path.GetFileName(file));
            if (!File.Exists(destination))
            {
                File.Copy(file, destination);
            }
        }

        _importAssets = null;
    }

    private static bool ExistsBeside(string folder, string relative)
    {
        try
        {
            var path = Uri.UnescapeDataString(relative.Split('#', '?')[0]).Replace('/', Path.DirectorySeparatorChar);
            return File.Exists(Path.GetFullPath(Path.Combine(folder, path.TrimStart(Path.DirectorySeparatorChar))));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
            return false;
        }
    }

    // A document made from HTML (a file or a web page): unsaved Markdown that
    // never writes over its source.
    // A sample from the Samples dialog opens the same way: a new, unsaved
    // document under the sample's own name, never written over the open file.
    private async Task OpenSampleAsync(string? text, string? name)
    {
        if (text is null || !await ConfirmSaveChangesAsync())
        {
            return;
        }

        var fileName = string.IsNullOrWhiteSpace(name) ? Strings.Tr("新しい文書.md") : Path.GetFileName(name);
        ApplyImported(text.Replace("\r\n", "\n"), null, fileName, false, null);
    }

    private void ApplyImported(string markdown, string? folder, string name, bool allowExternal, string? notice)
    {
        DeleteRecoveryCopy();
        _filePath = null;
        _importFolder = folder;
        _importName = name;
        _suggestFolder = folder;
        _importAssets = null;
        _importImageFile = null;
        _text = markdown;
        _encoding = Utf8NoBom;
        _encodingSource = "detected";
        _hasUndecodableBytes = false;
        _changeVersion++;
        _isDirty = true;
        ResetExternalConsent(allowExternal, notice);
        SendDocumentInfo();
        SendToPage(new { type = "setText", text = _text });
        SendEncodingNotice();
        if (notice is not null)
        {
            SendToPage(new { type = "notice", text = notice });
        }

        SendPreview();
        WriteRecoveryCopy();
    }

    // Opens a web page (https only) as Markdown, the way an HTML file opens:
    // no scripts run, nothing about the document is sent, and the page is read
    // only once, within a time and size limit. Images on it are consented to,
    // since the person chose to open this page.
    private async Task OpenUrlAsync(string? address)
    {
        if (!Uri.TryCreate(address?.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != "https")
        {
            MessageBox.Show(Strings.Tr("https:// で始まるアドレスだけ開けます。アドレスを確かめて、もう一度入力してください。"),
                "mdEditor", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (IsLocalName(uri.IdnHost) || (System.Net.IPAddress.TryParse(uri.IdnHost.Trim('[', ']'), out var literal) && !IsPublicAddress(literal)))
        {
            MessageBox.Show(Strings.Tr("この PC や社内ネットワークのアドレスは開けません。"),
                "mdEditor", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!await ConfirmSaveChangesAsync())
        {
            return;
        }

        try
        {
            SendToPage(new { type = "pdfExportStatus", message = Strings.Tr("ページを読み込んでいます…") });
            using var response = await Http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var final = response.RequestMessage?.RequestUri ?? uri;
            if (final.Scheme != "https")
            {
                throw new HttpRequestException(Strings.Tr("https ではないアドレスに転送されたため、開きませんでした。"));
            }

            if (response.Content.Headers.ContentLength > MaxPageBytes)
            {
                throw new HttpRequestException(Strings.Tr("ページが大きすぎるため、開きませんでした（10 MB まで）。"));
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > MaxPageBytes)
                {
                    throw new HttpRequestException(Strings.Tr("ページが大きすぎるため、開きませんでした（10 MB まで）。"));
                }
            }

            var bytes = buffer.ToArray();
            string text;
            try
            {
                var charset = response.Content.Headers.ContentType?.CharSet?.Trim('"');
                text = string.IsNullOrEmpty(charset) ? DecodeDocument(bytes).Text : Encoding.GetEncoding(charset).GetString(bytes);
            }
            catch (ArgumentException)
            {
                text = DecodeDocument(bytes).Text;
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            var path = final.AbsolutePath;
            var isMarkdown = mediaType is "text/markdown" or "text/x-markdown" or "text/plain"
                || path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase);
            var markdown = isMarkdown ? text : HtmlImport.ToMarkdown(text, final, null).Markdown;
            var segment = Path.GetFileNameWithoutExtension(final.Segments.LastOrDefault()?.Trim('/') ?? string.Empty);
            var name = (string.IsNullOrWhiteSpace(segment) ? final.Host : segment) + ".md";
            ApplyImported(markdown, null, name, true, string.Format(
                Strings.Tr("{0} を開きました。スクリプトは実行しないため、後から読み込まれる内容は入りません。保存すると Markdown ファイルになります。"),
                final.AbsoluteUri));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or IOException)
        {
            SendDocumentInfo();
            MessageBox.Show(
                $"{Strings.Tr("ページを開けませんでした。アドレスとインターネットの接続を確かめて、もう一度お試しください。")}\n\n{ex.Message}",
                "mdEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyDecodedDocument(DecodedDocument decoded)
    {
        // A freshly opened document has nothing to recover.
        DeleteRecoveryCopy();
        _importFolder = null;
        _importName = null;
        _suggestFolder = null;
        _importAssets = null;
        ResetExternalConsent(false, null);
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
    // A document image (relative to the open file) as a local path, never above
    // the document's folder and only for image types.
    private bool TryResolveDocumentImage(string relativeUrlPath, out string path, out string contentType)
    {
        path = string.Empty;
        contentType = string.Empty;
        var folder = _filePath is null ? _importFolder : Path.GetDirectoryName(_filePath);
        if (folder is null)
        {
            return false;
        }

        try
        {
            var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var relative = Uri.UnescapeDataString(relativeUrlPath).TrimStart('/');
            path = Path.GetFullPath(Path.Combine(root, relative));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                || !ImageContentTypes.TryGetValue(Path.GetExtension(path), out var type)
                || !File.Exists(path))
            {
                return false;
            }

            contentType = type;
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException
                                       or SecurityException or UriFormatException)
        {
            return false;
        }
    }

    // Save as HTML: one self-contained file with the preview as it looks (day
    // palette, switching to the night palette when the reader's system is dark),
    // document images embedded, and no address that is contacted on opening
    // except the document's own web images.
    private async Task ExportHtmlAsync(bool embedSource, string? presetPath = null)
    {
        if (_pdfExportActive)
        {
            return;
        }

        _pdfExportActive = true;
        try
        {
            SendToPage(new { type = "pdfExportStatus", message = Strings.Tr("HTMLの保存先を選択してください") });
            if ((presetPath ?? AskExportPath("HTML ファイル (*.html)|*.html", ".html")) is not { } targetPath)
            {
                return;
            }

            _previewTimer.Stop();
            var rendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _previewRendered = rendered;
            // Diagrams are redrawn in the day palette, as for a PDF.
            SendPreview(notifyRendered: true, forPrint: true);
            var finished = await Task.WhenAny(rendered.Task, Task.Delay(TimeSpan.FromSeconds(15))) == rendered.Task;
            _previewRendered = null;
            string json;
            try
            {
                if (!finished && MessageBox.Show(
                        Strings.Tr("図や数式の描画が15秒以内に終わりませんでした。このままHTMLに書き出しますか？"),
                        Strings.Tr("HTMLの書き出し"),
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }

                json = await EditorWebView.CoreWebView2.ExecuteScriptAsync("window.mdeditorExportHtml()");
            }
            finally
            {
                SendPreview();
            }

            using var result = JsonDocument.Parse(json);
            var page = result.RootElement;
            var body = EmbedDocumentImages(page.GetProperty("html").GetString() ?? string.Empty);
            var title = page.GetProperty("title").GetString();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = Path.GetFileNameWithoutExtension(targetPath);
            }

            var styles = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "wwwroot", "export.css"));
            var lang = DocumentLanguage(_text);
            var document = $"""
                <!doctype html>
                <html lang="{lang}">
                <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <meta name="generator" content="mdEditor {AppVersion}">
                <title>{System.Net.WebUtility.HtmlEncode(title)}</title>
                {(embedSource ? HtmlImport.SourceBlock(_text) : string.Empty)}
                <style>
                {styles}
                </style>
                </head>
                <body>
                <main>
                {body}
                </main>
                </body>
                </html>

                """;

            // Write beside the target first so a failure never damages an existing file.
            var tempPath = Path.Combine(
                Path.GetDirectoryName(Path.GetFullPath(targetPath))!,
                $".mdEditor-{Guid.NewGuid():N}.tmp");
            try
            {
                await File.WriteAllTextAsync(tempPath, document, Utf8NoBom);
                File.Move(tempPath, targetPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }

            MessageBox.Show(
                $"{Strings.Tr("HTMLを保存しました。")}\n\n{targetPath}",
                Strings.Tr("HTMLの書き出し"),
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowFileError("HTMLを書き込めません。同じ名前のファイルをほかのアプリで開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (IOException ex)
        {
            ShowFileError("HTMLを書き込めません。同じ名前のファイルをほかのアプリで開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or JsonException or KeyNotFoundException)
        {
            ShowFileError("HTMLを書き出せません。少し待ってから、もう一度お試しください", ex);
        }
        finally
        {
            _pdfExportActive = false;
            SendDocumentInfo();
        }
    }

    // The language of the document itself (not of the app): Japanese when its
    // kana and kanji are at least a quarter as many as its Latin letters.
    private static string DocumentLanguage(string text)
    {
        var japanese = text.Count(c => c is (>= '぀' and <= 'ヿ') or (>= '㐀' and <= '鿿'));
        var latin = text.Count(char.IsAsciiLetter);
        return japanese * 4 >= latin && japanese > 0 ? "ja" : "en";
    }

    // Document images reach the page as https://document.mdeditor.local/...; in
    // the saved file they become data: URLs, so the file stands on its own.
    private string EmbedDocumentImages(string html) =>
        System.Text.RegularExpressions.Regex.Replace(
            html,
            "src=\"https://" + System.Text.RegularExpressions.Regex.Escape(DocumentHost) + "(/[^\"]*)\"",
            match =>
            {
                var urlPath = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
                if (!TryResolveDocumentImage(urlPath, out var path, out var contentType))
                {
                    return match.Value;
                }

                try
                {
                    return $"src=\"data:{contentType};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}\"";
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    return match.Value;
                }
            });

    private async void DocumentResourceRequested(
        object? sender,
        CoreWebView2WebResourceRequestedEventArgs e)
    {
        var environment = _environment;
        // TryResolveDocumentImage finds the folder (the open file's, or the HTML file's).
        if (environment is null || !Uri.TryCreate(e.Request.Uri, UriKind.Absolute, out var uri))
        {
            return;
        }

        // The app's own files load as usual.
        if (uri.Host.Equals("mdeditor.local", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var deferral = e.GetDeferral();
        if (!uri.Host.Equals(DocumentHost, StringComparison.OrdinalIgnoreCase))
        {
            // An internet image the document may load (consent was given, so it
            // has an address): fetched by the app's own client, so it carries no
            // cookies or Referer and never reaches this PC or the local network.
            e.Response = await FetchInternetImageAsync(environment, uri);
            return;
        }

        try
        {
            if (!TryResolveDocumentImage(uri.AbsolutePath, out var path, out var contentType))
            {
                e.Response = environment.CreateWebResourceResponse(null, 404, "Not Found", "");
                return;
            }

            var bytes = await File.ReadAllBytesAsync(path);
            e.Response = environment.CreateWebResourceResponse(
                new MemoryStream(bytes), 200, "OK", $"Content-Type: {contentType}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                       or ArgumentException or NotSupportedException
                                       or SecurityException)
        {
            e.Response = environment.CreateWebResourceResponse(null, 404, "Not Found", "");
        }
    }

    private static async Task<CoreWebView2WebResourceResponse> FetchInternetImageAsync(
        CoreWebView2Environment environment, Uri uri)
    {
        var notFound = environment.CreateWebResourceResponse(null, 404, "Not Found", "");
        if (uri.Scheme != "https" || IsLocalName(uri.IdnHost)
            || (System.Net.IPAddress.TryParse(uri.IdnHost.Trim('[', ']'), out var literal) && !IsPublicAddress(literal)))
        {
            return notFound;
        }

        try
        {
            using var response = await Http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            var type = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (!response.IsSuccessStatusCode || !type.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || response.Content.Headers.ContentLength > MaxPageBytes)
            {
                return notFound;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(chunk)) > 0)
            {
                buffer.Write(chunk, 0, read);
                if (buffer.Length > MaxPageBytes)
                {
                    return notFound;
                }
            }

            buffer.Position = 0;
            return environment.CreateWebResourceResponse(buffer, 200, "OK", $"Content-Type: {type}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or InvalidOperationException)
        {
            return notFound;
        }
    }

    // PNG and Word output take pictures of the preview: it is drawn again in the
    // day palette (as for a PDF), laid out alone at a fixed width, and captured
    // with the DevTools screenshot, which also covers what is off screen.
    private const double CaptureScale = 2;
    private const double MaxCaptureHeight = 7800; // CSS px; 2x stays under the 16,384 px image limit

    private async Task<bool> RenderForCaptureAsync(string title)
    {
        _previewTimer.Stop();
        var rendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _previewRendered = rendered;
        SendPreview(notifyRendered: true, forPrint: true);
        var finished = await Task.WhenAny(rendered.Task, Task.Delay(TimeSpan.FromSeconds(15))) == rendered.Task;
        _previewRendered = null;
        if (!finished && MessageBox.Show(
                Strings.Tr("図や数式の描画が15秒以内に終わりませんでした。このまま書き出しますか？"),
                title, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return false;
        }

        // Page coordinates and the screenshot agree only at 100%, so the zoom
        // (Settings, Ctrl + wheel) is set aside while capturing.
        _zoomBeforeCapture = EditorWebView.ZoomFactor;
        EditorWebView.ZoomFactor = 1;
        await EditorWebView.CoreWebView2.ExecuteScriptAsync("window.mdeditorCaptureLayout(true)");
        await Task.Delay(300);
        return true;
    }

    private double? _zoomBeforeCapture;

    private async Task EndCaptureAsync()
    {
        try
        {
            await EditorWebView.CoreWebView2.ExecuteScriptAsync("window.mdeditorCaptureLayout(false)");
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or ObjectDisposedException)
        {
        }

        if (_zoomBeforeCapture is { } zoom)
        {
            EditorWebView.ZoomFactor = zoom;
            _zoomBeforeCapture = null;
        }

        SendPreview();
    }

    private async Task<JsonElement> PageValueAsync(string script)
    {
        using var result = JsonDocument.Parse(await EditorWebView.CoreWebView2.ExecuteScriptAsync(script));
        return result.RootElement.Clone();
    }

    private async Task<byte[]> CaptureAsync(double x, double y, double width, double height)
    {
        var request = JsonSerializer.Serialize(new
        {
            format = "png",
            captureBeyondViewport = true,
            clip = new { x, y, width, height, scale = CaptureScale }
        });
        using var result = JsonDocument.Parse(
            await EditorWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.captureScreenshot", request));
        return Convert.FromBase64String(result.RootElement.GetProperty("data").GetString()!);
    }

    private static async Task WriteAtomicallyAsync(string path, byte[] bytes)
    {
        var tempPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $".mdEditor-{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(tempPath, bytes);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
    }

    private string? AskExportPath(string filter, string extension)
    {
        var defaultName = _filePath is null
            ? Path.GetFileNameWithoutExtension(_importName ?? Strings.Tr("新しい文書.md")) + extension
            : Path.GetFileNameWithoutExtension(_filePath) + extension;
        var dialog = new SaveFileDialog
        {
            Filter = Strings.Tr(filter),
            DefaultExt = extension,
            AddExtension = true,
            CheckPathExists = true,
            OverwritePrompt = true,
            FileName = defaultName,
            InitialDirectory = _filePath is null ? _suggestFolder ?? string.Empty : string.Empty
        };
        Activate();
        return dialog.ShowDialog(this) == true ? dialog.FileName : null;
    }

    // Save as PNG: the whole preview as pictures; a long document is split into
    // name-1.png, name-2.png, and so on.
    private async Task ExportPngAsync(string? presetPath = null)
    {
        if (_pdfExportActive)
        {
            return;
        }

        _pdfExportActive = true;
        var title = Strings.Tr("PNGの書き出し");
        try
        {
            SendToPage(new { type = "pdfExportStatus", message = Strings.Tr("PNGの保存先を選択してください") });
            if ((presetPath ?? AskExportPath("PNG 画像 (*.png)|*.png", ".png")) is not { } path)
            {
                return;
            }

            var saved = new List<string>();
            try
            {
                if (!await RenderForCaptureAsync(title))
                {
                    return;
                }

                var size = await PageValueAsync("window.mdeditorCaptureSize()");
                var width = size.GetProperty("width").GetDouble();
                var height = size.GetProperty("height").GetDouble();
                var parts = Math.Max(1, (int)Math.Ceiling(height / MaxCaptureHeight));
                for (var part = 0; part < parts; part++)
                {
                    var top = part * MaxCaptureHeight;
                    var png = await CaptureAsync(0, top, width, Math.Min(MaxCaptureHeight, height - top));
                    var target = parts == 1
                        ? path
                        : Path.Combine(Path.GetDirectoryName(path)!, $"{Path.GetFileNameWithoutExtension(path)}-{part + 1}.png");
                    await WriteAtomicallyAsync(target, png);
                    saved.Add(target);
                }
            }
            finally
            {
                await EndCaptureAsync();
            }

            MessageBox.Show($"{Strings.Tr("PNGを保存しました。")}\n\n{string.Join("\n", saved)}", title,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowFileError("PNGを書き込めません。同じ名前のファイルをほかのアプリで開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or JsonException or KeyNotFoundException or FormatException)
        {
            ShowFileError("PNGを書き出せません。少し待ってから、もう一度お試しください", ex);
        }
        finally
        {
            _pdfExportActive = false;
            SendDocumentInfo();
        }
    }

    // Save as Word: Word's own styles for the text; diagrams and formulas as
    // pictures taken from the preview (Word cannot draw Mermaid or KaTeX).
    private async Task ExportDocxAsync(string? presetPath = null)
    {
        if (_pdfExportActive)
        {
            return;
        }

        _pdfExportActive = true;
        var title = Strings.Tr("Wordの書き出し");
        try
        {
            SendToPage(new { type = "pdfExportStatus", message = Strings.Tr("Word 文書の保存先を選択してください") });
            if ((presetPath ?? AskExportPath("Word 文書 (*.docx)|*.docx", ".docx")) is not { } path)
            {
                return;
            }

            var figures = new List<ExportFigure>();
            IReadOnlyList<IReadOnlyList<IReadOnlyList<CodeRun>>> codeColors = [];
            try
            {
                if (!await RenderForCaptureAsync(title))
                {
                    return;
                }

                foreach (var rect in (await PageValueAsync("window.mdeditorFigureRects()")).EnumerateArray())
                {
                    var png = await CaptureAsync(rect.GetProperty("x").GetDouble(), rect.GetProperty("y").GetDouble(),
                        rect.GetProperty("width").GetDouble(), rect.GetProperty("height").GetDouble());
                    var frame = System.Windows.Media.Imaging.BitmapDecoder.Create(
                        new MemoryStream(png), System.Windows.Media.Imaging.BitmapCreateOptions.None,
                        System.Windows.Media.Imaging.BitmapCacheOption.OnLoad).Frames[0];
                    figures.Add(new ExportFigure(png, frame.PixelWidth, frame.PixelHeight, CaptureScale));
                }

                codeColors = ReadCodeColors(await PageValueAsync("window.mdeditorCodeColors()"));
            }
            finally
            {
                await EndCaptureAsync();
            }

            var tempPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $".mdEditor-{Guid.NewGuid():N}.tmp");
            try
            {
                var titleText = _filePath is null ? Path.GetFileNameWithoutExtension(path) : Path.GetFileNameWithoutExtension(_filePath);
                new DocxExport(_markdownPipeline, figures, ResolveImageForExport, codeColors).Write(_text, tempPath, titleText);
                File.Move(tempPath, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }

            MessageBox.Show($"{Strings.Tr("Word 文書を保存しました。")}\n\n{path}", title,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowFileError("Word 文書を書き込めません。同じ名前の文書を Word で開いている場合は閉じるか、別の名前で保存してください", ex);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or JsonException or KeyNotFoundException
                                       or FormatException or ArgumentException or NotSupportedException)
        {
            ShowFileError("Word 文書を書き出せません。少し待ってから、もう一度お試しください", ex);
        }
        finally
        {
            _pdfExportActive = false;
            SendDocumentInfo();
        }
    }

    // Text without Markdown's marks (see PlainTextExport); the document stays as it is.
    // The document as a notebook: code blocks become code cells, the rest
    // Markdown cells; outputs are left for Jupyter to make again.
    private async Task ExportNotebookAsync(string path)
    {
        try
        {
            await WriteAtomicallyAsync(path, Utf8NoBom.GetBytes(NotebookFormat.FromMarkdown(_text, _markdownPipeline)));
            MessageBox.Show($"{Strings.Tr("Jupyter Notebook を保存しました。コードの枠はコードのセル、それ以外は Markdown のセルになります（実行結果は入りません）。")}\n\n{path}",
                Strings.Tr("Jupyter Notebook の書き出し"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
    }

    private async Task ExportPlainTextAsync(string path)
    {
        try
        {
            var text = PlainTextExport.Convert(_text, _markdownPipeline, Strings.Language == "ja");
            await WriteAtomicallyAsync(path, Utf8NoBom.GetBytes(text));
            MessageBox.Show($"{Strings.Tr("記号なしのテキストを保存しました。")}\n\n{path}", Strings.Tr("テキストの書き出し"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
    }

    // The tables in the document as CSV or TSV (UTF-8 with a BOM, so Excel reads
    // Japanese correctly) or JSON (UTF-8); several tables become name-1.csv,
    // name-2.csv, and so on.
    private async Task ExportTablesAsync(string path, string format)
    {
        var title = Strings.Tr(format switch { ".tsv" => "TSVの書き出し", ".json" => "JSONの書き出し", _ => "CSVの書き出し" });
        var tables = TableFiles.Tables(_text, _markdownPipeline, Strings.Language == "ja");
        if (tables.Count == 0)
        {
            MessageBox.Show(Strings.Tr("この文書には表がありません。表（| で区切った行）のある文書で保存してください。"), title,
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saved = new List<string>();
            for (var i = 0; i < tables.Count; i++)
            {
                var target = tables.Count == 1
                    ? path
                    : Path.Combine(Path.GetDirectoryName(path)!, $"{Path.GetFileNameWithoutExtension(path)}-{i + 1}{Path.GetExtension(path)}");
                var bytes = format == ".json"
                    ? Utf8NoBom.GetBytes(DataFiles.TableToJson(tables[i]))
                    : new UTF8Encoding(true).GetPreamble().Concat(Utf8NoBom.GetBytes(TableFiles.Write(tables[i], format == ".tsv" ? '\t' : ','))).ToArray();
                await WriteAtomicallyAsync(target, bytes);
                saved.Add(target);
            }

            MessageBox.Show($"{Strings.Tr(tables.Count == 1 ? "表を保存しました。" : "文書の中の表を、1 つずつ別のファイルに保存しました。")}\n\n{string.Join("\n", saved)}",
                title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
    }

    // The colored code lines from the preview (CSS colors such as "rgb(207, 34, 46)").
    private static List<IReadOnlyList<IReadOnlyList<CodeRun>>> ReadCodeColors(JsonElement blocks)
    {
        static string Hex(string? css)
        {
            var match = System.Text.RegularExpressions.Regex.Match(css ?? string.Empty, @"rgba?\((\d+),\s*(\d+),\s*(\d+)");
            return match.Success
                ? string.Concat(match.Groups.Values.Skip(1).Select(group => int.Parse(group.Value).ToString("X2")))
                : "1A1A1A";
        }

        var result = new List<IReadOnlyList<IReadOnlyList<CodeRun>>>();
        if (blocks.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var block in blocks.EnumerateArray())
        {
            result.Add(block.EnumerateArray().Select(line => (IReadOnlyList<CodeRun>)line.EnumerateArray().Select(run => new CodeRun(
                run.GetProperty("text").GetString() ?? string.Empty,
                Hex(run.GetProperty("color").GetString()),
                run.GetProperty("bold").GetBoolean(),
                run.GetProperty("italic").GetBoolean())).ToList()).ToList());
        }

        return result;
    }

    // A relative image in the document as a local file (internet images stay as text in Word).
    private string? ResolveImageForExport(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url, UriKind.Absolute, out _) || url.StartsWith('/'))
        {
            return null;
        }

        return TryResolveDocumentImage(url.Replace('\\', '/'), out var path, out _) ? path : null;
    }

    private async Task ExportPdfAsync(string? presetPath = null)
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
            if ((presetPath ?? AskExportPath("PDF ファイル (*.pdf)|*.pdf", ".pdf")) is not { } targetPath)
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
                    Path.GetDirectoryName(Path.GetFullPath(targetPath))!,
                    $".mdEditor-{Guid.NewGuid():N}.tmp");
                saved = await EditorWebView.CoreWebView2.PrintToPdfAsync(
                    tempPath,
                    settings);
                if (saved)
                {
                    File.Move(tempPath, targetPath, overwrite: true);
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
                    $"{Strings.Tr("PDFを保存しました。")}\n\n{targetPath}",
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

        ApplyPreviewTheme();
    }

    private void ApplyPreviewTheme()
    {
        if (_previewWindow is not { } window)
        {
            return;
        }

        var dark = _theme == "dark" || (_theme == "auto" && IsSystemDarkTheme());
        var background = dark ? Color.FromRgb(0x1A, 0x22, 0x12) : Color.FromRgb(0xFF, 0xF3, 0xD6);
        window.Background = new SolidColorBrush(background);
        window.WebView.DefaultBackgroundColor =
            System.Drawing.Color.FromArgb(background.R, background.G, background.B);
        var handle = new WindowInteropHelper(window).Handle;
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
            name = _filePath is null ? _importName ?? Strings.Tr("新しい文書.md") : Path.GetFileName(_filePath),
            isDirty = _isDirty,
            encoding = EncodingId(_encoding),
            encodingSource = _encodingSource,
            canReopen = _filePath is not null
        });
    }

    private void SendPreview(bool notifyRendered = false, bool forPrint = false, bool previewWindowOnly = false)
    {
        var toPreviewWindow = !forPrint && _previewWindow is { PageReady: true };
        var toPage = _pageReady && !previewWindowOnly;
        if (!toPage && !toPreviewWindow)
        {
            return;
        }

        var html = _htmlSanitizer.Sanitize(RenderMarkdown(_text));
        // The number of source lines lets either window map scroll positions to lines.
        var lines = _text.Count(character => character == '\n') + 1;
        if (toPage)
        {
            SendToPage(new
            {
                type = "preview",
                html,
                lines,
                notifyRendered,
                forPrint,
                requestId = notifyRendered ? ++_previewRequestId : 0
            });
        }

        if (toPreviewWindow)
        {
            SendToPreviewWindow(new { type = "preview", html, lines, notifyRendered = false, forPrint = false, requestId = 0 });
        }

        if (toPage && !forPrint)
        {
            UpdateExternalNotice();
        }
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
        // Images on the internet load only with consent (Settings: ask, always,
        // never; "ask" shows a notice with a Load button) and only over https.
        // A held-back image keeps its address in data-external-src for the page.
        var allowExternal = _externalImages == "always" || (_externalImages == "ask" && _externalAllowedHere);
        var blocked = 0;
        // Relative images need a folder: the open file's, or the HTML file's
        // folder for a document just opened from HTML and not saved yet.
        var hasFolder = _filePath is not null || _importFolder is not null;
        var baseUri = new Uri($"https://{DocumentHost}/");
        foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage))
        {
            var url = image.Url;
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith('#'))
            {
                continue;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            {
                var local = IsLocalName(absolute.IdnHost)
                    || (System.Net.IPAddress.TryParse(absolute.IdnHost.Trim('[', ']'), out var literal) && !IsPublicAddress(literal));
                if (absolute.Scheme is "http" or "https" && (absolute.Scheme != "https" || !allowExternal || local))
                {
                    image.GetAttributes().AddProperty("data-external-src", absolute.AbsoluteUri);
                    image.Url = string.Empty;
                    blocked++;
                }

                continue;
            }

            if (hasFolder && !url.StartsWith('/') && Uri.TryCreate(baseUri, url.Replace('\\', '/'), out var resolved))
            {
                image.Url = resolved.AbsoluteUri;
            }
        }

        _blockedImages = blocked;
    }

    // The notice for held-back images (setting "ask" only), shown again when
    // their number changes; text from opening an HTML file or page comes first.
    private void UpdateExternalNotice()
    {
        var count = _externalImages == "ask" && !_externalAllowedHere ? _blockedImages : 0;
        if (count == _noticedBlocked)
        {
            return;
        }

        _noticedBlocked = count;
        SendToPage(new { type = "externalNotice", count, prefix = _importNotice });
    }

    private static string NormalizeExternal(string? value) =>
        value is "always" or "never" ? value : "ask";

    // A new document starts without consent for its internet images.
    private void ResetExternalConsent(bool allowed, string? notice)
    {
        _externalAllowedHere = allowed;
        _importNotice = notice;
        _noticedBlocked = -1;
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
            // A bare <br> (no attributes) is the one tag that works: it is how a
            // table cell breaks a line, and it cannot carry anything harmful.
            if (System.Text.RegularExpressions.Regex.IsMatch(obj.Tag, @"^<br\s*/?>$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                renderer.Write("<br>");
            }
            else if (!obj.Tag.StartsWith("<!--", StringComparison.Ordinal))
            {
                renderer.WriteEscape(obj.Tag);
            }
        }
    }

    private void SendToPage(object message)
    {
        EditorWebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message));
    }

    private void SendToPreviewWindow(object message)
    {
        if (_previewWindow is { PageReady: true } window)
        {
            window.WebView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message));
        }
    }

    // A link to a heading scrolls whichever window shows the preview.
    private void SendScrollToAnchor(string name)
    {
        if (_previewWindow is { PageReady: true })
        {
            SendToPreviewWindow(new { type = "scrollToAnchor", name });
        }
        else
        {
            SendToPage(new { type = "scrollToAnchor", name });
        }
    }

    // The preview in its own window, for example on a second display. It shares
    // the WebView2 environment, so the page settings (language, text size, and
    // so on) are the same; this window relays the preview, the scroll position
    // (as a source line), and document commands such as Save between the two.
    private async Task OpenPreviewWindowAsync()
    {
        if (_previewWindow is not null)
        {
            _previewWindow.Activate();
            return;
        }

        if (_environment is null)
        {
            return;
        }

        var window = new PreviewWindow(LoadSetting("PreviewWindow"), new WindowInteropHelper(this).Handle) { Icon = Icon };
        _previewWindow = window;
        UpdatePreviewWindowTitle();
        window.SourceInitialized += (_, _) => ApplyPreviewTheme();
        window.Closing += (_, _) =>
        {
            if (window.SavePlacement() is { } placement)
            {
                SaveSetting("PreviewWindow", placement);
            }
        };
        window.Closed += (_, _) =>
        {
            _previewWindow = null;
            if (_closingApp)
            {
                // Closed with the app: it opens again at the next start.
                return;
            }

            SaveSetting("PreviewDetached", "off");
            SendToPage(new { type = "previewDetached", detached = false });
            Activate();
        };
        window.Show();
        // The editor keeps the keyboard focus.
        Activate();

        try
        {
            await window.WebView.EnsureCoreWebView2Async(_environment);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or ArgumentException)
        {
            window.Close();
            MessageBox.Show(
                $"{Strings.Tr("プレビューのウィンドウを開けませんでした。mdEditor を再起動してから、もう一度お試しください。")}\n\n{ex.Message}",
                "mdEditor",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (_previewWindow != window)
        {
            return;
        }

        var core = window.WebView.CoreWebView2;
        core.Settings.AreDevToolsEnabled = false;
        window.WebView.ZoomFactor = LoadZoom("PreviewZoom");
        window.WebView.ZoomFactorChanged += (_, _) => SaveSetting(
            "PreviewZoom",
            window.WebView.ZoomFactor.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
        ApplyPreviewTheme();
        core.SetVirtualHostNameToFolderMapping(
            "mdeditor.local",
            Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            CoreWebView2HostResourceAccessKind.DenyCors);
        core.WebMessageReceived += PreviewMessageReceived;
        core.NavigationStarting += NavigationStarting;
        core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
        core.WebResourceRequested += DocumentResourceRequested;
        window.WebView.Source = new Uri("https://mdeditor.local/index.html?view=preview");
        SaveSetting("PreviewDetached", "on");
        SendToPage(new { type = "previewDetached", detached = true });
    }

    private async void PreviewMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        if (_previewWindow is not { } window)
        {
            return;
        }

        using var message = JsonDocument.Parse(e.WebMessageAsJson);
        var root = message.RootElement;
        switch (root.GetProperty("type").GetString())
        {
            case "ready":
                window.PageReady = true;
                if (Strings.ForcedLanguage is not null)
                {
                    SendToPreviewWindow(new { type = "language", lang = Strings.ForcedLanguage });
                }
                SendToPreviewWindow(new { type = "theme", theme = _theme });
                SendPreview(previewWindowOnly: true);
                SendToPreviewWindow(new { type = "syncLine", line = _lastSyncLine, edge = _lastSyncEdge });
                break;
            case "syncLine":
                _lastSyncLine = root.GetProperty("line").GetDouble();
                _lastSyncEdge = root.TryGetProperty("edge", out var edge) ? edge.GetString() : null;
                SendToPage(new { type = "syncLine", line = _lastSyncLine, edge = _lastSyncEdge });
                break;
            case "command":
                // Document commands (Save, Open, Find, Settings, ...) belong to this window.
                SendToPage(new { type = "runCommand", id = root.GetProperty("id").GetString() });
                break;
            case "attachPreview":
                window.Close();
                break;
            case "externalLink":
                OpenExternalLink(root.GetProperty("url").GetString());
                break;
            case "documentLink":
                OpenDocumentLink(root.GetProperty("href").GetString());
                break;
            case "toggleWindowFullscreen":
                window.Fullscreen.Set(window, !window.Fullscreen.IsOn);
                break;
            case "exitWindowFullscreen":
                window.Fullscreen.Set(window, false);
                break;
            case "dropFile":
                await HandleDropAsync(e);
                break;
        }
    }

    private void UpdatePreviewWindowTitle()
    {
        if (_previewWindow is { } window)
        {
            window.Title = $"{Strings.Tr("プレビュー")} - mdEditor by 5garashi.com";
        }
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
