---
title: mdEditor — 書く、見る、渡す。ぜんぶ PC の中で。
tags: [Markdown, PDF, Word, Mermaid, KaTeX, Graphviz, 3D, 地図]
author:
  name: 5garashi.com設計事務所
  url: https://www.5garashi.com/
store: https://apps.microsoft.com/detail/9NWW2QRCLMHC
license: MIT
---
<!--
  ファイル名 / File: samples/key-features.md
  概要 / Description: mdEditor key features, shown with its own diagrams, math, 3D model, images, and map (Japanese)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-11-01
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->

<p align="center"><picture><source media="(prefers-color-scheme: dark)" srcset="import-test_files/5garashi_logotype_reversed_outlined.svg"><img src="import-test_files/5garashi_logotype_outlined.svg" alt="5garashi.com設計事務所" width="280"></picture></p>

<p align="center"><b>Markdown エディター & ビューアー for Windows</b></p>

<h1 align="center">mdEditor</h1>

<p align="center">
PDF も Word も、AI のチャットの回答も。<br>
開けば Markdown、直せば PDF。<b>ぜんぶ PC の中で。</b>
</p>

<p align="center">
<kbd>Ctrl</kbd>+<kbd>O</kbd> で開いて、<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> で PDF。それだけ。
</p>

> [!TIP]
> この文書そのものが、mdEditor のデモです。左の編集欄と右のプレビューを見比べてください。図も数式も 3D モデルも、すべてテキストから描いています。

> [!NOTE]
> この文書は mdEditor での表示を前提にしています。GitHub で開くと、Graphviz（` ```dot `）の図はコードのまま表示され、`\(…\)` の数式は数式になりません。表示の確認は mdEditor で行ってください（Identity 8-9 (8)）。

<!-- toc -->
- [1. mdEditor でできること](#1-mdeditor-でできること)
- [2. PDF も Word も、Markdown に](#2-pdf-も-word-もmarkdown-に)
- [3. AI のチャットの回答も、できる範囲で再現](#3-ai-のチャットの回答もできる範囲で再現)
- [4. 数式は KaTeX で](#4-数式は-katex-で)
- [5. 図は Graphviz でも](#5-図は-graphviz-でも)
- [6. 3D モデルは、ドラッグで回せる](#6-3d-モデルはドラッグで回せる)
- [7. 画像も、大きさと位置を自在に](#7-画像も大きさと位置を自在に)
- [8. 地図も描ける](#8-地図も描ける)
- [9. コードは色分け](#9-コードは色分け)
- [10. ここまでの歩み](#10-ここまでの歩み)
- [11. キーを覚えると、もっと速い](#11-キーを覚えるともっと速い)
- [12. はじめの一歩](#12-はじめの一歩)
<!-- /toc -->

---

## 1. mdEditor でできること

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723", "cScale0": "#FFFDF2", "cScaleLabel0": "#3E6B2A", "cScaleInv0": "#3E6B2A", "cScalePeer0": "#3E6B2A", "cScale1": "#F07B5F", "cScaleLabel1": "#1A2212", "cScaleInv1": "#1A2212", "cScalePeer1": "#1A2212", "cScale2": "#9FCC7E", "cScaleLabel2": "#1A2212", "cScaleInv2": "#1A2212", "cScalePeer2": "#1A2212", "cScale3": "#F7A58E", "cScaleLabel3": "#1A2212", "cScaleInv3": "#1A2212", "cScalePeer3": "#1A2212", "cScale4": "#FFE699", "cScaleLabel4": "#7B4A21", "cScaleInv4": "#7B4A21", "cScalePeer4": "#7B4A21", "cScale5": "#F9EFCB", "cScaleLabel5": "#8C1B0D", "cScaleInv5": "#8C1B0D", "cScalePeer5": "#8C1B0D", "cScale6": "#FFFDF2", "cScaleLabel6": "#3E6B2A", "cScaleInv6": "#3E6B2A", "cScalePeer6": "#3E6B2A", "cScale7": "#F07B5F", "cScaleLabel7": "#1A2212", "cScaleInv7": "#1A2212", "cScalePeer7": "#1A2212", "cScale8": "#9FCC7E", "cScaleLabel8": "#1A2212", "cScaleInv8": "#1A2212", "cScalePeer8": "#1A2212", "cScale9": "#F7A58E", "cScaleLabel9": "#1A2212", "cScaleInv9": "#1A2212", "cScalePeer9": "#1A2212", "cScale10": "#FFE699", "cScaleLabel10": "#7B4A21", "cScaleInv10": "#7B4A21", "cScalePeer10": "#7B4A21", "cScale11": "#F9EFCB", "cScaleLabel11": "#8C1B0D", "cScaleInv11": "#8C1B0D", "cScalePeer11": "#8C1B0D", "git0": "#FFE699", "gitBranchLabel0": "#385723"}}}%%
mindmap
  root((mdEditor))
    開く
      PDF
      Word
      Web ページ
      メール
      CSV・JSON
    書く
      色分け
      書き方の確認
      太字・リストをキー 1 つで
      入力の候補
    見る
      図とグラフ
      数式
      3D モデル
      地図
    渡す
      PDF
      Word
      HTML・PNG
      配布用 ZIP
```

