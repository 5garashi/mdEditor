/*
  ファイル名 / File: Strings.cs
  概要 / Description: Japanese, English, and Simplified Chinese text for native dialogs and messages
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
*/
namespace MdEditor;

internal static partial class Strings
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
        ["プレビュー"] = "Preview",
        ["図や数式の描画が15秒以内に終わりませんでした。このまま書き出しますか？"] =
            "Drawing the diagrams and math did not finish within 15 seconds. Export anyway?",
        ["PNGの書き出し"] = "PNG export",
        ["PNGの保存先を選択してください"] = "Choose where to save the PNG",
        ["PNG 画像 (*.png)|*.png"] = "PNG images (*.png)|*.png",
        ["PNGを保存しました。"] = "The PNG was saved.",
        ["PNGを書き込めません。同じ名前のファイルをほかのアプリで開いている場合は閉じるか、別の名前で保存してください"] =
            "The PNG cannot be written. If another app has a file with this name open, close it there, or save under another name",
        ["PNGを書き出せません。少し待ってから、もう一度お試しください"] = "The PNG cannot be exported. Wait a moment, then try again",
        ["Wordの書き出し"] = "Word export",
        ["Word 文書の保存先を選択してください"] = "Choose where to save the Word document",
        ["Word 文書 (*.docx)|*.docx"] = "Word documents (*.docx)|*.docx",
        ["Word 文書を保存しました。"] = "The Word document was saved.",
        ["Word 文書を書き込めません。同じ名前の文書を Word で開いている場合は閉じるか、別の名前で保存してください"] =
            "The Word document cannot be written. If Word has a document with this name open, close it there, or save under another name",
        ["Word 文書を書き出せません。少し待ってから、もう一度お試しください"] = "The Word document cannot be exported. Wait a moment, then try again",
        ["開けるファイル|*.md;*.markdown;*.txt;*.html;*.htm;*.docx;*.eml;*.csv;*.tsv;*.json;*.xml;*.ipynb;*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp;*.mp4;*.m4v;*.webm;*.ogv;*.mov|Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|テキスト ファイル (*.txt)|*.txt|HTML ファイル (*.html;*.htm)|*.html;*.htm|Word 文書 (*.docx)|*.docx|メール (*.eml)|*.eml|CSV / TSV (*.csv;*.tsv)|*.csv;*.tsv|JSON / XML (*.json;*.xml)|*.json;*.xml|Jupyter Notebook (*.ipynb)|*.ipynb|PDF (*.pdf)|*.pdf|画像 (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp|動画 (*.mp4;*.m4v;*.webm;*.ogv;*.mov)|*.mp4;*.m4v;*.webm;*.ogv;*.mov|すべてのファイル (*.*)|*.*"] =
            "Files mdEditor can open|*.md;*.markdown;*.txt;*.html;*.htm;*.docx;*.eml;*.csv;*.tsv;*.json;*.xml;*.ipynb;*.pdf;*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp;*.mp4;*.m4v;*.webm;*.ogv;*.mov|Markdown files (*.md;*.markdown)|*.md;*.markdown|Text files (*.txt)|*.txt|HTML files (*.html;*.htm)|*.html;*.htm|Word documents (*.docx)|*.docx|E-mail (*.eml)|*.eml|CSV / TSV (*.csv;*.tsv)|*.csv;*.tsv|JSON / XML (*.json;*.xml)|*.json;*.xml|Jupyter Notebook (*.ipynb)|*.ipynb|PDF (*.pdf)|*.pdf|Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp|Videos (*.mp4;*.m4v;*.webm;*.ogv;*.mov)|*.mp4;*.m4v;*.webm;*.ogv;*.mov|All files (*.*)|*.*",
        ["Markdown ファイル (*.md)|*.md|テキスト ファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*"] =
            "Markdown files (*.md)|*.md|Text files (*.txt)|*.txt|All files (*.*)|*.*",
        ["Markdown ファイル (*.md)|*.md|テキスト（そのまま）(*.txt)|*.txt|テキスト（記号なし）(*.txt)|*.txt|PDF (*.pdf)|*.pdf|HTML (*.html)|*.html|PNG 画像 (*.png)|*.png|Word 文書 (*.docx)|*.docx|CSV（文書の中の表）(*.csv)|*.csv|TSV（文書の中の表）(*.tsv)|*.tsv|JSON（文書の中の表）(*.json)|*.json|Jupyter Notebook (*.ipynb)|*.ipynb|配布用 ZIP（文書と「名前_files」フォルダー）(*.zip)|*.zip"] =
            "Markdown files (*.md)|*.md|Text, as written (*.txt)|*.txt|Text, without marks (*.txt)|*.txt|PDF (*.pdf)|*.pdf|HTML (*.html)|*.html|PNG images (*.png)|*.png|Word documents (*.docx)|*.docx|CSV, the tables in the document (*.csv)|*.csv|TSV, the tables in the document (*.tsv)|*.tsv|JSON, the tables in the document (*.json)|*.json|Jupyter Notebook (*.ipynb)|*.ipynb|ZIP for sharing, the document and its name_files folder (*.zip)|*.zip",
        ["Word の文書を Markdown に変換して開きました。見出し・リスト・表・リンク・画像は戻り、色や配置は戻りません。保存すると Markdown ファイルになります（Word の文書は変わりません）。"] =
            "Converted the Word document to Markdown. Headings, lists, tables, links, and images come back; colors and layout do not. Saving makes a Markdown file (the Word document stays as it is).",
        ["Word の文書を読み込めません。Word で開けるか確かめて、.docx 形式で保存し直してから、もう一度開いてください"] =
            "The Word document cannot be read. Check that it opens in Word, save it again as .docx, then try again",
        ["https:// で始まるアドレスだけ開けます。アドレスを確かめて、もう一度入力してください。"] =
            "Only addresses that start with https:// can be opened. Check the address, then enter it again.",
        ["ページを読み込んでいます…"] = "Loading the page…",
        ["この PC や社内ネットワークのアドレスは開けません。"] =
            "Addresses on this PC or the local network cannot be opened.",
        ["https ではないアドレスに転送されたため、開きませんでした。"] =
            "The page forwarded to an address that is not https, so it was not opened.",
        ["ページが大きすぎるため、開きませんでした（10 MB まで）。"] =
            "The page is too large to open (up to 10 MB).",
        ["{0} を開きました。スクリプトは実行しないため、後から読み込まれる内容は入りません。保存すると Markdown ファイルになります。"] =
            "Opened {0}. Scripts do not run, so content a page loads later is not included. Saving makes a Markdown file.",
        ["ページを開けませんでした。アドレスとインターネットの接続を確かめて、もう一度お試しください。"] =
            "The page could not be opened. Check the address and your internet connection, then try again.",
        ["Markdown・HTML ファイル (*.md;*.markdown;*.html;*.htm)|*.md;*.markdown;*.html;*.htm|Markdown ファイル (*.md;*.markdown)|*.md;*.markdown|HTML ファイル (*.html;*.htm)|*.html;*.htm|すべてのファイル (*.*)|*.*"] =
            "Markdown and HTML files (*.md;*.markdown;*.html;*.htm)|*.md;*.markdown;*.html;*.htm|Markdown files (*.md;*.markdown)|*.md;*.markdown|HTML files (*.html;*.htm)|*.html;*.htm|All files (*.*)|*.*",
        ["開けるファイル（.md、.txt、.html、.docx、.pdf、.eml、.csv、.tsv、.json、.xml、.ipynb、画像、動画）は、1つだけドロップしてください。編集画面にドロップすると、ファイルの中身・画像・動画・リンクを文書に入れます。フォルダーはドロップできません。"] =
            "Drop only one file that mdEditor opens (.md, .txt, .html, .docx, .pdf, .eml, .csv, .tsv, .json, .xml, .ipynb, an image, or a video). Files dropped on the editor go into the document instead, as their content, images and videos, or links. Folders cannot be dropped.",
        ["mdEditor では開けないファイルです。編集画面にドロップすると、リンクとして入ります。"] =
            "mdEditor cannot open this file. Drop it on the editor to insert it as a link.",
        ["「{0}」の中身を入れました。ファイルとして開くときは、編集画面の外にドロップしてください（Ctrl+Z で取り消せます）。"] =
            "The content of {0} was inserted. To open it as a file, drop it outside the editor (Ctrl+Z undoes this).",
        ["前の文書のファイルが見つかりません。移動・名前の変更・削除がされていないか確かめてください。\n\n{0}"] =
            "The previous document's file cannot be found. Check whether it was moved, renamed, or deleted.\n\n{0}",
        ["「{0}」を、閉じたとき（{1}）の状態に戻しました。まだ保存していません。"] =
            "{0} is back as it was when it was closed ({1}). It has not been saved yet.",
        ["「{0}」フォルダーを作り、{1}を保存しました。"] = "Created the {0} folder and saved {1} in it.",
        ["「{0}」フォルダーに{1}を保存しました。"] = "Saved {1} in the {0} folder.",
        ["「{0}」フォルダーにある同じファイル{1}を使いました。"] = "Used the identical {1} already in the {0} folder.",
        ["「{0}」フォルダーを複製しています… {1} / {2}（{3}）"] = "Copying the {0} folder... {1} of {2} ({3})",
        ["「{0}」と「{1}」フォルダーを {2} に複製しました。"] = "Copied {0} and the {1} folder to {2}.",
        ["「{0}」と「{1}」フォルダーとして複製しました。"] = "Copied as {0} and the {1} folder.",
        ["「{0}」と「{1}」フォルダーとして {2} に複製しました。"] = "Copied as {0} and the {1} folder to {2}.",
        ["同じ名前の別のファイルがあった {0} 個は、番号（(1) など）を付けて保存し、リンクも合わせました。"] =
            "{0} file(s) whose names were taken by different files were saved with a number such as (1), and their links follow.",
        ["「{0}」フォルダーの外を指すリンク {1} 個は、元のファイルを指すように書き換えました。"] =
            "{1} link(s) to files outside the {0} folder now point to the original files.",
        ["ファイルの中身を読み込めません。「開く」で開けるか確かめてから、もう一度お試しください"] =
            "The file's content cannot be read. Check that it opens with Open, then try again",
        ["Jupyter Notebook を読み込めません。Jupyter で開けるか確かめて、もう一度開いてください"] =
            "The Jupyter Notebook cannot be read. Check that it opens in Jupyter, then open it again",
        ["Jupyter Notebook を Markdown に変換して開きました。コードは言語付きのコードの枠、実行結果は「output」の枠・画像・表になります。保存すると Markdown ファイルになります（ノートブックは変わりません）。"] =
            "The Jupyter Notebook was converted to Markdown. Code is in code blocks with its language, and outputs are \"output\" blocks, pictures, or tables. Saving creates a Markdown file (the notebook is unchanged).",
        ["Jupyter Notebook を保存しました。コードの枠はコードのセル、それ以外は Markdown のセルになります（実行結果は入りません）。"] =
            "The Jupyter Notebook was saved. Code blocks became code cells and everything else Markdown cells (outputs are not included).",
        ["Jupyter Notebook の書き出し"] =
            "Jupyter Notebook export",
        ["画像を開きました。画像の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、画像もそこに写します。"] =
            "The image was opened. Write a description below it and save it as a Markdown file. If you save in another folder, the image is copied there too.",
        ["動画を開きました。動画の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、動画もそこに写します。"] =
            "The video was opened. Write a description below it and save it as a Markdown file. If you save in another folder, the video is copied there too.",
        ["JSON の一覧を Markdown の表にして開きました。項目の名前を見出しの行にしています。"] =
            "The JSON list was opened as a Markdown table, with the field names as the header row. ",
        ["JSON として読めない所があるので、中身をそのままコードの枠に入れて開きました。"] =
            "Part of the file is not valid JSON, so its content was opened as it is in a code block. ",
        ["XML として読めない所があるので、中身をそのままコードの枠に入れて開きました。"] =
            "Part of the file is not valid XML, so its content was opened as it is in a code block. ",
        ["JSON をコードの枠に入れて開きました（表にできる形ではないため）。前後に説明を書き足せます。"] =
            "The JSON was opened in a code block (it is not a list that fits a table). You can add notes before and after it. ",
        ["XML をコードの枠に入れて開きました。前後に説明を書き足せます。"] =
            "The XML was opened in a code block. You can add notes before and after it. ",
        ["保存すると Markdown ファイルになります（元のファイルは変わりません）。"] =
            "Saving creates a Markdown file (the original file is unchanged).",
        ["画像・動画・ファイルを入れるには、先に文書を保存します（文書の横の「名前_files」フォルダーに置きます）。保存先を選びますか？"] =
            "To insert images, videos, or files, save the document first (they go in a \"name_files\" folder beside it). Choose where to save?",
        ["ファイルを写せません。文書のフォルダーに書き込めるか確かめて、もう一度お試しください"] =
            "The file cannot be copied. Check that you can write to the document's folder, then try again",
        ["JSONの書き出し"] =
            "JSON export",
        ["メールを読み込めません。メールアプリから .eml 形式で保存し直してから、もう一度開いてください"] =
            "The e-mail cannot be read. Save it again as .eml from your mail app, then open it again",
        ["メールを Markdown に変換して開きました。件名・差出人・宛先・日付と本文を読み込み、添付ファイルは名前だけを載せました。保存すると Markdown ファイルになります（メールのファイルは変わりません）。"] =
            "The e-mail was converted to Markdown. The subject, sender, recipients, date, and body were read; attachments are listed by name only. Saving creates a Markdown file (the e-mail file is unchanged).",
        ["表のファイルを Markdown の表にして開きました。1 行目を見出しの行にしています。保存すると Markdown ファイルになります（元のファイルは変わりません）。"] =
            "The table file was opened as a Markdown table, with the first row as the header. Saving creates a Markdown file (the original file is unchanged).",
        ["記号なしのテキストを保存しました。"] =
            "The text without marks was saved.",
        ["テキストの書き出し"] =
            "Text export",
        ["CSVの書き出し"] =
            "CSV export",
        ["TSVの書き出し"] =
            "TSV export",
        ["この文書には表がありません。表（| で区切った行）のある文書で保存してください。"] =
            "This document has no tables. Save a document that has a table (rows separated by |).",
        ["表を保存しました。"] =
            "The table was saved.",
        ["文書の中の表を、1 つずつ別のファイルに保存しました。"] =
            "Each table in the document was saved as its own file.",
        ["配布用 ZIP の書き出し"] =
            "ZIP for sharing",
        ["「{0}」フォルダーに、文書から参照されていないファイルが {1} 個あります。ごみ箱に移してから ZIP にしますか？\n\n{2}\n\n「いいえ」を選ぶと、ファイルは残したまま、ZIP には入れません。"] =
            "The {0} folder has {1} files the document does not refer to. Move them to the Recycle Bin, then make the ZIP?\n\n{2}\n\nWith No, the files stay where they are and are left out of the ZIP.",
        ["「{0}」を ZIP に保存しました。"] =
            "Saved {0} in a ZIP.",
        ["「{0}」と「{1}」フォルダーのファイル {2} 個を ZIP に保存しました。"] =
            "Saved {0} and {2} files from the {1} folder in a ZIP.",
        ["文書から参照している「{0}」フォルダーの外のファイル {1} 個は、ZIP に入っていません。"] =
            "{1} files the document refers to outside the {0} folder are not in the ZIP.",
        ["mdEditor で保存した HTML から、元の Markdown を開きました。保存すると Markdown ファイルになります（HTML ファイルは変わりません）。"] =
            "Opened the original Markdown kept in this HTML file by mdEditor. Saving makes a Markdown file (the HTML file stays as it is).",
        ["HTML を Markdown に変換して開きました。色や配置、図の絵は元に戻りません。保存すると Markdown ファイルになります（HTML ファイルは変わりません）。"] =
            "Converted the HTML to Markdown. Colors, layout, and drawn diagrams do not come back. Saving makes a Markdown file (the HTML file stays as it is).",
        ["HTML ファイル (*.html)|*.html"] = "HTML files (*.html)|*.html",
        ["新しい文書.html"] = "Untitled.html",
        ["HTMLの保存先を選択してください"] = "Choose where to save the HTML file",
        ["HTMLの書き出し"] = "HTML export",
        ["HTMLを保存しました。"] = "The HTML file was saved.",
        ["図や数式の描画が15秒以内に終わりませんでした。このままHTMLに書き出しますか？"] =
            "Drawing the diagrams and math did not finish within 15 seconds. Save as HTML anyway?",
        ["HTMLを書き込めません。同じ名前のファイルをほかのアプリで開いている場合は閉じるか、別の名前で保存してください"] =
            "The HTML file cannot be written. If another app has a file with this name open, close it there, or save under another name",
        ["HTMLを書き出せません。少し待ってから、もう一度お試しください"] =
            "The HTML file cannot be exported. Wait a moment, then try again",
        ["プレビューのウィンドウを開けませんでした。mdEditor を再起動してから、もう一度お試しください。"] =
            "The preview window could not be opened. Restart mdEditor, then try again.",
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
            "This document contains characters that could not be read in the selected encoding (�). Overwriting the file loses those parts of the original. Overwrite anyway?",
        ["見出しのスタイルがないため、文字の大きさから {0} 個の段落を見出しにしました。"] =
            "The document has no heading styles, so {0} paragraphs were made headings from their text size.",
        ["PDF を Markdown に変換して開きました。文字・見出し（文字の大きさから推測）・箇条書き・罫線のある表・コード・画像を取り出します。図は「（図）」の印に置き換え、数式と段組みの配置は元に戻りません。保存すると Markdown ファイルになります（PDF ファイルは変わりません）。"] =
            "Converted the PDF to Markdown. Text, headings (guessed from text size), lists, ruled tables, code, and images come back. Figures become a \"(Figure)\" marker, and math and column layout do not come back. Saving makes a Markdown file (the PDF stays as it is).",
        ["（図）"] = "(Figure)",
        ["この PDF からは文字を取り出せませんでした。画像だけでできた PDF（スキャンしたものなど）の可能性があります。"] =
            "No text could be taken from this PDF. It may be made of images only (a scan, for example).",
        ["ページが多いため、最初の {0} ページだけを開きました（全部で {1} ページ）。"] =
            "The PDF is long, so only the first {0} pages were opened ({1} pages in all).",
        ["パスワードで保護された PDF は開けません。保護を外した PDF を開いてください"] =
            "A password-protected PDF cannot be opened. Open a copy without the protection",
        ["PDF を読み込めません。PDF を表示するアプリで開けるか確かめてから、もう一度開いてください"] =
            "The PDF cannot be read. Check that it opens in a PDF viewer, then try again",
        ["mdEditor（クリップボードから新規作成）"] = "mdEditor (new from clipboard)",
        ["コピーしたテキストや画像を、mdEditor の新しい文書に入れます。"] = "Puts copied text or pictures in a new mdEditor document.",
        ["デスクトップのショートカット「{0}」を削除しますか？"] = "Delete the desktop shortcut \"{0}\"?",
        ["デスクトップのショートカット「{0}」を削除しました。"] = "Deleted the desktop shortcut \"{0}\".",
        ["デスクトップに「{0}」のショートカットを作り、{1} を割り当てます。Windows では、このキーはデスクトップかスタートメニューのショートカットにしか割り当てられません。作ってもいいですか？"] =
            "mdEditor will create the shortcut \"{0}\" on the desktop and give it {1}. Windows lets such keys start only shortcuts on the desktop or in the Start menu. Create it?",
        ["{0} は、ほかのショートカット（{1}）でも使われています。Windows がどちらを起動するかは決まっていないため、別のキーを選ぶことをお勧めします。"] =
            "{0} is also used by other shortcuts ({1}). Windows may start either one, so choosing another key is recommended.",
        ["mdEditor を削除（アンインストール）する前に、このキーを「なし」に戻してください。ショートカットは自動では消えません。"] =
            "Before removing (uninstalling) mdEditor, set this key back to None. The shortcut is not removed automatically.",
        ["デスクトップに「{0}」を作りました。テキストや画像をコピーしてから {1} を押すと、mdEditor の新しい文書に入ります。"] =
            "Created \"{0}\" on the desktop. Copy text or a picture, then press {1} to put it in a new mdEditor document.",
        ["デスクトップのショートカットを作れませんでした。"] = "Could not create the desktop shortcut.",
        ["クリップボードのテキストを新しい文書に入れました。"] = "Put the text from the clipboard in a new document.",
        ["クリップボードの書式付きの内容（HTML）を Markdown にして、新しい文書に入れました。見出し・表・リンクなどは残り、色や配置は残りません。"] =
            "Converted the formatted content on the clipboard (HTML) to Markdown and put it in a new document. Headings, tables, and links stay; colors and layout do not.",
        ["クリップボードに、文書に入れられるもの（テキスト・画像・ファイル）がありません。コピーしてから、もう一度お試しください。"] =
            "The clipboard has nothing that can go into a document (text, a picture, or files). Copy something, then try again.",
        ["クリップボードを読み込めませんでした。少し待ってから、もう一度お試しください。"] = "Could not read the clipboard. Wait a moment and try again."
    };

    public static string Language { get; set; } = DefaultLanguage();

    // Set by the --lang command-line option; takes priority over the saved choice.
    public static string? ForcedLanguage { get; set; }

    // "ja", "en", or "zh" (Simplified Chinese), from the Windows display language.
    public static string DefaultLanguage() =>
        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "ja" => "ja",
            "zh" => "zh",
            _ => "en"
        };

    // The language codes the app accepts (--lang, the page's Language list).
    public static string? Normalize(string? language) => language?.Trim().ToLowerInvariant() switch
    {
        "ja" or "ja-jp" => "ja",
        "en" or "en-us" => "en",
        "zh" or "zh-cn" or "zh-hans" => "zh",
        _ => null
    };

    // Source strings are Japanese; English or Chinese is looked up for those UI languages.
    public static string Tr(string japanese) => Language switch
    {
        "en" when English.TryGetValue(japanese, out var english) => english,
        "zh" when Chinese.TryGetValue(japanese, out var chinese) => chinese,
        _ => japanese
    };

    // Between sentences joined into one notice: a space in English, nothing in Japanese and Chinese.
    public static string SentenceJoin => Language == "en" ? " " : string.Empty;

    // Names in a notice: 「a.png」「b.pdf」, "a.png", "b.pdf", or “a.png”“b.pdf”.
    public static string NameList(IEnumerable<string> names) => Language switch
    {
        "en" => string.Join(", ", names.Select(name => $"\"{name}\"")),
        "zh" => string.Concat(names.Select(name => $"“{name}”")),
        _ => string.Concat(names.Select(name => $"「{name}」"))
    };
}
