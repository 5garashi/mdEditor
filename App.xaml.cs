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
        MainWindow = new MainWindow(e.Args.FirstOrDefault());
        MainWindow.Show();
    }
}
