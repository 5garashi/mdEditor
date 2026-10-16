"""
File: build-update-kit.py
Description: Build bilingual mdEditor update materials from released screenshots
Author: 5garashi.com Design Office
Created: 2026-10-17
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import shutil, json, hashlib, zipfile
from PIL import Image, ImageDraw, ImageFont
import fitz

ROOT = Path(__file__).resolve().parents[4]
OUT = Path(__file__).resolve().parent
PROMO = OUT.parents[1]
OUT.mkdir(exist_ok=True)
PAPER, INK, HEAD, STEEL = '#F9EFCB','#385723','#FFE699','#7B4A21'
FONT = 'C:/Windows/Fonts/meiryob.ttc'
SIGN = '\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: 2026-10-17 JST\n'
def header(name,desc):
    return f'<!--\nファイル名 / File: {name}\n概要 / Description: {desc}\n作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n作成日 / Created: 2026-10-17\n版 / Version: 1.5.1\nライセンス / License: MIT（公式ロゴを除く / except official logos）\nSPDX-License-Identifier: MIT\n-->\n'
def write(name,desc,body):
    (OUT/name).write_text(header(name,desc)+body+SIGN,encoding='utf-8')
copy = {
'ja': dict(title='読み込む。整える。渡す。', sub='Word・Web・CSVから、Markdownへ。', lines=['Word・HTML・メール・CSV・Notebookを開く','図と数式を見ながら、文章を整える','Word・PDF・HTML・PNGへ書き出す'],note='Wordの色や配置は引き継ぎません。図・数式は画像としてWordへ。',url='https://5garashi.github.io/mdEditor/',alt='クリーム色のmdEditor更新案内。Word・Web・CSVからMarkdownへ取り込み、Word・PDF・HTML・PNGへ書き出す用途を紹介。実画面は1.0.10で編集欄と図・数式のプレビューを並べた場面。'),
'en': dict(title='Bring it in. Make it clear.',sub='From Word, web pages and CSV to Markdown.',lines=['Open Word, HTML, email, CSV and notebooks','Edit with diagrams and equations in view','Export to Word, PDF, HTML and PNG'],note='Word layout is not preserved. Diagrams and math export to Word as pictures.',url='https://5garashi.github.io/mdEditor/en.html',alt='Cream mdEditor update card showing import from Word, web pages and CSV, and export to Word, PDF, HTML and PNG. The actual 1.0.10 screenshot shows Markdown editing beside a preview with diagrams and math.')}
for lang,c in copy.items():

    shot=(PROMO/'assets')/f'app-{lang}.png'
    shutil.copy2(shot,OUT/f'app-{lang}.png')
    im=Image.new('RGB',(1600,1200),PAPER); d=ImageDraw.Draw(im)
    def txt(x,y,s,size=32,color=INK): d.text((x,y),s,font=ImageFont.truetype(FONT,size),fill=color)
    d.rectangle((0,0,1600,100),fill=HEAD); d.line((0,100,1600,100),fill=INK,width=5)
    logo=fitz.open(PROMO/f'assets/5garashi_logotype{"_en" if lang=="en" else ""}_outlined.svg')
    pix=logo[0].get_pixmap(matrix=fitz.Matrix(2,2),alpha=True)
    mark=Image.frombytes('RGBA',(pix.width,pix.height),pix.samples); mark.thumbnail((270,72)); im.paste(mark,(45,14),mark)
    txt(1200,28,'mdEditor',36)
    txt(55,125,c['title'],58); txt(55,211,c['sub'],34)
    for i,line in enumerate(c['lines']): txt(58,292+i*53,f'{i+1}  {line}',32)
    screenshot=Image.open(shot).convert('RGB'); screenshot.thumbnail((1480,570))
    im.paste(screenshot,((1600-screenshot.width)//2,475))
    txt(55,1060,c['note'],23,STEEL); txt(55,1112,c['url'],27)
    txt(55,1160,'2026-10-17 | Source 1.0.10 | Screenshot 1.0.10 | 5garashi.com Design Office',19,STEEL)
    im.save(OUT/f'update-{lang}-landscape.png')
    doc=fitz.open(); p=doc.new_page(width=595.28,height=841.89)
    p.insert_image(fitz.Rect(0,0,595.28,446.46),filename=str(OUT/f'update-{lang}-landscape.png'))
    font=fitz.Font(fontfile=FONT); writer=fitz.TextWriter(p.rect)
    details = (['mdEditor 更新案内','主な機能追加: 1.0.8 / 最新ソース: 1.0.10','Wordの見出し・リスト・表・リンク・画像を取り込めます。','書き出したWordでは、見出しなどの標準スタイルを使えます。','画像の貼り付けとドラッグ＆ドロップにも対応。','外部画像は初期設定では読み込まず、確認後に取得します。','Wordの色・配置やNotebookの実行結果の再出力は対象外です。','Microsoft Storeの配信版番号は今回未確認です。'] if lang=='ja' else ['mdEditor update','Main additions: 1.0.8 / Latest source: 1.0.10','Import Word headings, lists, tables, links and images.','Word export uses native heading and paragraph styles.','Paste or drag images into your document.','Internet images load only after consent by default.','Word layout and notebook output export are not preserved.','The current Microsoft Store build number is unverified.'])
    for i,line in enumerate(details): writer.append((28,482+i*28),line,font=font,fontsize=12 if i else 19)
    writer.append((28,744),c['url'],font=font,fontsize=10)
    writer.append((28,778),'Design: 5garashi Design Office Identity 4.15.0',font=font,fontsize=10)
    writer.append((28,801),'5garashi.com Design Office | 2026-10-17 JST',font=font,fontsize=10)
    writer.write_text(p,color=(56/255,87/255,35/255))
    p.insert_link({'kind':fitz.LINK_URI,'from':fitz.Rect(25,727,570,753),'uri':c['url']})
    doc.subset_fonts(); doc.save(OUT/f'one-page-{lang}.pdf', garbage=4, deflate=True)

ja='mdEditorを更新しました。Word・Webページ・CSVなどをMarkdownで開き、図や数式を見ながら編集。Word・PDF・HTML・PNGで渡せます。\n\n使い方とサンプル\nhttps://5garashi.github.io/mdEditor/\n\n#mdEditor #Markdown #Windows'
en='I updated mdEditor for Windows. Open Word, web pages and CSV as Markdown, then export to Word, PDF, HTML or PNG.\n\nGuide and samples:\nhttps://5garashi.github.io/mdEditor/en.html\n\n#Markdown #Windows'
for lang,body in [('ja',ja),('en',en)]:
    long=body+'\n\n'+ ('既存の資料を取り込み、文章を整えて渡す作業に使えます。Wordへの書き出しでは見出しや表に標準スタイルを使い、図と数式は画像として入ります。Wordを読み込む際、色や配置は引き継ぎません。インターネット上の画像は初期設定では確認後に読み込みます。\n\n主な追加は1.0.8、最新ソースは1.0.10です。今回の実画面は1.0.10。Storeの配信版番号は未確認です。' if lang=='ja' else 'Bring existing material in, edit it, and share it in the format your reader needs. Word export uses native heading and table styles; diagrams and math are pictures. Importing Word does not preserve colors or layout. Internet images require consent by default.\n\nMain additions are in 1.0.8; latest source is 1.0.10. Screenshots show 1.0.10. The Store build number is unverified.')
    write(f'posts-{lang}.md','SNS更新原稿 / Social update copy',f'# SNS更新原稿 / Social update copy\n\n## X\n\n```text\n{body}\n```\n\n## LinkedIn / YouTube update description\n\n```text\n{long}\n```\n\n## 画像 / Image\n\n`update-{lang}-landscape.png`\n\n代替テキスト / Alt text:\n\n{copy[lang]["alt"]}\n\n## TikTok\n\n'+ ('1.0.10の実演動画を公開済み。https://www.youtube.com/watch?v=pXQqzLLf8FM' if lang=='ja' else 'A new source 1.0.10 demo is public: https://www.youtube.com/watch?v=FxBILEFQMQk'))
write('README.md','更新資料と検証 / Update material and verification','# mdEditor 更新資料 / Update materials\n\n資料版 / Kit: 1.5.1 · 2026-10-17\n\n日英のSNS画像、紹介PDF、投稿原稿です。主な機能追加は1.0.8、最新ソースは1.0.10。画面はソース1.0.10の実録画から取得し、画面内容を加工していません。Storeの配信版番号は未確認です。\n\nBilingual social images, one-page PDFs and post copy. Main additions: 1.0.8; latest source: 1.0.10; official screenshots: 1.0.10.\n\n- `posts-ja.md` / `posts-en.md`: 媒体別原稿と画像の代替テキスト。\n- `update-*-landscape.png`: 1600×1200の投稿画像。\n- `one-page-*.pdf`: A4の紹介PDF。URLはクリック可能。\n- `PUBLICATION.md`: 公開結果。原稿の完成と公開を区別します。\n\n機能の根拠: https://github.com/5garashi/mdEditor/blob/main/CHANGELOG.md\n\nデザイン出典: [Design Office Identity 4.15.0](https://5garashi.github.io/5garashi-identity/)（第3・4・6・7・8・9・12・13条、2026-10-17確認）。本文・制作コードは既存資料に従いMIT。公式ロゴはCC BY-ND 4.0で、形と色を変更していません。アプリ画面はmdEditorの公式掲載素材です。\n\n確認範囲: Pillowで画像寸法、PyMuPDFでPDFのページ数・文字抽出・リンクを確認。本文ink/paperの比はWCAG相対輝度式で測定しvalidation.jsonに記録。PNGは固定画像のため代替テキストを併記。PDFはタグ付きPDFではありません。読み上げ・OSコントラストテーマ・拡大時の実ブラウザー表示は未確認で、WCAG 2.2 AAの完全準拠を宣言しません。新しい操作画面・音声・外部ファイル取得機能はありません。')
def lum(h):
    rgb=[int(h[i:i+2],16)/255 for i in (1,3,5)]
    rgb=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
    return sum(v*k for v,k in zip(rgb,[.2126,.7152,.0722]))
ratios={f'{a}/{b}':(max(lum(a),lum(b))+.05)/(min(lum(a),lum(b))+.05) for a,b in [(INK,PAPER),(STEEL,PAPER),(INK,HEAD)]}
assert ratios[f'{INK}/{PAPER}']>=7
checks={}
for lang in copy:
    doc=fitz.open(OUT/f'one-page-{lang}.pdf'); assert len(doc)==1 and doc[0].get_links() and len(doc[0].get_text())>200
    assert Image.open(OUT/f'update-{lang}-landscape.png').size==(1600,1200)
    checks[lang]={'pdf_pages':len(doc),'pdf_text_characters':len(doc[0].get_text()),'pdf_links':len(doc[0].get_links())}
(OUT/'validation.json').write_text(json.dumps({'date':'2026-10-17','contrast':ratios,'files':checks},indent=2),encoding='utf-8')
print(OUT)
print(json.dumps(checks))
