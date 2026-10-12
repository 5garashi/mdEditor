"""
ファイル名 / File: promotion/tools/samples.py
概要 / Description: Generate the six local-only quickstart documents
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from build import ROOT, header
SAMPLES={
'engineering-note':(r'''# 図と数式のメモ

抵抗 $R=100\,\Omega$、電圧 $V=10\,\mathrm{V}$ の計算例です。

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

## 試してみる

1. この文書のコピーをmdEditorで開く。
2. 最初の見出しを「図と数式の記録」に変更し、プレビューを確認する。
3. 図の「仕様を確認」を「測定で検証」に変更する。図にも同じ文字が表示される。
4. 「保存」でMarkdownを保存し、「PDFに保存」でPDFを書き出す。
5. PDFを開き、見出し、図、数式、表が含まれることを確かめる。

数式と表は計算結果の記載例です。値を変えたときは、自分で再計算して書き換えます。mdEditorは自動計算しません。実際の部品選定には温度とディレーティングの確認も必要です。
''',r'''# Diagrams and math

Example: resistance $R=100\,\Omega$ and voltage $V=10\,\mathrm{V}$.

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

## Try it

1. Open a copy of this document in mdEditor.
2. Change the first heading to “Diagrams and math: reviewed”. Check the preview.
3. Change “Check specifications” in the diagram to “Verify by measurement”. The diagram shows the new label.
4. Select “Save” to save the Markdown, then “Save as PDF” to export a PDF.
5. Open the PDF and check the heading, diagram, equation and table.

Recalculate and update the equation and table yourself when changing the values; mdEditor does not calculate them. Actual component selection also requires checking temperature and derating.
'''),
'workflow':('''# 文書レビューの手順

目的: 文書間の型式・定格・版番号を照合する。

1. 対象文書を集める。
2. 型式・定格・版番号を照合する。
3. 不一致を記録し、担当者に確認する。
4. 修正版を確認する。

```mermaid
flowchart TD
 A[文書を集める] --> B[照合する]
 B --> C{不一致あり?}
 C -->|はい| D[確認して修正]
 C -->|いいえ| E[確認完了]
 D --> B
 classDef default fill:#FFFDF2,stroke:#385723,color:#385723
 linkStyle default stroke:#385723
```

| 項目 | 確認内容 |
| --- | --- |
| 型式 | 文書間で同じか |
| 定格 | 数値と単位が一致するか |
| 版番号 | 対象の版か |

- [x] 対象文書を準備した
- [ ] 不一致を確認した
- [ ] 修正版を確認した

## 試してみる

この文書のコピーをmdEditorで開き、上の `[ ]` を `[x]` に変えてみてください。プレビューでチェックが付きます。「保存」で文書を保存し、「PDFに保存」で書き出します。PDFにも図・表・チェック結果が含まれることを確認してください。
''','''# Document review procedure

Purpose: compare model numbers, ratings and revisions.

1. Collect the documents.
2. Compare model numbers, ratings and revisions.
3. Record differences and ask the owner to check them.
4. Review the corrected documents.

```mermaid
flowchart TD
 A[Collect documents] --> B[Compare]
 B --> C{Differences?}
 C -->|Yes| D[Check and correct]
 C -->|No| E[Review complete]
 D --> B
 classDef default fill:#FFFDF2,stroke:#385723,color:#385723
 linkStyle default stroke:#385723
```

| Item | What to check |
| --- | --- |
| Model number | Same across documents? |
| Rating | Matching numbers and units? |
| Revision | The intended revision? |

- [x] Documents collected
- [ ] Differences checked
- [ ] Corrections reviewed

## Try it

Open a copy in mdEditor and change one `[ ]` above to `[x]`. A checked box appears in the preview. Select “Save”, then “Save as PDF”. Open the PDF and check that it includes the diagram, table and checkmarks.
'''),
'ai-notes':('''# 回答を整理するメモ

AIなどから得た回答の要点・出典・次の行動を整理する記入用テンプレートです。mdEditor自体にAI生成機能はありません。このサンプルから外部への送信は行いません。

## 質問

ここに、自分が確認したいことを書く。

## 回答の整理

| 提案 | 利点 | 確認事項・出典 |
| --- | --- | --- |
| 案A | ここに記入 | 数値や出典を確認 |
| 案B | ここに記入 | 実行条件を確認 |

## 次の行動

- [ ] 数値と出典を確認する
- [ ] 自分の条件に合わせて修正する
- [ ] 共有する内容を確認する

## 試してみる

この文書のコピーをmdEditorで開き、「案A」を自分の提案名に変えてみてください。プレビューの表にも反映されます。「保存」で文書を保存し、「PDFに保存」で書き出します。PDFで表とチェックリストを確認してください。
''','''# Notes for organizing an answer

A template for an answer's key points, sources and next actions. You can paste an answer from AI or another source. mdEditor has no AI generation feature. This sample sends nothing to an external service.

## Question

Write what you want to find out here.

## Organize the answer

| Proposal | Benefit | Checks and sources |
| --- | --- | --- |
| Option A | Fill in here | Verify numbers and sources |
| Option B | Fill in here | Check conditions |

## Next actions

- [ ] Verify numbers and sources
- [ ] Adapt the answer to your situation
- [ ] Check what you will share

## Try it

Open a copy in mdEditor. Replace “Option A” with your own proposal's name; the preview table updates. Select “Save”, then “Save as PDF”. Open the PDF and check the table and checklist.
''')}

(ROOT/'samples').mkdir(exist_ok=True)
for name,pair in SAMPLES.items():
    for en,body in enumerate(pair):
        file=f'quickstart-{name}{".en" if en else ""}.md'
        (ROOT/'samples'/file).write_text(header('promotion/samples/'+file,bool(en))+body,encoding='utf-8',newline='\n')
print('Generated six sample documents')
