<!--
ファイル名 / File: promotion/MARKDOWN-CHECKLIST.md
概要 / Description: Identity共通チェックリストに対するmdEditor固有の補足 / mdEditor-specific supplement to the shared Identity checklist
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-17
版 / Version: 1.5.4
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# Markdown・Mermaid確認のmdEditor補足

**共通の正本は、Identity側の[Markdown・図の書き方チェックリスト](https://github.com/5garashi/5garashi-identity/blob/main/docs/MARKDOWN-CHECKLIST.md)です。** 原則は[Design Office Identity](https://5garashi.github.io/5garashi-identity/) 第8条 8-9にあります。今回の移行で参照した版は[4.16.0のチェックリスト](https://github.com/5garashi/5garashi-identity/blob/v4.16.0/docs/MARKDOWN-CHECKLIST.md)です。

このファイルはmdEditor固有の描画環境・確認方法・制約・不具合記録だけを管理します。一般的なMarkdownの書き方と確認記録のひな形は複製せず、上の正本を参照してください。プロモーションの入口は[全体図と更新台帳](PROMOTION-SYSTEM.md)です。

English: Shared writing and review rules are maintained by Identity. This file keeps only mdEditor's rendering environment, product-specific checks and incident evidence. A successful render does not confirm readability or accessibility (Identity 8-9 (1), 7-7 (k)).

## 1. 同梱エンジンと記法の確認範囲

2026-10-17、当リポジトリの `wwwroot/lib/mermaid/mermaid.min.js` 内の版情報で **Mermaid 11.17.2** を確認しました。アプリのソース版は今回の収録対象1.0.10、Store配信バイナリの版は未確認です。今後ライブラリを更新した場合は、版と検証結果を更新します。

| 記法・機能 | この資料で確認したこと | 制約・未確認事項 |
| --- | --- | --- |
| `flowchart TD` / `LR` | プロモーション台帳でSVG描画を確認 | 向きやノード数を変えた後も表示確認が必要 |
| `A["文字列<br/>文字列"]` | flowchartの引用符付きラベルで改行表示を確認 | `<br/>`をすべての図種へ共通適用しない |
| `sequenceDiagram` | 台帳の参加者・メッセージ・分岐を描画確認 | 別の改行記法や全構文を検証したものではない |
| `stateDiagram-v2` | 台帳の状態・遷移・別名を描画確認 | 長い遷移ラベルの収まりは図ごとに確認 |
| `timeline` | 旧版台帳で描画自体は成功 | 長い日本語が列からはみ出した。今回の用途には使わず縦型flowchartへ変更 |
| 外部サイトの新記法・拡張 | この資料では利用していない | 同梱エンジンで確認するまで対応済みとしない |

上表は確認した記法の範囲であり、Mermaid全機能の対応一覧ではありません。timelineも構文非対応とは判定していません。ほかの図種の改行・HTMLラベル・拡張記法は、個別検証なしに使用可能と断定しません。

## 2. mdEditorでの確かめ方

1. 対象のソース版・Store版の区別と、同梱Mermaidの版を記録する。
2. mdEditorの「開く」で対象のMarkdownを開く。
3. 各図をプレビューし、通常幅・狭い表示、昼・夜でラベル、枠、矢印、本文の読みやすさを確認する。使用した書体と文字サイズも記録する。
4. 編集してプレビューを再描画し、初回表示だけでなく更新後も崩れないか確認する。
5. 配布する場合は「名前を付けて保存」でPDF・HTMLを選ぶ。PDFは改ページ・図の切れ・余白・リンクを、HTMLはブラウザーで図と本文を確認する。
6. 共通チェックリストの確認記録に、自動確認・目視確認・書き出し確認・未確認条件を分けて記載する。

同梱JSをChromeで直接描画した確認は、同じエンジンの動作確認です。mdEditor本体のプレビュー、テーマ処理、PDF・HTML出力まで確認したという意味ではありません。Identity 8-9 (1)と7-7 (k)に従い、SVG生成の成功だけで表示品質・アクセシビリティを確認済みとしません。

## 3. 製品固有の確認項目

- [ ] ソース1.0.10でPDFを保存する説明は、「名前を付けて保存」でPDF形式を選ぶ手順になっている。旧ボタン「PDFに保存」を現在の手順として案内していない。
- [ ] アプリのソース版、Store配信版、動画revision、資料キットの版を分けて記録した。
- [ ] 別ウィンドウ画像など旧1.0.5の記録は、現在の1.0.10の画面と区別した。
- [ ] 主画面、新しい実演動画、記事中の操作名とリンクが同じ対象版を示している。
- [ ] Word取り込みで色・配置を再現しないこと、Wordへの図・数式は画像になること、数値を自動計算しないことを、対象資料で必要に応じて説明した。
- [ ] 同梱エンジンでの構文・描画確認と、mdEditor本体の表示・書き出し確認を区別した。

## 4. 今回の修正から残す教訓

2026-10-17、プロモーション台帳の横型timelineで、長い日本語が日付ごとのカードからはみ出し、隣の列と重なりました。前回はSVG生成の成功までを確認し、文字の収まりを目視確認できていませんでした。短いラベルと `<br/>` を使う縦型flowchartへ変更しました。

「5図すべて描画成功」は構文・描画の確認です。「文字の重なりがない」は別の確認として記録します。今回の原稿は資料版1.5.3、修正コミットは `ac1bcb2`。共通部分はIdentity 4.16.0へ移管しました。

### 確認記録の実例（2026-10-17）

| 項目 | 実施結果 |
| --- | --- |
| 対象 | [プロモーション台帳](PROMOTION-SYSTEM.md)の5図、資料版1.5.3での修正 |
| 担当・描画環境 | Codex、ChromeとmdEditor同梱Mermaid 11.17.2 |
| 自動確認 | 5図のSVG生成成功、文書の相対リンクを確認 |
| 目視確認 | 修正した日付順フローチャートを900px幅・白背景の画像で確認。文字の枠内収まりと重なりがないことを確認 |
| 根拠 | [検証JSON](validation/promotion-system.json)、[確認画像](validation/promotion-system-flow-1.5.3.png) |
| 未確認 | mdEditor本体でこの文書を開く操作、昼夜両方の実プレビュー、全図の目視、PDF・HTML書き出し、読み上げ、実OSコントラストテーマ |

この実例を、全5図の表示品質やアクセシビリティを一括確認済みとする根拠には使いません。次回はIdentity側の[確認記録のひな形](https://github.com/5garashi/5garashi-identity/blob/main/docs/MARKDOWN-CHECKLIST.md)を使い、その回で実施した条件を記録します。

デザイン参照 / Design reference: [Design Office Identity 4.16.0](https://5garashi.github.io/5garashi-identity/) 第8条 8-9・[docs/MARKDOWN-CHECKLIST.md](https://github.com/5garashi/5garashi-identity/blob/main/docs/MARKDOWN-CHECKLIST.md)。共通原則の管理先はIdentity、製品固有の補足と記録の管理先はmdEditorです。文書はMIT。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-17 JST
