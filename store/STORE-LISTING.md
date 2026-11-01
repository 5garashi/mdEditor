<!--
  File: STORE-LISTING.md
  Description: Microsoft Store listing text (English, Japanese, and Simplified Chinese) and screenshot plan for mdEditor
  Author: 5garashi.com Design Office
  Created: 2026-10-11
  License: MIT
  SPDX-License-Identifier: MIT
-->
# Microsoft Store listing / ストア掲載文

Paste each section into the matching Partner Center field. Check the character limits there before submitting. Marks that the text names as written (`\(…\)`, `**`, `$`) are in code spans so this file shows them; copy from the preview, or leave out the backticks, so the Store shows them as plain text.

Since 1.0.16 the listing leads with opening PDF and Word files as Markdown and saving them back as PDF or Word, offline. Say what does not come back from a PDF (column layout, tables without cell borders, scans; figures and math become pictures) wherever PDF import is described, and do not claim exact conversion.
1.0.16 から、掲載文の先頭は「PDF や Word を Markdown として開き、PDF や Word で渡せる（PC の中だけで）」にしています。PDF の取り込みを書くところには、戻らないもの（段組み・罫線のない表・スキャンした PDF。図と数式は画像になる）を必ず併記し、正確に変換できるとは書きません。

## English (en-US)

**Product name:** mdEditor by 5garashi.com

**Short description (one line):**
Open PDF and Word files as Markdown, reproduce Markdown from AI chats as best it can, and save as PDF or Word. Offline, private, ad-free.

**Description:**

Turn PDFs, Word files, e-mails, and web pages into clean Markdown, and back into PDF or Word, offline on Windows. Open a file and its text, headings, lists, and pictures come back as Markdown you can read and fix right away, beside a live preview. There is nothing else to set up: no Python, no command line, no account. Your documents stay on your PC, with no ads.

Open as Markdown
- PDF: the text comes back in reading order, wrapped lines join into paragraphs, and headings (from text size and numbering), lists, the table of contents, and pictures come back. Ruled tables and code come back too. Diagrams, charts, and math are redrawn as pictures from their lines and labels; column layout and tables without cell borders do not come back (their text remains), and no text can be taken from a PDF that is only scanned images.
- Word: headings, lists, tables, links, and pictures come back (colors and layout do not). When a document has no heading styles, headings are guessed from text size.
- Also HTML, web pages, e-mail (.eml), CSV, TSV, JSON, XML, Jupyter Notebooks, and images. The original file is never changed, and it can be shown beside the preview to compare.
- Check and fix the result before you share it or paste it into an AI chat.

Markdown from AI chats, reproduced as closely as possible
- Open or paste an answer from an AI chat and mdEditor reproduces it as closely as it can. Math written `\(…\)` and `\[…\]`, `$$…$$` inside a line, and math followed straight by Japanese or Chinese text show as math. Not every way of writing can be fixed, so check the result.
- Bold whose `**` shows as written (common beside Japanese brackets) and math that lost its `$` are found when the document opens and fixed, with a button to put them back.
- Citation marks, hidden characters, and other leftovers that AI chats put in copied text are taken out by ready-made fix rules, and rules of your own can fix more.
- Each line break shows as written, and a check underlines mistakes such as a skipped heading level.

Save as PDF and Word
- One click saves a PDF that looks like the preview: diagrams, math, tables, 3D models, maps, and colored code included, on A4 or Letter (Auto follows your region). Pages are numbered, and the PDF is tagged so screen readers can follow its headings, lists, and tables.
- Save As also writes Word, HTML, PNG, CSV, TSV, JSON, Jupyter Notebook, and plain text, and a ZIP for sharing that holds the document and its name_files folder.
- Printing and PDF always use a white page, even in dark mode.

Write and see
- Type on the left and the preview on the right updates at once. Both sides scroll together.
- Markdown is colored as you type, and a check underlines mistakes such as a skipped heading level or a link to nowhere.
- Writing help: Ctrl+B and Ctrl+I for bold and italic, lists that go on when you press Enter, tables lined up in one key, numbered headings, a URL pasted on selected text becomes a link, and suggestions for links and math.
- Draw flowcharts and Gantt charts (Mermaid), Graphviz diagrams, and formulas (KaTeX) with a few lines of text. Math written the way AI chats write it, `\(…\)` and `\[…\]`, shows too.
- Turn 3D models (text STL) with the mouse, and draw maps (GeoJSON, TopoJSON) on a simple world map, all offline.
- Code is colored for about 40 languages, and Contents builds a table of contents from your headings.
- Reads Markdown the way GitHub does: alerts, front matter, math blocks, emoji short names, collapsible sections, and more. Videos in a document play too.
- Put the preview in its own window on a second display, or expand it to fill the window and read without the editor.
- Drop or paste pictures into the editor; they are saved beside your document. Copy text, a picture, or a file in any app, press your key, and it opens as a new document.
- New to Markdown? Open a built-in sample, or press F1 and insert examples with one click.

