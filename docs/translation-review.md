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
- 2026-11-06（1.0.22）: ウィンドウ全体に表示したときのファイル名（README の 1 項目、CHANGELOG の 1 項目）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語の「covered then」を「hidden in this mode」に、CHANGELOG の括弧内を「because the header, which normally shows it, is hidden」に、中国語の「被遮住」を「会被隐藏」にした。前からある文の「並列表示に戻ります」の訳し漏れ（B）を、英語「to return to the side-by-side view」、中国語「即可返回并排显示」で直し、ボタン名「Fill window」に引用符を付けた。指摘とは別に、依頼文で header を「标题栏」と書いたのが誤りで、README のほかの項目では header は「顶部栏」、欄の見出しが「标题栏」なので、それにそろえた。取り入れなかった指摘: 「the editor or the preview」の冠詞（どちらでも正しい）、「扩展到填满」（好みの範囲）。
- 2026-11-06（1.0.22）: 閉じる `$` の前に空白がある数式（README の表の 1 行と「設定」の 1 文、CHANGELOG の 1 項目）を、ほかの AI（Copilot）でレビューした。指摘は 4 件（すべて C）。取り入れた指摘: 中国語の「结尾的 `$` 前有空格的 `$x $`」の「的」の重なりを「闭合的 `$` 前有空格时（`$x $`）」にした。取り入れなかった指摘: 今回足していない文の「暂时不能更改」「meanwhile」（GitHub に合わせている間だけ選べないという意味で、今の文のとおり）、「选择一致时」に画面の名前を繰り返す案（直前の文に画面の名前があり、意味が通る）。
- 2026-11-07（1.0.22）: 画面の画像の説明 2 つ（README の「ダークモード」「プレビューを別のウィンドウに」、Store のスクリーンショットの説明 5・6）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: Store の説明 5 の英語「follow Windows」に、原文の「設定」を足して「follow your Windows setting」にした（B）。取り入れなかった指摘: 英語の「Theme」「Settings」「Menu」に引用符や太字を付ける案（README と Store のほかの説明が付けずに書いている）、中国語の「标题栏」が窓のタイトルバーと読める恐れ（README の「朗読」などの項目で、欄の見出しの行を「标题栏」と書いていて、それにそろえた）。
- 2026-11-07（1.0.22）: Store の掲載文の「このバージョンの新機能」（1.0.21 から 1.0.22 への更新のために書き直した 8 項目）と、説明に足した 4 か所（「メニュー」から開くこと、SVG の図、一部を変換できないとき）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語の「Scripts and outside files are removed」を、原文の「読み込み」のとおり「the loading of outside files」に（B）、中国語の「如图的说明」が「図のとおり」と読めるのを「例如图的说明」に（B）、「中…中」の重なりを「可以从应用“菜单”中的“关于此应用”打开…」に（B）、「去除」を「移除」に、「重复绘制」を「重叠绘制」に、新機能 2 行目の繰り返しを「也是如此」に、SVG の文の並列を「，以及…，也能显示」にした。取り入れなかった指摘: 中国語の「以纯文本书写」（2026-11-06 のレビューで「以文字书写」から直した言葉で、README と CHANGELOG にそろえた）、「放入」を「导入」にする案（画面の文全体で「放入」を使っている）、英語の「drawn several times」を「drawn over itself」にする案（CHANGELOG の「drawn several times at slightly offset positions」にそろえた）。
- 2026-11-08（1.0.23）: 「GitHub 変換」「GitHub 表示」（画面の文字、README の節、CHANGELOG の 3 項目、PRIVACY.md の追加の文）を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語の「anything to keep as it is」を「any item you want to keep as it is」に、列の見出し「Written as」を「Syntax」に、「while the question was open」を「while this dialog was open」に、PRIVACY の「in its question」を「in the confirmation」に、「back to … at」を「resets at」に、「handles it」を「handles this information」に、帯の「GitHub read the Markdown」を「GitHub interprets the Markdown; mdEditor draws …」に、回数切れの文を「You have used all the sends allowed per hour」に、エラーの文のボタン名に引用符を付けた。README・CHANGELOG・PRIVACY の「how many sends are left」を、原文の「送れる回数」のとおり「allowed」に、README の末尾を「Anything GitHub has no equivalent for … is left unchanged」に、PRIVACY の「To send nothing」を「If you do not want to send anything」にした。中国語は、ボタンの説明を「将在 GitHub 上无法按原样显示的写法，改写为 GitHub 支持的写法」に、確認の文に「会」を、「将于 … 恢复」「从 … 起即可再次发送」にした。CHANGELOG の日本語の `\begin` の `\` が制御文字になっていた（A）のを直し、変えたファイルに同じ制御文字がないことを確かめた。取り入れなかった指摘: README の英語と中国語の見出しを `###` にする案（日本語の README は「Markdown とプレビュー」の下の節だが、英語と中国語の README は隣の「GitHub と同じ書き方」が `##` で、それにそろえた）、中国語の「容器」と「提示框」の統一（`:::` の囲みと GitHub の注意書きは別のもので、分けて書いた）、PRIVACY の「whether documents show closer to GitHub」（今回足していない文）。
- 2026-11-08（1.0.23）: 帯の文の言い直し、設定の「表示するボタン」の欄、README の GitHub 表示の段落、CHANGELOG の 1.0.23 の項目を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 中国語の帯と README の「按照…的解读」「使用…的」を「由 GitHub 决定」「则是 mdEditor 自己的」「则由 mdEditor 负责」に、設定の注の「用…使用」の重なりを「仍可通过“按键操作”中设置的按键使用」に、英語の注を「can still be used with their keys」に、README の英語の「Back to the preview」をボタンとして読める文に、「Don't send is chosen first」を「"Don't send" is selected by default」に、CHANGELOG の英語の「above the editor / preview」を「to the editor / preview」に、「since」を「since then」に、「Pressing it again」を「Pressing GitHub view again」に、「in Japanese」を「in the Japanese UI」にした。取り入れなかった指摘: 「Menu」を小文字にする案（画面の名前が Menu）、設定の英語のボタン名に引用符を付ける案（隣の「Show the Convert for GitHub and GitHub view buttons」など、設定の文は付けずに書いている）、「allowed」を「remain」にする案（前回のレビューで原文の「送れる回数」に合わせた）。
- 2026-11-08（1.0.23）: README の「設定」に足した「表示するボタン」の 1 項目を、ほかの AI（Copilot）でレビューした。指摘 2 件（どちらも C）。取り入れた指摘: 英語の「their keys」を「their shortcut keys」にした。取り入れなかった指摘: 中国語の「中设置的按键」から「设置」を外す案（同じ意味の画面の文を、前のレビューでこの言い方にそろえた）。
- 2026-11-08（1.0.23）: 変換の書き方の名前 2 つ（日本語の文字のすぐ後の数式、HTML の中の数式）と、README・CHANGELOG に足した文を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語と中国語の「Japanese or Chinese」「中文或日文」が原文にない（A）との指摘は、GitHub の動きが ASCII 以外の文字すべてに当てはまるため、日本語の原文を「日本語や中国語の文字」に直して合わせた。CHANGELOG の英語にだけあった「`$x$` は直前が空白か ASCII の記号でないと数式にならない」の例（A）を日本語にも足し、英語の「not right after 、 or 。」を外した。README の英語の条件を「only when the `$` is preceded by a space or ASCII punctuation」に、CHANGELOG の英語の「What GitHub has nothing like」を README と同じ「Anything GitHub has no equivalent for」に、中国語の「只显示数量」を「只列出数量」に、係り方があいまいな「引用、列表或容器中的…」の前に「在」を足した。取り入れなかった指摘: README の「表の後の文」の原文が合わないという指摘（依頼文に貼った日本語の段落が誤っていたためで、ファイルの原文と訳は対応している）。
- 2026-11-08（1.0.23）: GitHub 表示の帯の文（前半と後半に分け、表示の間の編集は自動では反映されないことを足した）と、CHANGELOG の人名の頭文字の項目を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 英語の帯の「Edits made while it is shown do not update it」を「Edits made while this view is shown are not reflected automatically … to refresh it」に、中国語の「显示期间进行编辑，此显示也不会」の重なりと「也」を「在此显示期间编辑内容，显示结果不会自动更新」に、CHANGELOG の英語に「numbered」と「single … only one space」を足した。取り入れなかった指摘: 「made by GitHub」を「rendered by GitHub」にする案（README と CHANGELOG の「what GitHub made of it」にそろえた）。
- 2026-11-09（1.0.23）: 別行の `\[ … \]` のルールを開いたときに直すようにした CHANGELOG の 1 項目を、ほかの AI（Copilot）でレビューした。取り入れた指摘: 「The notice says so, and its button puts the places back」を「The notice shows what was fixed, and its "Put back" button undoes the change」に（B、画面の名前にそろえた）、「An earlier version's file does not say whether that was chosen」を「Settings saved by an earlier version do not show whether you chose "Ask on opening" yourself」に（B）、「is kept as it is」を「is left unchanged」にした。取り入れなかった指摘: ルールの名前に画面の名前の「(math on GitHub too)」まで書く案（日本語の原文も括弧の部分を省いて書いている）。
- 2026-11-09（1.0.23）: 「GitHub 変換」の新しい種類（GitHub が見出しや一覧として読む行を含む `$$…$$`）の画面の文字・README の表の 1 行・CHANGELOG の 2 か所と、自動修正のルールの名前から「（GitHub でも数式になる）」を外したことを、ほかの AI（Copilot）でレビューした。取り入れた指摘: 中国語の「会当作标题、列表等读取的行」を「会识别为标题、列表等的行」に（B、画面の文字と README の両方）、CHANGELOG の英語の「the math was lost」を、原文の「数式にならなかった」のとおり「the math did not render」にした。取り入れなかった指摘: 「the lines above it」を単数にする案（GitHub は `=` だけの行より上の段落の行をまとめて 1 つの見出しにする。`$$` の行も見出しに入る）。
- 2026-11-09（1.0.23）: 自動修正のルールを貼り付けたときにも当てるようにした README の 1 項目と CHANGELOG の 1 項目を、ほかの AI（Copilot）でレビューした。取り入れた指摘: README の英語を「when a document is opened or text is pasted」に（B）、CHANGELOG の英語の「inside」を「between `\[` and `\]`」に（B）、「what was fixed before」を「anything fixed earlier」にした。「the lines above it」は原文にない補足との指摘（B）は、見出しになるのは実際に上の行なので英語を残し、日本語の原文に「その上の行が」を足して合わせた。取り入れなかった指摘: 「ways of writing」を「writing patterns」にする案（README のほかの箇所が「ways of writing」）、中国語の「粘贴文字」を「粘贴文本」にする案（好みの範囲で、README のほかの箇所も「文字」）、「The notice」を「The notice area」にする案（画面の名前どおり）。
- 2026-11-09（1.0.23）: 「GitHub 変換」を「自動修正」のボタン（「GitHub 変換…」と「自動修正のルールを当てる」のメニュー）に変えたこと、貼り付けのたびに当てる設定、落としたファイルの中身にも当てること（画面の文字 10 個、README の 3 か所、CHANGELOG の 2 項目）を、ほかの AI（Copilot）でレビューした。取り入れた指摘（B）: 設定の注の英語の主語がなく「They」が何を指すか分からないのを「the fix rules correct …」に、中国語に「会」と「这些规则也会」を足し、末尾に目的語「这些规则」を足した。README と CHANGELOG の英語の無生物主語「a file dropped on the editor puts its content in」を「you drop a file on the editor to insert its content」に、「what was put in」を「what was inserted」に、CHANGELOG の「rules set to "Fix on opening" fix」を「fix the document」に、中国語の README の「拖放到编辑区放入」を「拖放到编辑区、载入」に、「放入的部分」を「插入的部分」にした。取り入れなかった指摘: ツールチップを命令形にする案（ほかのボタンのツールチップが三人称で書いている）、お知らせのボタン名の引用符を外す案（お知らせの文はボタン名に引用符を付けてそろえている）、「put back」を「restored」に、中国語の「恢复原样」を「撤销修正」にする案（お知らせ欄のボタンの名前が「Put back」「恢复原样」）、README の「修改」を「修正」にする案（今回足していない部分）。
- 2026-11-10（1.0.23）: Store の掲載文（「静かで、安心」の送らない文の例外、「製品の機能」の GitHub の 1 項目、1.0.22 と 1.0.23 の「このバージョンの新機能」8 項目）と、PRIVACY のボタンを隠す設定の欄の名前の直しを、ほかの AI（Copilot）でレビューした。A 1 件（英語の "with your key" が分かりにくい）は "with the New from clipboard key" に直した。取り入れた B・C: 例外を "and only when" で限定、中国語の「每次弹出的确认」「点击」「快捷键」「用“GitHub 转换”改写」「按写法分类列出数量」「姓名首字母不再被识别为列表」、SVG の項目の言い直し、英語の "In Settings, you can hide"・"A single Menu … now holds"・"with its count"・"are now read correctly"・"included in … exports"、`\[ … \]` の項目の言い直し。取り入れなかったもの: 画面の名前を引用符で囲む（掲載文と PRIVACY のほかの文は囲まない）、"writing" を "syntax" にする（掲載文の他の項目も "writing" で揃える）、中国語の「没有跟踪」を「无…」にする（前に確認した既存の文）、3-2 の "asks for confirmation"（"asks" で足りる）。
- 2026-11-10（1.0.24）: 「GitHub 変換」が先に自動修正のルールを当てるようにした画面の文字（見出し・説明・列名・お知らせ）、README の 1 項目、Store の新機能の 1 行、CHANGELOG の 1 項目を、ほかの AI（Copilot）でレビューした。A 2 件を直した（CHANGELOG の英語: 確認の画面を "the question" から "the confirmation dialog" に、最後の文に "were not rewritten for GitHub" を足した）。取り入れた B・C: 数式のルールを "rewrite math as $" に言い直し（説明・README・CHANGELOG）、"recounts"、"Ctrl+Z undoes it."、README の "because … already"、Store の英語に「チェックで選べる」を足し "in one edit" を外した、CHANGELOG の "A single Ctrl+Z undoes"・"Apply the fix rules" の引用符・"on the text left after the checked rules are applied"、中国語の「$ 形式」「勾选状态」。取り入れなかったもの: "put back" を "undone" にする（お知らせ欄のボタンの名前が "Put back"）、"The fix rules fixed" の言い換え（好みの範囲）。
- 2026-11-11（1.0.24）: 目次に見出しの数式などを残す直しの CHANGELOG の 1 項目（英語）を、ほかの AI（Copilot）でレビューした。A 2 件はどちらも、括弧を書くのが使う人だと読んだもの（"had to be written"・"must be written"）で、書くのは目次の機能のため、その言い回しは取り入れなかった。代わりに日本語・英語とも主語（目次）をはっきりさせ、英語を "unpaired ones" にした。取り入れた B: 直したことが分かる "Fixed …"、"from headings" の曖昧さ（"in headings out of the table of contents"）、"go in" を "go into the table of contents" に。取り入れなかったもの: "table of contents" を "Contents" にする（Contents はボタンの名前で、作られるものは table of contents）。
- 2026-11-11（1.0.24）: 同じ項目に足した、リンクの中の数式の下線の 1 文（英語）を、ほかの AI（Copilot）でレビューした。指摘は C 1 件だけで、取り入れなかった（"until now" は同じ版のほかの項目と揃える。"broke off" で意味は伝わる）。
- 2026-11-11（1.0.24）: 数式のある見出しの id と、見出しの数式の `{ }` の 2 項目（CHANGELOG の英語）を、ほかの AI（Copilot）でレビューした。A はなかった。取り入れた B・C: "the target of in-page links"・"as well"、"On GitHub, the heading text contains the math exactly as written"、"left the math out of the id"・"flagged with"、"mistaken for"・"after that point"、"treated as"。取り入れなかったもの: 注意の文が「…」で省略されているという指摘（"(#…)" は画面の文そのもの）、"lint check"・"will be updated when you press" など好みの範囲のもの。
- 2026-11-12（1.0.24）: Store の「このバージョンの新機能」に足した目次の 1 行（英語・中国語）と、字数のために短くした英語の「自動修正」の 1 行を、ほかの AI（Copilot）でレビューした。A はなかった。取り入れた B・C: 短くした行に数を示すことを戻した（"after showing the count of each kind"。字数は 1,500 字以内）、英語の "now"・"IDs"、中国語の「以公式形式显示」「ID」。取り入れなかったもの: "writing" を "syntax"、"shows" を "renders" にする（掲載文のほかの項目と揃える）、"runs" を "applies" にする・画面の名前を太字にする（掲載文の書き方と揃える）。
