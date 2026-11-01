---
title: mdEditor — Write, view, share. All on your PC.
tags: [Markdown, PDF, Word, Mermaid, KaTeX, Graphviz, 3D, Maps]
author:
  name: 5garashi.com Design Office
  url: https://www.5garashi.com/
store: https://apps.microsoft.com/detail/9NWW2QRCLMHC
license: MIT
---
<!--
  ファイル名 / File: samples/key-features.en.md
  概要 / Description: mdEditor key features, shown with its own diagrams, math, 3D model, images, and map (English)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-11-01
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->

<p align="center"><picture><source media="(prefers-color-scheme: dark)" srcset="import-test_files/5garashi_logotype_en_reversed_outlined.svg"><img src="import-test_files/5garashi_logotype_en_outlined.svg" alt="5garashi.com Design Office" width="280"></picture></p>

<p align="center"><b>Markdown editor & viewer for Windows</b></p>

<h1 align="center">mdEditor</h1>

<p align="center">
PDFs, Word files, answers from AI chats.<br>
Open them as Markdown, fix them, and get a PDF. <b>All on your PC.</b>
</p>

<p align="center">
Open with <kbd>Ctrl</kbd>+<kbd>O</kbd>, make a PDF with <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd>. That's it.
</p>

> [!TIP]
> This document is itself a demo of mdEditor. Compare the editor on the left with the preview on the right. The diagrams, the math and the 3D model are all drawn from text.

