<!--
  ファイル名 / File: CHANGELOG.md
  概要 / Description: mdEditor の版ごとの変更の記録 / Changes in each mdEditor version
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-12
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 変更の記録 / Changelog

版の番号は [VERSION](./VERSION) に書きます。新しい版は一番上に足します。各節の見出しは `## 版 — 日付` の形にします（GitHub の Release の本文に使います）。

The version number lives in [VERSION](./VERSION). Add each new version at the top, with a `## version — date` heading (used as the GitHub Release notes).

## 1.0.11 — 2026-10-17

- 「目次」（Ctrl+Shift+J）を加えた。見出しから目次を作ってカーソルの位置に入れる（`<!-- toc -->` と `<!-- /toc -->` の間）。もう一度押すと新しくなる。目次のリンクは、プレビュー・PDF・HTML・Word で見出しに移動する（Word では見出しにブックマークを付ける）。
- Graphviz の図（```dot、```graphviz、```gv）を、同梱した Viz.js 3.31.0（Graphviz 16.1.0）でオフラインで描くようにした。図の中のリンクと画像は取り除く。PlantUML は Java か外部のサーバーが要るため、今までどおりコードとして表示する。
- PDF・PNG・Word に保存するときと印刷するときは、長いコードの行を折り返すようにした（今までは横のスクロールバーが写ったり、はみ出した部分が切れたりした）。
- 「図・チャート表示テスト」のサンプルに、mdEditor の流れを表した色付きの Graphviz の図（16-1）と、コードの色分けの例（19。Python・JavaScript・TypeScript・C#・PowerShell・シェル・SQL・JSON・YAML・HTML・CSS・diff）を加えた。
- 言語名を書いたコードブロック（```python など）を、プレビューで色分けして表示するようにした（highlight.js 11.12.0 を同梱。約 40 の言語）。色は昼と夜の配色に合わせ、PDF・PNG・HTML・Word に保存したときも付く。言語名のないコードブロックは今までどおり色を付けない。

- Contents (Ctrl+Shift+J) inserts a table of contents built from the headings at the cursor, between <!-- toc --> and <!-- /toc -->; pressing it again updates it. Its links go to the headings in the preview, PDF, HTML, and Word (headings become Word bookmarks).
- Graphviz diagrams (```dot, ```graphviz, ```gv) are drawn offline with the bundled Viz.js 3.31.0 (Graphviz 16.1.0). Links and images inside a diagram are removed. PlantUML is still shown as code, since it needs Java or an outside server.
- Long code lines wrap when saving as PDF, PNG, or Word and when printing (before, a scroll bar was captured or the rest of the line was cut off).
- The Diagram and chart test sample has a colored Graphviz chart of how mdEditor works (16-1) and code color examples (19: Python, JavaScript, TypeScript, C#, PowerShell, shell, SQL, JSON, YAML, HTML, CSS, diff).
- Code blocks with a language name (```python and so on) are colored in the preview, with highlight.js 11.12.0 bundled (about 40 languages). The colors follow the day and night palettes and are kept in PDF, PNG, HTML, and Word. Code blocks without a language stay plain.

## 1.0.10 — 2026-10-16

- 配布物（setup.cmd で入れた版と Microsoft Store 版）に CHANGELOG.md を入れるようにした。LICENSE、THIRD-PARTY-NOTICES.md、PRIVACY.md と同じく、アプリの横に置く（Identity 12-6、品質確認規定 6-1）。

- The distribution (the setup.cmd install and the Microsoft Store version) now includes CHANGELOG.md next to the app, with LICENSE, THIRD-PARTY-NOTICES.md, and PRIVACY.md (Identity 12-6, Quality Rules 6-1).

## 1.0.9 — 2026-10-16

- 英語表示の「書き方」の一覧で、長い例に押されて「Insert」ボタンが右にはみ出していたのを直した（例の文を折り返す）。
- README と Microsoft Store の掲載用に、1.0.8 の画面の写真（日本語・英語）と説明文を加えた（docs/images/）。