| | mdEditor なら | |
| :--- | :--- | :---: |
| **開く** | PDF・Word・Web ページ・メール・CSV・JSON・XML・Jupyter Notebook を Markdown に | <kbd>Ctrl</kbd>+<kbd>O</kbd> |
| **書く** | 書くそばから色分けし、書き間違いに波線 | <kbd>F1</kbd> |
| **見る** | 図・数式・3D モデル・地図を、横のプレビューにすぐ表示 | <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>2</kbd> |
| **渡す** | プレビューと同じ見た目の PDF をワンクリックで | <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> |

---

## 2. PDF も Word も、Markdown に

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723"}}}%%
flowchart LR
    subgraph IN["開く"]
        P["PDF"]
        W["Word"]
        H["Web ページ"]
        A["AI のチャット"]
    end
    subgraph MD["mdEditor"]
        E["Markdown で<br>確かめて直す"]
    end
    subgraph OUT["渡す"]
        PDF["PDF<br>（ページ番号・タグ付き）"]
        DOC["Word"]
        Z["配布用 ZIP"]
    end
    P --> E
    W --> E
    H --> E
    A --> E
    E --> PDF
    E --> DOC
    E --> Z
    style P fill:#F7A58E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style W fill:#9FCC7E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style H fill:#FFE699,stroke:#7B4A21,stroke-width:2px,color:#7B4A21
    style A fill:#F07B5F,stroke:#1A2212,stroke-width:3px,color:#1A2212,font-weight:bold
    style E fill:#FFE699,stroke:#385723,stroke-width:4px,color:#385723
    style PDF fill:#F07B5F,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style DOC fill:#FFFDF2,stroke:#7B4A21,stroke-width:2px,color:#7B4A21
    style Z fill:#9FCC7E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style IN fill:#FFFDF2,stroke:#385723,color:#385723
    style MD fill:#F9EFCB,stroke:#385723,stroke-width:2px,color:#385723
    style OUT fill:#F9EFCB,stroke:#385723,color:#385723
```

> [!NOTE]
> 取り込みは、できる範囲で行います。段組みの配置、罫線のない表、スキャンした PDF は元どおりには戻りません。図と数式は画像になります。

---

## 3. AI のチャットの回答も、できる範囲で再現

AI のチャットからコピーした Markdown には、癖があります。mdEditor は、開いたときにそれを見つけて直します。直したことはお知らせ欄に出て、ボタン 1 つで元に戻せます。

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723", "actorBkg": "#F7A58E", "actorTextColor": "#1A2212", "actorBorder": "#1A2212", "actorLineColor": "#385723", "signalColor": "#385723", "signalTextColor": "#385723"}}}%%
sequenceDiagram
    box rgb(159, 204, 126) インターネット
    participant AI as AI のチャット
    end
    box rgb(249, 239, 203) あなたの PC
    participant あなた
    participant M as mdEditor
    end
    あなた->>AI: 質問する
    AI-->>あなた: Markdown の回答
    あなた->>M: 貼り付けて新規作成
    rect rgb(255, 230, 153)
    M->>M: 数式・太字・引用の印を直す
    M-->>あなた: お知らせ欄で「直しました」
    end
    あなた->>M: Ctrl+Shift+P
    M-->>あなた: PDF のできあがり
```

