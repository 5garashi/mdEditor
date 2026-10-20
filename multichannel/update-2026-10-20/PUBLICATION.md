<!--
ファイル名 / File: promotion/multichannel/update-2026-10-20/PUBLICATION.md
概要 / Description: 1.0.14公開用資料 / Release material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-20
版 / Version: 1.12.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 1.0.14の公開記録 / Publication register

2026-10-20 JST。ソース版1.0.14、動画revision 1.0.1、資料キット1.12.0、YouTube公開管理revision 4.0.0。収録対象はローカル更新版であり、Storeの配信版番号と区別します。

| 媒体 / Channel | 結果 / Result | 公開先 / Destination |
| --- | --- | --- |
| GitHub Pages | 日英紹介・資料・制作事例・埋め込みカードを更新。HTTP反映確認 | [日本語](https://5garashi.github.io/mdEditor/) / [English](https://5garashi.github.io/mdEditor/en.html) |
| 資料 | 日英画像・1枚PDF・20機能PDF・サンプル・ZIPを更新 | [日本語](https://5garashi.github.io/mdEditor/resources.html) / [English](https://5garashi.github.io/mdEditor/resources-en.html) |
| YouTube JA | 公開確認済み | [開く](https://www.youtube.com/watch?v=FCAnLgFUx9Q) |
| YouTube EN | 公開確認済み | [開く](https://www.youtube.com/watch?v=ELYbCiZKXMc) |
| TikTok JA | 投稿後の審査を経て「誰でも」を確認。自社ブランド開示・AI生成表示あり | [開く](https://www.tiktok.com/@5garashi_com/video/7698653479656557831) |
| TikTok EN | 投稿後の審査を経て「誰でも」を確認。自社ブランド開示・AI生成表示あり | [開く](https://www.tiktok.com/@5garashi_com/video/7698653923128823047) |
| X | @5garashi_comで公開。日英動画リンク・画像・代替テキストあり | [開く](https://x.com/5garashi_com/status/2112538505989820568) |
| LinkedIn | Anyoneで公開。画像と代替テキストあり | [開く](https://www.linkedin.com/feed/update/urn:li:activity:7518218531650383872/) |
| Zenn | 既存記事を更新。連携元2f470c2、記事1ファイルのみ。公開HTML確認 | [開く](https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf) |
| Reddit r/windowsapps | 既存本文に1.0.14動画リンクを追記。添付動画は旧1.0.12と明記 | [開く](https://www.reddit.com/r/windowsapps/comments/1x8rt4k/) |
| Reddit r/sideprojects | 既存本文に1.0.14動画リンクを追記。添付動画は旧1.0.12と明記。フィルター削除状態は継続。再投稿・追加modmailは実施なし | [開く](https://www.reddit.com/r/sideprojects/comments/1x8shm2/) |
| 事務所ホーム | 日英の紹介ページへの既存リンクを確認。リンク先は更新済み。サイト本体の文言変更は保留 | [5garashi.com](https://www.5garashi.com/) / [差し替え案と理由](OFFICE-HOMEPAGE.md) |

## 公開した内容

3Dの回転とmdEditorの紹介、diagram-testを上から下へ見るツアー、主要機能の紹介という流れを維持しました。画像をドロップし、幅50％と中央揃えにする実操作を追加。冒頭の文書名はmdEditor、BGMは冒頭から鳴るように調整。OSのファイル選択画面は除去し、撮影時に背景を覆っています。

- [動画と制作・検証記録](../../showcase-1.0.14-revision-1.0.1/README.md)
- [今回の資料](README.md)
- YouTube同期：[Actions](https://github.com/5garashi/mdEditor/actions/runs/39389089830)
- YouTube公開：[Actions](https://github.com/5garashi/mdEditor/actions/runs/39389533558)
- Zenn：[対象コミット](https://github.com/5garashi/galois-theory-introduction/commit/2f470c2)

## 確認範囲

日英Webはライト・ダーク、幅1280・640・320px、文字32pxを含む64条件で横はみ出し・画像読み込み・リンク操作領域を確認しました。640pxはリフローの近似で、ブラウザーの実際の200％ズームを行った結果ではありません。公開MP4の日英2本は確認済み原本とSHA-256が一致。4種類のPDFで文字領域がページ内に収まることも確認。YouTube同期の既存16テストは成功。公開YouTubeのタイトルをoEmbedでも確認しました。TikTok Studioで日英とも公開範囲「誰でも」を確認。LinkedInはPost successful、Xは投稿本文・画像代替テキスト・URLを確認しました。

画像・動画内のアプリ画面は収録した状態で、読み上げ・実際のWindowsコントラストテーマ・全形式の変換・復元は今回網羅的に再試験していません。Redditの継続掲載やSNS圧縮後の全フレーム・全音声は未確認です。

## 保留

事務所サイト本体の公開ボタン操作は、自動承認レビューが「既存の未確認変更を直ちに公開する可能性」を理由に拒否しました。公開を迂回していません。mdEditor紹介文の差し替え案は[OFFICE-HOMEPAGE.md](OFFICE-HOMEPAGE.md)に保存。サイト本体は未編集です。GitHub Pages側の日英紹介・資料・カードは公開済みです。

## 出典

Design Office Identity 4.20.0（2026-10-20に公開正本・変更履歴・関係条文を確認）。音楽Light Steps — 5garashi.com Design Office / Codex, CC BY 4.0（抜粋位置の移動・ループ・終端フェード）。サンプル内動画Big Buck Bunny © 2008 Blender Foundation, CC BY 3.0。地図 © OpenStreetMap contributors。原稿・コードMIT、公式ロゴCC BY-ND 4.0。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-20 JST
