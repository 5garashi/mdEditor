<!--
ファイル名 / File: promotion/integration/youtube/automation/CHANGELOG.md
概要 / Description: YouTube自動更新の変更履歴 / YouTube automation changes
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 変更履歴 / Changelog

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
**最終更新 / Last updated**: 2026-10-13 JST
