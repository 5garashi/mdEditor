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
| 初回アクセス許可 | PCの認証ヘルパーを起動。Googleの選択・許可操作は本人に引き継ぎ |
| GitHub Secret | まだ登録していない |
| YouTube API実投稿 | 未実施。release.jsonはenabled:falseのまま |

既存のAuth-5garashiプロジェクトやWorkspace組織全体の認証設定は変更していません。課金登録・VM等の作成は行っていません。

本人が認証を完了すると .youtube-runtime/youtube-credentials.json に保存されます。クライアントJSONはChromeのダウンロードフォルダーにも保存されています。両方とも公開リポジトリへ追加せず、GitHubにはSecrets経由で登録します。

次の作業: 本人のアクセス許可 → 対象チャンネル確認 → GitHub Secrets登録 → 非公開動画の初回同期・実機検証。手順は[README.md](README.md)を参照してください。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
