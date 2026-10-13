<!--
ファイル名 / File: promotion/integration/youtube/automation/SETUP-STATUS.md
概要 / Description: Google Cloud初期設定の実施記録 / Google Cloud setup status
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.0.1
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
| YouTube API実投稿 | 日英2本を非公開同期後、本人の明示的な承認で一般公開。release.jsonはenabled:true、operation:publish、target_privacy:public |

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

## 一般公開 / Public release — 2026-10-13

所有者から「動画内容は後で修正するので、現在の動画を公開」と明示的な指示を受け、日英2本を一般公開しました。動画の再録画は行っていません。

- 日本語: https://www.youtube.com/watch?v=nuddUJCy42k
- English: https://www.youtube.com/watch?v=pxWoPoUcOLI
- YouTube APIで両方のprivacyStatus:publicを再取得して確認。
- 認証情報を付けないYouTube oEmbedの取得が両方ともHTTP 200であることを確認。
- 説明文には、Store版1.0.5公開を2026-10-13の開発者確認として反映。録画がソース版1.0.5であることを維持。Storeから入手した版での録画・再検証とは記載していません。
- [公開確定の自動処理](https://github.com/5garashi/mdEditor/actions/runs/38399050166)が成功。
- 公開直後の読み取りで反映待ちが生じたため、上限付きの状態再取得を実装。テスト用公開範囲を本番設定から独立させ、16件のテストを確認。

最新版の動画ID・URLはpromotion/youtube-stateに保存済みです。PROMOTION_REPO_TOKENが未設定のため、この実行では紹介ページの自動更新をスキップしています。YouTubeの一般公開自体は完了しています。

## 紹介ページとの接続 / Landing-page integration — 2026-10-13

その後、ローカルのpublish.pyから最新版の状態を読み込み、既存のgh-pagesへ公開しました。WindowsでGitのUTF-8出力を読み取る修正を行っています。

- 日本語入口: https://5garashi.github.io/mdEditor/youtube.html
- 英語入口: https://5garashi.github.io/mdEditor/youtube-en.html
- 両方のHTTP 200と、各言語の正しい動画IDを確認。日英紹介ページにも入口リンクを追加。
- Pages公開コミット: 3c256915b46d6ff03b1c5c03263068419ee70ebf。
- 今回の公開はローカルで実施。PROMOTION_REPO_TOKEN未登録のため、将来のActionsからのPages自動更新は引き続き未有効。

## 操作デモ更新 / Extended demos — 2026-10-14

revision 1.1.0を日英とも一般公開しました。別ウィンドウ、ダークモード、Syntax（書き方）、Light Steps BGMを含みます。

- English: https://www.youtube.com/watch?v=KPgQHi1yYlA
- 日本語: https://www.youtube.com/watch?v=0cZfUyZr01M
- [公開ワークフロー](https://github.com/5garashi/mdEditor/actions/runs/38436292206)成功。APIでpublicと字幕serving、匿名oEmbedのHTTP 200を確認。
- 旧日英動画は保持して説明文に新版リンクを追加。latestの状態も新版IDへ更新。
- 元MP4は75秒。YouTube側の再生時間表示は1分16秒。
- 紹介ページと最新YouTube入口はクリーンなworktreeからpublish.pyで反映済み。公開MP4の原本との一致も確認。
- 初回のpublish要求は新revision未同期のため停止し、その後sync → publishの順に実行。新revisionの公開時には同じ順序が必要。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-14 JST
