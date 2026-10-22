---
title: GitHub 兼容性测试
tags: [markdown, github]
author:
  name: 5garashi.com Design Office
  url: https://www.5garashi.com/
options:
  toc: true
  levels: [2, 3]
---
<!--
  ファイル名 / File: samples/github-test.zh-CN.md
  概要 / Description: Display test for GitHub-compatible Markdown (Simplified Chinese)
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-21
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->

# GitHub 兼容性测试 :rocket:

这是用来检查 GitHub 能显示的写法在 mdEditor 中是否也能同样显示的文档（日语：[github-test.md](github-test.md)，英语：[github-test.en.md](github-test.en.md)）。

> [!NOTE]
> 补充信息。

> [!TIP]
> 实用的提示。

> [!IMPORTANT]
> 重要的事项。

> [!WARNING]
> 需要注意的事项。

> [!CAUTION]
> 有风险的事项。

表情符号 :smile: :+1: :tada:（表情 :) 保持原样）

公式块：

```math
\int_0^1 x^2\,dx = \frac{1}{3}
```

行内公式 $`\sqrt{2} \approx 1.414`$ 也会显示。

<details>
<summary>点击展开</summary>

**折叠**起来的正文。

</details>

按 <kbd>Ctrl</kbd>+<kbd>S</kbd> 保存。H<sub>2</sub>O 和 2<sup>10</sup>。<mark>强调</mark>

<p align="center"><img src="import-test_files/sample.png" width="120" alt="居中的图像"></p>

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="import-test_files/sample.png">
  <img src="import-test_files/sample.png" width="80" alt="昼夜切换的图像">
</picture>

![仅白天](import-test_files/sample.png#gh-light-mode-only) ![仅夜间](import-test_files/sample.png#gh-dark-mode-only)

## 图像的大小和位置

在编辑区中把光标放在图像所在的行并按“调整图像”（Ctrl+Shift+F），或在预览中右键单击图像，可以改写成下面这样的写法。

<p align="right"><img src="import-test_files/sample.png" alt="以正文宽度的 25% 放在右边的图像" width="25%"></p>

<img src="import-test_files/sample.png" alt="放在左边并环绕文字的图像" width="96" align="left">

这一段文字环绕在左边图像的右侧。与 GitHub 相同，`align="left"` 的图像靠左，后面的文字排在它的右边。

<img src="import-test_files/sample.png" alt="放在右边并环绕文字的图像" width="96" align="right">

这一段文字环绕在右边图像的左侧。

<br clear="all">

`<br clear="all">` 结束环绕，这一段从图像下方开始。

## 电子邮件地址和表格中的竖线

像 info@example.com 这样的电子邮件地址会与 GitHub 一样变成链接。

| 写法 | 含义 |
| --- | --- |
| `a \| b` | 代码中的竖线（写作 `\|`） |

## 视频

与文档在同一文件夹中的视频可以直接播放（无论写成图像、只有链接的一行，还是 `<video>`）。网络上的视频与图像一样，获得允许之前不加载。PDF、PNG 和 Word 中放入视频的名称，保存的 HTML 中放入指向视频的链接。

![示例视频](import-test_files/cube5.mp4)

<video src="import-test_files/cube5.mp4" width="240"></video>

https://www.w3schools.com/html/mov_bbb.mp4

## 被移除的内容

与 GitHub 相同，下面的 HTML 会被移除（脚本不会运行，页面不会变红，也不会出现输入框）。危险的链接只保留文字，网络上的图像在获得允许之前不加载。

<script>document.title = "hacked"</script>
<style>body { background: red; }</style>
<iframe src="https://example.com"></iframe>
<img src="import-test_files/sample.png" onerror="alert(1)" alt="带有 onerror" width="40">
<a href="javascript:alert(1)">危险的链接</a>
<form action="https://example.com"><input type="text" value="输入框"></form>
<img src="https://github.githubassets.com/favicons/favicon.png" width="32" alt="网络上的图像（GitHub 的图标）">
