"""
ファイル名 / File: document.py
概要 / Description: 動画の制作記録 / Document mdEditor update videos
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import shutil,json,zipfile
BASE=Path(__file__).resolve().parent
OUT=BASE.parents[1]/'deliverables/mdEditor-video-1.0.10'
OUT.mkdir(exist_ok=True)
OUT.joinpath('VERSION').write_text('2.0.0\n',encoding='utf-8')
body='''<!--
ファイル名 / File: README.md
概要 / Description: mdEditor最新版のプロモーション動画 / Updated mdEditor promotion videos
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 2.0.0
ライセンス / License: 下記の素材別表記 / See component licenses below
-->
# mdEditor 新しい操作デモ / Updated operation demos

動画版 / Video revision: 2.0.0 · [CHANGELOG.md](CHANGELOG.md)

mdEditorソース版1.0.10を実際に操作した、日英のプロモーション動画です。Microsoft Storeからインストールしたバイナリの録画ではありません。Storeの配信版番号は今回確認していません。SNSへの投稿はこの制作には含めていません。

Real Windows recordings of unmodified mdEditor source build 1.0.10. These are not recordings of the Microsoft Store binary. The current Store version is unverified. The new videos have not been posted to social media.

## 再生する / Play

| ファイル / File | 内容 / Use |
| --- | --- |
| mdEditor-update-ja.mp4 | 日本語・横長1280×720 / Japanese landscape |
| mdEditor-update-en.mp4 | 英語・横長1280×720 / English landscape |
| mdEditor-update-vertical-ja.mp4 | 日本語・縦長1080×1920 / Japanese portrait |
| mdEditor-update-vertical-en.mp4 | 英語・縦長1080×1920 / English portrait |
| mdEditor-update-ja.vtt / -en.vtt | 焼き込み字幕と同じ内容の時間付き字幕 / Timed captions |
| contact-sheet-ja.jpg / -en.jpg | 場面ごとの確認画像 / Review contact sheets |

全動画はH.264、20fps、ステレオAAC。ナレーションはなく、操作説明は映像と字幕で示します。音楽を消しても内容を確認できます。再生・停止・音量はお使いのプレーヤーで操作してください。

## 内容 / Scenes

1. Word文書を開き、Markdownに変換する。追記してMarkdownで保存。
2. CSVを開き、編集できる表として表示する。
3. 「URLを開く」で公開紹介ページをMarkdownに変換する。
4. 図と数式を含む技術メモを開き、見出しを編集する。
5. 「名前を付けて保存」の形式一覧を表示する。
6. Word・PDF・HTML・PNGを書き出し、保存完了を表示する。

Open Word and CSV, import a public web page, edit a note with diagrams and math, inspect the Save As formats, and export Word, PDF, HTML and PNG. The export scenes use the existing keyboard shortcuts after showing the format list.

取り込みは元の色・配置をそのまま復元する機能ではありません。Wordへの書き出しでは図と数式が画像になります。数値の自動計算はしません。実演用のWord・CSV・Markdownは新規のサンプルで、個人の文書は使用していません。

## 収録と編集 / Capture and editing

依頼: 5garashi.com設計事務所 / 5garashi.com Design Office。録画手順・字幕・編集: OpenAI Codex。新しい制作記録には担当を明記し、AIモデルの細かな識別名は未確認として記載しません。

撮影は1264×642の実画面。横長版は紙色の字幕帯を加えたもので、操作速度は変更していません。縦長版は同じ録画の全体表示と、同時刻の画面右側を拡大した表示を並べています。文書の表示中はプレビュー、ダイアログ中はその一部が拡大されます。保存先ダイアログの左側にあるPC名・共有先のナビゲーション列は、プライバシーのため無地で伏せています。対象領域と時間はprivacy-mask-*.jsonに記録。アプリ本文と操作結果の表示内容・配色は変更していません。拡大部は別の操作や別ウィンドウではありません。

専用WebView2プロファイルを使用。起動時に使用するTheme・Zoom等のレジストリ値は控えを取り、収録用プロセスの終了後に復元します。開いている利用者のmdEditorは操作しません。アプリは手元のソースを作業用フォルダーにコピーし、改変せずビルドしました。コピー元とのハッシュ一致はsource-verification.jsonを参照してください。ビルドは成功し、元のソースに由来するCS8602警告が1件あります。

再制作: 作業フォルダー `work/video-update-2026-10-16/` のrecord.py、encode.py、document.pyを使用します。Windows、.NET 10、WebView2、ffmpeg、Python、pywin32、pywinauto、Pillow、PyMuPDFと既存のC:/miFfplay/appAutoTesterが必要です。record.pyの第1引数はja/en、第2引数は未使用のテイク名です。既存テイクには上書きしません。実画面を録画するため、収録中は他のPC操作を避けます。

試し撮りはWeb応答待ちや完了ダイアログの操作で停止しました。収録スクリプトを調整し、読み込みを確認できた事務所の公開紹介ページを使用して再収録しています。失敗したテイクは作業フォルダーに保持し、配布動画には使用していません。

## 検証と制約 / Verification and limits

各validation.jsonに実際の操作時刻、書き出し結果、エンコード情報、SHA-256を記録しています。Word原本が変わっていないこと、Word出力に見出しと図・数式の画像があること、PDFに編集後の見出しがあること、HTMLにSVGとMathMLがあること、PNGの生成を確認しています。全動画をffmpegで最後までデコードし、時間と音声チャンネルも確認しています。

字幕のink/paperはWCAGの相対輝度式で約7.14:1。字幕は色だけに意味を持たせず、音楽にも操作情報を持たせません。横長字幕26px、縦長字幕38px。アプリ内の文字は実際の録画です。スマートフォンでアプリ全体の小さい文字を読む場合は横長版または原寸再生を使ってください。縦長版のプレビュー拡大は補助です。

字幕・操作画面・伏せ処理の視認性を抽出フレームで確認しました。スクリーンリーダー、実機の小画面・200%文字拡大、SNS圧縮後の字幕・画面UIとの重なり、すべてのプレーヤーでの字幕切替は未確認。WCAG 2.2 AAの完全準拠は宣言しません。録画を通してアプリ本体の全機能・互換性を保証するものではありません。

## 音楽・出典 / Music and credits

Light Steps — 5garashi.com設計事務所 / 5garashi.com Design Office（作曲・音源生成: Codex）— CC BY 4.0。

公式配信用MP3の先頭48秒を動画の長さに合わせて反復し、開始・終了にフェードを追加しています。独立した公式楽曲の新版ではありません。出典: https://5garashi.github.io/5garashi-identity/#a11 。ライセンス: https://creativecommons.org/licenses/by/4.0/ 。

デザイン基準: [Design Office Identity 4.15.0](https://5garashi.github.io/5garashi-identity/)、2026-10-16確認（第3・4・6・7・8・9・11・12・13条）。公式ロゴは原形・原色を維持し、CC BY-ND 4.0。アプリと制作コード・字幕は既存方針のMIT。音楽とロゴにMITを一括適用しません。

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-16 JST
'''
OUT.joinpath('README.md').write_text(body,encoding='utf-8')
OUT.joinpath('CHANGELOG.md').write_text('''<!--
ファイル名 / File: CHANGELOG.md
概要 / Description: 動画の変更記録 / Video revision history
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-16
版 / Version: 2.0.0
-->
# 変更記録 / Changelog

## 2.0.0 — 2026-10-16

- 最新ソース1.0.10を新規録画。Word・CSV・Web取り込みとWord・PDF・HTML・PNG書き出しを実演。
- 日英の横長・縦長MP4、VTT、確認画像、操作・出力検証を作成。
- 新しい「名前を付けて保存」の形式一覧を表示。旧録画と区別して保存。
- New bilingual real recordings of import, editing and export in source build 1.0.10.

---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office
**最終更新 / Last updated**: 2026-10-16 JST
''',encoding='utf-8')
shutil.copy2(BASE/'source-verification.json',OUT/'source-verification.json')
shutil.copy2(BASE/'source/LICENSE',OUT/'LICENSE-code.txt')
print(OUT,flush=True)
