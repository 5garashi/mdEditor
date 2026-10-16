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

バージョン / Version: 1.0.10（変更の記録 / Changes: [CHANGELOG.md](./CHANGELOG.md)）

## 特長

Markdown を書きながら、整形された仕上がりをすぐ横で確かめられるエディタです。図や数式もそのまま表示し、PDF に書き出せます。書く・読む・残すが、PC の中だけで完結します。

### 書く・見る・書き出す

- **エディタとビューアが一体**: 左で Markdown を書くと、右のプレビューにすぐ反映されます。両側のスクロールが連動し、検索・置換や見出しへのリンクで移動しても同じ場所を表示します。片側だけをウィンドウいっぱいに広げて、エディタやビューアとしても使えます。
- **2 台のディスプレイで、書く画面と見る画面を分けられる**: プレビューだけを別のウィンドウに出し、2 台目のディスプレイで最大化や全画面表示にできます。ウィンドウが分かれても、編集欄とプレビューのスクロールは文書の同じ行でそろいます。広い画面でも本文は読みやすい幅に収まり、表や図は広く表示します。ウィンドウの場所は次回の起動でも使います。
- **図・グラフ・数式・表がそのまま見える**: フローチャート、シーケンス図、ガントチャート、円グラフなどの Mermaid の図、KaTeX の数式、表、チェックリスト、文書内の画像をプレビューに表示します。
- **PDF と HTML に書き出し**: プレビューの見た目のまま、図や数式も含めて PDF に保存します。用紙は A4 と Letter から選べます（初期設定は地域に合わせて自動）。1 つのファイルで完結する HTML にも保存でき、メールで送ったり Web に載せたりできます。
- **いろいろな形式を開いて、保存できる**: Word・HTML・Web ページ・メール・CSV・JSON・Jupyter Notebook・画像を開いて Markdown として編集でき、「名前を付けて保存」で PNG・Word・CSV・JSON・Jupyter Notebook などにも保存できます。
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

**いろいろな形式で保存** — 「名前を付けて保存」で、Markdown・テキスト・PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook を選べます。Word・HTML・CSV・メール・Jupyter Notebook などのファイルも開けます。

