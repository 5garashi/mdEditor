/*
  File: App.xaml.cs
  Description: Starts the editor and forwards a file opened by Windows
  Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  Created: 2026-10-10
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Windows;

namespace MdEditor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        string? path = null;
        for (var i = 0; i < e.Args.Length; i++)
        {
            var arg = e.Args[i];
            string? value = null;
            if (arg.StartsWith("--lang=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg["--lang=".Length..];
            }
            else if (string.Equals(arg, "--lang", StringComparison.OrdinalIgnoreCase))
            {
                value = i + 1 < e.Args.Length ? e.Args[++i] : null;
            }
            else
            {
                path ??= arg;
                continue;
            }

            Strings.ForcedLanguage = value?.Trim().ToLowerInvariant() switch
            {
                "en" => "en",
                "ja" => "ja",
                _ => Strings.ForcedLanguage
            };
        }

        MainWindow = new MainWindow(path);
        MainWindow.Show();
    }
}