たとえば、AI がよく書く `\(…\)` の数式も、そのまま数式になります。光の速さ \(c \approx 3.0 \times 10^8\ \text{m/s}\) のように。

\[
E = mc^2 \qquad \text{と} \qquad e^{i\pi} + 1 = 0
\]

---

## 4. 数式は KaTeX で

$$
\int_{-\infty}^{\infty} e^{-x^2}\,dx = \sqrt{\pi}
\qquad
\sum_{k=1}^{n} k = \frac{n(n+1)}{2}
$$

$$
\begin{pmatrix} \cos\theta & -\sin\theta \\ \sin\theta & \cos\theta \end{pmatrix}
\begin{pmatrix} x \\ y \end{pmatrix}
=
\begin{pmatrix} x\cos\theta - y\sin\theta \\ x\sin\theta + y\cos\theta \end{pmatrix}
$$

文の中にも $\nabla \cdot \mathbf{E} = \rho / \varepsilon_0$ のように書けます。

---

## 5. 図は Graphviz でも

```dot
digraph promo {
    graph [rankdir=LR, pad=0.2, nodesep=0.35, ranksep=0.45, fontname="Helvetica", bgcolor="transparent"];
    node  [shape=box, style="rounded,filled", fontname="Helvetica", fontsize=12,
           color="#385723", fillcolor="#FFFDF2", fontcolor="#385723", penwidth=1.4, margin="0.2,0.08"];
    edge  [color="#385723", penwidth=1.6, arrowsize=0.8];

    text   [label="テキスト\n（Markdown）", fillcolor="#FFE699", fontcolor="#385723", color="#385723", penwidth=2.4];
    mermaid [label="Mermaid\n図・グラフ", fillcolor="#9FCC7E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    katex   [label="KaTeX\n数式", fillcolor="#F7A58E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    dot     [label="Graphviz\nこの図", fillcolor="#FFFDF2", fontcolor="#7B4A21", color="#7B4A21", penwidth=1.6];
    stl     [label="STL\n3D モデル", fillcolor="#F07B5F", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    geo     [label="GeoJSON\n地図", fillcolor="#F9EFCB", fontcolor="#3E6B2A", color="#3E6B2A", penwidth=1.6];
    code    [label="約 40 言語の\nコードの色分け", fillcolor="#9FCC7E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    pdf     [label="PDF・Word\nHTML・PNG", fillcolor="#F9EFCB", fontcolor="#7B4A21", color="#7B4A21", penwidth=2.4];

    text -> {mermaid katex dot stl geo code};
    {mermaid katex dot stl geo code} -> pdf;
}
```

---

## 6. 3D モデルは、ドラッグで回せる

テキスト形式の STL を書くだけで、3D モデルになります。マウスでつかんで回してください。インターネットにつながなくても表示します。

部品の色は `solid star #3E6B2A` のように名前の後ろに、背景の色は `solid background #F9EFCB` の行で書けます。