> [!NOTE]
> This document assumes it is shown in mdEditor. On GitHub, the Graphviz (` ```dot `) diagram stays as code, and `\(…\)` math does not show as math. Check the rendering in mdEditor (Identity 8-9 (8)).

<!-- toc -->
- [1. What mdEditor does](#1-what-mdeditor-does)
- [2. PDF and Word, into Markdown](#2-pdf-and-word-into-markdown)
- [3. Answers from AI chats, reproduced as closely as possible](#3-answers-from-ai-chats-reproduced-as-closely-as-possible)
- [4. Math with KaTeX](#4-math-with-katex)
- [5. Diagrams with Graphviz, too](#5-diagrams-with-graphviz-too)
- [6. 3D models turn as you drag](#6-3d-models-turn-as-you-drag)
- [7. Images at any size and position](#7-images-at-any-size-and-position)
- [8. Maps, too](#8-maps-too)
- [9. Code in color](#9-code-in-color)
- [10. The story so far](#10-the-story-so-far)
- [11. Faster with keys](#11-faster-with-keys)
- [12. First steps](#12-first-steps)
<!-- /toc -->

---

## 1. What mdEditor does

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723", "cScale0": "#FFFDF2", "cScaleLabel0": "#3E6B2A", "cScaleInv0": "#3E6B2A", "cScalePeer0": "#3E6B2A", "cScale1": "#F07B5F", "cScaleLabel1": "#1A2212", "cScaleInv1": "#1A2212", "cScalePeer1": "#1A2212", "cScale2": "#9FCC7E", "cScaleLabel2": "#1A2212", "cScaleInv2": "#1A2212", "cScalePeer2": "#1A2212", "cScale3": "#F7A58E", "cScaleLabel3": "#1A2212", "cScaleInv3": "#1A2212", "cScalePeer3": "#1A2212", "cScale4": "#FFE699", "cScaleLabel4": "#7B4A21", "cScaleInv4": "#7B4A21", "cScalePeer4": "#7B4A21", "cScale5": "#F9EFCB", "cScaleLabel5": "#8C1B0D", "cScaleInv5": "#8C1B0D", "cScalePeer5": "#8C1B0D", "cScale6": "#FFFDF2", "cScaleLabel6": "#3E6B2A", "cScaleInv6": "#3E6B2A", "cScalePeer6": "#3E6B2A", "cScale7": "#F07B5F", "cScaleLabel7": "#1A2212", "cScaleInv7": "#1A2212", "cScalePeer7": "#1A2212", "cScale8": "#9FCC7E", "cScaleLabel8": "#1A2212", "cScaleInv8": "#1A2212", "cScalePeer8": "#1A2212", "cScale9": "#F7A58E", "cScaleLabel9": "#1A2212", "cScaleInv9": "#1A2212", "cScalePeer9": "#1A2212", "cScale10": "#FFE699", "cScaleLabel10": "#7B4A21", "cScaleInv10": "#7B4A21", "cScalePeer10": "#7B4A21", "cScale11": "#F9EFCB", "cScaleLabel11": "#8C1B0D", "cScaleInv11": "#8C1B0D", "cScalePeer11": "#8C1B0D", "git0": "#FFE699", "gitBranchLabel0": "#385723"}}}%%
mindmap
  root((mdEditor))
    Open
      PDF
      Word
      Web pages
      Email
      CSV, JSON
    Write
      Syntax colors
      Markdown check
      Bold and lists in one key
      Completion
    View
      Diagrams and charts
      Math
      3D models
      Maps
    Share
      PDF
      Word
      HTML, PNG
      ZIP to hand out
```

| | With mdEditor | |
| :--- | :--- | :---: |
| **Open** | PDF, Word, web pages, email, CSV, JSON, XML and Jupyter notebooks, as Markdown | <kbd>Ctrl</kbd>+<kbd>O</kbd> |
| **Write** | Colors as you type, and wavy lines under mistakes | <kbd>F1</kbd> |
| **View** | Diagrams, math, 3D models and maps, shown at once in the preview beside the editor | <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>2</kbd> |
| **Share** | A PDF that looks just like the preview, in one click | <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> |

---

## 2. PDF and Word, into Markdown

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723"}}}%%
flowchart LR
    subgraph IN["Open"]
        P["PDF"]
        W["Word"]
        H["Web page"]
        A["AI chat"]
    end
    subgraph MD["mdEditor"]
        E["Check and fix<br>in Markdown"]
    end
    subgraph OUT["Share"]
        PDF["PDF<br>(page numbers, tags)"]
        DOC["Word"]
        Z["ZIP to hand out"]
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
> Importing keeps what it can, within these limits. Multi-column layouts, tables without rules and scanned PDFs do not come back as they were. Diagrams and math become images.

---

## 3. Answers from AI chats, reproduced as closely as possible

Markdown copied from an AI chat has its quirks. mdEditor finds them when you open the file and fixes them. What it fixed shows in the notice, and one button puts it back.

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723", "actorBkg": "#F7A58E", "actorTextColor": "#1A2212", "actorBorder": "#1A2212", "actorLineColor": "#385723", "signalColor": "#385723", "signalTextColor": "#385723"}}}%%
sequenceDiagram
    box rgb(159, 204, 126) Internet
    participant AI as AI chat
    end
    box rgb(249, 239, 203) Your PC
    participant You
    participant M as mdEditor
    end
    You->>AI: Ask
    AI-->>You: An answer in Markdown
    You->>M: Paste as a new file
    rect rgb(255, 230, 153)
    M->>M: Fix math, bold and quote marks
    M-->>You: Notice: fixed
    end
    You->>M: Ctrl+Shift+P
    M-->>You: Your PDF is ready
```

For example, math in `\(…\)`, which AI chats often write, shows as math. Like the speed of light, \(c \approx 3.0 \times 10^8\ \text{m/s}\).

\[
E = mc^2 \qquad \text{and} \qquad e^{i\pi} + 1 = 0
\]

---

## 4. Math with KaTeX

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

Math also goes inside a sentence, like $\nabla \cdot \mathbf{E} = \rho / \varepsilon_0$.

---

## 5. Diagrams with Graphviz, too

```dot
digraph promo {
    graph [rankdir=LR, pad=0.2, nodesep=0.35, ranksep=0.45, fontname="Helvetica", bgcolor="transparent"];
    node  [shape=box, style="rounded,filled", fontname="Helvetica", fontsize=12,
           color="#385723", fillcolor="#FFFDF2", fontcolor="#385723", penwidth=1.4, margin="0.2,0.08"];
    edge  [color="#385723", penwidth=1.6, arrowsize=0.8];

    text   [label="Text\n(Markdown)", fillcolor="#FFE699", fontcolor="#385723", color="#385723", penwidth=2.4];
    mermaid [label="Mermaid\nDiagrams, charts", fillcolor="#9FCC7E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    katex   [label="KaTeX\nMath", fillcolor="#F7A58E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    dot     [label="Graphviz\nThis diagram", fillcolor="#FFFDF2", fontcolor="#7B4A21", color="#7B4A21", penwidth=1.6];
    stl     [label="STL\n3D models", fillcolor="#F07B5F", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    geo     [label="GeoJSON\nMaps", fillcolor="#F9EFCB", fontcolor="#3E6B2A", color="#3E6B2A", penwidth=1.6];
    code    [label="Code colors for\nabout 40 languages", fillcolor="#9FCC7E", fontcolor="#1A2212", color="#1A2212", penwidth=1.6];
    pdf     [label="PDF, Word\nHTML, PNG", fillcolor="#F9EFCB", fontcolor="#7B4A21", color="#7B4A21", penwidth=2.4];

    text -> {mermaid katex dot stl geo code};
    {mermaid katex dot stl geo code} -> pdf;
}
```

---

## 6. 3D models turn as you drag

Write an STL in text form and it becomes a 3D model. Grab it with the mouse and turn it. It shows without an internet connection.

Write a part's color after its name, as in `solid star #3E6B2A`, and the background color in a `solid background #F9EFCB` line.

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

## 7. Images at any size and position

Size an image as a share of the text width or in pixels, center it or move it right, or let the text wrap around it (written the same way as on GitHub). Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>F</kbd> (Adjust image) on the image's line, or right-click the image in the preview, and just pick.

<p align="center"><img src="import-test_files/5garashi_identity_gray_reversal_en.png" alt="5garashi.com Design Office Identity, gray reversal of the mark and logotype (60% of the text width, centered)" width="60%"></p>

<p align="center"><img src="import-test_files/5garashi_mark_simple.svg" alt="Symbol mark (32 pixels)" width="32">&nbsp;&nbsp;&nbsp;&nbsp;<img src="import-test_files/5garashi_mark_simple.svg" alt="Symbol mark (64 pixels)" width="64">&nbsp;&nbsp;&nbsp;&nbsp;<img src="import-test_files/5garashi_mark_simple.svg" alt="Symbol mark (96 pixels)" width="96"></p>

<p align="right"><picture><source media="(prefers-color-scheme: dark)" srcset="import-test_files/5garashi_logotype_en_reversed_outlined.svg"><img src="import-test_files/5garashi_logotype_en_outlined.svg" alt="5garashi.com Design Office (right-aligned)" width="200"></picture></p>

<img src="import-test_files/5garashi_mark_simple.svg" alt="5garashi.com Design Office symbol mark" width="80" align="left" hspace="20" vspace="8">The symbol mark is a tally mark made of chili peppers. A fifth pepper runs across four upright ones: count 1, 2, 3, 4, and close the group with the fifth. It draws the name "5garashi" (go-garashi, five peppers) just as it sounds. An image with `align="left"` sits on the left, and this paragraph wraps around its right side.

<br clear="all">

<sub>The images are official assets of 5garashi.com Design Office (CC BY-ND 4.0), used unchanged. In dark mode, the logotype switches to its gray reversed version.</sub>

---

## 8. Maps, too

mdEditor comes in Japanese, English and Simplified Chinese.

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

## 9. Code in color

```python
from pathlib import Path

def count_headings(path: str) -> int:
    """Count the headings in a Markdown file"""
    text = Path(path).read_text(encoding="utf-8")
    return sum(1 for line in text.splitlines() if line.startswith("#"))

print(count_headings("key-features.en.md"))
```

```diff
- A **“light-year”**is a unit of distance.
+ A <strong>“light-year”</strong>is a unit of distance.
```

---

## 10. The story so far

```mermaid
%%{init: {"theme": "base", "themeVariables": {"primaryColor": "#FFE699", "primaryTextColor": "#385723", "primaryBorderColor": "#385723", "lineColor": "#385723", "textColor": "#385723", "secondaryColor": "#F9EFCB", "tertiaryColor": "#FFFDF2", "mainBkg": "#FFE699", "nodeBorder": "#385723", "clusterBkg": "#F9EFCB", "clusterBorder": "#385723"}}}%%
flowchart LR
    d1["<b>Oct 11</b><br>First release"]
    d2["<b>Oct 17</b><br>3D models, maps<br>GitHub syntax"]
    d3["<b>Oct 20</b><br>Adjust image<br>ZIP to hand out"]
    d4["<b>Oct 23</b><br>Simplified Chinese<br>Starts about 2x faster"]
    d5["<b>Oct 25</b><br>Open PDFs"]
    d6["<b>Oct 31</b><br>AI chat Markdown<br>Writing help<br>Markdown check"]
    d1 --> d2 --> d3 --> d4 --> d5 --> d6
    style d1 fill:#FFE699,stroke:#385723,stroke-width:2px,color:#385723
    style d2 fill:#F7A58E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d3 fill:#9FCC7E,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d4 fill:#FFFDF2,stroke:#7B4A21,stroke-width:2px,color:#7B4A21
    style d5 fill:#F07B5F,stroke:#1A2212,stroke-width:2px,color:#1A2212
    style d6 fill:#F9EFCB,stroke:#8C1B0D,stroke-width:2px,color:#8C1B0D
```

---

## 11. Faster with keys

<details>
<summary><b>Keys you will use often (click to open)</b></summary>

| Key | What it does |
| :--- | :--- |
| <kbd>Ctrl</kbd>+<kbd>B</kbd> / <kbd>Ctrl</kbd>+<kbd>I</kbd> | Bold / italic |
| <kbd>Enter</kbd> / <kbd>Tab</kbd> | Continue a list / indent |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>W</kbd> | Tidy a table |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>N</kbd> | Number the headings, or remove the numbers |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>J</kbd> | Insert or update the table of contents |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd> | Save as PDF |
| <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>3</kbd> | Preview in its own window |
| <kbd>F1</kbd> | List of Markdown syntax |

</details>

---

## 12. First steps

- [x] Install mdEditor from the Microsoft Store
- [ ] Open a PDF you have
- [ ] Paste an answer from an AI chat
- [ ] Make a PDF with <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>P</kbd>

> [!IMPORTANT]
> Your documents are never sent anywhere. Converting to and from PDF and Word happens on your PC, too. No ads, no tracking, no account.

---

<p align="center"><img src="import-test_files/5garashi_mark_simple.svg" alt="5garashi.com Design Office symbol mark" width="48"></p>

<p align="center">
<b>mdEditor by 5garashi.com</b><br>
<a href="https://apps.microsoft.com/detail/9NWW2QRCLMHC">Get it from Microsoft Store</a>
</p>

<p align="center"><sub>© 5garashi.com Design Office</sub></p>
