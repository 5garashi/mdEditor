/*
  ファイル名 / File: PasteShortcut.cs
  概要 / Description: The desktop shortcut that starts mdEditor with --paste from a key (Ctrl+Alt+letter)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-23
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Runtime.InteropServices;

namespace MdEditor;

// Windows starts a program from a key only through a shortcut (.lnk) on the desktop or
// in the Start menu, so the key chosen in Settings is given to a desktop shortcut that
// the user agrees to. The Start menu is not used: in the Store (MSIX) version, files the
// app writes there may be kept in the app's private copy, where Windows does not see them.
internal static class PasteShortcut
{
    public static string Hotkey(string letter) => $"Ctrl+Alt+{letter}";

    // A Store (MSIX) install runs from WindowsApps, which a shortcut cannot start; the
    // package's execution alias (AppxManifest: mdEditor.exe) can, and passes --paste on.
    private static string Target()
    {
        var process = Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "mdEditor.exe");
        return process.Contains(@"\WindowsApps\", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "WindowsApps", "mdEditor.exe")
            : process;
    }

    public static string NewPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        Strings.Tr("mdEditor（クリップボードから新規作成）") + ".lnk");

    public static void Create(string path, string letter)
    {
        var shortcut = Open(path);
        try
        {
            shortcut.TargetPath = Target();
            shortcut.Arguments = "--paste";
            shortcut.Hotkey = $"CTRL+ALT+{letter}";
            shortcut.IconLocation = $"{Environment.ProcessPath},0";
            shortcut.Description = Strings.Tr("コピーしたテキストや画像を、mdEditor の新しい文書に入れます。");
            shortcut.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            shortcut.Save();
        }
        finally
        {
            Marshal.FinalReleaseComObject(shortcut);
        }
    }

    // Our shortcut is there and still starts mdEditor with --paste from that key.
    public static bool Exists(string? path, string letter)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return false;
        }

        try
        {
            var shortcut = Open(path);
            try
            {
                return string.Equals((string)shortcut.Arguments, "--paste", StringComparison.Ordinal)
                    && SameKey((string)shortcut.Hotkey, letter);
            }
            finally
            {
                Marshal.FinalReleaseComObject(shortcut);
            }
        }
        catch (COMException)
        {
            return false;
        }
    }

    // Other shortcuts that Windows also starts with Ctrl+Alt+letter. Windows does not
    // warn about the clash and starts either one.
    public static List<string> OthersUsing(string letter, string? own)
    {
        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
        };
        var found = new List<string>();
        foreach (var folder in folders.Where(folder => !string.IsNullOrEmpty(folder) && Directory.Exists(folder)))
        {
            try
            {
                var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };
                foreach (var link in Directory.EnumerateFiles(folder, "*.lnk", options))
                {
                    if (own is not null && string.Equals(link, own, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        var shortcut = Open(link);
                        try
                        {
                            if (SameKey((string)shortcut.Hotkey, letter))
                            {
                                found.Add(Path.GetFileNameWithoutExtension(link));
                            }
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(shortcut);
                        }
                    }
                    catch (COMException)
                    {
                        // A shortcut that cannot be read is skipped.
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A folder that cannot be listed is skipped.
            }
        }

        return found;
    }

    // WScript.Shell writes "CTRL+ALT+M" and reads it back as "Alt+Ctrl+M".
    private static bool SameKey(string? hotkey, string letter)
    {
        if (string.IsNullOrWhiteSpace(hotkey))
        {
            return false;
        }

        var parts = hotkey.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.ToUpperInvariant())
            .ToHashSet();
        return parts.SetEquals(["CTRL", "ALT", letter.ToUpperInvariant()]);
    }

    private static dynamic Open(string path)
    {
        var type = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new COMException("WScript.Shell is not available.");
        dynamic shell = Activator.CreateInstance(type)!;
        try
        {
            return shell.CreateShortcut(path);
        }
        finally
        {
            Marshal.FinalReleaseComObject(shell);
        }
    }
}
