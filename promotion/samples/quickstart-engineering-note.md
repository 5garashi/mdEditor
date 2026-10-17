<!--
ファイル名 / File: promotion/samples/quickstart-engineering-note.md
概要 / Description: mdEditor紹介資料 / mdEditor promotion material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.5.0
ライセンス / License: MIT
最終更新 / Last updated: 2026-10-17
SPDX-License-Identifier: MIT
-->
# 図と数式のメモ

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
4. 「保存」でMarkdownを保存し、「名前を付けて保存」でPDFを選択でPDFを書き出す。
5. PDFを開き、見出し、図、数式、表が含まれることを確かめる。

数式と表は計算結果の記載例です。値を変えたときは、自分で再計算して書き換えます。mdEditorは自動計算しません。実際の部品選定には温度とディレーティングの確認も必要です。
