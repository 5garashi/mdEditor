<!--
ファイル名 / File: promotion/RECORDING.md
概要 / Description: mdEditor紹介資料 / mdEditor promotion material
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
版 / Version: 1.1.0
ライセンス / License: MIT
最終更新 / Last updated: 2026-10-13
SPDX-License-Identifier: MIT
-->
# Windows実演の制作記録 / Windows recording record

版 / Version: 1.1.0

日本語・英語を別々に録画しました。動画はスライドやWeb上の再現画面ではなく、変更していないmdEditor 1.0.5のWPF/WebView2ウィンドウをWindowsデスクトップから収録したものです。

| 秒 / Seconds | 実操作 / Real action |
| --- | --- |
| 0–3 | 新しい文書の画面 / New document |
| 3–9 | 「開く」のWindowsダイアログから専用コピーを選ぶ / Open the sample copy with the Windows dialog |
| 9–13 | 入力イベントで見出しを変更し、プレビュー更新を待つ / Edit heading and wait for preview |
| 13–17 | 数式をプレビュー / Preview the equation |
| 17–23 | Mermaidのラベルを変更し、再描画を確認 / Edit and render a Mermaid label |
| 23–25 | Markdownを保存 / Save Markdown |
| 25–30 | PDF保存のWindowsダイアログで保存先を選ぶ / Choose a PDF destination |
| 30–35 | アプリのPDF保存完了ダイアログ / Native saved confirmation |
| 35–40 | 編集後の実画面と字幕の案内 / Edited document and captioned call to action |

実際のイベント時刻はvalidation/recording-ja.jsonとrecording-en.jsonに記録しています。PDFには変更した見出しと図の文字が含まれることを抽出と画像表示で確認しました。PDFビューアーを開く場面は動画に含めていません。

撮影領域は1264×642。丸いウィンドウの角の外側を除き、背後の個人作業画面が写り込まないようにしています。これを1280×720のpaper色の面に配置し、画面下に24pxのMeiryo太字字幕を追加。原録画の速度は変更していません。音声は収録していません。

検証では専用のWebView2プロファイルとコピーしたサンプルのみを使いました。ユーザーが使用中のmdEditorは操作していません。テーマやOS配色の変更は行っていません。新しく起動した検証・録画用プロセスは終了しています。

再収録にはWindows、.NET 10、WebView2、ffmpeg、Pythonとpywin32・pywinauto・Pillow・PyMuPDF・websockets、ローカルappAutoTesterが必要です。`windows_capture.py --helper-root PATH --exe PATH --work PATH --record`は専用ウィンドウを前面に置くため、同時にPCで別の操作をしない時間に実行してください。既存のraw動画・デモPDFへの上書きは拒否します。原録画を保存した後、`media.py`で字幕付きMP4を生成します。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-13 JST
