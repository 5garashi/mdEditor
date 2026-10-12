"""
ファイル名 / File: promotion/tools/docs.py
概要 / Description: Write the kit documentation and honest verification record
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from build import ROOT, VERSION, DATE, header
import json
SIGNATURE=f'\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: {DATE} JST\n'

DOCS={
'README.md':'''# mdEditor 宣伝キット / Promotion kit

バージョン / Version: {version} · [CHANGELOG.md](CHANGELOG.md)

Windows版の実操作を使った日英紹介ページ、40秒の実演動画、初めて試すための日英6サンプルです。公開前のローカル成果物です。

## 開く / Open

- 公開用: [日本語](index.html)、[English](en.html)。promotionフォルダーをそのまま配置します。
- 単体確認用: [日本語](review/index.html)、[English](review/en.html)。各ファイルにCSS、操作スクリプト、公式SVG、実画面、MP4、字幕、サンプル、ライセンス案内を埋め込みました。ファイルを別のフォルダーに移しても表示・動画再生・サンプル保存ができます。言語ごとに独立したファイルで、言語切替リンクはありません。
- 実演動画: [日本語MP4](assets/mdEditor-demo-ja.mp4)、[English MP4](assets/mdEditor-demo-en.mp4)。各40秒、1280×720、20fps、H.264。無音、字幕を画面下部に焼き込み。VTTも同梱します。
- 主サンプル: [図と数式のメモ](samples/quickstart-engineering-note.md)、[Diagrams and math](samples/quickstart-engineering-note.en.md)。全6文書はsamplesフォルダーにあります。
- 検証: [IDENTITY-CHECK.md](IDENTITY-CHECK.md)。ソース作業コピーのvalidationに画面・PDF・JSONを保存。開発用のtoolsとvalidationは公開用ZIPに含めていません。

Single-file review copies contain all local dependencies. Public pages use the files in this folder. Videos are real Windows recordings with burned-in captions, no music or narration. All six samples were opened in the actual app and exported as PDFs.

## 版と保存状態 / Version and preservation

このフォルダーを独立した宣伝資料の単位として、promotion/VERSIONで管理します。新版サンプルはpromotion/samplesに置き、アプリのVERSIONとは区別しています。アプリ本体、ルートREADME・VERSION・ビルド設定、既存diagram-testと旧quickstart文書は変更していません。

旧PRの状態はaudit/baseline-fb118bfと、作業フォルダーのlaunch-kit-baseline.bundleに保持しています。旧スライド動画mdEditor-intro-en.mp4と旧video-poster.pngは元ファイルを残していますが、新ページと配布ZIPは参照・収録しません。物理的削除は行っていません。

## 検証と制約 / Verification and limits

日英、昼夜、320px、Chromeの実際の表示倍率200%、ブラウザーの既定文字サイズ200%を確認。文字サイズ200%は1280pxと320pxで確認しました。横スクロール、画像欠落、44px未満のページ操作部品を検出しませんでした。強制カラーとグレースケールはChromeで模擬確認。キーボードの移動、本文への移動、動画の操作、手順の開閉を検証しています。

実機版はmain 6440a2eのソースを変更せずビルドしたmdEditor 1.0.5です。Storeから導入した版ではありません。Storeの現在の提供版・認定状況・地域別価格は未確認で、紹介ページでも断定しません。ビルド成功、警告NU1900はネットワーク制限によりNuGet脆弱性情報を取得できなかったものです。

短い技術メモと回答整理サンプルのPDFでは、末尾に空白ページが付きました。本文・数式・図・表の出力は確認しています。文書レビューのPDFは2ページに内容があります。空白ページの原因は未確定で、宣伝資料のためにアプリ側を改変していません。

ナレーターの実際の読み上げ、OSの実際のコントラストテーマ、正式公開先でのHTTP・ダウンロードは未検証です。アクセシビリティツリーで操作部品の名前と役割を確認したことと、スクリーンリーダー検証は区別しています。WCAG 2.2 AAの完全準拠を宣言しません。

## 配置・制作 / Placement and production

公開用ZIPの中身はpromotionフォルダー単位です。index.html、en.html、style.css、player.js、assets、samples、LICENSE、THIRD-PARTY-NOTICES.md、PRIVACY.md、CHANGELOG.md、README.mdを一緒に配置します。アプリのwwwrootや実行ファイルは同梱しません。サンプルの相対リンクはsamples/配下に解決されます。

ローカルHTTP確認例: `python promotion/tools/serve.py --port 8765`。`http://127.0.0.1:8765/`で確認後、サーバーを停止してください。MP4の再生位置変更にはHTTP byte ranges（206 Partial Content）が必要です。簡易サーバーによっては再生位置を変えられないため、同梱の確認サーバーはRange応答に対応しています。公開後には日英リンク、画像、動画、VTT、Markdown保存の内容・名前、Range応答をその公開URLで再検証してください。

再生成（Gitのソース作業コピー向け）: `python promotion/tools/samples.py`、`python promotion/tools/docs.py`、`python promotion/tools/build.py`。素材がない状態でbuild.pyを実行しても完成した単体ファイルにはなりません。録画手順は[RECORDING.md](RECORDING.md)。配布ZIPは`python promotion/tools/package.py`で作成します。

公開・mainへのマージ・PR更新・ブランチや旧成果物の削除は未実施です。

## 出典 / Credits

デザイン基準: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)、5garashi.com設計事務所 / 5garashi.com Design Office。2026-10-13にも現行版を確認。原本文書・公式SVGはCC BY-ND 4.0、第9条のコード断片はMIT。公式SVG5点はGit blobと原本のバイト一致を確認しました。ロゴの変形・再着色はしていません。
''',
'CHANGELOG.md':'''# 変更履歴 / Changelog

## {version} — 2026-10-13

- 実画面を中心に日英紹介ページを再構成。ロゴ切替、ヘッダーの罫線・マーク、署名を修正。
- 実際のWindows操作を各40秒で収録。日英の焼き込み字幕とVTT、無音の実演に差替え。
- 6サンプルを修正し、独立したキット内へ配置。実機表示とPDF保存を検証。
- 全素材を埋め込んだ単体確認用を追加。公開用と確認用のZIPを生成。
- 狭幅、拡大、文字サイズ、強制カラー、キーボード、配布動作の確認記録を追加。

旧案はPR #9のhead fb118bfに保持。アプリの版と宣伝キットの版を分離したため、このフォルダーは本版から独立管理します。
''',
'PRIVACY.md':'''# 紹介ページのプライバシー / Promotion page privacy

版 / Version: {version}

この紹介ページにテレメトリ、広告、外部フォント、アクセス解析、Cookie、localStorage、フォームはありません。文書をアップロードする機能もありません。

公開用を開くと、配置先のサーバーからHTML、CSS、JavaScript、画像、動画のメタデータ、字幕を読み込みます。動画を再生するとMP4を読み込みます。サンプルやMP4を保存すると、使う人が選んだ保存先にファイルを書き込みます。配信サーバーは通常のHTTP通信に伴うIPアドレスなどを受け取ります。公開先は未決定で、サーバーのログ方針は公開時に確認してください。

単体確認用はCSS・スクリプト・素材・動画・字幕・サンプルをHTML内に持ち、表示・再生のために外部サーバーへ接続する必要はありません。保存操作はブラウザーのダウンロード機能を使います。

Store、事務所サイト、GitHub、Identityへのリンクを選ぶと、それぞれのサービスに接続します。その後の通信は各サービスのプライバシー方針に従います。アプリ自体の保存データ・WebView2・外部画像の扱いは[mdEditorのポリシー](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md)を参照してください。

The page contains no analytics, advertising, external fonts, cookies, local storage, forms or document uploads. Public pages fetch their local assets from the host. Portable review files embed their dependencies. Downloaded files are saved to the location chosen by the reader. External links connect to the selected service; the host's logging policy remains to be checked before publication.
''',
'THIRD-PARTY-NOTICES.md':'''# 素材・権利表記 / Asset and license notices

版 / Version: {version}

## 公式ロゴ・マーク / Official logos and mark

Copyright © 2026 5garashi.com設計事務所 / 5garashi.com Design Office。

出典: [5garashi-identity assets](https://github.com/5garashi/5garashi-identity/tree/main/assets)、[Identity](https://5garashi.github.io/5garashi-identity/)。公式アウトライン版の昼夜ロゴ4点とシンプル版マーク1点を改変せず使用。各SVGにある原著作権・ライセンス表記を維持しています。

License: Creative Commons Attribution-NoDerivatives 4.0 International (CC BY-ND 4.0)。[同梱本文](licenses/CC-BY-ND-4.0.txt)、[公式条文](https://creativecommons.org/licenses/by-nd/4.0/legalcode)。

## アプリ画面・動画・文書 / App captures, video and documents

mdEditorのソースライセンスはMIT、Copyright (c) 2026 5garashi.com設計事務所 / 5garashi.com Design Office。本キットのコード・文書・字幕もMIT。[LICENSE](LICENSE)に本文を同梱します。画面内の公式ロゴ・マークには上記CC BY-ND 4.0が適用されます。

日英の実演はmdEditorソース版1.0.5のWindows操作を録画し、字幕と余白を追加しています。公式テーマ音源・ナレーション・その他の音楽は使用していません。ロゴを再着色・変形していません。

字幕のMeiryo、ページと画面のYu Gothic・Segoe UI等はWindowsなどにインストールされた書体を使用。フォントファイルは同梱していません。アプリ画面にMermaid・KaTeX・CodeMirrorの描画は含まれますが、それらのソフトウェア自体はこのキットに同梱しません。アプリの部品の名前・版・ライセンスは[mdEditor THIRD-PARTY-NOTICES.md](https://github.com/5garashi/mdEditor/blob/main/THIRD-PARTY-NOTICES.md)を参照してください。

制作・検証ではffmpeg、Chrome、Python、PyMuPDF、Pillow、pywinauto、pywin32、websockets、ローカルappAutoTesterを利用しました。これらの実行ファイルやライブラリは配布ZIPに含めません。検証スクリプトは開発用です。
''',
'RECORDING.md':'''# Windows実演の制作記録 / Windows recording record

版 / Version: {version}

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
'''}

def lum(h):
    rgb=[int(h[i:i+2],16)/255 for i in (1,3,5)]
    a=[x/12.92 if x<=.04045 else ((x+.055)/1.055)**2.4 for x in rgb]
    return sum(v*w for v,w in zip(a,[.2126,.7152,.0722]))
palette={'day':{'paper':'#F9EFCB','card':'#FFFDF2','head':'#FFE699','ink':'#385723','steel':'#7B4A21','deep':'#8C1B0D','volt':'#B92613'},'night':{'paper':'#1A2212','card':'#222C1A','head':'#2B3A20','ink':'#C9C1A5','steel':'#D9B98C','deep':'#F7A58E','volt':'#F07B5F'}}
rows=[];ratios=[]
for mode,colors in palette.items():
    for fg,bg in [('ink','paper'),('ink','card'),('ink','head'),('steel','paper'),('steel','card'),('deep','paper'),('deep','card'),('volt','paper'),('volt','card')]:
        l1,l2=sorted([lum(colors[fg]),lum(colors[bg])]);ratio=(l2+.05)/(l1+.05)
        rows.append(f'| {mode} | {fg} / {bg} | {ratio:.2f}:1 |')
        ratios.append({'mode':mode,'foreground':colors[fg],'background':colors[bg],'ratio':round(ratio,4)})
DOCS['IDENTITY-CHECK.md']='''# Identity適用・検証記録 / Identity and verification record

版 / Version: {version}。確認日: 2026-10-12〜13 JST。

基準: [Design Office Identity v4.12.0](https://5garashi.github.io/5garashi-identity/)。原本文書はデザイン資料として参照。第3・4・6〜9・11〜13条に照合しました。

| 条項 / Article | 実装・確認 / Implementation and evidence |
| --- | --- |
| 3 ロゴ | 公式5SVGのGit blob原本一致。pictureで昼夜切替。ロゴ高さ54px、四方28pxを基準に保護余白。右マーク36px、狭幅28px。変形・再着色なし。実画面写真中のロゴは撮影対象アプリの表示。 |
| 4 配色 | 昼・夜パレット。本文ink/paper、カードink/card、注記steel。ヘッダーheadとink 3px罫。印刷は昼。文書の図はcard/inkの固定配色。 |
| 6 書体 | 和文ゴシック、英語はSegoe UI先頭。見出し800、本文600、行間1.7、本文16px、注記14px。文書面はアプリ側の400。動画字幕Meiryo太字24px。 |
| 7 操作 | 自作動画操作を含め44px以上。スキップリンク、3pxフォーカス、キーボード。自動再生・自動音声・自動消去通知・繰り返しアニメーションなし。reduced-motionでanimation/transition停止。 |
| 8 文言 | 実際の用途と操作を先に説明。Store提供版・認定を断定しない。AI生成・自動計算機能がない旨を明示。再生エラーは次の保存操作を案内。 |
| 9 署名 | ファイルヘッダーと文書末尾署名。原本・第三者ファイルは改変せず例外扱い。 |
| 11 音 | 新動画は無音。音源の編集・権利判断が必要な公式BGMを新動画に使用しない。 |
| 12 版・権利 | キット独立VERSION、CHANGELOG、LICENSE、THIRD-PARTY-NOTICES、CC条文。アプリVERSION不変。旧案は保持し配布対象外。 |
| 13 Privacy | ページの保存・通信をPRIVACY.mdに説明。外部フォント・計測・送信機能なし。 |

## ブラウザー / Browser

実際のGoogle Chrome 155.0.8059.39（headless）を起動し、ローカルHTTPで日英×昼夜を検証。通常1280px・320px、実際のブラウザー表示倍率200%（1280pxウィンドウでinnerWidth=640）、ブラウザーの既定文字サイズ200%（body 32px、1280px・320px）で、全20条件の横スクロール・画像・操作領域を確認。

TabとEnter・Spaceで本文への移動、リンク・動画操作部品への移動、手順の開閉を確認。動画の再生・停止・シークは専用の44px以上の操作部品を用意。フォーカス枠は3px。強制カラーとグレースケールはブラウザー模擬で確認。単体確認用は別フォルダーに移し、CSS・画像・動画の埋め込み動作を確認。

検証方法と結果はtools/browser_check.py、validation/browser-results.json、accessible-names.json、画面PNGに保存。実際のChromeで日英6文書をダウンロードし、名前とバイト一致を確認。配布ZIP展開後の日英各20参照先はHTTP 200、MP4のRange要求は206を確認。単体確認用でもキー入力で動画の再生・停止・シークができ、埋め込んだ6文書は原本と一致。結果はvalidation/release-results.json。

200%の設定形式は[Chromium ChromeZoomLevelPrefs](https://github.com/chromium/chromium/blob/main/chrome/browser/ui/zoom/chrome_zoom_level_prefs.cc)に照合。ブラウザープロファイルは作業用で、ユーザーの通常の設定は変更していません。

## コントラスト / Contrast

2026-10-13、WCAGのsRGB相対輝度 `(L明+0.05)/(L暗+0.05)` で計算。本文ink/paperとink/cardは昼夜とも7:1以上。注記・数値は4.5:1以上。操作境界のink、フォーカスのvoltは必要な非テキスト比3:1以上。lineは装飾に限定。

| モード / Mode | 組 / Pair | 比 / Ratio |
| --- | --- | --- |
'''+ '\n'.join(rows)+'''

## Windows実機 / Actual Windows app

変更していないmain 6440a2eのmdEditorソース版1.0.5をビルド。日英6サンプルを開き、Mermaid SVG、KaTeX、表、チェックリストの存在を確認。6件のPDF保存を実行し、保存完了ダイアログ、ページ内容、白い余白を確認。結果とPDF・ページ画像はvalidationに保管。

短い技術メモと回答整理のPDFは本文の後に空白ページが1ページあります。文書レビューは2ページに本文があります。これは未改変のアプリでの確認結果で、宣伝キットではアプリの印刷CSS等を修正していません。

## 未確認 / Not verified

- ナレーターやNVDAの実際の読み上げ。アクセシビリティツリーの確認と区別。
- OSでコントラストテーマを選択した実表示。ブラウザー強制カラーの模擬確認とは区別。
- Microsoft Storeから導入した版の動作、現在の認定状況・地域別の購入画面の価格。
- 正式公開先での日英リンク・HTTP応答・ファイル名・内容。公開前のキットなので未実施。
- WCAG 2.2 AAおよびWCAG2ICTの全面的な適合評価。適用と確認範囲を記録し、完全準拠とは表記しない。

静的紹介ページには文書編集・自動バックアップ・復元機能がないため、異常終了時の復元試験は対象外です。
'''

for name,body in DOCS.items():
    (ROOT/name).write_text(header('promotion/'+name)+body.replace('{version}',VERSION)+SIGNATURE,encoding='utf-8',newline='\n')
(ROOT/'validation/contrast-results.json').write_text(json.dumps(ratios,indent=2),encoding='utf-8')
print('Generated kit documentation')