- In the English Markdown quick reference, long examples no longer push the Insert buttons out of view (the examples wrap).
- Added 1.0.8 screenshots (Japanese and English) with captions for the README and the Microsoft Store listing (docs/images/).

## 1.0.8 — 2026-10-16

1.0.7 は公開せず、この版にまとめました。 / 1.0.7 was not released; its changes are part of this version.

- 「PNGに保存」（Ctrl+Shift+G）を加えた。プレビューを昼の配色で画像にし、長い文書は「名前-1.png」「名前-2.png」…に分けて保存する。
- 「Wordに保存」（Ctrl+Shift+D）を加えた。見出し・段落・リスト・引用・コード・表・リンク・画像を Word の標準のスタイルで書き出し（ナビゲーションや目次がそのまま使える）、図と数式はプレビューの画像として入れる。
- Word 文書（.docx）を開けるようにした。Markdown に変換して開き、見出し・リスト・表・リンク・画像が戻る（色や配置は戻らない）。画像は最初に保存するとき、Markdown ファイルの横に「名前-images」フォルダーとして置く。Word の文書は書き換えない。
- テキスト（.txt）を開いて、そのまま上書き保存できるようにした。
- 「名前を付けて保存」で、「開く」と同じようにファイルの種類から Markdown・テキスト・PDF・HTML・PNG・Word を選べるようにした。Markdown とテキストは文書そのものの保存（中身はそのまま）、ほかは書き出し。これに合わせて「PDFに保存」「HTMLに保存」「PNGに保存」「Wordに保存」のボタンをなくした（ショートカットキーはそのまま使える）。
- 「名前を付けて保存」に「テキスト（記号なし）」を加えた。プレビューに見えている文字だけを .txt にする（今までのテキストは「テキスト（そのまま）」）。
- メール（.eml）を開けるようにした。件名・差出人・宛先・日付・添付ファイルの名前と本文を Markdown にする。メールの中のインターネット上の画像は、確認するまで読み込まない。
- CSV・TSV を開くと Markdown の表になるようにした（Shift_JIS も可）。「名前を付けて保存」で、文書の中の表を CSV・TSV に書き出せる（UTF-8、BOM 付き）。
- 画像（.png、.jpg、.gif、.bmp、.webp）を開けるようにした。画像を表示する新しい文書になり、下に説明を書き足して Markdown として保存できる。
- 画像を編集画面にドラッグ＆ドロップするか貼り付ける（Ctrl+V）と、文書の横の「名前-images」フォルダーに置いて、カーソルの位置に入れるようにした。
- JSON を開けるようにした（一覧は表、それ以外はコードの枠）。XML はコードの枠に入れて開く。「名前を付けて保存」で、文書の中の表を JSON に書き出せる。
- Jupyter Notebook（.ipynb）を開けるようにした（Markdown のセル、言語付きのコード、実行結果の文字・画像・表）。「名前を付けて保存」で Jupyter Notebook に書き出せる（コードの枠がコードのセルになる。実行結果は入れない）。
- Microsoft Store 版と setup.cmd 版の両方で、mdEditor が開けるすべての形式（.md、.txt、.html、.htm、.docx、.eml、.csv、.tsv、.json、.xml、.ipynb、画像）を「プログラムから開く」と「既定のアプリ」の候補に出るようにした。既定のアプリ（ブラウザーなど）は変わらない。setup.cmd 版は候補に出すだけで、ほかに開けるアプリがない形式（.ipynb など）でも既定として使われない（以前の版の登録は取り除く）。Microsoft Store 版は、ほかに開けるアプリがない形式に限り、Windows の仕組みで mdEditor が開く。
- 「サンプル」は、編集欄を置き換えるのではなく、サンプルの名前の新しい文書として開くようにした。前の文書のファイル名やお知らせが残り、保存すると前のファイルに書き込まれてしまうことがあった。
- Markdown の中の `<br>`（属性のないもの）は、文字ではなく改行として表示するようにした（表のセルの中の改行に使う）。ほかの HTML は今までどおり文字として表示する。
- 読み上げの「言語」（自動・日本語・英語）と「音声」（PC に入っている音声から選ぶ）を「設定」に加えた。音声がないときは、追加のしかたを表示する。
- 表示倍率が 100% 以外でも、PNG と Word の図や数式の切り出しがずれないようにした。

