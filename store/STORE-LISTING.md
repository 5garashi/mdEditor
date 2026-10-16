<!--
  File: STORE-LISTING.md
  Description: Microsoft Store listing text (English and Japanese) and screenshot plan for mdEditor
  Author: 5garashi.com Design Office
  Created: 2026-10-11
  License: MIT
  SPDX-License-Identifier: MIT
-->
# Microsoft Store listing / ストア掲載文

Paste each section into the matching Partner Center field. Check the character limits there before submitting.

## English (en-US)

**Product name:** mdEditor by 5garashi.com

**Short description (one line):**
Write plain text. Get diagrams, math, and a polished PDF. Private, offline, ad-free.

**Description:**

Turn plain text into clear documents, with the finished result right beside you as you type.

mdEditor is a Markdown editor for Windows that shows your writing as a finished page in real time. Flowcharts, formulas, tables, and checklists appear as you write them, and one click saves the whole thing as a PDF.

**Write, see, share**
- Live preview on the right, your text on the left. Both scroll together.
- Two displays? Open the preview in its own window and put it on the second screen, maximized or full screen. It still scrolls with the editor, line by line, and Save, Find, and every other command work from either window.
- Also a Markdown viewer. Set mdEditor as the app for .md files, or drop a file onto it, and expand the preview to fill the window for comfortable reading, with no editing in the way. Links to headings and to other Markdown files in the same folder work too.
- Draw flowcharts, sequence diagrams, Gantt charts, and pie charts with a few lines of text (Mermaid). Write formulas the way you say them (KaTeX).
- Save a PDF that looks exactly like the preview, diagrams and math included. A4 or Letter.
- Open Word, HTML, web pages, e-mail (.eml), CSV, JSON, XML, Jupyter Notebooks, and images as Markdown, and save to PDF, HTML, PNG, Word, CSV, JSON, Jupyter Notebook, or plain text. The original file is never changed.
- Drop or paste pictures into the editor; they are saved beside your document.
- New to Markdown? Open a built-in sample, or press F1 for a quick syntax guide and insert examples with one click.

**Your work stays safe**
- Saving never damages your original file, even if something goes wrong mid-write.
- Unsaved changes are backed up automatically, so a crash does not cost you your work.
- Older Japanese files open correctly. Shift_JIS, EUC-JP, and UTF-8 are detected, and you choose how to save.

**Private and quiet**
- Your documents never leave your PC. No tracking, no accounts, no ads.
- Diagrams and math work offline.
- Internet images in a document load only when you allow them.

**Made for everyone**
- Everything works from the keyboard, with custom shortcuts.
- Screen reader support, read-aloud with the voices already on your PC, a readable BIZ UD typeface, adjustable text size and spacing, high contrast, and dark mode.

Available in Japanese and English. Made by 5garashi.com Design Office. Open About in the app to find the company, support, and privacy policy links.

**What's new in this version:**
Colored code: code blocks with a language name, such as Python, JavaScript, C#, PowerShell, or SQL, are colored in the preview and in saved PDF, PNG, HTML, and Word files. Contents inserts a table of contents built from your headings and updates it with one press; its links work in PDF and Word too. Graphviz (DOT) diagrams are drawn offline. Open more formats: Word, HTML and web pages, e-mail, CSV and TSV, JSON, XML, Jupyter Notebooks, and images, all as Markdown. Save As now lists every format, including Word, PNG, CSV, JSON, Jupyter Notebook, and text without Markdown marks. Drop or paste pictures into the editor. Internet images load only after you allow them. Choose the read-aloud language and voice. mdEditor now appears in Open with for these file types.