Your work stays safe
- Saving never damages your original file, even if something goes wrong mid-write.
- Unsaved changes are backed up automatically and come back after a crash.
- Shift_JIS, EUC-JP, and UTF-8 are detected, so older Japanese files open correctly.

Private and quiet
- The content of your documents is never sent anywhere. PDF and Word files are converted on your PC. No tracking, no accounts, no ads.
- Internet images, videos, and detailed maps load only when you allow them.

Made for everyone
- Everything works from the keyboard, and shortcuts can be changed.
- Works with screen readers and reads your document, or just the text you select, aloud with the voices on your PC. BIZ UD typeface, adjustable text size and line spacing, contrast themes, and dark mode.

Available in Japanese, English, and Simplified Chinese. Made by 5garashi.com Design Office. Open About in the app for the company, support, and privacy policy links.

**What's new in this version:**
- Open PDFs as Markdown, on your PC: text, headings, lists, the table of contents, tables, code, and pictures come back, and diagrams and math are redrawn as pictures. Tagged PDFs are read by their tags, and the original can be shown beside the preview. Column layout does not come back.
- Saved PDFs have page numbers and are tagged for screen readers. Word files without heading styles get headings from text size.
- Writing help: Markdown is colored as you type. Ctrl+B / Ctrl+I add bold and italic, lists go on with Enter and Tab, Ctrl+Shift+W lines up a table, Ctrl+Shift+N numbers the headings, a URL pasted on selected text makes a link, and links and math commands are suggested.
- A Markdown check underlines mistakes such as a skipped heading level or a link to nowhere.
- Markdown from AI chats is reproduced as closely as possible: `\(…\)` and `\[…\]` math shows as math, and bold whose `**` shows beside Japanese brackets and math that lost its `$` are fixed on opening, with a button to undo.
- Line breaks in a paragraph show as written (can be turned off).
- Simplified Chinese, New from clipboard, and about twice as fast to start.
- 3D models (STL), maps, Graphviz, GitHub-style Markdown (alerts, front matter, collapsible sections, videos), and code colored for about 40 languages.
- Save As lists every format, including a ZIP for sharing. Adjust image sets a picture's size and position. Drop or paste pictures into the editor.
- Internet images and videos load only when you allow them.

**Features (bullets):**
- Open PDF and Word files as Markdown, offline
- Headings, lists, tables, code, and pictures come back from PDFs
- Save as PDF in one click, looking as the preview does
- Save as Word, HTML, PNG, CSV, JSON, text, or a ZIP
- Live preview beside your text, scrolling together
- Markdown from AI chats reproduced as closely as possible
- Preview on a second display, following the line you edit
- Markdown colored as you type, with a check for mistakes
- Bold, italic, lists, tables, and heading numbers in one key
- Mermaid and Graphviz diagrams and math, offline
- 3D models (STL) and maps (GeoJSON), offline
- Colored code for about 40 languages, and a table of contents
- GitHub-style alerts, front matter, and collapsible sections
- Also opens HTML, e-mail, CSV, JSON, Jupyter Notebooks, and images
- Drop or paste pictures straight into your document
- Copy anything, press a key, and it opens as a new document
- Safe saving and automatic crash recovery
- Opens Shift_JIS, EUC-JP, and UTF-8 files correctly
- No ads, no tracking, no account; web content loads only when you allow it
- Keyboard, screen reader, read-aloud, and dark mode support

**Search terms (up to 7):** markdown editor, PDF to Markdown, Word to Markdown, Markdown to PDF, AI markdown, mermaid, preview

**Privacy policy URL:** https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md

**Support contact:** https://github.com/5garashi/mdEditor/issues

