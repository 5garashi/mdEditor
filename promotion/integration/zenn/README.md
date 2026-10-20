<!--
ファイル名 / File: promotion/integration/zenn/README.md
概要 / Description: Zenn公開記録と記事の更新元 / Zenn publication record and canonical source
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.3.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# Zenn公開記録 / Zenn publication record

2026-10-20：既存記事を1.0.14の動画・画像調整・ZIP共有へ更新。連携元2f470c2、対象1ファイル。 [今回の公開記録](../../multichannel/update-2026-10-20/PUBLICATION.md)。


2026-10-18：[既存記事](https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf)を1.0.12の3D画面・新動画・主な20機能へ更新。連携元7d19cea、対象記事1ファイルのみ。公開HTMLで反映確認。 [詳細記録](https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/update-2026-10-18/PUBLICATION.md)。

公開記事: https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf

2026-10-17: 既存記事を1.0.10の主画面・実演動画・「名前を付けて保存」の手順へ更新し、公開HTMLで反映を確認しました。連携元コミット91fb19d。Updated and verified the existing article for source build 1.0.10.

タイトル: Markdownを読むためにVS Codeを開くのが面倒で、Windowsアプリを作った

公開日時: 2026-10-13 14:35 JST。アカウント: `5garashi_com`。

## 更新する場所 / Canonical source

今後の記事修正は、[Zenn連携済みの原稿](https://github.com/5garashi/galois-theory-introduction/blob/master/articles/mdeditor-diagrams-math-pdf.md)を編集します。登録済みの `master` ブランチからZennへ自動同期します。既存のガロア理論の記事や連携設定は変更していません。

このフォルダーの `mdeditor-diagrams-math-pdf.md` は公開時点の控えです。記事の更新元として編集せず、必要なときに正本から取り直します。画像・サンプル・アプリ紹介ページは引き続きmdEditorリポジトリのGitHub Pagesに集約しています。

今回の記事を連携済みリポジトリに追加したため、記事原稿とアプリ資料のリポジトリは異なります。mdEditorリポジトリそのものとZennは連携していません。

## 検証結果 / Verification

- 非公開下書きの同期後、Zennの実ブラウザーでタイトル、本文、コード例、実画面画像を目視確認。
- 画像3件、サンプル、紹介ページの公開URLがHTTP 200であることを確認。
- ZennではHTMLコメントの管理情報が本文に表示されたため、公開用原稿の管理情報をYAML frontmatter内のコメントへ移動。本文から消えたことを確認。これはZennの表示に合わせた形式上の例外です。
- 公開後、認証情報を使わないHTTPアクセスで200を確認。取得した記事データの `status: published` と `publishedAt: 2026-10-13T14:35:33.549+09:00` を確認。
- 公開コミット: `2e95587`。追加・修正は今回の記事1ファイルのみ。
- Zenn側の小画面・読み上げ・200%拡大は今回未検証。記事内のWindowsアプリ検証範囲とStore提供版の未確認事項は本文に記載。

記事本文と記法例のMIT表記、公式ロゴのCC BY-ND 4.0表記を維持しました。連携先リポジトリの既存LICENSEは変更していません。

デザイン基準 / Design reference: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。2026-10-13にオンライン正本の版番号を確認。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST
