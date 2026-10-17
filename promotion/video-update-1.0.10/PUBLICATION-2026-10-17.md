<!--
ファイル名 / File: PUBLICATION-2026-10-17.md
概要 / Description: 1.0.10動画の公開記録 / Source 1.0.10 video publication
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-17
版 / Version: 2.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# 1.0.10動画の公開 / Video publication

2026-10-17、所有者の依頼に基づき日英の動画をYouTubeとTikTokへ公開し、日英のGitHub Pagesを更新しました。動画revisionは2.0.0、紹介資料は1.5.0です。

| 言語 / Language | YouTube | TikTok | 紹介ページ / Guide |
| --- | --- | --- | --- |
| 日本語 / Japanese | [動画](https://www.youtube.com/watch?v=pXQqzLLf8FM) | [縦動画](https://www.tiktok.com/@5garashi_com/video/7697326216935640372) | [日本語](https://5garashi.github.io/mdEditor/) |
| English | [Video](https://www.youtube.com/watch?v=FxBILEFQMQk) | [Vertical video](https://www.tiktok.com/@5garashi_com/video/7697324847185136912) | [English](https://5garashi.github.io/mdEditor/en.html) |

YouTubeはsync実行38942417181とpublish実行38942598944が成功し、日英とも一般公開を確認しました。TikTokは両方の審査完了後、管理画面で「誰でも」を確認しました。自社ブランドの宣伝開示とAI生成ラベルを有効にしています。

Pagesは自動公開用トークンが未設定だったため、既存のGitHub認証でpublish.py --pushを実行しました。公開ページの日英両方で1.0.10表記と新動画参照を確認し、配信された日英MP4のSHA-256が制作ファイルと一致しました。youtube.htmlとyoutube-en.htmlも新しい動画IDを参照しています。公開元の制作コミットはdf032abです。

Both languages are public on YouTube and TikTok. The Japanese and English landing pages now reference source build 1.0.10, and the deployed MP4 files match the production files by SHA-256. TikTok includes own-brand promotion disclosure and the AI-generated label.

日英・昼夜・320/1280px・Chromeの表示倍率200%・文字サイズ200%を含む26ケースで、横はみ出し、画像、44pxの操作領域、キーボードのフォーカスを検証済み。動画の再生・停止・シーク、字幕、サンプル保存も既存のChrome検証スクリプトで確認しました。強制カラーはChromeで模擬確認。実際のWindowsコントラストテーマと読み上げソフトは未確認で、完全準拠を宣言しません。

初回の検証は前ページの読み込み完了を拾う待機条件で停止したため、遷移先URLと画像の読み込み完了を待つよう検証スクリプトを修正し、26ケースを完了しました。検証結果の日付欄も実行日に訂正しています。

旧動画・旧投稿は保持しています。YouTubeは新しい動画IDで公開し、旧動画の説明にも最新版リンクを追加しました。

Design reference: [Design Office Identity 4.15.0](https://5garashi.github.io/5garashi-identity/)。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-17 JST