**Screenshots (1366 x 768, the Store minimum; English UI) with captions:**
1. `docs/images/10-pdf-open.en.png` — Open a PDF and it becomes Markdown on your PC: text, headings, lists, tables, and pictures come back. The notice says the conversion is best-effort.
2. `docs/images/9-pdf.en.png` — One click saves a PDF that looks like the preview: diagrams, tables, math, and colored code included, on A4 or Letter.
3. `docs/images/11-ai-chat.en.png` — Open an answer from an AI chat and it is reproduced as closely as possible: `\(…\)` and `\[…\]` math, bold, and line breaks. The notice tells what was fixed, with a button to put it back.
4. `docs/images/1-main.en.png` — Type Markdown on the left and the preview on the right updates at once. A 3D model written in STL turns when you drag it.
5. `docs/images/2-dark.en.png` — Day and night palettes follow Windows, or choose one with the Theme button in Settings. Printing and PDF always use a white page.
6. `docs/images/3-preview-window.en.png` — Put the preview on a second display to keep writing and reading apart. It scrolls with your editing.
7. `docs/images/4-syntax.en.png` — Press F1 for the Markdown quick reference. Insert puts the example at the cursor.
8. `docs/images/5-save-as.en.png` — Save As writes PDF, HTML, PNG, Word, CSV, TSV, JSON, Jupyter Notebook, text, and a ZIP for sharing.
9. `docs/images/6-code-toc.en.png` — Code with a language name is colored. Contents builds a table of contents from your headings.
10. `docs/images/8-map.en.png` — Allow it, and a map is drawn on a detailed OpenStreetMap map. Front matter at the top becomes a list of keys and values.

## Store art / ストア用の画像

The images in `store/listing/` are drawn from the official mark SVG by `store/render-assets.ps1`, on the paper ground with the clear space of Identity 3-5. Upload them under Store listing > Store logos. All are optional, but the 2:3 poster art is the one Windows shows first.
`store/listing/` の画像は `store/render-assets.ps1` が公式マークの SVG から描いたものです（紙色の地、Identity 3-5 の余白）。登録情報の「ストア ロゴ」に入れます。どれも任意ですが、Windows では 2:3 のポスター アートが最初に表示されます。

| Partner Center | File | Size |
| :--- | :--- | :--- |
| 2:3 Poster art | `poster-2x3@2x.png` (or `poster-2x3.png`) | 1440 x 2160 (720 x 1080) |
| 1:1 Box art | `boxart-1x1@2x.png` (or `boxart-1x1.png`) | 2160 x 2160 (1080 x 1080) |
| 16:9 Super hero art | `hero-16x9@2x.png` (or `hero-16x9.png`) | 3840 x 2160 (1920 x 1080) |

The app's own logos (Start, taskbar, File Explorer) are in `store/assets/` and are packed into the MSIX by `build-msix.ps1`.
アプリ自体のロゴ（スタート・タスクバー・エクスプローラー）は `store/assets/` にあり、`build-msix.ps1` が MSIX に入れます。

## 日本語 (ja-JP)

**製品名:** mdEditor by 5garashi.com

**簡単な説明:**
PDF や Word を Markdown として開いて直し、PDF や Word で渡せます。AI のチャットの Markdown も、できる範囲で再現します。PC の中だけで動き、広告もありません。

**説明:**

PDF・Word・メール・Web ページを、PC の中だけで Markdown にできます。直したら、PDF や Word で渡せます。ファイルを開くと、文字・見出し・箇条書き・画像が Markdown になり、横のプレビューで確かめながら、その場で直せます。Python もコマンドもアカウントも要りません。文書は PC の外に出ず、広告もありません。

Markdown として開く
- PDF: 文字を読む順に取り出し、折り返した行を段落にまとめます。見出し（文字の大きさと番号から推測）・箇条書き・目次・画像も戻ります。罫線のある表とコードも戻ります。図と数式は線と文字から画像に描き直し、段組みの配置・罫線のない表は戻りません（文字は残ります）。スキャンした画像だけの PDF からは、文字を取り出せません。
- Word: 見出し・箇条書き・表・リンク・画像が戻ります（色や配置は戻りません）。見出しのスタイルがない文書は、文字の大きさから見出しを推測します。
- HTML・Web ページ・メール（.eml）・CSV・TSV・JSON・XML・Jupyter Notebook・画像も開けます。元のファイルは書き換えず、プレビューの横に元のファイルを並べて見比べられます。
- 取り込んだ結果は、人に渡す前や AI のチャットに貼る前に、確かめて直せます。

