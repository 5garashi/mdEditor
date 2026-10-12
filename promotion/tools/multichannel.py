"""
ファイル名 / File: promotion/tools/multichannel.py
概要 / Description: 日英の投稿・記事・QR付き印刷資料を生成 / Generate bilingual media materials
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import html, json, sys
from build import ROOT, VERSION, DATE, uri
WORK=ROOT.parent.parent/'work'
sys.path.insert(0,str(WORK/'media-deps'))
import qrcode
import qrcode.image.svg

MEDIA=ROOT/'multichannel'
for folder in ['social','articles','print','qr']:(MEDIA/folder).mkdir(parents=True,exist_ok=True)
STORE='https://apps.microsoft.com/detail/9nww2qrclmhc'
OFFICE='https://www.5garashi.com/'
JA=STORE+'?hl=ja-JP&gl=JP'
EN=STORE+'?hl=en-US&gl=US'

def meta(file,en=False,description=None):
    labels=['File','Description','Author','Created','Version','License'] if en else ['ファイル名 / File','概要 / Description','作成者 / Author','作成日 / Created','版 / Version','ライセンス / License']
    values=[file,description or ('mdEditor media material' if en else 'mdEditor媒体別資料 / mdEditor media material'),'5garashi.com Design Office' if en else '5garashi.com設計事務所 / 5garashi.com Design Office',DATE,VERSION,'MIT']
    return '<!--\n'+'\n'.join(a+': '+b for a,b in zip(labels,values))+'\nSPDX-License-Identifier: MIT\n-->\n'

def sign(en=False):
    return f'\n---\n\n**{"Author" if en else "作成者 / Author"}**: {"5garashi.com Design Office" if en else "5garashi.com設計事務所 / 5garashi.com Design Office"}\n**{"Last updated" if en else "最終更新 / Last updated"}**: {DATE} JST\n'

def write_doc(path,text,en=False):
    path.write_text(meta('promotion/'+path.relative_to(ROOT).as_posix(),en)+text+sign(en),encoding='utf-8',newline='\n')

qr_specs=[]
for name,url in [('store',STORE),('office',OFFICE)]:
    qr=qrcode.QRCode(error_correction=qrcode.constants.ERROR_CORRECT_M,box_size=12,border=4)
    qr.add_data(url);qr.make(fit=True)
    # QR ink/white = 8.22:1; a functional code with a four-module quiet zone.
    qr.make_image(fill_color='#385723',back_color='#FFFFFF').save(MEDIA/'qr'/f'{name}.png')
    vector=qr.make_image(image_factory=qrcode.image.svg.SvgPathImage)
    vector.save(MEDIA/'qr'/f'{name}.svg')
    svg_path=MEDIA/'qr'/f'{name}.svg'
    svg_text=svg_path.read_text(encoding='utf-8')
    opening=svg_text.index('>')+1 if svg_text.startswith('<?xml') else 0
    svg_header=f'\n<!--\nFile: promotion/multichannel/qr/{name}.svg\nDescription: QR code for {url}; generated with qrcode 8.2\nAuthor: 5garashi.com Design Office\nCreated: {DATE}\nVersion: {VERSION}\nLicense: MIT\nSPDX-License-Identifier: MIT\n-->\n'
    svg_text=svg_text[:opening]+svg_header+svg_text[opening:]
    svg_text=svg_text.replace('<svg ',f'<svg role="img" aria-label="QR code: {url}" ',1).replace('fill="#000000"','fill="#385723"')
    root_end=svg_text.index('>',svg_text.index('<svg'))+1
    svg_text=svg_text[:root_end]+'<rect width="100%" height="100%" fill="#FFFFFF"/>'+svg_text[root_end:]
    svg_path.write_text(svg_text,encoding='utf-8')
    qr_specs.append({'name':name,'url':url,'modules':qr.modules_count,'quiet_zone_modules':4,'error_correction':'M'})
(MEDIA/'qr/targets.json').write_text(json.dumps(qr_specs,indent=2),encoding='utf-8')

TOPICS={
'read':{
'ja':{'title':'Markdownを、<br>すぐ読む。','lead':'手元のmdファイルを開いて確認。<br>必要なところだけ編集して、PDFへ。','tag':'無料 · Windows','post':'手元のmdファイルを、すぐ読んで、必要なら編集してPDFへ。Windows用「mdEditor」は無料です。5garashi.com設計事務所の開発作品です。','long':'mdファイルを読むたびに、使う道具を選ぶところから始めていませんか。\n\n無料のWindows用アプリ「mdEditor」は、Markdownの編集と整形済みプレビューを並べて使えます。読むことに集中したいときは、プレビューをウィンドウ全体に表示できます。必要な箇所を直したら、PDFとして保存できます。\n\n5garashi.com設計事務所の開発作品です。使ってみた感想をお寄せください。','alt':'無料のWindowsアプリmdEditorの紹介画像。見出しは「Markdownを、すぐ読む。」。実画面には左の編集欄と右の図・数式・表のプレビュー。開発者は5garashi.com設計事務所。'},
'en':{'title':'Open Markdown.<br>Read it right away.','lead':'Open a local .md file and preview it.<br>Edit what you need, then save as PDF.','tag':'FREE · WINDOWS','post':'Open a .md file, read it, make a small edit and save as PDF. mdEditor is a free Windows app by 5garashi.com Design Office.','long':'Need to read a Markdown file without setting up a writing workflow first?\n\nmdEditor is a free Windows app with editing and a rendered preview side by side. Fill the window with the preview when you want to read. Edit what you need and save the rendered document as PDF.\n\nBuilt by 5garashi.com Design Office. Feedback from real use is welcome.','alt':'Promotion for the free Windows app mdEditor. Heading: Open Markdown. Read it right away. The real app shows an editor beside a diagram, equation and table preview. Developed by 5garashi.com Design Office.'}},
'math':{
'ja':{'title':'図と数式も、<br>文書と一緒に。','lead':'Mermaidの図とKaTeXの数式を確認。<br>表も含めて、PDFに保存できます。','tag':'無料 · Windows','post':'図・数式・表を含む技術メモを、Markdownで書いてPDFに。無料のWindows用「mdEditor」で、編集とプレビューを並べて確認できます。開発：5garashi.com設計事務所。','long':'技術メモの文章、図、数式を一つのファイルにまとめたいときに。\n\nmdEditorではMermaidの図とKaTeXの数式をプレビューできます。表と一緒にPDFに保存し、Markdownの編集用ファイルと読み手向けのPDFを残せます。\n\n数値は自分で計算して記載します。mdEditorに自動計算機能はありません。Windows用・無料。5garashi.com設計事務所の開発作品です。','alt':'「図と数式も、文書と一緒に。」という見出しと、mdEditorの実画面。抵抗の消費電力の数式、フローチャート、数値の表がプレビューされている。Windows用・無料。'},
'en':{'title':'Diagrams and math.<br>In one document.','lead':'Preview Mermaid diagrams and KaTeX math.<br>Save them with your tables as PDF.','tag':'FREE · WINDOWS','post':'Keep diagrams, equations and tables in one Markdown note, then save it as PDF. mdEditor is a free Windows app by 5garashi.com Design Office.','long':'Keep the text, diagram and equation for a technical note in one Markdown file.\n\nmdEditor previews Mermaid diagrams and KaTeX equations, and exports the rendered note with its tables as PDF. Keep the Markdown for future edits and the PDF for readers.\n\nCalculate and enter numerical results yourself; mdEditor is not a calculation tool. Free for Windows. Built by 5garashi.com Design Office.','alt':'Heading: Diagrams and math. In one document. The real mdEditor screen previews a resistance power equation, a flowchart and a table. Free for Windows.'}},
'window':{
'ja':{'title':'書く画面と、<br>読む画面を分ける。','lead':'プレビューを別ウィンドウへ。<br>他の作業と、文書を見比べられます。','tag':'無料 · Windows','post':'文書を見ながら、別の画面で作業したいときに。mdEditorはプレビューを別ウィンドウに出せます。Windows用・無料。5garashi.com設計事務所の開発作品です。','long':'文書の内容を確認しながら、別の作業を進めたいときに。\n\nmdEditorではプレビューを別ウィンドウに表示できます。ウィンドウを好きな場所に置いたり、別のディスプレイに移したりして、編集する画面と読む画面を分けられます。\n\n無料のWindows用アプリです。図・数式の表示とPDF保存にも対応しています。開発：5garashi.com設計事務所。','alt':'「書く画面と、読む画面を分ける。」という見出し。mdEditorの編集とプレビューの実画面を掲載し、プレビューの別ウィンドウ表示を紹介。掲載画像は本体内の並列表示の例。'},
'en':{'title':'Write here.<br>Read in another window.','lead':'Open the preview in its own window.<br>Keep your document beside other work.','tag':'FREE · WINDOWS','post':'Keep your document beside other work: mdEditor can open its preview in a separate window. A free Windows app by 5garashi.com Design Office.','long':'Want to keep a document visible while working in another app?\n\nmdEditor can open its preview in a separate window. Move it where you need it, including to another display, while keeping the editor in the main window.\n\nFree for Windows, with diagram and equation previews and PDF export. Built by 5garashi.com Design Office.','alt':'Heading: Write here. Read in another window. An actual side-by-side mdEditor screen accompanies the separate-window feature description; the pictured screen is the main-window layout.'}}
}
TOPICS['window']['ja']['alt']='「書く画面と、読む画面を分ける。」という見出し。本体の編集画面と、別ウィンドウのプレビューの実画面を並べて掲載。ソース版1.0.5で取得。Windows用・無料。'
TOPICS['window']['en']['alt']='Heading: Write here. Read in another window. Two actual screens show the main editor and its detached preview window, captured with source build 1.0.5. Free for Windows.'
TOPICS['window']['ja']['post']='mdEditorのソース版1.0.5では、プレビューを別ウィンドウに表示できます。文書を見ながら他の作業をするときに。Windows用・無料。開発：5garashi.com設計事務所。Storeの提供版は掲載ページで確認してください。'
TOPICS['window']['en']['post']='mdEditor source build 1.0.5 can open its preview in another window. Free for Windows, by 5garashi.com Design Office. Check the Store for the available build.'
TOPICS['window']['ja']['long']+='\n\n別ウィンドウの実画面はソース版1.0.5で確認しました。Storeの提供版番号は未確認です。'
TOPICS['window']['en']['long']+='\n\nThe separate-window screens were checked with source build 1.0.5. The currently available Store build is unverified.'

CSS='''
:root{--paper:#F9EFCB;--head:#FFE699;--ink:#385723;--steel:#7B4A21;--volt:#B92613;--deep:#8C1B0D;--card:#FFFDF2;--line:#DFCF9A}
/* Official pairs: ink/paper 7.14:1, ink/card 8.05:1, steel/paper 6.41:1.
   Additional print pairs: ink/white 8.22:1, steel/white 7.38:1,
   deep/white 9.22:1. Calculation: tools/media-check.py. */
*{box-sizing:border-box}body{margin:0;color:var(--ink);font-family:'Yu Gothic',Meiryo,sans-serif;font-weight:600;line-height:1.7}html[lang=en] body{font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,'Noto Sans','Yu Gothic',Meiryo,sans-serif}h1,h2,h3{margin:0;font-weight:800;line-height:1.3}p{margin:0}img{display:block;max-width:100%;height:auto}a{color:var(--ink);text-underline-offset:.2em}a:focus-visible{outline:3px solid var(--volt);outline-offset:4px}.brandbar{display:flex;align-items:center;justify-content:space-between;background:var(--head);border-bottom:3px solid var(--ink);padding:28px 40px}.logo{width:156px}.mark{width:40px;margin:0 28px}.small{font-size:16px;font-weight:700;color:var(--steel)}.version{font:800 14px ui-monospace,monospace;color:var(--deep);white-space:nowrap}
.social{background:var(--paper);overflow:hidden}.social .brandbar{height:124px}.social .body{padding:42px 52px}.social h1{font-size:56px;letter-spacing:.04em}.social .lead{font-size:26px;line-height:1.65;margin-top:24px}.social .tag{font-size:24px;font-weight:800;color:var(--deep);margin-bottom:16px}.social .screen{border:1px solid var(--line);background:var(--card)}.social .screen-note{font-size:17px;color:var(--steel);font-weight:700;margin-top:12px}.social .creator{font-size:24px;font-weight:800}.social .address{font:800 19px ui-monospace,monospace;color:var(--deep)}.social footer{padding:0 52px 32px}.social .credit{font-size:14px;font-weight:700;color:var(--steel);margin-top:12px}.landscape{width:1600px;height:900px}.landscape .body{display:grid;grid-template-columns:520px 1fr;gap:36px;align-items:center;height:592px}.landscape footer{padding-top:18px}.square{width:1080px;height:1080px}.square .body{padding:32px 48px 20px}.square h1{font-size:49px}.square .lead{font-size:23px;margin-top:14px}.square .tag{font-size:21px;margin-bottom:10px}.square .copy{margin-bottom:26px}.square .screen{width:984px}.square footer{padding:0 48px 28px}.square .creator{font-size:22px}.square .address{font-size:17px}.square .credit{font-size:13px}
.flyer{background:#FFFFFF;font-size:16px}.flyer main{padding:22px 4px}.flyer .brandbar{padding:28px 30px;height:109px}.flyer .logo{width:128px}.flyer .mark{width:30px}.flyer h1{font-size:34px;letter-spacing:.03em}.flyer .intro{font-size:17px;margin:12px 0 18px}.flyer .pill{font-weight:800;color:var(--deep);font-size:16px}.flyer .screen{border:1px solid var(--line);margin:10px 0}.flyer .screen-labels{display:flex;justify-content:space-around;font-weight:800;font-size:15px}.flyer .benefits{display:grid;grid-template-columns:repeat(3,1fr);gap:16px;margin:18px 0}.flyer .benefits h2{font-size:17px}.flyer .benefits p{font-size:14px;line-height:1.65;margin-top:6px}.flyer .qr-row{display:grid;grid-template-columns:1.2fr 1fr;gap:20px;padding-top:16px;border-top:1px solid var(--line)}.flyer .qr-block{display:flex;gap:16px;align-items:center;min-width:0}.flyer .qr-block img{width:144px;flex-shrink:0}.flyer .qr-block.office img{width:116px}.flyer .qr-block h2{font-size:17px}.flyer .qr-block p{font-size:13px;font-weight:700;color:var(--steel)}.flyer .plain-link{font-family:ui-monospace,monospace;font-size:12px;overflow-wrap:anywhere}.flyer footer{margin-top:12px;border-top:1px solid var(--line);padding-top:9px;font-size:12px;line-height:1.65;font-weight:700;color:var(--steel)}.flyer .fine{font-size:12px;font-weight:700;color:var(--steel)}
@page{size:A4 portrait;margin:12mm}@media print{html,body{background:#FFFFFF}.flyer{width:auto;margin:0}.flyer main{padding-bottom:8px}a{color:var(--ink)}.flyer footer{break-inside:avoid}}
@media screen{.flyer{width:702px;min-height:1032px;margin:24px auto;padding:0 0 20px}}
@media(prefers-reduced-motion:reduce){*,*::before,*::after{animation:none!important;transition:none!important}}
@media(forced-colors:active){body,.brandbar{background:Canvas;color:CanvasText}.small,.version,.flyer .fine{color:CanvasText}a{color:LinkText}a:focus-visible{outline-color:Highlight}.logo,.mark,.qr-image{forced-color-adjust:none}}
'''
CSS+='''
.square .body{padding-top:24px}.square .screen{width:860px;margin:0 auto}.square h1{font-size:47px}.square .lead{font-size:22px}
.flyer h1{font-size:30px}.flyer main{padding-top:12px}.flyer .screen{width:540px;margin:10px auto}.flyer .intro{font-size:16px;line-height:1.6;margin:8px 0 12px}.flyer .benefits{gap:12px;margin:14px 0}.flyer .qr-block img{width:128px}.flyer .qr-block.office img{width:116px}
.flyer .plain-link{display:inline-block;min-width:44px;min-height:44px}
.separate-screens{display:grid;grid-template-columns:884fr 634fr;gap:12px}.separate-screens img{width:100%;border:1px solid var(--line)}.separate-screens p{font-size:17px;font-weight:800;margin-bottom:8px}.square .separate-screens{width:860px;margin:auto}.square .screen-note{margin-left:0}
.landscape.topic-window:lang(ja) h1{font-size:50px}
'''
(MEDIA/'layout.css').write_text('/*\nFile: promotion/multichannel/layout.css\nDescription: Native media and print layouts\nAuthor: 5garashi.com Design Office\nCreated: 2026-10-13\nVersion: '+VERSION+'\nLicense: MIT\nSPDX-License-Identifier: MIT\n*/\n'+CSS,encoding='utf-8')

def brand(lang):
    logo='5garashi_logotype_en_outlined.svg' if lang=='en' else '5garashi_logotype_outlined.svg'
    return '<header class="brandbar"><img class="logo" src="'+uri(ROOT/'assets'/logo)+'" alt="5garashi.com Design Office"><img class="mark" src="'+uri(ROOT/'assets/5garashi_mark_simple.svg')+'" alt="" aria-hidden="true"></header>'

def shell(file,lang,body,kind,title):
    return meta('promotion/'+file,lang=='en')+f'<!doctype html><html lang="{lang}"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>{title}</title><style>{CSS}</style></head><body class="{kind}">{body}</body></html>'

for topic,pair in TOPICS.items():
    for lang,content in pair.items():
        for fmt in ['landscape','square']:
            name=f'{topic}-{lang}-{fmt}'
            en=lang=='en'
            screen_note=('Two actual windows · source build 1.0.5' if topic=='window' else 'Actual Windows app · source build 1.0.5') if en else ('2つのウィンドウの実画面 · ソース版1.0.5' if topic=='window' else 'Windows版の実画面 · ソース版1.0.5')
            body=brand(lang)+f'<main class="body"><div class="copy"><p class="tag">{content["tag"]}</p><h1>{content["title"]}</h1><p class="lead">{content["lead"]}</p></div><figure style="margin:0"><img class="screen" src="{uri(ROOT/"assets"/f"app-{lang}.png")}" alt="{html.escape(content["alt"])}"><figcaption class="screen-note">{screen_note}</figcaption></figure></main><footer><p class="creator">mdEditor · {"5garashi.com Design Office" if en else "5garashi.com設計事務所"}</p><p>{"Free on Microsoft Store" if en else "Microsoft Storeで無料配布"}</p><p class="address">apps.microsoft.com/detail/9nww2qrclmhc</p><p class="credit">Design: 5garashi.com Design Office · Identity v4.12.0 · {DATE} · Kit <span class="version" data-version>{VERSION}</span></p></footer>'
            if topic=='window':
                single=f'<img class="screen" src="{uri(ROOT/"assets"/f"app-{lang}.png")}" alt="{html.escape(content["alt"])}">'
                images='<div class="separate-screens">'
                for role,label in [('editor','Editor window' if en else '編集ウィンドウ'),('preview','Preview window' if en else 'プレビューウィンドウ')]:
                    images+=f'<div><p>{label}</p><img src="{uri(MEDIA/"illustrations"/f"window-{role}-{lang}.png")}" alt="{label} — mdEditor 1.0.5"></div>'
                images+='</div>'
                body=body.replace(single,images)
            (MEDIA/'social'/f'{name}.html').write_text(shell(f'multichannel/social/{name}.html',lang,body,f'social {fmt} topic-{topic}',f'mdEditor — {topic}'),encoding='utf-8')

for lang in ['ja','en']:
    en=lang=='en';link=EN if en else JA
    text=('# Social post drafts\n\nThese are reviewable drafts, not published posts. Use the short copy with its image; use the longer copy where more context is appropriate. Adapt to the platform and check its current rules.\n\n' if en else '# SNS投稿文の下書き\n\n未投稿の確認用素材です。短文は画像と組み合わせ、長文は用途を説明する投稿に使います。投稿先の現在のルールと文字数を確認して調整してください。\n\n')
    for topic,pair in TOPICS.items():
        c=pair[lang]
        text+=f'## {html.unescape(c["title"].replace("<br>"," "))}\n\n'
        text+=('### Short copy\n\n' if en else '### 短文\n\n')+c['post']+'\n\n'+link+'\n\n'
        text+=('### Longer copy\n\n' if en else '### 長文\n\n')+c['long']+'\n\n'+link+'\n'+OFFICE+'\n\n'
        text+=('### Image and alt text\n\n' if en else '### 画像と代替テキスト\n\n')+f'[{"Landscape" if en else "横長"}]({topic}-{lang}-landscape.png) / [{"Square" if en else "正方形"}]({topic}-{lang}-square.png)\n\n'+c['alt']+'\n\n'
    text+=('Optional tags: `#Markdown #Windows`. Avoid treating these drafts as user reviews. Prices are free as confirmed by the owner and the Store listing; exact Store build availability remains unverified.\n\nDesign reference: [Design Office Identity](https://5garashi.github.io/5garashi-identity/). Official logos remain CC BY-ND 4.0; copy and layout are MIT.\n' if en else '任意のタグ: `#Markdown #Windows`。開発者の投稿として使い、第三者の口コミとしては使いません。無料配布は所有者の確認とStore掲載情報に基づきます。Storeの提供版番号は未確認です。\n\nデザイン基準出典: [Design Office Identity](https://5garashi.github.io/5garashi-identity/)。公式ロゴはCC BY-ND 4.0、文章・レイアウトはMIT。\n')
    write_doc(MEDIA/'social'/f'posts-{lang}.md',text,en)

for lang in ['ja','en']:
    en=lang=='en'
    t=lambda ja,eng:eng if en else ja
    title=t('Markdownを、すぐ読む。<br>必要なら、PDFに。','Open Markdown.<br>Read, edit and save as PDF.')
    body=brand(lang)+f'''<main><p class="pill">mdEditor · {t('無料 · Windows','FREE · WINDOWS')}</p><h1>{title}</h1><p class="intro">{t('手元のmdファイルを開いて、図・数式・表を確認。<br>編集とプレビューを並べて、必要なところだけ直せます。','Open a local .md file and preview its diagrams, math and tables.<br>See the rendered document beside your edits.')}</p><div class="screen-labels"><span>{t('左で編集','Edit on the left')}</span><span>{t('右で仕上がりを確認','Preview on the right')}</span></div><img class="screen" src="{uri(ROOT/'assets'/f'app-{lang}.png')}" alt="{html.escape(TOPICS['read'][lang]['alt'])}"><div class="benefits"><section><h2>{t('読む・少し直す','Read and edit')}</h2><p>{t('プレビューを広げて読む。必要な箇所を編集する。','Fill the window with the preview. Edit what you need.')}</p></section><section><h2>{t('図と数式をPDFへ','Diagrams and PDF')}</h2><p>{t('Mermaid・KaTeX・表を表示し、PDFに保存。','Preview Mermaid, KaTeX and tables. Save as PDF.')}</p></section><section><h2>{t('別窓で見比べる','A separate window')}</h2><p>{t('プレビューを別ウィンドウに出して使える。','Open the preview in its own window.')}</p></section></div><div class="qr-row"><section class="qr-block"><img class="qr-image" src="{uri(MEDIA/'qr/store.png')}" alt="{t('Microsoft StoreのmdEditorへ','mdEditor in Microsoft Store')}"><div><h2>{t('無料で試す','Try it for free')}</h2><p>Microsoft Store</p><a class="plain-link" href="{STORE}">apps.microsoft.com<br>/detail/9nww2qrclmhc</a></div></section><section class="qr-block office"><img class="qr-image" src="{uri(MEDIA/'qr/office.png')}" alt="{t('5garashi.com設計事務所のサイトへ','5garashi.com Design Office website')}"><div><h2>{t('開発した事務所','Meet the developer')}</h2><p>{t('5garashi.com設計事務所','5garashi.com Design Office')}</p><a class="plain-link" href="{OFFICE}">www.5garashi.com</a></div></section></div></main><footer><p>{t('画面・機能の確認はソース版1.0.5。Storeの提供版は掲載ページで確認してください。','Screen and features verified with source build 1.0.5. Check the Store for the available build.')}</p><p>{t('デザイン基準','Design reference')}: Identity v4.12.0 · {t('公式ロゴ','Official logos')}: CC BY-ND 4.0 · {t('文章・構成','Copy and layout')}: MIT</p><p>{t('作成者 / Author','Author')}: {t('5garashi.com設計事務所 / 5garashi.com Design Office','5garashi.com Design Office')} · {t('最終更新','Last updated')}: {DATE} JST · <span class="version" data-version>{VERSION}</span></p></footer>'''
    (MEDIA/'print'/f'one-page-{lang}.html').write_text(shell(f'multichannel/print/one-page-{lang}.html',lang,body,'flyer','mdEditor — '+t('1枚紹介','One-page introduction')),encoding='utf-8')

print('Generated 12 social layouts, bilingual post drafts, QR codes and two one-page print layouts')
