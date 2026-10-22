/*
  ファイル名 / File: App.xaml.cs
  概要 / Description: Starts the editor and forwards a file opened by Windows
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MdEditor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Lets older documents in Shift_JIS and other ANSI code pages be read and saved.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        string? path = null;
        string? anchor = null;
        var paste = false;
        for (var i = 0; i < e.Args.Length; i++)
        {
            var arg = e.Args[i];
            string? value = null;
            if (string.Equals(arg, "--paste", StringComparison.OrdinalIgnoreCase))
            {
                // A new document with what is on the clipboard (the desktop shortcut made in Settings).
                paste = true;
                continue;
            }

            if (arg.StartsWith("--lang=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg["--lang=".Length..];
            }
            else if (string.Equals(arg, "--lang", StringComparison.OrdinalIgnoreCase))
            {
                value = i + 1 < e.Args.Length ? e.Args[++i] : null;
            }
            else if (string.Equals(arg, "--anchor", StringComparison.OrdinalIgnoreCase))
            {
                // Heading to show first, for a window opened from a link such as other.md#heading.
                anchor = i + 1 < e.Args.Length ? e.Args[++i] : null;
                continue;
            }
            else
            {
                path ??= arg;
                continue;
            }

            // ja, en, or zh (zh-CN and zh-Hans are accepted too); anything else is ignored.
            Strings.ForcedLanguage = Strings.Normalize(value) ?? Strings.ForcedLanguage;
        }

        MainWindow = new MainWindow(path, anchor, paste);
        MainWindow.Show();
    }

    // Keeps the app running after an unexpected error (including ones from async
    // event handlers) so the open document can still be saved.
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        MessageBox.Show(
            $"{Strings.Tr("予期しないエラーが発生しました。作業中の文書を保存してから、mdEditor を再起動してください。")}\n\n{e.Exception.Message}",
            "mdEditor",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
