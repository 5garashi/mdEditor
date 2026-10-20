"""File: package.py
Description: Package local bilingual mdEditor 1.0.14 promotion videos and records
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,hashlib,shutil,zipfile
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.14-revision-1.0.0';P=ROOT/'audit-source/promotion';ARC=P/'showcase-1.0.14-revision-1.0.0'
def header(name,description):return f'''<!--
ファイル名 / File: {name}
概要 / Description: {description}
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-20
版 / Version: 1.0.0
ライセンス / License: MIT（動画・音楽等の権利は本文参照）
SPDX-License-Identifier: MIT
-->
'''
footer='''
---

**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office  
**最終更新 / Last updated**: 2026-10-20 JST  
**デザイン参照 / Design reference**: [Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/)、2026-10-20に正本の版・変更履歴・本文を確認。
'''
README='''# mdEditor 1.0.14 — 3Dから始まる日英実演 / Bilingual showcase

状態：ローカル制作済み。YouTube・TikTok・Reddit・公開サイトへのアップロードや差し替えは行っていません。

既存の構成を保ち、更新したソース1.0.14で再収録しました。3Dが回転する短い紹介、diagram-testの上から下へのツアー（途中でも3Dを拡大・回転）、主要機能の順です。画像のドラッグ＆ドロップ、50%サイズ・中央揃えを新たに実演します。指定のSTORE-LISTING.mdとアプリの1.0.13・1.0.14の変更内容を参照しました。

English: Recorded again in source build 1.0.14. A rotating-3D introduction leads into a top-to-bottom sample tour and the main features, including a file drag-and-drop followed by 50% image sizing and centered alignment. Prepared locally; not published.

| 形式 / Format | 日本語 | English |
| :--- | :--- | :--- |
| 横1280×720 | [日本語MP4](mdEditor-showcase-ja.mp4) | [English MP4](mdEditor-showcase-en.mp4) |
| 縦1080×1920 | [日本語縦MP4](mdEditor-showcase-vertical-ja.mp4) | [English portrait](mdEditor-showcase-vertical-en.mp4) |
| 字幕 | [VTT](mdEditor-showcase-ja.vtt) | [VTT](mdEditor-showcase-en.vtt) |
| チャプター | [開始時刻](chapters-ja.txt) | [Chapters](chapters-en.txt) |
| 確認画像 | [一覧](contact-sheet-ja.jpg) | [Contact sheet](contact-sheet-en.jpg) |

資料パッケージ版：1.0.0。アプリのソース版1.0.14とは別に管理します。収録はワークスペースへコピーしたソースの新規ビルドです。Store配信バイナリを撮影したものではありません。

## 見どころと確認範囲 / Coverage

- 冒頭・ツアー内の3D：実際のSTLキャンバスをドラッグ・拡大し、描画変化を確認。
- 日本語はdiagram-test.md、英語はdiagram-test.en.mdを使用。意図的な構文エラーの例も含む試験サンプルであり、その区間には説明字幕を付けます。
- 画像：CDPの実入力イベントでローカル画像をドラッグ＆ドロップ。アプリによる「文書名_files」へのコピーを確認後、画像の調整で50%幅・中央を選び、生成されたHTMLとプレビューを確認。
- 主要機能：横並び編集、連動スクロール、別窓、全画面プレビュー、図・数式、コード、目次、注意書き・折りたたみ、PDF、取り込み形式、保存形式、画像、安全な保存、文字コード、許可制の通信、プライバシー、キー操作・読み上げ・ダーク表示を紹介。
- ZIP保存などは製品文書に基づく機能紹介。全形式の書き出し・破損時の復元・スクリーンリーダーの網羅試験・実際の2台目ディスプレイへの移動は今回の実演検証には含みません。

## 背景と編集 / Recording and editing

収録範囲の背後を無地の画面で覆い、別窓の隙間・開閉時にもVS Codeなどが映らないようにしています。肌色の部分マスクは使いません。ファイル選択・保存ダイアログと通知の区間はシーン全体をカットしてつなぎます。カット前の原録画はworkフォルダーで保持します。

字幕と既存BGM付き、ナレーションなし。縦版は同じ実録画の全体と拡大部分を並べたものです。表示を合成した架空の製品画面ではありません。

## 権利と再制作 / Rights and production

- BGM：Light Steps、5garashi.com Design Office / Codex、[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)。既存音源の抜粋・繰り返し・フェード。
- サンプル内の映像：Big Buck Bunny © 2008 Blender Foundation、[CC BY 3.0](https://creativecommons.org/licenses/by/3.0/)、bigbuckbunny.org。実演区間にクレジットを表示。
- 詳細地図：© OpenStreetMap contributors。アプリの地図内に帰属表示があり、そのまま収録しています。
- 事務所の公式アセット：既存の公式SVG等を使用、CC BY-ND 4.0。製品のロゴを独自に描き直していません。
- アプリのソースと制作スクリプト：MIT。各依存ライブラリーの権利はアプリのTHIRD-PARTY-NOTICES.mdを参照。

制作作業：work/video-update-1.0.14。record.py、encode.py、package.pyを保存しています。再制作にはローカルのappAutoTester、旧encode.pyの字幕関数、既存BGM・公式アセット、ffmpeg、Windowsのフォントが必要です。このフォルダー単体での再制作は未検証です。

ビルド時、依存パッケージの脆弱性情報取得に通信制限（NU1900）、既存DocxExport.csにNULL参照警告（CS8602）がありました。ビルド自体は成功。アプリ本体の改修は行っていません。
'''
(OUT/'README.md').write_text(header('README.md','日英の1.0.14動画と制作・確認範囲 / 1.0.14 videos and verification scope')+README+footer,encoding='utf-8')
(OUT/'VERSION').write_text('1.0.0\n',encoding='utf-8')
(OUT/'CHANGELOG.md').write_text(header('CHANGELOG.md','動画資料の変更履歴 / Video package changelog')+'# 変更履歴 / Changelog\n\n## 1.0.0 — 2026-10-20\n\n- 1.0.14で日英を再収録。3D紹介・サンプルツアー・主要機能という流れを保ち、画像のドロップ・サイズ・中央揃えを追加。\n- Re-record in 1.0.14 and add image drag-and-drop, sizing and alignment.\n'+footer,encoding='utf-8')
for n in ['record.py','encode.py','package.py','shield.py','review.py']:
 shutil.copyfile(B/n,OUT/n)
manifest={'app_source_version':'1.0.14','package_version':'1.0.0','date':'2026-10-20 JST','publication':'not published','files':{}}
for f in OUT.iterdir():
 if f.is_file():manifest['files'][f.name]={'bytes':f.stat().st_size,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
(OUT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
shutil.copytree(OUT,ARC,dirs_exist_ok=True)
archive=OUT.parent/(OUT.name+'.zip')
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
 for f in OUT.iterdir():
  if f.is_file():z.write(f,OUT.name+'/'+f.name)
print(OUT);print(archive)
