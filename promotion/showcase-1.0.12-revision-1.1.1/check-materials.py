"""File: check-materials.py
Description: Check the bilingual showcase drafts and one-page PDFs
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import sys,subprocess,time,json,base64
import fitz
from PIL import Image
B=Path(__file__).resolve().parent;OUT=B.parents[1]/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.0'
sys.path.insert(0,r'C:\miFfplay\appAutoTester')
from autotester.cdp import CdpSession,wait_for_page
from autotester.webview_app import free_port
port=free_port();p=subprocess.Popen([r'C:\Program Files\Google\Chrome\Application\chrome.exe','--headless','--disable-gpu',f'--user-data-dir={B/"material-browser"}',f'--remote-debugging-port={port}','about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL);c=None;results={'date':'2026-10-18 JST','pages':[],'pdf':{}}
try:
 c=CdpSession(wait_for_page(port,timeout=20)['webSocketDebuggerUrl'])
 for lang,file in [('ja','index.html'),('en','en.html')]:
  for theme in ['light','dark']:
   for width in [320,1280]:
    url=(OUT/file).as_uri();c.send('Emulation.setDeviceMetricsOverride',{'width':width,'height':1000,'deviceScaleFactor':1,'mobile':False});c.send('Emulation.setEmulatedMedia',{'features':[{'name':'prefers-color-scheme','value':theme}]});c.send('Page.navigate',{'url':url})
    for _ in range(120):
     if c.evaluate(f'location.href==={json.dumps(url)} && document.readyState==="complete" && [...document.images].every(i=>i.complete&&i.naturalWidth>0)'):break
     time.sleep(.1)
    else:raise RuntimeError('not loaded '+file)
    data=c.evaluate('({overflow:document.documentElement.scrollWidth>innerWidth,images:[...document.images].every(i=>i.naturalWidth>0),controls:document.querySelector("video").controls,autoplay:document.querySelector("video").autoplay})');assert not data['overflow'] and data['images'] and data['controls'] and not data['autoplay'],data
    results['pages'].append({'language':lang,'theme':theme,'width':width,**data})
    if theme=='light' and width==1280:(B/f'page-{lang}.png').write_bytes(base64.b64decode(c.send('Page.captureScreenshot',{'format':'png'})['data']))
  d=fitz.open(OUT/f'one-page-{lang}.pdf');text=d[0].get_text();assert len(d)==1 and len(text)>150 and d[0].get_links();assert Image.open(OUT/f'social-{lang}.png').size==(1600,1200)
  results['pdf'][lang]={'pages':len(d),'textCharacters':len(text),'links':len(d[0].get_links())}
 results['unverified']=['screen reader','actual OS contrast themes','SNS recompression','public hosting of new drafts']
 (OUT/'materials-validation.json').write_text(json.dumps(results,indent=2),encoding='utf-8');print('PASS',len(results['pages']),'page cases; two PDFs and two social cards')
finally:
 if c:c.close()
 p.terminate();p.wait(timeout=15)
