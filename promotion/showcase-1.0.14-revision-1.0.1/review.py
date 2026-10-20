"""File: review.py
Description: Extract video transition frames for visual review
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,subprocess,os
from PIL import Image,ImageDraw
B=Path(__file__).resolve().parent;O=B.parents[1]/'deliverables/mdEditor-showcase-1.0.14-revision-1.0.0'
if os.environ.get('PROMO_VIDEO_REVISION'):O=B.parents[1]/('deliverables/mdEditor-showcase-1.0.14-revision-'+os.environ['PROMO_VIDEO_REVISION'])
for lang in ['ja','en']:
 d=json.loads((O/f'mdEditor-showcase-{lang}-validation.json').read_text(encoding='utf-8'));es=d['events'];times={e['key']:e['seconds'] for e in es}
 points=[('opening',2),('tour',times['tour']+5),('model',times['3d']+2),('detach-open',times['detach']+.1),('detached',times['detach']+3),('detach-close',times['viewer']-.1),('viewer',times['viewer']+.3),('drop',times['images']+2),('image-dialog',times['image-adjust']+2),('centered',times['share']-.5),('dark',times['accessibility']+4),('ending',times['outro']+1)]
 sheet=Image.new('RGB',(1200,1020),'#F9EFCB');draw=ImageDraw.Draw(sheet)
 for i,(label,t) in enumerate(points):
  f=B/f'{lang}-review-{i}.png';subprocess.run(['ffmpeg','-v','error','-y','-ss',str(t),'-i',str(O/f'mdEditor-showcase-{lang}.mp4'),'-frames:v','1','-vf','scale=400:225',str(f)],check=True)
  x=i%3*400;y=i//3*255;sheet.paste(Image.open(f),(x,y));draw.text((x+5,y+229),f'{label} {t:.1f}s',fill='#385723')
 sheet.save(O/f'transition-review-{lang}.jpg',quality=92)
 print(lang,points)
