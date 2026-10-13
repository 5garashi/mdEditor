<!--
ファイル名 / File: DEMO-UPDATE-1.1.0.md
概要 / Description: 日英操作デモ更新の制作と公開記録 / Bilingual demo update record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
ライセンス / License: MIT（本記録のみ / this record only）
SPDX-License-Identifier: MIT
-->
# 操作デモ 1.1.0 / Demo update

ユーザーの依頼に基づき、日英の横画面デモを更新します。既存の「開く・編集・数式・図・保存・PDF」に、Syntax、別ウィンドウ、ダークモード、Light StepsのBGMを追加しています。

| 時刻 | 実操作 |
| --- | --- |
| 0–25秒 | 文書を開き、見出しとMermaidラベルを編集し、Markdownを保存 |
| 25–33秒 | Syntax（日本語UIでは「書き方」）を開き、書式と表示例の一覧を見る |
| 35–46秒 | プレビューを実際の別ウィンドウで開き、編集ウィンドウと並べる |
| 46–53秒 | テーマボタンでダークモードへ。別プレビューも同時に切り替わる |
| 53–56秒 | プレビューを本体に戻す |
| 56–68秒 | ダークモードのままPDF保存。ネイティブ保存完了通知を表示 |
| 68–75秒 | 文書に戻り、ライトモードで案内 |

録画は未改変のWindowsソースビルド1.0.5、専用WebView2プロファイルとサンプルのコピーを使っています。Storeからインストールした版の録画ではありません。Theme・PreviewDetached・PreviewWindow・PreviewZoomの元の設定を終了時に戻します。最初の英語テイクはダークモードの検証条件がアプリの実装に合っておらず停止したためwork/demo-v2に保持。正しいWebView2の配色判定で再収録したものはwork/demo-v2-take2です。

BGMはLight Steps v1の20小節48秒を繰り返し、75秒の映像に合わせてフェードしています。曲のテンポは125 BPM、5/4。WAV原本は変更しません。音源の公開ライセンスについて担当Identity AIへ送る回答案は作成しましたが、正式な採用結果・許諾の反映は本工程で推定しません。音楽の制作経緯と元ファイルはintegration/tiktok/musicにあります。

制作手順: `python promotion/tools/windows_capture_v2.py` → `python promotion/tools/media_v2.py` → `python promotion/tools/build.py`。旧動画はGit履歴と旧録画で保持します。再収録時にはWORKを新しいフォルダーに変え、元の収録を上書きしないでください。

録画とPDFの検証結果はvalidation/media-v2-results.jsonに記録しました。日英のPDF中に編集された見出し・図が含まれ、白い紙面であることを確認しました。Chromeで日英動画の75秒再生、音量、ミュート、音量ゼロからの復帰、Esc一時停止、シーク、昼夜・幅320/1280px・文字100/200%の16条件を検証しました。結果はvalidation/player-v2-results.json。YouTube自動処理の16テストも通過しています。今回の更新で実際のWindows強制カラー・ナレーターを再検証したとは記載しません。

サンプル内で明示したMermaid色はダーク表示でも優先されるため、図の色すべてが夜の配色に切り替わるとは案内しません。従来確認したPDF末尾の空白ページの制約も、アプリ本体を変更して解消する工程には含めません。

YouTubeは動画の差し替えを提供しないため、revision 1.1.0の新規動画を作り、前版は保持して説明文に最新版リンクを追加する仕組みを使います。GitHub Pagesと単体確認用ページのMP4・VTTは更新します。既に投稿したTikTokの縦動画はこの日英横動画更新では変更しません。

## 公開結果 / Publication results

- 日本語: https://www.youtube.com/watch?v=0cZfUyZr01M
- English: https://www.youtube.com/watch?v=KPgQHi1yYlA
- 一般公開と日英字幕の配信状態servingをYouTube APIで確認。ログイン不要の公式oEmbedも両方HTTP 200。
- 元のMP4は各75秒。YouTube APIの再生時間表示は双方PT1M16Sで、配信側の表示と原本の秒数を区別して記録。
- 旧動画pxWoPoUcOLI / nuddUJCy42kは一般公開のまま保持し、説明文末尾に各言語の新版リンクを追加済み。
- 公開処理: https://github.com/5garashi/mdEditor/actions/runs/38436292206 （成功）。非公開同期を先に行い、字幕の管理情報を確定した後に公開。
- 紹介ページ: https://5garashi.github.io/mdEditor/ と https://5garashi.github.io/mdEditor/en.html 。公開MP4が検証済み原本とSHA-256一致することを確認。
- 最新版入口youtube.html / youtube-en.htmlも新版IDへ更新。Pagesの配布元コミットはee544f2b899d31e59723cc2a44ea385bca4fb123。

今後も新しいrevisionでは、release.jsonのoperationをsyncにしてGitHubへ反映 → 同期成功を確認 → operationをpublishにして反映、の順に進めてください。未同期版をpublishだけで公開する処理は意図的に停止します。PROMOTION_REPO_TOKENは未設定なので、今回のPages更新はユーザーの未コミット変更を含めないクリーンなworktreeから既存publish.pyで実施しました。

## 旧版の削除 / Retirement of previous videos

2026-10-14、ユーザーから「差し替えできない場合は古い動画を削除」と明示的な依頼を受け、旧英語pxWoPoUcOLIと旧日本語nuddUJCy42k（revision 1.0.0、各40秒）をYouTubeから完全削除しました。同じURLを保った動画差し替えはできないことを[YouTube公式ヘルプ](https://support.google.com/youtube/answer/55770?co=GENIE.Platform%3DDesktop&hl=ja)で確認しています。

削除前にチャンネル・版の識別・再生時間を照合し、元のMP4をGitコミット361c5bfから取り出して、YouTube連携に保存したSHA-256と一致することを確認しました。動画と削除前のメタデータはwork/youtube-retired-1.0.0へバックアップ済み。旧版のAPI検索結果が空、更新版2本がpublicのままであることを確認しました。削除直後のAPI反映には短い待ち時間があり、再取得して完了を確認しています。

promotion/youtube-stateでは旧版レコードを削除済みとして保持し、新版のprevious_id参照を除去しました。これにより新版を再公開しても削除済みの旧動画へ説明文を書き込もうとしません。新しい動画・Pagesの最新版入口は変更していません。検証記録はvalidation/youtube-old-deletion.jsonです。前節およびyoutube-v2-results.jsonの旧動画publicは、削除前の公開確認時点の履歴です。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-14 JST
