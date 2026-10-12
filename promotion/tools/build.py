"""
ファイル名 / File: promotion/tools/build.py
概要 / Description: Build public and self-contained bilingual promotion pages
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-12
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import base64, mimetypes, re
ROOT=Path(__file__).resolve().parents[1]
VERSION=(ROOT/'VERSION').read_text().strip()
DATE='2026-10-13'

def header(file,en=False):
    labels=['File','Description','Author','Created','Version','License'] if en else ['ファイル名 / File','概要 / Description','作成者 / Author','作成日 / Created','版 / Version','ライセンス / License']
    values=[file,'mdEditor promotion material' if en else 'mdEditor紹介資料 / mdEditor promotion material','5garashi.com Design Office' if en else '5garashi.com設計事務所 / 5garashi.com Design Office','2026-10-12',VERSION,'MIT']
    updated='Last updated' if en else '最終更新 / Last updated'
    return '<!--\n'+'\n'.join(a+': '+b for a,b in zip(labels,values))+f'\n{updated}: {DATE}\nSPDX-License-Identifier: MIT\n-->\n'

def page(en=False):
    def t(ja,eng): return eng if en else ja
    lang='en' if en else 'ja'
    suffix='.en' if en else ''
    logo='5garashi_logotype_en' if en else '5garashi_logotype'
    samples=''
    for name,title,detail in [('engineering-note',t('図と数式のメモ','Diagrams and math'),t('まずはこれから。見出しと図の文字を変え、数式・表ごとPDFへ。','Start here. Edit a heading and a diagram label, then export the equation and table too.')),('workflow',t('文書レビューの手順','Document review'),t('フローチャート、表、チェックリストで作業の流れを残す。','Keep a procedure in a flowchart, table and checklist.')),('ai-notes',t('回答を整理するメモ','Organize an answer'),t('回答の要点・出典・次の行動を整理する記入用テンプレート。','A template for key points, sources and next actions.'))]:
        samples+=f'<article class="sample-card"><h3>{title}</h3><p>{detail}</p><a class="sample-download" href="samples/quickstart-{name}{suffix}.md" download="quickstart-{name}{suffix}.md">{t("Markdownを保存","Download Markdown")}<span class="sr-only"> — {title}</span></a></article>'
    alt=t('Windows版mdEditorの実画面。左のMarkdown編集欄と、右の図・数式・表のプレビュー。','Actual Windows mdEditor: Markdown on the left, a diagram, equation and table in the preview on the right.')
    return header('promotion/'+('en.html' if en else 'index.html'),en)+f'''<!doctype html>
<html lang="{lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="color-scheme" content="light dark">
<title>mdEditor — {t('図・数式と書くMarkdown','Markdown with diagrams and math')} | 5garashi.com</title>
<meta name="description" content="{t('WindowsでMarkdownを編集し、図・数式・表をプレビュー。そのままPDFに保存。実演動画と試せるサンプル。','Edit Markdown on Windows. Preview diagrams, math and tables, then save as PDF. A real demonstration and samples.')}" >
<link rel="icon" type="image/svg+xml" href="assets/5garashi_mark_simple.svg"><link rel="stylesheet" href="style.css"><script src="player.js" defer></script></head><body>
<a class="skip" href="#main">{t('本文へ移動','Skip to content')}</a>
<header class="site-header"><div class="header-inner"><a class="brand" href="https://www.5garashi.com/" aria-label="{t('5garashi.com設計事務所のサイト','5garashi.com Design Office website')}"><picture><source media="screen and (prefers-color-scheme:dark)" srcset="assets/{logo}_reversed_outlined.svg"><img src="assets/{logo}_outlined.svg" width="140" height="54" alt="{t('5garashi.com設計事務所 / 5garashi.com Design Office','5garashi.com Design Office')}"></picture></a><nav aria-label="{t('ページ案内','Page navigation')}"><a href="#samples">{t('サンプル','Samples')}</a><a class="language-link" href="{'index.html' if en else 'en.html'}" lang="{'ja' if en else 'en'}">{'日本語' if en else 'English'}</a></nav><img class="brand-mark" src="assets/5garashi_mark_simple.svg" width="36" height="36" alt="" aria-hidden="true"></div></header>
<main id="main" tabindex="-1"><section class="hero" aria-labelledby="hero-heading"><div class="hero-copy"><p class="eyebrow">mdEditor · WINDOWS</p><h1 id="hero-heading">{t('書く。確かめる。<br>PDFに残す。','Write. Preview.<br>Save as PDF.')}</h1><p class="lead">{t('図も数式も、Markdownと一緒に。<br>編集した文書を、その場で確認できます。','Diagrams and math, alongside your Markdown.<br>See your document as you edit.')}</p><div class="actions"><a class="button primary" href="https://apps.microsoft.com/detail/9nww2qrclmhc?hl={'en-US&gl=US' if en else 'ja-JP&gl=JP'}">{t('Microsoft Storeで見る','View in Microsoft Store')}</a><a class="button" href="#samples">{t('サンプルを試す','Try a sample')}</a></div><p class="note">{t('Windows用・無料。提供版・動作要件はStoreで確認できます。','Free for Windows. Check the Store for the available version and requirements.')}</p></div>
<figure class="app-figure"><a class="image-link" href="assets/app-{lang}.png" aria-label="{t('mdEditorの実画面を大きく見る','View the full-size mdEditor screenshot')}"><img src="assets/app-{lang}.png" width="1264" height="610" alt="{alt}"></a><figcaption>{t('実際のWindows版mdEditor。左で編集、右で仕上がりを確認。','The real Windows app: edit on the left, preview on the right.')}</figcaption></figure></section>
<section class="features" aria-label="{t('できること','What you can do')}"><article><h2>{t('図と数式も、そのまま','Keep diagrams and math')}</h2><p>{t('Mermaidの図、KaTeXの数式、表を表示。プレビューの内容をPDFに保存できます。','Preview Mermaid diagrams, KaTeX equations and tables. Export the rendered document as PDF.')}</p></article><article><h2>{t('書く画面と、読む画面','Room to write and read')}</h2><p>{t('編集とプレビューが連動。プレビューを別ウィンドウに出して使うこともできます。','The preview follows your edits. You can also open it in a separate window.')}</p></article><article><h2>{t('自分に合う読みやすさ','Read your way')}</h2><p>{t('文字の大きさ・行間・書体を調整。キーボード操作と、PC内の音声による読み上げに対応。','Adjust text size, spacing and typeface. Use the keyboard or voices installed on your PC.')}</p></article></section>
<section class="demo" aria-labelledby="demo-heading"><div class="section-heading"><p class="eyebrow">{t('実際の操作','THE REAL APP')}</p><h2 id="demo-heading">{t('開いて、直して、PDFへ。','Open, edit, export.')}</h2><p>{t('Windows版を操作した実演です。字幕だけで手順が分かります。','A recording of the Windows app, with captions for every step.')}</p></div>
<div class="video-stage"><video controls playsinline preload="metadata" poster="assets/video-poster-{lang}.png" aria-label="{t('mdEditorのWindows実演動画。無音・日本語字幕。','Windows mdEditor demonstration. Silent, with English captions.')}"><source src="assets/mdEditor-demo-{lang}.mp4" type="video/mp4"><track kind="captions" src="assets/demo-{lang}.vtt" srclang="{lang}" label="{'English' if en else '日本語'}"><p>{t('動画を再生できません。下のリンクから保存してください。','Video playback is unavailable. Use the download link below.')}</p></video>
<div class="player-controls" hidden><div class="play-actions"><button type="button" data-play>{t('再生','Play')}</button><button type="button" data-stop>{t('停止して先頭へ','Stop and restart')}</button><button type="button" data-fullscreen>{t('全画面表示','Full screen')}</button><output class="player-clock" aria-live="off">00:00 / 00:40</output></div><label class="seek-label">{t('再生位置','Playback position')}<input type="range" min="0" max="40" step="0.1" value="0" aria-label="{t('動画の再生位置','Video playback position')}"></label></div></div><p class="player-error" role="alert" hidden>{t('動画を再生できませんでした。下の「動画を保存」からMP4を保存し、動画プレーヤーで開いてください。','The video could not play. Download the MP4 below and open it in a video player.')}</p>
<div class="media-links"><a href="assets/mdEditor-demo-{lang}.mp4" download>{t('動画を保存（MP4）','Download video (MP4)')}</a><a href="assets/demo-{lang}.vtt" download>{t('字幕を保存（VTT）','Download captions (VTT)')}</a></div>
<details><summary>{t('動画の操作手順を読む','Read the video steps')}</summary><ol><li>{t('「開く」からサンプル文書を選ぶ。','Select Open and choose the sample document.')}</li><li>{t('見出しを変更し、プレビューへの反映を確認する。','Edit a heading and check the updated preview.')}</li><li>{t('数式とMermaidの図をプレビューで確認する。','Check the equation and Mermaid diagram in the preview.')}</li><li>{t('「PDFに保存」で保存先を選び、保存完了を確認する。','Select Save as PDF, choose a destination and confirm completion.')}</li></ol></details><p class="note">{t('撮影・検証はソース版mdEditor 1.0.5。Storeの公開版・認定状況は未確認です。動画に音声・BGMはありません。','Recorded and tested with source build mdEditor 1.0.5. The Store build and certification status are unverified. No audio or music.')}</p></section>
<section id="samples" aria-labelledby="samples-heading"><div class="section-heading"><p class="eyebrow">{t('まずは1つ','START WITH ONE')}</p><h2 id="samples-heading">{t('この文書で、試してみる。','Try it with a document.')}</h2><p>{t('外部画像のない、短いサンプルです。PCに保存して使えます。','Short samples with no external images. Download and use them on your PC.')}</p></div><ol class="steps"><li><strong>{t('保存する','Download')}</strong><span>{t('下からMarkdown文書を保存。元の文書を残したい場合はコピーを使います。','Download a Markdown document below. Use a copy to keep the original.')}</span></li><li><strong>{t('開いて、変える','Open and edit')}</strong><span>{t('mdEditorの「開く」で選択。文書内の「試してみる」に沿って編集します。','Choose it with Open in mdEditor. Follow the Try it instructions inside.')}</span></li><li><strong>{t('PDFに残す','Save as PDF')}</strong><span>{t('「保存」で文書を保存し、「PDFに保存」でPDFを書き出します。','Select Save for the Markdown, then Save as PDF for the rendered result.')}</span></li></ol><div class="sample-grid">{samples}</div><p class="note">{t('回答整理のサンプルは記入用です。mdEditorにAI生成機能はありません。数式・表の値は自動計算されません。','The answer template is for your own notes. mdEditor has no AI generation feature and does not calculate values in equations or tables.')}</p><a href="https://github.com/5garashi/mdEditor/blob/main/samples/diagram-test{suffix}.md">{t('より多くの図を試す既存サンプル','Existing sample with more diagram types')}</a></section>
<section class="privacy" aria-labelledby="privacy-heading"><h2 id="privacy-heading">{t('文書は、PCの中で。','Your documents stay on your PC.')}</h2><p>{t('文書の編集、図・数式の描画、PDF保存はPC内で処理します。mdEditor自体は文書や個人情報を収集・送信しません。','Editing, diagram and equation rendering, and PDF export happen locally. mdEditor itself does not collect or transmit documents or personal information.')}</p><p>{t('文書の外部画像は、その画像サーバーに接続します。WebView2の更新・診断はMicrosoftの設定に従います。','External images connect to their image servers. WebView2 updates and diagnostics follow Microsoft settings.')}</p><a href="https://github.com/5garashi/mdEditor/blob/main/PRIVACY.md">{t('プライバシーポリシーを読む','Read the privacy policy')}</a></section>
<section class="office" aria-labelledby="office-heading"><p class="eyebrow">5garashi.com DESIGN OFFICE</p><h2 id="office-heading">{t('現場の作業を、使える道具に。','Build tools for the work you do.')}</h2><p>{t('mdEditorは、5garashi.com設計事務所の開発作品です。文書作成や業務の手順を支える道具、技術システムの開発について、事務所サイトからご相談ください。','mdEditor is developed by 5garashi.com Design Office. Visit our website to discuss tools for documents, work procedures and technical systems.')}</p><div class="actions"><a class="button primary" href="https://www.5garashi.com/">{t('事務所・開発相談','Meet the office / discuss a project')}</a><a class="button" href="https://github.com/5garashi/mdEditor/issues">{t('不具合・要望','Feedback and support')}</a></div></section></main>
<footer><p><a href="https://github.com/5garashi/mdEditor">mdEditor · GitHub</a><a href="LICENSE">{t('ライセンス','License')}</a><a href="THIRD-PARTY-NOTICES.md">{t('素材の出典','Asset credits')}</a><a href="PRIVACY.md">{t('このページの通信','Page privacy')}</a></p><p class="note">{t('資料の版','Kit version')}: <span class="version" data-version>{VERSION}</span> · {DATE} JST</p><p class="note">{t('デザイン基準出典','Design reference')}: <a href="https://5garashi.github.io/5garashi-identity/">Design Office Identity v4.12.0</a></p><p class="note">{t('作成者','Author')}: {t('5garashi.com設計事務所 / 5garashi.com Design Office','5garashi.com Design Office')}<br>{t('最終更新','Last updated')}: {DATE} JST</p></footer></body></html>
'''

def uri(path):
    mime=mimetypes.guess_type(path.name)[0] or 'application/octet-stream'
    if path.suffix=='.md': mime='text/markdown;charset=utf-8'
    if path.suffix=='.vtt': mime='text/vtt;charset=utf-8'
    return 'data:'+mime+';base64,'+base64.b64encode(path.read_bytes()).decode()

def build():
    review=ROOT/'review'; review.mkdir(exist_ok=True)
    for en,file in [(False,'index.html'),(True,'en.html')]:
        content=page(en)
        (ROOT/file).write_text(content,encoding='utf-8',newline='\n')
        standalone=content.replace('<link rel="stylesheet" href="style.css">','<style>\n'+(ROOT/'style.css').read_text(encoding='utf-8-sig')+'\n</style>')
        standalone=standalone.replace('<script src="player.js" defer></script>','<script>\ndocument.addEventListener("DOMContentLoaded", () => {\n'+(ROOT/'player.js').read_text(encoding='utf-8')+'\n});\n</script>')
        def embed(m):
            attr,target=m.groups(); path=ROOT/target
            return f'{attr}="{uri(path)}"' if path.is_file() else m.group(0)
        standalone=re.sub(r'(src|srcset|poster|href)="((?:assets/|samples/)[^"]+)"',embed,standalone)
        standalone=re.sub(r'<a class="language-link".*?</a>','',standalone)
        for doc in ['LICENSE','THIRD-PARTY-NOTICES.md','PRIVACY.md']:
            if (ROOT/doc).exists(): standalone=standalone.replace(f'href="{doc}"',f'href="{uri(ROOT/doc)}" download="{doc}"')
        standalone=standalone.replace('File: promotion/'+file,'File: promotion/review/'+file)
        (review/file).write_text(standalone,encoding='utf-8',newline='\n')
    print('Built public pages and standalone review copies; kit '+VERSION)

if __name__=='__main__': build()
