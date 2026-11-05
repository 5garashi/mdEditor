<!--
  ファイル名 / File: docs/translation-review.md
  概要 / Description: 訳を足す・直すときの確認の手順と、レビューの依頼文 / How translations are checked when they are added or changed, and the review request
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-11-01
  版 / Version: 1.0.22
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 訳の確認の手順

画面の文字・README・サンプル・ストア掲載文・CHANGELOG の英語（en）と簡体字中国語（zh-CN）を足す、または直すときの手順です。原文は日本語です。

**このファイルを、依頼文なしで渡された AI へ**: このファイルは手順と依頼文のひな形で、これだけではレビューの対象が決まりません。手順を実行したり、ファイルを変えたりせず、何を見直すかをユーザーに聞いてください。

## 作業をする AI へ: 手順

1. 日本語の原文を書く（直す）。
2. 英語と簡体字中国語に訳す。画面の名前（ボタン・設定・メニュー）は、`wwwroot/app.js` の `messages` と `Strings.cs`・`Strings.Chinese.cs` にある画面の文字をそのまま使う。
3. 機械的な確認を流す: appAutoTester で `run.cmd mdeditor --no-build -k test_translations`（アプリは起動せず、数秒で終わる）。見るもの:
   - 画面の文字の項目が 3 つの言語でそろい、差し込む値（`{0}`、関数の引数）の数も同じか
   - 中国語に、中黒「・」・文中の半角「:」・かな・繁体字が混じっていないか
   - README・ストア掲載文・「主な機能」で「」に入れた画面の名前が、英語と中国語でも画面と同じ名前で書いてあるか
4. 意味と自然さの確認を、ほかの AI に頼む。訳を足した・直したときは必ず頼む。下の依頼文を次のとおり埋めて `local/` の下にファイルとして保存し(git に入らない)、共通の道具で送る(共通ルール AGENTS.md「ほかの AI に見てもらう」): `powershell -NoProfile -ExecutionPolicy Bypass -File C:\5garashi.com.programs\agentRules\tools\Ask-OtherAI.ps1 -From <作業している AI> -RequestFile <依頼文のファイル>`。答えは、依頼文の隣の `.answer.md` に保存される。
   - 「対象の本文」に、今回足した・直した箇所の日本語の原文と訳文を、そのまま貼る。相手がファイル・ブランチ・URL を開かなくても見直せるようにする（push の前でも、ほかの PC の AI でも使える）。長いときは、箇所ごとに分けて貼る。
   - 「対象の本文」の訳文に出てくる画面の名前は、`messages` の画面の文字を「画面の名前」の欄に並べる。
   - 道具がどの AI にも頼めなかったときだけ、依頼文を前後に説明を挟まない 1 つのコードブロックにしてユーザーに示し、「このブロックだけをコピーして渡してください」と添える。説明はブロックの外に書く。
5. 返ってきた指摘を、原文・画面の文字・実際の動き（コード）と照らし合わせ、取り入れるものは作業している AI が判断して直す（ユーザーの確認は待たない）。取り入れなかったものは理由を残す。結果を下の「これまでの確認」に、答えた AI とともに 1 行足し、指摘ごとの答えを報告に書く。
6. 画面の文字を直したときは `test_startup.py` も流す。

## レビューする人・AI へ: 依頼文

````text
あなたの役目は、下の「対象の本文」の訳を見直して、指摘を表で返すことだけです。ファイルを変えたり、テストを動かしたり、手順を実行したりしないでください。見直しに必要なものは、すべてこの依頼文の中にあります。

Windows 用の Markdown エディター「mdEditor」（Microsoft Store で公開中）の翻訳です。
原文は日本語です。英語（en）と簡体字中国語（zh-CN）が原文の意味どおりで、ネイティブの読み手に自然に読めるかを見てください。

■ 対象の本文
<ファイル>（<節・行・キー>）
日本語（原文）:
<原文をそのまま貼る>
英語:
<訳文をそのまま貼る>
簡体字中国語:
<訳文をそのまま貼る。この箇所にないときは「なし」>

■ 画面の名前（訳文の中で、この名前は変えない）
<日本語 / 英語 / 中国語 の組。ないときは「なし」>