AI のチャットの Markdown を、できる範囲で再現
- AI のチャットの回答を開くか貼り付けると、できる範囲で再現します。`\(…\)`・`\[…\]` で書いた数式、行の中の `$$…$$`、すぐ後に日本語が続く数式も、数式になります。すべての書き方を直せるわけではないので、結果を確かめてください。
- `**` がそのまま出てしまう太字（「」などのかっこの隣でよく起きます）や、`$` が抜けた数式は、開いたときに見つけて直します（元に戻すボタン付き）。
- AI のチャットからコピーした文に残る引用の印・見えない文字などは、初めから用意した「自動修正のルール」で取り除きます。自分でルールを足して、ほかの書き方も直せます。
- 行ごとの改行は書いたとおりに改行して表示し、見出しの段階の飛びなどの書き間違いには波線を引きます。

PDF や Word で保存する
- プレビューと同じ見た目の PDF を、ワンクリックで保存します。図・数式・表・3D モデル・地図・色分けしたコードも入ります。用紙は A4 と Letter から選べます（「自動」は地域の設定に合わせます）。ページ番号が入り、画面読み上げで見出し・箇条書き・表をたどれるタグ付きの PDF になります。
- 「名前を付けて保存」で、Word・HTML・PNG・CSV・TSV・JSON・Jupyter Notebook・テキストにも保存できます。文書と「名前_files」フォルダーをまとめた配布用 ZIP も作れます。
- ダークモードでも、印刷と PDF は白地です。

書く・見る
- 左に書くと、右のプレビューにすぐ反映されます。両側のスクロールは連動します。
- 書くそばから Markdown を色分けし、見出しの段階の飛びや行き先のないリンクなどの書き間違いに波線を引きます。
- 書く手助け: Ctrl+B・Ctrl+I で太字・斜体、Enter でリストの続き、キー 1 つで表を整える・見出しに番号を付ける、選んだ文字に URL を貼るとリンク、リンク先と数式の入力の候補。
- フローチャートやガントチャート（Mermaid）、Graphviz の図、数式（KaTeX）を、数行のテキストで描けます。AI のチャットが書く `\(…\)`・`\[…\]` の数式も表示します。
- 3D モデル（テキスト形式の STL）はマウスで回して確かめられ、地図（GeoJSON・TopoJSON）は簡単な世界地図の上に描きます。どちらもオフラインで表示します。
- 約 40 の言語のコードを色分けし、「目次」ボタンで見出しから目次を作れます。
- GitHub と同じ書き方に対応します（注意書き、フロントマター、数式のコードブロック、絵文字の短縮名、折りたたみなど）。文書の中の動画も再生できます。
- プレビューだけを別のウィンドウにして 2 台目のディスプレイに表示したり、ウィンドウいっぱいに広げて編集欄なしで読んだりできます。
- 画像は、編集欄にドロップするか貼り付けるだけで、文書の横に保存して入れます。ほかのアプリでテキスト・画像・ファイルをコピーしてから設定したキーを押すと、新しい文書として開きます。
- Markdown が初めてでも、内蔵のサンプルや F1 の書き方一覧から、ワンクリックで例を入れられます。

書いたものを守る
- 保存の途中で失敗しても、元のファイルを壊しません。
- 保存していない変更は自動でバックアップし、アプリが異常終了しても次の起動で復元できます。
- Shift_JIS・EUC-JP・UTF-8 を自動で判定するので、古い日本語のファイルも正しく開けます。

静かで、安心
- 文書の内容は、どこにも送りません。PDF や Word の変換も PC の中で行います。計測も、アカウントも、広告もありません。
- インターネット上の画像・動画と詳しい地図は、許可したときだけ読み込みます。

だれにでも使いやすく
- すべての操作をキーボードで行え、ショートカットも変えられます。
- 画面読み上げに対応し、PC の音声で文書を読み上げます（選んだ部分だけも読めます）。BIZ UD ゴシック、文字の大きさと行間の調整、コントラスト テーマ、ダークモードにも対応します。

日本語・英語・簡体字中国語に対応します。5garashi.com Design Office が作りました。会社紹介・サポート・プライバシーポリシーは、アプリの「このアプリについて」から開けます。