- Save as PNG (Ctrl+Shift+G): the preview as pictures in the day palette; a long document is split into name-1.png, name-2.png, and so on.
- Save as Word (Ctrl+Shift+D): headings, paragraphs, lists, quotes, code, tables, links, and images use Word's own styles (so the navigation pane and a table of contents work); diagrams and math go in as pictures from the preview.
- Word documents (.docx) open as Markdown; headings, lists, tables, links, and images come back (colors and layout do not). Images are placed beside the Markdown file in a "name-images" folder on the first save. The Word document is never written.
- Text files (.txt) open and save back as they are.
- Save As now offers every format in its file type list, like Open: Markdown, text, PDF, HTML, PNG, and Word. Markdown and text save the document itself (the content unchanged); the others export it. The Save as PDF, HTML, PNG, and Word buttons are gone (their shortcut keys still work).
- Save As: Text without marks writes only the words the preview shows (the previous text type is now Text, as written).
- E-mail (.eml) files open as Markdown: subject, sender, recipients, date, attachment names, and the body. Internet images in a mail do not load until allowed.
- CSV and TSV files open as a Markdown table (Shift_JIS too), and Save As writes the tables in a document as CSV or TSV (UTF-8 with a BOM).
- Images (.png, .jpg, .gif, .bmp, .webp) open as a new document that shows the image, ready for a description and saving as Markdown.
- Images dropped on the editor or pasted (Ctrl+V) go in a "name-images" folder beside the document and are inserted at the cursor.
- JSON opens as a table (for a list) or a code block; XML opens in a code block. Save As writes the tables in a document as JSON.
- Jupyter Notebooks (.ipynb) open as Markdown (Markdown cells, code with its language, and text, picture, and table outputs). Save As writes a Jupyter Notebook (code blocks become code cells; outputs are not included).
- Both the Microsoft Store version and the setup.cmd install list every type mdEditor opens (.md, .txt, .html, .htm, .docx, .eml, .csv, .tsv, .json, .xml, .ipynb, and images) in "Open with" and Default apps. Default apps (such as the browser) do not change. The setup.cmd install only lists mdEditor as a choice, so it is never used as the default even for a type no other app opens (such as .ipynb); the earlier registration is removed. In the Microsoft Store version, Windows opens with mdEditor only the types no other app on the PC can open.
- Samples now open as a new document under the sample's name instead of replacing the editor text. Before, the previous file name and notice stayed, and saving could write the sample over the previous file.
- A bare `<br>` in Markdown now breaks the line instead of showing as text (useful inside table cells). Other HTML is still shown as text.
- Settings: read-aloud Language (Automatic, Japanese, English) and Voice (from the voices on this PC). When no voice is installed, the app explains how to add one.
- PNG and Word figures are cut out correctly at any zoom level.

