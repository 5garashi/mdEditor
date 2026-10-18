<!--
ファイル名 / File: promotion/integration/reddit/SIDEPROJECTS.md
概要 / Description: Redditでの初回試行と公開記録 / Initial Reddit trial and publication record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.9.3
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# r/sideprojects — 動画投稿 / Video post

**現在の状態：Redditのフィルターにより削除。2026-10-18、所有者の依頼でモデレーターへ投稿URL付きの確認・承認依頼を送信済み。返信・再掲載は未確認です。** 以下の一覧表示・自動再生の確認は投稿直後の履歴です。

2026-10-18、**u/5garashi**から[実演動画付き投稿](https://www.reddit.com/r/sideprojects/comments/1x8shm2/i_built_mdeditor_a_free_opensource_windows/)を公開しました。主媒体として動画を添付し、「本文に追加」は使用していません。コミュニティのカード一覧で、投稿を開かずに動画が表示され、自動再生で時刻が進むことを確認しました。閲覧者の自動再生設定によって再生の動作は変わります。

Published an English video post. Feed display and autoplay were verified in the signed-in desktop Chrome session; other viewers' autoplay preferences remain their own.

| 項目 | 記録 |
| :--- | :--- |
| フレア | Showcase: Open Source |
| 開示 | 開発者本人の投稿、AI支援開発（Codexを含む）を本文に明記 |
| 動画 | [既存の英語実演](mdEditor-windowsapps-en-1.0.0.mp4)、30.65秒、収録ソース1.0.12 |
| 内容 | 解決する用途、主要機能、制約、Store・MITソースへのリンク |
| ルール確認 | [現在のルール](https://www.reddit.com/r/sideprojects/)。詳細説明、過剰宣伝禁止、関係開示、適切なフレアを確認 |
| フレア選択肢 | AI開発向けの区分はルールに記載があるが、投稿時の選択肢にはなし。選択可能なOpen Sourceを使い、本文でAI支援を開示 |
| 確認環境 | ログイン中のWindows Chrome、カード表示 |
| 未確認 | ログアウト時・スマートフォン・自動再生無効時の見え方、再圧縮後の全フレーム・音声 |
| 効果測定 | 翌日・1週間後の閲覧数、コメント、返信時間は未記録。自動監視は設定していない |

投稿前のチェックで詳細説明を求められたため、説明欄への入力を確認してから公開しました。次回も送信前に本文が保持されていることを確認します。

## Title

I built mdEditor: a free, open-source Windows Markdown editor with diagrams and interactive 3D

## Body

I'm the developer of mdEditor, a free Markdown editor and viewer for Windows from 5garashi.com Design Office.

Development is AI-assisted, including Codex. The video is a recording of the real app, not a generated product mockup.

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

A few limits: Windows only. Word import does not preserve colors or layout. GitHub also renders ASCII STL and GeoJSON code blocks in Markdown. Rendering and interaction may differ between GitHub and mdEditor; identical behavior has not been verified. The recording uses source build 1.0.12; I haven't verified the current Store build number.

Which would be most useful in your own workflow: reading Markdown, diagrams/PDF, or bringing existing documents into Markdown?

Clip music: Light Steps, 5garashi.com Design Office / Codex, CC BY 4.0 (excerpt with fades).

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office  
**最終更新 / Last updated**: 2026-10-18 JST  
**デザイン参照 / Design reference**: Design Office Identity 4.20.0、第8条・第9条・第12条。https://5garashi.github.io/5garashi-identity/

## モデレーターへの確認依頼 / Moderator review request

送信者：u/5garashi。宛先：/r/sideprojects。件名：Request to review a post removed by Reddit filters。

```text
Hello r/sideprojects moderators,

Could you please review this post? It now shows that it was removed by Reddit's filters:
https://www.reddit.com/r/sideprojects/comments/1x8shm2/i_built_mdeditor_a_free_opensource_windows/

I am the developer of mdEditor, a free, MIT-licensed Windows Markdown editor. The post includes an actual demo video, the problem it addresses, features and limitations, and links to the Microsoft Store and source code. I also disclosed that development is AI-assisted, including Codex.

I selected Showcase: Open Source because I could not find a vibecoded showcase flair in the available choices. If a different flair or other changes are required, please let me know.

If the post is appropriate for the community, would you consider approving it? I have not reposted it.

Thank you for your time.
u/5garashi
```

## 2026-10-18 訂正 / Correction

GitHubでもMarkdown内のASCII STL・GeoJSONを描画できるため、mdEditor固有との説明を訂正しました。Redditの既存投稿を編集し、保存後の本文で訂正を確認。動画は保持しています。GitHubとmdEditorの表示・操作の完全一致は検証していません。

根拠：[GitHub公式 Creating diagrams](https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams)。
