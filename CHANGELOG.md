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

## 1.0.18 — 2026-10-25

- タグ付きの PDF（Word・Acrobat・LibreOffice・ブラウザーなどで作ったもの）を開いたときに、PDF の中のタグ（見出し・段落・箇条書き・表と、その読む順）を使うようにした。ページの文字のほとんどがタグに入っていれば、読む順・段落の区切り・見出しの段階（H1〜H6）・箇条書きをタグのとおりにし、罫線のない表も Markdown の表に戻し、柱やページ番号として印の付いた文字は省く。次のページに続いた箇条書きや段落の続きも、正しい場所に読む。タグのない PDF や、タグが文字を覆っていないページは、これまでどおり文字の位置から読む。図・数式・罫線のある表・コードは、タグがあってもこれまでどおり。
- PDF で、文字の箱に高さがない書体（ブラウザーがフォントなしで印刷した Type 3 の書体）や絵文字の行が、2 行に分かれて順番が入れ替わることがあったのを直した。
- PDF で、文字をはさまずに並んだ図が、ページの順（上から、左から）でなく見つけた順に出ていた（069 の「主な摂取量」の折れ線グラフが、後の「おやつ」のグラフの下に出ていた）のを直した。横に並んだグラフは、間にラベルしかなければ離れていても 1 枚の画像にする（これまでは 0.5 インチまで）。
- ファイルをドラッグしているとき、編集画面の上で表示する「カーソルの位置に入れる」と編集画面の枠を赤（ボルト）にして、「ファイルとして開く」と見分けやすくした。
- PDF に書き出すと、タグ付きの PDF になるようにした。見出し・段落・箇条書き・表の区別が PDF に残り、読み上げソフトがたどれるほか、mdEditor で開き直すとタグのとおりに戻る（数式・図・コードの読み方は変わらない）。タグ付きで書き出せないときは、これまでどおりの PDF にする。

- Opening a tagged PDF (one made by Word, Acrobat, LibreOffice, or a browser, for example) now uses the tags in it: what is a heading, a paragraph, a list item, or a table, and the order they are read in. Where the tags hold nearly all of a page's text, they decide the reading order, where paragraphs end, the heading levels (H1 to H6), and the list items; a table without cell borders comes back as a Markdown table, and running titles and page numbers marked as such are left out. A list item or paragraph that goes on to the next page is read in its place. A PDF without tags, and a page whose tags do not hold its text, is read from where the letters stand as before. Figures, math, ruled tables, and code are found as before, tags or not.
- PDF: a line in a typeface whose letters have boxes of no height (a Type 3 font, as a browser printing without the fonts writes them), or a line with emoji, could be read as two lines in the wrong order; it is now read as one.
- PDF: figures with no text between them came out in the order they were found, not the page's order (top to bottom, then left to right); in 069, the line charts under "主な摂取量" landed below the later charts. Charts side by side become one picture however far apart they are when only their labels stand between them (half an inch at most before).
- While files are dragged over the editor, the "Insert at the cursor" label and the editor's frame turn red (volt), so it is not taken for "Open as a file".
- Export to PDF now writes a tagged PDF: what is a heading, a paragraph, a list, or a table stays in the PDF, so screen readers can follow it, and opening it again in mdEditor reads it by its tags (math, figures, and code are read as before). When a tagged PDF cannot be written, the PDF is written as before.

## 1.0.17 — 2026-10-25

- PDF を開いたときに、罫線のある表を Markdown の表に、等幅の書体（Consolas など）の行をコードのブロック（字下げと空行を保つ。日本語のコメントの行も含む）に、図形で描いた箇条書きの印とチェックボックス（ブラウザーや mdEditor で保存した PDF など）を箇条書き（入れ子と `[x]` を含む）にするようにした。線や図形で描いた図（Mermaid・Graphviz のダイアグラムやグラフなど）は、線・図形・色・図の中の文字から SVG の画像に描き直して「名前_files」に保存する（直線に沿ったグラデーションはそのまま、それ以外の模様は灰色になる）。罫線の上下だけがページに入った表の続きも、表として読む。KaTeX や LaTeX（Computer Modern）で組んだ数式も画像に描き直す（1 行に単独の数式は中央寄せ、文中の数式はその位置。数式の文字を画像の説明に入れる）。これまで数式が別の場所（後の見出しの下など）に出たり、分数の中の日本語が見出しになったりしていたのも直った。画像は、元の大きさと違う大きさ（`width`）・中央・右寄せ・文章の回り込み（`align` と `<br clear="all">`）を戻す。読み込んでいなかったインターネット上の動画の注記は、アドレスだけの行に戻す。和文の中に等幅の語（`align="left"` など）がある行を、コードとして読まないようにした（コードの背景の中の行は、和文が多くてもコードのまま。diff の `- 保存` など）。文字を制御文字（NUL）に対応づけた PDF で、プレビューに � が出たり、空の見出しができたりしたのを直した。ページ番号がページの下にある PDF で、そのページの図と表が本文より前に出ていたのを直した。グラフの軸の数字・凡例・横のラベルのように図のまわりにある短い文字は、本文に置くと順番が崩れて意味が通じなくなるため、図と一緒に画像に描き直す（見出しと長い文は本文に残す）。並んだグラフは 1 枚の画像になる。図の半透明の塗り（Mermaid のガントチャートのセクションの色など）は半透明のまま描き、透明な図形は描かない。図の線の太さを、ページでの実際の太さで描く（これまでは推定で、太い線は図の高さの 4 分の 1 までに抑えていたため、サンキー図の帯が細くなっていた）。文中のキー（`<kbd>`）・上付き（`<sup>`）・下付き（`<sub>`）・マーカー（`<mark>`）を戻す（これまで上付きの文字は別の行に出ていた）。段組みの配置と罫線のない表は、これまでどおり文字だけが残る。お知らせ欄の説明も合わせた。
- PDF で、広い間隔をあけて同じ行に書いた項目（「File Number: …   Type dsg: …」など）が、下の行より後に読まれることがあったのを直した。本文と離れた「・」が段落の途中の行にあっても、その行から箇条書きにする。ページの上にある見出しや枠の文を、ページが後から描いていると本文の後に読んでいた（章の扉の「第4章」、条文の枠など）のを直した。目次のページ番号が離れた列になっている PDF（LaTeX など）で、番号が目次の後にまとめて出ていたのを直し、ページの端から少し離れたページ番号（LaTeX の 1 インチほどの余白）も取り除く。LaTeX の数式の根号（√）が上にずれて分子と重なっていたのを直した。
- サンプルの pdf-test.pdf・pdf-test.en.pdf にコード・箇条書き・やること・図・数式・画像の大きさと位置・動画を足し、pdf-toc-test.en.pdf に間隔の広い行を足して、samples/import-test の確かめ方を直した。

