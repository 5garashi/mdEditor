<!--
ファイル名 / File: promotion/integration/CHANNELS.md
概要 / Description: GitHub Hubと媒体の実装順 / Promotion hub and channel sequence
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.1.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# GitHub Hubと媒体の運用 / Channel operations

資料の原本は promotion/、公開サイトは生成した gh-pages ブランチ、作業・レビューは promotion/site-integration ブランチです。gh-pages を手編集せず、原本から生成して公開します。mainへの統合前でも原本リンクは作業ブランチを指して動作します。統合後はbuild.pyのSOURCE_REFをmainに変更して再公開できます。

| 順番 | 媒体 | 原本・使用素材 | 作業 |
| --- | --- | --- | --- |
| 1 | 5garashi.com (Google Sites) | 公開紹介ページ index.html / en.html | 公開済み。日英の幅広リンクボタンを追加。Store直リンクを維持 |
| 2 | LinkedIn | multichannel/social/posts-ja.md / posts-en.md の長文、landscape PNG | 開発者として投稿。英語は英語の紹介ページへ、日本語は日本語へ |
| 3 | Zenn | multichannel/articles/technical-note-ja.md | 手順記事として編集・確認。画像はアップロード、サンプルは公開Pages URLに変更 |
| 後続 | X | social/posts-*.md の短文、landscape PNG | 本文と画像・代替テキストを組み合わせる |
| 後続 | YouTube | assets/mdEditor-demo-ja.mp4 / en.mp4 | 横長の操作実演。概要欄は紹介ページ・Store・事務所のリンク |
| 後続 | TikTok | 既存動画は参考用 | 縦画面で文字が読める実演を別途制作。横長動画を未検証のまま完成扱いしない |

5garashi.comとLinkedIn英語投稿は2026-10-13に公開確認済み。LinkedInの公開原稿・画像・URLは[LINKEDIN-FIRST-POST.md](LINKEDIN-FIRST-POST.md)に記録しています。Zenn、X、YouTube、TikTokへの投稿はまだ行っていません。次にZennを進めます。配布済み素材と公開済み投稿を区別し、公開URL・日付・使用した資料版を記録します。SNSの閲覧・相談件数などは実際に確認できた結果だけ記録します。

## Google Sites: GitHubに集約する埋め込み / Single-source embed

公開後の実ブラウザー確認で、390px幅ではGoogle Sitesがiframeを約200px高へ縮め、入手ボタンが枠内のスクロール先に隠れました。このためホームページの埋め込みは外し、「mdEditor 日本語」「mdEditor English」のネイティブリンクボタンへ変更しました。以下の埋め込みコードは別環境向けの参考です。現在のホームページには使用していません。ボタンの接続先は紹介ページ日英のURLで、原本・資料・公開ページはGitHubで一元管理します。

Google Sites編集画面の「挿入」→「埋め込む」で、次のURLを指定します。貼り付けたHTML全体をSites内で複製する方法より、公開した紹介枠URLを埋め込む方法を推奨します。紹介枠もGitHubで更新できます。

- 日本語: https://5garashi.github.io/mdEditor/office-card-ja.html
- English: https://5garashi.github.io/mdEditor/office-card-en.html

HTMLのiframeを使う場合（Google Sitesでは「埋め込みコード」）:

```html
<iframe
  src="https://5garashi.github.io/mdEditor/office-card-ja.html"
  title="mdEditorの紹介と入手方法"
  width="100%"
  height="620"
  style="border:0"
  loading="lazy">
</iframe>
```

高さ620pxは開始値です。320px幅、文字拡大、日英で確認して必要な高さに調整します。Google Sites側の外枠も調整します。公開紹介枠はリンクが新しいタブで開くことを明記し、iframe内の遷移でサイト全体の操作を閉じ込めません。公開配信の埋め込み可否・Google Sites内での表示は実機確認が必要です。

## LinkedIn: 初回公開済み / First post published

長文投稿にStoreリンクを残し、使い方の公開ページを追加します。日本語 https://5garashi.github.io/mdEditor/ 、英語 https://5garashi.github.io/mdEditor/en.html 。画像は read-ja-landscape.png / read-en-landscape.png から開始します。投稿先の個人プロフィールまたは会社ページURL、投稿者名、公開する言語を確認して投稿します。本人の開発作品であることを明記します。

## Zenn: その次の実装 / Following channel

記事の検証・開発者表示を維持し、宣伝だけの記事にしません。相対サンプルリンクを https://5garashi.github.io/mdEditor/samples/quickstart-engineering-note.md に変更。本文画像はZennへアップロードして、そのURLへ差し替えます。開発相談のリンクは末尾にまとめます。公開記事URLをGitHubに記録し、記事内容を改訂した場合は原本へ反映します。Zennへの転載後は同期を自動化していません。

English: Maintain source materials on GitHub and publish a generated gh-pages branch. The office site now uses two native links to the Japanese and English pages. Embedded cards were removed after live narrow-screen checks revealed nested scrolling. The office website was implemented first; LinkedIn is next, then Zenn. Other channel posts are not published yet; TikTok needs a separate readable portrait recording.

デザイン基準 / Design reference: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。コードはMIT、公式SVGは原本のCC BY-ND 4.0を維持します。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
