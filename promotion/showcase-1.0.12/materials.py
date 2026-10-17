"""File: materials.py
Description: Create bilingual promotion drafts around the actual visual showcase
Author: 5garashi.com Design Office
Created: 2026-10-17
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import shutil,json,html
from PIL import Image,ImageDraw,ImageFont
import fitz
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.12';OUT.mkdir(exist_ok=True);A=OUT/'assets';A.mkdir(exist_ok=True)
FONT='C:/Windows/Fonts/meiryob.ttc'
COPY={
'ja':{'title':'図も、3Dも、動画も。','sub':'ひとつのMarkdownから、表現が広がる。','intro':'最初にdiagram-test.mdを上から下へ。図の連続表示、3Dの拡大・回転、詳細地図、文書内動画を見てから、新機能と編集・保存の操作を紹介します。','features':['3Dはドラッグで回転、ホイールで拡大・縮小','GitHub形式の注意書き・数式・HTML・YAML','地図と動画を読み込み、文書の中で表示','Wordを取り込み、Word・PDF・HTML・PNGへ'],'guide':'https://5garashi.github.io/mdEditor/','note':'実録画はソース版1.0.12。Store配信版の番号は今回未確認です。','alt':'mdEditor 1.0.12の実画面。クリーム色の立方体に赤い5が浮き出た3Dモデルと、東京・大阪の線と富士山周辺の円を描いた詳細地図。'},
'en':{'title':'One Markdown. A visual world.','sub':'Diagrams, 3D, maps and video — then edit and share.','intro':'Begin with a top-to-bottom tour of diagram-test.en.md. Explore diagrams, zoom and rotate 3D, and load maps and video. Then follow the new features and real editing and saving operations.','features':['Rotate 3D by dragging; zoom with the wheel','GitHub-style alerts, math, HTML and YAML cards','Load maps and video inside your document','Import Word; export Word, PDF, HTML and PNG'],'guide':'https://5garashi.github.io/mdEditor/en.html','note':'Real recordings use source build 1.0.12. The current Store build number is unverified.','alt':'Actual mdEditor 1.0.12 screens showing a cream 3D cube with raised red fives, and a detailed map with Tokyo and Osaka joined by a line and a circle around Mt. Fuji.'}}
def header(name):return f'<!--\nファイル名 / File: {name}\n概要 / Description: 1.0.12の視覚的な紹介資料 / Visual showcase materials\n作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n作成日 / Created: 2026-10-17\n版 / Version: 1.0.0\nライセンス / License: MIT（第三者素材を除く / excluding third-party assets）\nSPDX-License-Identifier: MIT\n-->\n'
def logo_image(lang):
 path=ROOT/f'official-assets/5garashi_logotype{"_en" if lang=="en" else ""}_outlined.svg';shutil.copy2(path,A/f'logo-{lang}.svg');reverse=ROOT/f'official-assets/5garashi_logotype{"_en" if lang=="en" else ""}_reversed_outlined.svg';shutil.copy2(reverse,A/f'logo-{lang}-night.svg');doc=fitz.open(path);pix=doc[0].get_pixmap(matrix=fitz.Matrix(3,3),alpha=True);return Image.frombytes('RGBA',(pix.width,pix.height),pix.samples)
for lang,take in [('en','take2'),('ja','take1')]:
 c=COPY[lang];w=B/f'{take}-{lang}'
 if not (w/'model-after.png').exists():continue
 for name in ['model-after','map','github-alerts','frontmatter']:
  if (w/f'{name}.png').exists():shutil.copy2(w/f'{name}.png',A/f'{name}-{lang}.png')
 im=Image.new('RGB',(1600,1200),'#F9EFCB');d=ImageDraw.Draw(im)
 def text(x,y,s,size=32,color='#385723'):d.text((x,y),s,font=ImageFont.truetype(FONT,size),fill=color)
 d.rectangle((0,0,1600,105),fill='#FFE699');d.line((0,105,1600,105),fill='#385723',width=4);logo=logo_image(lang);logo.thumbnail((300,80));im.paste(logo,(45,10),logo);text(1250,25,'mdEditor',42)
 text(48,140,c['title'],62 if lang=='ja' else 48);text(50,225,c['sub'],32 if lang=='ja' else 28)
 # Crop only the app's content regions; official logo is kept as a separate untouched asset.
 model=Image.open(w/'model-after.png').crop((270,96,950,490));model.thumbnail((1060,610));im.paste(model,((1600-model.width)//2,310))
 text(50,735,'3D / Rotate + Zoom' if lang=='en' else '3Dを、拡大して回す。',40)
 for i,line in enumerate(c['features'][1:]):text(50,815+i*60,line,32 if lang=='ja' else 28)
 text(50,1040,c['guide'],28);text(50,1110,'Source 1.0.12 | Actual app recording | 5garashi.com Design Office',23);text(50,1150,'Design Office Identity 4.19.0 | 2026-10-17',20)
 im.save(OUT/f'social-{lang}.png')
 pdf=fitz.open();p=pdf.new_page(width=595.28,height=841.89);p.insert_image(fitz.Rect(0,0,595.28,446.46),filename=str(OUT/f'social-{lang}.png'))
 writer=fitz.TextWriter(p.rect);font=fitz.Font(fontfile=FONT)
 lines=([c['title'],'まず実演を見る。そのあと、操作を理解する。','冒頭は図・3D・地図・動画のサンプルを順に表示。','3Dは拡大と回転を実操作で収録。','後半で新機能とWord取り込み・PDF保存を紹介。',c['note'],'外部動画・地図は、この収録では読み込みを許可。'] if lang=='ja' else ['See the impact. Then learn the workflow.','A real sample tour opens the video.','Watch actual 3D zoom and rotation, maps and video.','Then see new features, Word import and PDF export.','Recorded source build: 1.0.12; Store version unverified.','Internet media and map loading were allowed for recording.'])
 for i,line in enumerate(lines):writer.append((26,485+i*29),line,font=font,fontsize=17 if i==0 else 11)
 writer.append((26,742),c['guide'],font=font,fontsize=10);writer.append((26,784),'5garashi.com Design Office | Identity 4.19.0 | 2026-10-17',font=font,fontsize=9);writer.write_text(p,color=(56/255,87/255,35/255));p.insert_link({'kind':fitz.LINK_URI,'from':fitz.Rect(20,725,580,754),'uri':c['guide']});pdf.subset_fonts();pdf.save(OUT/f'one-page-{lang}.pdf',garbage=4,deflate=True)
 css=':root{color-scheme:light dark}*{box-sizing:border-box}body{margin:0;background:#F9EFCB;color:#385723;font:600 17px/1.75 "Yu Gothic",Meiryo,sans-serif}header,main,footer{max-width:1100px;margin:auto;padding:28px}header{background:#FFE699;border-bottom:3px solid #385723}header img{width:230px;max-width:70%}h1{font-size:clamp(28px,5vw,48px);line-height:1.4}video,img{max-width:100%;height:auto}a{color:inherit;overflow-wrap:anywhere}a.button{display:inline-flex;min-height:44px;padding:12px 20px;border:2px solid currentColor;margin:8px 8px 8px 0;align-items:center}a:focus-visible{outline:3px solid #B92613;outline-offset:3px}.note{font-size:14px}section{margin:42px 0}@media(prefers-color-scheme:dark){body{background:#1A2212;color:#C9C1A5}header{background:#2B3A20;border-color:#C9C1A5}}'
 other='ja' if lang=='en' else 'en';file='en.html' if lang=='en' else 'index.html';otherfile='index.html' if lang=='en' else 'en.html'
 doc=header(file)+f'<!doctype html><html lang="{lang}"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>mdEditor 1.0.12 — {html.escape(c["title"])}</title><style>{css}</style><header><img src="assets/logo-{lang}.svg" alt="5garashi.com"><nav><a class="button" href="{otherfile}">{"日本語" if lang=="en" else "English"}</a></nav></header><main><h1>{html.escape(c["title"])}</h1><p>{html.escape(c["intro"])}</p><section><video controls playsinline preload="metadata" poster="social-{lang}.png"><source src="mdEditor-showcase-{lang}.mp4" type="video/mp4"><track kind="subtitles" src="mdEditor-showcase-{lang}.vtt" srclang="{lang}" label="{lang}"></video><p class="note">{html.escape(c["note"])}</p><a class="button" href="mdEditor-showcase-{lang}.mp4">MP4</a><a class="button" href="one-page-{lang}.pdf">PDF</a></section><section><h2>{"Explore, then learn" if lang=="en" else "見て驚き、触って理解する"}</h2><img src="assets/model-after-{lang}.png" alt="{html.escape(c["alt"])}"><ul>'+''.join(f'<li>{html.escape(v)}</li>' for v in c['features'])+f'</ul><a class="button" href="{c["guide"]}">{"Current guide" if lang=="en" else "現在公開中の使い方"}</a><a class="button" href="https://apps.microsoft.com/detail/9nww2qrclmhc">Microsoft Store</a></section><section><h2>{"Recording notes" if lang=="en" else "収録について"}</h2><p>{"Internet loading was allowed for this recording. Word import does not preserve colors or layout. Diagrams and equations export to Word as images. No automatic numerical calculation." if lang=="en" else "この収録では外部の画像・動画・詳細地図を許可しています。Wordの色や配置は取り込みません。Wordへの図・数式は画像になります。数値の自動計算はしません。"}</p></section></main><footer><p>Music: Light Steps — 5garashi.com Design Office — CC BY 4.0 (excerpt repeated and faded).</p><p>External sample video: Big Buck Bunny © 2008 Blender Foundation — CC BY 3.0 — <a href="https://peach.blender.org/about/">source</a>. Map data: © OpenStreetMap contributors.</p><p><a href="https://5garashi.github.io/5garashi-identity/">Design Office Identity 4.19.0</a> · 5garashi.com設計事務所 / 5garashi.com Design Office · 2026-10-17 JST</p></footer></html>'
 doc=doc.replace(f'<img src="assets/logo-{lang}.svg" alt="5garashi.com">',f'<picture><source media="(prefers-color-scheme:dark)" srcset="assets/logo-{lang}-night.svg"><img src="assets/logo-{lang}.svg" alt="5garashi.com"></picture>')
 (OUT/file).write_text(doc,encoding='utf-8')
 short=(f'mdEditor 1.0.12: 図も、3Dも、動画も。サンプルを上から下へ、3Dを拡大・回転。そのあと新機能と編集・保存を実演します。\n\n{c["guide"]}\n#mdEditor #Markdown #Windows' if lang=='ja' else f'mdEditor 1.0.12: one Markdown, a visual world. Tour the sample, zoom and rotate 3D, then explore new features and real editing and saving.\n\n{c["guide"]}\n#Markdown #Windows')
 body=f'# 投稿・記事原稿 / Channel copy\n\n公開時に確認済みの新動画URLを追加してください。現在はレビュー用原稿です。 / Add verified new video URLs at publication; this is draft copy.\n\n## X\n\n```text\n{short}\n```\n\n## LinkedIn / Zenn introduction\n\n{c["intro"]}\n\n'+ '\n\n'.join(c['features'])+f'\n\n{c["note"]}\n\n画像の代替テキスト / Alt text: {c["alt"]}\n\nデザイン参照 / Design reference: https://5garashi.github.io/5garashi-identity/ (4.19.0).\n\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: 2026-10-17 JST\n'
 (OUT/f'posts-{lang}.md').write_text(header(f'posts-{lang}.md')+body,encoding='utf-8')
print('Built bilingual materials in',OUT,flush=True)
