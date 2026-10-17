<!--
ファイル名 / File: FEATURE-COVERAGE.md
概要 / Description: 3D冒頭・主な機能紹介・マスクを使わない改訂動画 / Revised main-feature videos with rotating 3D opening and no masking rectangles
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.1.1
ライセンス / License: MIT（第三者素材を除く / excluding third-party assets）
SPDX-License-Identifier: MIT
-->
# 主な20機能と収録範囲 / Feature coverage

ユーザー指定と現行store/STORE-LISTING.mdの20項目に対応します。開始時刻はダイアログ区間をカットした後の動画です。機能紹介と今回の試験範囲を区別します。

| 機能 | 日本語開始 | English start | 今回の範囲 |
| --- | --- | --- | --- |
| 書きながら横で仕上がりを確認。スクロールも連動<br/>Live preview beside your text, scrolling together | 01:40 | 01:42 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| プレビューを 2 台目のディスプレイに。編集中の行を表示<br/>Preview on a second display, following the line you edit | 01:44 | 01:46 | 別ウィンドウを同一画面内に並べる。実2台目への移動は未試験。背景除去のため、安定した1秒の実録画を繰り返して紹介 |
| ビューアーとしても。プレビューをウィンドウいっぱいに<br/>Also a viewer: the preview fills the window | 01:50 | 01:53 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| Mermaid・Graphviz の図と数式をオフラインで表示<br/>Mermaid and Graphviz diagrams and math, offline | 01:55 | 01:57 | 同梱エンジンで描画。OS全体のネット切断試験ではない |
| 3D モデル（STL）をマウスで回して確認<br/>3D models (STL) you turn with the mouse | 01:21 | 01:23 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| 地図（GeoJSON）。許可すると OpenStreetMap の地図も<br/>Maps (GeoJSON), with OpenStreetMap when you allow it | 01:25 | 01:27 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| 約 40 の言語のコードを色分け。PDF・Word にも色が残る<br/>Colored code for about 40 languages, kept in PDF and Word | 02:03 | 02:06 | 画面の色分けを収録。Wordの色維持の再出力試験は今回なし |
| 見出しから目次を作り、ボタン 1 つで新しくする<br/>Table of contents from your headings, updated with one press | 02:10 | 02:13 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| GitHub と同じ注意書き・フロントマター・折りたたみ<br/>GitHub-style alerts, front matter, and collapsible sections | 02:14 | 02:17 | 注意書き・折りたたみを操作。フロントマターはサンプル冒頭に表示 |
| 文書の中の動画をプレビューで再生<br/>Videos play right in the preview | 01:30 | 01:32 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| プレビューと同じ見た目の PDF をワンクリックで<br/>One-click PDF that looks exactly like the preview | 02:18 | 02:20 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| Word・HTML・メール・CSV・JSON・Jupyter Notebook・画像を開ける<br/>Opens Word, HTML, e-mail, CSV, JSON, Jupyter Notebooks, and images | 02:19 | 02:22 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| HTML・PNG・Word・CSV・JSON・Jupyter Notebook・テキストに保存<br/>Saves as HTML, PNG, Word, CSV, JSON, Jupyter Notebook, or text | 02:30 | 02:33 | 対応する保存形式を字幕で案内。全形式の再出力は未試験 |
| 画像はドロップや貼り付けで、そのまま文書に<br/>Drop or paste pictures straight into your document | 02:33 | 02:36 | アプリの貼り付けハンドラーへ画像入力し、保存・挿入を確認 |
| 安全な保存と、異常終了からの自動復元<br/>Safe saving and automatic crash recovery | 02:42 | 02:45 | 設定画面と機能説明。異常終了の再現試験は実施なし |
| Shift_JIS・EUC-JP・UTF-8 のファイルを正しく開ける<br/>Opens Shift_JIS, EUC-JP, and UTF-8 files correctly | 02:37 | 02:40 | Shift_JISのファイルと文字コード選択画面を表示。全3形式の再試験ではない |
| ネット上の画像・動画・地図は、許可したときだけ読み込む<br/>Internet images, videos, and maps load only when you allow them | 02:45 | 02:48 | 該当の実画面・操作を収録。検証JSONの範囲を参照 |
| 広告なし、計測なし、アカウント不要<br/>No ads, no tracking, no account | 02:48 | 02:51 | 現行PRIVACYと製品文書に基づく紹介。全通信の監査ではない |
| キーボードで全操作。ショートカットも変更できる<br/>Everything from the keyboard, with custom shortcuts | 02:52 | 02:55 | キー操作と変更画面を紹介。全操作の網羅試験ではない |
| 画面読み上げ・文書の読み上げ・ダークモードに対応<br/>Screen reader support, read-aloud, and dark mode | 02:56 | 02:59 | 設定とダーク表示を収録。Narrator等の網羅試験は未実施 |

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST
