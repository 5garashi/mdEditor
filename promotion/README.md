<!--
ファイル名 / File: promotion/README.md
概要 / Description: mdEditor紹介資料 / mdEditor promotion material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.9.4
ライセンス / License: MIT
最終更新 / Last updated: 2026-10-18
SPDX-License-Identifier: MIT
-->
# mdEditor 宣伝キット / Promotion kit

2026-10-18：Redditの2投稿を、Storeでも配信中のmdEditor 1.0.12を紹介する指定文へ更新しました。[変更記録](integration/reddit/SIDEPROJECTS.md)。

2026-10-18：r/sideprojectsはフィルター削除、モデレーターへ確認依頼済み。STL・GeoJSONのGitHub対応に関する誤記はRedditの2投稿と原稿で訂正しました。[現在の状態と訂正記録](integration/reddit/SIDEPROJECTS.md)。

2026-10-18：[r/sideprojectsの動画投稿と確認記録](integration/reddit/SIDEPROJECTS.md)を追加。一覧での動画表示と自動再生を確認しました。

2026-10-18：[Reddit初回試行](integration/reddit/README.md)を追加。u/5garashiからr/windowsappsへ[英語の実演付き開発者投稿](https://www.reddit.com/r/windowsapps/comments/1x8rt4k/i_built_a_free_windows_markdown_editor_with_live/)を公開。[反応と返信負担の記録](integration/reddit/TRIAL.md)はここへ戻します。

2026-10-18：[日英の紹介画像・PDF・20機能ガイド・投稿原稿](multichannel/update-2026-10-18/README.md)と紹介・資料ページを、ソース1.0.12の3Dから始まる構成へ更新。動画は背景の映り込みを除いたrevision 1.1.1です。[公開記録](multichannel/update-2026-10-18/PUBLICATION.md)に各媒体の公開URL・確認結果を記録しました。

**更新作業の入口：[プロモーションの全体図と更新台帳](PROMOTION-SYSTEM.md)**。GitHubを核にした各媒体の関係、公開先、今回の実施結果、次回の一括更新手順を5つのMermaidチャートと表にまとめています。Start here for the promotion workflow, publication links and next-release checklist.

2026-10-17: 紹介ページと実操作動画をソース版1.0.10対応へ更新。[制作記録](video-update-1.0.10/README.md)。

作成・修正時はIdentity管理の[共通チェックリスト（参照タグ4.20.0）](https://github.com/5garashi/5garashi-identity/blob/v4.20.0/docs/MARKDOWN-CHECKLIST.md)と[mdEditor固有の補足](MARKDOWN-CHECKLIST.md)を使い、構文・表示・内容を分けて確認します。図はGitHubとmdEditorで共通に描画できるMermaidを選び、両環境の実表示を別々に確認します。

バージョン / Version: 1.9.4 · [CHANGELOG.md](CHANGELOG.md)

Windows版の実操作を使った日英紹介ページ、1.0.10の操作デモ、初めて試すための日英6サンプルです。Word・CSV・Webの取り込みとWord・PDF・HTML・PNGの書き出しを実演します。

## 2026-10-16 更新案内 / Update materials

[日英の更新資料](multichannel/update-2026-10-16/README.md)に、Word・Web・CSVの取り込みとWord・PNG・HTMLへの書き出しを紹介する投稿画像・A4 PDF・SNS原稿を追加しました。主な追加は1.0.8、最新ソースは1.0.10です。新しい1.0.10の実演は[公開記録](video-update-1.0.10/PUBLICATION-2026-10-17.md)を参照。

## 追加した媒体素材 / Added media materials

[媒体別の一覧・使い方](multichannel/README.md)に、日英の投稿文、横長・正方形の投稿画像12枚、用途記事2本、QR付きA4・1ページPDF2枚をまとめました。未投稿・未公開の素材です。無料配布は所有者の確認とStore掲載情報に基づいて記載しています。

## 開く / Open

- 公開用: [日本語](index.html)、[English](en.html)。promotionフォルダーをそのまま配置します。
- 単体確認用: [日本語](review/index.html)、[English](review/en.html)。各ファイルにCSS、操作スクリプト、公式SVG、実画面、MP4、字幕、サンプル、ライセンス案内を埋め込みました。ファイルを別のフォルダーに移しても表示・動画再生・サンプル保存ができます。言語ごとに独立したファイルで、言語切替リンクはありません。
- 実演動画: [日本語MP4](assets/mdEditor-demo-ja.mp4)、[English MP4](assets/mdEditor-demo-en.mp4)。各75秒、1280×720、20fps、H.264、ステレオAAC BGM。字幕を画面下部に焼き込み、VTTも同梱します。
- 主サンプル: [図と数式のメモ](samples/quickstart-engineering-note.md)、[Diagrams and math](samples/quickstart-engineering-note.en.md)。全6文書はsamplesフォルダーにあります。
- 検証: [IDENTITY-CHECK.md](IDENTITY-CHECK.md)。ソース作業コピーのvalidationに画面・PDF・JSONを保存。開発用のtoolsとvalidationは公開用ZIPに含めていません。

Single-file review copies contain all local dependencies. Public pages use the files in this folder. Videos are real Windows recordings with burned-in captions, no music or narration. All six samples were opened in the actual app and exported as PDFs.

## 版と保存状態 / Version and preservation

このフォルダーを独立した宣伝資料の単位として、promotion/VERSIONで管理します。新版サンプルはpromotion/samplesに置き、アプリのVERSIONとは区別しています。アプリ本体、ルートREADME・VERSION・ビルド設定、既存diagram-testと旧quickstart文書は変更していません。

旧PRの状態はaudit/baseline-fb118bfと、作業フォルダーのlaunch-kit-baseline.bundleに保持しています。旧スライド動画mdEditor-intro-en.mp4と旧video-poster.pngは元ファイルを残していますが、新ページと配布ZIPは参照・収録しません。物理的削除は行っていません。

## 検証と制約 / Verification and limits

日英、昼夜、320px、Chromeの実際の表示倍率200%、ブラウザーの既定文字サイズ200%を確認。文字サイズ200%は1280pxと320pxで確認しました。横スクロール、画像欠落、44px未満のページ操作部品を検出しませんでした。強制カラーとグレースケールはChromeで模擬確認。キーボードの移動、本文への移動、動画の操作、手順の開閉を検証しています。

実機版はmain 6440a2eのソースを変更せずビルドしたmdEditor 1.0.5です。Storeから導入した版ではありません。Storeの現在の提供版・認定状況・地域別価格は未確認で、紹介ページでも断定しません。ビルド成功、警告NU1900はネットワーク制限によりNuGet脆弱性情報を取得できなかったものです。

短い技術メモと回答整理サンプルのPDFでは、末尾に空白ページが付きました。本文・数式・図・表の出力は確認しています。文書レビューのPDFは2ページに内容があります。空白ページの原因は未確定で、宣伝資料のためにアプリ側を改変していません。

ナレーターの実際の読み上げ、OSの実際のコントラストテーマ、正式公開先でのHTTP・ダウンロードは未検証です。アクセシビリティツリーで操作部品の名前と役割を確認したことと、スクリーンリーダー検証は区別しています。WCAG 2.2 AAの完全準拠を宣言しません。

## 配置・制作 / Placement and production

公開用ZIPの中身はpromotionフォルダー単位です。index.html、en.html、style.css、player.js、assets、samples、LICENSE、THIRD-PARTY-NOTICES.md、PRIVACY.md、CHANGELOG.md、README.mdを一緒に配置します。アプリのwwwrootや実行ファイルは同梱しません。サンプルの相対リンクはsamples/配下に解決されます。

ローカルHTTP確認例: `python promotion/tools/serve.py --port 8765`。`http://127.0.0.1:8765/`で確認後、サーバーを停止してください。MP4の再生位置変更にはHTTP byte ranges（206 Partial Content）が必要です。簡易サーバーによっては再生位置を変えられないため、同梱の確認サーバーはRange応答に対応しています。公開後には日英リンク、画像、動画、VTT、Markdown保存の内容・名前、Range応答をその公開URLで再検証してください。

再生成（Gitのソース作業コピー向け）: `python promotion/tools/samples.py`、`python promotion/tools/docs.py`、`python promotion/tools/build.py`。素材がない状態でbuild.pyを実行しても完成した単体ファイルにはなりません。録画手順は[RECORDING.md](RECORDING.md)。配布ZIPは`python promotion/tools/package.py`で作成します。

公開・mainへのマージ・PR更新・ブランチや旧成果物の削除は未実施です。

## 出典 / Credits

デザイン基準: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)、5garashi.com設計事務所 / 5garashi.com Design Office。2026-10-13にも現行版を確認。原本文書・公式SVGはCC BY-ND 4.0、第9条のコード断片はMIT。公式SVG5点はGit blobと原本のバイト一致を確認しました。ロゴの変形・再着色はしていません。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST
