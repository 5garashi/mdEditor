<!--
ファイル名 / File: promotion/integration/DEPLOYMENT.md
概要 / Description: GitHub Hubとホームページの公開記録 / Hub and website deployment record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.1.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 公開記録 / Deployment record

2026-10-13 JST。依頼: GitHubをHubにして媒体パッケージを構築し、GitHubと5garashi.comのGoogle Sitesホームページへ実装する。

## GitHub

- Repository: https://github.com/5garashi/mdEditor
- 資料原本 / Source: https://github.com/5garashi/mdEditor/tree/promotion/site-integration/promotion
- 公開生成物 / Published output: gh-pages ブランチのルート。mainと既存PR #9は変更・削除・マージしていません。
- 最初の公開原本コミット: ec78954。最初のgh-pagesコミット: 5444a6c28766f15893fb7fbe83598f1b1c881b68。
- Published site: https://5garashi.github.io/mdEditor/
- English: https://5garashi.github.io/mdEditor/en.html
- Resources: https://5garashi.github.io/mdEditor/resources.html / resources-en.html
- Case study: https://5garashi.github.io/mdEditor/case-study.html / case-study-en.html
- 公開原本SHAは配信する SOURCE-COMMIT.txt に記録。更新方法は publish.py。自動公開ワークフローの設置はしていません。

GitHub認証済みの所有者権限を確認。Pagesは最初404、gh-pagesのpush後はbuilt、公開元はgh-pages /。Pages作成APIはalready enabledの409を返したため、再設定・上書きせず既存の公開元が意図したものになったことを確認しました。カスタムドメイン変更はしていません。

## Google Sites / 5garashi.com

https://www.5garashi.com/ のHomeに「mdEditor 日本語」「mdEditor English」を追加・公開。配置は既存のお問い合わせ等の案内の後です。接続先は上記の日英紹介ページ。既存のRecent Works、Storeリンク、事務所案内を維持。

最初はGitHub Pagesの紹介枠をiframeで追加・公開。実ブラウザーの390px幅で枠が小さくなり、内部スクロールで操作が隠れることを確認。この二つの新規セクションだけを削除し、幅広の日英ネイティブボタンへ置き換えて再公開しました。紹介枠HTMLは別環境で使える参考コードとしてGitHubに残しています。Google Sites全体のテーマ・既存のロゴ・文面は変更していません。

公開前の確認画面はHomeのみの更新を表示。Google Sitesの公開後HTMLとChromeの公開画面でリンクを確認しました。Google Sitesの外部リンクはGoogleのURL転送を経由するため、転送先qパラメーターが日英のPages URLに一致することも確認しました。

## 検証 / Verification

- Chrome 155.0.8059.39で新規日英ページ・紹介枠の昼夜、1280px、320px、640pxの再配置、320px文字32pxを48条件確認。横はみ出し・画像欠落・自作リンク領域44px未満なし。
- 640pxは200%拡大時の幅に相当する再配置確認。今回の追加ページでは実際のブラウザー200%ズームを検証していません。
- 公開URL13個をHTTP 200確認。日英ページ、資料一覧、制作事例、紹介枠、PDF二つ、サンプル一つ、実演動画二つ。
- Google Sitesの公開ページは1280pxと390pxで日英ボタンを確認。幅広・高さ54px、横はみ出しなし、埋め込み削除済み、転送先一致。
- ローカルの参照先確認は validation.json、公開結果は live-validation.json に記録。
- Storeの提供版・認定状況、Google Sites全体のIdentity/WCAG準拠、Narrator、実際のWindowsコントラストテーマ、全リンクを人がクリックした操作は未確認。アプリ由来のPDF末尾空白ページの制約は継続。

## 次の媒体 / Next channels

LinkedIn、Zennへの投稿は未実施。原本素材と実装順はCHANNELS.md。LinkedInの投稿先プロフィール・会社ページ、Zennのアカウントが分かった後、LinkedIn→Zennの順に公開します。X、YouTube、TikTokは後続。認知や相談につながった成果は未計測です。

English: Published the promotion hub through gh-pages and added Japanese and English links to the office website. Main and draft PR #9 remain unchanged. Embedded cards were replaced with native wide links after a mobile layout issue. LinkedIn and Zenn are the next channels and have not been posted.

デザイン基準 / Design reference: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。コード・文面はMIT、公式アセットはCC BY-ND 4.0を維持。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
