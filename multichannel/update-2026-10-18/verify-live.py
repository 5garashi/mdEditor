from pathlib import Path
import urllib.request,json,hashlib,time
B=Path(__file__).resolve().parent;P=B/'repo/promotion';BASE='https://5garashi.github.io/mdEditor/';results={}
def get(path):
 return urllib.request.urlopen(BASE+path,timeout=90).read()
for page in ['','en.html','resources.html','resources-en.html','case-study.html','case-study-en.html','office-card-ja.html','office-card-en.html']:
 text=get(page).decode('utf-8');assert '1.0.12' in text or page.startswith('case-study'),page
 if page in ['', 'en.html']:assert 'mdEditor-demo-1.0.12-' in text and 'hero-3d-1.0.12-' in text and '20' in text
 results[page or 'index.html']={'http':200,'source_1_0_12': '1.0.12' in text}
for lang in ['ja','en']:
 for rel in [f'assets/mdEditor-demo-1.0.12-{lang}.mp4',f'multichannel/update-2026-10-18/one-page-{lang}.pdf',f'multichannel/update-2026-10-18/features-{lang}.pdf',f'multichannel/update-2026-10-18/social-{lang}.png']:
  actual=get(rel);expected=(P/rel).read_bytes();assert hashlib.sha256(actual).digest()==hashlib.sha256(expected).digest(),rel
  results[rel]={'http':200,'sha256':hashlib.sha256(actual).hexdigest(),'matches_source':True}
for lang,id in [('ja','CAi5q1c0cuo'),('en','valiuj76iG4')]:
 url='https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v='+id+'&format=json'
 data=json.load(urllib.request.urlopen(url,timeout=45));assert '1.0.12' in data['title'];results['youtube-'+lang]={'public_oembed':True,'id':id,'title':data['title']}
try:
 text=urllib.request.urlopen('https://www.5garashi.com/',timeout=45).read().decode('utf-8');results['office_site']={'http':200,'links_to_mdEditor':'5garashi.github.io/mdEditor' in text}
except Exception as e:results['office_site']={'unverified':type(e).__name__}
(B/'live-validation.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8');print('PASS live pages, eight distributed assets and public YouTube metadata')
