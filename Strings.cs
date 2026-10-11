/*
  ファイル名 / File: Strings.cs
  概要 / Description: Japanese and English text for native dialogs and messages
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
namespace MdEditor;

internal static class Strings
{
    private static readonly Dictionary<string, string> English = new()
    {
        ["Microsoft Edge WebView2 Runtime が必要です。Microsoft のサイトから「WebView2 Runtime」をインストールしてから、もう一度起動してください。"] = "Microsoft Edge WebView2 Runtime is required. Install \"WebView2 Runtime\" from Microsoft's website, then start mdEditor again.",
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
        ["ファイルが見つかりません。移動・名前の変更・削除がされていないか確かめて、もう一度開いてください"] = "The file was not found. Check that it has not been moved, renamed, or deleted, then open it again",
        ["フォルダーが見つかりません。フォルダーがあるか確かめて、別の場所を選んでください"] = "The folder was not found. Check that it exists, or choose another location",
        ["ファイルを読み取れません。ほかのアプリで開いている場合は閉じてから、もう一度開いてください"] = "The file cannot be read. If another app has it open, close it there, then open it again",
        ["ファイルの場所の指定が正しくありません。場所と名前を確かめて、もう一度選んでください"] = "The file location is not valid. Check the location and name, then choose it again",
        ["この形式のファイルの場所には対応していません。PC のドライブか共有フォルダーにあるファイルを選んでください"] = "This kind of file location is not supported. Choose a file on a drive or shared folder of your PC",
        ["ファイルへのアクセスが許可されていません。読み書きできる場所（ドキュメントなど）を選ぶか、ファイルの権限を確かめてください"] = "Access to the file is not permitted. Choose a location you can write to (such as Documents), or check the file's permissions",
        ["ファイルを保存できません。ほかのアプリで開いている場合は閉じるか、「名前を付けて保存」で別の場所に保存してください"] = "The file cannot be saved. If another app has it open, close it there, or use Save As to save it somewhere else",
        ["PDFの保存先を選択してください"] = "Choose where to save the PDF",
        ["PDFを書き出せませんでした。ほかのPDF書き出しが完了してから、もう一度お試しください。"] =
            "The PDF could not be exported. Wait for any other PDF export to finish, then try again.",
        ["PDFの書き出し"] = "PDF export",
        ["PDFを保存しました。"] = "The PDF was saved.",
        ["PDFを書き込めません。同じ名前の PDF をほかのアプリで開いている場合は閉じるか、別の名前で保存してください"] = "The PDF cannot be written. If another app has a PDF with this name open, close it there, or save under another name",
        ["PDFの保存先の指定が正しくありません。保存先と名前を確かめて、もう一度選んでください"] = "The PDF destination is not valid. Check the location and name, then choose it again",
        ["PDFを書き出せません。少し待ってから、もう一度お試しください"] = "The PDF cannot be exported. Wait a moment, then try again",
        ["PDFの書き出しを開始できません。mdEditor を再起動してから、もう一度お試しください"] = "The PDF export cannot start. Restart mdEditor, then try again",
        ["この環境ではPDFを書き出せません。WebView2 Runtime を最新にしてから、もう一度お試しください"] = "PDF export is not available in this environment. Update WebView2 Runtime, then try again",
        ["変更を保存しますか？"] = "Save your changes?",
        ["未保存の変更"] = "Unsaved changes",
        ["リンクを開けませんでした。リンク先の場所を確かめて、ブラウザーやエクスプローラーから開いてください。"] = "The link could not be opened. Check where it points, then open it from your browser or File Explorer.",
        ["保存していない文書では、ほかのファイルへのリンクを開けません。先に文書を保存してください。"] =
            "Links to other files cannot be opened from a document that has not been saved. Save the document first.",
        ["リンク先のファイルが見つかりません。文書の中のリンクの書き方と、ファイルの場所を確かめてください。\n\n{0}"] = "The linked file was not found. Check how the link is written in the document and where the file is.\n\n{0}",
        ["画面の表示機能（WebView2）を初期化できませんでした。PC を再起動してから、もう一度起動してください。"] =
            "The display component (WebView2) could not be initialized. Restart your PC, then start mdEditor again.",
        ["この文書には {0} で保存できない文字があります。UTF-8 で保存しますか？"] =
            "This document contains characters that cannot be saved in {0}. Save it as UTF-8?",
        ["図や数式の描画が15秒以内に終わりませんでした。このままPDFに書き出しますか？"] =
            "Diagrams and math did not finish drawing within 15 seconds. Export the PDF anyway?",
        ["予期しないエラーが発生しました。作業中の文書を保存してから、mdEditor を再起動してください。"] =
            "An unexpected error occurred. Save your document, then restart mdEditor.",
        ["前回、保存されないまま終了した文書があります。\n\n文書: {0}\n最後のバックアップ: {1}\n\n復元しますか？\n「いいえ」を選ぶとバックアップを削除します。「キャンセル」を選ぶと、次回の起動時にもう一度確認します。"] =
            "A document was not saved when mdEditor last closed unexpectedly.\n\nDocument: {0}\nLast backup: {1}\n\nRestore it?\nNo deletes the backup. Cancel asks again at the next start.",
        ["未保存の変更を破棄して、選んだ文字コードで開き直しますか？"] =
            "Discard unsaved changes and reopen the file in the selected encoding?",
        ["この文書には、選んだ文字コードで読めなかった文字（�）が含まれています。上書きすると、元のファイルのその部分は失われます。上書き保存しますか？"] =
            "This document contains characters that could not be read in the selected encoding (�). Overwriting the file loses those parts of the original. Overwrite anyway?"
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