■ 見てほしい点
- 誤訳、訳し漏れ、原文にないことの付け足し
- 不自然な言い回し、機械翻訳らしい直訳
- 用語の不統一（上の「画面の名前」と、訳文の中の名前が一致しているか）
- zh-CN の表記（繁体字の混入、句読点が全角か、中国大陸のソフトウェアで普通に使われる用語か）
- 誇張になっていないか（原文が書く「できないこと・制限」が訳で弱まっていないか）、他社の製品名・商標を新たに出していないか

■ 変えないでほしいもの
- Markdown・Mermaid・Graphviz・数式・STL・コードブロックの中身や記法（図の中に表示される文字はレビューの対象）
- 色コード、ファイル名、URL、"Identity 8-9 (8)" のような規則の番号
- 製品名「mdEditor」「5garashi.com」

■ 返してほしい形
| ファイル | 行（または見出し・キー） | 現在の訳 | 修正案 | 理由 | 重さ |
重さ: A＝誤訳・意味が変わる、B＝不自然・用語の不統一、C＝好みの範囲
問題のない箇所は「問題なし」と 1 行で。指摘だけを返してください。
````

## これまでの確認

- 2026-11-01（1.0.20）: 英語と簡体字中国語の全体を、ほかの AI 2 つでレビューした。取り入れなかった指摘: 自動修正のルールの「跨行查找」（オンにすると行をまたいで一致するので、訳のとおり）、掲載文の GBK（簡体字中国語の Windows の既定の文字コードで、実際に判別する）。
- 2026-11-02（1.0.22）: CHANGELOG の 1.0.22 の英語の 2 項目を、ほかの AI でレビューした。指摘 2 件（B）を取り入れた。「new from the clipboard」は、言い換えずに画面の名前「New from clipboard」にそろえた（日本語の原文も「クリップボードから新規作成」にそろえた）。
- 2026-11-02（1.0.22）: 上のレビューで、最初は相手の AI に依頼が伝わらなかった。依頼文が対象の場所だけを書き、push 前のブランチを指していたうえ、このファイルを渡されて手順のほうを始めたため。依頼文に役目の 1 行と「対象の本文」を足し、このファイルの冒頭に、依頼文なしで渡されたときの扱いを書いた。
- 2026-11-02（1.0.22）: 変換に失敗したときのお知らせ 4 つと CHANGELOG の 1 項目を、ほかの AI 2 つでレビューした。取り入れた指摘: 英語の「put in」「text could not be taken」「Saving makes」「left out」「Each case is told」を、inserted・extracted・creates・not inserted・reported に直した。中国語の「取出」を「提取」に、述語の足りない「将整页作为图片」を「将整页转为图片」に直した（同じ書き方のほかのお知らせ 4 つと README も合わせた）。取り入れなかった指摘: 「picture」を「image」にする案（隣のお知らせが picture を使うのでそろえた）、中国語の「放入」を「插入」「导入」にする案（画面の文で 17 か所使っていて、ここだけ変えるとそろわない）、「会成为」を「将生成」にする案（ほかの文の「将成为」にそろえた）、CHANGELOG に中国語の訳を足す案（CHANGELOG は日本語と英語だけで書いている）。
- 2026-11-02（1.0.22）: CHANGELOG の PDF の 2 項目（重ね描きの文字、手順の番号）を、ほかの AI 2 つでレビューした。取り入れた指摘: 「PDF を開くとき」を When opening a PDF に、「少しずらして」を at slightly offset positions に、say を for example に、混ざる様子を interleaved に、「本文」を body text に、「題を上付き文字にした」を caused the heading … to become superscript に。取り入れなかった指摘: to simulate bold（to look bold と意味が同じで、平易なほうを残した）、its corresponding body text（its own body text で意味が通る）。
- 2026-11-02（1.0.22）: 書き込みのある図（画面の文 4 つ、README、CHANGELOG）を、ほかの AI でレビューした。指摘 5 件をすべて取り入れた: 画面の文 d の「(they are with looks like the page)」を「(select "looks like the page" to include them)」に、README と CHANGELOG の「ringed and named」を「circled and labeled」に、CHANGELOG に原文の「枠の中の」（inside the frame）を足し、「when looks come first」「When editing comes first」を、画面の名前どおり「when "looks like the page" is chosen」「When "can be edited" is chosen」にした。
- 2026-11-02（1.0.22）: 同じ箇所を、もう 1 つの AI でもレビューした（上の修正の前の文を見たもの）。新しく取り入れた指摘: 画面の文 c の英語を「redrawn, together with those lines and words, as pictures」に、中国語に「一并」を足した。README の英語を「also become pictures together with those markings」に、CHANGELOG の英語を「the circles and labels were lost」「the notice bar reports this」にした。取り入れなかった指摘: ボタンの「Make tables and figures into pictures」（隣のボタン「Make tables Markdown tables」と同じ形にそろえた）、画面の文 d の「only the image of the figure」（the picture で意味が通る）、中国語の「放入」を「插入」にする案（前回と同じ理由で、画面の文全体の言葉にそろえた）。
- 2026-11-06（1.0.22）: SVG の図の表示（README の 1 段落、CHANGELOG の 1 項目）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語の「loading pictures」を images に、「works」を also takes effect に、「The document」を The original document に、中国語の「与…中一样」を「与在…中一样」に、移除するものの並びを「动画以及加载图像或其他文件的部分」にした。「SVG のまま」の指摘から、PNG は SVG のままではないことに気づき、日本語の原文も「プレビューのとおりに（HTML には SVG のまま）」に直し、訳を合わせた。取り入れなかった指摘: 「drawing parts」（例を続けて書いているので意味が通る）、中国語の「保存为」を「导出为」にする案（画面の名前が「保存为 HTML」など）、今回足していない文の「wait for permission」と「未经你同意」（README のほかの箇所と同じ言葉で、ここだけ変えるとそろわない）、中国語の「图・图片・图像」（絵は图、Word は图片、読み込む物は图像で、指摘の分け方のとおり）。
- 2026-11-06（1.0.22）: HTML の中に文字で書いた数式（README の 1 文、CHANGELOG の 1 項目）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 中国語の「显示…显示为」の重なりを「使用 MathJax 的网页一样渲染为公式」に、「以文字书写」を「以纯文本书写」に、英語の CHANGELOG の「get it as math」を「exports include it as math」に、「showed as plain text」を「stayed as plain text」にした。取り入れなかった指摘: README の英語の「shows as math」を「is rendered as math」にする案（同じ段落の SVG の文が「show as」で、そろえた）。
- 2026-11-06（1.0.22）: ヘッダーの「メニュー」（画面の文字 appMenu、README の項目 4 つ、CHANGELOG の 2 項目）を、ほかの AI（Copilot）でレビューした。項目の全文を見てもらったため、前からある文への指摘もあった。取り入れた指摘: README の英語と中国語で「編集欄だけになり」が訳し漏れていた（A）のを直した。英語の「the Menu」、ボタン名の引用符、「by clearing "Use keyboard shortcuts"」、「lets you adjust」、「kept the next time you start the app」、「You can put it…」、中国語の「关闭“使用键盘快捷键”」「设置在下次启动时仍会保留」「可根据个人需要」「点击」「在浏览器中打开」、CHANGELOG の英語の「so the buttons fit」「use the arrow keys」。取り入れなかった指摘: 中国語の「放入原来的 Markdown」を「嵌入原始 Markdown」にする案（画面の設定の名前に合わせた）、「mark」を「logo/mark」にする案（Identity の用語のシンボルマーク）、「只有需要的人才需开启」（今の文で意味が通る）。
- 2026-11-06（1.0.22）: 「サンプル」をメニューに移した文（README の 1 項目、CHANGELOG の 1 項目）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 中国語の「后两个」が並びの最後の 2 つと読める（A）のを「其中较长的两个」に、「作为新文档放入」を「打开」に、英語の「opens … and puts」を「opens from the Menu …. Choosing one opens」にした。取り入れなかった指摘: 中国語の「顶部栏」を「页眉」にする案（页眉は紙の上端の欄の意味が強く、ほかの 3 項目も顶部栏）、英語の「Diagram and chart test」に display を足す案（画面の名前どおり）、「tour」「logo mark」（意味が通る）、中国語の CHANGELOG（日本語と英語だけで書いている）。
