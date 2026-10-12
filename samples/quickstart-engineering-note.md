<!--
ファイル名 / File: samples/quickstart-engineering-note.md
概要 / Description: mdEditor 紹介資料 / mdEditor promotion material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.0.5
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 図と数式の技術メモ

mdEditorで開き、編集して、Ctrl+Shift+PでPDFに保存してください。

## 抵抗の消費電力

例：抵抗 $R=100\,\Omega$ に電圧 $V=10\,\mathrm{V}$ を印加する。

$$P=\frac{V^2}{R}=\frac{10^2}{100}=1\,\mathrm{W}$$

| 電圧 V | 抵抗 Ω | 消費電力 W |
| --- | --- | --- |
| 5 | 100 | 0.25 |
| 10 | 100 | 1.00 |

実際の部品選定では温度とディレーティングも確認します。

```mermaid
flowchart LR
 A[条件を整理] --> B[計算]
 B --> C[部品仕様を確認]
 C --> D[測定で検証]
```
