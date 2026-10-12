<!--
ファイル名 / File: promotion/IDENTITY-CHECK.md
概要 / Description: mdEditor紹介資料 / mdEditor promotion material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.1.0
ライセンス / License: MIT
最終更新 / Last updated: 2026-10-13
SPDX-License-Identifier: MIT
-->
# Identity適用・検証記録 / Identity and verification record

版 / Version: 1.1.0。確認日: 2026-10-12〜13 JST。

基準: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。原本文書はデザイン資料として参照。第3・4・6〜9・11〜13条に照合しました。

| 条項 / Article | 実装・確認 / Implementation and evidence |
| --- | --- |
| 3 ロゴ | 公式5SVGのGit blob原本一致。pictureで昼夜切替。ロゴ高さ54px、四方28pxを基準に保護余白。右マーク36px、狭幅28px。変形・再着色なし。実画面写真中のロゴは撮影対象アプリの表示。 |
| 4 配色 | 昼・夜パレット。本文ink/paper、カードink/card、注記steel。ヘッダーheadとink 3px罫。印刷は昼。文書の図はcard/inkの固定配色。 |
| 6 書体 | 和文ゴシック、英語はSegoe UI先頭。見出し800、本文600、行間1.7、本文16px、注記14px。文書面はアプリ側の400。動画字幕Meiryo太字24px。 |
| 7 操作 | 自作動画操作を含め44px以上。スキップリンク、3pxフォーカス、キーボード。自動再生・自動音声・自動消去通知・繰り返しアニメーションなし。reduced-motionでanimation/transition停止。 |
| 8 文言 | 実際の用途と操作を先に説明。Store提供版・認定を断定しない。AI生成・自動計算機能がない旨を明示。再生エラーは次の保存操作を案内。 |
| 9 署名 | ファイルヘッダーと文書末尾署名。原本・第三者ファイルは改変せず例外扱い。 |
| 11 音 | 新動画は無音。音源の編集・権利判断が必要な公式BGMを新動画に使用しない。 |
| 12 版・権利 | キット独立VERSION、CHANGELOG、LICENSE、THIRD-PARTY-NOTICES、CC条文。アプリVERSION不変。旧案は保持し配布対象外。 |
| 13 Privacy | ページの保存・通信をPRIVACY.mdに説明。外部フォント・計測・送信機能なし。 |

## ブラウザー / Browser

実際のGoogle Chrome 155.0.8059.39（headless）を起動し、ローカルHTTPで日英×昼夜を検証。通常1280px・320px、実際のブラウザー表示倍率200%（1280pxウィンドウでinnerWidth=640）、ブラウザーの既定文字サイズ200%（body 32px、1280px・320px）で、全20条件の横スクロール・画像・操作領域を確認。

TabとEnter・Spaceで本文への移動、リンク・動画操作部品への移動、手順の開閉を確認。動画の再生・停止・シークは専用の44px以上の操作部品を用意。フォーカス枠は3px。強制カラーとグレースケールはブラウザー模擬で確認。単体確認用は別フォルダーに移し、CSS・画像・動画の埋め込み動作を確認。

検証方法と結果はtools/browser_check.py、validation/browser-results.json、accessible-names.json、画面PNGに保存。実際のChromeで日英6文書をダウンロードし、名前とバイト一致を確認。配布ZIP展開後の日英各20参照先はHTTP 200、MP4のRange要求は206を確認。単体確認用でもキー入力で動画の再生・停止・シークができ、埋め込んだ6文書は原本と一致。結果はvalidation/release-results.json。

200%の設定形式は[Chromium ChromeZoomLevelPrefs](https://github.com/chromium/chromium/blob/main/chrome/browser/ui/zoom/chrome_zoom_level_prefs.cc)に照合。ブラウザープロファイルは作業用で、ユーザーの通常の設定は変更していません。

## コントラスト / Contrast

2026-10-13、WCAGのsRGB相対輝度 `(L明+0.05)/(L暗+0.05)` で計算。本文ink/paperとink/cardは昼夜とも7:1以上。注記・数値は4.5:1以上。操作境界のink、フォーカスのvoltは必要な非テキスト比3:1以上。lineは装飾に限定。

| モード / Mode | 組 / Pair | 比 / Ratio |
| --- | --- | --- |
| day | ink / paper | 7.14:1 |
| day | ink / card | 8.05:1 |
| day | ink / head | 6.66:1 |
| day | steel / paper | 6.41:1 |
| day | steel / card | 7.24:1 |
| day | deep / paper | 8.01:1 |
| day | deep / card | 9.04:1 |
| day | volt / paper | 5.44:1 |
| day | volt / card | 6.15:1 |
| night | ink / paper | 9.09:1 |
| night | ink / card | 8.08:1 |
| night | ink / head | 6.74:1 |
| night | steel / paper | 8.79:1 |
| night | steel / card | 7.81:1 |
| night | deep / paper | 8.37:1 |
| night | deep / card | 7.43:1 |
| night | volt / paper | 6.01:1 |
| night | volt / card | 5.34:1 |

## Windows実機 / Actual Windows app

変更していないmain 6440a2eのmdEditorソース版1.0.5をビルド。日英6サンプルを開き、Mermaid SVG、KaTeX、表、チェックリストの存在を確認。6件のPDF保存を実行し、保存完了ダイアログ、ページ内容、白い余白を確認。結果とPDF・ページ画像はvalidationに保管。

短い技術メモと回答整理のPDFは本文の後に空白ページが1ページあります。文書レビューは2ページに本文があります。これは未改変のアプリでの確認結果で、宣伝キットではアプリの印刷CSS等を修正していません。

## 未確認 / Not verified

- ナレーターやNVDAの実際の読み上げ。アクセシビリティツリーの確認と区別。
- OSでコントラストテーマを選択した実表示。ブラウザー強制カラーの模擬確認とは区別。
- Microsoft Storeから導入した版の動作、現在の認定状況・地域別の購入画面の価格。
- 正式公開先での日英リンク・HTTP応答・ファイル名・内容。公開前のキットなので未実施。
- WCAG 2.2 AAおよびWCAG2ICTの全面的な適合評価。適用と確認範囲を記録し、完全準拠とは表記しない。

静的紹介ページには文書編集・自動バックアップ・復元機能がないため、異常終了時の復元試験は対象外です。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
