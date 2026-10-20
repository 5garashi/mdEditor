"""
File: promotion/integration/build.py
Description: Build a GitHub Pages site and bilingual office-site embed from existing materials
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import html
import shutil
import argparse

HERE = Path(__file__).resolve().parent
PROMO = HERE.parent
BASE = 'https://5garashi.github.io/mdEditor/'
VERSION = (HERE / 'VERSION').read_text().strip()
SOURCE_REF = 'promotion/site-integration'
SOURCE = 'https://github.com/5garashi/mdEditor/'

def generate(output):
    if output == PROMO or PROMO in output.parents or output in PROMO.parents:
        raise ValueError('Choose a dedicated output folder outside the source promotion directory.')
    output.mkdir(parents=True, exist_ok=True)
    for name in ['index.html', 'en.html', 'style.css', 'player.js', 'VERSION', 'LICENSE', 'PRIVACY.md', 'THIRD-PARTY-NOTICES.md']:
        shutil.copy2(PROMO / name, output / name)
    for folder in ['assets', 'samples', 'licenses', 'multichannel']:
        shutil.copytree(PROMO / folder, output / folder, dirs_exist_ok=True)
    # Old slide video is retained in Git but excluded from this public output.
    for name in ['mdEditor-intro-en.mp4', 'video-poster.png']:
        old = output / 'assets' / name
        if old.exists():
            old.unlink()
    (output / '.nojekyll').write_text('', encoding='utf-8')
    for en in [False, True]:
        lang = 'en' if en else 'ja'
        landing = 'en.html' if en else 'index.html'
        resources = 'resources-en.html' if en else 'resources.html'
        case = 'case-study-en.html' if en else 'case-study.html'
        store = 'https://apps.microsoft.com/detail/9nww2qrclmhc?hl=' + ('en-US&gl=US' if en else 'ja-JP&gl=JP')
        def t(ja, eng): return eng if en else ja
        def link(url, label): return '<a class="button" href="' + html.escape(url, quote=True) + '">' + html.escape(label) + '</a>'
        def section(title, body): return '<section><h2>' + title + '</h2>' + body + '</section>'
        def page(title, body, filename):
            original = (PROMO / landing).read_text(encoding='utf-8')
            header = original[original.index('<header'):original.index('</header>') + len('</header>')]
            # Keep official logo layout/theme handling, replace navigation only.
            nav = '<nav aria-label="' + t('主なページ', 'Main pages') + '">' + ''.join([
                link(landing, t('アプリ紹介', 'App')),
                link(resources, t('資料・使い方', 'Resources')),
                link(case, t('制作事例', 'Case study')),
                link(('resources.html' if en else 'resources-en.html') if filename == resources else ('case-study.html' if en else 'case-study-en.html'), t('English', '日本語'))]) + '</nav>'
            import re
            header = re.sub(r'<nav\b.*?</nav>', nav, header, flags=re.S)
            doc = f'''<!--
ファイル名 / File: {filename}
概要 / Description: 日英の公開導線 / Bilingual publication links
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
版 / Version: {VERSION} (integration)
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
<!doctype html><html lang="{lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="color-scheme" content="light dark"><title>{html.escape(title)} | 5garashi.com</title><link rel="stylesheet" href="style.css"><script src="player.js" defer></script><style>main{{padding-top:2rem}}section{{max-width:900px}}.actions{{align-items:start}}</style></head><body><a class="skip" href="#main">{t('本文へ移動','Skip to content')}</a>{header}<main id="main"><h1>{title}</h1>{body}</main><footer><p>{link('https://www.5garashi.com/', t('開発した事務所','Developer'))}{link('https://github.com/5garashi/mdEditor/tree/main/promotion', t('資料の原本・更新履歴','Source and history'))}</p><p class="note">{t('資料キット','Material kit')}: {(PROMO/'VERSION').read_text().strip()} · {t('導線','Integration')}: {VERSION}</p><p class="note">Design Office Identity 4.20.0 · <a href="https://5garashi.github.io/5garashi-identity/">{t('デザイン基準','Design reference')}</a></p><p class="note">5garashi.com設計事務所 / 5garashi.com Design Office · 2026-10-20 JST</p></footer></body></html>'''
            doc = doc.replace(SOURCE+'tree/main/promotion',SOURCE+'tree/'+SOURCE_REF+'/promotion')
            doc = doc.replace(SOURCE+'blob/main/promotion',SOURCE+'blob/'+SOURCE_REF+'/promotion')
            (output / filename).write_text(doc, encoding='utf-8')
        body = '<p>' + t('無料のWindowsアプリmdEditorを試すための資料です。', 'Resources for trying mdEditor, a free Windows app.') + '</p><div class="actions">' + link(store,t('Microsoft Storeで無料入手','Get it free from Microsoft Store')) + link(landing+'#demo',t('1.0.14の実演を見る','Watch the 1.0.14 demo')) + '</div>'
        body += section(t('サンプルを保存して開く','Save and open a sample'), '<p>'+t('MarkdownファイルをPCに保存し、mdEditorの「開く」で選びます。','Save the Markdown file to your PC, then choose Open in mdEditor.')+'</p><div class="actions">'+''.join(link('samples/'+name+('.en' if en else '')+'.md',label) for name,label in [('quickstart-engineering-note',t('図と数式の技術メモ','Diagrams and math')),('quickstart-workflow',t('業務手順書','Workflow')),('quickstart-ai-notes',t('回答整理用テンプレート','Answer notes template'))])+'</div><p class="note">'+t('リンク先が表示された場合はブラウザーの保存操作で保存してください。AI生成や数値の自動計算機能はありません。','If your browser displays the file, save it using the browser. No AI generation or automatic calculation is provided.')+'</p>')
        body += section(t('動画と同じサンプルを見る','Try the sample from the video'),link('samples/diagram-test'+('.en' if en else '')+'.md',t('diagram-testを開く','Open diagram-test'))+'<p>'+t('STL・DOT・GeoJSONを含むmdEditor向けサンプルです。ネット上の画像・動画・詳細地図は、許可した場合に読み込みます。','A product sample including STL, DOT and GeoJSON. Internet media and detailed maps load only with permission.')+'</p>')
        article = 'technical-note-'+lang+'.md'
        body += section(t('図・3Dから始めて、文書を共有する','Explore diagrams and 3D, then share a document'),'<p>'+t('記述例と操作手順を読む。記事の原本をGitHubで表示します。','Read examples and steps. The original article opens on GitHub.')+'</p>'+link('https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/articles/'+article,t('使い方の記事を読む','Read the use-case article')))
        body += section(t('人に紹介する','Share with someone'), '<div class="actions">'+link('multichannel/update-2026-10-20/mdEditor-materials-1.0.14.zip',t('日英の配布ZIPを保存','Download the bilingual kit'))+link('multichannel/update-2026-10-20/one-page-'+lang+'.pdf',t('1枚紹介PDFを開く','Open the one-page PDF'))+link('https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/update-2026-10-20/posts-'+lang+'.md',t('投稿文と画像の一覧','Social copy and images'))+'</div>')
        body += section(t('主な20機能を読む','Read the twenty main features'),link('multichannel/update-2026-10-20/features-'+lang+'.pdf',t('機能一覧PDFを開く','Open the feature guide PDF')))+ '<p class="note">'+t('主画面・新動画はソース版1.0.14。旧素材は各資料に版を明記。','Main screenshots and new demos use source build 1.0.14. Earlier assets retain version labels. ')+'</p>'+link(case,t('この資料の制作事例','About this production'))
        page(t('mdEditor 資料・使い方','mdEditor resources'),body,resources)
        body = '<p>'+t('Windowsアプリの開発と、その用途を伝える日英の資料制作を紹介します。','A Windows app and bilingual materials explaining how to use it.')+'</p>'
        for title,text in [(t('アプリと実演','App and demonstration'),t('編集、図・数式のプレビュー、PDF保存を実際の操作で紹介。','Actual editing, diagram and math previews, and PDF export.')),(t('Web・記事・サンプル','Web, articles and samples'),t('用途を知る、操作を見る、文書を開いて試す、という流れを用意。','A path from discovering a use case to watching and trying it.')),(t('SNS画像と印刷資料','Social images and print'),t('用途別の日英画像と、Store・事務所へつながるQR付きPDFを制作。','Bilingual use-case images and a PDF linking to the Store and office.'))]:
            body += section(title,'<p>'+text+'</p>')
        body += '<figure><img src="multichannel/update-2026-10-20/social-'+lang+'.png" alt="'+t('mdEditorのSNS紹介画像','mdEditor social introduction image')+'"><figcaption>'+t('制作したSNS画像の例','Example social image')+'</figcaption></figure><div class="actions">'+link(resources,t('資料と使い方を見る','Explore the materials'))+link('https://sites.google.com/5garashi.com/contact',t('開発・資料制作を相談する','Discuss development and materials'))+'</div><p class="note">'+t('これは制作物の紹介です。未確認の集客効果や利用者数は掲載していません。','This describes produced materials. Audience growth and user counts have not been established.')+'</p>'
        page(t('制作事例：mdEditor','Case study: mdEditor'),body,case)
        # Navigation is generated for publication; the kit source remains unchanged.
        published = (output / landing).read_text(encoding='utf-8')
        published = published.replace('</nav>', link(resources,t('資料・使い方','Resources')) + link(case,t('制作事例','Case study')) + '</nav>', 1)
        (output / landing).write_text(published,encoding='utf-8')
    for en in [False, True]:
        lang='en' if en else 'ja'
        target = BASE + ('en.html' if en else '')
        logo=(PROMO/'assets'/('5garashi_logotype_en_outlined.svg' if en else '5garashi_logotype_outlined.svg')).read_text(encoding='utf-8')
        mark=(PROMO/'assets/5garashi_mark_simple.svg').read_text(encoding='utf-8')
        css=(PROMO/'style.css').read_text(encoding='utf-8')
        bridge=f'''<!-- File: office-card-{lang}.html | Author: 5garashi.com Design Office | Created: 2026-10-13 | Version: {VERSION} | License: MIT | SPDX-License-Identifier: MIT -->
<!doctype html><html lang="{lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><meta name="color-scheme" content="light dark"><title>mdEditor</title><style>{css}
.office-card{{padding:1.5rem;max-width:900px;margin:auto}}.card-heading{{display:flex;align-items:center;justify-content:space-between;gap:2rem;background:var(--head);border-bottom:3px solid var(--ink);padding:28px}}.card-heading .logo svg{{width:140px;height:auto;min-height:32px}}.card-heading .mark svg{{width:32px;height:32px}}.office-card h1{{font-size:1.6rem;margin-top:1rem}}.office-card .button{{flex:1;min-width:0}}@media(prefers-color-scheme:dark){{.card-heading .logo{{display:none}}}}@media(forced-colors:active){{.card-heading .logo,.card-heading .mark{{forced-color-adjust:none}}}}</style></head><body><article class="office-card"><header class="card-heading"><span class="logo" aria-label="5garashi.com">{logo}</span><span class="mark" aria-hidden="true">{mark}</span></header><h1>{'mdEditor — Diagrams, 3D and video.' if en else 'mdEditor — 図も、3Dも、動画も。'}</h1><p>{'Source 1.0.14: diagrams, math, interactive 3D, maps and video in your document. Edit with a live preview and share PDF or Word. A free Windows app by 5garashi.com Design Office.' if en else 'ソース1.0.14：図・数式・回転する3D・地図・動画を文書の中で。書きながら確認し、PDF・Wordなどで共有。5garashi.com設計事務所が開発する無料のWindowsアプリです。'}</p><div class="actions"><a class="button primary" target="_blank" rel="noopener" href="https://apps.microsoft.com/detail/9nww2qrclmhc?hl={'en-US&amp;gl=US' if en else 'ja-JP&amp;gl=JP'}">{'Get it free from Microsoft Store' if en else 'Microsoft Storeで無料入手'}</a><a class="button" target="_blank" rel="noopener" href="{target}">{'See how it works' if en else '使い方・実演を見る'}</a></div><p><a target="_blank" rel="noopener" href="{BASE}{'resources-en.html' if en else 'resources.html'}">{'Samples and PDF' if en else 'サンプル・紹介PDF'}</a> · <a target="_blank" rel="noopener" href="{BASE}{'case-study-en.html' if en else 'case-study.html'}">{'Case study' if en else '制作事例'}</a></p><p class="note">{'Links open in a new tab.' if en else 'リンクは新しいタブで開きます。'}</p></article></body></html>'''
        # Use the official reversed logo in night mode, without recoloring.
        reverse=(PROMO/'assets'/('5garashi_logotype_en_reversed_outlined.svg' if en else '5garashi_logotype_reversed_outlined.svg')).read_text(encoding='utf-8')
        bridge=bridge.replace('</span><span class="mark"',f'</span><span class="logo-night" aria-label="5garashi.com">{reverse}</span><span class="mark"',1)
        bridge=bridge.replace('</style>','.logo-night{display:none}.logo-night svg{width:140px;height:auto;min-height:32px}@media(prefers-color-scheme:dark){.logo-night{display:block}}@media(forced-colors:active){.logo-night{forced-color-adjust:none}}</style>')
        if not en:
            bridge=bridge.replace('<!-- File:', '<!-- ファイル名 / File:').replace(' | Author: 5garashi.com Design Office', ' | 作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office').replace(' | Created:', ' | 作成日 / Created:').replace(' | Version:', ' | 版 / Version:').replace(' | License:', ' | ライセンス / License:')
        (HERE/f'office-card-{lang}.html').write_text(bridge,encoding='utf-8')
        # Embed this URL in Google Sites to keep card maintenance in GitHub too.
        (output/f'office-card-{lang}.html').write_text(bridge,encoding='utf-8')
    from youtube.automation.latest_pages import generate_latest
    generate_latest(output, PROMO.parent / '.youtube-runtime' / 'state.json')
    print('Built:', output)

if __name__ == '__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--output',type=Path,required=True)
    generate(parser.parse_args().output.resolve())