```stl
solid base #F07B5F
facet normal 0 0 1 outer loop vertex 0 0 0 vertex 18.48 7.65 0 vertex 7.65 18.48 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex 7.65 18.48 -4 vertex 18.48 7.65 -4 endloop endfacet
facet normal 0.7071 0.7071 0 outer loop vertex 18.48 7.65 -4 vertex 7.65 18.48 -4 vertex 7.65 18.48 0 endloop endfacet
facet normal 0.7071 0.7071 -0 outer loop vertex 18.48 7.65 -4 vertex 7.65 18.48 0 vertex 18.48 7.65 0 endloop endfacet
facet normal 0 -0 1 outer loop vertex 0 0 0 vertex 7.65 18.48 0 vertex -7.65 18.48 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex -7.65 18.48 -4 vertex 7.65 18.48 -4 endloop endfacet
facet normal 0 1 0 outer loop vertex 7.65 18.48 -4 vertex -7.65 18.48 -4 vertex -7.65 18.48 0 endloop endfacet
facet normal 0 1 -0 outer loop vertex 7.65 18.48 -4 vertex -7.65 18.48 0 vertex 7.65 18.48 0 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 0 vertex -7.65 18.48 0 vertex -18.48 7.65 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex -18.48 7.65 -4 vertex -7.65 18.48 -4 endloop endfacet
facet normal -0.7071 0.7071 0 outer loop vertex -7.65 18.48 -4 vertex -18.48 7.65 -4 vertex -18.48 7.65 0 endloop endfacet
facet normal -0.7071 0.7071 0 outer loop vertex -7.65 18.48 -4 vertex -18.48 7.65 0 vertex -7.65 18.48 0 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 0 vertex -18.48 7.65 0 vertex -18.48 -7.65 0 endloop endfacet
facet normal -0 0 -1 outer loop vertex 0 0 -4 vertex -18.48 -7.65 -4 vertex -18.48 7.65 -4 endloop endfacet
facet normal -1 0 0 outer loop vertex -18.48 7.65 -4 vertex -18.48 -7.65 -4 vertex -18.48 -7.65 0 endloop endfacet
facet normal -1 0 0 outer loop vertex -18.48 7.65 -4 vertex -18.48 -7.65 0 vertex -18.48 7.65 0 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 0 vertex -18.48 -7.65 0 vertex -7.65 -18.48 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex -7.65 -18.48 -4 vertex -18.48 -7.65 -4 endloop endfacet
facet normal -0.7071 -0.7071 0 outer loop vertex -18.48 -7.65 -4 vertex -7.65 -18.48 -4 vertex -7.65 -18.48 0 endloop endfacet
facet normal -0.7071 -0.7071 0 outer loop vertex -18.48 -7.65 -4 vertex -7.65 -18.48 0 vertex -18.48 -7.65 0 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 0 vertex -7.65 -18.48 0 vertex 7.65 -18.48 0 endloop endfacet
facet normal 0 -0 -1 outer loop vertex 0 0 -4 vertex 7.65 -18.48 -4 vertex -7.65 -18.48 -4 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.65 -18.48 -4 vertex 7.65 -18.48 -4 vertex 7.65 -18.48 0 endloop endfacet
facet normal 0 -1 0 outer loop vertex -7.65 -18.48 -4 vertex 7.65 -18.48 0 vertex -7.65 -18.48 0 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 0 vertex 7.65 -18.48 0 vertex 18.48 -7.65 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex 18.48 -7.65 -4 vertex 7.65 -18.48 -4 endloop endfacet
facet normal 0.7071 -0.7071 0 outer loop vertex 7.65 -18.48 -4 vertex 18.48 -7.65 -4 vertex 18.48 -7.65 0 endloop endfacet
facet normal 0.7071 -0.7071 0 outer loop vertex 7.65 -18.48 -4 vertex 18.48 -7.65 0 vertex 7.65 -18.48 0 endloop endfacet
facet normal -0 0 1 outer loop vertex 0 0 0 vertex 18.48 -7.65 0 vertex 18.48 7.65 0 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 -4 vertex 18.48 7.65 -4 vertex 18.48 -7.65 -4 endloop endfacet
facet normal 1 0 0 outer loop vertex 18.48 -7.65 -4 vertex 18.48 7.65 -4 vertex 18.48 7.65 0 endloop endfacet
facet normal 1 0 0 outer loop vertex 18.48 -7.65 -4 vertex 18.48 7.65 0 vertex 18.48 -7.65 0 endloop endfacet
endsolid base
solid star #3E6B2A
facet normal 0 -0 1 outer loop vertex 0 0 7 vertex 0 16 7 vertex -3.82 5.26 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex -3.82 5.26 0 vertex 0 16 0 endloop endfacet
facet normal -0.9422 0.3351 0 outer loop vertex 0 16 0 vertex -3.82 5.26 0 vertex -3.82 5.26 7 endloop endfacet
facet normal -0.9422 0.3351 0 outer loop vertex 0 16 0 vertex -3.82 5.26 7 vertex 0 16 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex -3.82 5.26 7 vertex -15.22 4.94 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex -15.22 4.94 0 vertex -3.82 5.26 0 endloop endfacet
facet normal -0.0281 0.9996 0 outer loop vertex -3.82 5.26 0 vertex -15.22 4.94 0 vertex -15.22 4.94 7 endloop endfacet
facet normal -0.0281 0.9996 0 outer loop vertex -3.82 5.26 0 vertex -15.22 4.94 7 vertex -3.82 5.26 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex -15.22 4.94 7 vertex -6.18 -2.01 7 endloop endfacet
facet normal -0 0 -1 outer loop vertex 0 0 0 vertex -6.18 -2.01 0 vertex -15.22 4.94 0 endloop endfacet
facet normal -0.6095 -0.7928 0 outer loop vertex -15.22 4.94 0 vertex -6.18 -2.01 0 vertex -6.18 -2.01 7 endloop endfacet
facet normal -0.6095 -0.7928 0 outer loop vertex -15.22 4.94 0 vertex -6.18 -2.01 7 vertex -15.22 4.94 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex -6.18 -2.01 7 vertex -9.4 -12.94 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex -9.4 -12.94 0 vertex -6.18 -2.01 0 endloop endfacet
facet normal -0.9592 0.2826 0 outer loop vertex -6.18 -2.01 0 vertex -9.4 -12.94 0 vertex -9.4 -12.94 7 endloop endfacet
facet normal -0.9592 0.2826 0 outer loop vertex -6.18 -2.01 0 vertex -9.4 -12.94 7 vertex -6.18 -2.01 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex -9.4 -12.94 7 vertex -0 -6.5 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex -0 -6.5 0 vertex -9.4 -12.94 0 endloop endfacet
facet normal 0.5652 -0.825 0 outer loop vertex -9.4 -12.94 0 vertex -0 -6.5 0 vertex -0 -6.5 7 endloop endfacet
facet normal 0.5652 -0.825 0 outer loop vertex -9.4 -12.94 0 vertex -0 -6.5 7 vertex -9.4 -12.94 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex -0 -6.5 7 vertex 9.4 -12.94 7 endloop endfacet
facet normal 0 -0 -1 outer loop vertex 0 0 0 vertex 9.4 -12.94 0 vertex -0 -6.5 0 endloop endfacet
facet normal -0.5652 -0.825 0 outer loop vertex -0 -6.5 0 vertex 9.4 -12.94 0 vertex 9.4 -12.94 7 endloop endfacet
facet normal -0.5652 -0.825 0 outer loop vertex -0 -6.5 0 vertex 9.4 -12.94 7 vertex -0 -6.5 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex 9.4 -12.94 7 vertex 6.18 -2.01 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex 6.18 -2.01 0 vertex 9.4 -12.94 0 endloop endfacet
facet normal 0.9592 0.2826 0 outer loop vertex 9.4 -12.94 0 vertex 6.18 -2.01 0 vertex 6.18 -2.01 7 endloop endfacet
facet normal 0.9592 0.2826 -0 outer loop vertex 9.4 -12.94 0 vertex 6.18 -2.01 7 vertex 9.4 -12.94 7 endloop endfacet
facet normal -0 0 1 outer loop vertex 0 0 7 vertex 6.18 -2.01 7 vertex 15.22 4.94 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex 15.22 4.94 0 vertex 6.18 -2.01 0 endloop endfacet
facet normal 0.6095 -0.7928 0 outer loop vertex 6.18 -2.01 0 vertex 15.22 4.94 0 vertex 15.22 4.94 7 endloop endfacet
facet normal 0.6095 -0.7928 0 outer loop vertex 6.18 -2.01 0 vertex 15.22 4.94 7 vertex 6.18 -2.01 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex 15.22 4.94 7 vertex 3.82 5.26 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex 3.82 5.26 0 vertex 15.22 4.94 0 endloop endfacet
facet normal 0.0281 0.9996 0 outer loop vertex 15.22 4.94 0 vertex 3.82 5.26 0 vertex 3.82 5.26 7 endloop endfacet
facet normal 0.0281 0.9996 -0 outer loop vertex 15.22 4.94 0 vertex 3.82 5.26 7 vertex 15.22 4.94 7 endloop endfacet
facet normal 0 0 1 outer loop vertex 0 0 7 vertex 3.82 5.26 7 vertex 0 16 7 endloop endfacet
facet normal 0 0 -1 outer loop vertex 0 0 0 vertex 0 16 0 vertex 3.82 5.26 0 endloop endfacet
facet normal 0.9422 0.3351 0 outer loop vertex 3.82 5.26 0 vertex 0 16 0 vertex 0 16 7 endloop endfacet
facet normal 0.9422 0.3351 -0 outer loop vertex 3.82 5.26 0 vertex 0 16 7 vertex 3.82 5.26 7 endloop endfacet
endsolid star
solid background #F9EFCB
endsolid background
```