![「名前を付けて保存」で、Markdown・テキスト・PDF・HTML・PNG・Word・CSV・TSV・JSON・Jupyter Notebook を選べます。Word・HTML・CSV・メール・Jupyter Notebook などのファイルも開けます。](docs/images/5-save-as.ja.png)

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
- **開く**: Markdown ファイルを選択します。テキスト（.txt）、Word 文書（.docx）、HTML ファイル（.html、.htm）も開けます。テキストはそのまま開いて、そのまま上書き保存できます。Word 文書は Markdown に変換して開き、見出し・リスト・表・リンク・画像が戻ります（色や配置は戻りません）。画像は最初に保存するとき、Markdown ファイルの横に「名前-images」フォルダーとして置きます。Word の文書は書き換えません。HTML ファイル（.html、.htm）も開けます。HTML は Markdown に変換して開き、見出し・段落・リスト・表・リンク・画像・数式が戻ります（色や配置、図の絵は戻りません）。保存すると Markdown ファイルになり、元の HTML ファイルは書き換えません。
  - **メール（.eml）**: 件名を見出しにし、差出人・宛先・日付・添付ファイルの名前と本文を Markdown に変換して開きます。本文の中の画像は Word 文書と同じように扱います。メールの中のインターネット上の画像（受信の確認に使われることがあります）は、確認するまで読み込みません。添付ファイルの中身は取り出しません。
  - **CSV・TSV（.csv、.tsv）**: Markdown の表にして開きます。1 行目を見出しの行にします。Excel で保存した Shift_JIS の CSV も開けます。
  - **JSON（.json）**: 一覧（オブジェクトの配列）は表にして開き、項目の名前を見出しの行にします。それ以外の JSON はコードの枠に入れて開くので、前後に説明を書き足せます。
  - **XML（.xml）**: コードの枠に入れて開きます。前後に説明を書き足せます。
  - **Jupyter Notebook（.ipynb）**: Markdown のセルはそのまま、コードのセルは言語付きのコードの枠（```python など）になります。実行結果は、文字は「output」の枠、画像と表（pandas など）はそのまま表示します。セルに貼り付けた画像も表示します。
  - **画像（.png、.jpg、.gif、.bmp、.webp）**: その画像を表示する新しい文書として開きます。画像の下に説明を書き足して、Markdown ファイルとして保存できます。別のフォルダーに保存したときは、画像もそこに写します。
- **画像を文書に入れる**: 画像ファイルを編集画面にドラッグ＆ドロップするか、画面の切り取りなどを編集画面に貼り付ける（Ctrl+V）と、文書の横の「名前-images」フォルダーに画像を置き、カーソルの位置に `![](名前-images/画像.png)` を入れます。文書のフォルダーの中にある画像は、写さずにそのまま使います。まだ保存していない文書では、先に保存先を聞きます。編集画面以外にドロップした画像は、画像の文書として開きます。
- **URLを開く / Ctrl+Shift+K**: `https://` で始まる Web ページを、Markdown に変換して開きます。ブラウザーからリンクをドラッグして開くこともできます。ページのスクリプトは実行しないので、開いたあとに読み込まれる内容（ランキングの数字など）は入りません。
- **インターネット上の画像**: 初期設定では読み込まず、お知らせ欄の「画像を読み込む」で読み込みます（その文書だけ）。「設定」→「インターネット」で「いつも読み込む」「読み込まない」も選べます。`http://` の画像は読み込みません。手元にない画像やリンクは、ページの元のアドレス（`<base>`、canonical、og:url）から探します。
- **ドラッグ&ドロップ**: `.md`・`.markdown`・`.html`・`.htm` のファイルをアプリ画面へ1つドロップして開きます。未保存の変更がある場合は、開く前に確認します。
- **保存**: 現在のファイルに上書きします。新規文書は保存先を選びます。
- **名前を付けて保存**: 保存先とファイル名を選びます。「開く」と同じように、ファイルの種類で保存の形式を選べます。
  - **Markdown（.md）・テキスト（そのまま）（.txt）**: 文書そのものを保存します（中身はそのまま）。以後の「保存」はこのファイルに上書きします。
  - **テキスト（記号なし）・PDF・HTML・PNG・Word（.docx）・CSV・TSV・JSON・Jupyter Notebook**: 文書を書き出します。編集中の文書のファイルはそのままです。書き出しの内容は下のとおりです。
- **テキスト（記号なし）**: プレビューに見えている文字だけを .txt にします。見出しや太字の記号は消え、行は書いたとおりに改行します。リストは「・」か番号、表はタブ区切り、リンクは「文字（アドレス）」、図は「［図］」になります。
- **CSV・TSV**: 文書の中の表を CSV（カンマ区切り）か TSV（タブ区切り）にします。表が 2 つ以上あるときは「名前-1.csv」「名前-2.csv」…に分けて保存します。Excel で文字化けしないよう、UTF-8（BOM 付き）で保存します。
- **JSON**: 文書の中の表を、見出しの行を項目の名前にしたオブジェクトの配列にします（UTF-8）。値は文字のまま書き出すので、「007」のような番号の 0 も消えません。表が 2 つ以上あるときは「名前-1.json」…に分けて保存します。
- **Jupyter Notebook**: 言語付きのコードの枠をコードのセルに、それ以外を Markdown のセルにします。実行結果は入れません（Jupyter で実行すると作り直されます）。.ipynb を開いて編集した文書を保存し直すと、元のノートブックと同じセルの並びに戻ります。
- **Ctrl+O**: ファイルを開きます。
- **Ctrl+S**: 保存します。
- **Ctrl+Shift+S**: 名前を付けて保存します。
- **PDF**: 整形したプレビューをPDFに書き出します。
- **PNG / Ctrl+Shift+G**: プレビューを昼の配色で画像（PNG）にします。長い文書は「名前-1.png」「名前-2.png」…に分けて保存します。
- **Word / Ctrl+Shift+D**: Word 文書（.docx）に書き出します。見出し・段落・リスト・引用・コード・表・リンク・画像は Word の標準のスタイルになるので、Word のナビゲーションや目次がそのまま使えます。図と数式はプレビューの画像として入ります（Word では編集できません）。
- **HTML / Ctrl+Shift+M**: 整形したプレビューを、1 つで完結する HTML ファイルに書き出します。図は SVG、数式は MathML で入り、文書と同じフォルダーの画像はファイルの中に埋め込みます。ブラウザーで開くと、ダークモードなら夜の配色で表示します。開いても外部に通信しません（文書の中のネット上の画像だけは、その画像のサーバーから読み込みます）。
- **用紙**: PDFの用紙を「自動」→「A4」→「Letter」の順に切り替えます。「自動」は Windows の地域の設定に従います（ヤード・ポンド法の地域は Letter、それ以外は A4）。選んだ用紙は次回の起動でも使います。
- **文字コード**: ファイル名の横に文書の文字コードを表示します。押すと文字コードを選び、「この文字コードで開き直す」または「次の保存からこの文字コードにする」を選べます。
- **Ctrl+Shift+P**: PDFに保存します。
- **Ctrl+H**: 編集画面の検索・置換バーを開きます。「置換」「すべて置換」が使えます（`Enter` で次を検索、`Esc` で閉じる）。
- **書き方 / F1**: Markdown の書き方を「基本」「図（Mermaid）」「数式（KaTeX）」に分けて一覧で表示します。図はフローチャート・シーケンス図・ガントチャート・円グラフ・クラス図・状態遷移図・ER 図・マインドマップ、数式は分数・平方根・総和・行列・場合分けなど 10 種類で、図は一覧の中に描いて見せます。「挿入」で編集欄のカーソル位置に例を入れます（`Esc` で閉じる）。
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