**Features (bullets):**
- Live side-by-side preview that scrolls with you
- Preview on a second display, still in sync with the editor
- Works as a Markdown viewer too: full-window preview for reading
- Diagrams and math with no internet connection
- PDF that looks exactly like the preview
- Safe saving and automatic crash recovery
- Opens Shift_JIS, EUC-JP, and UTF-8 files
- Keyboard-first, screen reader friendly, dark mode
- No ads, no tracking, no account
- Open Word, HTML, web pages, e-mail, CSV, JSON, Jupyter Notebooks, and images
- Save as Word, PNG, HTML, CSV, JSON, Jupyter Notebook, or plain text
- Drop or paste pictures straight into your document
- Internet images load only when you allow them
- Colored code for about 40 languages, kept in PDF and Word
- Table of contents from your headings, updated with one press
- Graphviz (DOT) diagrams, drawn offline

**Search terms (up to 7):** markdown editor, preview, mermaid, diagram, PDF, second display, jupyter notebook

**Privacy policy URL:** https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md

**Support contact:** https://github.com/5garashi/mdEditor/issues

**Screenshots (1366 x 768, the Store minimum; English UI) with captions:**
1. `docs/images/1-main.en.png` — Write Markdown on the left and the preview on the right updates as you type, with diagrams, tables, and math.
2. `docs/images/2-dark.en.png` — Follows the Windows setting, or choose day or night with the Theme button. Printing and PDF always use a white page.
3. `docs/images/3-preview-window.en.png` — Put the preview on a second display to keep writing and reading apart. It scrolls along with your editing.
4. `docs/images/4-syntax.en.png` — The Markdown quick reference (F1). Insert puts the example at the cursor.
5. `docs/images/5-save-as.en.png` — Save As writes Markdown, text, PDF, HTML, PNG, Word, CSV, TSV, JSON, or Jupyter Notebook. Word, HTML, CSV, e-mail, Jupyter Notebook, and more open too.

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
文字を書くだけで、図・数式・きれいな PDF に。オフラインで使え、広告もありません。

**説明:**

書いた文章が、そのまま読みやすい文書になります。仕上がりを横に見ながら書ける Markdown エディタです。

mdEditor は、左で書くと右にすぐ仕上がりが表示される Windows アプリです。フローチャートも数式も表もチェックリストも、書いた瞬間に見えます。ワンクリックで PDF にもできます。

**書く・見る・渡す**
- 左が文章、右が仕上がり。スクロールが連動します。
- ディスプレイが 2 台なら、プレビューだけを別のウィンドウにして 2 台目に表示できます。最大化しても全画面にしても、編集欄と同じ行を表示し続けます。保存や検索などの操作は、どちらのウィンドウからでも使えます。
- ビューアとしても使えます。`.md` ファイルの既定アプリにするか、ファイルをドロップして開き、プレビューをウィンドウいっぱいに広げれば、編集欄なしで読めます。見出しへのリンクや、同じフォルダーのほかの Markdown ファイルへのリンクも使えます。
- 数行のテキストで、フローチャート・シーケンス図・ガントチャート・円グラフを描けます（Mermaid）。数式も書いたまま表示します（KaTeX）。
- プレビューそのままの見た目で PDF に保存。図も数式も含みます。用紙は A4 と Letter。
- Word・HTML・Web ページ・メール（.eml）・CSV・JSON・XML・Jupyter Notebook・画像を Markdown として開き、PDF・HTML・PNG・Word・CSV・JSON・Jupyter Notebook・テキストに保存できます。元のファイルは書き換えません。
- 画像は編集画面にドロップするか貼り付けるだけで、文書の横に保存して入れます。
- Markdown が初めてでも大丈夫。内蔵のサンプルを開くか、F1 の書き方一覧からワンクリックで例を入れられます。

**書いたものを守る**
- 保存の途中で失敗しても、元のファイルを壊しません。
- 保存していない変更は自動でバックアップ。アプリが異常終了しても、次の起動で復元できます。
- 古い日本語のファイルも正しく開けます。Shift_JIS・EUC-JP・UTF-8 を判定し、保存する文字コードも選べます。

**静かで、安心**
- 文書は PC の外に出ません。計測も、アカウントも、広告もありません。
- 図と数式はオフラインで表示できます。
- 文書の中のインターネット上の画像は、許可したときだけ読み込みます。