---

## 7. 画像も、大きさと位置を自在に

画像は、本文の幅に対する割合やピクセルで大きさを決め、中央・右に寄せたり、文章を回り込ませたりできます（GitHub と同じ書き方）。画像の行で <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>F</kbd>（画像の調整）を押すか、プレビューの画像を右クリックすると、選ぶだけで書き換えます。

<p align="center"><img src="import-test_files/5garashi_og_image.png" alt="5garashi.com設計事務所 Design Office Identity の紹介画像（本文の幅の 60%、中央）" width="60%"></p>

<p align="center"><img src="import-test_files/5garashi_mark_simple.svg" alt="シンボルマーク（32 ピクセル）" width="32">&nbsp;&nbsp;&nbsp;&nbsp;<img src="import-test_files/5garashi_mark_simple.svg" alt="シンボルマーク（64 ピクセル）" width="64">&nbsp;&nbsp;&nbsp;&nbsp;<img src="import-test_files/5garashi_mark_simple.svg" alt="シンボルマーク（96 ピクセル）" width="96"></p>

<p align="right"><picture><source media="(prefers-color-scheme: dark)" srcset="import-test_files/5garashi_logotype_reversed_outlined.svg"><img src="import-test_files/5garashi_logotype_outlined.svg" alt="5garashi.com設計事務所（右寄せ）" width="200"></picture></p>

