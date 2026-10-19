---
title: mdEditor
description: Windows の Markdown エディター / Markdown editor and viewer for Windows
author:
  name: 5garashi.com設計事務所 / 5garashi.com Design Office
  url: https://www.5garashi.com/
license: MIT
tags: [markdown, windows, wpf, webview2]
---
<!--
  ファイル名 / File: README.md
  概要 / Description: Setup and usage guide for mdEditor
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-10
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# mdEditor

Windows で Markdown ファイルを開き、テキストと整形済み表示を並べて編集するアプリです。

*A Windows app that opens Markdown files and lets you edit the text side by side with a rendered preview.* [日本語](#特長) / [English](#english)

バージョン / Version: 1.0.14（変更の記録 / Changes: [CHANGELOG.md](./CHANGELOG.md)）

## 特長

Markdown を書きながら、整形された仕上がりをすぐ横で確かめられるエディタです。図や数式もそのまま表示し、PDF に書き出せます。書く・読む・残すが、PC の中だけで完結します。

### 書く・見る・書き出す

- **エディタとビューアが一体**: 左で Markdown を書くと、右のプレビューにすぐ反映されます。両側のスクロールが連動し、検索・置換や見出しへのリンクで移動しても同じ場所を表示します。片側だけをウィンドウいっぱいに広げて、エディタやビューアとしても使えます。
- **2 台のディスプレイで、書く画面と見る画面を分けられる**: プレビューだけを別のウィンドウに出し、2 台目のディスプレイで最大化や全画面表示にできます。ウィンドウが分かれても、編集欄とプレビューのスクロールは文書の同じ行でそろいます。広い画面でも本文は読みやすい幅に収まり、表や図は広く表示します。ウィンドウの場所は次回の起動でも使います。
- **図・グラフ・数式・表がそのまま見える**: フローチャート、シーケンス図、ガントチャート、円グラフなどの Mermaid の図、KaTeX の数式、表、チェックリスト、文書内の画像をプレビューに表示します。
- **PDF と HTML に書き出し**: プレビューの見た目のまま、図や数式も含めて PDF に保存します。用紙は A4 と Letter から選べます（初期設定は地域に合わせて自動）。1 つのファイルで完結する HTML にも保存でき、メールで送ったり Web に載せたりできます。
- **いろいろな形式を開いて、保存できる**: Word・HTML・Web ページ・メール・CSV・JSON・Jupyter Notebook・画像を開いて Markdown として編集でき、「名前を付けて保存」で PNG・Word・CSV・JSON・Jupyter Notebook・配布用 ZIP などにも保存できます。
- **書き方をその場で確認**: F1 で Markdown の書き方の一覧を表示し、例を編集欄に挿入できます。
- **文字コードで困らない**: UTF-8 だけでなく Shift_JIS や EUC-JP などの古いファイルも判定して開き、推定したときや読めない文字があるときは知らせます。保存する文字コードも選べます。
- **書いたものを守る**: 保存は書き込みが途中で失敗しても元のファイルが壊れない方法で行います。保存していない変更は自動でバックアップし、アプリが異常終了しても次の起動で復元できます。
- **ダークモード**: Windows の設定に合わせて自動で切り替わり、目にやさしい夜の配色で表示します。印刷と PDF は常に白地です。
- **プライバシー**: 文書の内容や利用状況を外部に送りません。日本語と英語の表示に対応します。

### だれにでも使いやすく（ユニバーサルデザイン）

- **読みやすさを選べる**: 読みやすい BIZ UD ゴシック、文字の大きさ（最大200%）と行間、画面全体の拡大、いまの行の強調、太いカーソルを選べます。Windows のコントラスト テーマにも対応します。必要な人だけがオンにでき、ふだんは標準のまま使えます。
- **キーボードだけで全操作**: すべてのボタンにショートカットキーがあり、好きなキーに変えたり、外したりできます。Tab と F6 で全部の欄を移動でき、押しやすい大きさ（44px 以上）のボタンにしています。
- **画面読み上げソフトに配慮**: Windows ナレーターで実際に確認し、長い文書でも止まらないように編集欄を作り直しました。ボタンには名前とショートカットキーを読み上げ用に付けています。
- **文章の読み上げ**: プレビューを段落ごとに読み上げ、読んでいる場所を画面にも示します。速さと段落の間を選べます。PC に入っている音声だけを使うので、文章が外に出ません。

## 画面

**書きながら、すぐ横で仕上がりを確認** — 左で Markdown を書くと、右のプレビューにすぐ反映されます。図・表・数式も、そのまま表示します。

![左で Markdown を書くと、右のプレビューにすぐ反映されます。図・表・数式も、そのまま表示します。](docs/images/1-main.ja.png)

**ダークモード** — Windows の設定に合わせて、昼と夜の配色を自動で切り替えます。「表示」ボタンで選ぶこともできます。印刷と PDF は常に白地です。

![Windows の設定に合わせて、昼と夜の配色を自動で切り替えます。「表示」ボタンで選ぶこともできます。印刷と PDF は常に白地です。](docs/images/2-dark.ja.png)

**プレビューを別のウィンドウに** — 2 台目のディスプレイに置いて、書く画面と見る画面を分けられます。スクロールは編集と連動します。

![2 台目のディスプレイに置いて、書く画面と見る画面を分けられます。スクロールは編集と連動します。](docs/images/3-preview-window.ja.png)

**書き方をその場で確認** — Markdown の書き方の一覧（F1）。「挿入」を押すと、カーソルの位置に例が入ります。

![Markdown の書き方の一覧（F1）。「挿入」を押すと、カーソルの位置に例が入ります。](docs/images/4-syntax.ja.png)

**いろいろな形式で保存** — 「名前を付けて保存」で、Markdown・テキスト・PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook・配布用 ZIP（文書と「名前_files」フォルダー）を選べます。Word・HTML・CSV・メール・Jupyter Notebook などのファイルも開けます。

![「名前を付けて保存」で、Markdown・テキスト・PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook・配布用 ZIP（文書と「名前_files」フォルダー）を選べます。Word・HTML・CSV・メール・Jupyter Notebook などのファイルも開けます。](docs/images/5-save-as.ja.png)

**コードの色分けと目次** — 言語名を書いたコードを色分けして表示します。「目次」ボタンで、見出しから目次を作って入れます。

![言語名を書いたコードを色分けして表示します。「目次」ボタンで、見出しから目次を作って入れます。](docs/images/6-code-toc.ja.png)

**3D モデル** — STL で書いた 3D モデルを、ドラッグで回して確かめられます。インターネットにつながなくても表示します。

![STL で書いた 3D モデルを、ドラッグで回して確かめられます。インターネットにつながなくても表示します。](docs/images/7-3d-model.ja.png)

**地図とフロントマター** — GeoJSON の地図は、許可すると OpenStreetMap の詳しい地図の上に描きます。先頭の YAML は、項目と値の一覧になります。

![GeoJSON の地図は、許可すると OpenStreetMap の詳しい地図の上に描きます。先頭の YAML は、項目と値の一覧になります。](docs/images/8-map.ja.png)

## セットアップ

1. .NET 10 SDK がインストールされた Windows で、`setup.cmd` をダブルクリックします（PowerShell から `powershell -ExecutionPolicy Bypass -File .\setup.ps1` を実行しても同じです）。`.ps1` はダブルクリックではメモ帳で開くだけで実行されないため、`setup.cmd` から呼び出します。アプリは `%LOCALAPPDATA%\Programs\mdEditor` に発行され、現在のユーザー向けに登録されます。`.md`・`.markdown` のほか、`.html`・`.htm` も「プログラムから開く」と「既定のアプリ」の候補に出ます（ブラウザーの関連付けは変えません）。Microsoft Store 版は `.html` を関連付けません（「開く」とドラッグ＆ドロップで開けます）。
2. Windows の「設定」→「アプリ」→「既定のアプリ」を開き、`.md` の既定アプリとして `mdEditor` を選びます。
3. `.md` ファイルを開くと、mdEditor の左側にテキスト、右側にプレビューが表示されます。

Windows は既定アプリをユーザーの操作で設定するため、セットアップ後に開く `mdEditor` の既定アプリ設定で、`.md` の既定アプリとして `mdEditor` を選択してください。Windowsの確認なしに既定アプリを変更することはできません。

mdEditor が開けるほかの形式（.txt、.html、.htm、.docx、.eml、.csv、.tsv、.json、.xml、.ipynb、画像）は、右クリックの「プログラムから開く」に mdEditor が出るようにしています（Microsoft Store 版も同じ）。これらの既定のアプリ（ブラウザーやメモ帳など）は変わりません。setup.cmd 版は候補に出るだけで、既定のアプリには使われません。Microsoft Store 版は、その PC にほかに開けるアプリがない形式（.ipynb や .json など）に限り、Windows の仕組みでダブルクリックでも mdEditor が開きます（ほかのアプリを既定に選べば、そちらで開きます）。

Microsoft Edge WebView2 Runtime が必要です。Microsoft Edge がインストールされている環境では、通常このランタイムも利用できます。

**Microsoft Store 用のパッケージ**: `setup.cmd` は開発用の手順です。Store 向けには、Windows SDK を入れた PC で `store\build-msix.ps1`（Partner Center の「製品の ID」の値を `-IdentityName` と `-Publisher` に指定）を実行すると、署名前の MSIX が `artifacts` に作られます。署名は Store が行います。

## バージョンの管理

バージョンは、リポジトリの一番上にある [VERSION](./VERSION) ファイルだけに書きます。アプリ（「このアプリについて」に表示）と Microsoft Store 用の MSIX（末尾に `.0` を付けた 4 桁）は、ここから読み取ります。新しい版を出すときは、次の 3 つを同じプルリクエストで変えます。

1. `VERSION` の番号を上げる（例: `1.0.3` → `1.0.4`）。
2. [CHANGELOG.md](./CHANGELOG.md) に `## 1.0.4` の節を足す。
3. この README の「バージョン / Version」の行を合わせる。

プルリクエストでは、GitHub Actions がこの 3 つがそろっているかを確かめます。main にマージされると、`v1.0.4` のタグと GitHub の Release（本文は CHANGELOG の節）が自動で作られます。

## 操作

- **サンプル**: 「図と数式」「基本の書き方」の短いサンプルと、使えるすべての図・チャート・数式を確かめる「図・チャート表示テスト」（日本語・英語。[samples/](./samples/) の 2 つのファイル）を、選んだときだけ新しい文書として開きます（開いていた文書に変更があれば、先に保存するか聞きます。開いていたファイルには書き込みません）。
- **このアプリについて**: 会社紹介・作品・サポート・プライバシーポリシーへのリンクを表示します。リンクは選んだときだけブラウザーで開きます。
- **開く**: Markdown ファイルを選択します。テキスト（.txt）、Word 文書（.docx）、HTML ファイル（.html、.htm）も開けます。テキストはそのまま開いて、そのまま上書き保存できます。Word 文書は Markdown に変換して開き、見出し・リスト・表・リンク・画像が戻ります（色や配置は戻りません）。画像は最初に保存するとき、Markdown ファイルの横に「名前_files」フォルダーとして置きます。Word の文書は書き換えません。HTML ファイル（.html、.htm）も開けます。HTML は Markdown に変換して開き、見出し・段落・リスト・表・リンク・画像・数式が戻ります（色や配置、図の絵は戻りません）。保存すると Markdown ファイルになり、元の HTML ファイルは書き換えません。
  - **メール（.eml）**: 件名を見出しにし、差出人・宛先・日付・添付ファイルの名前と本文を Markdown に変換して開きます。本文の中の画像は Word 文書と同じように扱います。メールの中のインターネット上の画像（受信の確認に使われることがあります）は、確認するまで読み込みません。添付ファイルの中身は取り出しません。
  - **CSV・TSV（.csv、.tsv）**: Markdown の表にして開きます。1 行目を見出しの行にします。Excel で保存した Shift_JIS の CSV も開けます。
  - **JSON（.json）**: 一覧（オブジェクトの配列）は表にして開き、項目の名前を見出しの行にします。それ以外の JSON はコードの枠に入れて開くので、前後に説明を書き足せます。
  - **XML（.xml）**: コードの枠に入れて開きます。前後に説明を書き足せます。
  - **Jupyter Notebook（.ipynb）**: Markdown のセルはそのまま、コードのセルは言語付きのコードの枠（```python など）になります。実行結果は、文字は「output」の枠、画像と表（pandas など）はそのまま表示します。セルに貼り付けた画像も表示します。
  - **画像（.png、.jpg、.gif、.bmp、.webp）**: その画像を表示する新しい文書として開きます。画像の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、画像もそこに写します。
  - **動画（.mp4、.m4v、.webm、.ogv、.mov）**: その動画を表示する新しい文書として開きます。扱いは画像と同じです。
- **ドラッグ＆ドロップ**: 編集画面にドロップしたファイルはカーソルの位置に入り、編集画面の外にドロップしたファイルは開きます。ドラッグしている間は、ポインターの横に「カーソルの位置に入れる」か「ファイルとして開く」を表示します。

  | ファイル | 編集画面にドロップ | 編集画面の外にドロップ |
  | :--- | :--- | :--- |
  | 画像・動画 | 文書の横の「名前_files」フォルダーに写し、`![](名前_files/画像.png)` を入れる | 画像・動画の文書として開く |
  | 「開く」で開けるファイル（.md、.txt、.html、.docx、.eml、.csv、.json、.ipynb など） | 「開く」と同じように Markdown にした中身を入れる | 開く（未保存の変更があれば、先に確認する） |
  | 開けないファイル（.pdf、.pptx、.zip など） | 「名前_files」フォルダーに写し、リンク `[資料.pdf](名前_files/資料.pdf)` を入れる | 開かずに、編集画面にドロップするとリンクとして入ることを知らせる |

  - 中身を入れたときは、お知らせ欄に表示します。開くつもりだったときは、Ctrl+Z で取り消して、編集画面の外にドロップし直します。
  - 中身が表示する画像・動画やリンク先のファイルも、「名前_files」フォルダーに写します。Markdown の先頭のフロントマター（`---` で囲んだ部分）は入れません。
  - 「名前_files」フォルダーの外にあるファイルは、文書と同じフォルダーにあるものも含めて、すべて「名前_files」フォルダーに写します。すでに「名前_files」フォルダーの中にあるファイルは、写さずにそのまま使います。
  - 写したことはお知らせ欄に出します（「「A_files」フォルダーを作り、「B.pdf」を保存しました。」など）。フォルダーに同じ名前の別のファイルがあるときは、ブラウザーのダウンロードと同じように「B (1).pdf」「B (2).pdf」と番号を付けます。中身がまったく同じファイルがあるときは、写さずにそれを使います。
  - ファイルを写す必要があるのにまだ保存していない文書では、先に保存先を聞きます。
  - 画面の切り取りなどを編集画面に貼り付けた（Ctrl+V）ときも、画像を「名前_files」フォルダーに置いて入れます。
  - **Markdown のまま人に渡すときは、「名前_files」フォルダーも一緒に渡してください。** 文書は、入れた画像・動画・ファイルを、文書のある場所からの相対パス（`名前_files/…`）で指しています。.md ファイルだけを渡すと、相手の PC では表示されず、リンクも開けません。.md ファイルとフォルダーを同じ場所に置いたまま、まとめて（たとえば 1 つの zip にして）渡します。「名前_files」フォルダーの外を指すリンク（自分で書いた `../資料/…` など）は、そのファイルも一緒に渡さないと開けません。PDF・HTML・PNG・Word に保存したものは画像が中に入りますが、動画と、リンクで入れたファイルは入りません。
- **URLを開く / Ctrl+Shift+K**: `https://` で始まる Web ページを、Markdown に変換して開きます。ブラウザーからリンクをドラッグして開くこともできます。ページのスクリプトは実行しないので、開いたあとに読み込まれる内容（ランキングの数字など）は入りません。
- **インターネット上の画像・動画**: 初期設定では読み込まず、お知らせ欄の「画像・動画を読み込む」で読み込みます（その文書だけ）。「設定」→「インターネット」で「いつも読み込む」「読み込まない」も選べます。`http://` の画像は読み込みません。手元にない画像やリンクは、ページの元のアドレス（`<base>`、canonical、og:url）から探します。
- **保存**: 現在のファイルに上書きします。新規文書は保存先を選びます。
- **名前を付けて保存**: 保存先とファイル名を選びます。「開く」と同じように、ファイルの種類で保存の形式を選べます。
  - Markdown（またはテキストのそのまま）で別の場所か別の名前に保存すると、「名前_files」フォルダーも丸ごと複製します。名前を変えたときは、フォルダーも新しい名前（「C.md」なら「C_files」）で複製し、文書の中のリンクも書き換えます（保存するファイルと編集画面の両方。Ctrl+Z で戻せます）。元の文書とフォルダーは変えません。
  - 複製している間は、お知らせ欄に進み具合（何個目か）を出し、終わると「「A.md」と「A_files」フォルダーを D:\資料 に複製しました。」「「C.md」と「C_files」フォルダーとして複製しました。」「「C.md」と「C_files」フォルダーとして D:\資料 に複製しました。」と出します。
  - 保存先に同じ名前のフォルダーがあるときは、上書きせずに中へ足します。同じ名前の別のファイルには「(1)」などの番号を付け、リンクも合わせます。中身が同じファイルは、そのまま使います。
  - 別の場所に保存したとき、「名前_files」フォルダーの外を指す相対リンク（自分で書いた `../資料/x.pdf` など）は、元のファイルを指すように書き換えます。
  - PDF・HTML・PNG・Word などへの保存は、画像が中に入るので、フォルダーは複製しません。
  - **Markdown（.md）・テキスト（そのまま）（.txt）**: 文書そのものを保存します（中身はそのまま）。以後の「保存」はこのファイルに上書きします。
  - **テキスト（記号なし）・PDF・HTML・PNG・Word（.docx）・CSV・TSV・JSON・Jupyter Notebook・配布用 ZIP**: 文書を書き出します。編集中の文書のファイルはそのままです。書き出しの内容は下のとおりです。
- **テキスト（記号なし）**: プレビューに見えている文字だけを .txt にします。見出しや太字の記号は消え、行は書いたとおりに改行します。リストは「・」か番号、表はタブ区切り、リンクは「文字（アドレス）」、図は「［図］」になります。
- **CSV・TSV**: 文書の中の表を CSV（カンマ区切り）か TSV（タブ区切り）にします。表が 2 つ以上あるときは「名前-1.csv」「名前-2.csv」…に分けて保存します。Excel で文字化けしないよう、UTF-8（BOM 付き）で保存します。
- **JSON**: 文書の中の表を、見出しの行を項目の名前にしたオブジェクトの配列にします（UTF-8）。値は文字のまま書き出すので、「007」のような番号の 0 も消えません。表が 2 つ以上あるときは「名前-1.json」…に分けて保存します。
- **Jupyter Notebook**: 言語付きのコードの枠をコードのセルに、それ以外を Markdown のセルにします。実行結果は入れません（Jupyter で実行すると作り直されます）。.ipynb を開いて編集した文書を保存し直すと、元のノートブックと同じセルの並びに戻ります。
- **配布用 ZIP**: 人に渡すための ZIP にします。中身は文書（.md）と「名前_files」フォルダーで、名前はリンクのとおりなので、展開するとそのまま表示できます。文書は編集画面の内容（未保存の変更も含む）で入れます。「名前_files」フォルダーがない文書（まだ保存していない文書も）は、文書だけを入れます。
  - 「名前_files」フォルダーに、文書から参照されていないファイル（リンク・画像・HTML の src や href のどれにも書かれていないもの）があるときは、一覧を見せて確かめます。「はい」でごみ箱に移してから ZIP にし、「いいえ」ではファイルを残したまま ZIP には入れません。どちらでも、参照されていないファイルは ZIP に入りません。
  - 「名前_files」フォルダーの外のファイルへのリンクは、ZIP に入れず、その数を知らせます。
- **Ctrl+O**: ファイルを開きます。
- **前の文書に戻す / Ctrl+Shift+B**: 別の文書を開く前に開いていた文書に戻します。「開く」の横のボタンは、戻せる文書があるときだけ表示します。たとえば、編集画面の外にファイルをドロップして別の文書を開いてしまったときや、そのとき「キャンセル」のつもりで「いいえ」を押したときに使います。
  - 「保存しますか？」で「いいえ」を選んで閉じた文書は、閉じたときの状態（書きかけの内容・ファイルの場所・文字コード）に、まだ保存していない状態で戻ります。保存すると、元のファイルに上書きします。
  - 変更のないまま閉じた文書は、そのファイルを開き直します。ファイルが移動・削除されていたときは、そう知らせます。
  - 戻せるのは、最後に閉じた 1 つの文書だけです。ほかのウィンドウで閉じた文書や、アプリを終了するときに「いいえ」で閉じた文書も戻せます（次の起動のあとも）。「開く」のファイル選択を取り消したときのように、文書が入れ替わらなかったときは、前の文書はそのままです。
  - 戻す前に、今の文書に変更があれば保存するか聞きます。ここで「いいえ」を選ぶと、今の文書と入れ替わり、もう一度押すと今の文書に戻れます。
  - 控えは自動バックアップと同じ場所に置きます。変更のない文書は、ファイルの場所だけを控えます。設定で自動バックアップをオフにしているときは控えを残さず、このボタンも出ません。
- **Ctrl+S**: 保存します。
- **Ctrl+Shift+S**: 名前を付けて保存します。
- **PDF**: 整形したプレビューをPDFに書き出します。
- **PNG / Ctrl+Shift+G**: プレビューを昼の配色で画像（PNG）にします。長い文書は「名前-1.png」「名前-2.png」…に分けて保存します。
- **Word / Ctrl+Shift+D**: Word 文書（.docx）に書き出します。見出し・段落・リスト・引用・コード・表・リンク・画像は Word の標準のスタイルになるので、Word のナビゲーションや目次がそのまま使えます。図と数式はプレビューの画像として入ります（Word では編集できません）。
- **HTML / Ctrl+Shift+M**: 整形したプレビューを、1 つで完結する HTML ファイルに書き出します。図は SVG、数式は MathML で入り、文書と同じフォルダーの画像はファイルの中に埋め込みます。ブラウザーで開くと、ダークモードなら夜の配色で表示します。開いても外部に通信しません（文書の中のネット上の画像だけは、その画像のサーバーから読み込みます）。
- **用紙**: PDFの用紙を「自動」→「A4」→「Letter」の順に切り替えます。「自動」は Windows の地域の設定に従います（ヤード・ポンド法の地域は Letter、それ以外は A4）。選んだ用紙は次回の起動でも使います。
- **文字コード**: ファイル名の横に文書の文字コードを表示します。押すと文字コードを選び、「この文字コードで開き直す」または「次の保存からこの文字コードにする」を選べます。
- **Ctrl+Shift+P**: PDFに保存します。
- **目次 / Ctrl+Shift+J**: 文書の見出しから目次（見出しへのリンクの一覧）を作り、カーソルの位置に入れます。目次は `<!-- toc -->` と `<!-- /toc -->` の間に入り（プレビューには出ません）、見出しを変えたあともう一度押すと、その場所の目次を新しくします。文書の題名（先頭のただ 1 つの `#` 見出し）は目次に入れず、3 段階の深さまで入れます。目次のリンクは、プレビュー・PDF・HTML・Word のどれでも見出しに移動します。
- **画像の調整 / Ctrl+Shift+F**: 画像の大きさと位置を選び、GitHub と同じ書き方（HTML の `<img>`）に書き換えます。編集欄で画像の行にカーソルを置いて押すか、プレビューの画像を右クリックします。
  - 大きさ: 元の大きさ、本文の幅の 25%・50%・75%・100%、幅のピクセル指定（`width="50%"`、`width="300"`）。高さは縦横の比を保ちます。
  - 位置: 指定しない（左）、中央（`<p align="center">`）、右（`<p align="right">`）、左または右に置いて文章を回り込ませる（`align="left"`・`align="right"`）。中央と右は、画像だけの行で選べます。画像の下に続けて書いた文があるときは、空行を 1 つ入れます（HTML のかたまりが文を飲み込まないように）。
  - 「元の大きさ」と「指定しない」に戻すと、Markdown の `![説明](ファイル)` に戻ります。書き換えは Ctrl+Z で取り消せます。
  - PDF・PNG・HTML はプレビューのとおりになります。Word では、大きさと、中央・右・左右への寄せを反映します（文章の回り込みは、左右に寄せた画像になります）。記号なしのテキストでは、Markdown の画像と同じく「［説明］」になります。
- **Ctrl+H**: 編集画面の検索・置換バーを開きます。「置換」「すべて置換」が使えます（`Enter` で次を検索、`Esc` で閉じる）。
- **書き方 / F1**: Markdown の書き方を「基本」「図（Mermaid）」「数式（KaTeX）」「GitHub と同じ書き方」「そのほかの図と動画」に分けて一覧で表示します。図はフローチャート・シーケンス図・ガントチャート・円グラフ・クラス図・状態遷移図・ER 図・マインドマップ・棒グラフと折れ線グラフ・タイムライン、数式は分数・平方根・総和・行列・場合分けなど 10 種類で、図は一覧の中に描いて見せます。GitHub と同じ書き方は注意書き・折りたたみ・キーの表示・絵文字、そのほかの図と動画は Graphviz・地図（GeoJSON）・動画です（どれも samples/diagram-test から選んで短くした例）。「挿入」で編集欄のカーソル位置に例を入れます（`Esc` で閉じる）。
- **Ctrl+F**: ウィンドウ標準の検索です。プレビューの検索にも使えます。
- **スクロールの連動**: 編集欄とプレビューのどちらかをスクロールすると、もう一方も同じ位置に移動します。検索・置換や見出しへのリンクで移動したときも連動します。
- **見出しへのリンク**: `[日本語](#セットアップ)` のような文書内のリンクを押すと、プレビューがその見出しへ移動します。見出しのIDは GitHub と同じ規則（日本語はそのまま、英字は小文字、空白は `-`）で作ります。
- **ほかのファイルへのリンク**: `[ライセンス](./LICENSE)` のように、文書と同じフォルダーにあるファイルへのリンクを押すと、Markdown とテキストのファイル（拡張子なし、`.md`、`.txt` など）は新しい mdEditor のウィンドウで開きます。元の文書はそのまま残ります。`other.md#見出し` ならその見出しを表示します。PDF・画像・CSV などはふだん使うアプリで開き、プログラムなど開くと危険なファイルは実行せず、エクスプローラーで場所を示します。
- **左右の区切り**: 中央の境界線をドラッグすると編集欄とプレビューの幅を調整できます。区切りをフォーカスして左右矢印キーを押すと、幅を5%ずつ調整します。狭い画面では縦方向の区切りを上下矢印キーで操作します。幅の設定も次回起動時に保持されます。
- **編集画面 / プレビューをウィンドウ全体に表示**: 各欄のボタンで、その欄をウィンドウいっぱいに広げます（OSの全画面にはなりません）。「元の表示に戻す」ボタンまたは `Esc` で並列表示に戻ります。
- **ウィンドウの全画面表示**: `F11` で切り替えます。解除は `F11` または `Esc` です。
- **プレビューを別ウィンドウで表示 / Ctrl+Shift+3**: プレビューの見出しの「別ウィンドウで表示」で、プレビューだけのウィンドウを開きます。2 台目のディスプレイに置いて最大化したり、`F11` で全画面にしたりできます。本体は編集欄だけになり、横幅いっぱいに使えます。
  - 編集欄とプレビューのスクロールは、ウィンドウが分かれても連動します（文書の同じ行を表示します）。見出しへのリンク、検索・置換での移動、読み上げもそのまま使えます。
  - 別ウィンドウで押したショートカット（保存、検索・置換、設定など）は本体で実行します。
  - 「本体に戻す」（別ウィンドウ）、「プレビューを本体に戻す」（本体の編集欄）、または別ウィンドウを閉じると、元の並んだ表示に戻ります。
  - ウィンドウの位置・大きさ・最大化と、別ウィンドウにしていたかどうかは、次回の起動でも使います。そのディスプレイが外されていたら、見える場所に移します。
- **キー操作 / Shift+F1**: すべての操作のキーボードショートカットを一覧で表示します。「変更」でキーを好きな組み合わせ（Ctrl と英字・数字、または F キー）に変え、「なし」で外せます。「キーボードショートカットを使う」をオフにすると、すべてのショートカットを止められます。設定は次回の起動でも使います。
- **設定 / Ctrl+Shift+O**: 読みやすさと操作のしやすさを、使う人に合わせて調整します。自動バックアップ以外は初期設定がオフ（標準）で、必要な人だけが選べます。設定は次回の起動でも使います。
  - **書体**: 「BIZ UD ゴシック」（Windows に入っているユニバーサルデザイン書体）に切り替えます。
  - **文字の大きさ・行間**: 編集欄とプレビューの文字を 125%・150%・200% に、行間を「広め」「とても広い」にできます。
  - **Markdown の書き方**: 「GitHub での表示に近づける」をオンにすると（初期設定はオフ）、GitHub にない mdEditor 独自の書き方（`x^2^` の上付き、`H~2~O` の下付き、`==強調==`、`++挿入++`、定義リスト、`:::` の囲み、`{#id}` などの属性、略語、罫線の表、`a.` の番号）を、GitHub と同じく書いたとおりの文字で表示し、`~文字~` を取り消し線にします。PDF・Word などの書き出しも同じです。GitHub と完全に同じになるわけではありません（図・地図・3D モデル、コードの色分け、数式の描き方などは mdEditor のままです）。設定は次回の起動でも使います。
  - **書き出し**: 「HTMLに保存するとき、元の Markdown も入れる」をオンにすると（初期設定はオフ）、保存した HTML を mdEditor で開いたとき、書いたときのとおり（図や数式の書き方も含めて）に戻ります。HTML の中に元の文章がそのまま入り、画面に出ないコメント（`<!-- -->`）も含まれるので、人に渡す前に確かめてください。
  - **プレビューの本文の幅**: 広い画面では、読みやすいように本文の幅を全角でおよそ 44 字までにして中央に置きます（表・図・コード・数式はもっと広く表示します）。「プレビューの本文の幅を制限しない」で、画面の端まで使えます。
  - **画面全体の表示倍率**: 「拡大」「縮小」「100% に戻す」で画面全体を拡大・縮小します。Ctrl + マウスホイール、Ctrl + プラス／マイナス、Ctrl + 0 でも変えられ、倍率は次回の起動でも使います。
  - **編集欄**: 「いまの行を強調する」「カーソルを太くする」を選べます。
  - **読み上げ**: オンにすると、プレビューの見出しに「読み上げ」「停止」ボタンが出ます。プレビューに見えている段落から順に読み上げ、読んでいる段落を枠で示します。速さ（5段階）、段落の間（なし・0.5秒・1秒・2秒）、言語（自動・日本語・英語）、音声（この PC に入っている音声から）を選べます。その言語の音声がないときは、追加のしかたを表示します。この PC に入っている音声だけを使うため、文章を外部に送りません。読み上げ中は `Esc` でも止まります。
  - **自動バックアップ**（初期設定はオン）: 保存していない変更を15秒ごとに `%LOCALAPPDATA%\mdEditor\Recovery` に控えます。アプリが異常終了したときは、次の起動時に復元するか確認します。保存するか、正常に終了すると控えは消えます。
- **Windows のコントラスト テーマ**: Windows の「コントラスト テーマ」を使うと、その配色に合わせて表示し、ボタンの枠とフォーカスの印を保ちます。

### キーボードショートカット（初期設定）

| 操作 | キー | 操作 | キー |
| :--- | :--- | :--- | :--- |
| 開く | Ctrl+O | 検索・置換 | Ctrl+H |
| 目次を入れる・新しくする | Ctrl+Shift+J | 前の文書に戻す | Ctrl+Shift+B |
| 画像の大きさと位置 | Ctrl+Shift+F | | |
| 保存 | Ctrl+S | 次を検索 / 前を検索 | F3 / Shift+F3 |
| 名前を付けて保存 | Ctrl+Shift+S | すべて置換 | Ctrl+Shift+H |
| PDFに保存 | Ctrl+Shift+P | Markdown の書き方 | F1 |
| 用紙の切り替え | Ctrl+Shift+A | キー操作の一覧 | Shift+F1 |
| 表示テーマの切り替え | Ctrl+Shift+T | 編集画面 / プレビューをウィンドウ全体に表示 | Ctrl+Shift+1 / Ctrl+Shift+2 |
| 表示言語の切り替え | Ctrl+Shift+L | 次の欄 / 前の欄へ移動（ツールバー・編集欄・プレビュー） | F6 / Shift+F6 |
| 文字コード | Ctrl+Shift+E | ウィンドウの全画面表示 | F11 |
| 設定 | Ctrl+Shift+O | 読み上げ・一時停止 / 停止（読み上げがオンのとき） | Ctrl+Shift+Y / Ctrl+Shift+U |
| プレビューを別ウィンドウで表示・本体に戻す | Ctrl+Shift+3 | HTMLに保存 | Ctrl+Shift+M |
| URLを開く | Ctrl+Shift+K | PNGに保存 / Wordに保存 | Ctrl+Shift+G / Ctrl+Shift+D |

`Esc` は常に、読み上げを止め、全体表示や全画面表示を元に戻します。各ボタンのヒントと、画面読み上げソフトに伝える情報（`aria-keyshortcuts`）にも、いまのキーを表示します。

Markdown の変更はプレビューに反映されます。未保存の変更がある状態でファイルを開く、またはアプリを閉じると、保存するか確認します。

## 表示言語 / Display language

画面と各種ダイアログは日本語と英語に対応します。初回は OS の表示言語（日本語なら日本語、それ以外は英語）で起動し、ツールバーの「English / 日本語」ボタンでいつでも切り替えられます。選択は次回以降も保持されます。

起動オプションで言語を指定することもできます。指定した言語はその起動の間だけ有効で、保存済みの選択は書き換えません。

```
mdEditor.exe --lang en
mdEditor.exe --lang ja "C:\docs\memo.md"
```

The interface and dialogs support Japanese and English. On first launch the language follows the OS display language (Japanese if it is Japanese, otherwise English). Use the "English / 日本語" button in the toolbar to switch at any time; the choice is remembered. The `--lang en` or `--lang ja` option (also `--lang=en`) sets the language for that launch only and does not overwrite the saved choice. (The same section applies to the English part below.)

## Markdown とプレビュー

見出し、リスト、チェックリスト、引用、表（左・中央・右寄せ）、コード、リンク、画像などのMarkdownを表示します。Markdown の中に書いた HTML は、GitHub が許しているもの（`<details>`・`<summary>`・`<kbd>`・`<sup>`・`<sub>`・`<br>`・`<picture>`、画像の幅、`align` による中央寄せなど）だけを表示し、それ以外は取り除きます。スクリプト・`<style>`・`<iframe>`・埋め込み・フォーム・`onclick` などのイベント属性・`javascript:` のリンクは、GitHub と同じく取り除かれ、動きません。HTML で書いたインターネット上の画像も、Markdown の画像と同じく、許可するまで読み込みません。HTMLコメント（`<!-- -->`）は表示しません。Webリンクは既定のブラウザー、メールリンクは既定のメールアプリで開きます。PDFには整形済みのプレビューのみを出力し、編集中のテキスト欄や操作ボタンは含めません。

`images/a.png` のような相対パスの画像は、文書と同じフォルダー（とその中のフォルダー）から読み込みます。読み込むのは画像ファイル（PNG、JPEG、GIF、WebP、AVIF、BMP、ICO、SVG）だけで、文書のフォルダーより上の階層は読みません。

文書の文字コードは、UTF-8（BOMの有無）、UTF-16、Windowsの既定の文字コード（日本語環境ではShift_JIS）を自動で判別し、保存するときも元の文字コードのまま書き込みます。元の文字コードで保存できない文字がある場合は、UTF-8で保存するか確認します。UTF-8 ではないファイルを推定した文字コードで開いたときや、読めない文字（�）があるときは、ツールバーの下にお知らせを表示します。読めない文字があるまま上書き保存しようとすると、元のファイルのその部分が失われることを確認します。EUC-JP や ISO-2022-JP のファイルは、文字コードを選んで開き直してください。保存は一時ファイルに書いてから置き換えるので、途中で失敗しても元の文書は壊れません。

` ```mermaid ` のコードブロックは [Mermaid](https://mermaid.js.org/) で図として表示します（フローチャート、シーケンス図、クラス図、状態遷移図、ER図、ガントチャート、円グラフなど）。`$...$` と `$$...$$` の数式は [KaTeX](https://katex.org/) で表示します。どちらもアプリに同梱しているため、オフラインでも表示でき、PDFにも出力されます。` ```python ` のように言語名を書いたコードブロックは、[highlight.js](https://highlightjs.org/) で色分けして表示します（Python、JavaScript、TypeScript、C#、C/C++、Java、PowerShell、シェル、SQL、HTML、CSS、JSON、YAML、diff など約 40 の言語）。色はプレビューの昼と夜の配色に合わせ、PDF・PNG・HTML・Word に保存したときも付きます。言語名のないコードブロックは色を付けません。図の構文に誤りがある場合は、エラーメッセージと元のコードを表示します。表示の確認には [samples/diagram-test.md](./samples/diagram-test.md)（英語版: [samples/diagram-test.en.md](./samples/diagram-test.en.md)）を使えます。

### GitHub と同じ書き方

GitHub の README などで使われる次の書き方も、GitHub と同じように表示します。確認には [samples/github-test.md](./samples/github-test.md)（英語版: [samples/github-test.en.md](./samples/github-test.en.md)）を使えます。

| 書き方 | 例 | 表示 |
| :--- | :--- | :--- |
| 注意書き（アラート） | `> [!NOTE]`、`[!TIP]`、`[!IMPORTANT]`、`[!WARNING]`、`[!CAUTION]` | 色付きの線・記号・題名の枠 |
| フロントマター | 先頭の `---` と `---` の間の `title: …` など（YAML） | 「項目名と値」を縦に並べたカード。一覧はタグのような札、入れ子の項目はカードの中に並ぶ（Word も同じ形）。GitHub では表になる |
| 動画 | `![](clip.mp4)`、動画へのリンクだけの行、`<video src="…">`（.mp4・.webm・.mov など） | 音を消して自動で再生し、繰り返す（音と一時停止は動画のボタンで）。Windows の「アニメーション効果」がオフのときは、「設定」→「動画」で選ぶまで再生ボタンを表示。インターネット上の動画は画像と同じく許可するまで読み込まない。PDF・PNG・Word には動画の名前、HTML には動画へのリンク |
| 数式のコードブロック | ` ```math ` | 数式（KaTeX） |
| 文中の数式（GitHub の書き方） | `` $`x^2`$ `` | 数式 |
| 絵文字の短縮名 | `:smile:`、`:+1:` | 絵文字（`:)` などはそのまま） |
| 折りたたみ | `<details><summary>…</summary>…</details>` | 押すと開く。PDF・PNG・Word に保存するときは開いた状態で入る |
| 画像の大きさと位置 | `<img src="…" alt="…" width="50%">`、`<p align="center"><img …></p>`、`<img … align="left">` | 本文の幅に対する割合かピクセルの幅、中央・右寄せ、文章の回り込み。「画像の調整」（Ctrl+Shift+F）かプレビューの右クリックで書き換えられる。Word にも大きさと寄せが入る |
| メールアドレス | `info@example.com` | メールのリンク（文末の「.」は含めない。コードの中はリンクにしない） |
| 表の中のコードの縦棒 | 表のセルのコードの中で、縦棒の前に `\` を付ける | コードの中に縦棒 1 本（`\` は表示しない） |
| GitHub が残す古い属性 | `<br clear="all">`、`<table border="1">`、`nowrap`、`cellpadding`、`hspace` など | 書いたとおりに効く。`<br clear="all">` で画像の回り込みを終えられる |
| 昼と夜で切り替わる画像 | `<picture>` と `<source media="(prefers-color-scheme: dark)">`、画像の URL の末尾の `#gh-dark-mode-only` / `#gh-light-mode-only` | 表示の配色に合う画像だけを表示 |

### 3D モデル・地図・動画の例

この README の先頭の表（mdEditor ではカード）は、YAML のフロントマター（`---` で囲んだ部分）です。下の 3 つも、GitHub と mdEditor のどちらでも表示されます（動画は、GitHub では動画へのリンクになります。GitHub が再生するのは、GitHub に上げた動画だけです）。

```stl
solid cube
facet normal 0 -1 0 outer loop vertex -14 -14 -14 vertex 14 -14 -14 vertex 14 -14 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -14 -14 -14 vertex 14 -14 14 vertex -14 -14 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 14 14 -14 vertex -14 14 -14 vertex -14 14 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 14 14 -14 vertex -14 14 14 vertex 14 14 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 14 -14 -14 vertex 14 14 -14 vertex 14 14 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 14 -14 -14 vertex 14 14 14 vertex 14 -14 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -14 14 -14 vertex -14 -14 -14 vertex -14 -14 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -14 14 -14 vertex -14 -14 14 vertex -14 14 14 endloop endfacet
facet normal 0 0 1 outer loop vertex -14 -14 14 vertex 14 -14 14 vertex 14 14 14 endloop endfacet
facet normal 0 0 1 outer loop vertex -14 -14 14 vertex 14 14 14 vertex -14 14 14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 14 -14 vertex 14 14 -14 vertex 14 -14 -14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 14 -14 vertex 14 -14 -14 vertex -14 -14 -14 endloop endfacet
endsolid cube
solid letters
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 7.5 vertex 7.5 -15.5 7.5 vertex 7.5 -15.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 7.5 vertex 7.5 -15.5 10.5 vertex -7.5 -15.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 1.5 vertex -4.5 -15.5 1.5 vertex -4.5 -15.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 1.5 vertex -4.5 -15.5 7.5 vertex -7.5 -15.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 1.5 vertex 4.5 -15.5 1.5 vertex 4.5 -15.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 1.5 vertex 4.5 -15.5 4.5 vertex -4.5 -15.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -15.5 -7.5 vertex 7.5 -15.5 -7.5 vertex 7.5 -15.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -15.5 -7.5 vertex 7.5 -15.5 1.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 -7.5 vertex -4.5 -15.5 -7.5 vertex -4.5 -15.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 -7.5 vertex -4.5 -15.5 -4.5 vertex -7.5 -15.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 -10.5 vertex 4.5 -15.5 -10.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 -10.5 vertex 4.5 -15.5 -7.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 10.5 vertex 7.5 -15.5 10.5 vertex 7.5 -14 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 10.5 vertex 7.5 -14 10.5 vertex -7.5 -14 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 7.5 vertex 7.5 -14 7.5 vertex 7.5 -15.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 7.5 vertex 7.5 -15.5 7.5 vertex -4.5 -15.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 4.5 vertex 4.5 -15.5 4.5 vertex 4.5 -14 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 4.5 vertex 4.5 -14 4.5 vertex -4.5 -14 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 1.5 vertex 4.5 -14 1.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 1.5 vertex 4.5 -15.5 1.5 vertex -7.5 -15.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -15.5 1.5 vertex 7.5 -15.5 1.5 vertex 7.5 -14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -15.5 1.5 vertex 7.5 -14 1.5 vertex 4.5 -14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 -4.5 vertex -4.5 -15.5 -4.5 vertex -4.5 -14 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 -4.5 vertex -4.5 -14 -4.5 vertex -7.5 -14 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 -7.5 vertex -4.5 -14 -7.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 -7.5 vertex -4.5 -15.5 -7.5 vertex -7.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 -14 -7.5 vertex 7.5 -14 -7.5 vertex 7.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 -14 -7.5 vertex 7.5 -15.5 -7.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 -7.5 vertex 4.5 -15.5 -7.5 vertex 4.5 -14 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 -7.5 vertex 4.5 -14 -7.5 vertex -4.5 -14 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 -10.5 vertex 4.5 -14 -10.5 vertex 4.5 -15.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 -10.5 vertex 4.5 -15.5 -10.5 vertex -4.5 -15.5 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 10.5 vertex -7.5 -14 1.5 vertex -7.5 -15.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 10.5 vertex -7.5 -15.5 1.5 vertex -7.5 -15.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 -4.5 vertex -7.5 -14 -7.5 vertex -7.5 -15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 -4.5 vertex -7.5 -15.5 -7.5 vertex -7.5 -15.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 7.5 vertex -4.5 -15.5 4.5 vertex -4.5 -14 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 7.5 vertex -4.5 -14 4.5 vertex -4.5 -14 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 -4.5 vertex -4.5 -15.5 -7.5 vertex -4.5 -14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 -4.5 vertex -4.5 -14 -7.5 vertex -4.5 -14 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -14 -7.5 vertex -4.5 -14 -10.5 vertex -4.5 -15.5 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -14 -7.5 vertex -4.5 -15.5 -10.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 4.5 vertex 4.5 -15.5 1.5 vertex 4.5 -14 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 4.5 vertex 4.5 -14 1.5 vertex 4.5 -14 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 -7.5 vertex 4.5 -15.5 -10.5 vertex 4.5 -14 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 -7.5 vertex 4.5 -14 -10.5 vertex 4.5 -14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -14 1.5 vertex 4.5 -14 -7.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -14 1.5 vertex 4.5 -15.5 -7.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 10.5 vertex 7.5 -15.5 7.5 vertex 7.5 -14 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 10.5 vertex 7.5 -14 7.5 vertex 7.5 -14 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 1.5 vertex 7.5 -15.5 -7.5 vertex 7.5 -14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 1.5 vertex 7.5 -14 -7.5 vertex 7.5 -14 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 7.5 vertex -7.5 15.5 7.5 vertex -7.5 15.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 7.5 vertex -7.5 15.5 10.5 vertex 7.5 15.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 1.5 vertex 4.5 15.5 1.5 vertex 4.5 15.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 1.5 vertex 4.5 15.5 7.5 vertex 7.5 15.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 1.5 vertex -4.5 15.5 1.5 vertex -4.5 15.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 1.5 vertex -4.5 15.5 4.5 vertex 4.5 15.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 15.5 -7.5 vertex -7.5 15.5 -7.5 vertex -7.5 15.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 15.5 -7.5 vertex -7.5 15.5 1.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 -7.5 vertex 4.5 15.5 -7.5 vertex 4.5 15.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 -7.5 vertex 4.5 15.5 -4.5 vertex 7.5 15.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 -10.5 vertex -4.5 15.5 -10.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 -10.5 vertex -4.5 15.5 -7.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 10.5 vertex -7.5 15.5 10.5 vertex -7.5 14 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 10.5 vertex -7.5 14 10.5 vertex 7.5 14 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 7.5 vertex -7.5 14 7.5 vertex -7.5 15.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 7.5 vertex -7.5 15.5 7.5 vertex 4.5 15.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 4.5 vertex -4.5 15.5 4.5 vertex -4.5 14 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 4.5 vertex -4.5 14 4.5 vertex 4.5 14 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 1.5 vertex -4.5 14 1.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 1.5 vertex -4.5 15.5 1.5 vertex 7.5 15.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 15.5 1.5 vertex -7.5 15.5 1.5 vertex -7.5 14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 15.5 1.5 vertex -7.5 14 1.5 vertex -4.5 14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 -4.5 vertex 4.5 15.5 -4.5 vertex 4.5 14 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 -4.5 vertex 4.5 14 -4.5 vertex 7.5 14 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 -7.5 vertex 4.5 14 -7.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 -7.5 vertex 4.5 15.5 -7.5 vertex 7.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 14 -7.5 vertex -7.5 14 -7.5 vertex -7.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 14 -7.5 vertex -7.5 15.5 -7.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 -7.5 vertex -4.5 15.5 -7.5 vertex -4.5 14 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 -7.5 vertex -4.5 14 -7.5 vertex 4.5 14 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 -10.5 vertex -4.5 14 -10.5 vertex -4.5 15.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 -10.5 vertex -4.5 15.5 -10.5 vertex 4.5 15.5 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 10.5 vertex 7.5 14 1.5 vertex 7.5 15.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 10.5 vertex 7.5 15.5 1.5 vertex 7.5 15.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 -4.5 vertex 7.5 14 -7.5 vertex 7.5 15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 -4.5 vertex 7.5 15.5 -7.5 vertex 7.5 15.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 7.5 vertex 4.5 15.5 4.5 vertex 4.5 14 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 7.5 vertex 4.5 14 4.5 vertex 4.5 14 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 -4.5 vertex 4.5 15.5 -7.5 vertex 4.5 14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 -4.5 vertex 4.5 14 -7.5 vertex 4.5 14 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 14 -7.5 vertex 4.5 14 -10.5 vertex 4.5 15.5 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 14 -7.5 vertex 4.5 15.5 -10.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 4.5 vertex -4.5 15.5 1.5 vertex -4.5 14 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 4.5 vertex -4.5 14 1.5 vertex -4.5 14 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 -7.5 vertex -4.5 15.5 -10.5 vertex -4.5 14 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 -7.5 vertex -4.5 14 -10.5 vertex -4.5 14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 14 1.5 vertex -4.5 14 -7.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 14 1.5 vertex -4.5 15.5 -7.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 10.5 vertex -7.5 15.5 7.5 vertex -7.5 14 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 10.5 vertex -7.5 14 7.5 vertex -7.5 14 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 1.5 vertex -7.5 15.5 -7.5 vertex -7.5 14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 1.5 vertex -7.5 14 -7.5 vertex -7.5 14 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 7.5 vertex 15.5 7.5 7.5 vertex 15.5 7.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 7.5 vertex 15.5 7.5 10.5 vertex 15.5 -7.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 1.5 vertex 15.5 -4.5 1.5 vertex 15.5 -4.5 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 1.5 vertex 15.5 -4.5 7.5 vertex 15.5 -7.5 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 1.5 vertex 15.5 4.5 1.5 vertex 15.5 4.5 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 1.5 vertex 15.5 4.5 4.5 vertex 15.5 -4.5 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 7.5 -7.5 vertex 15.5 7.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 7.5 1.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 -7.5 vertex 15.5 -4.5 -7.5 vertex 15.5 -4.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 -7.5 vertex 15.5 -4.5 -4.5 vertex 15.5 -7.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 -10.5 vertex 15.5 4.5 -10.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 -10.5 vertex 15.5 4.5 -7.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 10.5 vertex 15.5 7.5 10.5 vertex 14 7.5 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 10.5 vertex 14 7.5 10.5 vertex 14 -7.5 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 7.5 vertex 14 7.5 7.5 vertex 15.5 7.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 7.5 vertex 15.5 7.5 7.5 vertex 15.5 -4.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 4.5 vertex 15.5 4.5 4.5 vertex 14 4.5 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 4.5 vertex 14 4.5 4.5 vertex 14 -4.5 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 1.5 vertex 14 4.5 1.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 1.5 vertex 15.5 4.5 1.5 vertex 15.5 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 4.5 1.5 vertex 15.5 7.5 1.5 vertex 14 7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 4.5 1.5 vertex 14 7.5 1.5 vertex 14 4.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 -4.5 vertex 15.5 -4.5 -4.5 vertex 14 -4.5 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 -4.5 vertex 14 -4.5 -4.5 vertex 14 -7.5 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 -7.5 vertex 14 -4.5 -7.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 -7.5 vertex 15.5 -4.5 -7.5 vertex 15.5 -7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 4.5 -7.5 vertex 14 7.5 -7.5 vertex 15.5 7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 4.5 -7.5 vertex 15.5 7.5 -7.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 -7.5 vertex 15.5 4.5 -7.5 vertex 14 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 -7.5 vertex 14 4.5 -7.5 vertex 14 -4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 -10.5 vertex 14 4.5 -10.5 vertex 15.5 4.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 -10.5 vertex 15.5 4.5 -10.5 vertex 15.5 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 10.5 vertex 14 -7.5 1.5 vertex 15.5 -7.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 10.5 vertex 15.5 -7.5 1.5 vertex 15.5 -7.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 -4.5 vertex 14 -7.5 -7.5 vertex 15.5 -7.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 -4.5 vertex 15.5 -7.5 -7.5 vertex 15.5 -7.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 7.5 vertex 15.5 -4.5 4.5 vertex 14 -4.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 7.5 vertex 14 -4.5 4.5 vertex 14 -4.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 -4.5 vertex 15.5 -4.5 -7.5 vertex 14 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 -4.5 vertex 14 -4.5 -7.5 vertex 14 -4.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -4.5 -7.5 vertex 14 -4.5 -10.5 vertex 15.5 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -4.5 -7.5 vertex 15.5 -4.5 -10.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 4.5 vertex 15.5 4.5 1.5 vertex 14 4.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 4.5 vertex 14 4.5 1.5 vertex 14 4.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 4.5 -10.5 vertex 14 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 -7.5 vertex 14 4.5 -10.5 vertex 14 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 4.5 1.5 vertex 14 4.5 -7.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 4.5 1.5 vertex 15.5 4.5 -7.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 10.5 vertex 15.5 7.5 7.5 vertex 14 7.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 10.5 vertex 14 7.5 7.5 vertex 14 7.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 1.5 vertex 15.5 7.5 -7.5 vertex 14 7.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 1.5 vertex 14 7.5 -7.5 vertex 14 7.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 7.5 vertex -15.5 -7.5 7.5 vertex -15.5 -7.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 7.5 vertex -15.5 -7.5 10.5 vertex -15.5 7.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 1.5 vertex -15.5 4.5 1.5 vertex -15.5 4.5 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 1.5 vertex -15.5 4.5 7.5 vertex -15.5 7.5 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 1.5 vertex -15.5 -4.5 1.5 vertex -15.5 -4.5 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 1.5 vertex -15.5 -4.5 4.5 vertex -15.5 4.5 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -7.5 -7.5 vertex -15.5 -7.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -7.5 1.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 -7.5 vertex -15.5 4.5 -7.5 vertex -15.5 4.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 -7.5 vertex -15.5 4.5 -4.5 vertex -15.5 7.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 -10.5 vertex -15.5 -4.5 -10.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 -10.5 vertex -15.5 -4.5 -7.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 10.5 vertex -15.5 -7.5 10.5 vertex -14 -7.5 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 10.5 vertex -14 -7.5 10.5 vertex -14 7.5 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 7.5 vertex -14 -7.5 7.5 vertex -15.5 -7.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 7.5 vertex -15.5 -7.5 7.5 vertex -15.5 4.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 4.5 vertex -15.5 -4.5 4.5 vertex -14 -4.5 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 4.5 vertex -14 -4.5 4.5 vertex -14 4.5 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 1.5 vertex -14 -4.5 1.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 1.5 vertex -15.5 -4.5 1.5 vertex -15.5 7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 -4.5 1.5 vertex -15.5 -7.5 1.5 vertex -14 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 -4.5 1.5 vertex -14 -7.5 1.5 vertex -14 -4.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 -4.5 vertex -15.5 4.5 -4.5 vertex -14 4.5 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 -4.5 vertex -14 4.5 -4.5 vertex -14 7.5 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 -7.5 vertex -14 4.5 -7.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 -7.5 vertex -15.5 4.5 -7.5 vertex -15.5 7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 -4.5 -7.5 vertex -14 -7.5 -7.5 vertex -15.5 -7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 -4.5 -7.5 vertex -15.5 -7.5 -7.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 -7.5 vertex -15.5 -4.5 -7.5 vertex -14 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 -7.5 vertex -14 -4.5 -7.5 vertex -14 4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 -10.5 vertex -14 -4.5 -10.5 vertex -15.5 -4.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 -10.5 vertex -15.5 -4.5 -10.5 vertex -15.5 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 10.5 vertex -14 7.5 1.5 vertex -15.5 7.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 10.5 vertex -15.5 7.5 1.5 vertex -15.5 7.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 -4.5 vertex -14 7.5 -7.5 vertex -15.5 7.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 -4.5 vertex -15.5 7.5 -7.5 vertex -15.5 7.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 7.5 vertex -15.5 4.5 4.5 vertex -14 4.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 7.5 vertex -14 4.5 4.5 vertex -14 4.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 -4.5 vertex -15.5 4.5 -7.5 vertex -14 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 -4.5 vertex -14 4.5 -7.5 vertex -14 4.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 4.5 -7.5 vertex -14 4.5 -10.5 vertex -15.5 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 4.5 -7.5 vertex -15.5 4.5 -10.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 4.5 vertex -15.5 -4.5 1.5 vertex -14 -4.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 4.5 vertex -14 -4.5 1.5 vertex -14 -4.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -4.5 -10.5 vertex -14 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 -7.5 vertex -14 -4.5 -10.5 vertex -14 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 -4.5 1.5 vertex -14 -4.5 -7.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 -4.5 1.5 vertex -15.5 -4.5 -7.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 10.5 vertex -15.5 -7.5 7.5 vertex -14 -7.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 10.5 vertex -14 -7.5 7.5 vertex -14 -7.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 1.5 vertex -15.5 -7.5 -7.5 vertex -14 -7.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 1.5 vertex -14 -7.5 -7.5 vertex -14 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 7.5 15.5 vertex 7.5 7.5 15.5 vertex 7.5 10.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 7.5 15.5 vertex 7.5 10.5 15.5 vertex -7.5 10.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 1.5 15.5 vertex -4.5 1.5 15.5 vertex -4.5 7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 1.5 15.5 vertex -4.5 7.5 15.5 vertex -7.5 7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 1.5 15.5 vertex 4.5 1.5 15.5 vertex 4.5 4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 1.5 15.5 vertex 4.5 4.5 15.5 vertex -4.5 4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -7.5 15.5 vertex 7.5 -7.5 15.5 vertex 7.5 1.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -7.5 15.5 vertex 7.5 1.5 15.5 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -7.5 15.5 vertex -4.5 -7.5 15.5 vertex -4.5 -4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -7.5 15.5 vertex -4.5 -4.5 15.5 vertex -7.5 -4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -10.5 15.5 vertex 4.5 -10.5 15.5 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -10.5 15.5 vertex 4.5 -7.5 15.5 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 10.5 15.5 vertex 7.5 10.5 15.5 vertex 7.5 10.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 10.5 15.5 vertex 7.5 10.5 14 vertex -7.5 10.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 14 vertex 7.5 7.5 14 vertex 7.5 7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 14 vertex 7.5 7.5 15.5 vertex -4.5 7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 4.5 15.5 vertex 4.5 4.5 15.5 vertex 4.5 4.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 4.5 15.5 vertex 4.5 4.5 14 vertex -4.5 4.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 1.5 14 vertex 4.5 1.5 14 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 1.5 14 vertex 4.5 1.5 15.5 vertex -7.5 1.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 1.5 15.5 vertex 7.5 1.5 15.5 vertex 7.5 1.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 1.5 15.5 vertex 7.5 1.5 14 vertex 4.5 1.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -4.5 15.5 vertex -4.5 -4.5 15.5 vertex -4.5 -4.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -4.5 15.5 vertex -4.5 -4.5 14 vertex -7.5 -4.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -7.5 14 vertex -4.5 -7.5 14 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -7.5 14 vertex -4.5 -7.5 15.5 vertex -7.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -7.5 14 vertex 7.5 -7.5 14 vertex 7.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -7.5 14 vertex 7.5 -7.5 15.5 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 15.5 vertex 4.5 -7.5 15.5 vertex 4.5 -7.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 15.5 vertex 4.5 -7.5 14 vertex -4.5 -7.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -10.5 14 vertex 4.5 -10.5 14 vertex 4.5 -10.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -10.5 14 vertex 4.5 -10.5 15.5 vertex -4.5 -10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 10.5 14 vertex -7.5 1.5 14 vertex -7.5 1.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 10.5 14 vertex -7.5 1.5 15.5 vertex -7.5 10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -4.5 14 vertex -7.5 -7.5 14 vertex -7.5 -7.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -4.5 14 vertex -7.5 -7.5 15.5 vertex -7.5 -4.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 7.5 15.5 vertex -4.5 4.5 15.5 vertex -4.5 4.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 7.5 15.5 vertex -4.5 4.5 14 vertex -4.5 7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -4.5 15.5 vertex -4.5 -7.5 15.5 vertex -4.5 -7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -4.5 15.5 vertex -4.5 -7.5 14 vertex -4.5 -4.5 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -7.5 14 vertex -4.5 -10.5 14 vertex -4.5 -10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -7.5 14 vertex -4.5 -10.5 15.5 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 4.5 15.5 vertex 4.5 1.5 15.5 vertex 4.5 1.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 4.5 15.5 vertex 4.5 1.5 14 vertex 4.5 4.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -7.5 15.5 vertex 4.5 -10.5 15.5 vertex 4.5 -10.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -7.5 15.5 vertex 4.5 -10.5 14 vertex 4.5 -7.5 14 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 1.5 14 vertex 4.5 -7.5 14 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 1.5 14 vertex 4.5 -7.5 15.5 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 10.5 15.5 vertex 7.5 7.5 15.5 vertex 7.5 7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 10.5 15.5 vertex 7.5 7.5 14 vertex 7.5 10.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 1.5 15.5 vertex 7.5 -7.5 15.5 vertex 7.5 -7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 1.5 15.5 vertex 7.5 -7.5 14 vertex 7.5 1.5 14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -7.5 -15.5 vertex 7.5 -7.5 -15.5 vertex 7.5 -10.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -7.5 -15.5 vertex 7.5 -10.5 -15.5 vertex -7.5 -10.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -1.5 -15.5 vertex -4.5 -1.5 -15.5 vertex -4.5 -7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -1.5 -15.5 vertex -4.5 -7.5 -15.5 vertex -7.5 -7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -1.5 -15.5 vertex 4.5 -1.5 -15.5 vertex 4.5 -4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -1.5 -15.5 vertex 4.5 -4.5 -15.5 vertex -4.5 -4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 7.5 -15.5 vertex 7.5 7.5 -15.5 vertex 7.5 -1.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 7.5 -15.5 vertex 7.5 -1.5 -15.5 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 7.5 -15.5 vertex -4.5 7.5 -15.5 vertex -4.5 4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 7.5 -15.5 vertex -4.5 4.5 -15.5 vertex -7.5 4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 10.5 -15.5 vertex 4.5 10.5 -15.5 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 10.5 -15.5 vertex 4.5 7.5 -15.5 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -10.5 -15.5 vertex 7.5 -10.5 -15.5 vertex 7.5 -10.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -10.5 -15.5 vertex 7.5 -10.5 -14 vertex -7.5 -10.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 -14 vertex 7.5 -7.5 -14 vertex 7.5 -7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 -14 vertex 7.5 -7.5 -15.5 vertex -4.5 -7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -4.5 -15.5 vertex 4.5 -4.5 -15.5 vertex 4.5 -4.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -4.5 -15.5 vertex 4.5 -4.5 -14 vertex -4.5 -4.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -1.5 -14 vertex 4.5 -1.5 -14 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -1.5 -14 vertex 4.5 -1.5 -15.5 vertex -7.5 -1.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -1.5 -15.5 vertex 7.5 -1.5 -15.5 vertex 7.5 -1.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -1.5 -15.5 vertex 7.5 -1.5 -14 vertex 4.5 -1.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 4.5 -15.5 vertex -4.5 4.5 -15.5 vertex -4.5 4.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 4.5 -15.5 vertex -4.5 4.5 -14 vertex -7.5 4.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 7.5 -14 vertex -4.5 7.5 -14 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 7.5 -14 vertex -4.5 7.5 -15.5 vertex -7.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 7.5 -14 vertex 7.5 7.5 -14 vertex 7.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 7.5 -14 vertex 7.5 7.5 -15.5 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 -15.5 vertex 4.5 7.5 -15.5 vertex 4.5 7.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 -15.5 vertex 4.5 7.5 -14 vertex -4.5 7.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 10.5 -14 vertex 4.5 10.5 -14 vertex 4.5 10.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 10.5 -14 vertex 4.5 10.5 -15.5 vertex -4.5 10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -10.5 -14 vertex -7.5 -1.5 -14 vertex -7.5 -1.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -10.5 -14 vertex -7.5 -1.5 -15.5 vertex -7.5 -10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 4.5 -14 vertex -7.5 7.5 -14 vertex -7.5 7.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 4.5 -14 vertex -7.5 7.5 -15.5 vertex -7.5 4.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -7.5 -15.5 vertex -4.5 -4.5 -15.5 vertex -4.5 -4.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -7.5 -15.5 vertex -4.5 -4.5 -14 vertex -4.5 -7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 4.5 -15.5 vertex -4.5 7.5 -15.5 vertex -4.5 7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 4.5 -15.5 vertex -4.5 7.5 -14 vertex -4.5 4.5 -14 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 7.5 -14 vertex -4.5 10.5 -14 vertex -4.5 10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 7.5 -14 vertex -4.5 10.5 -15.5 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -4.5 -15.5 vertex 4.5 -1.5 -15.5 vertex 4.5 -1.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -4.5 -15.5 vertex 4.5 -1.5 -14 vertex 4.5 -4.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 7.5 -15.5 vertex 4.5 10.5 -15.5 vertex 4.5 10.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 7.5 -15.5 vertex 4.5 10.5 -14 vertex 4.5 7.5 -14 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -1.5 -14 vertex 4.5 7.5 -14 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -1.5 -14 vertex 4.5 7.5 -15.5 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -10.5 -15.5 vertex 7.5 -7.5 -15.5 vertex 7.5 -7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -10.5 -15.5 vertex 7.5 -7.5 -14 vertex 7.5 -10.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -1.5 -15.5 vertex 7.5 7.5 -15.5 vertex 7.5 7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -1.5 -15.5 vertex 7.5 7.5 -14 vertex 7.5 -1.5 -14 endloop endfacet
endsolid letters
```

```geojson
{
  "type": "FeatureCollection",
  "features": [
    { "type": "Feature", "properties": { "name": "東京" }, "geometry": { "type": "Point", "coordinates": [139.767, 35.681] } },
    { "type": "Feature", "properties": { "name": "大阪", "marker-color": "#0969DA" }, "geometry": { "type": "Point", "coordinates": [135.502, 34.693] } },
    { "type": "Feature", "properties": { "name": "東海道" }, "geometry": { "type": "LineString", "coordinates": [[139.767, 35.681], [138.383, 34.976], [136.906, 35.170], [135.502, 34.693]] } },
    { "type": "Feature", "properties": { "name": "富士山", "fill": "#B92613" }, "geometry": { "type": "Polygon", "coordinates": [[[138.7274, 35.4731], [138.6080, 35.3043], [138.8468, 35.3043], [138.7274, 35.4731]]] } }
  ]
}
```

[立方体の「5」が回る動画](samples/import-test-images/cube5.mp4)

### 対応している図・チャート

コードブロックの言語名によって、表示のしかたが次のように変わります。

```mermaid
flowchart LR
    src["コードブロック・数式"] --> kind{"記法"}
    kind -- "mermaid" --> mermaid["図として表示<br/>Mermaid 11.17.2"]
    kind -- "数式 (TeX)" --> math["数式として表示<br/>KaTeX 0.18.9"]
    kind -- "dot・graphviz" --> graphviz["図として表示<br/>Graphviz 16.1.0"]
    kind -- "plantuml・nomnoml など" --> code["コードブロックのまま表示"]

    classDef supported fill:#d4f7d4,stroke:#2a7a2a,color:#1a1a1a;
    classDef unsupported fill:#eeeeee,stroke:#777777,color:#1a1a1a;
    class mermaid,math,graphviz supported;
    class code unsupported;
```

| 種類 | 記法 | 表示 | 補足 |
| :--- | :--- | :---: | :--- |
| フローチャート | ` ```mermaid ` + `flowchart` / `graph` | 対応 | |
| シーケンス図 | `sequenceDiagram` | 対応 | |
| クラス図 | `classDiagram` | 対応 | |
| 状態遷移図 | `stateDiagram-v2` | 対応 | |
| ER図 | `erDiagram` | 対応 | |
| ガントチャート | `gantt` | 対応 | |
| 円グラフ | `pie` | 対応 | |
| 棒グラフ・折れ線グラフ | `xychart-beta` | 対応 | 軸の日本語ラベルは `"1月"` のように `"..."` で囲みます |
| 4象限チャート | `quadrantChart` | 対応 | |
| ユーザージャーニー | `journey` | 対応 | |
| Git グラフ | `gitGraph` | 対応 | |
| マインドマップ | `mindmap` | 対応 | |
| タイムライン | `timeline` | 対応 | |
| サンキーダイアグラム | `sankey-beta` | 対応 | ノード名は英数字のみ（Mermaid の制限で、日本語は使えません） |
| 数式 | `$...$`（インライン）、`$$...$$`（ブロック） | 対応 | KaTeX で表示 |
| Graphviz | ` ```dot `、` ```graphviz `、` ```gv ` | 対応 | アプリに同梱した Graphviz（WebAssembly 版）で、オフラインで描きます。図の中のリンクと画像は使えません（安全のため取り除きます）。日本語のラベルが枠からはみ出すときは、文字を小さくして枠に収めます |
| 3D モデル | ` ```stl `（文字の STL） | 対応 | 同梱した three.js で、オフラインで表示します。ドラッグで回転、ホイールで拡大・縮小。PDF・PNG・HTML・Word には、その時点の見た目が画像として入ります |
| 地図 | ` ```geojson `、` ```topojson ` | 対応 | 点・線・面を、まず同梱した簡単な世界地図（Natural Earth）の上にオフラインで描きます。初期設定では、お知らせ欄で「詳細な地図を読み込む」かを確認し、押すとその文書だけ、道路や地名の入った OpenStreetMap の地図を読み込みます（地図のサーバーに表示している場所と IP アドレスが伝わります。保存した HTML には入れません）。「設定」→「インターネット」→「詳細な地図（OpenStreetMap）」で「いつも読み込む」「読み込まない」も選べます。GitHub と同じく、`marker-color`・`stroke`・`fill` などの色の指定が使えます |
| PlantUML | ` ```plantuml ` | 非対応 | コードブロックとして表示。描くには Java と PlantUML を PC に入れるか、文書を外部の PlantUML サーバーに送る必要があり、文書を PC の外に出さない方針に合わないため対応していません |
| nomnoml | ` ```nomnoml ` | 非対応 | コードブロックとして表示 |

上の表で「対応」の図は、テスト文書で表示を確認済みです。これ以外の Mermaid の図（`block-beta` など）も Mermaid 11.17.2 が対応していれば表示されますが、確認はしていません。

[samples/diagram-test.md](./samples/diagram-test.md) と [samples/diagram-test.en.md](./samples/diagram-test.en.md) の 2 つは、mdEditor での表示を前提にした文書で、GitHub での表示との互換の決まり（Identity 8-9 (7)）を当てはめません。mdEditor が描ける図と描けない図をそのまま示して確かめるためのサンプルなので、GitHub では描けない書き方（Graphviz、PlantUML）も含み、表示は mdEditor で確かめています（Identity 8-9 (8)、1-4 (1)）。この前提は、それぞれの文書の冒頭にも書いています。この README のそれ以外の説明、CHANGELOG.md、PRIVACY.md、ほかのサンプルは、GitHub で表示できる書き方だけを使います。

## アクセシビリティの確認記録

Identity 第7条 7-7 の出荷前チェックの記録です。

- 確認日: 2026-10-11
- 使ったツール: Microsoft Edge（ヘッドレス）で画面の大きさを指定した iframe に表示して計測、mdEditor 本体（WebView2）を UI オートメーションとキー入力で操作、スクリーンショットのグレースケール変換
- 確認した組み合わせ: ライト・ダーク × 日本語・英語 × 1280×800、1024×768、640×400（1280×800 の 200% 拡大に相当）、320×640

| 項目 | 結果 | 内容 |
| :--- | :--- | :--- |
| (a) 拡大200%で横スクロールが出ない | OK | 横にはみ出す要素なし。高さが足りないときは画面全体が縦にスクロールし、編集欄とプレビューの高さを保つ（この確認で修正） |
| (b) マウスなしで全操作できる | OK | Tab で全ボタン・編集欄・区切り・プレビューに移動できる。ダイアログは Enter で開き、矢印キーで選び、Esc で閉じて元の位置に戻る（プレビューに移動できなかった点をこの確認で修正） |
| (c) グレースケールでも意味が取れる | OK | 状態は文字（「未保存の変更」「（推定）」）、お知らせとエラーは記号と文言、選択は形で示す |
| (d) 画面幅320pxで縦スクロールだけで読める | OK | ファイル名の省略と、編集欄の見出しのボタンのはみ出しをこの確認で修正 |
| (e) 夜の配色で (a)〜(d) | OK | ダークでも同じ結果 |
| (f) コントラストテーマ（強制カラー） | OK | Edge の強制カラー（`--force-high-contrast`）で、ボタンの枠・区切り・フォーカスの印・本文が読める。編集欄の選んだ範囲と読み上げ中の枠はシステムの強調色（Highlight）、ロゴと図は元の色で表示する |
| (g) 画面読み上げソフト | OK | Windows ナレーターで、すべてのボタン・編集欄・区切りに名前と役割がある。チェックリストのチェックボックスに項目名を付けた。ナレーターをオンにして長い文書（約260行）の文字を選ぶと、以前の編集欄（textarea）では入力が約20秒止まっていたため、編集欄を CodeMirror 6 に置き換えた。置き換え後は約1秒で反応する（この確認で修正） |
| (h) 文字の大きさの設定200% | 要確認 | 本アプリの「文字の大きさ」の最大（200%）・行間「とても広い」では、文字が欠けたり重なったりせず、横にもはみ出さない（1280×800、320×640）。Windows の「テキストのサイズ」だけを200%にした確認は、まだ行っていない |
| (i) 自動バックアップからの復元 | OK | 編集後に強制終了し、次の起動で復元の確認が出て、ファイル名・文字コード・本文・「未保存の変更」が戻る。正常に終了すると控えが消える |
| (j) 外部のファイルの読み込み | 要確認 | 「確認してから読み込む」では、確認の前は読み込まず、お知らせ欄に件数と「画像を読み込む」が出る。「いつも読み込む」では https の公開アドレスの画像だけを読み込み、localhost・192.168.x.x・http:// の画像は読み込まない。「画像を読み込む」を押したあとと「読み込まない」の確認は、まだ行っていない |

そのほか、文章の読み上げで PC の音声（Microsoft Ayumi など）が使えること、速さと段落の間を選べること、読んでいる段落を画面でも示すこと（Identity 7-3 (3)、7-4）、設定画面のボタン・選択欄・チェックボックスの高さが44px以上であること（7-3 (1)）を確認しました。

## デザインと公式素材

画面デザインは5garashi.com Design Office Identity v4.20.0（2026-10-17）に基づきます。ツールバーの「表示」ボタンで、表示テーマを「自動」（Windows の「アプリ モード」に合わせる）→「ライト」→「ダーク」の順に切り替えられます。選んだテーマは次回の起動でも使います。ダークでは第4条 4-2 の夜の配色（ダークモード）で表示し、ロゴタイプは第3条 3-7 の灰色抜きの反転版に切り替わります（右上のシンボルマークは、3-7 (5) によりダークでも赤のままです）。編集欄・プレビュー・図もダーク表示になり、ウィンドウのタイトルバーも合わせます。印刷とPDFは常に昼の配色です。公式ロゴタイプ（日本語版・英語版と、それぞれのダークモード用の反転版。表示言語に合わせて切り替えます）とシンプル版マークは [5garashi/5garashi-identity](https://github.com/5garashi/5garashi-identity) のSVG素材を改変せずに同梱し、アプリのウィンドウ・実行ファイル・Markdown関連付けのアイコンにはシンプル版マークを使用します。各SVGファイルに記載された作者、著作権、CC BY-ND 4.0 ライセンスを保持しています。samples/diagram-test の「22. 画像の大きさと位置」の例にも、同じ公式アセット（シンボルマーク、ロゴタイプとその反転版、紹介画像 og_image.png。CC BY-ND 4.0）を改変せずに使っています（samples/import-test-images）。

## ライセンス

ソースコードは [MIT License](./LICENSE) です（Identity 第12-1条）。`wwwroot/assets/` のロゴタイプとシンボルマークは MIT の対象外で、CC BY-ND 4.0 で提供されます。名称とロゴに関する権利は留保され、公式の提供物・承認・提携と誤認される使い方はできません（第12-2条）。

`wwwroot/lib/` には、それぞれの MIT License のもとで Mermaid 11.17.2、KaTeX 0.18.9、CodeMirror 6（編集欄。`tools/codemirror` でビルド）を、BSD 3-Clause License のもとで highlight.js 11.12.0（コードの色分け）を、MIT License のもとで three.js 0.186.1（3D モデル）を、ISC License のもとで d3-geo 3.1.1・topojson-client 3.1.0・world-atlas 2.0.2（地図。世界地図のデータは Natural Earth のパブリックドメイン）を、MIT License のもとで Viz.js 3.31.0（Graphviz 16.1.0 の WebAssembly 版。Graphviz は Eclipse Public License 1.0）を、改変せずに同梱しています。同梱しているすべての第三者ソフトウェア（Markdig、HtmlSanitizer、WebView2 SDK、.NET ランタイム、Mermaid に含まれるライブラリなど）とライセンス本文は [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md) にあります。

## プライバシー

本アプリは文書の内容や個人情報を収集・送信しません。自動バックアップの控えも PC の中だけに置きます。外部の画像を含む文書では、その画像のサーバーへ直接接続します。詳しくは [PRIVACY.md](./PRIVACY.md) をご覧ください。

## English

[日本語](#特長) / English

### Highlights

An editor that shows the formatted result right beside the Markdown you are writing, diagrams and math included, and exports it to PDF. Writing, reading, and keeping your work all happen on your PC.

#### Write, view, and export

- **Editor and viewer in one**: what you write on the left appears at once in the preview on the right. Both sides scroll together, and stay in step when you move with find/replace or heading links. Expand either side to fill the window to use it as just an editor or just a viewer.
- **Write on one display, read on another**: open the preview in its own window and maximize it, or make it full screen, on a second display. The editor and the preview still scroll together, matched line by line. On a wide screen the text keeps a readable width while tables and diagrams spread out. The window's place is remembered for next time.
- **Diagrams, charts, math, and tables as they look**: Mermaid diagrams such as flowcharts, sequence diagrams, Gantt charts, and pie charts, KaTeX math, tables, task lists, and images in the document appear in the preview.
- **Export to PDF and HTML**: save the preview as a PDF exactly as it looks, diagrams and math included, on A4 or Letter paper (chosen automatically by region at first). Or save it as one self-contained HTML file to email or put on the web.
- **Open and save many formats**: Word, HTML, web pages, e-mail, CSV, JSON, Jupyter Notebooks, and images open as Markdown, and Save As also writes PNG, Word, CSV, JSON, Jupyter Notebook, a ZIP for sharing, and more.
- **Syntax help at hand**: F1 lists the Markdown syntax and inserts examples into the editor.
- **No trouble with text encodings**: besides UTF-8, older files in Shift_JIS, EUC-JP, and more are detected and opened, with a notice when the encoding was guessed or some characters could not be read. You can also choose the encoding to save in.
- **Your work is protected**: saving never leaves the original file broken, even if writing fails midway. Unsaved changes are backed up automatically and can be restored at the next launch after a crash.
- **Dark mode**: follows your Windows setting with an easy-on-the-eyes night palette. Printing and PDFs always use a white page.
- **Privacy**: your documents and usage are never sent anywhere. The interface is available in Japanese and English.

#### Easy for everyone (Universal Design)

- **Readability your way**: choose the legible BIZ UD Gothic typeface, larger text (up to 200%) and line spacing, whole-window zoom, a highlighted current line, and a thick cursor. Windows contrast themes are supported. Each aid is opt-in, so the standard look stays as it is for everyone else.
- **Everything from the keyboard**: every button has a shortcut you can change or remove. Tab and F6 reach every pane, and buttons are large enough to hit easily (at least 44 px).
- **Works with screen readers**: tested with Windows Narrator; the editor was rebuilt so long documents never freeze. Buttons expose their names and shortcuts to assistive technology.
- **Read aloud**: the preview is read paragraph by paragraph, with the current paragraph shown on screen. Choose the speed and the pause between paragraphs. Only voices on your PC are used, so your text stays with you.

### Screenshots

**See the result as you write** — Write Markdown on the left and the preview on the right updates as you type, with diagrams, tables, and math.

![Write Markdown on the left and the preview on the right updates as you type, with diagrams, tables, and math.](docs/images/1-main.en.png)

**Dark mode** — Follows the Windows setting, or choose day or night with the Theme button. Printing and PDF always use a white page.

![Follows the Windows setting, or choose day or night with the Theme button. Printing and PDF always use a white page.](docs/images/2-dark.en.png)

**The preview in its own window** — Put the preview on a second display to keep writing and reading apart. It scrolls along with your editing.

![Put the preview on a second display to keep writing and reading apart. It scrolls along with your editing.](docs/images/3-preview-window.en.png)

**Markdown at a glance** — The Markdown quick reference (F1). Insert puts the example at the cursor.

![The Markdown quick reference (F1). Insert puts the example at the cursor.](docs/images/4-syntax.en.png)

**Save in many formats** — Save As writes Markdown, text, PDF, HTML, PNG, Word, CSV, TSV, JSON, Jupyter Notebook, or a ZIP for sharing (the document and its name_files folder). Word, HTML, CSV, e-mail, Jupyter Notebook, and more open too.

![Save As writes Markdown, text, PDF, HTML, PNG, Word, CSV, TSV, JSON, Jupyter Notebook, or a ZIP for sharing (the document and its name_files folder). Word, HTML, CSV, e-mail, Jupyter Notebook, and more open too.](docs/images/5-save-as.en.png)

**Colored code and contents** — Code with a language name is shown in color. Contents builds a table of contents from your headings.

![Code with a language name is shown in color. Contents builds a table of contents from your headings.](docs/images/6-code-toc.en.png)

**3D models** — A 3D model written in STL turns as you drag it. It shows without an internet connection.

![A 3D model written in STL turns as you drag it. It shows without an internet connection.](docs/images/7-3d-model.en.png)

**Maps and front matter** — A GeoJSON map is drawn on a detailed OpenStreetMap map once you allow it. YAML front matter at the top becomes a list of keys and values.

![A GeoJSON map is drawn on a detailed OpenStreetMap map once you allow it. YAML front matter at the top becomes a list of keys and values.](docs/images/8-map.en.png)

### Setup

1. On Windows with the .NET 10 SDK installed, double-click `setup.cmd` (or run `powershell -ExecutionPolicy Bypass -File .\setup.ps1` from PowerShell). Double-clicking a `.ps1` file only opens it in Notepad, so `setup.cmd` runs it for you. The app is published to `%LOCALAPPDATA%\Programs\mdEditor` and registered for the current user. Besides `.md` and `.markdown`, `.html` and `.htm` list mdEditor in Open with and Default apps (browser associations are not changed). The Microsoft Store version does not associate `.html` (Open and drag and drop still open it).
2. Open Windows Settings, Apps, Default apps, and choose `mdEditor` as the default app for `.md`.
3. When you open a `.md` file, mdEditor shows the text on the left and the preview on the right.

Windows requires the user to set default apps, so choose `mdEditor` for `.md` in the Settings page that opens after setup. An app cannot change the default without the user's confirmation.

The other types mdEditor opens (.txt, .html, .htm, .docx, .eml, .csv, .tsv, .json, .xml, .ipynb, and images) list mdEditor under Open with on the right-click menu (the Microsoft Store version does the same). Their default apps (such as the browser or Notepad) do not change. In the setup.cmd install mdEditor is only listed as a choice and never used as the default. In the Microsoft Store version, Windows opens a type with mdEditor on double-click only when no other app on the PC can open it (such as .ipynb or .json); choosing another app as the default takes over.

Microsoft Edge WebView2 Runtime is required. It is normally available wherever Microsoft Edge is installed.

**Package for the Microsoft Store**: `setup.cmd` is the development procedure. For the Store, run `store\build-msix.ps1` on a PC with the Windows SDK, passing the values from Partner Center's Product identity as `-IdentityName` and `-Publisher`. It creates an unsigned MSIX in `artifacts`; the Store signs it.

### Versioning

The version lives only in the [VERSION](./VERSION) file at the repository root. The app (shown in About) and the Microsoft Store MSIX (four parts, with `.0` added) read it from there. To release a new version, change these three in the same pull request:

1. Raise the number in `VERSION` (for example, `1.0.3` to `1.0.4`).
2. Add a `## 1.0.4` section to [CHANGELOG.md](./CHANGELOG.md).
3. Update the "バージョン / Version" line near the top of this README.

On pull requests, GitHub Actions checks that the three match. When merged into main, the `v1.0.4` tag and a GitHub Release (with the CHANGELOG section as its notes) are created automatically.

### Usage

- **Samples**: puts a short sample ("Diagrams and math" or "Basic writing") or the "Diagram and chart test" of every supported diagram, chart, and formula (Japanese or English; the two files in [samples/](./samples/)) as a new document, only when you choose it (if the open document has changes, you are asked to save them first; the open file is never written).
- **About**: shows links to the company, works, support, and the privacy policy. Links open in your browser only when you choose them.
- **Open**: choose a Markdown file. Text (.txt), Word (.docx), and HTML (.html, .htm) files open too. Text opens and saves back as it is. A Word document opens as Markdown: headings, lists, tables, links, and images come back (colors and layout do not); its images are placed beside the Markdown file in a "name_files" folder on the first save, and the Word document is never written. HTML files (.html, .htm) open too: they are converted to Markdown, and headings, paragraphs, lists, tables, links, images, and math come back (colors, layout, and drawn diagrams do not). Saving makes a Markdown file and never writes the HTML file.
  - **E-mail (.eml)**: opens as Markdown with the subject as the heading, then the sender, recipients, date, attachment names, and the body. Pictures in the body are handled like a Word document's. Internet images in the mail (sometimes used to track opening) do not load until you allow them. Attachments are not extracted.
  - **CSV, TSV (.csv, .tsv)**: opens as a Markdown table with the first row as the header. Shift_JIS CSV files saved by Excel open too.
  - **JSON (.json)**: a list (an array of objects) opens as a table with the field names as the header row. Other JSON opens in a code block, so you can add notes before and after it.
  - **XML (.xml)**: opens in a code block, with room for notes before and after it.
  - **Jupyter Notebook (.ipynb)**: Markdown cells stay as they are and code cells become code blocks with their language (```python and so on). Text outputs become "output" blocks; pictures and tables (such as pandas) show as they are, and so do pictures pasted into cells.
  - **Images (.png, .jpg, .gif, .bmp, .webp)**: open as a new document that shows the image. Write a description below it and save it as Markdown; saving in another folder copies the image there too.
  - **Videos (.mp4, .m4v, .webm, .ogv, .mov)**: open as a new document that shows the video, the same way as images.
- **Drag and drop**: files dropped on the editor go in at the cursor, and files dropped anywhere else open. While you drag, a label beside the pointer says "Insert at the cursor" or "Open as a file".

  | File | Dropped on the editor | Dropped anywhere else |
  | :--- | :--- | :--- |
  | Images and videos | Copied to a "name_files" folder beside the document, with `![](name_files/image.png)` inserted | Opens as an image or video document |
  | Files Open accepts (.md, .txt, .html, .docx, .eml, .csv, .json, .ipynb, and so on) | Their content goes in, converted to Markdown as Open does | Opens (unsaved changes are asked about first) |
  | Files mdEditor cannot open (.pdf, .pptx, .zip, and so on) | Copied to the "name_files" folder, with a link such as `[report.pdf](name_files/report.pdf)` inserted | Not opened; a message says that dropping it on the editor inserts a link |

  - When content goes in, the notice bar says so. If you meant to open the file, press Ctrl+Z and drop it outside the editor instead.
  - Images and videos the content shows, and files it links to, are copied to the "name_files" folder too. Front matter at the top of Markdown (the part between `---` lines) is left out.
  - Every file outside the "name_files" folder, including files in the document's own folder, is copied into the "name_files" folder. Files already inside it are used where they are.
  - The notice bar says what was copied ("Created the A_files folder and saved "B.pdf" in it." and so on). When a different file in the folder already has the name, a number is added as browsers do for downloads ("B (1).pdf", "B (2).pdf"). A file with exactly the same content is used instead of a new copy.
  - A document that has not been saved asks where to save first when files have to be copied.
  - A picture pasted into the editor (Ctrl+V), such as a screen capture, also goes in the "name_files" folder.
  - **When you give someone the Markdown file, give them the "name_files" folder too.** The document points to the images, videos, and files you inserted by paths relative to its own location (`name_files/...`). With the .md file alone, they do not show and the links do not open on the other PC. Keep the .md file and the folder side by side and send them together (for example, in one zip file). Links outside the "name_files" folder (such as `../docs/...` you wrote yourself) open only if that file goes along too. Files saved as PDF, HTML, PNG, or Word include the images, but not videos or files inserted as links.
- **Open URL / Ctrl+Shift+K**: opens a web page that starts with `https://` as Markdown. You can also drag a link in from a browser. The page's scripts never run, so content it loads afterwards (such as live rankings) is not included.
- **Images and videos on the internet**: not loaded by default; Load images and videos in the notice bar loads them for that document. Settings > Internet can also be Always load or Never load. Images over `http://` are never loaded. Images and links not found beside the file resolve from the page's own address (`<base>`, canonical, or og:url).
- **Save**: overwrite the current file. A new document asks where to save.
- **Save As**: choose a location and file name. As with Open, the file type chooses the format.
  - Saving as Markdown (or text as written) to another folder or under another name copies the whole "name_files" folder too. Under a new name, the folder is copied under the new name too ("C_files" for "C.md"), and the links in the document are rewritten, both in the saved file and in the editor (Ctrl+Z undoes it). The original document and folder are not changed.
  - While it copies, the notice bar shows how far it has got (which file of how many), then says "Copied A.md and the A_files folder to D:\Docs.", "Copied as C.md and the C_files folder.", or "Copied as C.md and the C_files folder to D:\Docs.".
  - If a folder with that name is already there, files are added to it instead of written over: a different file with the same name gets a number such as (1), and its links follow. Identical files are shared.
  - In another folder, relative links outside the "name_files" folder (such as `../docs/x.pdf` you wrote yourself) are changed to point to the original files.
  - Saving as PDF, HTML, PNG, Word, and the other export formats copies no folder, because the images go inside the file.
  - **Markdown (.md), text as written (.txt)**: save the document itself (the content as it is). Save then writes to this file.
  - **Text without marks, PDF, HTML, PNG, Word (.docx), CSV, TSV, JSON, Jupyter Notebook, ZIP for sharing**: export the document; the file you are editing stays as it was. Each export is described below.
- **Text without marks**: only the words the preview shows, as a .txt file. Heading and emphasis marks are removed and lines break where you wrote them. Lists use a bullet or number, tables are tab-separated, links become "text (address)", and diagrams become "[Figure]".
- **CSV, TSV**: the tables in the document as CSV (comma-separated) or TSV (tab-separated). Two or more tables are saved as name-1.csv, name-2.csv, and so on. Files are UTF-8 with a BOM so Excel shows Japanese correctly.
- **JSON**: the tables in the document as arrays of objects keyed by the header row (UTF-8). Values are written as text, so numbers such as 007 keep their zeros. Two or more tables are saved as name-1.json and so on.
- **Jupyter Notebook**: code blocks with a language become code cells and everything else Markdown cells. Outputs are not included (Jupyter makes them again when run). A document opened from an .ipynb file saves back to the same cells.
- **ZIP for sharing**: a ZIP to hand to someone, holding the document (.md) and its "name_files" folder under the names the links use, so it shows as it is once extracted. The document goes in as the editor has it (unsaved changes included). A document without a "name_files" folder (or not saved yet) goes in alone.
  - When the "name_files" folder has files the document does not refer to (in no link, picture, or HTML src or href), they are listed and you choose: Yes moves them to the Recycle Bin, then makes the ZIP; No keeps them where they are. Either way they stay out of the ZIP.
  - Links to files outside the "name_files" folder are not put in the ZIP; the message says how many there are.
- **Ctrl+O**: open a file.
- **Back to previous document / Ctrl+Shift+B**: goes back to the document that was open before another one took its place. The button beside Open shows only while there is a document to go back to. Use it when, for example, you dropped a file outside the editor and it opened another document, perhaps after pressing "No" when you meant "Cancel".
  - A document closed with "No" to "Save changes?" comes back as it was when you closed it (the unsaved text, its file, and its encoding), still unsaved. Saving it writes over its file as usual.
  - A document closed without changes is opened again from its file. If the file was moved or deleted, a message says so.
  - Only the last document closed can come back. That includes one closed in another window, or with "No" when closing the app (also after the next start). When no other document took its place, as when you cancel the file dialog of Open, the previous document stays as it was.
  - If the current document has changes, you are asked to save them first. Choosing "No" there swaps the two, so pressing the button again returns to the current one.
  - The copy is kept where automatic backup keeps its copies. For a document without changes, only its file location is kept. With automatic backup turned off in Settings, no copy is kept and the button does not show.
- **Ctrl+S**: save.
- **Ctrl+Shift+S**: save as.
- **PDF**: export the rendered preview as a PDF.
- **PNG / Ctrl+Shift+G**: the preview as pictures (PNG) in the day palette; a long document is split into name-1.png, name-2.png, and so on.
- **Word / Ctrl+Shift+D**: export a Word document (.docx). Headings, paragraphs, lists, quotes, code, tables, links, and images use Word's own styles, so Word's navigation pane and table of contents work. Diagrams and math go in as pictures from the preview (not editable in Word).
- **HTML / Ctrl+Shift+M**: export the rendered preview as one self-contained HTML file. Diagrams are SVG, math is MathML, and images beside the document are embedded. In a browser it switches to the night palette in dark mode. Opening it makes no connections, except that web images in the document load from their servers.
- **Paper**: cycle the PDF paper through Auto, A4, and Letter. Auto follows the Windows region (Letter where US customary units are used, A4 elsewhere). The choice is remembered.
- **Encoding**: the document's encoding is shown next to the file name. Press it to choose an encoding and either reopen the file in it or use it from the next save.
- **Ctrl+Shift+P**: save as PDF.
- **Contents / Ctrl+Shift+J**: build a table of contents (links to the headings) and insert it at the cursor. It goes between `<!-- toc -->` and `<!-- /toc -->` (not shown in the preview); after changing headings, press it again to update that table in place. The document title (a single `#` heading at the top) is left out, and three levels are kept. The links go to the headings in the preview, PDF, HTML, and Word.
- **Adjust image / Ctrl+Shift+F**: choose an image's size and position, and have it rewritten the way GitHub does it (HTML `<img>`). Put the cursor on the image's line in the editor and press it, or right-click the image in the preview.
  - Size: original size, 25%, 50%, 75%, or 100% of the text width, or a width in pixels (`width="50%"`, `width="300"`). The height keeps the picture's proportions.
  - Position: not set (left), center (`<p align="center">`), right (`<p align="right">`), or on the left or right with text flowing around it (`align="left"`, `align="right"`). Center and Right need the image on a line of its own. If text follows on the next line, a blank line goes in between, so the HTML block does not swallow it.
  - Choosing Original size and Not set turns the image back into Markdown, `![description](file)`. Ctrl+Z undoes the change.
  - PDF, PNG, and HTML follow the preview. Word gets the size and the center, right, or left placement (text does not flow around it there; the image is placed left or right). Text without marks shows "[description]", as for Markdown images.
- **Ctrl+H**: open the editor's find/replace bar with Replace and Replace all (`Enter` finds next, `Esc` closes).
- **Syntax / F1**: show a Markdown quick reference grouped into Basics, Diagrams (Mermaid), Math (KaTeX), Written as on GitHub, and Other drawings and video: flowchart, sequence, Gantt, pie, class, state, ER, mind map, bar and line chart, and timeline diagrams (drawn in the list), and ten math examples such as fractions, roots, sums, matrices, and cases. Written as on GitHub has alerts, collapsible sections, keys, and emoji; Other drawings and video has Graphviz, a map (GeoJSON), and video (all short versions of examples in samples/diagram-test). Insert puts an example at the cursor in the editor (`Esc` closes).
- **Ctrl+F**: the standard window find, which also works in the preview.
- **Scroll sync**: scrolling the editor or the preview moves the other to the same place, including jumps from find/replace and heading links.
- **Heading links**: links within the document such as `[English](#english)` move the preview to that heading. Heading ids follow GitHub rules (Japanese kept as is, letters in lowercase, spaces as `-`).
- **Links to other files**: a link to a file beside the document, such as `[License](./LICENSE)`, opens Markdown and text files (no extension, `.md`, `.txt`, and so on) in a new mdEditor window, leaving the current document open. `other.md#heading` shows that heading. PDFs, images, CSV files, and similar open in their usual app; programs and other files that could be unsafe are never run but shown in File Explorer.
- **Pane divider**: drag the center divider to resize the editor and the preview. Focus the divider and press the left or right arrow keys to resize in 5% steps. On narrow windows the divider is vertical and uses the up and down arrow keys. The width is remembered for the next launch.
- **Fill window with editor / preview**: each pane has a button that expands that pane to fill the app window (not OS full screen). Return with "Restore layout" or `Esc`.
- **Window full screen**: toggle with `F11`; exit with `F11` or `Esc`.
- **Preview in its own window / Ctrl+Shift+3**: "Open in its own window" in the preview heading opens a window with only the preview. Put it on a second display, maximize it, or press `F11` for full screen. The main window then shows the editor across its full width.
  - The editor and the preview still scroll together (they show the same line of the document). Heading links, moving with find/replace, and read-aloud keep working.
  - Shortcuts pressed in the preview window (Save, Find/Replace, Settings, and so on) run in the main window.
  - "Return to main window" (in the preview window), "Return preview here" (in the editor), or closing the preview window brings back the side-by-side layout.
  - The window's position, size, and maximized state, and whether the preview was in its own window, are remembered. If that display is no longer connected, the window moves to one you can see.
- **Keys / Shift+F1**: list the keyboard shortcuts for every command. Change any key with Change (Ctrl with a letter or digit, or an F key), remove it with None, or turn every shortcut off with Use keyboard shortcuts. The settings are remembered.
- **Settings / Ctrl+Shift+O**: adjust readability and ease of use to suit you. Every option except automatic backup starts off (standard), so only the people who need one turn it on. Settings are remembered.
  - **Typeface**: switch to BIZ UD Gothic, a Universal Design typeface included with Windows.
  - **Text size and line spacing**: make the editor and preview text 125%, 150%, or 200%, and the line spacing Wide or Wider.
  - **Markdown syntax**: with "Show documents closer to GitHub" on (off by default), mdEditor's own syntax that GitHub does not have (`x^2^` superscript, `H~2~O` subscript, `==highlight==`, `++insert++`, definition lists, `:::` containers, attributes such as `{#id}`, abbreviations, grid tables, and `a.` numbering) shows as the plain text you wrote, as on GitHub, and `~text~` is struck through. Exports such as PDF and Word follow it too. It does not make the display identical to GitHub (diagrams, maps, 3D models, code colors, and how math is drawn stay mdEditor's own). The setting is remembered.
  - **Export**: with "Keep the original Markdown in saved HTML" on (off by default), HTML you save opens back in mdEditor exactly as written, diagram and math source included. The original text goes into the HTML file as it is, including comments (`<!-- -->`) that are never shown, so check it before you share the file.
  - **Preview text width**: on wide screens the preview text keeps a comfortable width (about 44 full-width characters), centered; tables, diagrams, code, and display math may be wider. "Don't limit the preview text width" uses the full width.
  - **Zoom**: zoom the whole window with Zoom in, Zoom out, and Reset to 100%. Ctrl + mouse wheel, Ctrl + plus/minus, and Ctrl + 0 also work; the zoom level is remembered.
  - **Editor**: highlight the current line and use a thick cursor.
  - **Read aloud**: when on, Read aloud and Stop buttons appear in the preview heading. Reading starts from the first paragraph visible in the preview and outlines the paragraph being read. Choose the speed (five steps), the pause between paragraphs (none, 0.5, 1, or 2 seconds), the language (Automatic, Japanese, English), and the voice (from those on this PC). If no voice is installed for the language, the app explains how to add one. Only voices installed on this PC are used, so the text is never sent anywhere. `Esc` also stops reading.
  - **Automatic backup** (on by default): unsaved changes are copied every 15 seconds to `%LOCALAPPDATA%\mdEditor\Recovery`. If the app ends abnormally, the next launch asks whether to restore them. The copy is removed when you save or close normally.
- **Windows contrast themes**: with a Windows contrast theme, the app uses its colors and keeps button outlines and focus indicators.

#### Keyboard shortcuts (defaults)

| Command | Key | Command | Key |
| :--- | :--- | :--- | :--- |
| Open | Ctrl+O | Find/Replace | Ctrl+H |
| Insert or update the table of contents | Ctrl+Shift+J | Back to previous document | Ctrl+Shift+B |
| Image size and position | Ctrl+Shift+F | | |
| Save | Ctrl+S | Find next / previous | F3 / Shift+F3 |
| Save As | Ctrl+Shift+S | Replace all | Ctrl+Shift+H |
| Save as PDF | Ctrl+Shift+P | Markdown syntax | F1 |
| Switch paper size | Ctrl+Shift+A | Keyboard shortcuts | Shift+F1 |
| Switch theme | Ctrl+Shift+T | Fill window with editor / preview | Ctrl+Shift+1 / Ctrl+Shift+2 |
| Switch language | Ctrl+Shift+L | Next / previous pane (toolbar, editor, preview) | F6 / Shift+F6 |
| Text encoding | Ctrl+Shift+E | Window full screen | F11 |
| Settings | Ctrl+Shift+O | Read aloud / pause, Stop (when read-aloud is on) | Ctrl+Shift+Y / Ctrl+Shift+U |
| Preview in its own window / return | Ctrl+Shift+3 | Save as HTML | Ctrl+Shift+M |
| Open URL | Ctrl+Shift+K | Save as PNG / Save as Word | Ctrl+Shift+G / Ctrl+Shift+D |

`Esc` always stops reading aloud, restores the layout, or leaves full screen. Each button's tooltip and the information given to screen readers (`aria-keyshortcuts`) show the current key.

Changes in the Markdown are reflected in the preview. If there are unsaved changes when you open a file or close the app, you are asked whether to save.

### Markdown and preview

Headings, lists, task lists, quotes, tables (left, center, and right alignment), code, links, and images are rendered. HTML written in Markdown shows only what GitHub allows (`<details>`, `<summary>`, `<kbd>`, `<sup>`, `<sub>`, `<br>`, `<picture>`, image widths, centering with `align`, and so on); everything else is removed. As on GitHub, scripts, `<style>`, `<iframe>`, embeds, forms, event attributes such as `onclick`, and `javascript:` links are removed and never run. Internet images written in HTML wait for permission, like Markdown images. HTML comments (`<!-- -->`) are not shown. Web links open in the default browser and mail links in the default mail app. The PDF contains only the rendered preview, not the editing text area or buttons.

Relative images such as `images/a.png` load from the document's folder and its subfolders. Only image files (PNG, JPEG, GIF, WebP, AVIF, BMP, ICO, SVG) are read, and nothing above the document's folder.

The text encoding is detected automatically: UTF-8 (with or without BOM), UTF-16, or the Windows default code page (Shift_JIS on Japanese Windows). Saving keeps the original encoding; if some characters cannot be saved in it, the app asks whether to save as UTF-8. A notice appears under the toolbar when a non-UTF-8 file was opened with a guessed encoding or when some bytes could not be read (shown as �); overwriting such a file asks for confirmation first, because those parts of the original would be lost. For EUC-JP or ISO-2022-JP files, choose the encoding and reopen the file. Saves are written to a temporary file and then swapped in, so a failure part-way never damages the original document.

` ```mermaid ` code blocks are drawn as diagrams with [Mermaid](https://mermaid.js.org/) (flowcharts, sequence, class, state, ER, Gantt, pie charts, and more). Math in `$...$` and `$$...$$` is rendered with [KaTeX](https://katex.org/). Both are bundled with the app, so they work offline and are included in PDFs. Code blocks with a language name, such as ` ```python `, are colored with [highlight.js](https://highlightjs.org/) (about 40 languages, including Python, JavaScript, TypeScript, C#, C/C++, Java, PowerShell, shell, SQL, HTML, CSS, JSON, YAML, and diff). The colors follow the preview's day and night palettes and are kept when you save as PDF, PNG, HTML, or Word. Code blocks without a language stay plain. A diagram with a syntax error shows the error message and its source. Use [samples/diagram-test.en.md](./samples/diagram-test.en.md) (Japanese: [samples/diagram-test.md](./samples/diagram-test.md)) to check the rendering.

#### Supported diagrams and charts

How a block is rendered depends on the language name of the code block.

```mermaid
flowchart LR
    src["Code block or math"] --> kind{"Notation"}
    kind -- "mermaid" --> mermaid["Drawn as a diagram<br/>Mermaid 11.17.2"]
    kind -- "Math (TeX)" --> math["Rendered as math<br/>KaTeX 0.18.9"]
    kind -- "dot, graphviz" --> graphviz["Drawn as a diagram<br/>Graphviz 16.1.0"]
    kind -- "plantuml, nomnoml, etc." --> code["Shown as a code block"]

    classDef supported fill:#d4f7d4,stroke:#2a7a2a,color:#1a1a1a;
    classDef unsupported fill:#eeeeee,stroke:#777777,color:#1a1a1a;
    class mermaid,math,graphviz supported;
    class code unsupported;
```

| Type | Syntax | Rendered | Notes |
| :--- | :--- | :---: | :--- |
| Flowchart | ` ```mermaid ` + `flowchart` / `graph` | Yes | |
| Sequence diagram | `sequenceDiagram` | Yes | |
| Class diagram | `classDiagram` | Yes | |
| State diagram | `stateDiagram-v2` | Yes | |
| ER diagram | `erDiagram` | Yes | |
| Gantt chart | `gantt` | Yes | |
| Pie chart | `pie` | Yes | |
| Bar and line chart | `xychart-beta` | Yes | Quote non-ASCII axis labels, such as `"1月"` |
| Quadrant chart | `quadrantChart` | Yes | |
| User journey | `journey` | Yes | |
| Git graph | `gitGraph` | Yes | |
| Mind map | `mindmap` | Yes | |
| Timeline | `timeline` | Yes | |
| Sankey diagram | `sankey-beta` | Yes | ASCII node names only (a Mermaid limitation) |
| Math | `$...$` (inline), `$$...$$` (display) | Yes | Rendered with KaTeX |
| Graphviz | ` ```dot `, ` ```graphviz `, ` ```gv ` | Yes | Drawn offline with the bundled Graphviz (WebAssembly). Links and images inside a diagram are removed for safety. Japanese labels that would overflow their box are made smaller to fit |
| 3D model | ` ```stl ` (ASCII STL) | Yes | Shown offline with the bundled three.js. Drag to turn, use the wheel to zoom. PDF, PNG, HTML, and Word get a picture of the current view |
| Map | ` ```geojson `, ` ```topojson ` | Yes | Points, lines, and areas are first drawn offline on a simple bundled world map (Natural Earth). By default the notice bar asks whether to load detailed maps; pressing Load detailed maps loads an OpenStreetMap map with roads and place names for that document only (the map server sees the area shown and your IP address; it is not kept in saved HTML). Settings > Internet > Detailed maps (OpenStreetMap) also offers Always load and Never load. GitHub's `marker-color`, `stroke`, and `fill` styles work |
| PlantUML | ` ```plantuml ` | No | Shown as a code block. Drawing it needs Java and PlantUML on the PC, or sending the document to an outside PlantUML server, which goes against keeping documents on the PC |
| nomnoml | ` ```nomnoml ` | No | Shown as a code block |

Everything marked Yes is verified with the test documents. Other Mermaid diagrams (such as `block-beta`) render if Mermaid 11.17.2 supports them, but they have not been checked.

Two documents, [samples/diagram-test.md](./samples/diagram-test.md) and [samples/diagram-test.en.md](./samples/diagram-test.en.md), assume they are shown in mdEditor and are not held to GitHub compatibility (Identity 8-9 (7)). They show as they are which diagrams mdEditor can and cannot draw, so they include syntax GitHub cannot render (Graphviz, PlantUML), and their rendering is checked in mdEditor (Identity 8-9 (8), 1-4 (1)). Each says so at the top. The rest of this README, CHANGELOG.md, PRIVACY.md, and the other samples keep to what GitHub can show.


### Written as on GitHub

These ways of writing, common in GitHub READMEs, show the same way as on GitHub. Use [samples/github-test.en.md](./samples/github-test.en.md) (Japanese: [samples/github-test.md](./samples/github-test.md)) to check them.

| Syntax | Example | Shown as |
| :--- | :--- | :--- |
| Alerts | `> [!NOTE]`, `[!TIP]`, `[!IMPORTANT]`, `[!WARNING]`, `[!CAUTION]` | A box with a colored bar, symbol, and title |
| Front matter | `title: …` and so on between `---` lines at the top (YAML) | A card of key and value rows; lists become small labels and nested items sit inside the card (the same in Word). GitHub shows a table |
| Videos | `![](clip.mp4)`, a lone link to a video, or `<video src="…">` (.mp4, .webm, .mov, and so on) | A video that starts by itself without sound and loops (the controls turn the sound on or pause it). With Windows animation effects off, it shows a play button unless autoplay is chosen in Settings > Videos. Internet videos wait for permission, like images. PDF, PNG, and Word get the video's name, HTML a link to it |
| Math code block | ` ```math ` | Math (KaTeX) |
| Inline math, GitHub style | `` $`x^2`$ `` | Math |
| Emoji short names | `:smile:`, `:+1:` | Emoji (smileys such as `:)` stay text) |
| Collapsible sections | `<details><summary>…</summary>…</details>` | Opens when pressed; saved open in PDF, PNG, and Word |
| Image size and position | `<img src="…" alt="…" width="50%">`, `<p align="center"><img …></p>`, `<img … align="left">` | A width as a share of the text width or in pixels, centered or on the right, or with text flowing around it. Adjust image (Ctrl+Shift+F) or a right-click in the preview rewrites it. Word gets the size and placement too |
| E-mail addresses | `info@example.com` | A mailto link (a full stop at the end is left out; addresses in code stay text) |
| Bars in code inside a table | A `\` before the bar, in code inside a table cell | A single bar in the code (the `\` is not shown) |
| Older attributes GitHub keeps | `<br clear="all">`, `<table border="1">`, `nowrap`, `cellpadding`, `hspace`, and so on | Work as written; `<br clear="all">` ends text flowing around a picture |
| Day and night pictures | `<picture>` with `<source media="(prefers-color-scheme: dark)">`, or `#gh-dark-mode-only` / `#gh-light-mode-only` at the end of an image URL | Only the picture for the current palette |

### Examples: 3D model, map, and video

The table at the top of this README (a card in mdEditor) is the YAML front matter (the part between `---` lines). The three examples below also show on both GitHub and mdEditor (on GitHub the video is a link to it; GitHub plays only videos uploaded to GitHub).

```stl
solid cube
facet normal 0 -1 0 outer loop vertex -14 -14 -14 vertex 14 -14 -14 vertex 14 -14 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -14 -14 -14 vertex 14 -14 14 vertex -14 -14 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 14 14 -14 vertex -14 14 -14 vertex -14 14 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 14 14 -14 vertex -14 14 14 vertex 14 14 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 14 -14 -14 vertex 14 14 -14 vertex 14 14 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 14 -14 -14 vertex 14 14 14 vertex 14 -14 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -14 14 -14 vertex -14 -14 -14 vertex -14 -14 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -14 14 -14 vertex -14 -14 14 vertex -14 14 14 endloop endfacet
facet normal 0 0 1 outer loop vertex -14 -14 14 vertex 14 -14 14 vertex 14 14 14 endloop endfacet
facet normal 0 0 1 outer loop vertex -14 -14 14 vertex 14 14 14 vertex -14 14 14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 14 -14 vertex 14 14 -14 vertex 14 -14 -14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 14 -14 vertex 14 -14 -14 vertex -14 -14 -14 endloop endfacet
endsolid cube
solid letters
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 7.5 vertex 7.5 -15.5 7.5 vertex 7.5 -15.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 7.5 vertex 7.5 -15.5 10.5 vertex -7.5 -15.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 1.5 vertex -4.5 -15.5 1.5 vertex -4.5 -15.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 1.5 vertex -4.5 -15.5 7.5 vertex -7.5 -15.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 1.5 vertex 4.5 -15.5 1.5 vertex 4.5 -15.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 1.5 vertex 4.5 -15.5 4.5 vertex -4.5 -15.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -15.5 -7.5 vertex 7.5 -15.5 -7.5 vertex 7.5 -15.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -15.5 -7.5 vertex 7.5 -15.5 1.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 -7.5 vertex -4.5 -15.5 -7.5 vertex -4.5 -15.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -15.5 -7.5 vertex -4.5 -15.5 -4.5 vertex -7.5 -15.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 -10.5 vertex 4.5 -15.5 -10.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -15.5 -10.5 vertex 4.5 -15.5 -7.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 10.5 vertex 7.5 -15.5 10.5 vertex 7.5 -14 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 10.5 vertex 7.5 -14 10.5 vertex -7.5 -14 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 7.5 vertex 7.5 -14 7.5 vertex 7.5 -15.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 7.5 vertex 7.5 -15.5 7.5 vertex -4.5 -15.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 4.5 vertex 4.5 -15.5 4.5 vertex 4.5 -14 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 4.5 vertex 4.5 -14 4.5 vertex -4.5 -14 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 1.5 vertex 4.5 -14 1.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 1.5 vertex 4.5 -15.5 1.5 vertex -7.5 -15.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -15.5 1.5 vertex 7.5 -15.5 1.5 vertex 7.5 -14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -15.5 1.5 vertex 7.5 -14 1.5 vertex 4.5 -14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 -4.5 vertex -4.5 -15.5 -4.5 vertex -4.5 -14 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -15.5 -4.5 vertex -4.5 -14 -4.5 vertex -7.5 -14 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 -7.5 vertex -4.5 -14 -7.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -14 -7.5 vertex -4.5 -15.5 -7.5 vertex -7.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 -14 -7.5 vertex 7.5 -14 -7.5 vertex 7.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 -14 -7.5 vertex 7.5 -15.5 -7.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 -7.5 vertex 4.5 -15.5 -7.5 vertex 4.5 -14 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -15.5 -7.5 vertex 4.5 -14 -7.5 vertex -4.5 -14 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 -10.5 vertex 4.5 -14 -10.5 vertex 4.5 -15.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -14 -10.5 vertex 4.5 -15.5 -10.5 vertex -4.5 -15.5 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 10.5 vertex -7.5 -14 1.5 vertex -7.5 -15.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 10.5 vertex -7.5 -15.5 1.5 vertex -7.5 -15.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 -4.5 vertex -7.5 -14 -7.5 vertex -7.5 -15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -14 -4.5 vertex -7.5 -15.5 -7.5 vertex -7.5 -15.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 7.5 vertex -4.5 -15.5 4.5 vertex -4.5 -14 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 7.5 vertex -4.5 -14 4.5 vertex -4.5 -14 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 -4.5 vertex -4.5 -15.5 -7.5 vertex -4.5 -14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -15.5 -4.5 vertex -4.5 -14 -7.5 vertex -4.5 -14 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -14 -7.5 vertex -4.5 -14 -10.5 vertex -4.5 -15.5 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -14 -7.5 vertex -4.5 -15.5 -10.5 vertex -4.5 -15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 4.5 vertex 4.5 -15.5 1.5 vertex 4.5 -14 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 4.5 vertex 4.5 -14 1.5 vertex 4.5 -14 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 -7.5 vertex 4.5 -15.5 -10.5 vertex 4.5 -14 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -15.5 -7.5 vertex 4.5 -14 -10.5 vertex 4.5 -14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -14 1.5 vertex 4.5 -14 -7.5 vertex 4.5 -15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -14 1.5 vertex 4.5 -15.5 -7.5 vertex 4.5 -15.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 10.5 vertex 7.5 -15.5 7.5 vertex 7.5 -14 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 10.5 vertex 7.5 -14 7.5 vertex 7.5 -14 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 1.5 vertex 7.5 -15.5 -7.5 vertex 7.5 -14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -15.5 1.5 vertex 7.5 -14 -7.5 vertex 7.5 -14 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 7.5 vertex -7.5 15.5 7.5 vertex -7.5 15.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 7.5 vertex -7.5 15.5 10.5 vertex 7.5 15.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 1.5 vertex 4.5 15.5 1.5 vertex 4.5 15.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 1.5 vertex 4.5 15.5 7.5 vertex 7.5 15.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 1.5 vertex -4.5 15.5 1.5 vertex -4.5 15.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 1.5 vertex -4.5 15.5 4.5 vertex 4.5 15.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 15.5 -7.5 vertex -7.5 15.5 -7.5 vertex -7.5 15.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 15.5 -7.5 vertex -7.5 15.5 1.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 -7.5 vertex 4.5 15.5 -7.5 vertex 4.5 15.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.5 15.5 -7.5 vertex 4.5 15.5 -4.5 vertex 7.5 15.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 -10.5 vertex -4.5 15.5 -10.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 15.5 -10.5 vertex -4.5 15.5 -7.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 10.5 vertex -7.5 15.5 10.5 vertex -7.5 14 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 10.5 vertex -7.5 14 10.5 vertex 7.5 14 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 7.5 vertex -7.5 14 7.5 vertex -7.5 15.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 7.5 vertex -7.5 15.5 7.5 vertex 4.5 15.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 4.5 vertex -4.5 15.5 4.5 vertex -4.5 14 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 4.5 vertex -4.5 14 4.5 vertex 4.5 14 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 1.5 vertex -4.5 14 1.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 1.5 vertex -4.5 15.5 1.5 vertex 7.5 15.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 15.5 1.5 vertex -7.5 15.5 1.5 vertex -7.5 14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 15.5 1.5 vertex -7.5 14 1.5 vertex -4.5 14 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 -4.5 vertex 4.5 15.5 -4.5 vertex 4.5 14 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 7.5 15.5 -4.5 vertex 4.5 14 -4.5 vertex 7.5 14 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 -7.5 vertex 4.5 14 -7.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 7.5 14 -7.5 vertex 4.5 15.5 -7.5 vertex 7.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 14 -7.5 vertex -7.5 14 -7.5 vertex -7.5 15.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 14 -7.5 vertex -7.5 15.5 -7.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 -7.5 vertex -4.5 15.5 -7.5 vertex -4.5 14 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 15.5 -7.5 vertex -4.5 14 -7.5 vertex 4.5 14 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 -10.5 vertex -4.5 14 -10.5 vertex -4.5 15.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 14 -10.5 vertex -4.5 15.5 -10.5 vertex 4.5 15.5 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 10.5 vertex 7.5 14 1.5 vertex 7.5 15.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 10.5 vertex 7.5 15.5 1.5 vertex 7.5 15.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 -4.5 vertex 7.5 14 -7.5 vertex 7.5 15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 14 -4.5 vertex 7.5 15.5 -7.5 vertex 7.5 15.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 7.5 vertex 4.5 15.5 4.5 vertex 4.5 14 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 7.5 vertex 4.5 14 4.5 vertex 4.5 14 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 -4.5 vertex 4.5 15.5 -7.5 vertex 4.5 14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 15.5 -4.5 vertex 4.5 14 -7.5 vertex 4.5 14 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 14 -7.5 vertex 4.5 14 -10.5 vertex 4.5 15.5 -10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 14 -7.5 vertex 4.5 15.5 -10.5 vertex 4.5 15.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 4.5 vertex -4.5 15.5 1.5 vertex -4.5 14 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 4.5 vertex -4.5 14 1.5 vertex -4.5 14 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 -7.5 vertex -4.5 15.5 -10.5 vertex -4.5 14 -10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 15.5 -7.5 vertex -4.5 14 -10.5 vertex -4.5 14 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 14 1.5 vertex -4.5 14 -7.5 vertex -4.5 15.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 14 1.5 vertex -4.5 15.5 -7.5 vertex -4.5 15.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 10.5 vertex -7.5 15.5 7.5 vertex -7.5 14 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 10.5 vertex -7.5 14 7.5 vertex -7.5 14 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 1.5 vertex -7.5 15.5 -7.5 vertex -7.5 14 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 15.5 1.5 vertex -7.5 14 -7.5 vertex -7.5 14 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 7.5 vertex 15.5 7.5 7.5 vertex 15.5 7.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 7.5 vertex 15.5 7.5 10.5 vertex 15.5 -7.5 10.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 1.5 vertex 15.5 -4.5 1.5 vertex 15.5 -4.5 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 1.5 vertex 15.5 -4.5 7.5 vertex 15.5 -7.5 7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 1.5 vertex 15.5 4.5 1.5 vertex 15.5 4.5 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 1.5 vertex 15.5 4.5 4.5 vertex 15.5 -4.5 4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 7.5 -7.5 vertex 15.5 7.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 7.5 1.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 -7.5 vertex 15.5 -4.5 -7.5 vertex 15.5 -4.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -7.5 -7.5 vertex 15.5 -4.5 -4.5 vertex 15.5 -7.5 -4.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 -10.5 vertex 15.5 4.5 -10.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 15.5 -4.5 -10.5 vertex 15.5 4.5 -7.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 10.5 vertex 15.5 7.5 10.5 vertex 14 7.5 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 10.5 vertex 14 7.5 10.5 vertex 14 -7.5 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 7.5 vertex 14 7.5 7.5 vertex 15.5 7.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 7.5 vertex 15.5 7.5 7.5 vertex 15.5 -4.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 4.5 vertex 15.5 4.5 4.5 vertex 14 4.5 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 4.5 vertex 14 4.5 4.5 vertex 14 -4.5 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 1.5 vertex 14 4.5 1.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 1.5 vertex 15.5 4.5 1.5 vertex 15.5 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 4.5 1.5 vertex 15.5 7.5 1.5 vertex 14 7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 4.5 1.5 vertex 14 7.5 1.5 vertex 14 4.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 -4.5 vertex 15.5 -4.5 -4.5 vertex 14 -4.5 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -7.5 -4.5 vertex 14 -4.5 -4.5 vertex 14 -7.5 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 -7.5 vertex 14 -4.5 -7.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -7.5 -7.5 vertex 15.5 -4.5 -7.5 vertex 15.5 -7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 4.5 -7.5 vertex 14 7.5 -7.5 vertex 15.5 7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 4.5 -7.5 vertex 15.5 7.5 -7.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 -7.5 vertex 15.5 4.5 -7.5 vertex 14 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 15.5 -4.5 -7.5 vertex 14 4.5 -7.5 vertex 14 -4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 -10.5 vertex 14 4.5 -10.5 vertex 15.5 4.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 14 -4.5 -10.5 vertex 15.5 4.5 -10.5 vertex 15.5 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 10.5 vertex 14 -7.5 1.5 vertex 15.5 -7.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 10.5 vertex 15.5 -7.5 1.5 vertex 15.5 -7.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 -4.5 vertex 14 -7.5 -7.5 vertex 15.5 -7.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -7.5 -4.5 vertex 15.5 -7.5 -7.5 vertex 15.5 -7.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 7.5 vertex 15.5 -4.5 4.5 vertex 14 -4.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 7.5 vertex 14 -4.5 4.5 vertex 14 -4.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 -4.5 vertex 15.5 -4.5 -7.5 vertex 14 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 -4.5 -4.5 vertex 14 -4.5 -7.5 vertex 14 -4.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -4.5 -7.5 vertex 14 -4.5 -10.5 vertex 15.5 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 -4.5 -7.5 vertex 15.5 -4.5 -10.5 vertex 15.5 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 4.5 vertex 15.5 4.5 1.5 vertex 14 4.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 4.5 vertex 14 4.5 1.5 vertex 14 4.5 4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 -7.5 vertex 15.5 4.5 -10.5 vertex 14 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 4.5 -7.5 vertex 14 4.5 -10.5 vertex 14 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 4.5 1.5 vertex 14 4.5 -7.5 vertex 15.5 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 14 4.5 1.5 vertex 15.5 4.5 -7.5 vertex 15.5 4.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 10.5 vertex 15.5 7.5 7.5 vertex 14 7.5 7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 10.5 vertex 14 7.5 7.5 vertex 14 7.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 1.5 vertex 15.5 7.5 -7.5 vertex 14 7.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 15.5 7.5 1.5 vertex 14 7.5 -7.5 vertex 14 7.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 7.5 vertex -15.5 -7.5 7.5 vertex -15.5 -7.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 7.5 vertex -15.5 -7.5 10.5 vertex -15.5 7.5 10.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 1.5 vertex -15.5 4.5 1.5 vertex -15.5 4.5 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 1.5 vertex -15.5 4.5 7.5 vertex -15.5 7.5 7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 1.5 vertex -15.5 -4.5 1.5 vertex -15.5 -4.5 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 1.5 vertex -15.5 -4.5 4.5 vertex -15.5 4.5 4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -7.5 -7.5 vertex -15.5 -7.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -7.5 1.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 -7.5 vertex -15.5 4.5 -7.5 vertex -15.5 4.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 7.5 -7.5 vertex -15.5 4.5 -4.5 vertex -15.5 7.5 -4.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 -10.5 vertex -15.5 -4.5 -10.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -15.5 4.5 -10.5 vertex -15.5 -4.5 -7.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 10.5 vertex -15.5 -7.5 10.5 vertex -14 -7.5 10.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 10.5 vertex -14 -7.5 10.5 vertex -14 7.5 10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 7.5 vertex -14 -7.5 7.5 vertex -15.5 -7.5 7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 7.5 vertex -15.5 -7.5 7.5 vertex -15.5 4.5 7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 4.5 vertex -15.5 -4.5 4.5 vertex -14 -4.5 4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 4.5 vertex -14 -4.5 4.5 vertex -14 4.5 4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 1.5 vertex -14 -4.5 1.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 1.5 vertex -15.5 -4.5 1.5 vertex -15.5 7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 -4.5 1.5 vertex -15.5 -7.5 1.5 vertex -14 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 -4.5 1.5 vertex -14 -7.5 1.5 vertex -14 -4.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 -4.5 vertex -15.5 4.5 -4.5 vertex -14 4.5 -4.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 7.5 -4.5 vertex -14 4.5 -4.5 vertex -14 7.5 -4.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 -7.5 vertex -14 4.5 -7.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 7.5 -7.5 vertex -15.5 4.5 -7.5 vertex -15.5 7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 -4.5 -7.5 vertex -14 -7.5 -7.5 vertex -15.5 -7.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 -4.5 -7.5 vertex -15.5 -7.5 -7.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 -7.5 vertex -15.5 -4.5 -7.5 vertex -14 -4.5 -7.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -15.5 4.5 -7.5 vertex -14 -4.5 -7.5 vertex -14 4.5 -7.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 -10.5 vertex -14 -4.5 -10.5 vertex -15.5 -4.5 -10.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -14 4.5 -10.5 vertex -15.5 -4.5 -10.5 vertex -15.5 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 10.5 vertex -14 7.5 1.5 vertex -15.5 7.5 1.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 10.5 vertex -15.5 7.5 1.5 vertex -15.5 7.5 10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 -4.5 vertex -14 7.5 -7.5 vertex -15.5 7.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 7.5 -4.5 vertex -15.5 7.5 -7.5 vertex -15.5 7.5 -4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 7.5 vertex -15.5 4.5 4.5 vertex -14 4.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 7.5 vertex -14 4.5 4.5 vertex -14 4.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 -4.5 vertex -15.5 4.5 -7.5 vertex -14 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 4.5 -4.5 vertex -14 4.5 -7.5 vertex -14 4.5 -4.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 4.5 -7.5 vertex -14 4.5 -10.5 vertex -15.5 4.5 -10.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 4.5 -7.5 vertex -15.5 4.5 -10.5 vertex -15.5 4.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 4.5 vertex -15.5 -4.5 1.5 vertex -14 -4.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 4.5 vertex -14 -4.5 1.5 vertex -14 -4.5 4.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 -7.5 vertex -15.5 -4.5 -10.5 vertex -14 -4.5 -10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -4.5 -7.5 vertex -14 -4.5 -10.5 vertex -14 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 -4.5 1.5 vertex -14 -4.5 -7.5 vertex -15.5 -4.5 -7.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -14 -4.5 1.5 vertex -15.5 -4.5 -7.5 vertex -15.5 -4.5 1.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 10.5 vertex -15.5 -7.5 7.5 vertex -14 -7.5 7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 10.5 vertex -14 -7.5 7.5 vertex -14 -7.5 10.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 1.5 vertex -15.5 -7.5 -7.5 vertex -14 -7.5 -7.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -15.5 -7.5 1.5 vertex -14 -7.5 -7.5 vertex -14 -7.5 1.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 7.5 15.5 vertex 7.5 7.5 15.5 vertex 7.5 10.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 7.5 15.5 vertex 7.5 10.5 15.5 vertex -7.5 10.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 1.5 15.5 vertex -4.5 1.5 15.5 vertex -4.5 7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 1.5 15.5 vertex -4.5 7.5 15.5 vertex -7.5 7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 1.5 15.5 vertex 4.5 1.5 15.5 vertex 4.5 4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 1.5 15.5 vertex 4.5 4.5 15.5 vertex -4.5 4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -7.5 15.5 vertex 7.5 -7.5 15.5 vertex 7.5 1.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex 4.5 -7.5 15.5 vertex 7.5 1.5 15.5 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -7.5 15.5 vertex -4.5 -7.5 15.5 vertex -4.5 -4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -7.5 -7.5 15.5 vertex -4.5 -4.5 15.5 vertex -7.5 -4.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -10.5 15.5 vertex 4.5 -10.5 15.5 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal 0 0 1 outer loop vertex -4.5 -10.5 15.5 vertex 4.5 -7.5 15.5 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 10.5 15.5 vertex 7.5 10.5 15.5 vertex 7.5 10.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 10.5 15.5 vertex 7.5 10.5 14 vertex -7.5 10.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 14 vertex 7.5 7.5 14 vertex 7.5 7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 14 vertex 7.5 7.5 15.5 vertex -4.5 7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 4.5 15.5 vertex 4.5 4.5 15.5 vertex 4.5 4.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 4.5 15.5 vertex 4.5 4.5 14 vertex -4.5 4.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 1.5 14 vertex 4.5 1.5 14 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 1.5 14 vertex 4.5 1.5 15.5 vertex -7.5 1.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 1.5 15.5 vertex 7.5 1.5 15.5 vertex 7.5 1.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 1.5 15.5 vertex 7.5 1.5 14 vertex 4.5 1.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -4.5 15.5 vertex -4.5 -4.5 15.5 vertex -4.5 -4.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -4.5 15.5 vertex -4.5 -4.5 14 vertex -7.5 -4.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -7.5 14 vertex -4.5 -7.5 14 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -7.5 14 vertex -4.5 -7.5 15.5 vertex -7.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -7.5 14 vertex 7.5 -7.5 14 vertex 7.5 -7.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -7.5 14 vertex 7.5 -7.5 15.5 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 15.5 vertex 4.5 -7.5 15.5 vertex 4.5 -7.5 14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 15.5 vertex 4.5 -7.5 14 vertex -4.5 -7.5 14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -10.5 14 vertex 4.5 -10.5 14 vertex 4.5 -10.5 15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -10.5 14 vertex 4.5 -10.5 15.5 vertex -4.5 -10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 10.5 14 vertex -7.5 1.5 14 vertex -7.5 1.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 10.5 14 vertex -7.5 1.5 15.5 vertex -7.5 10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -4.5 14 vertex -7.5 -7.5 14 vertex -7.5 -7.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -4.5 14 vertex -7.5 -7.5 15.5 vertex -7.5 -4.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 7.5 15.5 vertex -4.5 4.5 15.5 vertex -4.5 4.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 7.5 15.5 vertex -4.5 4.5 14 vertex -4.5 7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -4.5 15.5 vertex -4.5 -7.5 15.5 vertex -4.5 -7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -4.5 15.5 vertex -4.5 -7.5 14 vertex -4.5 -4.5 14 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -7.5 14 vertex -4.5 -10.5 14 vertex -4.5 -10.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 -7.5 14 vertex -4.5 -10.5 15.5 vertex -4.5 -7.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 4.5 15.5 vertex 4.5 1.5 15.5 vertex 4.5 1.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 4.5 15.5 vertex 4.5 1.5 14 vertex 4.5 4.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -7.5 15.5 vertex 4.5 -10.5 15.5 vertex 4.5 -10.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -7.5 15.5 vertex 4.5 -10.5 14 vertex 4.5 -7.5 14 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 1.5 14 vertex 4.5 -7.5 14 vertex 4.5 -7.5 15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 1.5 14 vertex 4.5 -7.5 15.5 vertex 4.5 1.5 15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 10.5 15.5 vertex 7.5 7.5 15.5 vertex 7.5 7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 10.5 15.5 vertex 7.5 7.5 14 vertex 7.5 10.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 1.5 15.5 vertex 7.5 -7.5 15.5 vertex 7.5 -7.5 14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 1.5 15.5 vertex 7.5 -7.5 14 vertex 7.5 1.5 14 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -7.5 -15.5 vertex 7.5 -7.5 -15.5 vertex 7.5 -10.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -7.5 -15.5 vertex 7.5 -10.5 -15.5 vertex -7.5 -10.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -1.5 -15.5 vertex -4.5 -1.5 -15.5 vertex -4.5 -7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 -1.5 -15.5 vertex -4.5 -7.5 -15.5 vertex -7.5 -7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -1.5 -15.5 vertex 4.5 -1.5 -15.5 vertex 4.5 -4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 -1.5 -15.5 vertex 4.5 -4.5 -15.5 vertex -4.5 -4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 7.5 -15.5 vertex 7.5 7.5 -15.5 vertex 7.5 -1.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex 4.5 7.5 -15.5 vertex 7.5 -1.5 -15.5 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 7.5 -15.5 vertex -4.5 7.5 -15.5 vertex -4.5 4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -7.5 7.5 -15.5 vertex -4.5 4.5 -15.5 vertex -7.5 4.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 10.5 -15.5 vertex 4.5 10.5 -15.5 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal 0 0 -1 outer loop vertex -4.5 10.5 -15.5 vertex 4.5 7.5 -15.5 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -10.5 -15.5 vertex 7.5 -10.5 -15.5 vertex 7.5 -10.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 -10.5 -15.5 vertex 7.5 -10.5 -14 vertex -7.5 -10.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 -14 vertex 7.5 -7.5 -14 vertex 7.5 -7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 -7.5 -14 vertex 7.5 -7.5 -15.5 vertex -4.5 -7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -4.5 -15.5 vertex 4.5 -4.5 -15.5 vertex 4.5 -4.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 -4.5 -15.5 vertex 4.5 -4.5 -14 vertex -4.5 -4.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -1.5 -14 vertex 4.5 -1.5 -14 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 -1.5 -14 vertex 4.5 -1.5 -15.5 vertex -7.5 -1.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -1.5 -15.5 vertex 7.5 -1.5 -15.5 vertex 7.5 -1.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex 4.5 -1.5 -15.5 vertex 7.5 -1.5 -14 vertex 4.5 -1.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 4.5 -15.5 vertex -4.5 4.5 -15.5 vertex -4.5 4.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.5 4.5 -15.5 vertex -4.5 4.5 -14 vertex -7.5 4.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 7.5 -14 vertex -4.5 7.5 -14 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -7.5 7.5 -14 vertex -4.5 7.5 -15.5 vertex -7.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 7.5 -14 vertex 7.5 7.5 -14 vertex 7.5 7.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex 4.5 7.5 -14 vertex 7.5 7.5 -15.5 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 -15.5 vertex 4.5 7.5 -15.5 vertex 4.5 7.5 -14 endloop endfacet
facet normal 0 -1 0 outer loop vertex -4.5 7.5 -15.5 vertex 4.5 7.5 -14 vertex -4.5 7.5 -14 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 10.5 -14 vertex 4.5 10.5 -14 vertex 4.5 10.5 -15.5 endloop endfacet
facet normal 0 1 0 outer loop vertex -4.5 10.5 -14 vertex 4.5 10.5 -15.5 vertex -4.5 10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -10.5 -14 vertex -7.5 -1.5 -14 vertex -7.5 -1.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 -10.5 -14 vertex -7.5 -1.5 -15.5 vertex -7.5 -10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 4.5 -14 vertex -7.5 7.5 -14 vertex -7.5 7.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -7.5 4.5 -14 vertex -7.5 7.5 -15.5 vertex -7.5 4.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -7.5 -15.5 vertex -4.5 -4.5 -15.5 vertex -4.5 -4.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 -7.5 -15.5 vertex -4.5 -4.5 -14 vertex -4.5 -7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 4.5 -15.5 vertex -4.5 7.5 -15.5 vertex -4.5 7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex -4.5 4.5 -15.5 vertex -4.5 7.5 -14 vertex -4.5 4.5 -14 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 7.5 -14 vertex -4.5 10.5 -14 vertex -4.5 10.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex -4.5 7.5 -14 vertex -4.5 10.5 -15.5 vertex -4.5 7.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -4.5 -15.5 vertex 4.5 -1.5 -15.5 vertex 4.5 -1.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 -4.5 -15.5 vertex 4.5 -1.5 -14 vertex 4.5 -4.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 7.5 -15.5 vertex 4.5 10.5 -15.5 vertex 4.5 10.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 4.5 7.5 -15.5 vertex 4.5 10.5 -14 vertex 4.5 7.5 -14 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -1.5 -14 vertex 4.5 7.5 -14 vertex 4.5 7.5 -15.5 endloop endfacet
facet normal -1 0 0 outer loop vertex 4.5 -1.5 -14 vertex 4.5 7.5 -15.5 vertex 4.5 -1.5 -15.5 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -10.5 -15.5 vertex 7.5 -7.5 -15.5 vertex 7.5 -7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -10.5 -15.5 vertex 7.5 -7.5 -14 vertex 7.5 -10.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -1.5 -15.5 vertex 7.5 7.5 -15.5 vertex 7.5 7.5 -14 endloop endfacet
facet normal 1 0 0 outer loop vertex 7.5 -1.5 -15.5 vertex 7.5 7.5 -14 vertex 7.5 -1.5 -14 endloop endfacet
endsolid letters
```

```geojson
{
  "type": "FeatureCollection",
  "features": [
    { "type": "Feature", "properties": { "name": "British Museum", "marker": "triangle", "description": "The British Museum" }, "geometry": { "type": "Polygon", "coordinates": [[[-0.1270, 51.5199], [-0.1280, 51.5184], [-0.1260, 51.5184], [-0.1270, 51.5199]]] } },
    { "type": "Feature", "properties": { "name": "Royal Observatory, Greenwich", "marker": "circle", "description": "Royal Observatory, Greenwich" }, "geometry": { "type": "Polygon", "coordinates": [[[0.0005, 51.4770], [0.0002, 51.4769], [0.0000, 51.4767], [0.0000, 51.4764], [0.0002, 51.4762], [0.0005, 51.4761], [0.0008, 51.4762], [0.0010, 51.4764], [0.0010, 51.4767], [0.0008, 51.4769], [0.0005, 51.4770]]] } },
    { "type": "Feature", "properties": { "name": "Route", "description": "A line connecting the British Museum and the Royal Observatory, Greenwich" }, "geometry": { "type": "LineString", "coordinates": [[-0.1270, 51.5190], [0.0005, 51.4766]] } }
  ]
}
```

[A video of the "5" cube turning](samples/import-test-images/cube5.mp4)

### Accessibility check record

Record of the pre-release check in Identity Art. 7-7.

- Date: 2026-10-11
- Tools: Microsoft Edge (headless) with the page in fixed-size iframes, mdEditor itself (WebView2) driven by UI Automation and keystrokes, grayscale conversion of screenshots
- Combinations: Light and Dark × Japanese and English × 1280×800, 1024×768, 640×400 (200% zoom of 1280×800), and 320×640

| Item | Result | Notes |
| :--- | :--- | :--- |
| (a) No horizontal scroll at 200% zoom | OK | Nothing overflows horizontally. Short windows scroll the whole page so the editor and preview keep their height (fixed during this check) |
| (b) Full operation without a mouse | OK | Tab reaches every button, the editor, the divider, and the preview. Dialogs open with Enter, choices use the arrow keys, and Esc closes them and returns focus (preview focus fixed during this check) |
| (c) Meaning survives in grayscale | OK | States are words ("Unsaved changes", "(guessed)"); notices and errors use a symbol and wording; selection uses shape |
| (d) 320 CSS px, vertical scrolling only | OK | Truncated file name and an overflowing pane-heading button fixed during this check |
| (e) (a)–(d) in the night palette | OK | Same results in Dark |
| (f) Contrast themes (forced colors) | OK | With Edge forced colors (`--force-high-contrast`), button borders, dividers, focus indicators, and body text stay readable. Editor selections and the read-aloud frame use the system Highlight color; the logos and diagrams keep their own colors |
| (g) Screen readers | OK | With Windows Narrator, every button, the editor, and the divider have names and roles; task-list checkboxes carry their item text. With Narrator on, selecting text in a long document (about 260 lines) froze input for about 20 seconds in the old editor (a textarea), so the editor was replaced with CodeMirror 6; it now responds in about 1 second (fixed during this check) |
| (h) 200% text size | Needs checking | At the app's largest text size (200%) with Wider line spacing, no text is cut off or overlaps and nothing overflows horizontally (1280×800 and 320×640). Setting only the Windows text size to 200% has not been checked yet |
| (i) Recovery from automatic backup | OK | After editing and killing the process, the next launch offers recovery and restores the file name, encoding, text, and the Unsaved changes state. A normal close removes the copy |
| (j) Loading external files | Needs checking | With "Ask before loading", nothing loads before consent and the notice bar shows the count with Load images. With "Always load", only https images at public addresses load; localhost, 192.168.x.x, and http:// images do not. Pressing Load images and the "Never load" setting have not been checked yet |

Also confirmed: read-aloud can use voices on the PC (such as Microsoft Ayumi), the speed and pause are chosen by the user, and the paragraph being read is shown on screen (Identity 7-3 (3), 7-4); buttons, lists, and checkboxes in Settings are at least 44 px tall (7-3 (1)).

### Design and official assets

The screen design follows the 5garashi.com Design Office Identity v4.20.0 (2026-10-17). The "Theme" button in the toolbar cycles the theme through Auto (follows the Windows app mode), Light, and Dark, and the choice is remembered for the next launch. In Dark, the app uses the night palette (dark mode) of Art. 4-2, and the logotype switches to the gray reversal of Art. 3-7 (the symbol mark at the top right stays red in Dark too, as Art. 3-7 (5) allows). The editor, preview, diagrams, and window title bar follow as well. Printing and PDFs always use the day palette. The official logotype (Japanese and English versions, each with its dark-mode reversal, chosen by the display language) and simple symbol mark are bundled unmodified from the SVG assets of [5garashi/5garashi-identity](https://github.com/5garashi/5garashi-identity). The simple mark is used for the window, the executable, and the Markdown file association icons. The author, copyright, and CC BY-ND 4.0 license stated in each SVG file are preserved. The image size and position examples in samples/diagram-test (section 22) use the same official assets, unmodified (the symbol mark, the logotypes and their reversals, and the banner og_image.png; CC BY-ND 4.0), in samples/import-test-images.

### License

The source code is under the [MIT License](./LICENSE) (Identity Art. 12-1). The logotype and symbol mark in `wwwroot/assets/` are not covered by MIT and are provided under CC BY-ND 4.0. All rights in the name and logos are reserved, and they may not be used in a way that implies official origin, endorsement, or affiliation (Art. 12-2).

`wwwroot/lib/` bundles Mermaid 11.17.2, KaTeX 0.18.9, and CodeMirror 6 (the editor, built with `tools/codemirror`) unmodified under their MIT Licenses, highlight.js 11.12.0 (code colors) under the BSD 3-Clause License, three.js 0.186.1 (3D models) under the MIT License, d3-geo 3.1.1, topojson-client 3.1.0, and world-atlas 2.0.2 (maps; the world outline is Natural Earth, public domain) under the ISC License, and Viz.js 3.31.0 (a WebAssembly build of Graphviz 16.1.0; Graphviz is under the Eclipse Public License 1.0) under the MIT License. All bundled third-party software (Markdig, HtmlSanitizer, the WebView2 SDK, the .NET runtime, the libraries inside Mermaid, and more) and the full license texts are listed in [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md).

### Privacy

The app does not collect or transmit your documents or personal information. Automatic backup copies stay on your PC. Documents with external images connect directly to those image servers. See [PRIVACY.md](./PRIVACY.md) for details.

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-11 JST
