/*
  ファイル名 / File: FixRules.cs
  概要 / Description: The fix rules file fix-rules.json (rules that fix a document when it opens), read and written for the page
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-30
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
using System.IO;
using System.Text;
using System.Text.Json;

namespace MdEditor;

// The page owns the rules and their meaning; the host only keeps the file, so the rules
// survive clearing the WebView's data and can be copied to another PC (or kept in a
// settings repository) as one file.
internal static class FixRules
{
    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mdEditor", "fix-rules.json");

    // The file's text, or null when there is none or it is not JSON (the page then starts
    // from its own rules).
    public static string? Read(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var text = File.ReadAllText(path, Encoding.UTF8);
            using var _ = JsonDocument.Parse(text);
            return text;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    // Written beside the file first and then moved over it, so a failed write never leaves
    // half a file. The text must be JSON.
    public static void Write(string path, string json)
    {
        using (JsonDocument.Parse(json))
        {
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, json, new UTF8Encoding(false));
        File.Move(temporary, path, overwrite: true);
    }
}
