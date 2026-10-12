<!--
ファイル名 / File: promotion/IDENTITY-CHECK.md
概要 / Description: Identity適用と確認範囲 / Identity application and verification scope
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.0.5
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# Identity適用記録 / Identity application record

基準 / Reference: Design Office Identity v4.12.0（2026-10-12）。公式リポジトリの現行HTMLと素材を参照しました。

| 条項 / Article | 適用 / Application | 確認 / Verification |
| --- | --- | --- |
| 3 ロゴ / Logos | 公式SVGを改変せず使用。昼夜で原本を切替。四方に余白 / Unmodified masters, day/night variants and clear space | 素材を原本とバイト比較 / Byte comparison |
| 4 配色 / Colors | 公式の昼夜パレット、主要ボタンhead×ink、印刷は白地 / Official palettes and white print ground | CSSと文字色の比を確認 / CSS and contrast calculations |
| 6 書体 / Typography | 和文ゴシック、英語UI書体。見出し800、本文600、行間1.7 / Language-specific stacks and weights | CSS指定を確認。動画はNoto Sans Bold / Source review; video uses Noto Sans Bold |
| 7 操作 / Interaction | 44px以上のリンク領域、本文への移動、フォーカス枠、強制カラー対応 / Target sizes, skip link, focus and forced colors | ソース確認 / Source review |
| 8 言葉 / Voice | 用途を先に示し、認定中と実機録画でないことを明記 / Use first, certification and recording limits stated | 文面確認 / Copy review |
| 9・12 署名と版 / Signature and version | 日英署名、資料の版、著作権・ライセンス / Bilingual signatures, document versions and licenses | ヘッダーとVERSION照合 / Header/version comparison |
| 11 音 / Sound | 公式テーマを動画に使用。自動再生なし。停止・音量調整は動画プレーヤー / Official theme, no autoplay, player controls | 音声ストリームと40秒の長さを確認 / Stream and duration check |
| 13 プライバシー / Privacy | 計測・外部フォントなし。外部画像の通信説明 / No tracking or external fonts; image-network notice | ソース確認 / Source review |

## 出荷前チェック / Pre-release checks（7-7）

(a) 200%拡大、(b) 全操作のキーボード実行、(c) グレースケール、(d) 320px幅、(e) 夜の配色、(f) Windows強制カラー、(g) ナレーター、(h) 文字だけ200%拡大は、ブラウザー・Windows実機で未確認です。公開前に確認してください。バックアップ復元(i)は静的紹介ページには該当しません。

Browser and Windows checks for zoom, keyboard use, grayscale, 320px reflow, dark mode, forced colors, screen readers and text-only enlargement remain unverified. Backup/recovery does not apply to these static pages.

動画の文字が画面の安全領域に収まることと、ポスターの見た目は確認済みです。サンプルのWindows版mdEditorでの描画は未確認です。
Video text fits the safe area and the poster was visually inspected. Sample rendering in Windows mdEditor remains unverified.
