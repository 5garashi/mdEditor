<!--
ファイル名 / File: README.md
概要 / Description: 3D冒頭・主な機能紹介・マスクを使わない改訂動画 / Revised main-feature videos with rotating 3D opening and no masking rectangles
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.1.1
ライセンス / License: MIT（第三者素材を除く / excluding third-party assets）
SPDX-License-Identifier: MIT
-->
# mdEditor — 3Dから始まる、主な機能の紹介

冒頭を指定画像 `docs/images/7-3d-model.ja.png` と同じ編集欄＋3Dプレビューの実画面へ変更し、回転するモデルと字幕でmdEditorを紹介します。そのあと日英のdiagram-testを上から下へ見せ、主な機能全体を紹介します。

前作の肌色の長方形は、ファイル選択画面のPC名・共有先を隠すマスクでした。本作ではその矩形を一切描画せず、ファイル選択・保存先指定・完了通知のシーン全体をカットして、操作前後をつないでいます。元の録画は作業フォルダーへ保持しています。

English: The video now opens on the editor-and-3D-preview scene, with the cube rotating while mdEditor is introduced. The sample tour follows, then the main features. Native file-dialog scenes are cut entirely instead of covered with colored rectangles. The existing Light Steps music remains.

資料の版 / Package version: 1.1.1 · [CHANGELOG.md](CHANGELOG.md)

## 背景の映り込み対策 / Background isolation

別窓プレビュー紹介の全区間（開く途中・閉じた直後を含む）は、実録画で安定して表示された1秒を繰り返し、mdEditorの2つのウィンドウだけを切り出した映像へ差し替えました。ウィンドウ間の隙間も除去し、背後のVS Codeを含めません。日英・横縦の4本に反映しています。音声・字幕・動画の長さは維持しています。

The detached-preview segment uses a repeated stable second from the real recording, cropped to the two mdEditor windows. Background pixels and opening/closing transitions are excluded in all four videos.

制作手順は前版の収録・編集コードに続けて `fix.py` を使います。前版の録画は作業原本として保持しています。

## 動画 / Videos

| 形式 | 日本語 | English |
| --- | --- | --- |
| 横1280×720 | [日本語MP4](mdEditor-showcase-ja.mp4) | [English MP4](mdEditor-showcase-en.mp4) |
| 縦1080×1920 | [日本語縦MP4](mdEditor-showcase-vertical-ja.mp4) | [English portrait MP4](mdEditor-showcase-vertical-en.mp4) |
| 字幕 | [日本語VTT](mdEditor-showcase-ja.vtt) | [English VTT](mdEditor-showcase-en.vtt) |
| チャプター | [日本語](chapters-ja.txt) | [English](chapters-en.txt) |
| 確認用画像 | [日本語](contact-sheet-ja.jpg) | [English](contact-sheet-en.jpg) |

縦版は全体画面と、その同じ録画の拡大部分を並べます。字幕・BGM付き、ナレーションなし。3Dや画面を生成画像へ置き換えていません。

## 主な機能の扱い

[20項目の対応表](FEATURE-COVERAGE.md)に、指定された機能・動画内の開始時刻・今回の実操作と説明のみの範囲をまとめています。別ウィンドウを2台目のディスプレイへ置ける機能は紹介しますが、今回の録画は1画面内で2つのウィンドウを並べています。

安全な保存・異常終了からの自動復元・対応文字コード・読み上げ・全キー操作などは、現行のアプリ文書と設定画面に基づく紹介です。事故を起こして復元する試験、すべての形式での再出力、Narratorによる全操作試験まで新たに完了したという意味ではありません。自動入力で画像の貼り付けイベントを送り、アプリによる画像保存とMarkdown挿入を収録しています。

## 収録元と検証

ソース版は1.0.12、参照コミットは `8ab7cef`。元リポジトリを編集せず、このワークスペースへコピーして新規ビルドしました。元ソースの既存警告CS8602が1件あり、ビルドは成功しました。Store配信版の番号は今回未確認です。

アプリのバージョン、3D画像の回転変化、SVG・数式、目次、画像貼り付け、PDF出力、読み込んだ形式などを収録JSONへ記録。編集後の4本は全フレームのデコード、形式、音声を確認しました。選んだ場面の目視結果と未確認範囲は `review-validation.json` に記録しています。各MP4と同名の検証JSONには、カットした時刻と `colored_masks: false` を保存しています。

BGMは従来のLight Stepsの48秒抜粋を反復し、フェードを追加。外部の画像・動画・OpenStreetMapは、この収録では許可して読み込みます。文書の読み上げ音声とサンプル動画の音声は最終音声へ混ぜず、BGMを付けています。

## 関連資料と公開状態

[日本語Web原稿](index.html) / [English draft](en.html)、日英のSNS画像・PDF・投稿原稿も、3Dの冒頭と主な機能を伝える内容へ更新しています。現在は提示用パッケージで、新しいYouTube・TikTok投稿や公開ページの差し替えはまだ行っていません。公開済みの旧動画と混同しないよう、[プロモーション台帳](https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/PROMOTION-SYSTEM.md)で工程を分けて管理します。

制作コードはこのPCの `work/video-revision-2026-10-18/` の作業記録です。同梱アプリ・appAutoTester・以前のヘルパー・ffmpegを使い、配布フォルダーだけで単独実行できるという案内ではありません。

## 出典と未確認事項

- アプリ・サンプル：5garashi/mdEditor、MIT。
- BGM Light Steps：5garashi.com設計事務所、作曲・音源生成Codex、CC BY 4.0。48秒抜粋の反復・フェード。
- サンプル動画：Big Buck Bunny © 2008 Blender Foundation / www.bigbuckbunny.org、CC BY 3.0。[公式About](https://peach.blender.org/about/)。短い画面収録、音なし。
- 地図：© OpenStreetMap contributors。[著作権案内](https://www.openstreetmap.org/copyright)。画面の帰属表示を保持。
- ロゴ：公式outlined SVG、CC BY-ND 4.0。新規原稿・制作コードのMITは第三者素材の権利を上書きしません。

デザイン参照：[Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/)。製品機能を説明する収録のため、STL・DOT・GeoJSONなどはmdEditorでの実際の表示に従います。GitHubでも同じ直接描画になるとは案内しません（8-9 (8)）。新PDFはタグなし。SNS圧縮後、全OSコントラストテーマ、全読み上げ環境は未確認です。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-18 JST
