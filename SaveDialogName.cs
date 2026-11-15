/*
  File: SaveDialogName.cs
  Description: The file name box of a save dialog selects the name without its extension
  Author: 5garashi.com Design Office
  Created: 2026-11-15
  License: MIT
  SPDX-License-Identifier: MIT
*/
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace MdEditor;

// In a save dialog, Windows selects the whole file name, extension included, and
// choosing another file type rewrites the extension and leaves nothing selected.
// Here the name without its extension is selected instead, as File Explorer does when
// renaming, so typing replaces the name and keeps the type:
// - when the file name box gets the focus (but not from a click, which places the caret);
// - when choosing a type rewrites the extension. A type chosen from the open list also
//   moves the focus back to the file name box; stepping through the types with the
//   arrow keys keeps the focus in the type box.
// The dialog runs on this thread, so its events arrive here while it is open.
internal static class SaveDialogName
{
    public static bool? Show(SaveFileDialog dialog, Window owner)
    {
        using var watcher = new Watcher(owner.Dispatcher);
        return dialog.ShowDialog(owner);
    }

    // The name before the last dot of the file name (not of a folder in a typed path).
    // A name with no extension, or one that is only an extension (".md"), is selected whole.
    internal static int NameLength(string text)
    {
        var nameStart = text.LastIndexOfAny(['\\', '/']) + 1;
        var dot = text.LastIndexOf('.');
        return dot > nameStart ? dot : text.Length;
    }

    private sealed class Watcher : IDisposable
    {
        private const uint EventObjectShow = 0x8002;
        private const uint EventObjectHide = 0x8003;
        private const uint EventObjectFocus = 0x8005;
        private const uint EventObjectValueChange = 0x800E;
        // Delivered through this thread's message loop, which the open dialog runs.
        private const uint WinEventOutOfContext = 0x0000;
        private const int EmGetSel = 0x00B0;
        private const int EmSetSel = 0x00B1;
        private const int FileNameId = 1001;
        private const long ListClosedGraceMs = 500;

        private readonly Dispatcher _dispatcher;
        private readonly WinEventProc _callback;
        private readonly IntPtr _hook;
        private bool _listOpen;
        private long _listClosedAt = long.MinValue / 2;

        public Watcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _callback = OnEvent;
            _hook = SetWinEventHook(EventObjectShow, EventObjectValueChange, IntPtr.Zero, _callback,
                (uint)Environment.ProcessId, GetCurrentThreadId(), WinEventOutOfContext);
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWinEvent(_hook);
            }
        }

        private void OnEvent(IntPtr hook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint thread, uint time)
        {
            if (hwnd == IntPtr.Zero)
            {
                return;
            }

            switch (eventType)
            {
                case EventObjectShow when ClassName(hwnd) == "ComboLBox":
                    _listOpen = true;
                    break;
                case EventObjectHide when ClassName(hwnd) == "ComboLBox":
                    _listOpen = false;
                    _listClosedAt = Environment.TickCount64;
                    break;
                case EventObjectFocus when IsFileNameBox(hwnd):
                    SelectName(hwnd, focus: false, onlyIfAllSelected: true);
                    break;
                case EventObjectValueChange when IsFileNameBox(hwnd) && GetFocus() != hwnd:
                    // Not typed here: the dialog rewrote the extension for the chosen type.
                    SelectName(hwnd, focus: _listOpen || Environment.TickCount64 - _listClosedAt < ListClosedGraceMs);
                    break;
            }
        }

        // After the dialog has finished its own handling of the event. On a focus, only the
        // dialog's own "select all" is replaced: a click has already placed the caret, and
        // a range the user dragged stays as it is.
        private void SelectName(IntPtr edit, bool focus, bool onlyIfAllSelected = false) => _dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (!IsWindow(edit))
            {
                return;
            }

            var text = WindowText(edit);
            if (onlyIfAllSelected)
            {
                SendMessage(edit, EmGetSel, out var start, out var end);
                if (start != 0 || end != text.Length || text.Length == 0)
                {
                    return;
                }
            }

            if (focus)
            {
                SetFocus(edit);
            }

            // From the start to just before the extension.
            SendMessage(edit, EmSetSel, IntPtr.Zero, NameLength(text));
        });

        private static bool IsFileNameBox(IntPtr hwnd)
        {
            if (ClassName(hwnd) != "Edit")
            {
                return false;
            }

            // The box carries the id itself; older dialogs give it to the combo box around it.
            for (var window = hwnd; window != IntPtr.Zero && ClassName(window) != "#32770"; window = GetParent(window))
            {
                if (GetDlgCtrlID(window) == FileNameId)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ClassName(IntPtr hwnd)
        {
            var name = new StringBuilder(64);
            return GetClassName(hwnd, name, name.Capacity) > 0 ? name.ToString() : string.Empty;
        }

        private static string WindowText(IntPtr hwnd)
        {
            var text = new StringBuilder(GetWindowTextLength(hwnd) + 1);
            GetWindowText(hwnd, text, text.Capacity);
            return text.ToString();
        }

        private delegate void WinEventProc(IntPtr hook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint thread, uint time);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr module, WinEventProc callback, uint processId, uint threadId, uint flags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hook);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder name, int capacity);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int capacity);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetDlgCtrlID(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hwnd);

        // The Unicode SendMessage: through the ANSI one, Windows reads the positions of
        // EM_SETSEL as bytes of the ANSI text (5 bytes of "新しい文書" are 3 characters).
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, out int start, out int end);
    }
}
