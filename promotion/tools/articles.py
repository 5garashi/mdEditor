"""
ファイル名 / File: promotion/tools/articles.py
概要 / Description: 日英の用途記事と媒体案内を生成 / Generate bilingual use-case articles
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
from build import ROOT,VERSION,DATE
MEDIA=ROOT/'multichannel'
def header(file,en=False):
    return ('<!--\nFile: '+file+'\nDescription: mdEditor use-case article\nAuthor: 5garashi.com Design Office\nCreated: '+DATE+'\nVersion: '+VERSION+'\nLicense: MIT\nSPDX-License-Identifier: MIT\n-->\n') if en else ('<!--\nファイル名 / File: '+file+'\n概要 / Description: mdEditor用途記事 / mdEditor use-case article\n作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n作成日 / Created: '+DATE+'\n版 / Version: '+VERSION+'\nライセンス / License: MIT\nSPDX-License-Identifier: MIT\n-->\n')
def signature(en=False):
    return f'\n---\n\n**{"Author" if en else "作成者 / Author"}**: {"5garashi.com Design Office" if en else "5garashi.com設計事務所 / 5garashi.com Design Office"}\n**{"Last updated" if en else "最終更新 / Last updated"}**: {DATE} JST\n'
JA=r'''# Markdownで図と数式の技術メモを作り、PDFで渡す

数行の計算と作業の流れを残したいとき、文章・図・数式を一つのMarkdownファイルにまとめると、後から修正しやすくなります。読み手にはPDFを渡し、自分の手元には編集用のMarkdownを残す使い方を紹介します。

今回は、無料のWindows用アプリmdEditorを使います。作成者は、本稿の著者である5garashi.com設計事務所です。技術メモを例に、開く、編集する、プレビューする、PDFに保存するまでを説明します。

## 1. 編集用と配布用を分ける

Markdownはテキストファイルなので、見出しや表を文章と一緒に編集できます。Mermaidの図もテキストで記述でき、KaTeXで表示する数式はTeX記法で記述します。

- 編集用: `.md`。計算条件や作業手順を後から直す。
- 配布用: `.pdf`。読み手がMarkdownを表示する環境を用意しなくても読める形で渡す。

PDFは共有する時点の記録です。元のMarkdownを変更しても、以前に保存したPDFは自動更新されません。修正後はPDFも保存し直します。

## 2. 小さな文書で試す

[Microsoft Store](https://apps.microsoft.com/detail/9nww2qrclmhc?hl=ja-JP&gl=JP)からmdEditorを導入し、「開く」で手元のmdファイルを選びます。新しく試す場合は、[図と数式のサンプル](../../samples/quickstart-engineering-note.md)のコピーを使ってください。

以下は、抵抗の消費電力を記録する例です。`$...$` が文章中の数式、`$$...$$` が独立した数式です。Mermaidのコードブロックは図としてプレビューされます。

````markdown
# 抵抗の消費電力

条件は $R=100\,\Omega$、$V=10\,\mathrm{V}$ とする。

$$P=\frac{V^2}{R}=\frac{10^2}{100}=1\,\mathrm{W}$$

```mermaid
flowchart LR
 A[条件を整理] --> B[計算] --> C[仕様を確認]
 classDef default fill:#FFFDF2,stroke:#385723,color:#385723
 linkStyle default stroke:#385723
```

| 電圧 | 抵抗 | 消費電力 |
| --- | --- | --- |
| 5 V | 100 Ω | 0.25 W |
| 10 V | 100 Ω | 1.00 W |
````

数式と表は自分で計算した結果の記載です。mdEditorは数値を自動計算しません。電圧を変える場合、文章・式・表の結果を自分で再計算して書き換えてください。この例だけで実際の部品選定を決めるものではありません。

## 3. 文章と図を編集する

最初の見出しを「抵抗の消費電力の記録」に変え、右側のプレビューにも反映されることを確認します。次に図の「仕様を確認」を「測定で検証」に変えてみます。

図の見た目を直接操作するのではなく、元になる文字を修正する操作です。文章と図の変更履歴を同じmdファイルで残せることが、この形式の扱いやすさです。

![mdEditorで編集欄とプレビューを並べた実画面。数式・フローチャート・表が表示されている。](../../assets/app-ja.png)

画面はソース版mdEditor 1.0.5の実機表示です。実演動画も[日本語MP4](../../assets/mdEditor-demo-ja.mp4)で確認できます。

## 4. 別ウィンドウで見比べる

プレビュー側の「別ウィンドウで表示」を選ぶと、読む画面を本体から分けられます。ほかの作業と並べたり、別のディスプレイに置いたりして使います。単に読みたいときは、プレビューをウィンドウ全体に広げる操作もあります。

以下は、本体と別プレビューの実画面です。ソース版1.0.5で取得しました。Storeの提供版にこのボタンがない場合は、その版の機能・更新状況を確認してください。別ウィンドウ表示と、ウィンドウ全体のプレビュー表示はそれぞれ異なる操作です。

![本体の編集ウィンドウの実画面。Markdownだけを広く表示している。](../illustrations/window-editor-ja.png)

![別ウィンドウのプレビューの実画面。図・数式・表が表示されている。](../illustrations/window-preview-ja.png)

## 5. Markdownを保存し、PDFを確認する

1. 「保存」で編集したMarkdownを保存する。
2. 「PDFに保存」を選び、PDFの保存先を指定する。
3. 保存したPDFを開く。
4. 見出し・図・数式・表が含まれること、文章の切れや不要なページがないかを確認する。

検証したソース版では、短い技術メモのPDFに末尾空白ページが付く場合がありました。本文・図・数式・表は出力されていますが、配布前にページ数を確認してください。PDFの自動更新や、数値の自動計算は行われません。

文書内に外部画像を置くと、その画像サーバーへの通信が発生します。本稿のサンプルには外部画像がなく、図・数式はアプリに同梱された部品で描画します。[アプリのプライバシーポリシー](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md)も参照してください。

## 参考資料と作成者

- [Mermaid公式資料](https://mermaid.js.org/): テキストから図を描く記法。
- [KaTeX公式資料](https://katex.org/docs/supported.html): 表示できる数式の記法。
- [mdEditorのソースと使い方](https://github.com/5garashi/mdEditor)。

本稿の手順と画像は、変更していないソース版1.0.5で確認しています。Storeの提供版番号・認定状況は未確認です。価格は所有者の確認と掲載情報に基づき無料と記載しています。

mdEditorは[5garashi.com設計事務所](https://www.5garashi.com/)の開発作品です。本文は用途を試すための手順記事で、未投稿の原稿です。掲載先のルールに合わせ、開発者本人が内容を確認してから公開してください。

デザイン基準出典: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)、5garashi.com設計事務所 / 5garashi.com Design Office。公式ロゴはCC BY-ND 4.0、本文と記法例はMIT。
'''
EN=r'''# Make a technical note with Markdown diagrams and math, then share a PDF

When you need to record a short calculation and a work procedure, keeping the text, diagram and equation in one Markdown file makes later edits easier. Keep the Markdown for editing and give readers a PDF of the rendered document.

This walkthrough uses mdEditor, a free Windows app developed by the author, 5garashi.com Design Office. We will open a note, edit it, check its preview and export it as PDF.

## 1. Keep an editable source and a readable copy

Markdown is a text format for headings, tables and other document elements. Mermaid diagrams are also written as text, and equations rendered by KaTeX use TeX notation.

- Editable source: `.md`. Revise conditions and procedures later.
- Reader's copy: `.pdf`. Share a rendered document without requiring a Markdown setup.

A PDF records the document at the time of export. Editing the Markdown does not update an earlier PDF; export it again after making changes.

## 2. Try a small document

Install mdEditor from [Microsoft Store](https://apps.microsoft.com/detail/9nww2qrclmhc?hl=en-US&gl=US), then choose a local file with Open. You can start with a copy of the [diagrams and math sample](../../samples/quickstart-engineering-note.en.md).

The following example records power dissipated by a resistor. `$...$` marks inline math; `$$...$$` marks a separate equation. The Mermaid code block is rendered as a diagram.

````markdown
# Resistor power

Conditions: $R=100\,\Omega$ and $V=10\,\mathrm{V}$.

$$P=\frac{V^2}{R}=\frac{10^2}{100}=1\,\mathrm{W}$$

```mermaid
flowchart LR
 A[Set conditions] --> B[Calculate] --> C[Check specifications]
 classDef default fill:#FFFDF2,stroke:#385723,color:#385723
 linkStyle default stroke:#385723
```

| Voltage | Resistance | Power |
| --- | --- | --- |
| 5 V | 100 Ω | 0.25 W |
| 10 V | 100 Ω | 1.00 W |
````

The equation and table record results calculated by the author. mdEditor does not calculate them. When changing a voltage, recalculate and update the text, equation and table yourself. This small example is not sufficient for choosing an actual component.

## 3. Edit the text and diagram

Change the heading to “Resistor power: reviewed” and check that the preview updates. Then replace “Check specifications” in the Mermaid block with “Verify by measurement”.

You change the text that describes the diagram, rather than manipulating shapes directly. This keeps the diagram and the prose together in the same editable file.

![The actual mdEditor Windows app showing an editor beside a preview with an equation, flowchart and table.](../../assets/app-en.png)

This screenshot uses source build mdEditor 1.0.5. The [English demonstration](../../assets/mdEditor-demo-en.mp4) shows real Windows operations.

## 4. Put the preview in another window

Select Open in its own window in the preview pane to detach it. Place it beside other work or move it to another display. If you only want to read, you can also fill the main window with the preview.

The following screens show the main editor and its detached preview, captured with source build 1.0.5. If the Store build does not show this button, check the features and update status for that build. Detaching the preview and filling the main window with it are separate operations.

![The actual main editor window showing the Markdown source.](../illustrations/window-editor-en.png)

![The actual detached preview window showing a diagram, equation and table.](../illustrations/window-preview-en.png)

## 5. Save the Markdown and check the PDF

1. Select Save to preserve the edited Markdown.
2. Select Save as PDF and choose a destination.
3. Open the saved PDF.
4. Check the heading, diagram, equation and table, along with page breaks and unwanted pages.

In the tested source build, a short technical note could produce a trailing blank PDF page. The document content was exported, but check the page count before sharing. PDF files do not update automatically, and numerical results are not calculated by mdEditor.

External images in a document connect to their image servers. This sample has no external images; its diagrams and equations use components bundled with the app. See the [app privacy policy](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md).

## References and author

- [Mermaid documentation](https://mermaid.js.org/): text-based diagram notation.
- [KaTeX documentation](https://katex.org/docs/supported.html): supported math notation.
- [mdEditor source and usage](https://github.com/5garashi/mdEditor).

The steps and screenshots were checked with unmodified source build 1.0.5. The currently available Store build and certification status are unverified. The free price follows the owner's confirmation and Store listing data.

mdEditor is a development project by [5garashi.com Design Office](https://www.5garashi.com/). This is an unpublished use-case article. The developer should review it and adapt it to the destination's rules before publishing.

Design reference: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/), 5garashi.com Design Office. Official logos are CC BY-ND 4.0; this article and its notation examples are MIT.
'''
for lang,text in [('ja',JA),('en',EN)]:
    file=f'multichannel/articles/technical-note-{lang}.md'
    (ROOT/file).write_text(header('promotion/'+file,lang=='en')+text+signature(lang=='en'),encoding='utf-8',newline='\n')

print('Generated Japanese and English use-case articles')