- 「URLを開く」（Ctrl+Shift+K）を加えた。`https://` の Web ページを Markdown に変換して開く（スクリプトは実行しない。10 MB・20 秒まで）。ブラウザーからリンクをドラッグしても開ける。
- インターネット上の画像は、初期設定では読み込まず、お知らせ欄の「画像を読み込む」で読み込むようにした（その文書だけ）。「設定」→「インターネット」で「いつも読み込む」「読み込まない」も選べる。`http://` の画像は読み込まない。
- インターネットからの読み込み（ページと画像）は、すべてアプリ自身が行い、Cookie・認証情報・参照元（Referer）を送らず、この PC や社内ネットワークのアドレスには接続しない（Identity 4.14.0 の 13-3）。HTML の変換では、イベント属性・iframe・埋め込みのオブジェクトも取り除く。
- HTML の中の手元にない画像とリンクは、ページの元のアドレス（`<base>`、canonical、og:url）から探すようにした（ブラウザーの一時ファイルとして開いたページのロゴなど）。
- HTML ファイル（.html、.htm）を開けるようにした（「開く」とドラッグ＆ドロップ）。Markdown に変換して開き、見出し・段落・リスト・表・リンク・画像・数式は戻る（色や配置、図の絵は戻らない）。保存すると Markdown ファイルになり、HTML ファイルは書き換えない。
- ほかのページへ転送するだけの HTML（`<meta http-equiv="refresh">`。「最新のレポート」のページなど）は、ブラウザーと同じように転送先の HTML を開く（同じ PC のファイルだけ、3 段まで）。文書の中の .html へのリンクは、新しい mdEditor のウィンドウで開く。HTML から開いた文書は、保存する前でも、HTML と同じフォルダーの画像を表示し、リンクを開ける。Web コンポーネント（宣言的 Shadow DOM と slot）で作られたページ（保存した Microsoft Store のページなど）も、ブラウザーと同じように中身を組み立てて変換する。
- 「書き方」（F1）を「基本」「図（Mermaid）」「数式（KaTeX）」に分け、フローチャート・シーケンス図・ガントチャート・円グラフ・クラス図・状態遷移図・ER 図・マインドマップと、分数・平方根・総和・行列・場合分けなど 10 種類の数式、チェックリストを加えた。図は一覧の中に描いて見せる。
- 「設定」の「書き出し」に「HTMLに保存するとき、元の Markdown も入れる」を加えた（初期設定はオフ）。オンにして保存した HTML は、mdEditor で開くと書いたときのとおりに戻る。

- Open URL (Ctrl+Shift+K): an `https://` web page opens as Markdown (scripts never run; up to 10 MB and 20 seconds). Links dragged in from a browser open too.
- Images on the internet are not loaded by default; Load images in the notice bar loads them for that document. Settings > Internet can be Always load or Never load. Images over `http://` are never loaded.
- Every read from the internet (pages and images) is made by the app itself, with no cookies, credentials, or Referer, and never to addresses on this PC or the local network (Identity 4.14.0, 13-3). HTML conversion also removes event attributes, iframes, and embedded objects.
- Images and links in HTML that are not beside the file resolve from the page's own address (`<base>`, canonical, or og:url), such as the logo of a page opened from the browser cache.
- HTML files (.html, .htm) can be opened (Open and drag and drop). They are converted to Markdown; headings, paragraphs, lists, tables, links, images, and math come back, while colors, layout, and drawn diagrams do not. Saving makes a Markdown file and never writes the HTML file.
- An HTML page that only forwards to another (`<meta http-equiv="refresh">`, such as a "latest report" page) opens the target, as a browser would (local files only, up to three steps). Links to .html files in a document open in a new mdEditor window. A document opened from HTML shows images and opens links beside the HTML file even before it is saved. Pages built from web components (declarative shadow DOM and slots, such as a saved Microsoft Store page) are composed as a browser does before conversion.
- The Markdown syntax list (F1) is grouped into Basics, Diagrams (Mermaid), and Math (KaTeX), and adds flowchart, sequence, Gantt, pie, class, state, ER, and mind map diagrams, ten math examples (fractions, roots, sums, matrices, cases, and more), and checklists. Diagrams are drawn in the list.
- Settings > Export: "Keep the original Markdown in saved HTML" (off by default). HTML saved with it opens back in mdEditor exactly as written.

## 1.0.6 — 2026-10-14

- 「HTMLに保存」（Ctrl+Shift+M）を加えた。プレビューの見た目のまま、1 つの HTML ファイルに書き出す。図は SVG、数式は MathML で入り、文書と同じフォルダーの画像はファイルの中に埋め込む。開く側がダークモードなら夜の配色で表示し、外部への通信は起きない（文書の中のネット上の画像を除く）。
- 「サンプル」から、すべての図・チャート・数式を確かめる「図・チャート表示テスト」（日本語・英語）を開けるようにした（samples/ の 2 つのファイルをアプリに同梱）。
- 短い文書を PDF にすると、白紙のページが 1 枚増えていたのを直した。PDF の紙の幅でも狭い画面向けの配置が使われ、本文の最小の高さが 1 ページ分になっていたため。

