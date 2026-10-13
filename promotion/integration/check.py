"""
File: promotion/integration/check.py
Description: Check local references and new pages in Chrome
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import base64,json,re,subprocess,sys,time
from pathlib import Path
from urllib.parse import urlsplit,unquote
HERE=Path(__file__).resolve().parent
OUT=HERE.parents[2]/'work/github-pages-preview'
sys.path.insert(0,r'C:\miFfplay\appAutoTester')
from autotester.cdp import CdpSession,wait_for_page
from autotester.webview_app import free_port
broken=[]
for path in OUT.glob('*.html'):
    for url in re.findall(r'(?:src|href)="([^"]+)"',path.read_text(encoding='utf-8')):
        parsed=urlsplit(url)
        if parsed.scheme or not parsed.path: continue
        if not (path.parent/unquote(parsed.path)).exists():broken.append([path.name,url])
assert not broken,broken
port=free_port()
proc=subprocess.Popen([r'C:\Program Files\Google\Chrome\Application\chrome.exe','--headless','--disable-gpu','--no-first-run',f'--user-data-dir={OUT.parent/"integration-browser"}',f'--remote-debugging-port={port}','about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
cdp=None
results={'integration_version':(HERE/'VERSION').read_text().strip(),'date':'2026-10-13 JST','local_references':'pass','cases':[],'unverified':['Google Sites embed','live GitHub Pages','Narrator','actual Windows forced colors']}
try:
    cdp=CdpSession(wait_for_page(port,timeout=20)['webSocketDebuggerUrl'])
    results['browser']=cdp.send('Browser.getVersion')['product']
    pages=[OUT/x for x in ['resources.html','resources-en.html','case-study.html','case-study-en.html']]+[HERE/'office-card-ja.html',HERE/'office-card-en.html']
    for page in pages:
        for theme in ['light','dark']:
            for width,font in [(1280,16),(320,16),(640,16),(320,32)]:
                cdp.send('Emulation.setDeviceMetricsOverride',{'width':width,'height':900,'deviceScaleFactor':1,'mobile':False})
                cdp.send('Emulation.setEmulatedMedia',{'features':[{'name':'prefers-color-scheme','value':theme}]})
                cdp.send('Page.navigate',{'url':page.as_uri()})
                for attempt in range(100):
                    if cdp.evaluate("document.readyState==='complete' && [...document.images].every(i=>i.complete&&i.naturalWidth>0)"): break
                    time.sleep(.1)
                cdp.evaluate(f"document.documentElement.style.fontSize='{font}px'")
                time.sleep(.15)
                data=cdp.evaluate("({width:innerWidth,scrollWidth:document.documentElement.scrollWidth,broken:[...document.images].filter(i=>!i.complete||!i.naturalWidth).length,small:[...document.querySelectorAll('a')].filter(a=>{let r=a.getBoundingClientRect();return r.width>0&&(r.width<44||r.height<44)}).length})")
                assert data['scrollWidth']<=width and not data['broken'] and not data['small'],(page,theme,width,font,data)
                results['cases'].append({'page':page.name,'theme':theme,'width':width,'text_px':font,**data})
                if theme=='light' and width==1280:
                    (HERE/(page.stem+'-preview.png')).write_bytes(base64.b64decode(cdp.send('Page.captureScreenshot',{'format':'png','captureBeyondViewport':True})['data']))
    (HERE/'validation.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
    print('PASS: references and',len(results['cases']),'browser layout cases. 640px is a reflow approximation; actual browser zoom is not tested here.')
finally:
    if cdp:cdp.close()
    proc.terminate()
    proc.wait(timeout=15)
