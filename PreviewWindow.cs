/*
  File: PreviewWindow.cs
  Description: The preview in its own window (for a second display), with full screen and a remembered position
  Author: 5garashi.com Design Office
  Created: 2026-10-12
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Wpf;

namespace MdEditor;

// MainWindow owns the logic (messages, theme, document); this window only hosts
// the second WebView2 and keeps its own place on the screen.
internal sealed class PreviewWindow : Window
{
    public WebView2 WebView { get; } = new();

    public bool PageReady { get; set; }

    public WindowFullscreen Fullscreen { get; } = new();

    private readonly string? _savedPlacement;
    private readonly IntPtr _avoidMonitorOf;

    public PreviewWindow(string? savedPlacement, IntPtr mainWindowHandle)
    {
        _savedPlacement = savedPlacement;
        _avoidMonitorOf = mainWindowHandle;
        Content = WebView;
        Width = 900;
        Height = 820;
        MinWidth = 320;
        MinHeight = 320;
        // The editor keeps the keyboard focus when the preview opens.
        ShowActivated = false;
        SourceInitialized += (_, _) => PlaceWindow();
    }

    // "left,top,right,bottom,showCmd" from GetWindowPlacement; Windows moves a
    // window back on screen by itself if that display is no longer connected.
    public string? SavePlacement()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var placement = new NativeMethods.WindowPlacement { Length = Marshal.SizeOf<NativeMethods.WindowPlacement>() };
        if (handle == IntPtr.Zero || !NativeMethods.GetWindowPlacement(handle, ref placement))
        {
            return null;
        }

        var normal = placement.NormalPosition;
        var maximized = placement.ShowCmd == NativeMethods.ShowMaximized || Fullscreen.IsOn;
        return string.Join(",", new[] { normal.Left, normal.Top, normal.Right, normal.Bottom, maximized ? 1 : 0 }
            .Select(value => value.ToString(CultureInfo.InvariantCulture)));
    }

    private void PlaceWindow()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (TryParsePlacement(_savedPlacement, out var rect, out var maximized))
        {
            var placement = new NativeMethods.WindowPlacement
            {
                Length = Marshal.SizeOf<NativeMethods.WindowPlacement>(),
                ShowCmd = maximized ? NativeMethods.ShowMaximized : NativeMethods.ShowNormal,
                NormalPosition = rect
            };
            NativeMethods.SetWindowPlacement(handle, ref placement);
            return;
        }

        // First time: another display if there is one, otherwise beside the editor.
        var mainMonitor = NativeMethods.MonitorFromWindow(_avoidMonitorOf, NativeMethods.MonitorDefaultToNearest);
        NativeMethods.Rect? target = null;
        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (monitor, _, _, _) =>
        {
            if (monitor != mainMonitor && target is null)
            {
                var info = new NativeMethods.MonitorInfo { Size = Marshal.SizeOf<NativeMethods.MonitorInfo>() };
                if (NativeMethods.GetMonitorInfo(monitor, ref info))
                {
                    target = info.WorkArea;
                }
            }

            return true;
        }, IntPtr.Zero);
        if (target is { } area)
        {
            var width = (area.Right - area.Left) * 8 / 10;
            var height = (area.Bottom - area.Top) * 9 / 10;
            NativeMethods.SetWindowPos(handle, IntPtr.Zero,
                area.Left + (area.Right - area.Left - width) / 2, area.Top + (area.Bottom - area.Top - height) / 2,
                width, height, NativeMethods.SwpNoZOrder | NativeMethods.SwpNoActivate);
        }
        else if (NativeMethods.GetWindowRect(_avoidMonitorOf, out var main))
        {
            var width = (main.Right - main.Left) / 2;
            NativeMethods.SetWindowPos(handle, IntPtr.Zero, main.Right - width, main.Top + 40,
                width, main.Bottom - main.Top - 40, NativeMethods.SwpNoZOrder | NativeMethods.SwpNoActivate);
        }
    }

    private static bool TryParsePlacement(string? value, out NativeMethods.Rect rect, out bool maximized)
    {
        rect = default;
        maximized = false;
        var parts = value?.Split(',');
        if (parts is not { Length: 5 }
            || !parts.All(part => int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)))
        {
            return false;
        }

        var numbers = parts.Select(part => int.Parse(part, CultureInfo.InvariantCulture)).ToArray();
        rect = new NativeMethods.Rect { Left = numbers[0], Top = numbers[1], Right = numbers[2], Bottom = numbers[3] };
        maximized = numbers[4] == 1;
        return rect.Right - rect.Left >= 200 && rect.Bottom - rect.Top >= 200;
    }

    private static class NativeMethods
    {
        public const int ShowNormal = 1;
        public const int ShowMaximized = 3;
        public const uint MonitorDefaultToNearest = 2;
        public const uint SwpNoZOrder = 0x0004;
        public const uint SwpNoActivate = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPlacement
        {
            public int Length;
            public int Flags;
            public int ShowCmd;
            public Point MinPosition;
            public Point MaxPosition;
            public Rect NormalPosition;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public int Size;
            public Rect Monitor;
            public Rect WorkArea;
            public uint Flags;
        }

        public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr rect, IntPtr data);

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hwnd, ref WindowPlacement placement);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hwnd, ref WindowPlacement placement);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc callback, IntPtr data);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);
    }
}

// Window full screen (F11): no title bar or border, covering the display.
internal sealed class WindowFullscreen
{
    private WindowState _state;
    private WindowStyle _style;
    private ResizeMode _resizeMode;
    private Rect _bounds;

    public bool IsOn { get; private set; }

    public void Set(Window window, bool on)
    {
        if (IsOn == on)
        {
            return;
        }

        if (on)
        {
            _state = window.WindowState;
            _style = window.WindowStyle;
            _resizeMode = window.ResizeMode;
            var bounds = window.WindowState == WindowState.Normal
                ? new Rect(window.Left, window.Top, window.Width, window.Height)
                : window.RestoreBounds;
            _bounds = new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);

            window.WindowState = WindowState.Normal;
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowState = WindowState.Maximized;
        }
        else
        {
            window.WindowState = WindowState.Normal;
            window.WindowStyle = _style;
            window.ResizeMode = _resizeMode;
            window.Left = _bounds.Left;
            window.Top = _bounds.Top;
            window.Width = _bounds.Width;
            window.Height = _bounds.Height;
            window.WindowState = _state;
        }

        IsOn = on;
    }
}
