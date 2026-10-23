---
title: GitHub compatibility test
tags: [markdown, github]
author:
  name: 5garashi.com Design Office
  url: https://www.5garashi.com/
options:
  toc: true
  levels: [2, 3]
---
<!--
  File: samples/github-test.en.md
  Description: Display test for GitHub-compatible Markdown
  Author: 5garashi.com Design Office
  Created: 2026-10-17
  License: MIT
  SPDX-License-Identifier: MIT
-->

# GitHub compatibility test :rocket:

A document to check that what GitHub shows also shows the same way in mdEditor (Japanese: [github-test.md](github-test.md), Chinese: [github-test.zh-CN.md](github-test.zh-CN.md)).

> [!NOTE]
> Useful information.

> [!TIP]
> A helpful hint.

> [!IMPORTANT]
> Something important.

> [!WARNING]
> Something to watch out for.

> [!CAUTION]
> Something risky.

Emoji :smile: :+1: :tada: (the smiley :) stays text)

Math block:

```math
\int_0^1 x^2\,dx = \frac{1}{3}
```

Inline math $`\sqrt{2} \approx 1.414`$ here.

<details>
<summary>Click to open</summary>

A **collapsed** section.

</details>

<kbd>Ctrl</kbd>+<kbd>S</kbd> saves. H<sub>2</sub>O and 2<sup>10</sup>. <mark>Highlighted</mark>

<p align="center"><img src="import-test_files/sample.png" width="120" alt="A centered image"></p>

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="import-test_files/sample.png">
  <img src="import-test_files/sample.png" width="80" alt="A picture that switches between day and night">
</picture>

![Day only](import-test_files/sample.png#gh-light-mode-only) ![Night only](import-test_files/sample.png#gh-dark-mode-only)

## Image size and position

Put the cursor on an image's line in the editor and press Adjust image (Ctrl+Shift+F), or right-click the image in the preview, to have it rewritten like these.

<p align="right"><img src="import-test_files/sample.png" alt="An image at 25% of the text width, on the right" width="25%"></p>

<img src="import-test_files/sample.png" alt="An image on the left with text flowing around it" width="96" align="left">

This paragraph flows on the right of the image. As on GitHub, an image with `align="left"` sits on the left and the text after it runs beside it.

<img src="import-test_files/sample.png" alt="An image on the right with text flowing around it" width="96" align="right">

This paragraph flows on the left of the image.

<br clear="all">

`<br clear="all">` ends the flow, so this paragraph starts below the images.

## E-mail addresses and bars in tables

An e-mail address such as info@example.com becomes a link, as on GitHub.

| Syntax | Meaning |
| --- | --- |
| `a \| b` | A bar inside code (written `\|`) |

## Video

A video beside the document plays as it is (written as an image, as a lone link, or as `<video>`). A video on the internet waits for permission, like an image. PDF, PNG, and Word get the video's name, and saved HTML a link to it.

![Sample video](import-test_files/cube5.mp4)

<video src="import-test_files/cube5.mp4" width="240"></video>

https://www.w3schools.com/html/mov_bbb.mp4

## Removed

As on GitHub, the HTML below is removed (no script runs, the page does not turn red, and no input box appears). The dangerous link keeps only its text, and the internet image waits for permission.

<script>document.title = "hacked"</script>
<style>body { background: red; }</style>
<iframe src="https://example.com"></iframe>
<img src="import-test_files/sample.png" onerror="alert(1)" alt="with onerror" width="40">
<a href="javascript:alert(1)">Dangerous link</a>
<form action="https://example.com"><input type="text" value="input box"></form>
<img src="https://github.githubassets.com/favicons/favicon.png" width="32" alt="An internet image (the GitHub icon)">
