<!--
ファイル名 / File: PUBLICATION.md
概要 / Description: 更新資料とSNSの反映記録 / Update kit publication record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 1.4.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 公開記録 / Publication record

2026-10-16 JST確認。資料の完成、GitHubへの保存、SNSへの公開を区別します。

| 対象 / Channel | 結果 / Result |
| --- | --- |
| 日英の投稿画像・A4 PDF・投稿文 | 作成・検証済み。promotion/site-integrationへ保存 |
| YouTube English | [既存動画の説明を更新済み](https://www.youtube.com/watch?v=KPgQHi1yYlA) |
| YouTube 日本語 | [既存動画の説明を更新済み](https://www.youtube.com/watch?v=0cZfUyZr01M) |
| X | 原稿・画像を作成。Chromeの投稿画面がログイン待ちで、未投稿 |
| LinkedIn | [英語の更新案内と画像を公開済み](https://www.linkedin.com/feed/update/urn:li:activity:7516763044513886208/)。画像の代替テキストを設定 |
| TikTok | [英語の更新写真投稿](https://www.tiktok.com/@5garashi_com/photo/7697165960637091090)を送信済み。管理画面は「コンテンツ審査中」「自分のみ」。一般公開完了は未確認 |
| Zenn | [既存記事を更新・公開確認済み](https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf)。新機能の追記を反映 |
| GitHub Pages | 今回は再公開していない。新しい資料はGitHubの資料ブランチから取得可能 |

YouTubeの動画本体とURLは維持しました。追加機能と動画の実演範囲を分けて、主な追加1.0.8・最新ソース1.0.10・録画1.0.5・Store配信版未確認を説明欄に記載しています。

[同期ジョブ](https://github.com/5garashi/mdEditor/actions/runs/38866905364)は2026-10-16 16:23:52 JSTに正常終了。認証なしの公開動画HTMLでも日英とも更新日と1.0.10の文字列を確認しました。YouTubeクライアントアプリでの表示は未確認です。

最初の同期は、状態ブランチのファイル名末尾にCRが入っていたため停止しました。動画IDとJSONの内容をそのまま保ってファイル名を修復し、Windowsからの保存にNUL終端を使う修正を加えています。16件の既存テストと、その後の本番同期が成功しました。

Zennの正本は、既存連携により `5garashi/galois-theory-introduction` の `articles/mdeditor-diagrams-math-pdf.md` です。自動承認レビューで一度停止した後、ユーザーからこの記事1ファイルの公開許可を受けました。コミット `8958731` でこの1ファイルだけをmasterへ反映。認証なしの公開HTMLで「2026-10-16追記」「1.0.10」を確認しました。ほかの記事と連携設定は変更していません。

LinkedInは MOTOMASA IGARASHI の既存プロフィールで英語原稿と `update-en-landscape.png` を一般公開し、「Your post was successful」を確認。画像の代替テキストは、取り込み・書き出しの用途と、実画面のフローチャート・表を説明しています。

TikTokは @5garashi_com / 5garashi Design Office で写真1枚の投稿を作成。タイトルは「mdEditor Update — Word, Web & CSV to Markdown」。英語原稿にスクリーンショットによる案内であることを追記し、公開範囲「誰でも」、宣伝表示「あなたのブランド」、即時投稿で送信しました。楽曲・場所は追加せず、AI生成コンテンツの設定はオフのままです。画像はPillowで組んだ文字と公式スクリーンショットで、AI生成の人物・音声・映像を含みません。送信後に管理一覧と投稿IDを確認しました。審査中のため、一般公開・匿名閲覧の完了とは区別します。

English: Both existing YouTube descriptions were updated. The LinkedIn image post and the Zenn article update are published. The TikTok photo post was submitted with public visibility and an own-brand disclosure, and remains under review. X still requires login. No new operation video was recorded.

デザイン参照 / Design reference: [Design Office Identity 4.15.0](https://5garashi.github.io/5garashi-identity/)。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-16 JST
