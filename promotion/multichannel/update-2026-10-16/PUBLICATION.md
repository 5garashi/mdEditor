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
| LinkedIn | 原稿・画像を作成。Chromeのfeedがログイン待ちで、未投稿 |
| TikTok | ChromeのStudioがログイン待ち。新機能の実演動画は未制作で、未投稿 |
| Zenn | 既存記事の更新原稿をローカル保存。別リポジトリへの公開は承認待ち、未反映 |
| GitHub Pages | 今回は再公開していない。新しい資料はGitHubの資料ブランチから取得可能 |

YouTubeの動画本体とURLは維持しました。追加機能と動画の実演範囲を分けて、主な追加1.0.8・最新ソース1.0.10・録画1.0.5・Store配信版未確認を説明欄に記載しています。

[同期ジョブ](https://github.com/5garashi/mdEditor/actions/runs/38866905364)は2026-10-16 16:23:52 JSTに正常終了。認証なしの公開動画HTMLでも日英とも更新日と1.0.10の文字列を確認しました。YouTubeクライアントアプリでの表示は未確認です。

最初の同期は、状態ブランチのファイル名末尾にCRが入っていたため停止しました。動画IDとJSONの内容をそのまま保ってファイル名を修復し、Windowsからの保存にNUL終端を使う修正を加えています。16件の既存テストと、その後の本番同期が成功しました。

Zennの正本は、既存連携により `5garashi/galois-theory-introduction` の `articles/mdeditor-diagrams-math-pdf.md` です。自動承認レビューが別リポジトリのmasterへの反映を誤公開リスクとして拒否したため、ユーザーへこの記事1ファイルの公開許可を確認中です。ほかの記事と連携設定は変更していません。

English: Both existing YouTube descriptions were updated and verified on public pages. X, LinkedIn and TikTok require login. The Zenn article update remains local pending explicit approval for its separate connected repository. No new social posts or videos were published.

デザイン参照 / Design reference: [Design Office Identity 4.15.0](https://5garashi.github.io/5garashi-identity/)。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-16 JST