- Added Save as HTML (Ctrl+Shift+M): the preview as it looks, in one HTML file. Diagrams are SVG, math is MathML, and images beside the document are embedded. It switches to the night palette when the reader's system is dark, and makes no connections when opened (except for web images in the document).
- Samples can open the "Diagram and chart test" of every supported diagram, chart, and formula (Japanese and English; the two files in samples/ are bundled with the app).
- Fixed an extra blank page in PDFs of short documents. The narrow-screen layout also applied to the PDF page width and made the body at least one page tall.

## 1.0.5 — 2026-10-12

- 幅の狭いウィンドウ（760px 以下や 200% の拡大）で、ツールバーのファイル名の行の下に大きな空白ができていたのを直した。

- Fixed a large blank space under the file name in the toolbar in narrow windows (760 px or less, or at 200% zoom).

## 1.0.4 — 2026-10-12

- プレビューを別のウィンドウで表示できるようにした（2 台目のディスプレイなど）。編集欄とのスクロールの連動、見出しへのリンク、保存などの操作はそのまま使える。ウィンドウの位置・大きさ・最大化と、別ウィンドウにしていたかどうかは次回の起動でも使う。別ウィンドウでも F11 で全画面表示にできる。
- 広い画面でプレビューの 1 行が長くなりすぎないよう、本文の幅を全角でおよそ 44 字までにした（中央に置く）。表・図・コード・数式は広く表示する。「設定」で制限しないこともできる。
- F11 などのウィンドウの全画面表示の処理を、本体とプレビューのウィンドウで共通にした。

- The preview can open in its own window (for example on a second display). Scrolling stays in sync with the editor, and heading links and commands such as Save keep working. The window's position, size, and maximized state, and whether the preview was in its own window, are remembered. F11 makes that window full screen too.
- On wide screens the preview text keeps a comfortable width (about 44 full-width characters), centered; tables, diagrams, code, and display math may be wider. Settings can turn this off.
- Window full screen (F11) works the same way in the main and preview windows.

## 1.0.3 — 2026-10-12

- 「このアプリについて」にバージョンを表示するようにした。
- バージョンを `VERSION` ファイルだけで管理するようにした。アプリと MSIX はそこから番号を読み取り、main にマージすると GitHub Actions がタグと Release を作る。

- About now shows the version.
- The version is kept only in the `VERSION` file. The app and the MSIX read it from there, and merging into main makes GitHub Actions create the tag and the Release.

## 1.0.2 — 2026-10-12

Microsoft Store に最初に申請した版です。 / The first version submitted to the Microsoft Store.

- 英語表示のときは、英語版のロゴタイプ（ライト・ダーク）を表示するようにした。

- In English, the header shows the English logotype (light and dark).

## 1.0.1 — 2026-10-12

- スタート・タスクバー・エクスプローラーのロゴと Store の画像を、公式マークの SVG から大きさごとに描くようにした（拡大による粗さをなくした）。

- Start, taskbar, and File Explorer logos and the Store art are drawn from the official mark SVG at each size, so nothing is enlarged.

## 1.0.0 — 2026-10-11

最初の MSIX（Store には出していない）。 / The first MSIX (not released in the Store).

- 編集とプレビューを並べた Markdown エディタ。スクロールの連動、見出しへのリンク、ほかのファイルへのリンク。
- Mermaid の図と KaTeX の数式。プレビューそのままの PDF（A4 / Letter）。
- 安全な保存、自動バックアップと異常終了からの復元、文字コードの判定（UTF-8、Shift_JIS、EUC-JP など）。
- キーボードだけで全操作、変更できるショートカット、画面読み上げソフト対応、読み上げ、読みやすさの設定、コントラスト テーマ、ダークモード。
- 日本語と英語の表示、サンプル、「このアプリについて」。

- Markdown editor with a side-by-side preview, synced scrolling, heading links, and links to other files.
- Mermaid diagrams and KaTeX math; PDF export that matches the preview (A4 or Letter).
- Safe saving, automatic backup and crash recovery, and encoding detection (UTF-8, Shift_JIS, EUC-JP, and more).
- Full keyboard operation with changeable shortcuts, screen reader support, read-aloud, readability settings, contrast themes, and dark mode.
- Japanese and English UI, samples, and About.
