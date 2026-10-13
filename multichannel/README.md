<!--
ファイル名 / File: promotion/multichannel/README.md
概要 / Description: 媒体別素材の一覧と使い方 / Media materials and usage
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.2.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# mdEditor 媒体別素材 / Media materials

版 / Version: 1.2.0。日英の投稿、用途記事、QR付きの1枚PDFを用意しました。すべて未投稿・未公開です。

## SNS投稿 / Social posts

投稿文: [日本語](social/posts-ja.md) / [English](social/posts-en.md)。各3題材に短文・長文・代替テキストを用意しています。

| 題材 / Topic | 日本語 横長 / Landscape | 日本語 正方形 / Square | English landscape | English square |
| --- | --- | --- | --- | --- |
| すぐ読む / Read | [PNG](social/read-ja-landscape.png) | [PNG](social/read-ja-square.png) | [PNG](social/read-en-landscape.png) | [PNG](social/read-en-square.png) |
| 図と数式 / Math | [PNG](social/math-ja-landscape.png) | [PNG](social/math-ja-square.png) | [PNG](social/math-en-landscape.png) | [PNG](social/math-en-square.png) |
| 別ウィンドウ / Window | [PNG](social/window-ja-landscape.png) | [PNG](social/window-ja-square.png) | [PNG](social/window-en-landscape.png) | [PNG](social/window-en-square.png) |

横長は1600×900、正方形は1080×1080。日英・各題材・2サイズで12枚です。同名HTMLは編集用で、CSS・公式SVG・実画面を埋め込んでいます。追加のフォントファイルや外部素材を読み込みません。

短文と対応する画像を1つ選び、本文のStoreリンクから導入できる形で投稿します。画像だけではリンクが押せないため、本文にもリンクを入れてください。代替テキストは各投稿文書にあります。長文は用途の説明に使います。投稿先の現行ルール・文字数・画像の見切れを確認してください。

別ウィンドウ題材は、専用プロファイルで起動したソース版1.0.5の本体と独立したプレビューの実画面を取得し、並べて紹介しています。取得時に変更されたアプリのプレビュー配置設定は復元しました。画面取得の記録はvalidation/separate-window-results.jsonです。Store用スクリーンショットとは別の宣伝画像です。

## 用途記事 / Use-case articles

- [日本語原稿](articles/technical-note-ja.md): Markdownで図と数式の技術メモを作り、PDFで渡す。
- [English draft](articles/technical-note-en.md): Make a technical note with Markdown diagrams and math, then share a PDF.

試せる記法例、編集と配布の考え方、別ウィンドウの用途、PDF確認手順、参考資料を含む手順記事です。mdEditorの開発者による記事であることを明示しています。相対画像・サンプル・動画リンクは、キット内の構造を保ったときに使えます。

Zennや別のブログへ載せる場合は、画像を掲載先にアップロードし、サンプル・動画の相対リンクを公開済みURLに置き換えてください。現在の宣伝ページと追加素材はローカル成果物なので、未公開のリンクをそのまま投稿しないでください。記事本文を著者が確認し、掲載先のルールに合わせて編集してから公開します。

## QR付き1枚PDF / One-page PDF

- [日本語PDF](print/one-page-ja.pdf) / [English PDF](print/one-page-en.pdf)
- [日本語プレビュー](print/one-page-ja.png) / [English preview](print/one-page-en.png)
- 編集用HTML: [日本語](print/one-page-ja.html) / [English](print/one-page-en.html)

各A4縦・1ページ。用途・実画面・無料配布・開発者を示し、Storeと事務所サイトのQRを載せています。PDFのURLはクリックもできます。印刷時は用紙A4・倍率100%を基準にします。用紙に合わせる縮小やQRのトリミングは、読み取りに影響するため避けてください。

| QR | 読み取り先 / Destination |
| --- | --- |
| 無料で試す / Try it | https://apps.microsoft.com/detail/9nww2qrclmhc |
| 開発した事務所 / Developer | https://www.5garashi.com/ |

QRは短縮URLや計測サービスを使わず、正式URLへ直接つなぎます。四方4モジュールの余白、誤り訂正M。PNGとSVGをqrフォルダーに用意しています。ロゴや装飾を重ねないでください。

## 確認結果と制約 / Checks and limits

Chromeで画像12枚を出力し、実寸と文字・画像・フッターの欠けを確認。日英PDFは各1ページ、A4相当、白い余白、文字の抽出、リンク、タグ付きPDFの生成を確認しました。OpenCVで元のQRと、PDFを画像化した状態のQRを読み取り、両方の正式URLに一致しました。

実際の紙への印刷、スマートフォンのカメラ、SNSの投稿後の縮小・切り抜き、スクリーンリーダーによるPDFの読み上げは未検証です。PDF/UAの完全準拠とは表記していません。画像内の小さいアプリ文字は補助的な実画面で、用途の説明は大きい本文と投稿文で伝えます。

無料は所有者の確認とStore掲載情報に基づきます。画面・機能の検証は未改変のソース版1.0.5。Storeの提供版番号や認定状況を断定しません。「VS Codeより速い」「唯一の機能」といった未測定・未確認の比較は使っていません。

今回の成果物は、まだ発信実績や集客成果ではありません。公開先・投稿日・実際の反応を確認した後、事務所の制作・発信事例として記録してください。

## 再生成・配布 / Rebuild and package

制作用: Python、qrcode 8.2、Chrome、PyMuPDF、OpenCV、Pillow、ローカルappAutoTesterのCDPヘルパー。qrcodeは作業フォルダーのwork/media-depsに入れています。フォントはインストール済みのYu Gothic・Meiryo・Segoe UIを使い、フォントファイルは配布しません。

ソース作業コピーで、`python promotion/tools/multichannel.py`、`python promotion/tools/articles.py`、`python promotion/tools/render_multichannel.py`の順に実行します。`render_multichannel.py`は専用のChromeプロファイルを使って終了時に閉じます。原稿は生成スクリプトを編集してから再生成します。

作業フォルダーdeliverablesのmdEditor-multichannel-1.2.0.zipには、この素材と記事が参照する画像・動画・サンプル、ライセンス文書をまとめています。公開用キットmdEditor-promotion-1.2.0.zipにも追加素材を含めています。旧1.1.0のZIPとGit保全ブランチを維持しています。

デザイン基準出典: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)、5garashi.com設計事務所 / 5garashi.com Design Office。公式SVGは改変せず使用し、CC BY-ND 4.0を維持。文章・レイアウトはMIT。[権利表記](../THIRD-PARTY-NOTICES.md)を参照してください。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