見出し、リスト、チェックリスト、引用、表（左・中央・右寄せ）、コード、リンク、画像などのMarkdownを表示します。セキュリティのため、Markdown内の生HTMLは文字として表示し（属性のない `<br>` だけは改行として扱います。表のセルの中で改行するときに使えます）、プレビュー内容をサニタイズします。HTMLコメント（`<!-- -->`）は表示しません。Webリンクは既定のブラウザー、メールリンクは既定のメールアプリで開きます。PDFには整形済みのプレビューのみを出力し、編集中のテキスト欄や操作ボタンは含めません。

`images/a.png` のような相対パスの画像は、文書と同じフォルダー（とその中のフォルダー）から読み込みます。読み込むのは画像ファイル（PNG、JPEG、GIF、WebP、AVIF、BMP、ICO、SVG）だけで、文書のフォルダーより上の階層は読みません。

文書の文字コードは、UTF-8（BOMの有無）、UTF-16、Windowsの既定の文字コード（日本語環境ではShift_JIS）を自動で判別し、保存するときも元の文字コードのまま書き込みます。元の文字コードで保存できない文字がある場合は、UTF-8で保存するか確認します。UTF-8 ではないファイルを推定した文字コードで開いたときや、読めない文字（�）があるときは、ツールバーの下にお知らせを表示します。読めない文字があるまま上書き保存しようとすると、元のファイルのその部分が失われることを確認します。EUC-JP や ISO-2022-JP のファイルは、文字コードを選んで開き直してください。保存は一時ファイルに書いてから置き換えるので、途中で失敗しても元の文書は壊れません。

` ```mermaid ` のコードブロックは [Mermaid](https://mermaid.js.org/) で図として表示します（フローチャート、シーケンス図、クラス図、状態遷移図、ER図、ガントチャート、円グラフなど）。`$...$` と `$$...$$` の数式は [KaTeX](https://katex.org/) で表示します。どちらもアプリに同梱しているため、オフラインでも表示でき、PDFにも出力されます。図の構文に誤りがある場合は、エラーメッセージと元のコードを表示します。表示の確認には [samples/diagram-test.md](./samples/diagram-test.md)（英語版: [samples/diagram-test.en.md](./samples/diagram-test.en.md)）を使えます。

### 対応している図・チャート

コードブロックの言語名によって、表示のしかたが次のように変わります。

```mermaid
flowchart LR
    src["コードブロック・数式"] --> kind{"記法"}
    kind -- "mermaid" --> mermaid["図として表示<br/>Mermaid 11.17.2"]
    kind -- "数式 (TeX)" --> math["数式として表示<br/>KaTeX 0.18.9"]
    kind -- "dot・plantuml・nomnoml など" --> code["コードブロックのまま表示"]

    classDef supported fill:#d4f7d4,stroke:#2a7a2a,color:#1a1a1a;
    classDef unsupported fill:#eeeeee,stroke:#777777,color:#1a1a1a;
    class mermaid,math supported;
    class code unsupported;
