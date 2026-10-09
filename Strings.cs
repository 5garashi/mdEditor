/*
  File: Strings.cs
  Description: Japanese and English text for native dialogs and messages
  Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  Created: 2026-10-10
  License: MIT
  SPDX-License-Identifier: MIT
*/
namespace MdEditor;

internal static class Strings
{
    private static readonly Dictionary<string, string> English = new()
    {
        ["Microsoft Edge WebView2 Runtime が必要です。"] = "Microsoft Edge WebView2 Runtime is required.",
        ["mdEditor を起動できません"] = "mdEditor cannot start",
        ["Markdown ファイル（.md または .markdown）を1つだけドロップしてください。"] =
            "Drop exactly one Markdown file (.md or .markdown).",
        ["Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|すべてのファイル (*.*)|*.*"] =
            "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
        ["Markdown ファイル (*.md)|*.md|すべてのファイル (*.*)|*.*"] =
            "Markdown files (*.md)|*.md|All files (*.*)|*.*",
        ["PDF ファイル (*.pdf)|*.pdf"] = "PDF files (*.pdf)|*.pdf",
        ["新しい文書.md"] = "Untitled.md",
        ["新しい文書.pdf"] = "Untitled.pdf",
        ["ファイルが見つかりません"] = "The file was not found",
        ["フォルダーが見つかりません"] = "The folder was not found",
        ["ファイルを読み取れません"] = "The file cannot be read",
        ["ファイルのパスを確認してください"] = "Check the file path",
        ["このファイルのパス形式には対応していません"] = "This file path format is not supported",
        ["ファイルへのアクセスが許可されていません"] = "Access to the file is not permitted",
        ["ファイルを保存できません"] = "The file cannot be saved",
        ["PDFの保存先を選択してください"] = "Choose where to save the PDF",
        ["PDFを書き出せませんでした。ほかのPDF書き出しが完了してから、もう一度お試しください。"] =
            "The PDF could not be exported. Wait for any other PDF export to finish, then try again.",
        ["PDFの書き出し"] = "PDF export",
        ["PDFを保存しました。"] = "The PDF was saved.",
        ["PDFを書き込めません"] = "The PDF cannot be written",
        ["PDFの保存先を確認してください"] = "Check the PDF destination",
        ["PDFを書き出せません"] = "The PDF cannot be exported",
        ["PDFの書き出しを開始できません"] = "The PDF export cannot start",
        ["この環境ではPDFを書き出せません"] = "PDF export is not available in this environment",
        ["変更を保存しますか？"] = "Save your changes?",
        ["未保存の変更"] = "Unsaved changes",
        ["リンクを開けませんでした。"] = "The link could not be opened."
    };

    public static string Language { get; set; } = DefaultLanguage();

    // Set by the --lang command-line option; takes priority over the saved choice.
    public static string? ForcedLanguage { get; set; }

    public static string DefaultLanguage() =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja"
            ? "ja"
            : "en";

    // Source strings are Japanese; English is looked up when the UI language is English.
    public static string Tr(string japanese) =>
        Language == "en" && English.TryGetValue(japanese, out var english)
            ? english
            : japanese;
}
