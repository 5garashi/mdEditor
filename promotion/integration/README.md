<!--
ファイル名 / File: promotion/integration/README.md
概要 / Description: GitHubで一元管理する公開構成 / Single-source publication plan
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# GitHub一元管理と5garashi.comからの導線 / Single-source publication

資料原本は `5garashi/mdEditor` の `promotion/` に置きます。公開HTML・画像・動画・サンプル・PDFをGitHub Pagesから配信し、5garashi.comには紹介文とリンクだけを置きます。記事の原本はGitHubのMarkdown表示で読みます。動画、PDFをGoogle Sitesに複製しません。

これは未公開の構成案とコードです。URLは公開予定URLで、既存のPages設定・他のPages用途・管理権限は未確認です。公開前に競合がないか確認します。既存サイト全体の置き換えは行いません。

## 構成 / Structure

| 5garashi.comに置く項目 | 接続先（公開予定） |
| --- | --- |
| Microsoft Storeで無料入手 / Get it free | 現在のStoreリンクを維持 |
| 使い方・実演を見る / See how it works | https://5garashi.github.io/mdEditor/ （日本語） / https://5garashi.github.io/mdEditor/en.html |
| サンプル・紹介PDF / Resources | https://5garashi.github.io/mdEditor/resources.html / resources-en.html |
| 制作事例 / Case study | https://5garashi.github.io/mdEditor/case-study.html / case-study-en.html |

紹介ページ → Store・実演動画・サンプル。資料ページ → サンプル・技術記事・紹介PDF・投稿文。制作事例 → 制作物と役割・既存の問い合わせ先。日英は別ページで、互いに切り替えられます。制作事例は制作内容を示し、未確認の宣伝効果を主張しません。

## ローカルで見る / Local review

リポジトリのルートで実行します。Python標準ライブラリだけを使います。

```powershell
python promotion/integration/build.py --output ../work/github-pages-preview
python -m http.server 8766 --bind 127.0.0.1 --directory ../work/github-pages-preview
```

`http://127.0.0.1:8766/` と `resources.html`、`case-study.html`、各英語ページを開きます。動画のシーク確認には既存のRange対応サーバーを使用してください。簡易サーバーは配信確認用です。

生成先は新しい専用フォルダーを指定してください。同じ出力先はファイルを上書きしますが、不要になったすべての古いファイルを掃除する機能はありません。生成先を原本の `promotion/` に指定しないでください。

`office-card-ja.html` と `office-card-en.html` は5garashi.com用の単体紹介枠です。CSSと公式SVGを埋め込み、外部アセット読み込み不要。リンクはPagesの公開予定URLで、公開後に使用します。

## Google Sitesに置く / Add to Google Sites

簡単な方法: Google Sitesの編集画面で、日英の短い説明を入れ、「Microsoft Storeで無料入手」「使い方・実演を見る」の二つのボタンを作り、上表のURLを設定します。既存のStoreボタンは維持します。これならGoogle Sites側で見た目を調整できます。

HTMLを使う方法: 「挿入」→「埋め込む」→「埋め込みコード」で `office-card-ja.html` または英語版の全文を貼ります。表示を確認し、枠の高さを調整します。埋め込みはiframeなので、CSSはGoogle Sites全体に適用されません。リンクは新しいタブで開きます。Google Sites内の実表示・拡大・iframeの高さは未確認です。

5garashi.com側には短い導入だけを置き、詳しい説明はPagesに集約します。紹介枠の文面を変える場合はGitHubの原本を更新して貼り直します。ボタンのリンクだけにすれば通常の資料更新時に貼り直しは不要です。

## GitHub Pages公開の手順 / Publication

1. ユーザーの判断後、レビューした変更をGitHubに送り、mainへ統合します。現在のmain・PRは変更していません。
2. 他のPages用途がないことを確認します。競合がある場合は専用リポジトリを使い、build.pyのBASEと原本リンクを変更します。
3. `pages.yml.example` を `.github/workflows/promotion-pages.yml` として配置します。現在は例示ファイルなので自動実行されません。
4. GitHubリポジトリの Settings → Pages → Source で GitHub Actions を選びます。github-pages環境の公開ブランチをmainに制限します。
5. Actionsの「Publish mdEditor materials」からmainを選んで手動実行します。成功後、公開URLで日英のページ・動画・サンプル・PDF・記事リンクを確認します。
6. 確認後、5garashi.comのリンクを追加・公開します。

ワークフローは手動起動だけです。mainへの変更だけでは公開せず、レビュー後に実行します。Actionsの版は導入時に公式リリースと確認してください。参考: [GitHub Pagesの公開ソース設定](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site)。`promotion/` はブランチ直接公開で選べる `/`・`/docs` と異なるため、Actionsで必要な資料だけを生成・配信します。

## 更新 / Maintenance

編集元は promotion/index.html・en.html・multichannel・samples・assets。既存の資料生成スクリプトで管理する内容はその生成元も直します。生成した `_site` やローカルプレビューを手編集しません。記事は既存Markdown一つを維持し、GitHubへのリンクで読みます。資料キット1.2.0と導線1.0.0の版は別に記録します。配布ZIPとPagesは同じ原本から生成しますが、今回既存ZIPは更新していません。

サイト訪問者はPagesの紹介を読み、Storeで入手します。開発相談は制作事例から既存問い合わせページへ進みます。Google Sites→GitHub Pagesへのドメイン移動があるため、ボタンの近くに「紹介ページはGitHub Pagesで公開」と記載してもよいです。

## 検証と制約 / Checks and limits

公式SVGと既存のIdentity配色・書体・昼夜処理を再利用。動画の自動再生、アクセス解析、広告コードは追加していません。新規ページは既存のフォーカス、強制カラー、縮小幅のCSSを使用します。生成ファイルの参照先と実ブラウザーの表示確認結果は validation.json に記録します。既存アプリ側のPDF末尾の空白ページ、Store提供版未確認、実際のGoogle Sites埋め込み・公開配信・読み上げ未確認は継続します。完全準拠を主張しません。

公式ロゴはCC BY-ND 4.0、作成コード・文面は既存資料のMITを維持。出典・LICENSE・THIRD-PARTY-NOTICES・PRIVACYを配信物に含めます。基準: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。

English: Keep all material sources in promotion/ in the mdEditor repository. Publish a generated static site through a manually triggered GitHub Pages workflow. The office site links to that site and the Store. The examples are not deployed; verify the repository's existing Pages configuration first. Use office-card-en.html for an optional Google Sites embed. Original Markdown articles remain on GitHub.

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
