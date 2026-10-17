<!--
ファイル名 / File: promotion/IMPLEMENTATION.md
概要 / Description: 宣伝資料の実装結果 / Promotion implementation result
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: 1.1.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# mdEditor 宣伝資料の実装結果 / Implementation result

方針どおり、公式素材とサンプルの題材を再利用し、紹介ページ・動画・操作手順を修正しました。現在はローカルで確認できる宣伝キットです。

## 成果物 / Deliverables

- 公開用紹介ページ: [日本語](index.html)、[English](en.html)。promotionフォルダーだけで配置できます。
- 単体確認用: [日本語](review/index.html)、[English](review/en.html)。CSS・JavaScript・公式SVG・実画面・動画・字幕・サンプルを埋め込み、別フォルダーへ移して検証しました。
- Windows実演: [日本語40秒](assets/mdEditor-demo-ja.mp4)、[English 40 seconds](assets/mdEditor-demo-en.mp4)。実際の「開く→編集→図・数式プレビュー→Markdown保存→PDF保存」の操作を収録。無音、日英の焼き込み字幕とVTT。
- 新サンプル: [図と数式のメモ](samples/quickstart-engineering-note.md)を主役に、文書レビューと回答整理を加えた日英6文書。PDF保存手順を具体化し、AI生成・自動計算と誤認しない説明を付けました。
- ZIP: 作業フォルダーdeliverablesに、公開用mdEditor-promotion-1.1.0.zipと単体確認用mdEditor-review-1.1.0.zip。

## 再利用・差替え / Reuse and replacement

公式SVG5点は、原本一致のまま再利用。ページの配色・書体と用途別サンプルの題材は活用しました。CSSの昼夜ロゴ重複をpictureで修正し、ヘッダー下の罫線・右マーク・文書署名を補いました。動画の自作操作領域は44px以上にし、キー操作で再生・停止・位置変更・全画面表示ができます。

旧スライド動画と旧ポスターは新ページ・配布ZIPから除外しました。元ファイルとGit履歴は保管し、削除していません。新版サンプルは独立した版管理のためpromotion/samplesに配置し、アプリ側の旧samples文書を維持しています。

## 検証 / Verification

- Chrome 155.0.8059.39で日英・昼夜の通常表示1280px/320px、実際の表示倍率200%、文字サイズ200%を確認。文字サイズ200%は1280pxと320pxで確認。全20表示条件で横スクロール・欠落画像・44px未満のページ操作領域なし。
- キーボードによる本文への移動、動画再生・停止・位置変更、手順の開閉。ブラウザー強制カラー・グレースケール模擬。アクセシビリティツリーの名前と役割。
- 6サンプルを実際のWindows版で表示し、図・数式・表・チェックリストと6PDFの保存を確認。動画の操作イベントとPDF保存完了を記録。
- 実際のChromeから6文書をダウンロードして、名前・バイト一致を確認。ZIP展開後の参照先は日英各20件がHTTP 200、動画のRange応答は206。単体確認用は独立した場所で動画再生・停止・位置変更・全画面表示とサンプル原本一致を確認。

証拠は[検証記録](IDENTITY-CHECK.md)とvalidationフォルダーのJSON・画面・PDF。Identityの現行版は2026-10-13にも4.12.0を確認しました。

## 残る制約 / Limits

短い技術メモと回答整理のPDFには末尾空白ページが付きます。本文等の欠落は確認していません。原因は未確定で、アプリの印刷CSS等をこの宣伝作業では変更していません。

Store提供版・認定状況・地域別の実際の購入画面の価格、実際のナレーター、OSのコントラストテーマ、正式公開先での動作は未確認です。ページ上の機能説明は、main 6440a2eのソースを変更せずビルドしたmdEditor 1.0.5に基づきます。Store提供版と一致するとは宣伝していません。WCAGの完全準拠を宣言していません。

## Gitと公開 / Git and publication

作業ブランチはpromotion/implementation-review。変更前はaudit/baseline-fb118bfとlaunch-kit-baseline.bundleに保持しています。アプリ本体・root VERSION・ビルド設定・既存README・diagram-testは変更していません。宣伝キットの版はpromotion/VERSIONを正とします。

公開、リモートへのpush、PR #9の更新、mainへのマージ、PR・ブランチ・旧ファイルの削除は行っていません。

デザイン基準出典: [5garashi.com設計事務所 / 5garashi.com Design Office — Design Office Identity](https://5garashi.github.io/5garashi-identity/)。原本文書・公式素材はCC BY-ND 4.0、第9条コード断片はMIT。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