**このバージョンの新機能:**
- PDF を Markdown として開けるようになりました（PC の中で変換します）。文字・見出し・箇条書き・目次・表・コード・画像が戻り、図と数式は画像に描き直します。タグ付きの PDF はタグのとおりに読み、元の PDF をプレビューの横に並べて見比べられます。段組みの配置は戻りません。
- 保存した PDF にページ番号が入り、画面読み上げ用のタグも付きます。見出しのスタイルがない Word 文書は、文字の大きさから見出しを推測します。
- 書く手助け: 書くそばから Markdown を色分けします。Ctrl+B / Ctrl+I で太字・斜体、Enter と Tab でリストの続きと段、Ctrl+Shift+W で表を整え、Ctrl+Shift+N で見出しに番号を付けます。選んだ文字に URL を貼るとリンクになり、リンク先と数式の命令を候補に出します。
- 書き方の確認: 見出しの段階の飛びや行き先のないリンクなどの書き間違いに、波線を引きます。
- AI のチャットの Markdown を、できる範囲で再現します。`\(…\)`・`\[…\]` の数式を表示し、かっこの隣で `**` が出てしまう太字や `$` が抜けた数式は、開いたときに直します（元に戻すボタン付き）。コピーした文に残る引用の印なども取り除きます。
- 段落の中の改行を、書いたとおりに改行して表示します（オフにすると GitHub と同じ）。
- 簡体字中国語、「クリップボードから新規作成」に対応し、起動がおよそ 2 倍速くなりました。
- 3D モデル（STL）・地図・Graphviz の図、GitHub と同じ書き方（注意書き、フロントマター、折りたたみ、動画）、約 40 の言語のコードの色分け（PDF・Word にも残る）に対応しました。
- 「名前を付けて保存」で、配布用 ZIP を含むすべての形式を選べます。「画像の調整」で画像の大きさと位置を変えられ、画像はドロップや貼り付けで文書に入ります。
- インターネット上の画像と動画は、許可したときだけ読み込みます。読み上げで、選んだ部分だけを読めます。

**製品の機能（箇条書き）:**
- PDF や Word を Markdown として開ける。PC の中だけで変換
- PDF から見出し・箇条書き・表・コード・画像を取り出す
- ワンクリックで PDF に保存。プレビューと同じ見た目のまま
- Word・HTML・PNG・CSV・JSON・テキスト・ZIP にも保存
- 書きながら横で仕上がりを確認。スクロールも連動
- AI のチャットの Markdown も、できる範囲で再現
- プレビューを 2 台目のディスプレイに。編集中の行を表示
- 書くそばから色分けし、書き間違いに波線を引く
- 太字・斜体・リスト・表・見出しの番号をキー 1 つで
- Mermaid・Graphviz の図と数式をオフラインで表示
- 3D モデル（STL）と地図（GeoJSON）をオフラインで表示
- 約 40 の言語のコードを色分け。見出しから目次も作る
- GitHub と同じ注意書き・フロントマター・折りたたみ
- HTML・メール・CSV・JSON・Jupyter Notebook・画像も開ける
- 画像はドロップや貼り付けで、そのまま文書に
- コピーしてキーを押すと、新しい文書として開く
- 安全な保存と、異常終了からの自動復元
- Shift_JIS・EUC-JP・UTF-8 のファイルを正しく開ける
- 広告なし、計測なし、アカウント不要。ネット上の内容は許可したときだけ
- キーボード操作・画面読み上げ・文書の読み上げ・ダークモード

**検索用語（7つまで）:** Markdown, マークダウン, PDF変換, Word変換, エディタ, プレビュー, 生成AI

**スクリーンショット（1366 x 768。Store の最小の大きさ。日本語の画面）と説明文:**
1. `docs/images/10-pdf-open.ja.png` — PDF を開くと、PC の中で Markdown になります。文字・見出し・箇条書き・表・画像が戻ります。できる範囲での変換であることを、お知らせ欄に示します。
2. `docs/images/9-pdf.ja.png` — ワンクリックで、プレビューと同じ見た目の PDF に保存します。図・表・数式・色分けしたコードもそのまま入り、用紙は A4 か Letter です。
3. `docs/images/11-ai-chat.ja.png` — AI のチャットの回答を開くと、できる範囲で再現します。`\(…\)`・`\[…\]` の数式、かっこの隣の太字、行ごとの改行。直したことはお知らせ欄に出て、元に戻すこともできます。
4. `docs/images/1-main.ja.png` — 左に Markdown を書くと、右のプレビューにすぐ反映されます。STL で書いた 3D モデルも、ドラッグで回せます。
5. `docs/images/2-dark.ja.png` — 昼と夜の配色は Windows の設定に合わせて切り替わり、「設定」の「表示」ボタンでも選べます。印刷と PDF は常に白地です。
6. `docs/images/3-preview-window.ja.png` — プレビューを別のウィンドウにして 2 台目のディスプレイへ。書く画面と見る画面を分けられ、スクロールは編集と連動します。
7. `docs/images/4-syntax.ja.png` — F1 で Markdown の書き方の一覧を表示します。「挿入」を押すと、カーソルの位置に例が入ります。
8. `docs/images/5-save-as.ja.png` — 「名前を付けて保存」で、PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook・テキスト・配布用 ZIP を選べます。
9. `docs/images/6-code-toc.ja.png` — 言語名を書いたコードを色分けして表示します。「目次」ボタンで、見出しから目次を作れます。
10. `docs/images/8-map.ja.png` — 許可すると、地図を OpenStreetMap の詳しい地図の上に描きます。先頭の YAML は、項目と値の一覧になります。