```

| 種類 | 記法 | 表示 | 補足 |
| :--- | :--- | :---: | :--- |
| フローチャート | ` ```mermaid ` + `flowchart` / `graph` | ✅ | |
| シーケンス図 | `sequenceDiagram` | ✅ | |
| クラス図 | `classDiagram` | ✅ | |
| 状態遷移図 | `stateDiagram-v2` | ✅ | |
| ER図 | `erDiagram` | ✅ | |
| ガントチャート | `gantt` | ✅ | |
| 円グラフ | `pie` | ✅ | |
| 棒グラフ・折れ線グラフ | `xychart-beta` | ✅ | 軸の日本語ラベルは `"1月"` のように `"..."` で囲みます |
| 4象限チャート | `quadrantChart` | ✅ | |
| ユーザージャーニー | `journey` | ✅ | |
| Git グラフ | `gitGraph` | ✅ | |
| マインドマップ | `mindmap` | ✅ | |
| タイムライン | `timeline` | ✅ | |
| サンキーダイアグラム | `sankey-beta` | ✅ | ノード名は英数字のみ（Mermaid の制限で、日本語は使えません） |
| 数式 | `$...$`（インライン）、`$$...$$`（ブロック） | ✅ | KaTeX で表示 |
| Graphviz | ` ```dot ` | ❌ | コードブロックとして表示 |
| PlantUML | ` ```plantuml ` | ❌ | コードブロックとして表示 |
| nomnoml | ` ```nomnoml ` | ❌ | コードブロックとして表示 |

上の表で ✅ の図は、テスト文書で表示を確認済みです。これ以外の Mermaid の図（`block-beta` など）も Mermaid 11.17.2 が対応していれば表示されますが、確認はしていません。

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

画面デザインは5garashi.com Design Office Identity v4.14.0（2026-10-15）に基づきます。ツールバーの「表示」ボタンで、表示テーマを「自動」（Windows の「アプリ モード」に合わせる）→「ライト」→「ダーク」の順に切り替えられます。選んだテーマは次回の起動でも使います。ダークでは第4条 4-2 の夜の配色（ダークモード）で表示し、ロゴタイプは第3条 3-7 の灰色抜きの反転版に切り替わります（右上のシンボルマークは、3-7 (5) によりダークでも赤のままです）。編集欄・プレビュー・図もダーク表示になり、ウィンドウのタイトルバーも合わせます。印刷とPDFは常に昼の配色です。公式ロゴタイプ（日本語版・英語版と、それぞれのダークモード用の反転版。表示言語に合わせて切り替えます）とシンプル版マークは [5garashi/5garashi-identity](https://github.com/5garashi/5garashi-identity) のSVG素材を改変せずに同梱し、アプリのウィンドウ・実行ファイル・Markdown関連付けのアイコンにはシンプル版マークを使用します。各SVGファイルに記載された作者、著作権、CC BY-ND 4.0 ライセンスを保持しています。

## ライセンス

ソースコードは [MIT License](./LICENSE) です（Identity 第12-1条）。`wwwroot/assets/` のロゴタイプとシンボルマークは MIT の対象外で、CC BY-ND 4.0 で提供されます。名称とロゴに関する権利は留保され、公式の提供物・承認・提携と誤認される使い方はできません（第12-2条）。

`wwwroot/lib/` には、それぞれの MIT License のもとで Mermaid 11.17.2、KaTeX 0.18.9、CodeMirror 6（編集欄。`tools/codemirror` でビルド）を改変せずに同梱しています。同梱しているすべての第三者ソフトウェア（Markdig、HtmlSanitizer、WebView2 SDK、.NET ランタイム、Mermaid に含まれるライブラリなど）とライセンス本文は [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md) にあります。

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
- **Open and save many formats**: Word, HTML, web pages, e-mail, CSV, JSON, Jupyter Notebooks, and images open as Markdown, and Save As also writes PNG, Word, CSV, JSON, Jupyter Notebook, and more.
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

**Save in many formats** — Save As writes Markdown, text, PDF, HTML, PNG, Word, CSV, TSV, JSON, or Jupyter Notebook. Word, HTML, CSV, e-mail, Jupyter Notebook, and more open too.

![Save As writes Markdown, text, PDF, HTML, PNG, Word, CSV, TSV, JSON, or Jupyter Notebook. Word, HTML, CSV, e-mail, Jupyter Notebook, and more open too.](docs/images/5-save-as.en.png)

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
- **Open**: choose a Markdown file. Text (.txt), Word (.docx), and HTML (.html, .htm) files open too. Text opens and saves back as it is. A Word document opens as Markdown: headings, lists, tables, links, and images come back (colors and layout do not); its images are placed beside the Markdown file in a "name-images" folder on the first save, and the Word document is never written. HTML files (.html, .htm) open too: they are converted to Markdown, and headings, paragraphs, lists, tables, links, images, and math come back (colors, layout, and drawn diagrams do not). Saving makes a Markdown file and never writes the HTML file.
  - **E-mail (.eml)**: opens as Markdown with the subject as the heading, then the sender, recipients, date, attachment names, and the body. Pictures in the body are handled like a Word document's. Internet images in the mail (sometimes used to track opening) do not load until you allow them. Attachments are not extracted.
  - **CSV, TSV (.csv, .tsv)**: opens as a Markdown table with the first row as the header. Shift_JIS CSV files saved by Excel open too.
  - **JSON (.json)**: a list (an array of objects) opens as a table with the field names as the header row. Other JSON opens in a code block, so you can add notes before and after it.
  - **XML (.xml)**: opens in a code block, with room for notes before and after it.
  - **Jupyter Notebook (.ipynb)**: Markdown cells stay as they are and code cells become code blocks with their language (```python and so on). Text outputs become "output" blocks; pictures and tables (such as pandas) show as they are, and so do pictures pasted into cells.
  - **Images (.png, .jpg, .gif, .bmp, .webp)**: open as a new document that shows the image. Write a description below it and save it as Markdown; saving in another folder copies the image there too.
