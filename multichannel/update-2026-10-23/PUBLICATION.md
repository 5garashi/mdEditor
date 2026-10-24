<!--
ファイル名 / File: PUBLICATION.md
概要 / Description: 1.0.15の媒体別公開記録 / Publication register
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-23
版 / Version: 1.13.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# mdEditor 1.0.15 公開記録 / Publication register

2026-10-23〜24 JST（Xは24日に公開）。ソース版1.0.15、動画revision 1.1.0、資料キット1.13.0、YouTube公開管理revision 5.0.0。収録はソースビルドです。

**サイト・YouTube・TikTok・X・Zenn・LinkedInは公開確認済み。Redditは既存本文を更新しましたが2投稿ともフィルター削除表示です。**

| 媒体 / Channel | 確認結果 / Result | 公開先 / URL |
| --- | --- | --- |
| GitHub紹介サイト | 日英中の紹介・資料・動画入口、日英制作事例・カードを更新。公開HTTPを確認 | [日本語](https://5garashi.github.io/mdEditor/) / [English](https://5garashi.github.io/mdEditor/en.html) / [简体中文](https://5garashi.github.io/mdEditor/zh-CN.html) |
| 配布資料 | 3言語の紹介画像・1枚PDF・20機能PDF・サンプル・資料ZIPを更新 | [日本語](https://5garashi.github.io/mdEditor/resources.html) / [English](https://5garashi.github.io/mdEditor/resources-en.html) / [简体中文](https://5garashi.github.io/mdEditor/resources-zh-CN.html) |
| YouTube 日本語 | APIでpublic、公開oEmbedのタイトルを確認 | [動画](https://www.youtube.com/watch?v=xQ4febIk19I) |
| YouTube English | APIでpublic、公開oEmbedのタイトルを確認 | [Video](https://www.youtube.com/watch?v=mRyXDg3urwc) |
| YouTube 简体中文 | APIでpublic、公開oEmbedのタイトルを確認 | [视频](https://www.youtube.com/watch?v=pLHtnbmDRi8) |
| TikTok 日本語 | 自社ブランド・AI生成音楽の開示あり。Studioで「誰でも」を目視確認 | [動画](https://www.tiktok.com/@5garashi_com/video/7699817112948313362) |
| TikTok English | 審査後、Studioで「誰でも」を目視確認 | [Video](https://www.tiktok.com/@5garashi_com/video/7699818110081043719) |
| TikTok 简体中文 | 審査後、Studioで「誰でも」を目視確認 | [视频](https://www.tiktok.com/@5garashi_com/video/7699818620511161618) |
| LinkedIn | MOTOMASA IGARASHI本人プロフィール、Anyone。画像・代替テキストあり。Post successfulを確認 | [投稿](https://www.linkedin.com/feed/update/urn:li:activity:7519355801665261568/) |
| Zenn | 連携元の記事1ファイルのみ更新。公開HTMLに新動画3言語と1.0.15の説明を確認 | [記事](https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf) |
| Reddit r/windowsapps | 本人の既存本文を更新。旧添付動画1.0.12を保持し、新動画3言語へのリンクを追記。フィルター削除表示あり | [既存投稿](https://www.reddit.com/r/windowsapps/comments/1x8rt4k/) |
| Reddit r/sideprojects | 本人の既存本文を更新。旧添付動画を保持。フィルター削除表示あり | [既存投稿](https://www.reddit.com/r/sideprojects/comments/1x8shm2/) |
| X | 2026-10-24、@5garashi_comで送信成功・投稿URLを確認。画像・代替テキストあり | [投稿](https://x.com/5garashi_com/status/2113793970975518721) |

## 今回伝えたこと

新機能は、簡体字中国語の画面・サンプル・読み上げ、クリップボードからの文書新規作成、起動処理の改善です。クリップボード用の起動キーは任意設定で、確認後にショートカットを作成します。起動時間はPC環境によって異なるため、無条件の速度向上率は掲載していません。

従来の強みも合わせて紹介しました。ライブプレビューと同期スクロール、オフラインのMermaid・Graphviz・数式、回転するSTL、地図・文書内動画、画像のサイズ・中央揃え、PDF・Word・ZIPでの共有、広告なし・計測なし・アカウント不要です。

動画は3Dから始まり、各言語のdiagram-testを上から下へ閲覧し、主要機能へ進みます。日本語の文字コード紹介は省き、後半も各言語の文書を背景にしました。BGMは従来のLight Stepsです。クリップボード新規作成は原稿で紹介しており、今回の動画にその操作実演があるとは記載していません。

## 確認した範囲

- 日英中のWebと既存導線をChromeで80条件確認。昼・夜、1280・640・320px、文字32pxで横はみ出し・画像読み込み・リンク領域を検査。640pxはリフローの近似で、実際のブラウザー200％ズームの試験ではありません。
- 公開MP4 3本のSHA-256が確認済み原本と一致。6種類のPDFの文字領域はページ内。6動画のデコードと冒頭100msのBGMは制作段階で検査済み。
- YouTube同期の既存16テストと、中国語入口の追加テスト、計17件が成功。公開API処理・oEmbedで3言語を確認。
- TikTok Studioで新動画3本の「誰でも」を目視確認。LinkedInはPost successful・投稿URL・Anyoneを確認。
- Zenn記事の公開HTMLと、紹介サイトの日英中7ページのHTTP反映を確認。

画面読み上げ・実際のWindowsコントラストテーマ・全形式変換・異常終了からの復元、SNS圧縮後の全フレームと全音声は、今回の販促更新で網羅的に再試験していません。Redditの掲載復帰は未確認です。構文や描画の成功だけを表示品質・アクセシビリティの確認とはしていません。

## 公開元・記録

- 素材と同期設定：[0298b91](https://github.com/5garashi/mdEditor/commit/0298b91)
- YouTube同期：[Actions](https://github.com/5garashi/mdEditor/actions/runs/39943060188)
- YouTube一般公開：[Actions](https://github.com/5garashi/mdEditor/actions/runs/39943318819)
- Zenn連携元：[8f46f2d](https://github.com/5garashi/galois-theory-introduction/commit/8f46f2d) — articles/mdeditor-diagrams-math-pdf.md のみ
- [動画の制作・検証記録](../../showcase-1.0.15-revision-1.1.0/README.md)
- [Web配信確認](live-pages-check.json) / [動画原本一致・PDF検査](public-asset-validation.json) / [YouTube・Zenn公開確認](youtube-zenn-public-check.json)

GitHub ActionsのPages段階は成功表示でしたが、直後のHTTP確認では更新されていなかったため、既存publish.pyからgh-pagesを公開し、実際の反映を確認しました。

事務所のGoogle Sites本体と「お支払いのページ」は変更していません。GitHub Pagesで管理している紹介ページと埋め込み用カードを更新しました。Redditへの再投稿・新たなモデレーター宛メッセージは送っていません。

## 出典

Design Office Identity 4.20.0（2026-10-23に公開正本・変更履歴・関係条文を確認）。Light Steps — 5garashi.com Design Office / Codex, CC BY 4.0（抜粋・反復・終端フェード）。Big Buck Bunny © 2008 Blender Foundation, CC BY 3.0。地図 © OpenStreetMap contributors。原稿・コードMIT、公式ロゴCC BY-ND 4.0。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-24 JST
