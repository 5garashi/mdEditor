<!--
  ファイル名 / File: CHANGELOG.md
  概要 / Description: mdEditor の版ごとの変更の記録 / Changes in each mdEditor version
  作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
  作成日 / Created: 2026-10-12
  ライセンス / License: MIT
  SPDX-License-Identifier: MIT
-->
# 変更の記録 / Changelog

版の番号は [VERSION](./VERSION) に書きます。新しい版は一番上に足します。各節の見出しは `## 版 — 日付` の形にします（GitHub の Release の本文に使います）。

The version number lives in [VERSION](./VERSION). Add each new version at the top, with a `## version — date` heading (used as the GitHub Release notes).

## 1.0.3 — 2026-10-12

- 「このアプリについて」にバージョンを表示するようにした。
- バージョンを `VERSION` ファイルだけで管理するようにした。アプリと MSIX はそこから番号を読み取り、main にマージすると GitHub Actions がタグと Release を作る。

- About now shows the version.
- The version is kept only in the `VERSION` file. The app and the MSIX read it from there, and merging into main makes GitHub Actions create the tag and the Release.

## 1.0.2 — 2026-10-12

Microsoft Store に最初に申請した版です。 / The first version submitted to the Microsoft Store.

- 英語表示のときは、英語版のロゴタイプ（ライト・ダーク）を表示するようにした。

- In English, the header shows the English logotype (light and dark).

## 1.0.1 — 2026-10-12

- スタート・タスクバー・エクスプローラーのロゴと Store の画像を、公式マークの SVG から大きさごとに描くようにした（拡大による粗さをなくした）。

- Start, taskbar, and File Explorer logos and the Store art are drawn from the official mark SVG at each size, so nothing is enlarged.

## 1.0.0 — 2026-10-11

最初の MSIX（Store には出していない）。 / The first MSIX (not released in the Store).

- 編集とプレビューを並べた Markdown エディタ。スクロールの連動、見出しへのリンク、ほかのファイルへのリンク。
- Mermaid の図と KaTeX の数式。プレビューそのままの PDF（A4 / Letter）。
- 安全な保存、自動バックアップと異常終了からの復元、文字コードの判定（UTF-8、Shift_JIS、EUC-JP など）。
- キーボードだけで全操作、変更できるショートカット、画面読み上げソフト対応、読み上げ、読みやすさの設定、コントラスト テーマ、ダークモード。
- 日本語と英語の表示、サンプル、「このアプリについて」。

- Markdown editor with a side-by-side preview, synced scrolling, heading links, and links to other files.
- Mermaid diagrams and KaTeX math; PDF export that matches the preview (A4 or Letter).
- Safe saving, automatic backup and crash recovery, and encoding detection (UTF-8, Shift_JIS, EUC-JP, and more).
- Full keyboard operation with changeable shortcuts, screen reader support, read-aloud, readability settings, contrast themes, and dark mode.
- Japanese and English UI, samples, and About.