- **Insert images**: drag image files onto the editor, or paste a picture such as a screen capture into it (Ctrl+V). The images go in a "name-images" folder beside the document, and `![](name-images/image.png)` is inserted at the cursor. Images already inside the document's folder are used where they are. A document that has not been saved asks where to save first. An image dropped anywhere other than the editor opens as an image document.
- **Open URL / Ctrl+Shift+K**: opens a web page that starts with `https://` as Markdown. You can also drag a link in from a browser. The page's scripts never run, so content it loads afterwards (such as live rankings) is not included.
- **Images on the internet**: not loaded by default; Load images in the notice bar loads them for that document. Settings > Internet can also be Always load or Never load. Images over `http://` are never loaded. Images and links not found beside the file resolve from the page's own address (`<base>`, canonical, or og:url).
- **Drag and drop**: drop one `.md`, `.markdown`, `.html`, or `.htm` file onto the window to open it. If there are unsaved changes, you are asked before it opens.
- **Save**: overwrite the current file. A new document asks where to save.
- **Save As**: choose a location and file name. As with Open, the file type chooses the format.
  - **Markdown (.md), text as written (.txt)**: save the document itself (the content as it is). Save then writes to this file.
  - **Text without marks, PDF, HTML, PNG, Word (.docx), CSV, TSV, JSON, Jupyter Notebook**: export the document; the file you are editing stays as it was. Each export is described below.