- Opening a PDF now brings back tables with ruled cells as Markdown tables, lines in a monospaced typeface (Consolas and so on) as code blocks (keeping indentation and empty lines, including lines with Japanese comments), and bullets and check boxes drawn as shapes (as in PDFs saved from a browser or mdEditor) as list items, nested and with `[x]`. A figure drawn with lines and shapes (a Mermaid or Graphviz diagram or chart, for example) is redrawn from its lines, shapes, colors, and labels as an SVG picture in "name_files" (a gradient along a line is kept; other patterns come back as gray). The rest of a table that goes on from the page before, with only its top and bottom borders on the page, is read as a table too. Math set by KaTeX or by LaTeX (Computer Modern) is redrawn as pictures too (a formula on a line of its own centered, one in a sentence in its place, with its letters as the description); math no longer lands elsewhere (under a later heading), and Japanese in a fraction no longer becomes headings. Pictures keep a size other than their own (`width`), centering, right alignment, and text flowing around them (`align` and `<br clear="all">`). The note for an internet video that was not loaded comes back as the address on a line of its own. A line of Japanese prose with monospaced words in it (`align="left"` and so on) is no longer read as code (a line on a code background stays code even when it is mostly Japanese, as in a diff's `- 保存`). In a PDF that maps letters to control characters (NUL), the preview no longer shows � and no empty headings appear. In a PDF with page numbers at the foot of the page, the page's figures and tables no longer come before its text. Short text around a figure (a chart's axis numbers, legend, and labels beside it), which would lose its meaning out of place in the running text, is redrawn in the figure's picture (headings and longer text stay text), and charts set side by side become one picture. Semi-transparent fills in figures (the section colors of a Mermaid Gantt chart, for example) stay semi-transparent, and fully transparent shapes are not drawn. Lines in figures are drawn as thick as they are on the page (they were guessed before and kept to a quarter of the figure's height, which made the bands of a Sankey diagram too thin). Keys (`<kbd>`), superscripts (`<sup>`), subscripts (`<sub>`), and highlights (`<mark>`) in a line come back (a superscript used to land on a line of its own). Column layout and tables without cell borders still come back as their text only. The notice bar says so.
- PDF: items on one line with a wide gap between them ("File Number: …   Type dsg: …", for example) could be read after the line below; they are now read in order. A bullet set apart from its text starts a list item even on a line in the middle of a paragraph. A heading or framed text at the top of a page that the page draws later (a chapter's title page, a framed law excerpt) is no longer read after the text below it. In a PDF whose contents page has its page numbers in a column of their own (LaTeX, for example), the numbers no longer come all together after the contents, and a page number set a little in from the edge of the page (LaTeX's inch or so) is removed too. The radical sign (√) in LaTeX math no longer sits too high over the numerator.
- The samples pdf-test.pdf and pdf-test.en.pdf gain code, bullets, a to-do list, a figure, math, image size and position, and a video, and pdf-toc-test.en.pdf a line with a wide gap; samples/import-test is updated.

## 1.0.16 — 2026-10-25

- PDF（.pdf）を開けるようにした（「開く」と、編集画面の外へのドロップ）。PdfPig（Apache 2.0、PC の中だけで動く）で文字を読む順に取り出し、折り返した行を 1 つの段落にまとめ、見出しを文字の大きさから推し量り、「・」や番号で始まる行を箇条書きにする。画像は Word 文書と同じく「名前_files」フォルダーに取り出す。表・数式・図・段組みの配置は戻らない（文字は残る）。ページの上下の端のページ番号は省く。ブラウザーで作った PDF に多い、漢字が部首の文字（「⽅」など）になる問題を直して取り出す。画像だけの PDF とパスワード付きの PDF はお知らせする。開くのは 500 ページまで。編集画面にドロップした PDF は、これまでどおり「名前_files」に写してリンクを入れる。
- 見出しのスタイルを使わず、文字を大きくしただけで見出しを作った Word 文書は、文字の大きさから見出しを推し量るようにした（本文よりはっきり大きい文字、または一回り大きい太字を、大きさ・太さの順に見出し 1〜3 にする）。見出しのスタイルを使った文書は、これまでどおり。いくつ見出しにしたかはお知らせ欄に出す。
- 見出しに番号（1.・3.1・(1)・（ア）・① など）を付け、本文と別の書体（明朝の本文にゴシックの見出しなど）で書いた PDF は、番号の付いた短い行を見出しにし、大きさと番号の深さで見出しの深さを決める（表紙の題などの番号のない大きな文字は見出し 1、番号付きの見出しは見出し 2 から）。目次の行（点線とページ番号で終わる行）は、PDF の字下げのとおりに入れ子にした 1 つの箇条書きにし、点線を「……」に縮め、見出しにしない（長くて折り返した項目は 1 つにつなぐ。「1.」は番号付きリストと読まれないよう「1\.」と書く）。行は、文字の並ぶ高さから自分で組み立てるようにした（日本語で見出しと本文が 1 行につながることがあったため）。本文と離れた「・」は、同じ高さの段落に戻して箇条書きにする。
- 読み上げで、プレビューか編集欄で文字を選んでいるときは、選んだ部分だけを読むようにした（編集欄では見出し・太字・リンクなどの Markdown の記号を除いて読む。編集欄の選択は、編集欄にフォーカスがあるとき、またはボタンを押す直前にあったときだけ使う）。長い選択は文ごとに分けて読む。選んでいないときは、これまでどおり見えている段落から読む。
- 「クリップボードから新規作成」（`--paste`）で、エクスプローラーでコピーしたファイルが 1 つで、mdEditor で開ける形式なら、そのファイルを開くようにした（編集画面の外にドロップしたときと同じ）。2 つ以上のファイルや開けない形式は、これまでどおり新しい文書に入れる。
- サンプルに pdf-test.pdf・pdf-test.en.pdf・pdf-toc-test.en.pdf と word-headings-test.docx・word-headings-test.en.docx を足し、samples/import-test に確かめ方を書いた。
- THIRD-PARTY-NOTICES.md に PdfPig と、その中の Adobe Glyph List のライセンスを足した。

- PDFs (.pdf) open as Markdown (Open, or a drop outside the editor). PdfPig (Apache 2.0, runs only on your PC) reads the text in reading order; wrapped lines join into paragraphs, headings are guessed from text size, and lines starting with a bullet or number become list items. Pictures go in the "name_files" folder as with Word documents. Tables, math, drawings, and column layout do not come back (their text does). Page numbers at the top or bottom of a page are left out. Kanji that PDFs made by browsers often store as radicals (such as ⽅) are put back. PDFs made only of images and password-protected PDFs are reported. Up to 500 pages. A PDF dropped on the editor still goes into "name_files" as a link.
- A Word document whose headings are made only with larger text (no heading styles) gets its headings from text size: clearly larger text, or bold text one step larger, becomes heading 1 to 3 by size and then boldness. Documents with heading styles open as before. The notice bar says how many paragraphs became headings.
- PDFs that number their headings (1. / 3.1 / (1) / (ア) / ① and so on) and set them in another typeface than the body (Gothic headings over Mincho text, for example) get those short numbered lines as headings, nested by size and then by how deep the numbering is (larger unnumbered text such as a cover title is heading 1, and numbered headings start at heading 2). The table of contents (lines ending in leader dots and a page number) becomes one list, nested as the PDF indents it, with the dots shortened to "……"; wrapped entries are joined, "1." is written "1\." so it is not read as a numbered list, and contents lines are never headings. Lines are now built from the height the letters sit at (in Japanese, a heading and the text below it could run into one line). A bullet set apart from its text goes back to the paragraph at the same height, as a list item.
- Read aloud reads only the selection when text is selected in the preview or the editor (Markdown marks such as headings, bold, and links are left out in the editor; the editor's selection is used only when the editor has the focus, or had it just before the button was pressed). A long selection is read a sentence or two at a time. Without a selection, reading starts at the first paragraph in view, as before.
- New from clipboard (`--paste`): one copied file (from File Explorer) that mdEditor opens is now opened, as when dropped outside the editor. Several files, or a type mdEditor does not open, still go into the new document.
- New samples: pdf-test.pdf, pdf-test.en.pdf, pdf-toc-test.en.pdf, word-headings-test.docx, and word-headings-test.en.docx, with steps in samples/import-test.
- THIRD-PARTY-NOTICES.md lists PdfPig and the license of the Adobe Glyph List inside it.

## 1.0.15 — 2026-10-23

- 簡体字中国語の表示を足した。ツールバーの言語の切り替えを「Language」の一覧（日本語・English・简体中文）にし、Ctrl+Shift+L では 3 つの言語を順に切り替える。初回は Windows の表示言語に合わせる（中国語なら簡体字中国語）。`--lang zh`（`zh-CN` も可）で起動時に指定できる。
- 簡体字中国語の画面では、漢字を中国語の書体（Microsoft YaHei）で表示し、ロゴは英語のロゴタイプを使う。記号なしのテキスト・メールの読み込みなどの言葉も中国語にした。
- 読み上げの言語に「中国語（簡体字）」を足した。自動では、かなを含まず漢字を含む段落を中国語の音声で読む。
- 「書き方」の例とサンプルに中国語版を足した（samples/diagram-test.zh-CN.md、github-test.zh-CN.md、import-test.zh-CN.md）。
- README を英語（README.md）・日本語（README.ja.md）・簡体字中国語（README.zh-CN.md）の 3 つに分けた。
- サンプルの画像・動画のフォルダーを「import-test-images」から、アプリが使う名前に合わせて「import-test_files」にした。samples/import-test（日本語・英語）の説明に残っていた「名前-images」も「名前_files」に直した。
- 「設定」に「クリップボードから新規作成」を足した。起動するキー（Ctrl+Alt と英字・数字）を選ぶと、確認のうえ、そのキーを割り当てたショートカットをデスクトップに作る。ほかのアプリでテキスト・画像・ファイルをコピーしてからそのキーを押すと、mdEditor が起動して新しい文書に入れる。書式付きの内容（HTML）は Markdown にし、画像とファイルは編集画面にドロップしたときと同じく「名前_files」フォルダーに置く。同じキーを使うほかのショートカットがあれば知らせる。「なし」にするとショートカットを削除する。起動オプション `--paste` でも同じことができる。Microsoft Store 版でも使えるよう、パッケージに起動用の別名（mdEditor.exe）を足した。
- 起動を速くした（この PC で、起動してから使えるようになるまで約 4.3〜4.8 秒 → 約 2.1〜2.2 秒）。画面の中の表示（アプリ自身のファイルだけを読む `mdeditor.local`）を Microsoft Defender SmartScreen に確かめさせるのをやめた（毎回約 2 秒待っていた。Web のリンクはこれまでどおりブラウザーで開く）。Mermaid と KaTeX は、図や数式のある文書を初めて表示するときに読み込む。発行するアプリは事前にコンパイルする（ReadyToRun。アプリのファイルは大きくなる）。
- 編集欄の見出しの「Markdown を編集」を画面から外し（読み上げソフトには伝える）、ボタンを左に寄せて 1 行に収まりやすくした。「編集画面をウィンドウ全体に表示」「プレビューをウィンドウ全体に表示」のボタンの文字を「ウィンドウ全体に表示」に短くした（読み上げソフトには、どちらの欄かを含めた元の名前を伝える）。
- 画面上部を詰めた。文書名・保存の状態・文字コードを、ロゴの段（ロゴタイプとシンボルマークの間。どちらからも十分に離す）に移した。「表示」（表示テーマ）と「用紙」のボタンは「設定」の中の「表示テーマと PDF の用紙」に移した（Ctrl+Shift+T と Ctrl+Shift+A はこれまでどおり）。幅 1262 px（倍率 90%）の画面で、ボタンが 1 行に収まる。狭い画面では、文書名はロゴの段の 2 行目に回る。
- Microsoft Store のパッケージに簡体字中国語（zh-CN）を足し、Store の掲載文に中国語の節を足した。スクリーンショット（`docs/images`）を今の画面で撮り直し、中国語版（`.zh-CN.png`）を足した。1 枚目は 3D モデルの画面にした。
- 画面全体の表示倍率の初期値を 90% にした。「初期値（90%）に戻す」と Ctrl + 0 も 90% に戻す。倍率を変えて保存している場合は、その倍率のまま。
- 「このアプリについて」の「作品・ほかのアプリ」のリンクを表示しないようにした。

- Added Simplified Chinese. The language switch in the toolbar is now a Language list (日本語, English, 简体中文); Ctrl+Shift+L goes through the three languages in turn. On first launch the language follows the Windows display language (Simplified Chinese for Chinese). `--lang zh` (or `zh-CN`) sets it at launch.
- In Simplified Chinese, Chinese characters use a Chinese typeface (Microsoft YaHei) and the logo is the English logotype. Plain text export, mail import, and other generated words are in Chinese too.
- Read-aloud has a Chinese (Simplified) language. In Automatic, paragraphs with Chinese characters and no kana are read with a Chinese voice.
- Chinese versions of the syntax examples and samples (samples/diagram-test.zh-CN.md, github-test.zh-CN.md, import-test.zh-CN.md).
- The README is split into English (README.md), Japanese (README.ja.md), and Simplified Chinese (README.zh-CN.md).
- The samples' image and video folder is now "import-test_files" (was "import-test-images"), the name the app itself uses. samples/import-test (Japanese and English) no longer describes the old "name-images" folders; it says "name_files".
- New in Settings: New from clipboard. Choose a start key (Ctrl+Alt with a letter or digit), and after you confirm, mdEditor creates a desktop shortcut with that key. Copy text, a picture, or files in any app, then press the key: mdEditor starts and puts them in a new document. Formatted content (HTML) becomes Markdown; pictures and files go in the "name_files" folder as when dropped on the editor. Other shortcuts using the same key are pointed out. None deletes the shortcut. The `--paste` option does the same. The Store package has an execution alias (mdEditor.exe) so the shortcut works there too.
- Faster startup (on this PC, from launch to ready: about 4.3-4.8 s to about 2.1-2.2 s). The app's own page (`mdeditor.local`, which reads only the app's files) is no longer checked by Microsoft Defender SmartScreen, which held up every launch by about 2 seconds; web links still open in the browser. Mermaid and KaTeX load the first time a document with a diagram or math is shown. Published builds are compiled ahead of time (ReadyToRun; the app files are larger).
- The editor heading no longer shows "Edit Markdown" on screen (screen readers still get it), and its buttons start at the left, so they fit on one line more often. The "Fill window with editor" and "Fill window with preview" buttons now read just "Fill window" (screen readers still get the full name, with the pane).
- A slimmer top of the window. The file name, save state, and text encoding moved into the header, between the logotype and the symbol mark (well clear of both). The Theme and Paper buttons moved into Settings under "Theme and PDF paper" (Ctrl+Shift+T and Ctrl+Shift+A work as before). At 1262 px (90% zoom) the toolbar fits on one line; on narrow screens the file name takes a second line in the header.
- The Microsoft Store package declares Simplified Chinese (zh-CN), and the Store listing has a Chinese section. The screenshots (`docs/images`) were retaken on the current screen, with Chinese versions added (`.zh-CN.png`); the first one now shows a 3D model.
- The whole window now starts at 90% zoom. Reset to default (90%) and Ctrl + 0 go back to 90%. A zoom level you saved before stays as it is.
- About no longer shows the Works and other apps link.

## 1.0.14 — 2026-10-20

- 文書に入れる画像・動画・ファイルのフォルダーを「名前-files」から「名前_files」にした（ブラウザーが Web ページを「完全」で保存するときの名前に合わせた）。1.0.13 で作った「名前-files」を使っている文書は、そのまま表示できる。
- ドロップしたファイルは、「名前_files」フォルダーの外にあるものをすべて（文書と同じフォルダーにあるものも、中身を入れた文書のリンク先も）「名前_files」に写すようにした。写したことはお知らせ欄に出す（「「A_files」フォルダーを作り、「B.pdf」を保存しました。」など）。同じ名前の別のファイルがあるときは、ブラウザーのダウンロードと同じ「B (1).pdf」の形で番号を付け（これまでは「B-2.pdf」）、中身が同じファイルがあるときは写さずにそれを使う。
- 「名前を付けて保存」で Markdown（またはテキストのそのまま）を別の場所か別の名前に保存すると、「名前_files」フォルダーも丸ごと新しい名前で複製し、文書の中のリンクを書き換えるようにした（保存するファイルと編集画面の両方。Ctrl+Z で戻せる。書き換えても未保存にはならない）。複製中は進み具合を、終わると「「C.md」と「C_files」フォルダーとして D:\資料 に複製しました。」などをお知らせ欄に出す。保存先に同じ名前のフォルダーがあれば中に足し、同じ名前の別のファイルには「(1)」などを付けてリンクも合わせる。別の場所では、フォルダーの外への相対リンクを元のファイルを指すように書き換える。元の文書とフォルダーは変えない。PDF・HTML・PNG・Word などへの保存は対象外。
- 「名前を付けて保存」に「配布用 ZIP（文書と「名前_files」フォルダー）」を足した。文書（.md）と「名前_files」フォルダーを、リンクのとおりの名前で 1 つの ZIP にする。フォルダーの中に文書から参照されていないファイルがあれば一覧で確かめ、「はい」でごみ箱に移してから、「いいえ」では残したまま、どちらも ZIP には入れない。フォルダーがない文書は、文書だけを ZIP にする。
- 「画像の調整」（Ctrl+Shift+F、編集欄の見出しのボタン）を足した。編集欄で画像の行にカーソルを置くか、プレビューの画像を右クリックすると、大きさ（元の大きさ、本文の幅の 25%・50%・75%・100%、ピクセル）と位置（指定しない、中央、右、左右に置いて文章を回り込ませる）を選べ、GitHub と同じ HTML（`<img width="50%">`、`<p align="center">`、`<img align="left">`）に書き換える。元に戻すと Markdown の画像に戻る。Ctrl+Z で取り消せる。
- Word に保存するとき、HTML で書いた画像（`<img>`）も画像として入れ、`width`・`height` と、中央・右・左右への寄せを反映するようにした（これまでは説明の文字だけ）。記号なしのテキストでは「［説明］」にした（これまでは消えていた）。
- 「書き方」に「画像の大きさと位置」を、samples/github-test（日本語・英語）に画像の大きさと位置の例を足した。
- samples/diagram-test（日本語・英語）に「22. 画像の大きさと位置」を足した（本文の幅に対する割合、中央・右、文章の回り込みと `<br clear="all">`）。画像は 5garashi.com設計事務所の公式アセット（シンボルマーク、ロゴタイプ、紹介画像。CC BY-ND 4.0）を改変せずに使い、ダークモードではロゴタイプを灰色抜きの版に切り替える。英語版では、日本語の入った紹介画像の代わりに英語のロゴタイプを使う。サンプルの画像はアプリにも同梱する。
- 地図の例（samples/diagram-test の 21-2 と README）を書き換えた。日本語版は富士山を、これまでの円の半分の大きさの三角にした。英語版はロンドン（大英博物館の三角とグリニッジ天文台の円、それを結ぶ線）にし、詳しい地図（OpenStreetMap）の地名も英語になるようにした。
- 「書き方」に、diagram-test から選んで短くした例を足した: 図（Mermaid）に「棒グラフ・折れ線グラフ」「タイムライン」、新しい群「GitHub と同じ書き方」に「注意書き」「折りたたみ」「キーの表示」「絵文字」、新しい群「そのほかの図と動画」に「Graphviz の図」「地図」「動画」。
- GitHub Flavored Markdown に合わせた: 文中のメールアドレス（`info@example.com`）をリンクにする。表の中のコードの `\|` を `|` にする（これまでは `\` が残っていた）。GitHub が残す古い属性（`<br clear="all">`、`border`、`nowrap`、`cellpadding`、`hspace` など）を取り除かずに使う（`<br clear="all">` で画像の回り込みを終えられる）。
- 「設定」→「Markdown の書き方」に「GitHub での表示に近づける」を足した（初期設定はオフ）。オンにすると、GitHub にない mdEditor 独自の書き方（`x^2^`、`H~2~O`、`==強調==`、`++挿入++`、定義リスト、`:::`、`{#id}`、略語、罫線の表、`a.` の番号）を書いたとおりの文字で表示し、`~文字~` を GitHub と同じく取り消し線にする。書き出しも同じ。図・コードの色分け・数式の描き方などは mdEditor のままなので、GitHub と完全に同じにはならない。
- samples/github-test.en.md の数式の `\`（`\int`・`\frac`・`\sqrt`・`\approx`）が抜けていたのを直した。

- The folder for images, videos, and files inserted in a document is now "name_files" instead of "name-files" (the name browsers use when saving a complete web page). Documents that use a "name-files" folder from 1.0.13 still show as before.
- Every dropped file outside the "name_files" folder (including files in the document's own folder and files that inserted content links to) is copied into "name_files", and the notice bar says so ("Created the A_files folder and saved "B.pdf" in it." and so on). A different file with the same name gets a number as browsers do for downloads, "B (1).pdf" (was "B-2.pdf"); a file with the same content already there is used instead.
- Save As to another folder or name, as Markdown (or text as written), copies the whole "name_files" folder under the new name and rewrites the document's links, both in the saved file and in the editor (Ctrl+Z undoes it; the document stays saved). The notice bar shows progress while copying, then, for example, "Copied as C.md and the C_files folder to D:\Docs.". An existing folder of that name gets the files added, with a number such as (1) for different files with the same name and links to match. In another folder, relative links outside the folder are changed to point to the original files. The original document and folder are not changed. Exports such as PDF, HTML, PNG, and Word are not affected.
- Save As has a new type, "ZIP for sharing, the document and its name_files folder". It puts the document (.md) and its "name_files" folder in one ZIP under the names the links use. Files in the folder that the document does not refer to are listed first: Yes moves them to the Recycle Bin, No keeps them; either way they stay out of the ZIP. A document without the folder goes in alone.
- New Adjust image command (Ctrl+Shift+F, a button in the editor heading). With the cursor on an image's line, or by right-clicking an image in the preview, choose its size (original, 25%, 50%, 75%, or 100% of the text width, or pixels) and position (not set, center, right, or left or right with text flowing around it). The image is rewritten in GitHub's HTML (`<img width="50%">`, `<p align="center">`, `<img align="left">`), and back into Markdown when reset. Ctrl+Z undoes it.
- Saving as Word now puts images written as HTML (`<img>`) in as pictures, with their `width`, `height`, and center, right, or left placement (before, only their description). Text without marks shows them as "[description]" (before, they were dropped).
- The Syntax list has Image size and position, and samples/github-test (Japanese and English) has examples of it.
- samples/diagram-test (Japanese and English) has a new section, 22. Image size and position (share of the text width, center and right, text flowing around an image and `<br clear="all">`). The images are official assets of 5garashi.com Design Office (symbol mark, logotype, and banner; CC BY-ND 4.0), used unmodified; in dark mode the logotype switches to its gray reversal. The English version uses the English logotype instead of the banner, which has Japanese in it. The sample images are bundled with the app.
- The map examples (samples/diagram-test 21-2 and the README) changed. The Japanese one marks Mt. Fuji with a triangle half the size of the old circle. The English one shows London (a triangle at the British Museum and a circle at the Royal Observatory, Greenwich, joined by a line), so the detailed OpenStreetMap map has English place names too.
- The Syntax list has short versions of examples from diagram-test: Bar and line chart and Timeline under Diagrams (Mermaid), a new group Written as on GitHub (Alert, Collapsible section, Keys, Emoji), and a new group Other drawings and video (Graphviz diagram, Map, Video).
- Closer to GitHub Flavored Markdown: e-mail addresses in the text (`info@example.com`) become links; `\|` in code inside a table shows as `|` (the `\` used to stay); and older attributes GitHub keeps (`<br clear="all">`, `border`, `nowrap`, `cellpadding`, `hspace`, and so on) are no longer removed, so `<br clear="all">` ends text flowing around a picture.
- New setting, Settings > Markdown syntax > "Show documents closer to GitHub" (off by default). When on, mdEditor's own syntax that GitHub does not have (`x^2^`, `H~2~O`, `==highlight==`, `++insert++`, definition lists, `:::`, `{#id}`, abbreviations, grid tables, `a.` numbering) shows as written, and `~text~` is struck through as on GitHub. Exports follow it. Diagrams, code colors, and how math is drawn stay mdEditor's own, so the display is closer to GitHub, not identical.
- Fixed the missing backslashes in the math of samples/github-test.en.md (`\int`, `\frac`, `\sqrt`, `\approx`).

## 1.0.13 — 2026-10-18

- ドラッグ＆ドロップを「編集画面に落とせば入れる、外に落とせば開く」にそろえた。ドラッグしている間は、ポインターの横に「カーソルの位置に入れる」「ファイルとして開く」を表示する。
  - 動画（.mp4・.m4v・.webm・.ogv・.mov）は画像と同じ扱いにした。「開く」で開け、編集画面に落とすと文書の横の「名前-files」フォルダーに写して `![](名前-files/動画.mp4)` を入れる。
  - 「開く」で開けるファイル（.md・.txt・.html・.docx・.eml・.csv・.json・.ipynb など）を編集画面に落とすと、開かずに、「開く」と同じように Markdown にした中身をカーソルの位置に入れる（これまでは開いていた）。中身の中の画像・動画は「名前-files」フォルダーに写し、ほかのファイルへのリンクは元のファイルを指すように書き換える。Markdown のフロントマターは入れない。入れたことはお知らせ欄に表示し、Ctrl+Z で取り消せる。
  - 開けないファイル（.pdf・.pptx・.zip など）は、編集画面に落とすと「名前-files」フォルダーに写してリンク（`[資料.pdf](名前-files/資料.pdf)`）を入れる。外に落としたときは、編集画面に落とすとリンクとして入ることを知らせる（これまでは「開けるファイルを1つだけドロップしてください」とだけ表示していた）。
- 文書に入れる画像・動画・ファイルの置き場所を、「名前-images」から「名前-files」フォルダーにした（ドロップ、貼り付け、Word・メール・Jupyter Notebook を開いたときの画像も）。前の版で作った「名前-images」を使っている文書は、そのまま表示できる。
- 「前の文書に戻す」（Ctrl+Shift+B）を加えた。別の文書を開く前に開いていた最後の文書に戻す。「保存しますか？」で「いいえ」を選んで閉じた文書は、閉じたときの状態（書きかけの内容・ファイルの場所・文字コード）に未保存のまま戻し、変更のないまま閉じた文書はそのファイルを開き直す。ボタンは戻せる文書があるときだけ「開く」の横に出る。控えは自動バックアップと同じ `%LOCALAPPDATA%\mdEditor\Recovery` に 1 つだけ置き（変更のない文書はファイルの場所だけ）、使うか次に文書を閉じると入れ替わる。自動バックアップがオフのときは控えを残さない（PRIVACY.md に追記）。
- README に、Markdown のまま人に渡すときは「名前-files」フォルダーも一緒に渡すこと（文書は相対パスで指しているため）を書いた。

- Drag and drop now follows one rule: dropped on the editor, a file goes in; dropped anywhere else, it opens. While you drag, a label beside the pointer says "Insert at the cursor" or "Open as a file".
  - Videos (.mp4, .m4v, .webm, .ogv, .mov) work like images: they open with Open, and dropped on the editor they are copied to the "name-files" folder beside the document with `![](name-files/video.mp4)` inserted.
  - A file Open accepts (.md, .txt, .html, .docx, .eml, .csv, .json, .ipynb, and so on) dropped on the editor no longer opens: its content, converted to Markdown as Open does, goes in at the cursor. Images and videos in it are copied to the "name-files" folder, and links to other files are changed to point to the original files. Front matter is left out. The notice bar says what went in, and Ctrl+Z undoes it.
  - A file mdEditor cannot open (.pdf, .pptx, .zip, and so on) dropped on the editor is copied to the "name-files" folder and inserted as a link (`[report.pdf](name-files/report.pdf)`). Dropped anywhere else, a message says that dropping it on the editor inserts a link (before, it only said "drop exactly one file mdEditor can open").
- Images, videos, and files inserted in a document now go in a "name-files" folder instead of "name-images" (drops, pastes, and the pictures of Word documents, e-mails, and Jupyter Notebooks that are opened). Documents that use a "name-images" folder from earlier versions still show as before.
- Back to previous document (Ctrl+Shift+B): goes back to the last document that was open before another one took its place. One closed with "No" to "Save changes?" comes back unsaved, as it was when it was closed (its text, file, and encoding); one closed without changes is opened again from its file. The button beside Open shows only while there is one. A single copy is kept in `%LOCALAPPDATA%\mdEditor\Recovery`, where automatic backup keeps its copies (only the file location for a document without changes), until it is used or replaced when the next document is closed. With automatic backup off, no copy is kept (added to PRIVACY.md).
- The README says to give the "name-files" folder along with the Markdown file, because the document points to it by relative paths.

## 1.0.12 — 2026-10-17

- GitHub と同じ書き方に対応した: 注意書き（`> [!NOTE]` など。色付きの枠）、フロントマター（YamlDotNet 18.1.0 で読み、「項目名と値」を縦に並べたカードとして表示。一覧はタグのような札、入れ子の項目はカードの中に並べ、Web のアドレスはリンクにする。左の線と項目名は灰色（Identity 4-3 の --document-muted））、` ```math ` のコードブロック、`` $`x`$ `` の文中の数式、絵文字の短縮名（`:smile:` など）。
- Markdown の中の HTML を、文字としてではなく、GitHub が許しているものだけ表示するようにした（`<details>`、`<summary>`、`<kbd>`、`<sup>`、`<sub>`、`<picture>`、画像の幅、中央寄せなど）。スクリプト・`<style>`・`<iframe>`・フォーム・イベント属性・`javascript:` のリンクは取り除く。HTML で書いたインターネット上の画像も、許可するまで読み込まない。昼と夜で切り替わる画像（`<picture>`、`#gh-dark-mode-only`）にも対応した。
- PDF・PNG・Word に保存するときは、折りたたみ（`<details>`）を開いた状態で入れる。Word には、注意書きの題名、フロントマターのカード（項目名と値の表。入れ子も同じ形）、HTML の部分の文字も入る。
- 動画（.mp4・.webm・.mov など）を再生できるようにした。画像の書き方（`![](clip.mp4)`）、動画へのリンクだけの行、`<video>` のどれでもよい。文書と同じフォルダーの動画はそのまま再生し、インターネット上の動画は画像と同じ設定（「設定」→「インターネット」）で、許可するまで読み込まない（お知らせ欄のボタンは「画像・動画を読み込む」）。動画は音を消して自動で再生し、繰り返す（音を出す・止めるは動画のボタンで）。Windows の「アニメーション効果」がオフのときは、初期設定では自動で再生せず、最初の画面と再生ボタンを表示する。「設定」→「動画」→「自動再生」で「いつも自動で再生する」「自動で再生しない」も選べる（Identity 7-5）。PDF・PNG・Word には動画の名前が、保存した HTML には動画へのリンクが入る。
- 確認用のサンプル samples/github-test.md（英語版あり。入れ子のフロントマターと動画の例を含む）を加えた。
- 3D モデル（```stl。文字の STL）を、同梱した three.js 0.186.1 でオフラインで表示するようにした。ドラッグで回転、ホイールで拡大・縮小。色は 5garashi の配色で、1 つ目の solid をクリーム（#FFF3D6）、2 つ目以降を volt（#B92613）で塗る（solid の名前に `#RRGGBB` を書くとその色）。クリームが見分けられるよう、地は濃い緑（#2B3A20。クリームとの比 11.0:1）にした（Identity 4-4）。PDF・PNG・HTML・Word には、その時点の見た目が画像として入る。
- 地図（```geojson・```topojson）を、同梱した d3-geo と簡単な世界地図（Natural Earth）でオフラインで描くようにした。初期設定では、お知らせ欄で「詳細な地図を読み込む」かを確認し、押すとその文書だけ、道路や地名の入った OpenStreetMap の地図を読み込む。「設定」→「インターネット」→「詳細な地図（OpenStreetMap）」で「いつも読み込む」「読み込まない」も選べる。保存した HTML には地図の画像を入れない。
- 「図・チャート表示テスト」のサンプルに、先頭のフロントマター（入れ子あり）、「20. GitHub と同じ書き方」、「21. 3D モデル・地図・動画」（各面に「5」の入った立方体、富士山を囲む円の地図、動画）を加えた。動画はアプリに同梱し、「サンプル」から開いたときも再生できる。
- README の先頭にフロントマターを置き、3D モデル・地図・動画の例を加えた（GitHub でも表示される。動画は GitHub ではリンクになる）。
- 保存してあるファイルを開いたとき、インターネット上の画像のお知らせ（「画像を読み込む」）が、文字コードのお知らせに消されて出なかったのを直した（1.0.8 から）。

- GitHub-style syntax: alerts (`> [!NOTE]` and so on, as colored boxes), front matter (read with YamlDotNet 18.1.0 and shown as a card of key and value rows: lists as small labels, nested items inside the card, and web addresses as links; the left line and keys use the muted gray of Identity 4-3), ` ```math ` code blocks, `` $`x`$ `` inline math, and emoji short names (`:smile:` and so on).
- HTML written in Markdown now shows what GitHub allows (`<details>`, `<summary>`, `<kbd>`, `<sup>`, `<sub>`, `<picture>`, image widths, centering, and so on) instead of showing as text. Scripts, `<style>`, `<iframe>`, forms, event attributes, and `javascript:` links are removed. Internet images written in HTML also wait for permission. Day and night pictures (`<picture>`, `#gh-dark-mode-only`) work.
- PDF, PNG, and Word include collapsed `<details>` sections opened. Word also gets alert titles, the front matter card (a key and value table, nested the same way), and the text of HTML parts.
- Videos (.mp4, .webm, .mov, and so on) play, written as an image (`![](clip.mp4)`), as a lone link, or as `<video>`. A video in the document's folder plays as it is; an internet video follows the image setting (Settings > Internet) and waits for permission (the notice bar button is now Load images and videos). Videos start by themselves without sound and loop (the video's controls turn the sound on or pause it). When Windows animation effects are off, by default they do not start and show the first frame with a play button; Settings > Videos > Autoplay also offers Always play automatically and Never play automatically (Identity 7-5). PDF, PNG, and Word get the video's name, and saved HTML a link to it.
- A test sample, samples/github-test.md (with an English version, including nested front matter and videos).
- 3D models (```stl, ASCII STL) are shown offline with the bundled three.js 0.186.1: drag to turn, wheel to zoom. Colors follow the 5garashi palette: the first solid is cream (#FFF3D6) and the others volt (#B92613); a `#RRGGBB` in a solid's name sets its color. The stage is dark green (#2B3A20, 11.0:1 against cream) so the cream stands out (Identity 4-4). PDF, PNG, HTML, and Word get a picture of the current view.
- Maps (```geojson, ```topojson) are drawn offline with the bundled d3-geo on a simple world map (Natural Earth). By default the notice bar asks whether to load detailed maps; Load detailed maps loads an OpenStreetMap map with roads and place names for that document only. Settings > Internet > Detailed maps (OpenStreetMap) also offers Always load and Never load. Saved HTML never includes the map images.
- The Diagram and chart test sample has front matter at the top (with nesting), "20. Written as on GitHub", and "21. 3D models, maps, and videos" (a cube with a "5" on each face, a map with a circle around Mt. Fuji, and videos). The video is bundled, so it plays when the sample is opened from Samples.
- The README starts with front matter and has examples of a 3D model, a map, and a video (they show on GitHub too; there the video is a link).
- Opening a saved file no longer loses the internet image notice (Load images): the encoding notice cleared it (since 1.0.8).

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
