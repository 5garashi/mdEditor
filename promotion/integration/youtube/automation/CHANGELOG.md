<!--
ファイル名 / File: promotion/integration/youtube/automation/CHANGELOG.md
概要 / Description: YouTube自動更新の変更履歴 / YouTube automation changes
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.0.2
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 変更履歴 / Changelog

## 1.0.2 — 2026-10-16

- Windowsで状態ファイル名末尾にCRが入る問題を、GitツリーのNUL終端で修正。既存状態の動画IDと内容を保ってファイル名を修復。

- 日英の動画説明にWord・Web・CSV取り込みとWord・PNG・HTML書き出しの案内を追加。動画の1.0.5と最新ソース1.0.10を区別。
- Existing video descriptions updated; no new recording or video upload.

## 1.0.1 — 2026-10-13

- 公開直後のYouTube状態反映待ちに対応し、読み取りだけを上限付きで再実行。
- テストの公開範囲を本番のrelease.jsonから分離。16件のテストを確認。
- 本人の指示で日英の動画を一般公開。APIのpublic状態と匿名oEmbedのHTTP 200を確認。
- Store版1.0.5の公開を、開発者からの確認として日英の動画説明に反映。

## 1.0.0 — 2026-10-13

- 日英2本の動画を対象とする公開設定ファイルを追加。初期値はenabled:false。
- GitHub Actionsによる明示的な同期・公開、永続的なアップロード予約、動画IDの保存、途中失敗の回収を実装。
- 公開済み動画の説明・字幕・サムネイル更新と、新しいMP4の非公開アップロードを区別。
- 一般公開確認後の最新版入口ページ生成と旧動画へのリンク追記を追加。
- Google Cloud作成、初回OAuth、GitHub Secrets、日常更新と復旧の操作手順をREADMEに記載。
- ローカル14件のテストが成功。Googleクライアント導入とAPIメソッドの読み込み、既存サイトの生成が成功。
- [GitHub Actionsの検証実行](https://github.com/5garashi/mdEditor/actions/runs/38371234040)が成功。投稿は無効のままで、API認証・実投稿は未実施。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-16 JST
