"""File: promotion/tools/showcase_article.py
Description: Bilingual source 1.0.12 walkthrough and feature guide
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.8.0
License: MIT
SPDX-License-Identifier: MIT
"""
import json
from pathlib import Path
FEATURES=json.loads(Path(__file__).with_name('showcase-features.json').read_text(encoding='utf-8'))
def article(lang):
 if lang=='en':
  s='''# Diagrams, 3D and video in one Markdown document

Start with an STL cube you can rotate and zoom. Then explore diagrams, math, maps and video inside the same document, and the main editing and sharing features of mdEditor, a free Windows app developed by 5garashi.com Design Office.

[Watch the source 1.0.12 demonstration](https://5garashi.github.io/mdEditor/en.html#demo) · [YouTube entry](https://5garashi.github.io/mdEditor/youtube-en.html)

![Actual mdEditor: STL source beside a 3D cube with a red 5.](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.12-en.png)

## Try the same sample

Save [diagram-test.en.md](https://5garashi.github.io/mdEditor/samples/diagram-test.en.md), then choose Open in mdEditor. Scroll from the top; drag the STL canvas to rotate and use the wheel to zoom. Internet images, video and detailed OpenStreetMap tiles load only with permission. Servers receive information such as your IP address when you permit a request. The sample deliberately contains one syntax-error example.

The sample targets mdEditor and includes STL, DOT and GeoJSON. GitHub does not directly render all of these formats. Use the sample in mdEditor; use Mermaid that renders in both environments for shared Markdown documents.

## Write with the result beside you

Edit a heading and check the live preview. Synchronized scrolling keeps the result near your current line. Detach the preview to put it in another window, or expand it to read the document. A second physical display was not tested in this recording.

## Share a document

Use Save to keep the Markdown. Use Save As and choose PDF, Word, HTML, PNG or another supported format. Open the result and check diagrams, text and page breaks before sharing. Current source 1.0.12 also offers one-click PDF export. Word import does not preserve its colors or layout; notebook export omits execution output. Equations and table values are not automatically calculated.

## Main features

'''
 else:
  s='''# 図も、3Dも、動画も。ひとつのMarkdownから

まず、文書内のSTL立方体を回転・拡大。そのあと図・数式・地図・動画を一つの文書で確認し、編集と共有へ進みます。5garashi.com設計事務所が開発する無料のWindowsアプリmdEditorの使い方を紹介します。

[ソース1.0.12の実演を見る](https://5garashi.github.io/mdEditor/#demo) · [YouTubeの入口](https://5garashi.github.io/mdEditor/youtube.html)

![mdEditorの実画面。左にSTLソース、右に赤い5が付いたクリーム色の3D立方体。](https://5garashi.github.io/mdEditor/assets/hero-3d-1.0.12-ja.png)

## 動画と同じサンプルを試す

[diagram-test.md](https://5garashi.github.io/mdEditor/samples/diagram-test.md)を保存し、mdEditorの「開く」で選びます。上から下へスクロールし、STLの画面をドラッグすると回転、ホイールで拡大・縮小できます。ネット上の画像・動画・OpenStreetMapの詳細地図は、許可したときだけ読み込みます。許可した取得先にはIPアドレスなどが伝わります。構文エラーの表示を試すための例も意図的に含まれています。

このサンプルはmdEditor向けで、STL・DOT・GeoJSONを含みます。GitHubですべて同じように直接描画されるものではありません。共通のMarkdown文書には、GitHubとmdEditorの両方で描けるMermaidを使います。

## 書きながら、横で仕上がりを確認する

見出しを直してプレビューへの反映を確認します。スクロールを連動でき、プレビューを別ウィンドウへ分けたり、ウィンドウ全体へ広げて読んだりできます。今回の録画で実際の2台目ディスプレイへの移動は試験していません。

## 必要な形式で共有する

「保存」でMarkdownを残し、「名前を付けて保存」の形式からPDF・Word・HTML・PNGなどを選びます。現行ソース1.0.12はワンクリックのPDF保存にも対応しています。配布前に、出力した文書を開いて図・文字・改ページを確認してください。Wordの色や配置、Notebookの実行結果は再現しません。数式や表の値を自動計算する機能はありません。

## 主な機能

'''
 s+='\n'.join('- '+f[lang] for f in FEATURES)+'\n\n'
 s+=('''## Scope and credits

The new video uses source build 1.0.12 (8ab7cef), with captions and Light Steps BGM. Current Store build number is unverified. Import of seven formats, PDF export, diagrams, math, maps, video and rotating 3D were checked during recording. Full keyboard coverage, crash recovery, all export formats and screen-reader behavior were not exhaustively retested. The detached-preview segment repeats one stable second cropped to the actual app windows to exclude the desktop background.

[PDFs, images and samples](https://5garashi.github.io/mdEditor/resources-en.html) · [App privacy](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md)

''' if lang=='en' else '''## 確認範囲と出典

新動画はソース版1.0.12（8ab7cef）を収録し、字幕とBGM「Light Steps」を付けています。Store配信版番号は未確認です。7形式の取り込み、PDF保存、図・数式、地図・動画、3Dの回転を録画時に確認しました。全キー操作、異常終了からの復元、全形式の書き出し、読み上げ環境を網羅的に再試験したものではありません。別窓紹介は背景を映さないため、安定した実録画1秒をアプリのウィンドウだけに切り出して繰り返しています。

[PDF・画像・サンプル](https://5garashi.github.io/mdEditor/resources.html) · [アプリのプライバシー](https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md)

''')
 s+='Design reference: [Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/). App, article and notation examples: MIT. Official logos: CC BY-ND 4.0.\n'
 return s
