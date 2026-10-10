/*
  File: MainWindow.xaml.cs
  Description: Loads, saves, renders, and safely previews Markdown documents
  Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  Created: 2026-10-10
  License: MIT
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
using System.Windows.Threading;
using Ganss.Xss;
using Markdig;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;

namespace MdEditor;

public partial class MainWindow : Window
{
    private readonly string? _initialPath;
    private readonly MarkdownPipeline _markdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
    private readonly HtmlSanitizer _htmlSanitizer = new();
    private readonly DispatcherTimer _previewTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(200)
    };
    private string _text = string.Empty;
    private string? _filePath;
    private bool _isDirty;
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

    public MainWindow(string? initialPath)
    {
        InitializeComponent();
        _initialPath = initialPath;
        _previewTimer.Tick += (_, _) =>
        {
            _previewTimer.Stop();
            SendPreview();
        };

        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedTags.UnionWith(
        [
            "a", "blockquote", "br", "code", "del", "div", "em", "h1", "h2", "h3", "h4", "h5",
            "h6", "hr", "img", "li", "ol", "p", "pre", "span", "strong", "table", "tbody", "td",
            "th", "thead", "tr", "ul"
        ]);
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedAttributes.UnionWith(
        [
            "alt", "class", "colspan", "href", "rowspan", "src", "title"
        ]);
        _htmlSanitizer.AllowedSchemes.Clear();
        _htmlSanitizer.AllowedSchemes.UnionWith(["http", "https", "mailto"]);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await EditorWebView.EnsureCoreWebView2Async();
        }
        catch (WebView2RuntimeNotFoundException ex)
        {
            MessageBox.Show(
                $"{Strings.Tr("Microsoft Edge WebView2 Runtime が必要です。")}\n\n{ex.Message}",
                Strings.Tr("mdEditor を起動できません"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Close();
            return;
        }

        EditorWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        EditorWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "mdeditor.local",
            Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            CoreWebView2HostResourceAccessKind.DenyCors);
        EditorWebView.CoreWebView2.WebMessageReceived += WebMessageReceived;
        EditorWebView.CoreWebView2.NavigationStarting += NavigationStarting;
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
            case "setLanguage":
                Strings.Language = root.GetProperty("lang").GetString() == "en" ? "en" : "ja";
                SendDocumentInfo();
                break;
            case "dropFile":
                await HandleDropAsync(e);
                break;
            case "change":
                _text =  root.GetProperty("text").GetString() ?? string.Empty;
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
                _previewRendered?.TrySetResult();
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
            var text = await File.ReadAllTextAsync(fullPath);
            _filePath = fullPath;
            _text = text;
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
            await File.WriteAllTextAsync(_filePath, _text, new UTF8Encoding(false));
            _isDirty = false;
            SendDocumentInfo();
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
            await File.WriteAllTextAsync(fullPath, _text, new UTF8Encoding(false));
            _filePath = fullPath;
            _isDirty = false;
            SendDocumentInfo();
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
            settings.PageWidth = 8.27;
            settings.PageHeight = 11.69;
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
                _previewRendered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                SendPreview(notifyRendered: true);
                await Task.WhenAny(_previewRendered.Task, Task.Delay(TimeSpan.FromSeconds(15)));
                _previewRendered = null;
                await EditorWebView.CoreWebView2.ExecuteScriptAsync(
                    "document.body.classList.add('printing-preview')");
                saved = await EditorWebView.CoreWebView2.PrintToPdfAsync(
                    dialog.FileName,
                    settings);
            }
            finally
            {
                await EditorWebView.CoreWebView2.ExecuteScriptAsync(
                    "document.body.classList.remove('printing-preview')");
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

    private void SendPreview(bool notifyRendered = false)
    {
        if (!_pageReady)
        {
            return;
        }

        var html = Markdown.ToHtml(_text, _markdownPipeline);
        SendToPage(new { type = "preview", html = _htmlSanitizer.Sanitize(html), notifyRendered });
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
}