- **Text without marks**: only the words the preview shows, as a .txt file. Heading and emphasis marks are removed and lines break where you wrote them. Lists use a bullet or number, tables are tab-separated, links become "text (address)", and diagrams become "[Figure]".
- **CSV, TSV**: the tables in the document as CSV (comma-separated) or TSV (tab-separated). Two or more tables are saved as name-1.csv, name-2.csv, and so on. Files are UTF-8 with a BOM so Excel shows Japanese correctly.
- **JSON**: the tables in the document as arrays of objects keyed by the header row (UTF-8). Values are written as text, so numbers such as 007 keep their zeros. Two or more tables are saved as name-1.json and so on.
- **Jupyter Notebook**: code blocks with a language become code cells and everything else Markdown cells. Outputs are not included (Jupyter makes them again when run). A document opened from an .ipynb file saves back to the same cells.
- **Ctrl+O**: open a file.
- **Ctrl+S**: save.
- **Ctrl+Shift+S**: save as.
- **PDF**: export the rendered preview as a PDF.
- **PNG / Ctrl+Shift+G**: the preview as pictures (PNG) in the day palette; a long document is split into name-1.png, name-2.png, and so on.
- **Word / Ctrl+Shift+D**: export a Word document (.docx). Headings, paragraphs, lists, quotes, code, tables, links, and images use Word's own styles, so Word's navigation pane and table of contents work. Diagrams and math go in as pictures from the preview (not editable in Word).
- **HTML / Ctrl+Shift+M**: export the rendered preview as one self-contained HTML file. Diagrams are SVG, math is MathML, and images beside the document are embedded. In a browser it switches to the night palette in dark mode. Opening it makes no connections, except that web images in the document load from their servers.
- **Paper**: cycle the PDF paper through Auto, A4, and Letter. Auto follows the Windows region (Letter where US customary units are used, A4 elsewhere). The choice is remembered.
- **Encoding**: the document's encoding is shown next to the file name. Press it to choose an encoding and either reopen the file in it or use it from the next save.
- **Ctrl+Shift+P**: save as PDF.
- **Ctrl+H**: open the editor's find/replace bar with Replace and Replace all (`Enter` finds next, `Esc` closes).
- **Syntax / F1**: show a Markdown quick reference grouped into Basics, Diagrams (Mermaid), and Math (KaTeX): flowchart, sequence, Gantt, pie, class, state, ER, and mind map diagrams (drawn in the list), and ten math examples such as fractions, roots, sums, matrices, and cases. Insert puts an example at the cursor in the editor (`Esc` closes).
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

Headings, lists, task lists, quotes, tables (left, center, and right alignment), code, links, and images are rendered. For security, raw HTML inside Markdown is shown as text (except a bare `<br>`, which breaks the line, for example inside a table cell) and the preview content is sanitized. HTML comments (`<!-- -->`) are not shown. Web links open in the default browser and mail links in the default mail app. The PDF contains only the rendered preview, not the editing text area or buttons.

Relative images such as `images/a.png` load from the document's folder and its subfolders. Only image files (PNG, JPEG, GIF, WebP, AVIF, BMP, ICO, SVG) are read, and nothing above the document's folder.

The text encoding is detected automatically: UTF-8 (with or without BOM), UTF-16, or the Windows default code page (Shift_JIS on Japanese Windows). Saving keeps the original encoding; if some characters cannot be saved in it, the app asks whether to save as UTF-8. A notice appears under the toolbar when a non-UTF-8 file was opened with a guessed encoding or when some bytes could not be read (shown as �); overwriting such a file asks for confirmation first, because those parts of the original would be lost. For EUC-JP or ISO-2022-JP files, choose the encoding and reopen the file. Saves are written to a temporary file and then swapped in, so a failure part-way never damages the original document.

