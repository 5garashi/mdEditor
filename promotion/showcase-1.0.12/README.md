<!--
ファイル名 / File: README.md
概要 / Description: 見せてから説明する日英動画・資料の制作記録 / Bilingual visual-first showcase package
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-17
版 / Version: 1.0.0
ライセンス / License: MIT（第三者素材を除く / excluding third-party assets）
SPDX-License-Identifier: MIT
-->
# mdEditor 1.0.12 — 図も、3Dも、動画も。

日英の実録画と、同じコンセプトの紹介画像・PDF・Web原稿・SNS原稿です。最初にサンプルを上から下へ見せ、3Dを拡大・回転し、そのあと新機能と編集・保存の説明へつなぎます。実アプリのソース版は1.0.12、ソースコミットは62c1046。Store配信版の番号は今回未確認です。

English: Real Windows source-build 1.0.12 recordings open with a top-to-bottom sample tour, include actual 3D zoom and rotation, then explain new features and editing/saving. The accompanying cards, PDFs and landing-page drafts follow the same visual-first concept.

資料の版 / Package version: 1.0.0 · [CHANGELOG.md](CHANGELOG.md)

## 動画を見る / Watch

| 形式 | 日本語 | English |
| --- | --- | --- |
| 横1280×720 | [日本語MP4](mdEditor-showcase-ja.mp4) | [English MP4](mdEditor-showcase-en.mp4) |
| 縦1080×1920 | [日本語縦MP4](mdEditor-showcase-vertical-ja.mp4) | [English portrait MP4](mdEditor-showcase-vertical-en.mp4) |
| 字幕 | [日本語VTT](mdEditor-showcase-ja.vtt) | [English VTT](mdEditor-showcase-en.vtt) |
| 確認用一覧画像 | [日本語](contact-sheet-ja.jpg) | [English](contact-sheet-en.jpg) |

縦動画は全体画面と、同じ録画の中央部分を拡大した画面を並べています。図や3Dを生成画像へ置き換えていません。ナレーションはなく、字幕とBGMで案内します。

## 収録した構成

| 順序 | 内容 | 指定への対応 |
| --- | --- | --- |
| 冒頭 | diagram-test.md / diagram-test.en.mdを上から下へ連続表示 | 日英それぞれの原本コピーを使用。収録前後のSHA-256一致 |
| 外部読み込み | 画像・動画・詳細地図のお知らせ欄で許可 | 所有者の指定に従い、この収録で読み込みを許可 |
| 3D | 「5」の立方体をホイールで拡大し、ドラッグで回転 | 実際の入力操作とキャンバス画像の変化を確認 |
| 地図・動画 | OpenStreetMap詳細地図、ローカル動画、外部動画 | 画面と動画の再生状態を確認。動画の音はミュート |
| 新機能 | GitHub形式の注意書き・数式・HTMLとYAMLのカード | 対象節へ移動して表示を紹介 |
| 操作説明 | Wordを開いて追記、保存形式一覧、PDFへの書き出し | 実際の操作を撮影。出力PDFの存在を確認 |
| BGM | 現在のLight Stepsをベースに使用 | 48秒の抜粋を反復し、開始・終了にフェード |

サンプルには意図的な構文エラーのテスト節が含まれます。元の文書から削除せず、字幕でテスト例と説明しています。3D・DOT・地図などのmdEditor固有表示を、すべてGitHubでも同じ表示になるとは案内しません。

## 同じコンセプトの資料

- [日英Web原稿：日本語](index.html) / [English](en.html)：最初に動画を見せ、そのあと機能と制約を説明。動画は操作して再生し、停止・音量調整ができます。
- [SNS画像：日本語](social-ja.png) / [English](social-en.png)：3Dモデルの実画面を大きく使用。
- [1枚PDF：日本語](one-page-ja.pdf) / [English](one-page-en.pdf)：画像で引き付けてから、見られる内容と対象版を説明。
- [媒体別原稿：日本語](posts-ja.md) / [English](posts-en.md)：X、LinkedIn、記事の導入。公開時に確認済みの新動画URLを追加します。

## 公開状態

このパッケージは動画提示・内容確認用です。新しいYouTube・TikTokの投稿IDはまだ作成していません。既存の公開動画・紹介ページを新作へ切り替える工程と、素材の制作完了を区別します。次は[プロモーション台帳](https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/PROMOTION-SYSTEM.md)に従い、新動画公開、紹介ページ差し替え、記事・SNS案内、公開結果の記録を行います。

The package is ready for review. New social uploads and replacement of the currently published pages are separate subsequent publication steps.

## 検証と制約

実アプリのバージョン表示、元サンプルのSHA-256、3D操作後のキャンバス変化、ローカル・外部動画のreadyStateと再生時刻、PDF出力を記録しています。エンコード後は全フレームのデコードと音声・映像形式を検査。検証JSONは各MP4と同じ名前です。ファイル選択ダイアログのPC名・共有先が出るナビゲーション領域だけをマスクしています。

ソース版の収録であり、Microsoft Storeからインストールしたバイナリの検証ではありません。Wordの色や配置は取り込みません。Wordへの図・数式は画像です。数値の自動計算はありません。HTML保存時の地図など、形式ごとの制約はアプリのREADME・変更履歴を参照します。

新PDFはタグ付きPDFではありません。読み上げ、全OSのコントラストテーマ、すべての端末・SNS圧縮後の表示は未確認です。完全準拠を宣言しません。

同梱の制作コードは作業記録です。実行元はこのPCの `work/video-showcase/` で、未改変アプリ、専用プロファイル、appAutoTester、以前の収録・エンコード用ヘルパー、ffmpegを利用します。配布フォルダーだけで単独実行できるという意味ではありません。アプリ・サンプルの原本、ユーザーの保存文書や既存未コミット変更は編集していません。

## 出典・権利表記

| 対象 | 出典・扱い |
| --- | --- |
| 制作・画面操作・字幕・編集 | Codex。依頼：5garashi.com設計事務所 / 5garashi.com Design Office |
| アプリとサンプル | [5garashi/mdEditor](https://github.com/5garashi/mdEditor)、MIT。サンプル原文を改変せず収録 |
| BGM Light Steps | 5garashi.com設計事務所、作曲・音源生成Codex。CC BY 4.0。48秒の抜粋を反復・フェード |
| 外部のサンプル動画 | Big Buck Bunny © 2008 Blender Foundation / www.bigbuckbunny.org、CC BY 3.0。[公式About](https://peach.blender.org/about/)。文書内プレーヤーの短い画面収録、音なし |
| 地図 | © OpenStreetMap contributors。[著作権案内](https://www.openstreetmap.org/copyright)。アプリ画面の帰属表示を保持 |
| 公式ロゴ | 5garashi Identityの公式outlined SVG。変形・再着色なし、CC BY-ND 4.0 |
| 新規原稿と制作コード | 既存プロジェクトに従いMIT。第三者の映像・音楽・地図・ロゴの権利を上書きしない |

デザイン参照 / Design reference: [Design Office Identity 4.19.0](https://5garashi.github.io/5garashi-identity/)（今回取得した公開正本）。動画全体をMITのみで利用できるという案内ではありません。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-17 JST