## 简体中文 (zh-CN)

**产品名称：** mdEditor by 5garashi.com

**简短说明：**
把 PDF 和 Word 作为 Markdown 打开并修改，再保存为 PDF 或 Word。AI 聊天输出的 Markdown 也会尽量还原。只在电脑中运行，没有广告。

**说明：**

在 Windows 上离线把 PDF、Word、电子邮件和网页变成整洁的 Markdown，改好后再保存为 PDF 或 Word。打开文件后，文字、标题、列表和图片会变成 Markdown，可以一边看旁边的预览一边当场修改。不需要 Python、命令行或账户。文档留在你的电脑中，没有广告。

作为 Markdown 打开
- PDF：按阅读顺序提取文字，把折行合并为段落，标题（根据文字大小和编号推测）、列表、目录和图片也会提取。有边框的表格和代码也会提取。图表和公式会根据线条和文字重新绘制为图片，分栏排版和没有边框的表格不会恢复（文字会保留）。扫描得到的纯图像 PDF 无法提取文字。
- Word：标题、列表、表格、链接和图片会提取（颜色和排版不会保留）。没有使用标题样式的文档，会根据文字大小推测标题。
- 也可以打开 HTML、网页、电子邮件（.eml）、CSV、TSV、JSON、XML、Jupyter Notebook 和图像。原文件不会被改写，还可以把原文件显示在预览旁边进行对照。
- 在分享给别人或粘贴到 AI 聊天之前，可以先检查并修改提取的结果。

尽量还原 AI 聊天的 Markdown
- 打开或粘贴 AI 聊天的回答，会尽量还原显示。`\(…\)`、`\[…\]` 写法的公式、行内的 `$$…$$`，以及后面紧跟中文或日文的公式，都会显示为公式。并非所有写法都能修正，请检查结果。
- `**` 原样显示出来的粗体（常见于中文或日文括号旁边）和丢失了 `$` 的公式，会在打开文档时找出并修正（带撤销按钮）。
- 从 AI 聊天复制的文字中残留的引用标记、不可见字符等，会由预先准备的“自动修正规则”去除。也可以自己添加规则，修正其他写法。
- 每一行的换行按原样显示，标题层级跳跃等写法错误会加上波浪线。

保存为 PDF 和 Word
- 一键保存与预览外观完全相同的 PDF，包括图表、公式、表格、3D 模型、地图和着色的代码。纸张可选 A4 或 Letter（“自动”跟随地区设置）。页面带有页码，PDF 带有标签，屏幕阅读器可以按标题、列表和表格浏览。
- “另存为”还可以保存为 Word、HTML、PNG、CSV、TSV、JSON、Jupyter Notebook 和纯文本，以及包含文档和“名称_files”文件夹的用于分发的 ZIP。
- 即使在深色模式下，打印和 PDF 也始终使用白色页面。

写作和查看
- 在左边输入，右边的预览立即更新。两边同步滚动。
- 输入时 Markdown 会着色显示，标题层级跳跃、没有目标的链接等写法错误会加上波浪线。
- 写作辅助：Ctrl+B、Ctrl+I 设置粗体和斜体，按 Enter 继续列表，一个键整理表格、给标题编号，在选中的文字上粘贴 URL 即成为链接，链接目标和公式命令会给出输入候选。
- 用几行文字就能画出流程图和甘特图（Mermaid）、Graphviz 图表和公式（KaTeX）。AI 聊天中 `\(…\)`、`\[…\]` 写法的公式也能显示。
- 用鼠标旋转 3D 模型（文本格式的 STL），在简单的世界地图上绘制地图（GeoJSON、TopoJSON），全部离线完成。
- 约 40 种语言的代码会着色显示，“目录”可以根据标题生成目录。
- 支持与 GitHub 相同的写法：提示框、前置元数据、公式代码块、表情符号短名称、折叠等。文档中的视频也可以播放。
- 可以把预览放到单独的窗口中显示在第二个显示器上，也可以把预览扩展到填满窗口，不显示编辑区即可阅读。
- 把图片拖放或粘贴到编辑区，图片会保存在文档旁边。在任意应用中复制文本、图片或文件后，按你设置的按键，即可作为新文档打开。
- 刚接触 Markdown？打开内置的示例，或按 F1 一键插入示例。