<img src="import-test_files/5garashi_mark_simple.svg" alt="5garashi.com設計事務所シンボルマーク" width="80" align="left" hspace="20" vspace="8">シンボルマークは、唐辛子のタリーマーク（画線法）です。縦 4 本の唐辛子を、5 本目が横に貫きます。「1・2・3・4」と数えて 5 本目で締める——「5（ご）がらし」の名をそのまま図にした印です。`align="left"` の画像は左に寄り、この段落はその右側に回り込みます。

<br clear="all">

<sub>画像は 5garashi.com設計事務所の公式アセット（CC BY-ND 4.0）を、改変せずに使っています。ダークモードでは、ロゴタイプが灰色抜きの版に替わります。</sub>

---

## 8. 地図も描ける

mdEditor は日本語・英語・簡体字中国語に対応しています。

```geojson
{
  "type": "FeatureCollection",
  "features": [
    { "type": "Feature", "properties": { "name": "日本語", "marker-color": "#B92613" }, "geometry": { "type": "Point", "coordinates": [139.767, 35.681] } },
    { "type": "Feature", "properties": { "name": "简体中文", "marker-color": "#385723" }, "geometry": { "type": "Point", "coordinates": [116.397, 39.909] } },
    { "type": "Feature", "properties": { "name": "English", "marker-color": "#7B4A21" }, "geometry": { "type": "Point", "coordinates": [-0.128, 51.507] } },
    { "type": "Feature", "properties": { "name": "English", "marker-color": "#7B4A21" }, "geometry": { "type": "Point", "coordinates": [-74.006, 40.713] } }
  ]
}
```

