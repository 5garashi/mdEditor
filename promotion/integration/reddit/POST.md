<!--
ファイル名 / File: POST.md
概要 / Description: r/windowsapps初回投稿原稿 / Initial r/windowsapps developer post
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.12.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# r/windowsapps 初回投稿 / First post

2026-10-20：既存本文にソース1.0.14の新動画リンクと画像調整・ZIPの案内を追記。添付済み動画は旧1.0.12として残しました。[今回の結果](../../multichannel/update-2026-10-20/PUBLICATION.md)。


状態：2026-10-18、u/5garashiから[公開済み](https://www.reddit.com/r/windowsapps/comments/1x8rt4k/i_built_a_free_windows_markdown_editor_with_live/)。固定URLで本文・Developerフレア・動画の再生を確認。

投稿分類 / Flair: **Developer**

## Title

I built a free Windows Markdown editor with live preview, diagrams and interactive 3D

## Body

I'm the developer of mdEditor, a free Markdown editor and viewer for Windows from 5garashi.com Design Office.

I wanted a dedicated app for opening Markdown documents, checking diagrams and equations, and sharing the result without setting up a coding workspace.

The short clip shows the actual app: rotating an STL model, diagrams, editing beside the preview, and an actual exported PDF.

Some things it can do:
- Live preview with synchronized scrolling, plus a detached preview window.
- Mermaid, Graphviz and math rendering using bundled components.
- Interactive STL models, GeoJSON maps and video inside a document.
- Open Word, HTML, email, CSV, JSON and notebooks; save PDF, Word, HTML, PNG and other formats.

The app has no ads or tracking and needs no account. Internet images, video and detailed maps load only when you allow them.

Microsoft Store: https://apps.microsoft.com/detail/9nww2qrclmhc?hl=en-US&gl=US
Source code (MIT): https://github.com/5garashi/mdEditor

A few limits: Windows only. Word import does not preserve colors or layout. GitHub also renders ASCII STL and GeoJSON code blocks in Markdown. Rendering and interaction may differ between GitHub and mdEditor; identical behavior has not been verified. The video demonstrates mdEditor 1.0.12, also available from the Microsoft Store.

Which would be most useful in your own workflow: reading Markdown, diagrams/PDF, or bringing existing documents into Markdown?

Clip music: Light Steps, 5garashi.com Design Office / Codex, CC BY 4.0 (excerpt with fades).

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST

## 2026-10-18 訂正 / Correction

GitHubでもMarkdown内のASCII STL・GeoJSONを描画できるため、mdEditor固有との説明を訂正しました。Redditの既存投稿を編集し、保存後の本文で訂正を確認。動画は保持しています。GitHubとmdEditorの表示・操作の完全一致は検証していません。

根拠：[GitHub公式 Creating diagrams](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)。