保护你的工作
- 即使写入中途出错，保存也不会损坏原文件。
- 未保存的更改会自动备份，异常退出后可以恢复。
- 能识别 UTF-8、GBK（简体中文 Windows）、Shift_JIS 和 EUC-JP，旧文件也能正确打开。

安静、安心
- 文档内容不会发送到任何地方。PDF 和 Word 的转换也在你的电脑中完成。没有跟踪，没有账户，没有广告。
- 网络上的图像、视频和详细地图，只有在你允许时才会加载。

人人都好用
- 所有操作都可以用键盘完成，快捷键可以更改。
- 支持屏幕阅读器，并用电脑中的语音朗读文档（也可以只朗读选中的部分）。支持 BIZ UD Gothic 字体、文字大小和行距的调整、对比度主题和深色模式。

支持日语、英语和简体中文。由 5garashi.com Design Office 制作。公司介绍、支持和隐私政策的链接，可以在应用的“关于此应用”中打开。

**此版本的新功能：**
- 可以把 PDF 作为 Markdown 打开（在你的电脑中转换）。文字、标题、列表、目录、表格、代码和图片会提取，图表和公式重新绘制为图片。带标签的 PDF 按标签读取，原 PDF 可以显示在预览旁边对照。分栏排版不会恢复。
- 保存的 PDF 带有页码和供屏幕阅读器使用的标签。没有标题样式的 Word 文档，会根据文字大小推测标题。
- 写作辅助：输入时 Markdown 会着色显示。Ctrl+B / Ctrl+I 设置粗体和斜体，Enter 和 Tab 继续列表并调整层级，Ctrl+Shift+W 整理表格，Ctrl+Shift+N 给标题编号。在选中的文字上粘贴 URL 即成为链接，链接目标和公式命令会给出候选。
- 写法检查：标题层级跳跃、没有目标的链接等错误会加上波浪线。
- 尽量还原 AI 聊天的 Markdown：`\(…\)`、`\[…\]` 写法的公式会显示为公式，括号旁 `**` 原样显示的粗体和丢失了 `$` 的公式会在打开时修正（带撤销按钮），复制文字中残留的引用标记等也会去除。
- 段落中的换行按原样换行显示（关闭后与 GitHub 相同）。
- 支持简体中文和“从剪贴板新建”，启动速度提高约一倍。
- 支持 3D 模型（STL）、地图、Graphviz 图表、与 GitHub 相同的写法（提示框、前置元数据、折叠、视频），约 40 种语言的代码着色（PDF 和 Word 中也保留）。
- “另存为”可以选择包括用于分发的 ZIP 在内的所有格式。“调整图像”可以更改图像的大小和位置，图片可以通过拖放或粘贴放入文档。
- 网络上的图像和视频只有在你允许时才会加载。朗读可以只朗读选中的部分。

**产品功能（要点）:**
- 把 PDF 和 Word 作为 Markdown 打开，在电脑中转换
- 从 PDF 提取标题、列表、表格、代码和图片
- 一键保存为 PDF，外观与预览完全相同
- 也可保存为 Word、HTML、PNG、CSV、JSON、文本或 ZIP
- 边写边在旁边查看效果，滚动同步
- 尽量还原 AI 聊天的 Markdown
- 预览可放到第二个显示器上，跟随正在编辑的行
- 输入时着色显示，写法错误加上波浪线
- 一个键设置粗体、斜体、列表、表格和标题编号
- 离线显示 Mermaid、Graphviz 图表和公式
- 离线显示 3D 模型（STL）和地图（GeoJSON）
- 约 40 种语言的代码着色，并可根据标题生成目录
- 与 GitHub 相同的提示框、前置元数据和折叠
- 也可打开 HTML、电子邮件、CSV、JSON、Jupyter Notebook 和图像
- 图片拖放或粘贴即可放入文档
- 复制后按一个键，即可作为新文档打开
- 安全保存，异常退出后自动恢复
- 正确打开 UTF-8、GBK、Shift_JIS 和 EUC-JP 文件
- 没有广告、没有跟踪、不需要账户；网络内容只在允许时加载
- 支持键盘操作、屏幕阅读器、朗读和深色模式

**搜索词（最多 7 个）:** Markdown, Markdown编辑器, PDF转Markdown, Word转Markdown, Markdown转PDF, AI Markdown, Mermaid

