<!--
ファイル名 / File: promotion/integration/reddit/README.md
概要 / Description: Redditでの初回試行と公開記録 / Initial Reddit trial and publication record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.3.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
# Reddit — r/windowsapps

2026-10-20：既存のr/windowsapps・r/sideprojects本文へ1.0.14動画リンクを追記。添付動画は旧1.0.12を維持。r/sideprojectsのフィルター削除は継続。 [今回の公開記録](../../multichannel/update-2026-10-20/PUBLICATION.md)。


2026-10-18：[r/sideprojectsの動画投稿と確認記録](SIDEPROJECTS.md)を追加。一覧での動画表示と自動再生を確認しました。

2026-10-18、**u/5garashi** から[初回の開発者投稿](https://www.reddit.com/r/windowsapps/comments/1x8rt4k/i_built_a_free_windows_markdown_editor_with_live/)を公開しました。Developerフレア、開発元の明示、Microsoft StoreとMITソースへのリンクを付けています。アカウント登録の本人確認は所有者が実施しました。

First developer post published on 2026-10-18. The video demonstrates mdEditor 1.0.12, also available from the Microsoft Store.

## 素材 / Materials

| ファイル | 内容 |
| :--- | :--- |
| [POST.md](POST.md) | 公開した英文の原稿 |
| [実演動画](mdEditor-windowsapps-en-1.0.0.mp4) | 英語・30.65秒・1280×720・20fps・H.264/AAC |
| [実際のPDF出力](example-export.pdf) | 動画中に示した実出力 |
| [選択フレーム](contact-sheet.jpg) | 3D・図・編集・PDFの確認用一覧 |
| [検証記録](video-validation.json) | 元動画・出力ハッシュ、区間、デコード結果 |
| [試行記録](TRIAL.md) | 公開確認と反応・返信負担の記録欄 |

元動画はソース1.0.12・動画revision 1.1.1の英語版です。3D回転、図、横並び編集、数式、実際のPDFを短く紹介します。BGMは既存のLight Steps（5garashi.com Design Office / Codex、CC BY 4.0）から抜粋し、フェードを加えました。動画revisionは1.0.0で、アプリ版や資料版と区別します。

## 次回の手順 / Next post

1. [r/windowsappsの現在のルール](https://www.reddit.com/r/windowsapps/)を再確認する。2026-10-18確認時は、宣伝は週1投稿まで、Developerフレアが必須。リンクは主にMicrosoft Storeと安全な公式配布元を使う。
2. 実際のアプリの短い実演を用意し、開発者であること、用途、制約、配布元を英語で明示する。
3. u/5garashiでログインしていることを確認し、Developerフレアを付けて投稿する。本人認証は本人が行い、認証情報を記録しない。
4. 投稿URLで本文、配布リンク、フレア、動画再生、削除・審査表示の有無を確かめ、試行記録へ戻す。
5. 反応と返信に使った時間を記録する。削除された場合は連投せず、理由を確認する。別コミュニティへの横展開は別途判断する。

## 確認範囲 / Verification scope

- 全フレームのデコードは成功。選択した6フレームを目視し、背景画面の映り込みや部分マスクがないことを確認。
- 公開投稿の本文、Developerフレア、30秒動画プレーヤーを確認。再生時刻が0:11まで進むことを確認。
- ログイン中のブラウザーで確認。ログアウト状態、全機種での再生、Reddit再圧縮後の全フレームと音声は未確認。
- この文書は表と手順で記載。新しいMermaid図は追加していない。GitHubとmdEditorでのこの文書の目視確認は未実施。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office  
**最終更新 / Last updated**: 2026-10-18 JST  
**デザイン参照 / Design reference**: [Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/)、第8条・第9条・第12条。
