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
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    // Started with --paste: the clipboard goes into the new document.
    private readonly bool _initialPaste;
    // "Closer to GitHub" (Settings > Markdown) leaves out mdEditor's own syntax.
    private bool _nearGitHub = LoadSetting("NearGitHub") == "on";
    private MarkdownPipeline _markdownPipeline = MarkdownPipelines.Build(LoadSetting("NearGitHub") == "on");
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
    // "Load detailed maps" was chosen for the open document (map background: ask).
    private bool _mapsAllowedHere;
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
    // Images, videos, and files a document brings along go in a folder beside it
    // named "name_files", as browsers name the folder of a page saved complete.
    private const string FilesSuffix = "_files";

    // The last document left for another one, kept so that "Back to previous
    // document" can bring it back (one copy, shared by every window, kept across
    // restarts until it is replaced or used): closed with "No" to saving, its
    // unsaved text; left without changes, only its file, which is opened again.
    private static readonly string DiscardedFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mdEditor", "Recovery", "discarded.json");

    private sealed record DiscardedRecord(
        string? Path, string Encoding, string Text, string? ImportFolder, string? ImportName,
        string? SuggestFolder, string? ImportAssets, string? ImportImageFile, DateTime SavedAt, bool Changed = true);

    // A saved document with no changes that the save question let go: it is kept
    // as the previous document only once another document really takes its place.
    private string? _leavingClean;

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileOperationInfo
    {
        public IntPtr Hwnd;
        public uint Func;
        public string From;
        public string? To;
        public ushort Flags;
        public int AnyOperationsAborted;
        public IntPtr NameMappings;
        public string? ProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref ShFileOperationInfo operation);

    public MainWindow(string? initialPath, string? initialAnchor = null, bool paste = false)
    {
        InitializeComponent();
        _initialPath = initialPath;
        _initialAnchor = initialAnchor;
        _initialPaste = paste;
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
        // Another window may have kept or used the discarded copy meanwhile.
        Activated += (_, _) => SendDiscardedState();
        _backupTimer.Start();
        // A normal exit (saved, or changes deliberately discarded) leaves no copy behind.
        Closed += (_, _) => DeleteRecoveryCopy();
        Closed += (_, _) =>
        {
            _closingApp = true;
            _previewWindow?.Close();
        };

        _htmlSanitizer.AllowedTags.Clear();
        // HTML written in Markdown shows the way GitHub shows it: the tags and
        // attributes GitHub allows are kept, and everything that could run or load
        // something by itself (script, style, iframe, object, embed, forms, event
        // attributes, javascript: links) is removed.
        _htmlSanitizer.AllowedTags.UnionWith(
        [
            "a", "abbr", "b", "bdo", "blockquote", "br", "caption", "cite", "code", "dd", "del", "details", "dfn", "div",
            "dl", "dt", "em", "figcaption", "figure", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "i", "img", "input", "ins",
            "kbd", "li", "mark", "ol", "p", "picture", "pre", "q", "rp", "rt", "ruby", "s", "samp", "small", "source",
            "span", "strike", "strong", "sub", "summary", "sup", "table", "tbody", "td", "tfoot", "th", "thead", "time",
            "tr", "tt", "u", "ul", "var", "video", "wbr"
        ]);
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedAttributes.UnionWith(
        [
            "align", "alt", "checked", "cite", "class", "colspan", "controls", "data-external-src", "data-line", "data-source", "datetime", "dir",
            "disabled", "height", "href", "id", "lang", "loop", "media", "muted", "name", "open", "playsinline", "poster", "reversed",
            "rowspan", "src", "srcset", "start", "style", "title", "type", "valign", "width",
            // Older presentational attributes GitHub keeps too, such as <br clear="all">
            // to end text flowing around a picture and <table border="1">.
            "abbr", "axis", "border", "cellpadding", "cellspacing", "clear", "color", "compact", "frame", "headers",
            "hreflang", "hspace", "itemprop", "noshade", "nowrap", "rules", "scope", "size", "span", "summary", "vspace"
        ]);
        // Markdig writes table alignment as style="text-align: ..."; nothing else is kept.
        _htmlSanitizer.AllowedCssProperties.Clear();
        _htmlSanitizer.AllowedCssProperties.Add("text-align");
        _htmlSanitizer.AllowedSchemes.Clear();
        _htmlSanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
        // Task-list items are the only inputs Markdig emits; keep them read-only checkboxes.
        _htmlSanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is AngleSharp.Dom.IElement { TagName: "IMG" or "SOURCE" or "VIDEO" } picture)
            {
                ResolveHtmlImage(picture);
            }
            else if (e.Node is AngleSharp.Dom.IElement { TagName: "INPUT" } input)
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
        // The page only ever loads the app's own files (mdeditor.local): web links open in
        // the browser and internet images go through the app's own client. The SmartScreen
        // check of that address held up every launch by about 2 seconds.
        EditorWebView.CoreWebView2.Settings.IsReputationCheckingRequired = false;
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
        EditorWebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);
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
                    ?? (root.TryGetProperty("lang", out var lang) ? Strings.Normalize(lang.GetString()) : null)
                    ?? "ja";
                if (Strings.ForcedLanguage is not null)
                {
                    SendToPage(new { type = "language", lang = Strings.ForcedLanguage });
                }
                SendToPage(new { type = "theme", theme = _theme });
                SendDiscardedState();
                SendPaper();
                SendZoom();
                SendToPage(new { type = "autoBackup", enabled = _autoBackup });
                SendToPage(new { type = "nearGitHub", enabled = _nearGitHub });
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
                    if (_initialPaste)
                    {
                        await PasteClipboardAsync();
                    }
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
            case "setNearGitHub":
                _nearGitHub = root.GetProperty("enabled").GetBoolean();
                SaveSetting("NearGitHub", _nearGitHub ? "on" : "off");
                _markdownPipeline = MarkdownPipelines.Build(_nearGitHub);
                SendPreview();
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
                    // With backup off, no copy of a discarded document stays on the PC either.
                    TryDelete(DiscardedFile);
                }

                SendDiscardedState();
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
                Strings.Language = Strings.Normalize(root.GetProperty("lang").GetString()) ?? "ja";
                SendDocumentInfo();
                SendDiscardedState();
                UpdatePreviewWindowTitle();
                break;
            case "dropFile":
                await HandleDropAsync(e, root.TryGetProperty("onEditor", out var onEditor) && onEditor.GetBoolean());
                break;
            case "restoreDiscarded":
                await RestoreDiscardedAsync();
                break;
            case "toc":
                SendToPage(new { type = "tocText", text = TableOfContents.Build(_text, _markdownPipeline) });
                break;
            case "openSample":
                await OpenSampleAsync(root.GetProperty("text").GetString(),
                    root.TryGetProperty("name", out var sampleName) ? sampleName.GetString() : null);
                break;
            case "getPasteHotkey":
                SendPasteHotkey();
                break;
            case "setPasteHotkey":
                SetPasteHotkey(root.TryGetProperty("key", out var pasteKey) ? pasteKey.GetString() : null);
                break;
            case "pasteImage":
                var pastedType = root.TryGetProperty("mime", out var mime) ? mime.GetString() : null;
                await InsertFilesAsync([], Convert.FromBase64String(root.GetProperty("data").GetString() ?? string.Empty),
                    pastedType switch { "image/jpeg" => ".jpg", "image/gif" => ".gif", "image/bmp" => ".bmp", "image/webp" => ".webp", _ => ".png" });
                break;
            case "change":
                _text = root.GetProperty("text").GetString() ?? string.Empty;
                _changeVersion++;
                // Links the app rewrote while saving (host) are already in the saved file.
                if (!(root.TryGetProperty("host", out var host) && host.GetBoolean()))
                {
                    _isDirty = true;
                    SendDocumentInfo();
                }

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
            case "allowMaps":
                // "Load detailed maps" in the notice: for this document only.
                _mapsAllowedHere = true;
                SendPreview();
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

    // On the editor, dropped files go in at the cursor: images and videos as
    // pictures, files mdEditor opens as their content (converted as Open does),
    // and other files as links. Anywhere else, one file mdEditor opens is opened.
    private async Task HandleDropAsync(CoreWebView2WebMessageReceivedEventArgs e, bool onEditor)
    {
        var paths = (e.AdditionalObjects ?? []).OfType<CoreWebView2File>().Select(file => file.Path).ToList();
        if (paths.Count > 0 && !paths.Any(Directory.Exists))
        {
            if (onEditor)
            {
                await InsertFilesAsync(paths, null);
                return;
            }

            if (paths.Count == 1 && (IsDocumentPath(paths[0]) || IsMediaPath(paths[0])))
            {
                if (await ConfirmSaveChangesAsync())
                {
                    await LoadDocumentAsync(paths[0]);
                }

                return;
            }

            if (paths.All(path => !IsDocumentPath(path) && !IsMediaPath(path)))
            {
                MessageBox.Show(
                    Strings.Tr("mdEditor では開けないファイルです。編集画面にドロップすると、リンクとして入ります。"),
                    "mdEditor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
        }

        MessageBox.Show(
            Strings.Tr("開けるファイル（.md、.txt、.html、.docx、.pdf、.eml、.csv、.tsv、.json、.xml、.ipynb、画像、動画）は、1つだけドロップしてください。編集画面にドロップすると、ファイルの中身・画像・動画・リンクを文書に入れます。フォルダーはドロップできません。"),
            "mdEditor",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // Files that open as a document of their own (images and videos open too, see IsMediaPath).
    private static bool IsDocumentPath(string path) =>
        Path.GetExtension(path).ToLowerInvariant() is ".md" or ".markdown" or ".txt" or ".html" or ".htm" or ".docx"
            or ".eml" or ".csv" or ".tsv" or ".json" or ".xml" or ".ipynb" or ".pdf";

    private static bool IsPdfPath(string path) =>
        string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase);

    // Dropped on the editor, a file's content goes in, except a PDF: it is usually sent
    // along as it is, so it goes into name_files and in as a link, as before.
    private static bool InsertsContent(string path) => IsDocumentPath(path) && !IsPdfPath(path);

    private const string OpenFilter =
        "開けるファイル|*.md;*.markdown;*.txt;*.html;*.htm;*.docx;*.eml;*.csv;*.tsv;*.json;*.xml;*.ipynb;*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp;*.mp4;*.m4v;*.webm;*.ogv;*.mov"
        + "|Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|テキスト ファイル (*.txt)|*.txt|HTML ファイル (*.html;*.htm)|*.html;*.htm"
        + "|Word 文書 (*.docx)|*.docx|メール (*.eml)|*.eml|CSV / TSV (*.csv;*.tsv)|*.csv;*.tsv|JSON / XML (*.json;*.xml)|*.json;*.xml|Jupyter Notebook (*.ipynb)|*.ipynb|PDF (*.pdf)|*.pdf"
        + "|画像 (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp"
        + "|動画 (*.mp4;*.m4v;*.webm;*.ogv;*.mov)|*.mp4;*.m4v;*.webm;*.ogv;*.mov|すべてのファイル (*.*)|*.*";

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    private static bool IsImagePath(string path) => ImageExtensions.Contains(Path.GetExtension(path));

    private static bool IsVideoPath(string path) => VideoContentTypes.ContainsKey(Path.GetExtension(path));

    // Images and videos: shown in the document with the image syntax.
    private static bool IsMediaPath(string path) => IsImagePath(path) || IsVideoPath(path);

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
            RememberLeftDocument(fullPath);
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

            if (IsPdfPath(fullPath))
            {
                try
                {
                    ImportPdf(fullPath);
                }
                catch (UglyToad.PdfPig.Exceptions.PdfDocumentEncryptedException ex)
                {
                    ShowFileError("パスワードで保護された PDF は開けません。保護を外した PDF を開いてください", ex);
                }
                catch (Exception ex) when (ex is UglyToad.PdfPig.Core.PdfDocumentFormatException or InvalidOperationException
                                               or InvalidDataException or FormatException or NotSupportedException
                                               or ArgumentException or IndexOutOfRangeException)
                {
                    ShowFileError("PDF を読み込めません。PDF を表示するアプリで開けるか確かめてから、もう一度開いてください", ex);
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

            if (IsMediaPath(fullPath))
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
    private static readonly string[] ExportTypes = ["plain", ".pdf", ".html", ".png", ".docx", ".csv", ".tsv", ".json", ".ipynb", ".zip"];

    private const string SaveAsFilter =
        "Markdown ファイル (*.md)|*.md|テキスト（そのまま）(*.txt)|*.txt|テキスト（記号なし）(*.txt)|*.txt"
        + "|PDF (*.pdf)|*.pdf|HTML (*.html)|*.html|PNG 画像 (*.png)|*.png|Word 文書 (*.docx)|*.docx"
        + "|CSV（文書の中の表）(*.csv)|*.csv|TSV（文書の中の表）(*.tsv)|*.tsv|JSON（文書の中の表）(*.json)|*.json|Jupyter Notebook (*.ipynb)|*.ipynb"
        + "|配布用 ZIP（文書と「名前_files」フォルダー）(*.zip)|*.zip";

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
                case ".zip": await ExportZipAsync(target); break;
                default: await ExportTablesAsync(target, type); break;
            }

            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(dialog.FileName);
            var moved = _filePath is not null && !string.Equals(_filePath, fullPath, StringComparison.OrdinalIgnoreCase)
                ? await CarryFilesFolderAsync(_filePath, fullPath)
                : null;
            if (!await WriteDocumentAsync(fullPath))
            {
                return false;
            }

            CopyImportedImages(Path.GetDirectoryName(fullPath)!);
            _filePath = fullPath;
            SendDocumentInfo();
            SendPreview();
            if (moved is not null)
            {
                SendToPage(new { type = "notice", text = moved });
            }

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

    // Save As to another folder or name: the document's name_files folder is
    // copied whole beside the new file (as "newname_files"; files whose names are
    // taken there get " (1)" and so on, identical ones are shared), and links are
    // rewritten in the text and the editor: links into the folder follow the copy,
    // and other relative links point to the same files from the new place. The
    // original file and folder are not changed. Returns the notice, or null.
    private async Task<string?> CarryFilesFolderAsync(string oldPath, string newPath)
    {
        var oldFolder = Path.GetDirectoryName(oldPath)!;
        var newFolder = Path.GetDirectoryName(newPath)!;
        var oldFiles = Path.Combine(oldFolder, Path.GetFileNameWithoutExtension(oldPath) + FilesSuffix);
        var newFiles = Path.Combine(newFolder, Path.GetFileNameWithoutExtension(newPath) + FilesSuffix);
        var sameFolder = string.Equals(oldFolder, newFolder, StringComparison.OrdinalIgnoreCase);
        var hasFiles = Directory.Exists(oldFiles) && !string.Equals(oldFiles, newFiles, StringComparison.OrdinalIgnoreCase);
        var renamed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (hasFiles)
        {
            var all = Directory.GetFiles(oldFiles, "*", SearchOption.AllDirectories);
            var folderName = Path.GetFileName(oldFiles);
            var lastReport = DateTime.MinValue;
            for (var i = 0; i < all.Length; i++)
            {
                if (DateTime.UtcNow - lastReport > TimeSpan.FromMilliseconds(250))
                {
                    lastReport = DateTime.UtcNow;
                    SendToPage(new
                    {
                        type = "notice",
                        text = string.Format(Strings.Tr("「{0}」フォルダーを複製しています… {1} / {2}（{3}）"), folderName, i + 1, all.Length, Path.GetFileName(all[i]))
                    });
                }

                var relative = Path.GetRelativePath(oldFiles, all[i]);
                var target = Path.Combine(newFiles, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                var source = all[i];
                var (placed, _) = await Task.Run(() => PlaceFile(target, source, null));
                if (!string.Equals(placed, target, StringComparison.OrdinalIgnoreCase))
                {
                    renamed[relative] = Path.GetRelativePath(newFiles, placed);
                }
            }
        }

        // Links into the folder follow the copy; others are kept, or, in another folder, point to the same files.
        var outside = 0;
        var edits = ReferenceEdits(_text, [oldFolder], target =>
        {
            if (hasFiles && IsInside(target, oldFiles))
            {
                var relative = Path.GetRelativePath(oldFiles, target);
                return LinkTarget(newFolder, Path.Combine(newFiles, renamed.GetValueOrDefault(relative, relative)));
            }

            // From a sibling folder, "../x" may still be right; only links that change are rewritten.
            var moved = LinkTarget(newFolder, target);
            if (sameFolder || moved == LinkTarget(oldFolder, target))
            {
                return null;
            }

            outside++;
            return moved;
        });
        if (edits.Count > 0)
        {
            var text = new StringBuilder(_text);
            foreach (var edit in edits.OrderByDescending(edit => edit.Start))
            {
                text.Remove(edit.Start, edit.Length).Insert(edit.Start, edit.Target);
            }

            // The editor counts lines with "\n" only, so "\r" before an edit moves it left.
            int EditorOffset(int index) => index - _text.AsSpan(0, index).Count('\r');
            SendToPage(new
            {
                type = "applyEdits",
                edits = edits.Select(edit => new { from = EditorOffset(edit.Start), to = EditorOffset(edit.Start + edit.Length), insert = edit.Target })
            });
            _text = text.ToString();
        }

        var notices = new List<string>();
        if (hasFiles)
        {
            var sameName = string.Equals(Path.GetFileName(oldPath), Path.GetFileName(newPath), StringComparison.OrdinalIgnoreCase);
            notices.Add(string.Format(Strings.Tr(sameName
                    ? "「{0}」と「{1}」フォルダーを {2} に複製しました。"
                    : sameFolder ? "「{0}」と「{1}」フォルダーとして複製しました。" : "「{0}」と「{1}」フォルダーとして {2} に複製しました。"),
                Path.GetFileName(newPath), Path.GetFileName(newFiles), newFolder));
            if (renamed.Count > 0)
            {
                notices.Add(string.Format(Strings.Tr("同じ名前の別のファイルがあった {0} 個は、番号（(1) など）を付けて保存し、リンクも合わせました。"), renamed.Count));
            }
        }

        if (outside > 0)
        {
            notices.Add(string.Format(Strings.Tr("「{0}」フォルダーの外を指すリンク {1} 個は、元のファイルを指すように書き換えました。"), Path.GetFileName(newFiles), outside));
        }

        return notices.Count == 0 ? null : string.Join(Strings.SentenceJoin, notices);
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

    // The whole window starts at 90%, which fits more of the toolbar and the document.
    private const double DefaultZoom = 0.9;

    private static double LoadZoom(string name = "Zoom") =>
        double.TryParse(LoadSetting(name), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var zoom) && zoom >= 0.25 && zoom <= 5
            ? zoom
            : DefaultZoom;

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
            _ => DefaultZoom
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
            SendEncodingNotice();
            SendPreview();
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
        var (markdown, assets, count) = ConvertDocx(path, out var guessed);
        var notice = Strings.Tr("Word の文書を Markdown に変換して開きました。見出し・リスト・表・リンク・画像は戻り、色や配置は戻りません。保存すると Markdown ファイルになります（Word の文書は変わりません）。");
        if (guessed > 0)
        {
            notice += Strings.SentenceJoin + string.Format(Strings.Tr("見出しのスタイルがないため、文字の大きさから {0} 個の段落を見出しにしました。"), guessed);
        }

        ApplyImported(markdown, assets, name + ".md", false, notice);
        _suggestFolder = Path.GetDirectoryName(path);
        _importAssets = count > 0 ? Path.Combine(assets, name + FilesSuffix) : null;
    }

    // A PDF opens as Markdown (see PdfImport): text in reading order, headings guessed
    // from text size, lists, and pictures, which are handled like a Word document's.
    // The PDF itself is never written.
    private void ImportPdf(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var assets = NewImportFolder();
        var result = PdfImport.ToMarkdown(path, assets, name + FilesSuffix);
        var notices = new List<string>
        {
            Strings.Tr("PDF を Markdown に変換して開きました。できる範囲で変換したもので、元の PDF を Markdown で忠実に再現したものではありません。文字・見出し・箇条書き・表・コード・画像を取り出し、図と数式は線と文字から画像（SVG）に描き直します。保存すると Markdown ファイルになります。")
        };
        if (!result.HasText)
        {
            notices.Add(Strings.Tr("この PDF からは文字を取り出せませんでした。画像だけでできた PDF（スキャンしたものなど）の可能性があります。"));
        }

        if (result.ReadPages < result.Pages)
        {
            notices.Add(string.Format(Strings.Tr("ページが多いため、最初の {0} ページだけを開きました（全部で {1} ページ）。"), result.ReadPages, result.Pages));
        }

        if (result.PicturePages.Count > 0)
        {
            notices.Add(string.Format(Strings.Tr("{0} ページ目は、文字の並び（縦書き・重なった文字など）を読み取れないため、ページ全体を画像（SVG）にしました。"),
                string.Join(", ", result.PicturePages)));
        }

        ApplyImported(result.Markdown, assets, name + ".md", false, string.Join(Strings.SentenceJoin, notices));
        _suggestFolder = Path.GetDirectoryName(path);
        _importAssets = result.Images > 0 ? Path.Combine(assets, name + FilesSuffix) : null;
    }

    private static string NewImportFolder() =>
        Path.Combine(Path.GetTempPath(), "mdEditor-import-" + Guid.NewGuid().ToString("N"));

    // Word to Markdown; its images are written to "name_files" in a new temporary folder.
    // Headings made only by a larger text size become headings too (DocxHeadings).
    private static (string Markdown, string Assets, int Images) ConvertDocx(string path) =>
        ConvertDocx(path, out _);

    private static (string Markdown, string Assets, int Images) ConvertDocx(string path, out int guessedHeadings)
    {
        var imagesName = Path.GetFileNameWithoutExtension(path) + FilesSuffix;
        var assets = NewImportFolder();
        var count = 0;
        var (source, guessed) = DocxHeadings.Prepare(path, Path.Combine(assets, "word"));
        guessedHeadings = guessed;
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
        string html;
        try
        {
            html = converter.ConvertToHtml(source).Value;
        }
        finally
        {
            Directory.Delete(Path.Combine(assets, "word"), true);
        }

        return (HtmlImport.ToMarkdown(html, null, _ => true).Markdown, assets, count);
    }

    // A mail opens as Markdown (see MailImport); its pictures are handled like a
    // Word document's, and the .eml file itself is never written.
    private void ImportMail(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var imagesName = name + FilesSuffix;
        var assets = Path.Combine(Path.GetTempPath(), "mdEditor-import-" + Guid.NewGuid().ToString("N"));
        var result = MailImport.ToMarkdown(path, assets, imagesName, Strings.Language);
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

    // An image or a video opens as a new document that shows it, ready for a
    // description below. The file stays where it is; the document refers to it by name.
    private void ImportImage(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var markdown = $"![{EscapeAlt(name)}]({EscapeImagePath(Path.GetFileName(path))})\n\n";
        ApplyImported(markdown, Path.GetDirectoryName(path)!, name + ".md", false, Strings.Tr(IsVideoPath(path)
            ? "動画を開きました。動画の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、動画もそこに写します。"
            : "画像を開きました。画像の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、画像もそこに写します。"));
        _importImageFile = path;
    }

    // JSON (a list of records becomes a table; other JSON a code block) and XML
    // (a code block) open as Markdown; the original file is never written.
    // A Jupyter Notebook opens as Markdown (see NotebookFormat); pictures in it
    // are handled like a Word document's, and the notebook itself is never written.
    private void ImportNotebook(string path, DecodedDocument decoded)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var imagesName = name + FilesSuffix;
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

    // Dropped files and pasted pictures go in at the cursor. Images, videos, and
    // other files are put in a "name_files" folder beside the document (or used
    // where they are, when already inside the document's folder) and inserted as
    // pictures or links; files mdEditor opens are inserted as their content. A new
    // document is saved first when something has to be placed beside it.
    private async Task InsertFilesAsync(IReadOnlyList<string> files, byte[]? pasted, string pastedExtension = ".png")
    {
        var contents = new Dictionary<string, (string Markdown, string[] Folders)>();
        foreach (var file in files.Where(InsertsContent))
        {
            try
            {
                contents[file] = await ConvertForInsertAsync(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
                                           or NotSupportedException or InvalidDataException or InvalidOperationException
                                           or FormatException or System.Xml.XmlException or KeyNotFoundException
                                           or JsonException or MimeKit.ParseException)
            {
                ShowFileError("ファイルの中身を読み込めません。「開く」で開けるか確かめてから、もう一度お試しください", ex);
                return;
            }
        }

        var needsFolder = pasted is not null || files.Any(file => !InsertsContent(file))
            || contents.Values.Any(content => RebaseReferences(content.Markdown, content.Folders, null).Count > 0);
        if (_filePath is null && needsFolder)
        {
            var answer = MessageBox.Show(
                Strings.Tr("画像・動画・ファイルを入れるには、先に文書を保存します（文書の横の「名前_files」フォルダーに置きます）。保存先を選びますか？"),
                "mdEditor", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (answer != MessageBoxResult.OK || !await SaveDocumentAsAsync() || _filePath is null)
            {
                return;
            }
        }

        try
        {
            var documentFolder = _filePath is null ? null : Path.GetDirectoryName(_filePath)!;
            var filesFolder = _filePath is null ? null
                : Path.Combine(documentFolder!, Path.GetFileNameWithoutExtension(_filePath) + FilesSuffix);
            var folderIsNew = filesFolder is not null && !Directory.Exists(filesFolder);
            var inserted = new List<string>();
            var saved = new List<string>();
            var reused = new List<string>();

            // A file as this document refers to it: copied into its name_files folder
            // unless it is already there (an identical file there is used instead).
            string Copied(string full)
            {
                if (IsInside(full, filesFolder!))
                {
                    return LinkTarget(documentFolder!, full);
                }

                Directory.CreateDirectory(filesFolder!);
                var (target, same) = PlaceFile(Path.Combine(filesFolder!, Path.GetFileName(full)), full, null);
                (same ? reused : saved).Add(Path.GetFileName(target));
                return LinkTarget(documentFolder!, target);
            }

            foreach (var file in files)
            {
                var full = Path.GetFullPath(file);
                if (contents.TryGetValue(file, out var content))
                {
                    // Every file the content shows or links to comes along into name_files.
                    inserted.Add(documentFolder is null ? content.Markdown
                        : RebaseReferences(content.Markdown, content.Folders, Copied).Text);
                }
                else if (IsMediaPath(full))
                {
                    inserted.Add($"![{EscapeAlt(Path.GetFileNameWithoutExtension(full))}]({Copied(full)})");
                }
                else
                {
                    inserted.Add($"[{EscapeAlt(Path.GetFileName(full))}]({Copied(full)})");
                }
            }

            if (pasted is not null)
            {
                Directory.CreateDirectory(filesFolder!);
                var (target, _) = PlaceFile(Path.Combine(filesFolder!, $"image-{DateTime.Now:yyyyMMdd-HHmmss}{pastedExtension}"), null, pasted);
                saved.Add(Path.GetFileName(target));
                inserted.Add($"![]({LinkTarget(documentFolder!, target)})");
            }

            if (inserted.Count > 0)
            {
                // Content goes in as blocks of its own (blank lines around it), so its headings and tables stay intact.
                SendToPage(new { type = "insertText", text = string.Join("\n\n", inserted.Select(text => text.Trim('\n'))) + "\n", block = contents.Count > 0 });
            }

            var notices = new List<string>();
            if (contents.Count > 0)
            {
                notices.Add(string.Format(Strings.Tr("「{0}」の中身を入れました。ファイルとして開くときは、編集画面の外にドロップしてください（Ctrl+Z で取り消せます）。"),
                    string.Join(Strings.Language == "en" ? ", " : "、", contents.Keys.Select(Path.GetFileName))));
                // Japanese and Chinese both list names with 、.
            }

            if (filesFolder is not null)
            {
                var folderName = Path.GetFileName(filesFolder);
                if (saved.Count > 0)
                {
                    notices.Add(string.Format(Strings.Tr(folderIsNew ? "「{0}」フォルダーを作り、{1}を保存しました。" : "「{0}」フォルダーに{1}を保存しました。"),
                        folderName, NameList(saved)));
                }

                if (reused.Count > 0)
                {
                    notices.Add(string.Format(Strings.Tr("「{0}」フォルダーにある同じファイル{1}を使いました。"), folderName, NameList(reused)));
                }
            }

            if (notices.Count > 0)
            {
                SendToPage(new { type = "notice", text = string.Join(Strings.SentenceJoin, notices) });
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            ShowFileError("ファイルを写せません。文書のフォルダーに書き込めるか確かめて、もう一度お試しください", ex);
        }
    }

    private static string NameList(IEnumerable<string> names) => Strings.NameList(names);

    private static bool IsInside(string path, string folder) =>
        path.StartsWith(folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    // Writes a file (a copy of source, or bytes) at target, or, when another file
    // already has that name, at "name (1).ext", "name (2).ext", ... as browsers
    // do. A file with the same content already there is used instead (Same).
    private static (string Path, bool Same) PlaceFile(string target, string? source, byte[]? bytes)
    {
        var folder = Path.GetDirectoryName(target)!;
        var stem = Path.GetFileNameWithoutExtension(target);
        var extension = Path.GetExtension(target);
        for (var n = 1; ; n++)
        {
            if (!File.Exists(target))
            {
                if (source is not null)
                {
                    File.Copy(source, target);
                }
                else
                {
                    File.WriteAllBytes(target, bytes!);
                }

                return (target, false);
            }

            if (source is not null ? SameContent(source, target) : SameContent(bytes!, target))
            {
                return (target, true);
            }

            target = Path.Combine(folder, $"{stem} ({n}){extension}");
        }
    }

    private static bool SameContent(string a, string b)
    {
        if (string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (new FileInfo(a).Length != new FileInfo(b).Length)
        {
            return false;
        }

        using var first = File.OpenRead(a);
        using var second = File.OpenRead(b);
        return System.Security.Cryptography.SHA256.HashData(first).AsSpan().SequenceEqual(System.Security.Cryptography.SHA256.HashData(second));
    }

    private static bool SameContent(byte[] bytes, string file)
    {
        if (new FileInfo(file).Length != bytes.Length)
        {
            return false;
        }

        using var stream = File.OpenRead(file);
        return System.Security.Cryptography.SHA256.HashData(bytes).AsSpan().SequenceEqual(System.Security.Cryptography.SHA256.HashData(stream));
    }

    // The Markdown a file mdEditor opens adds when dropped on the editor, made as
    // Open makes it, and the folders its relative image and link targets start
    // from (none for text, tables, and data, whose content is not Markdown links).
    private async Task<(string Markdown, string[] Folders)> ConvertForInsertAsync(string path)
    {
        var folder = Path.GetDirectoryName(Path.GetFullPath(path))!;
        var imagesName = Path.GetFileNameWithoutExtension(path) + FilesSuffix;
        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension == ".docx")
        {
            var (markdown, assets, _) = ConvertDocx(path);
            return (markdown, [assets, folder]);
        }

        if (extension == ".eml")
        {
            var assets = NewImportFolder();
            return (MailImport.ToMarkdown(path, assets, imagesName, Strings.Language).Markdown, [assets, folder]);
        }

        var text = DecodeDocument(await File.ReadAllBytesAsync(path)).Text;
        if (TableFiles.IsTablePath(path))
        {
            return (TableFiles.ToMarkdown(text, extension == ".tsv" ? '\t' : ','), []);
        }

        if (extension == ".ipynb")
        {
            var assets = NewImportFolder();
            return (NotebookFormat.ToMarkdown(text, assets, imagesName).Markdown, [assets, folder]);
        }

        if (DataFiles.IsDataPath(path))
        {
            return ((extension == ".json" ? DataFiles.JsonToMarkdown(text) : DataFiles.XmlToMarkdown(text)).Markdown, []);
        }

        if (IsHtmlPath(path))
        {
            return (HtmlImport.ToMarkdown(text, null, relative => ExistsBeside(folder, relative)).Markdown, [folder]);
        }

        // Markdown goes in without its front matter, which only belongs at the top of a document.
        return extension is ".md" or ".markdown"
            ? (System.Text.RegularExpressions.Regex.Replace(text, @"\A\uFEFF?---[ \t]*\r?\n.*?\r?\n(?:---|\.\.\.)[ \t]*(?:\r?\n|\z)", string.Empty,
                System.Text.RegularExpressions.RegexOptions.Singleline), [folder])
            : (text, []);
    }

    private static readonly MarkdownPipeline ReferencePipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePreciseSourceLocation().Build();

    // Relative image and link targets in inserted content start from the file it
    // came from. Each one found as a file under one of the folders is replaced by
    // what relocate returns (with relocate null, they are only counted; when it
    // returns null, that target stays). Targets written through a reference
    // definition are left as they are.
    private static (string Text, int Count) RebaseReferences(string markdown, IReadOnlyList<string> folders, Func<string, string?>? relocate)
    {
        var edits = ReferenceEdits(markdown, folders, relocate ?? (_ => string.Empty));
        if (relocate is null)
        {
            return (markdown, edits.Count);
        }

        var text = new StringBuilder(markdown);
        foreach (var edit in edits.OrderByDescending(edit => edit.Start))
        {
            text.Remove(edit.Start, edit.Length).Insert(edit.Start, edit.Target);
        }

        return (text.ToString(), edits.Count);
    }

    private static List<(int Start, int Length, string Target)> ReferenceEdits(
        string markdown, IReadOnlyList<string> folders, Func<string, string?> relocate)
    {
        var edits = new List<(int Start, int Length, string Target)>();
        if (folders.Count == 0)
        {
            return edits;
        }

        foreach (var link in Markdig.Markdown.Parse(markdown, ReferencePipeline).Descendants<LinkInline>())
        {
            if (link.Url is not { Length: > 0 } url || link.UrlSpan.IsEmpty || link.UrlSpan.End >= markdown.Length
                || markdown.Substring(link.UrlSpan.Start, link.UrlSpan.Length).Trim('<', '>') != url
                || ResolveLocalFile(url, folders) is not { } local
                || relocate(local.Path) is not { } target)
            {
                continue;
            }

            edits.Add((link.UrlSpan.Start, link.UrlSpan.Length, target + local.Suffix));
        }

        return edits;
    }

    // A relative target ("images/a.png#x") as a file that exists under one of the folders, and its "#"/"?" part.
    private static (string Path, string Suffix)? ResolveLocalFile(string url, IReadOnlyList<string> folders)
    {
        if (url.StartsWith('#') || url.StartsWith('/') || url.StartsWith('\\')
            || System.Text.RegularExpressions.Regex.IsMatch(url, @"^[A-Za-z][A-Za-z0-9+.-]*:"))
        {
            return null;
        }

        var cut = url.IndexOfAny(['#', '?']);
        var part = cut < 0 ? url : url[..cut];
        var suffix = cut < 0 ? string.Empty : url[cut..];
        try
        {
            var relative = Uri.UnescapeDataString(part).Replace('/', Path.DirectorySeparatorChar);
            foreach (var folder in folders)
            {
                var full = Path.GetFullPath(Path.Combine(folder, relative));
                if (part.Length > 0 && File.Exists(full))
                {
                    return (full, suffix);
                }
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or UriFormatException)
        {
        }

        return null;
    }

    // How a document in this folder links to a file: a relative path, or a file:
    // address when the file is on another drive.
    private static string LinkTarget(string documentFolder, string file)
    {
        var relative = Path.GetRelativePath(documentFolder, file);
        return Path.IsPathRooted(relative) ? new Uri(file).AbsoluteUri : EscapeImagePath(relative);
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
    // --- New document from the clipboard (--paste) ---

    // Started from the desktop shortcut made in Settings: what the user copied goes into
    // the new document. One copied file that mdEditor opens is opened (as a drop outside
    // the editor); other files and pictures are placed as a drop on the editor places them;
    // formatted content (HTML from a web page or Word) becomes Markdown as "Open" does.
    private async Task PasteClipboardAsync()
    {
        try
        {
            if (Clipboard.ContainsFileDropList())
            {
                var files = Clipboard.GetFileDropList().Cast<string>().Where(File.Exists).ToList();
                // One file mdEditor opens is opened, as when dropped outside the editor;
                // several files (or one it cannot open) go into the new document as a drop
                // on the editor puts them.
                if (files.Count == 1 && (IsDocumentPath(files[0]) || IsMediaPath(files[0])))
                {
                    await LoadDocumentAsync(files[0]);
                    return;
                }

                if (files.Count > 0)
                {
                    await InsertFilesAsync(files, null);
                    return;
                }
            }

            var html = Clipboard.ContainsText(TextDataFormat.Html) ? Clipboard.GetText(TextDataFormat.Html) : null;
            var picture = ClipboardPicture();
            if (html is not null)
            {
                var (fragment, source) = ReadClipboardHtml(html);
                var document = new AngleSharp.Html.Parser.HtmlParser().ParseDocument(fragment);
                var onlyPictures = string.IsNullOrWhiteSpace(document.Body?.TextContent)
                    && document.QuerySelector("img") is not null;
                // "Copy image" in a browser puts both the picture and an <img> pointing at
                // the internet; the picture itself is what the user copied.
                if (!(onlyPictures && picture is not null)
                    && document.QuerySelector("h1, h2, h3, h4, h5, h6, table, ul, ol, a[href], img, strong, b, em, i, pre, blockquote") is not null)
                {
                    var markdown = HtmlImport.ToMarkdown(fragment, source, null).Markdown.Trim('\n');
                    if (markdown.Length > 0)
                    {
                        SendToPage(new { type = "insertText", text = markdown + "\n", block = true });
                        SendToPage(new { type = "notice", text = Strings.Tr("クリップボードの書式付きの内容（HTML）を Markdown にして、新しい文書に入れました。見出し・表・リンクなどは残り、色や配置は残りません。") });
                        return;
                    }
                }
            }

            if (picture is not null && !Clipboard.ContainsText())
            {
                await InsertFilesAsync([], picture, ".png");
                return;
            }

            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText().Replace("\r\n", "\n");
                SendToPage(new { type = "insertText", text = text.EndsWith('\n') ? text : text + "\n", block = false });
                SendToPage(new { type = "notice", text = Strings.Tr("クリップボードのテキストを新しい文書に入れました。") });
                return;
            }

            if (picture is not null)
            {
                await InsertFilesAsync([], picture, ".png");
                return;
            }

            SendToPage(new { type = "notice", text = Strings.Tr("クリップボードに、文書に入れられるもの（テキスト・画像・ファイル）がありません。コピーしてから、もう一度お試しください。") });
        }
        catch (Exception ex) when (ex is COMException or ExternalException or InvalidOperationException)
        {
            SendToPage(new { type = "notice", text = Strings.Tr("クリップボードを読み込めませんでした。少し待ってから、もう一度お試しください。") });
        }
    }

    // A copied picture as PNG: the "PNG" format keeps transparency; other apps only
    // put a bitmap.
    private static byte[]? ClipboardPicture()
    {
        if (Clipboard.GetData("PNG") is MemoryStream png && png.Length > 0)
        {
            return png.ToArray();
        }

        if (!Clipboard.ContainsImage() || Clipboard.GetImage() is not { } bitmap)
        {
            return null;
        }

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    // CF_HTML: a header (Version, StartHTML, ..., SourceURL) and the copied part
    // between <!--StartFragment--> and <!--EndFragment-->.
    private static (string Html, Uri? Source) ReadClipboardHtml(string data)
    {
        Uri? source = null;
        foreach (var line in data.Split('\n').TakeWhile(line => !line.TrimStart().StartsWith('<')))
        {
            if (line.StartsWith("SourceURL:", StringComparison.OrdinalIgnoreCase)
                && Uri.TryCreate(line["SourceURL:".Length..].Trim(), UriKind.Absolute, out var url)
                && url.Scheme is "https" or "http")
            {
                source = url;
            }
        }

        const string startMarker = "<!--StartFragment-->";
        var start = data.IndexOf(startMarker, StringComparison.Ordinal);
        var end = data.IndexOf("<!--EndFragment-->", StringComparison.Ordinal);
        var html = start >= 0 && end > start
            ? data[(start + startMarker.Length)..end]
            : data[Math.Max(0, data.IndexOf('<'))..];
        return (html, source);
    }

    // Settings > New from clipboard: the key is given to a desktop shortcut that starts
    // mdEditor with --paste, made only after the user agrees.
    private void SendPasteHotkey()
    {
        var letter = LoadSetting("PasteHotkey") ?? string.Empty;
        var exists = letter.Length == 1 && PasteShortcut.Exists(LoadSetting("PasteShortcut"), letter);
        SendToPage(new { type = "pasteHotkey", key = letter, exists });
    }

    private void SetPasteHotkey(string? letter)
    {
        letter = (letter ?? string.Empty).Trim().ToUpperInvariant();
        var oldPath = LoadSetting("PasteShortcut");
        var oldExists = !string.IsNullOrEmpty(oldPath) && File.Exists(oldPath);
        try
        {
            if (letter.Length == 0)
            {
                if (oldExists)
                {
                    var name = Path.GetFileNameWithoutExtension(oldPath!);
                    if (MessageBox.Show(string.Format(Strings.Tr("デスクトップのショートカット「{0}」を削除しますか？"), name),
                            "mdEditor", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        SendPasteHotkey();
                        return;
                    }

                    File.Delete(oldPath!);
                    SendToPage(new { type = "notice", text = string.Format(Strings.Tr("デスクトップのショートカット「{0}」を削除しました。"), name) });
                }

                SaveSetting("PasteHotkey", string.Empty);
                SaveSetting("PasteShortcut", string.Empty);
                SendPasteHotkey();
                return;
            }

            if (letter.Length != 1 || !char.IsAsciiLetterOrDigit(letter[0]))
            {
                SendPasteHotkey();
                return;
            }

            var path = PasteShortcut.NewPath();
            var shortcutName = Path.GetFileNameWithoutExtension(path);
            var key = PasteShortcut.Hotkey(letter);
            var message = string.Format(Strings.Tr("デスクトップに「{0}」のショートカットを作り、{1} を割り当てます。Windows では、このキーはデスクトップかスタートメニューのショートカットにしか割り当てられません。作ってもいいですか？"), shortcutName, key);
            var others = PasteShortcut.OthersUsing(letter, oldPath);
            if (others.Count > 0)
            {
                message += "\n\n" + string.Format(Strings.Tr("{0} は、ほかのショートカット（{1}）でも使われています。Windows がどちらを起動するかは決まっていないため、別のキーを選ぶことをお勧めします。"), key, NameList(others));
            }

            message += "\n\n" + Strings.Tr("mdEditor を削除（アンインストール）する前に、このキーを「なし」に戻してください。ショートカットは自動では消えません。");
            if (MessageBox.Show(message, "mdEditor", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                SendPasteHotkey();
                return;
            }

            if (oldExists && !string.Equals(oldPath, path, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(oldPath!);
            }

            PasteShortcut.Create(path, letter);
            SaveSetting("PasteHotkey", letter);
            SaveSetting("PasteShortcut", path);
            SendPasteHotkey();
            SendToPage(new { type = "notice", text = string.Format(Strings.Tr("デスクトップに「{0}」を作りました。テキストや画像をコピーしてから {1} を押すと、mdEditor の新しい文書に入ります。"), shortcutName, key) });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            SendPasteHotkey();
            MessageBox.Show($"{Strings.Tr("デスクトップのショートカットを作れませんでした。")}\n\n{ex.Message}",
                "mdEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task OpenSampleAsync(string? text, string? name)
    {
        if (text is null || !await ConfirmSaveChangesAsync())
        {
            return;
        }

        var fileName = string.IsNullOrWhiteSpace(name) ? Strings.Tr("新しい文書.md") : Path.GetFileName(name);
        // The bundled samples folder is where their images and video come from,
        // but never where Save As suggests (it is inside the app).
        var samples = Path.Combine(AppContext.BaseDirectory, "wwwroot", "samples");
        ApplyImported(text.Replace("\r\n", "\n"), Directory.Exists(samples) ? samples : null, fileName, false, null);
        _suggestFolder = null;
    }

    private void ApplyImported(string markdown, string? folder, string name, bool allowExternal, string? notice)
    {
        RememberLeftDocument(null);
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
        SendEncodingNotice();
        SendPreview();
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

    private static readonly Dictionary<string, string> VideoContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".mp4"] = "video/mp4",
            [".m4v"] = "video/mp4",
            [".webm"] = "video/webm",
            [".ogv"] = "video/ogg",
            [".mov"] = "video/quicktime"
        };

    private static bool IsVideoAddress(string url)
    {
        var path = url.Split('#', '?')[0];
        return VideoContentTypes.ContainsKey(Path.GetExtension(path));
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
        if (e.ResourceContext == CoreWebView2WebResourceContext.Media)
        {
            var range = e.Request.Headers.Contains("Range") ? e.Request.Headers.GetHeader("Range") : null;
            e.Response = uri.Host.Equals(DocumentHost, StringComparison.OrdinalIgnoreCase)
                ? ServeDocumentVideo(environment, uri, range)
                : await FetchInternetVideoAsync(environment, uri, range);
            return;
        }

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

    private const long VideoChunkBytes = 4L * 1024 * 1024;

    // "bytes=start-end" (end optional); null when there is no usable range.
    private static (long Start, long? End)? ParseRange(string? header)
    {
        var match = System.Text.RegularExpressions.Regex.Match(header ?? string.Empty, @"^bytes=(\d+)-(\d*)$");
        if (!match.Success || !long.TryParse(match.Groups[1].Value, out var start))
        {
            return null;
        }

        return (start, long.TryParse(match.Groups[2].Value, out var end) ? end : null);
    }

    private CoreWebView2WebResourceResponse ServeDocumentVideo(CoreWebView2Environment environment, Uri uri, string? rangeHeader)
    {
        var notFound = environment.CreateWebResourceResponse(null, 404, "Not Found", "");
        var folder = _filePath is null ? _importFolder : Path.GetDirectoryName(_filePath);
        if (folder is null)
        {
            return notFound;
        }

        try
        {
            var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var path = Path.GetFullPath(Path.Combine(root, Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/')));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                || !VideoContentTypes.TryGetValue(Path.GetExtension(path), out var type) || !File.Exists(path))
            {
                return notFound;
            }

            var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var length = file.Length;
            if (ParseRange(rangeHeader) is not { } range)
            {
                return environment.CreateWebResourceResponse(file, 200, "OK",
                    $"Content-Type: {type}\nContent-Length: {length}\nAccept-Ranges: bytes");
            }

            using (file)
            {
                var start = Math.Min(range.Start, Math.Max(0, length - 1));
                var end = Math.Min(range.End ?? long.MaxValue, Math.Min(length - 1, start + VideoChunkBytes - 1));
                var buffer = new byte[Math.Max(0, end - start + 1)];
                file.Position = start;
                file.ReadExactly(buffer);
                return environment.CreateWebResourceResponse(new MemoryStream(buffer), 206, "Partial Content",
                    $"Content-Type: {type}\nContent-Range: bytes {start}-{end}/{length}\nContent-Length: {buffer.Length}\nAccept-Ranges: bytes");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
                                       or NotSupportedException or SecurityException or UriFormatException)
        {
            return notFound;
        }
    }

    // An internet video the document may play (consent was given, so it has an
    // address): read by the app's own client a piece at a time, with the same
    // rules as images (https only, no cookies or Referer, never this PC or the
    // local network).
    private static async Task<CoreWebView2WebResourceResponse> FetchInternetVideoAsync(
        CoreWebView2Environment environment, Uri uri, string? rangeHeader)
    {
        var notFound = environment.CreateWebResourceResponse(null, 404, "Not Found", "");
        if (uri.Scheme != "https" || IsLocalName(uri.IdnHost)
            || (System.Net.IPAddress.TryParse(uri.IdnHost.Trim('[', ']'), out var literal) && !IsPublicAddress(literal)))
        {
            return notFound;
        }

        try
        {
            var range = ParseRange(rangeHeader) ?? (0, null);
            var last = range.Start + VideoChunkBytes - 1;
            if (range.End is { } end && end < last)
            {
                last = end;
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(range.Start, last);
            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var type = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (!response.IsSuccessStatusCode || !(type.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                                                   || type.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                                                   || type == "application/octet-stream"))
            {
                return notFound;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int read;
            while (buffer.Length < VideoChunkBytes && (read = await stream.ReadAsync(chunk)) > 0)
            {
                buffer.Write(chunk, 0, read);
            }

            if (buffer.Length > VideoChunkBytes)
            {
                buffer.SetLength(VideoChunkBytes);
            }

            buffer.Position = 0;
            var total = response.Content.Headers.ContentRange?.Length
                        ?? (response.StatusCode == System.Net.HttpStatusCode.OK ? response.Content.Headers.ContentLength : null);
            var first = response.StatusCode == System.Net.HttpStatusCode.PartialContent ? response.Content.Headers.ContentRange?.From ?? range.Start : 0;
            var lastByte = first + buffer.Length - 1;
            return environment.CreateWebResourceResponse(buffer, 206, "Partial Content",
                $"Content-Type: {type}\nContent-Range: bytes {first}-{lastByte}/{(total is { } length ? length.ToString(System.Globalization.CultureInfo.InvariantCulture) : "*")}\nContent-Length: {buffer.Length}\nAccept-Ranges: bytes");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or InvalidOperationException)
        {
            return notFound;
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
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            // OpenStreetMap asks every app that uses its map tiles to name itself.
            if (uri.Host.Equals("tile.openstreetmap.org", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.UserAgent.ParseAdd($"mdEditor/{AppVersion} (+https://github.com/5garashi/mdEditor)");
            }

            using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
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
            var text = PlainTextExport.Convert(_text, _markdownPipeline, Strings.Language);
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
        var tables = TableFiles.Tables(_text, _markdownPipeline, Strings.Language);
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

    // A ZIP to hand the document to someone: the Markdown file and its name_files
    // folder, under the names the links use. Files in the folder the document does
    // not refer to are left out and, when the user agrees, moved to the Recycle Bin
    // first. Without a folder (or a saved document), the ZIP holds the text only.
    private async Task ExportZipAsync(string path)
    {
        var title = Strings.Tr("配布用 ZIP の書き出し");
        var documentName = _filePath is null ? Path.GetFileNameWithoutExtension(path) + ".md" : Path.GetFileName(_filePath);
        var documentFolder = _filePath is null ? null : Path.GetDirectoryName(_filePath)!;
        var filesFolder = documentFolder is null ? null
            : Path.Combine(documentFolder, Path.GetFileNameWithoutExtension(_filePath) + FilesSuffix);
        byte[] bytes;
        try
        {
            bytes = EncodeDocument(_text, _encoding);
        }
        catch (EncoderFallbackException)
        {
            bytes = EncodeDocument(_text, Utf8NoBom);
        }

        try
        {
            var included = new List<string>();
            var outside = 0;
            if (documentFolder is not null && filesFolder is not null)
            {
                var referenced = ReferencedFiles(_text, documentFolder, filesFolder);
                outside = referenced.Count(file => !IsInside(file, filesFolder));
                if (Directory.Exists(filesFolder))
                {
                    var all = Directory.GetFiles(filesFolder, "*", SearchOption.AllDirectories);
                    included.AddRange(all.Where(referenced.Contains));
                    var unused = all.Where(file => !referenced.Contains(file)).ToList();
                    if (unused.Count > 0)
                    {
                        var shown = unused.Take(10).Select(file => Path.GetRelativePath(filesFolder, file)).ToList();
                        if (unused.Count > shown.Count)
                        {
                            shown.Add("…");
                        }

                        var answer = MessageBox.Show(
                            string.Format(Strings.Tr("「{0}」フォルダーに、文書から参照されていないファイルが {1} 個あります。ごみ箱に移してから ZIP にしますか？\n\n{2}\n\n「いいえ」を選ぶと、ファイルは残したまま、ZIP には入れません。"),
                                Path.GetFileName(filesFolder), unused.Count, string.Join("\n", shown)),
                            title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (answer == MessageBoxResult.Cancel)
                        {
                            return;
                        }

                        if (answer == MessageBoxResult.Yes)
                        {
                            MoveToRecycleBin(unused);
                            RemoveEmptyFolders(filesFolder);
                        }
                    }
                }
            }

            var tempPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $".mdEditor-{Guid.NewGuid():N}.tmp");
            try
            {
                await Task.Run(() =>
                {
                    using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
                    {
                        using (var entry = archive.CreateEntry(documentName, CompressionLevel.Optimal).Open())
                        {
                            entry.Write(bytes);
                        }

                        foreach (var file in included)
                        {
                            var name = Path.GetFileName(filesFolder!) + "/"
                                + Path.GetRelativePath(filesFolder!, file).Replace(Path.DirectorySeparatorChar, '/');
                            archive.CreateEntryFromFile(file, name, CompressionLevel.Optimal);
                        }
                    }

                    File.Move(tempPath, path, overwrite: true);
                });
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }

            var message = included.Count == 0
                ? string.Format(Strings.Tr("「{0}」を ZIP に保存しました。"), documentName)
                : string.Format(Strings.Tr("「{0}」と「{1}」フォルダーのファイル {2} 個を ZIP に保存しました。"),
                    documentName, Path.GetFileName(filesFolder!), included.Count);
            if (outside > 0)
            {
                message += "\n\n" + string.Format(Strings.Tr("文書から参照している「{0}」フォルダーの外のファイル {1} 個は、ZIP に入っていません。"),
                    Path.GetFileName(filesFolder!), outside);
            }

            MessageBox.Show($"{message}\n\n{path}", title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            ShowFileError("ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください", ex);
        }
    }

    // Every local file the document refers to: Markdown links and pictures (also
    // through reference definitions) and src/href/poster in raw HTML. A file in
    // name_files whose path from the document is written anywhere in the text
    // counts too, so a file is only left out when nothing in the document can mean it.
    private static HashSet<string> ReferencedFiles(string markdown, string documentFolder, string filesFolder)
    {
        var parsed = Markdig.Markdown.Parse(markdown, ReferencePipeline);
        var urls = parsed.Descendants<LinkInline>().Select(link => link.Url)
            .Concat(parsed.Descendants<LinkReferenceDefinition>().Select(definition => definition.Url))
            .Concat(System.Text.RegularExpressions.Regex.Matches(markdown,
                    """\b(?:src|href|poster|data)\s*=\s*(?:"([^"]*)"|'([^']*)')""",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                .Select(match => System.Net.WebUtility.HtmlDecode(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value)));

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in urls.OfType<string>())
        {
            if (ResolveLocalFile(url.Trim(), [documentFolder]) is { } local)
            {
                result.Add(local.Path);
            }
        }

        if (Directory.Exists(filesFolder))
        {
            foreach (var file in Directory.GetFiles(filesFolder, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(documentFolder, file);
                if (markdown.Contains(relative, StringComparison.OrdinalIgnoreCase)
                    || markdown.Contains(relative.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase)
                    || markdown.Contains(EscapeImagePath(relative), StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(file);
                }
            }
        }

        return result;
    }

    // To the Recycle Bin, so a file left out by mistake can be put back.
    private void MoveToRecycleBin(IReadOnlyList<string> files)
    {
        const uint delete = 3;
        const ushort silent = 0x4, noConfirmation = 0x10, allowUndo = 0x40, noErrorUi = 0x400;
        var operation = new ShFileOperationInfo
        {
            Hwnd = new WindowInteropHelper(this).Handle,
            Func = delete,
            From = string.Join('\0', files) + "\0\0",
            Flags = silent | noConfirmation | allowUndo | noErrorUi
        };
        if (SHFileOperation(ref operation) != 0 || operation.AnyOperationsAborted != 0)
        {
            throw new IOException("SHFileOperation could not move the files to the Recycle Bin.");
        }
    }

    // Folders left empty inside name_files (the folder itself stays).
    private static void RemoveEmptyFolders(string folder)
    {
        foreach (var child in Directory.GetDirectories(folder))
        {
            RemoveEmptyFolders(child);
            if (!Directory.EnumerateFileSystemEntries(child).Any())
            {
                Directory.Delete(child);
            }
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

    // The page printed to a tagged PDF with the same paper, margins, and backgrounds as
    // the print settings; false when DevTools cannot print it.
    private async Task<bool> PrintTaggedPdfAsync(string path, CoreWebView2PrintSettings settings)
    {
        try
        {
            var parameters = JsonSerializer.Serialize(new
            {
                paperWidth = settings.PageWidth,
                paperHeight = settings.PageHeight,
                marginTop = settings.MarginTop,
                marginBottom = settings.MarginBottom,
                marginLeft = settings.MarginLeft,
                marginRight = settings.MarginRight,
                landscape = settings.Orientation == CoreWebView2PrintOrientation.Landscape,
                printBackground = settings.ShouldPrintBackgrounds,
                displayHeaderFooter = settings.ShouldPrintHeaderAndFooter,
                generateTaggedPDF = true,
            });
            var result = await EditorWebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.printToPDF", parameters);
            using var json = JsonDocument.Parse(result);
            if (!json.RootElement.TryGetProperty("data", out var data) || data.GetString() is not { Length: > 0 } base64)
            {
                return false;
            }

            await File.WriteAllBytesAsync(path, Convert.FromBase64String(base64));
            return true;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or JsonException or FormatException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            return false;
        }
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
                // A tagged PDF keeps what is a heading, a paragraph, a list, or a table (opening
                // it again reads them back, and screen readers use them). WebView2's own print
                // has no such setting, so DevTools prints it; if that fails, the plain print.
                saved = await PrintTaggedPdfAsync(tempPath, settings)
                        || await EditorWebView.CoreWebView2.PrintToPdfAsync(tempPath, settings);
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
            _leavingClean = _filePath;
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
            MessageBoxResult.No => KeepDiscardedCopy(changed: true),
            _ => false
        };
    }

    // "No" to saving still keeps the document's last state (unless automatic
    // backup is off), so that a wrong "No" can be undone with "Back to previous document".
    // A document without changes keeps only its file (its text is already there).
    private bool KeepDiscardedCopy(bool changed)
    {
        _leavingClean = null;
        if (_autoBackup)
        {
            try
            {
                Directory.CreateDirectory(RecoveryFolder);
                var record = new DiscardedRecord(_filePath, EncodingId(_encoding), changed ? _text : string.Empty, _importFolder, _importName,
                    _suggestFolder, _importAssets, _importImageFile, DateTime.Now, changed);
                var tempPath = $"{DiscardedFile}.{Environment.ProcessId}.tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(record), Utf8NoBom);
                File.Move(tempPath, DiscardedFile, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
            {
                // Best effort, like the recovery copy.
            }
        }

        SendDiscardedState();
        return true;
    }

    // Called where another document takes the place of the open one (newPath is
    // its file, null for a new one): a saved document without changes that the
    // save question let go becomes the previous document. Opening the same file
    // again (another encoding, say) does not count.
    private void RememberLeftDocument(string? newPath)
    {
        if (_leavingClean is { } path && !_isDirty && string.Equals(path, _filePath, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(path, newPath, StringComparison.OrdinalIgnoreCase))
        {
            KeepDiscardedCopy(changed: false);
        }

        _leavingClean = null;
    }

    private static DiscardedRecord? ReadDiscarded()
    {
        try
        {
            return File.Exists(DiscardedFile) ? JsonSerializer.Deserialize<DiscardedRecord>(File.ReadAllText(DiscardedFile)) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException or JsonException)
        {
            return null;
        }
    }

    // Tells the page whether "Back to previous document" has something to bring back, and its name.
    private void SendDiscardedState()
    {
        if (!_pageReady)
        {
            return;
        }

        var record = _autoBackup ? ReadDiscarded() : null;
        SendToPage(new
        {
            type = "discarded",
            name = record is null ? null : record.Path is null ? record.ImportName ?? Strings.Tr("新しい文書.md") : Path.GetFileName(record.Path)
        });
    }

    // Opens the last discarded document as it was when it was closed: unsaved,
    // with the same file and encoding, so that saving writes it as usual.
    private async Task RestoreDiscardedAsync()
    {
        if (ReadDiscarded() is not { } record)
        {
            SendDiscardedState();
            return;
        }

        // A document without changes comes back from its file; when the file is
        // gone there is nothing to go back to, and the open document stays as it is.
        if (!record.Changed && (record.Path is null || !File.Exists(record.Path)))
        {
            TryDelete(DiscardedFile);
            SendDiscardedState();
            MessageBox.Show(
                string.Format(Strings.Tr("前の文書のファイルが見つかりません。移動・名前の変更・削除がされていないか確かめてください。\n\n{0}"), record.Path),
                "mdEditor", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!await ConfirmSaveChangesAsync())
        {
            return;
        }

        // The copy is used up, unless the question just now replaced it with the document closed in its place.
        if (ReadDiscarded() is { } current && current.SavedAt == record.SavedAt && current.Text == record.Text)
        {
            TryDelete(DiscardedFile);
        }

        if (!record.Changed)
        {
            // It had no changes: its file is opened again as it is now.
            await LoadDocumentAsync(record.Path!);
            SendDiscardedState();
            return;
        }

        RememberLeftDocument(record.Path);
        DeleteRecoveryCopy();
        _filePath = record.Path;
        _importFolder = record.ImportFolder;
        _importName = record.ImportName;
        _suggestFolder = record.SuggestFolder;
        _importAssets = record.ImportAssets;
        _importImageFile = record.ImportImageFile;
        _encoding = EncodingFromId(record.Encoding) ?? Utf8NoBom;
        _encodingSource = "detected";
        _hasUndecodableBytes = false;
        _text = record.Text;
        _changeVersion++;
        _isDirty = true;
        var notice = string.Format(Strings.Tr("「{0}」を、閉じたとき（{1}）の状態に戻しました。まだ保存していません。"),
            record.Path is null ? record.ImportName ?? Strings.Tr("新しい文書.md") : Path.GetFileName(record.Path), record.SavedAt.ToString("g"));
        ResetExternalConsent(false, notice);
        SendDocumentInfo();
        SendToPage(new { type = "setText", text = _text });
        SendEncodingNotice();
        SendToPage(new { type = "notice", text = notice });
        SendPreview();
        WriteRecoveryCopy();
        SendDiscardedState();
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
                mapsAllowed = _mapsAllowedHere,
                notifyRendered,
                forPrint,
                requestId = notifyRendered ? ++_previewRequestId : 0
            });
        }

        if (toPreviewWindow)
        {
            SendToPreviewWindow(new { type = "preview", html, lines, mapsAllowed = _mapsAllowedHere, notifyRendered = false, forPrint = false, requestId = 0 });
        }

        if (toPage && !forPrint)
        {
            UpdateExternalNotice();
        }
    }

    // HTML written in Markdown is kept (the sanitizer then keeps only what GitHub
    // allows); comments are notes in the source (such as file headers) and are
    // dropped. Front matter shows as a table, as on GitHub.
    private string RenderMarkdown(string text)
    {
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        _markdownPipeline.Setup(renderer);
        // Each image carries data-source, where it starts in the editor (which has "\n"
        // line breaks only), so Image size and position finds it from the preview.
        var returns = new List<int>();
        for (var index = text.IndexOf('\r'); index >= 0; index = text.IndexOf('\r', index + 1))
        {
            returns.Add(index);
        }

        int EditorOffset(int index)
        {
            var before = returns.BinarySearch(index);
            return index - (before >= 0 ? before : ~before);
        }

        renderer.ObjectRenderers.ReplaceOrAdd<HtmlBlockRenderer>(new RawHtmlBlockRenderer(EditorOffset));
        renderer.ObjectRenderers.ReplaceOrAdd<HtmlInlineRenderer>(new RawHtmlInlineRenderer(EditorOffset));
        renderer.ObjectRenderers.ReplaceOrAdd<Markdig.Extensions.Yaml.YamlFrontMatterHtmlRenderer>(new FrontMatterTableRenderer());
        var document = Markdown.Parse(text, _markdownPipeline);
        ResolveRelativeImages(document);
        foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage))
        {
            image.GetAttributes().AddProperty(
                "data-source", EditorOffset(image.Span.Start).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
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
        foreach (var paragraph in document.Descendants<ParagraphBlock>())
        {
            var only = paragraph.Inline?.FirstChild;
            if (only is null || only.NextSibling is not null)
            {
                continue;
            }

            switch (only)
            {
                case AutolinkInline { IsEmail: false } autolink when IsVideoAddress(autolink.Url):
                    only.ReplaceBy(new LinkInline(autolink.Url, string.Empty) { IsImage = true });
                    break;
                case LinkInline { IsImage: false, Url: { } url } link when IsVideoAddress(url):
                    // The link's text stays as the video's name.
                    link.IsImage = true;
                    break;
            }
        }

        foreach (var image in document.Descendants<LinkInline>().Where(link => link.IsImage))
        {
            switch (ResolveImageAddress(image.Url, allowExternal))
            {
                case (null, { } held):
                    image.GetAttributes().AddProperty("data-external-src", held);
                    image.Url = string.Empty;
                    blocked++;
                    break;
                case ({ } address, null):
                    image.Url = address;
                    break;
            }
        }

        _blockedImages = blocked;
    }

    // The address to load (or null to leave it), and the address held back for
    // consent (or null). Anything that is not http, https, or a relative path is
    // dropped (Address and Held both empty).
    private (string? Address, string? Held) ResolveImageAddress(string? url, bool allowExternal)
    {
        if (string.IsNullOrWhiteSpace(url) || url.StartsWith('#'))
        {
            return (null, null);
        }

        url = url.Trim();
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
        {
            if (absolute.Host == DocumentHost || absolute.Host == "mdeditor.local")
            {
                return (null, null);
            }

            if (absolute.Scheme is not ("http" or "https"))
            {
                return (string.Empty, null);
            }

            var local = IsLocalName(absolute.IdnHost)
                || (System.Net.IPAddress.TryParse(absolute.IdnHost.Trim('[', ']'), out var literal) && !IsPublicAddress(literal));
            return absolute.Scheme != "https" || !allowExternal || local
                ? (null, absolute.AbsoluteUri)
                : (null, null);
        }

        var hasFolder = _filePath is not null || _importFolder is not null;
        return hasFolder && !url.StartsWith('/') && Uri.TryCreate(new Uri($"https://{DocumentHost}/"), url.Replace('\\', '/'), out var resolved)
            ? (resolved.AbsoluteUri, null)
            : (null, null);
    }

    // An image written as HTML (<img>, or <source> in <picture>, as GitHub READMEs
    // use for light and dark pictures) gets the same treatment as a Markdown image.
    private void ResolveHtmlImage(AngleSharp.Dom.IElement element)
    {
        var allowExternal = _externalImages == "always" || (_externalImages == "ask" && _externalAllowedHere);
        if (element.TagName == "VIDEO" && element.GetAttribute("poster") is { } poster)
        {
            // A poster loads like an image, or not at all.
            switch (ResolveImageAddress(poster, allowExternal))
            {
                case ({ Length: > 0 } address, null):
                    element.SetAttribute("poster", address);
                    break;
                case (null, null) when Uri.TryCreate(poster, UriKind.Absolute, out var posterUri) && posterUri.Host == DocumentHost:
                    break;
                default:
                    element.RemoveAttribute("poster");
                    break;
            }
        }

        if (element.TagName is "IMG" or "VIDEO" || (element.TagName == "SOURCE" && element.HasAttribute("src")))
        {
            element.RemoveAttribute("srcset");
            if (element.HasAttribute("data-external-src") && string.IsNullOrEmpty(element.GetAttribute("src")))
            {
                return;
            }

            switch (ResolveImageAddress(element.GetAttribute("src"), allowExternal))
            {
                case (null, { } held):
                    element.SetAttribute("data-external-src", held);
                    element.SetAttribute("src", string.Empty);
                    _blockedImages++;
                    break;
                case ({ Length: 0 }, null):
                    element.Remove();
                    break;
                case ({ } address, null):
                    element.SetAttribute("src", address);
                    break;
            }

            return;
        }

        // <source srcset="a.png 1x, b.png 2x">: every address must be allowed to
        // load, or the source is dropped and the <img> inside <picture> is used.
        var candidates = (element.GetAttribute("srcset") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rewritten = new List<string>();
        foreach (var candidate in candidates)
        {
            var parts = candidate.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            switch (ResolveImageAddress(parts[0], allowExternal))
            {
                case ({ Length: > 0 } address, null):
                    rewritten.Add(parts.Length > 1 ? $"{address} {parts[1]}" : address);
                    break;
                case (null, null) when Uri.TryCreate(parts[0], UriKind.Absolute, out _):
                    rewritten.Add(candidate);
                    break;
                default:
                    element.Remove();
                    return;
            }
        }

        if (rewritten.Count == 0)
        {
            element.Remove();
            return;
        }

        element.SetAttribute("srcset", string.Join(", ", rewritten));
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
        _mapsAllowedHere = false;
        _importNotice = notice;
        _noticedBlocked = -1;
    }

    // An <img> written as HTML gets data-source too (see RenderMarkdown).
    private static string MarkImageSources(string html, int start, Func<int, int> editorOffset) =>
        System.Text.RegularExpressions.Regex.Replace(html, @"<img\b", match =>
                $"<img data-source=\"{editorOffset(start + match.Index).ToString(System.Globalization.CultureInfo.InvariantCulture)}\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    private sealed class RawHtmlBlockRenderer(Func<int, int> editorOffset) : HtmlObjectRenderer<HtmlBlock>
    {
        protected override void Write(HtmlRenderer renderer, HtmlBlock obj)
        {
            if (obj.Type == HtmlBlockType.Comment)
            {
                return;
            }

            foreach (var line in obj.Lines.Lines.Take(obj.Lines.Count))
            {
                renderer.Write(MarkImageSources(line.Slice.ToString(), line.Slice.Start, editorOffset));
                renderer.WriteLine();
            }
        }
    }

    private sealed class RawHtmlInlineRenderer(Func<int, int> editorOffset) : HtmlObjectRenderer<HtmlInline>
    {
        protected override void Write(HtmlRenderer renderer, HtmlInline obj)
        {
            if (!obj.Tag.StartsWith("<!--", StringComparison.Ordinal))
            {
                renderer.Write(MarkImageSources(obj.Tag, obj.Span.Start, editorOffset));
            }
        }
    }

    // Front matter (the --- block at the top) as tables, as GitHub shows it.
    private sealed class FrontMatterTableRenderer : HtmlObjectRenderer<Markdig.Extensions.Yaml.YamlFrontMatterBlock>
    {
        protected override void Write(HtmlRenderer renderer, Markdig.Extensions.Yaml.YamlFrontMatterBlock obj)
        {
            var lines = obj.Lines.Lines.Take(obj.Lines.Count).Select(line => line.Slice.ToString()).ToList();
            if (FrontMatter.Read(lines) is { } matter)
            {
                renderer.Write("<div class=\"front-matter-block\"").WriteAttributes(obj).Write(">")
                    .Write(FrontMatter.ToHtml(matter)).WriteLine("</div>");
            }
            else
            {
                // Not valid YAML: shown as written, as a code block.
                renderer.Write("<pre").WriteAttributes(obj).Write("><code>");
                renderer.WriteEscape(string.Join("\n", lines.Where(line => line.Trim() != "---")));
                renderer.WriteLine("</code></pre>");
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
        core.Settings.IsReputationCheckingRequired = false;
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
        core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);
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
                SendToPage(new
                {
                    type = "runCommand",
                    id = root.GetProperty("id").GetString(),
                    // Image size and position from the preview window: where the image starts.
                    at = root.TryGetProperty("at", out var at) && at.ValueKind == JsonValueKind.Number ? at.GetInt32() : (int?)null
                });
                break;
            case "attachPreview":
                window.Close();
                break;
            case "zoom":
                // Ctrl + 0 in the preview window goes back to the default zoom, as in this window.
                window.WebView.ZoomFactor = DefaultZoom;
                SaveSetting("PreviewZoom", DefaultZoom.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
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
                await HandleDropAsync(e, false);
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
        var stop = Strings.Language == "en" ? "." : "。"; // Chinese ends sentences with 。 as Japanese does
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
