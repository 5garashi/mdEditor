"""File: prepare.py
Description: Prepare consistent visual-first public materials and YouTube release
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.8.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,shutil,subprocess,re
from PIL import Image,ImageDraw,ImageFont
import fitz
B=Path(__file__).resolve().parent;ROOT=B.parents[1];P=B/'repo/promotion';KIT=P/'multichannel/update-2026-10-18';KIT.mkdir(exist_ok=True)
S=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.1'
features=json.loads((S/'features.json').read_text(encoding='utf-8'))
(P/'tools/showcase-features.json').write_text(json.dumps(features,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
for lang in ['ja','en']:
 for name in [f'social-{lang}.png',f'one-page-{lang}.pdf',f'posts-{lang}.md']:shutil.copy2(S/name,KIT/name)
 shutil.copy2(S/f'mdEditor-showcase-{lang}.mp4',P/f'assets/mdEditor-demo-1.0.12-{lang}.mp4')
 shutil.copy2(S/f'mdEditor-showcase-{lang}.vtt',P/f'assets/demo-1.0.12-{lang}.vtt')
 for stem in ['1-main','7-3d-model']:
  src=Path(f'C:/miFfplay/mdEditor/docs/images/{stem}.{lang}.png')
  if src.exists():shutil.copy2(src,P/f'assets/{"app" if stem=="1-main" else "model"}-{lang}.png')
 hero=P/f'assets/hero-3d-1.0.12-{lang}.png'
 subprocess.run(['ffmpeg','-v','error','-y','-ss','3','-i',str(S/f'mdEditor-showcase-{lang}.mp4'),'-frames:v','1','-vf','crop=1264:642:8:0',str(hero)],check=True)
 shutil.copy2(hero,P/f'assets/video-poster-1.0.12-{lang}.png')
 im=Image.new('RGB',(1280,720),'#F9EFCB');d=ImageDraw.Draw(im);font=ImageFont.truetype('C:/Windows/Fonts/meiryob.ttc',36)
 d.text((30,10),'図も、3Dも、動画も。' if lang=='ja' else 'Diagrams. 3D. Video.',font=font,fill='#385723')
 shot=Image.open(hero);shot.thumbnail((1160,575));im.paste(shot,((1280-shot.width)//2,70));d.text((30,665),'mdEditor 1.0.12 · Windows',font=ImageFont.truetype('C:/Windows/Fonts/meiryob.ttc',27),fill='#385723');im.save(P/f'assets/youtube-1.0.12-{lang}.png')
 # A text-based two-page companion covers every requested feature.
 pdf=fitz.open();font=fitz.Font(fontfile='C:/Windows/Fonts/meiryob.ttc')
 for page_num in range(2):
  page=pdf.new_page(width=595.28,height=841.89);page.draw_rect(page.rect,color=None,fill=(249/255,239/255,203/255));tw=fitz.TextWriter(page.rect)
  tw.append((30,48),'mdEditor — '+('主な20機能' if lang=='ja' else '20 main features'),font=font,fontsize=21)
  tw.append((30,76),'Source 1.0.12 | 2026-10-18 | '+str(page_num+1)+'/2',font=font,fontsize=11)
  for i,f in enumerate(features[page_num*10:(page_num+1)*10]):
   text=str(page_num*10+i+1)+'. '+f[lang];lines=[];line=''
   for char in text:
    if font.text_length(line+char,fontsize=12)>530:lines.append(line);line=char
    else:line+=char
   lines.append(line)
   for j,line in enumerate(lines):tw.append((30,125+i*61+j*19),line,font=font,fontsize=12)
  url='https://5garashi.github.io/mdEditor/'+('en.html' if lang=='en' else '')
  tw.append((30,766),url,font=font,fontsize=10);tw.append((30,795),'5garashi.com Design Office | Identity 4.20.0',font=font,fontsize=10);tw.write_text(page,color=(56/255,87/255,35/255));page.insert_link({'kind':fitz.LINK_URI,'from':fitz.Rect(27,750,560,779),'uri':url})
 pdf.save(KIT/f'features-{lang}.pdf')
 # Publish only the bundled test document and its directly referenced local assets.
 name='diagram-test'+('.en' if lang=='en' else '')+'.md';shutil.copy2(Path('C:/miFfplay/mdEditor/samples')/name,P/'samples'/name)
AUTO=P/'integration/youtube/automation'
c=json.loads((AUTO/'release.json').read_text(encoding='utf-8'));c.update(operation='sync',request_id='2026-10-18-source-1.0.12-showcase-3.0.0-sync')
for v in c['videos']:
 lang=v['language'];v.update(revision='3.0.0',media=f'promotion/assets/mdEditor-demo-1.0.12-{lang}.mp4',thumbnail=f'promotion/assets/youtube-1.0.12-{lang}.png',captions=f'promotion/assets/demo-1.0.12-{lang}.vtt',title='mdEditor 1.0.12 — '+('図・3D・地図・動画から、編集と共有まで' if lang=='ja' else 'Diagrams, 3D, Maps & Video — Edit and Share'),target_privacy='public')
 intro=('回転する3Dモデルから、mdEditorの表現と使い方を紹介します。diagram-testを上から下へ見たあと、主な20機能を実画面と字幕で紹介。' if lang=='ja' else 'Start with a rotating 3D model. Tour diagram-test, then explore twenty main features through the actual app and captions.')
 body=intro+'\n\n'+ '\n'.join('- '+f[lang] for f in features)+'\n\n'+('使い方・サンプル・PDF' if lang=='ja' else 'Guide, samples and PDFs')+'\nhttps://5garashi.github.io/mdEditor/'+('en.html' if lang=='en' else '')+'\n\n'
 body+=('収録は更新済みソース版1.0.12（8ab7cef）のビルド。Store配信版番号は未確認です。インターネット上の画像・動画・地図は許可して収録。別窓の紹介は、背景を除いた安定した実録画1秒を繰り返して編集しています。紹介機能すべての網羅的な再試験ではありません。Wordの色・配置、Notebookの実行結果は再現しません。\n' if lang=='ja' else 'Recorded from updated source build 1.0.12 (8ab7cef). Store build number unverified. Internet media and maps were allowed. The detached-preview segment repeats one stable second cropped to the app windows. This is a feature introduction, not an exhaustive retest. Word import does not preserve layout; notebook export omits execution output.\n')
 body+='\nMusic: Light Steps v1 — 5garashi.com Design Office; composition/synthesis: Codex. CC BY 4.0. https://creativecommons.org/licenses/by/4.0/ Excerpt repeated and faded.\nSample video: Big Buck Bunny © 2008 Blender Foundation, CC BY 3.0. https://peach.blender.org/about/ Short silent screen capture.\nMap: © OpenStreetMap contributors https://www.openstreetmap.org/copyright\nApp and samples: MIT. https://github.com/5garashi/mdEditor\n'
 assert len(body)<4900,len(body)
 (AUTO/f'description-{lang}.txt').write_text(body,encoding='utf-8')
 caption=intro+'\n\nhttps://5garashi.github.io/mdEditor/'+('en.html' if lang=='en' else '')+'\n\nMusic: Light Steps — 5garashi.com Design Office / Codex, CC BY 4.0.\nBig Buck Bunny © 2008 Blender Foundation, CC BY 3.0, bigbuckbunny.org.\n© OpenStreetMap contributors.\n#mdEditor #Markdown #Windows'
 (B/f'tiktok-{lang}.txt').write_text(caption,encoding='utf-8')
(AUTO/'release.json').write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(P/'VERSION').write_text('1.8.0\n',encoding='utf-8');(P/'integration/VERSION').write_text('1.2.0\n',encoding='utf-8')
f=P/'tools/build.py';s=f.read_text(encoding='utf-8').replace("DATE='2026-10-17'","DATE='2026-10-18'");s=s.replace('content=page(en)','from showcase import apply\n        content=apply(page(en), "en" if en else "ja")');f.write_text(s,encoding='utf-8')
f=P/'THIRD-PARTY-NOTICES.md';s=f.read_text(encoding='utf-8');s+='\n## 2026-10-18 showcase\n\nBig Buck Bunny © 2008 Blender Foundation / www.bigbuckbunny.org, CC BY 3.0. https://peach.blender.org/about/ — short silent screen capture inside the mdEditor sample. Map: © OpenStreetMap contributors, https://www.openstreetmap.org/copyright; attribution retained.\n';f.write_text(s,encoding='utf-8')
print('Prepared bilingual feature materials, images, samples and YouTube sync request')