**だれにでも使いやすく**
- 全操作をキーボードで行え、ショートカットも変更できます。
- 画面読み上げ対応、PC 内の音声による読み上げ、BIZ UD ゴシック、文字の大きさ・行間の調整、コントラスト対応、ダークモード。

日本語と英語に対応します。5garashi.com Design Office が作りました。アプリの「このアプリについて」から、会社紹介・サポート・プライバシーポリシーを開けます。

**このバージョンの新機能:**
コードの色分け：Python・JavaScript・C#・PowerShell・SQL など、言語名を書いたコードブロックを色分けして表示し、PDF・PNG・HTML・Word に保存しても色が残ります。「目次」ボタンで、見出しから目次を作って入れ、押し直すと新しくなります（目次のリンクは PDF・Word でも使えます）。Graphviz（DOT）の図をオフラインで描けるようになりました。Word・HTML・Web ページ・メール・CSV・TSV・JSON・XML・Jupyter Notebook・画像を、Markdown として開けるようになりました。「名前を付けて保存」で、Word・PNG・CSV・JSON・Jupyter Notebook・記号なしのテキストなど、すべての形式を選べます。画像は、編集画面にドロップするか貼り付けるだけで文書に入ります。インターネット上の画像は、許可したときだけ読み込みます。読み上げの言語と音声を選べるようになりました。これらの形式で、「プログラムから開く」に mdEditor が出ます。

**製品の機能（箇条書き）:**
- 書きながら横で仕上がりを確認。スクロールも連動
- 2 台目のディスプレイにプレビューを表示。編集と連動
- ビューアとしても。プレビューをウィンドウいっぱいに表示
- 図と数式をオフラインで表示
- プレビューそのままの見た目で PDF に保存
- 安全な保存と、異常終了からの自動復元
- Shift_JIS・EUC-JP・UTF-8 のファイルを開ける
- キーボードで全操作、画面読み上げ対応、ダークモード
- 広告なし、計測なし、アカウント不要
- Word・HTML・Web ページ・メール・CSV・JSON・Jupyter Notebook・画像を開ける
- Word・PNG・HTML・CSV・JSON・Jupyter Notebook・テキストに保存
- 画像はドロップや貼り付けで、そのまま文書に
- ネット上の画像は、許可したときだけ読み込む
- 約 40 の言語のコードを色分け。PDF・Word にも色が残る
- 見出しから目次を作り、ボタン 1 つで新しくする
- Graphviz（DOT）の図をオフラインで表示

**検索用語（7つまで）:** Markdown, マークダウン, エディタ, プレビュー, PDF, 数式, デュアルモニター

**スクリーンショット（1366 x 768。Store の最小の大きさ。日本語の画面）と説明文:**
1. `docs/images/1-main.ja.png` — 左で Markdown を書くと、右のプレビューにすぐ反映されます。図・表・数式も、そのまま表示します。
2. `docs/images/2-dark.ja.png` — Windows の設定に合わせて、昼と夜の配色を自動で切り替えます。「表示」ボタンで選ぶこともできます。印刷と PDF は常に白地です。
3. `docs/images/3-preview-window.ja.png` — 2 台目のディスプレイに置いて、書く画面と見る画面を分けられます。スクロールは編集と連動します。
4. `docs/images/4-syntax.ja.png` — Markdown の書き方の一覧（F1）。「挿入」を押すと、カーソルの位置に例が入ります。
5. `docs/images/5-save-as.ja.png` — 「名前を付けて保存」で、Markdown・テキスト・PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook を選べます。Word・HTML・CSV・メール・Jupyter Notebook などのファイルも開けます。

## Before you submit / 提出前の確認

- Reserve the name in Partner Center and confirm it is available. Use the same spelling ("5garashi.com Design Office") in the Store, the app, and the website.
- Replace the privacy or support links if the final URLs differ. The company URL in the app is `https://www.5garashi.com/`.
- "Worldwide" distribution and "all languages" are different settings. The app supports Japanese and English only.
- The screenshots are in `docs/images/` (taken with 1.0.8 on a 1366 x 768 display). Retake them when the screen changes.