` ```mermaid ` code blocks are drawn as diagrams with [Mermaid](https://mermaid.js.org/) (flowcharts, sequence, class, state, ER, Gantt, pie charts, and more). Math in `$...$` and `$$...$$` is rendered with [KaTeX](https://katex.org/). Both are bundled with the app, so they work offline and are included in PDFs. A diagram with a syntax error shows the error message and its source. Use [samples/diagram-test.en.md](./samples/diagram-test.en.md) (Japanese: [samples/diagram-test.md](./samples/diagram-test.md)) to check the rendering.

#### Supported diagrams and charts

How a block is rendered depends on the language name of the code block.

```mermaid
flowchart LR
    src["Code block or math"] --> kind{"Notation"}
    kind -- "mermaid" --> mermaid["Drawn as a diagram<br/>Mermaid 11.17.2"]
    kind -- "Math (TeX)" --> math["Rendered as math<br/>KaTeX 0.18.9"]
    kind -- "dot, plantuml, nomnoml, etc." --> code["Shown as a code block"]

    classDef supported fill:#d4f7d4,stroke:#2a7a2a,color:#1a1a1a;
    classDef unsupported fill:#eeeeee,stroke:#777777,color:#1a1a1a;
    class mermaid,math supported;
    class code unsupported;
```

| Type | Syntax | Rendered | Notes |
| :--- | :--- | :---: | :--- |
| Flowchart | ` ```mermaid ` + `flowchart` / `graph` | ✅ | |
| Sequence diagram | `sequenceDiagram` | ✅ | |
| Class diagram | `classDiagram` | ✅ | |
| State diagram | `stateDiagram-v2` | ✅ | |
| ER diagram | `erDiagram` | ✅ | |
| Gantt chart | `gantt` | ✅ | |
| Pie chart | `pie` | ✅ | |
| Bar and line chart | `xychart-beta` | ✅ | Quote non-ASCII axis labels, such as `"1月"` |
| Quadrant chart | `quadrantChart` | ✅ | |
| User journey | `journey` | ✅ | |
| Git graph | `gitGraph` | ✅ | |
| Mind map | `mindmap` | ✅ | |
| Timeline | `timeline` | ✅ | |
| Sankey diagram | `sankey-beta` | ✅ | ASCII node names only (a Mermaid limitation) |
| Math | `$...$` (inline), `$$...$$` (display) | ✅ | Rendered with KaTeX |
| Graphviz | ` ```dot ` | ❌ | Shown as a code block |
| PlantUML | ` ```plantuml ` | ❌ | Shown as a code block |
| nomnoml | ` ```nomnoml ` | ❌ | Shown as a code block |

Everything marked ✅ is verified with the test documents. Other Mermaid diagrams (such as `block-beta`) render if Mermaid 11.17.2 supports them, but they have not been checked.

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

The screen design follows the 5garashi.com Design Office Identity v4.14.0 (2026-10-15). The "Theme" button in the toolbar cycles the theme through Auto (follows the Windows app mode), Light, and Dark, and the choice is remembered for the next launch. In Dark, the app uses the night palette (dark mode) of Art. 4-2, and the logotype switches to the gray reversal of Art. 3-7 (the symbol mark at the top right stays red in Dark too, as Art. 3-7 (5) allows). The editor, preview, diagrams, and window title bar follow as well. Printing and PDFs always use the day palette. The official logotype (Japanese and English versions, each with its dark-mode reversal, chosen by the display language) and simple symbol mark are bundled unmodified from the SVG assets of [5garashi/5garashi-identity](https://github.com/5garashi/5garashi-identity). The simple mark is used for the window, the executable, and the Markdown file association icons. The author, copyright, and CC BY-ND 4.0 license stated in each SVG file are preserved.

### License

The source code is under the [MIT License](./LICENSE) (Identity Art. 12-1). The logotype and symbol mark in `wwwroot/assets/` are not covered by MIT and are provided under CC BY-ND 4.0. All rights in the name and logos are reserved, and they may not be used in a way that implies official origin, endorsement, or affiliation (Art. 12-2).

`wwwroot/lib/` bundles Mermaid 11.17.2, KaTeX 0.18.9, and CodeMirror 6 (the editor, built with `tools/codemirror`) unmodified under their MIT Licenses. All bundled third-party software (Markdig, HtmlSanitizer, the WebView2 SDK, the .NET runtime, the libraries inside Mermaid, and more) and the full license texts are listed in [THIRD-PARTY-NOTICES.md](./THIRD-PARTY-NOTICES.md).

### Privacy

The app does not collect or transmit your documents or personal information. Automatic backup copies stay on your PC. Documents with external images connect directly to those image servers. See [PRIVACY.md](./PRIVACY.md) for details.

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-11 JST
