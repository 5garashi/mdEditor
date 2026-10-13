<!--
ファイル名 / File: promotion/integration/youtube/automation/SETUP-STATUS.md
概要 / Description: Google Cloud初期設定の実施記録 / Google Cloud setup status
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 初期設定の実施記録 / Setup status

2026-10-13 JST、本人から入力操作を依頼された範囲で以下を実施しました。

| 項目 | 設定・確認結果 |
| --- | --- |
| Google Cloudアカウント | moto@5garashi.comのログインを画面で確認 |
| プロジェクト名 | 5garashi YouTube Promotion |
| プロジェクトID | garashi-youtube-promotion |
| プロジェクト番号 | 449331456788 |
| API | YouTube Data API v3を有効化し、状態「有効」を確認 |
| OAuthアプリ名 | 5garashi YouTube Promotion |
| OAuth利用対象 | 内部。5garashi.com組織内 |
| サポート・連絡先 | moto@5garashi.com |
| OAuthクライアント | デスクトップアプリ、5garashi Promotion PC |
| クライアントJSON | ダウンロードし、専用プロジェクトを照合。Git非管理の .youtube-runtime/client_secret.json に保存。内容はログへ出力していない |
| 初回アクセス許可 | 本人による許可が完了。APIで対象チャンネルIDを確認 |
| GitHub Secret | YOUTUBE_CREDENTIALS_JSONを登録済み。値はログへ表示していない |
| YouTube API実投稿 | 日英2本をGitHub Actionsから非公開で同期。release.jsonはenabled:true、operation:sync |

既存のAuth-5garashiプロジェクトやWorkspace組織全体の認証設定は変更していません。課金登録・VM等の作成は行っていません。

本人認証後の情報は .youtube-runtime/youtube-credentials.json に保存されています。クライアントJSONはChromeのダウンロードフォルダーにも保存されています。いずれも公開リポジトリへ追加せず、認証情報はGitHub Secrets経由で登録しました。

## 初回同期の検証 / First live synchronization

[GitHub Actions実行](https://github.com/5garashi/mdEditor/actions/runs/38387765873)が成功しました。

| 動画 | YouTube URL | API読み取り確認 |
| --- | --- | --- |
| English | https://www.youtube.com/watch?v=pxWoPoUcOLI | private、processing:succeeded、タイトル・説明が原稿と一致、英語字幕serving |
| 日本語 | https://www.youtube.com/watch?v=nuddUJCy42k | private、processing:succeeded、タイトル・説明が原稿と一致、日本語字幕serving |

サムネイル設定APIも成功しました。一般公開・限定公開への切り替えはまだ実行していません。動画URLは所有者アカウントでログインして確認します。実プレーヤーでの字幕・画質・透かしの目視確認はまだ未実施です。

同じGitHub Actions実行を再実行し、成功を確認。状態ブランチの動画レコードは2件のままで、両方のIDとready状態が維持されました。再実行で新しい動画を作らず更新できることを実環境で確認しました。

PROMOTION_REPO_TOKENはまだ登録していないため、将来の一般公開に伴うPagesの自動更新は未有効です。現在は公開動画がないので、最新版入口ページの公開も行っていません。

次の作業: 非公開動画を実プレーヤーで確認 → 必要な修正 → 公開範囲を指定してpublish。手順は[README.md](README.md)を参照してください。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
