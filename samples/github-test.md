---
title: GitHub 互換テスト
tags: [markdown, github]
author:
  name: 5garashi.com設計事務所
  url: https://www.5garashi.com/
options:
  toc: true
  levels: [2, 3]
---
<!--
  ファイル名 / File: samples/github-test.md
  概要 / Description: GitHub 互換の書き方の表示テスト
  作成者 / Author: 5garashi.com設計事務所
  作成日 / Created: 2026-10-17
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->

# GitHub 互換テスト :rocket:

GitHub が表示する書き方を、mdEditor でも同じように表示できるかを確かめる文書です（英語版: [github-test.en.md](github-test.en.md)）。

> [!NOTE]
> 補足の情報です。

> [!TIP]
> 便利な使い方です。

> [!IMPORTANT]
> 大事なことです。

> [!WARNING]
> 気を付けることです。

> [!CAUTION]
> 危険があることです。

絵文字 :smile: :+1: :tada:（スマイリー :) はそのまま）

数式ブロック：

```math
\int_0^1 x^2\,dx = \frac{1}{3}
```

文中の数式 $`\sqrt{2} \approx 1.414`$ です。

<details>
<summary>クリックで開く</summary>

折りたたまれた **本文** です。

</details>

<kbd>Ctrl</kbd>+<kbd>S</kbd> で保存。H<sub>2</sub>O と 2<sup>10</sup>。<mark>強調</mark>

<p align="center"><img src="import-test-images/sample.png" width="120" alt="中央の画像"></p>

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="import-test-images/sample.png">
  <img src="import-test-images/sample.png" width="80" alt="昼夜で切り替わる画像">
</picture>

![昼だけ](import-test-images/sample.png#gh-light-mode-only) ![夜だけ](import-test-images/sample.png#gh-dark-mode-only)

## 動画

同じフォルダーの動画は、そのまま再生できる（画像の書き方・リンクだけの行・`<video>` のどれでもよい）。インターネット上の動画は、画像と同じく、許可するまで読み込まない。PDF・PNG・Word には動画の名前が、保存した HTML には動画へのリンクが入る。

![サンプル動画](import-test-images/cube5.mp4)

<video src="import-test-images/cube5.mp4" width="240"></video>

https://www.w3schools.com/html/mov_bbb.mp4

## 取り除かれるもの

GitHub と同じく、次の HTML は取り除かれます（スクリプトは動かず、背景も赤くならず、入力欄も出ない）。危ないリンクは文字だけ、外部の画像は許可するまで読み込みません。

<script>document.title = "hacked"</script>
<style>body { background: red; }</style>
<iframe src="https://example.com"></iframe>
<img src="import-test-images/sample.png" onerror="alert(1)" alt="onerror付き" width="40">
<a href="javascript:alert(1)">危ないリンク</a>
<form action="https://example.com"><input type="text" value="入力欄"></form>
<img src="https://github.githubassets.com/favicons/favicon.png" width="32" alt="外部画像（GitHub のアイコン）">