---

## 9. コードは色分け

```python
from pathlib import Path

def count_headings(path: str) -> int:
    """Markdown の見出しの数を数える"""
    text = Path(path).read_text(encoding="utf-8")
    return sum(1 for line in text.splitlines() if line.startswith("#"))

print(count_headings("key-features.md"))
```

```diff
- 太字は**「光年」**は、距離の単位です。
+ 太字は<strong>「光年」</strong>は、距離の単位です。
```

---

## 10. ここまでの歩み

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723"}}}%%
flowchart LR
    d1["<b>10月11日</b><br>最初の版"]
    d2["<b>10月17日</b><br>3D モデル・地図<br>GitHub の書き方"]
    d3["<b>10月20日</b><br>画像の調整<br>配布用 ZIP"]
    d4["<b>10月23日</b><br>簡体字中国語<br>起動が約 2 倍速く"]
    d5["<b>10月25日</b><br>PDF を開く"]
    d6["<b>10月31日</b><br>AI の Markdown<br>書く手助け<br>書き方の確認"]
    d1 --> d2 --> d3 --> d4 --> d5 --> d6
    style d1 fill:#FFE699,stroke:#385723,stroke-width:2px,color:#385723
    style d2 fill:#F7A58E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d3 fill:#9FCC7E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d4 fill:#FFFDF2,stroke:#7B4A21,stroke-width:2px,color:#7B4A21
    style d5 fill:#F07B5F,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d6 fill:#F9EFCB,stroke:#8C1B0D,stroke-width:2px,color:#8C1B0D
```

---

## 11. キーを覚えると、もっと速い

<details>
<summary><b>よく使うキー（クリックで開く）</b></summary>

| キー | すること |
| :--- | :--- |
| <kbd>Ctrl</kbd>+<kbd>B</kbd> / <kbd>Ctrl</kbd>+<kbd>I</kbd> | 太字 / 斜体 |
| <kbd>Enter</kbd> / <kbd>Tab</kbd> | リストの続き / 段を下げる |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>W</kbd> | 表を整える |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>N</kbd> | 見出しに番号を付ける・外す |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>J</kbd> | 目次を入れる・新しくする |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> | PDF に保存 |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>3</kbd> | プレビューを別のウィンドウに |
| <kbd>F1</kbd> | Markdown の書き方の一覧 |

</details>

---

## 12. はじめの一歩

- [x] Microsoft Store から mdEditor を入れる
- [ ] 手元の PDF を開いてみる
- [ ] AI のチャットの回答を貼り付けてみる
- [ ] <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> で PDF にする

> [!IMPORTANT]
> 文書の内容は、どこにも送りません。PDF や Word の変換も PC の中で行います。広告も、計測も、アカウントもありません。

---

<p align="center"><img src="import-test_files/5garashi_mark_simple.svg" alt="5garashi.com設計事務所シンボルマーク" width="48"></p>

<p align="center">
<b>mdEditor by 5garashi.com</b><br>
<a href="https://apps.microsoft.com/detail/9NWW2QRCLMHC">Microsoft Store で入手</a>
</p>

<p align="center"><sub>© 5garashi.com設計事務所</sub></p>