**屏幕截图（1366 x 768，Store 的最小尺寸；简体中文界面）及说明：**
1. `docs/images/10-pdf-open.zh-CN.png` — 打开 PDF，就会在电脑中变成 Markdown。文字、标题、列表、表格和图片会提取，通知栏会说明转换会尽量保留原 PDF 的内容，但无法完整还原。
2. `docs/images/9-pdf.zh-CN.png` — 一键保存与预览外观相同的 PDF。图表、表格、公式和着色的代码都会原样放入，纸张为 A4 或 Letter。
3. `docs/images/11-ai-chat.zh-CN.png` — 打开 AI 聊天的回答，会尽量还原显示：`\(…\)`、`\[…\]` 公式、括号旁的粗体和每行的换行。修正的内容会显示在通知栏中，也可以撤销。
4. `docs/images/1-main.zh-CN.png` — 在左边写 Markdown，右边的预览立即更新。用 STL 写的 3D 模型也可以拖动旋转。
5. `docs/images/2-dark.zh-CN.png` — 白天和夜间的配色跟随 Windows 的设置，也可以在“设置”中用“显示”按钮选择。打印和 PDF 始终使用白色页面。
6. `docs/images/3-preview-window.zh-CN.png` — 把预览放到第二个显示器上，写作和阅读分开进行。预览会跟随编辑滚动。
7. `docs/images/4-syntax.zh-CN.png` — 按 F1 显示 Markdown 简明写法。按“插入”会把示例放到光标位置。
8. `docs/images/5-save-as.zh-CN.png` — “另存为”可以保存为 PDF、HTML、PNG、Word、CSV、TSV、JSON、Jupyter Notebook、文本和用于分发的 ZIP。
9. `docs/images/6-code-toc.zh-CN.png` — 写了语言名的代码会着色显示。“目录”会根据标题生成目录。
10. `docs/images/8-map.zh-CN.png` — 允许后，地图会绘制在 OpenStreetMap 的详细地图上。开头的 YAML 显示为项目名和值的列表。

## Before you submit / 提出前の確認

- Reserve the name in Partner Center and confirm it is available. Use the same spelling ("5garashi.com Design Office") in the Store, the app, and the website.
- Replace the privacy or support links if the final URLs differ. The company URL in the app is `https://www.5garashi.com/`.
- "Worldwide" distribution and "all languages" are different settings. The app supports Japanese, English, and Simplified Chinese (1.0.15) only. Since 1.0.15 the package declares ja-JP, en-US, and zh-CN, so Partner Center expects a listing in each of the three languages; the zh-CN text and its screenshots are above. The Chinese text has not been reviewed by a native speaker.
- The screenshots are in `docs/images/` (1366 x 768 window, 90% zoom), in Japanese (`.ja`), English (`.en`), and Simplified Chinese (`.zh-CN`). All were retaken with 1.0.18 on 2026-10-31 (the editor colors Markdown, F1 shows the keys) with the scratch kit take-shots.ps1, make_pdfs.py, take-import18.ps1, compose_pdf.py, and compose_import.py; 1-main shows the 3D model of samples/diagram-test (21-1). `10-pdf-open` (listed first to lead with PDF import) puts page 1 of a short report PDF beside the window that opened it as Markdown; the report was written for the screenshot and saved as PDF by mdEditor, with numbered lists. `9-pdf` (listed second) puts the window beside the first page of the PDF it saved, with its page number. `11-ai-chat` (listed third) opens an AI-chat-style answer written for the screenshot (extra\7-ai-chat.* in the kit); the default fix rules fix its bold and offer to rewrite its `\(…\)` math, so the notice shows both. `7-ai-chat` is saved as `11-ai-chat`, and `7-3d-model` is no longer listed (1-main shows the same model) but stays in the READMEs. The window shots are 1352 x 761 (the visible frame of a 1366 x 768 window) and `5-save-as` is a crop, so upload the copies in `artifacts/store-shots/` instead: each sits on the 1366 x 768 ground of 3, 9, and 10 (5-save-as at 1.5 times). Upload the screenshots in the listed order; ten is the Store maximum. Retake them when the screen changes.
- The map screenshot (`8-map`, listed tenth) shows OpenStreetMap map tiles. Keep the credit "© OpenStreetMap contributors" visible; do not crop it out.
  地図のスクリーンショット（`8-map`、一覧の 10 番目）には OpenStreetMap の地図が写っています。右下の「© OpenStreetMap contributors」を切り取らないでください。
- Partner Center limits: What's new 1,500 characters, each feature 200 characters (up to 20), each screenshot caption 200 characters (up to 10 screenshots).
