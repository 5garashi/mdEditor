"""
ファイル名 / File: promotion/tools/render_multichannel.py
概要 / Description: 投稿画像・PDFを実ブラウザーで出力しQRを検証 / Render media and verify QR codes
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import argparse,base64,json,subprocess,sys,time
from pathlib import Path
import cv2,fitz
from PIL import Image
from build import ROOT,VERSION
WORK=ROOT.parent.parent/'work'
MEDIA=ROOT/'multichannel'
parser=argparse.ArgumentParser();parser.add_argument('--topic',choices=['read','math','window'])
args=parser.parse_args()
sys.path.insert(0,r'C:\miFfplay\appAutoTester')
from autotester.cdp import CdpSession,wait_for_page
from autotester.webview_app import free_port
port=free_port()
proc=subprocess.Popen([r'C:\Program Files\Google\Chrome\Application\chrome.exe','--headless','--disable-gpu','--no-first-run',f'--user-data-dir={WORK/"multichannel-browser"}',f'--remote-debugging-port={port}','about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
cdp=None
results={'kit_version':VERSION,'date':'2026-10-13 JST','social':[],'pdf':[],'qr':[]}
if args.topic:
    results=json.loads((ROOT/'validation/multichannel-results.json').read_text(encoding='utf-8'))
    results['social']=[x for x in results['social'] if not x['file'].startswith(args.topic+'-')]
try:
    cdp=CdpSession(wait_for_page(port,timeout=20)['webSocketDebuggerUrl'])
    cdp.send('Page.enable');cdp.send('Runtime.enable')
    results['browser']=cdp.send('Browser.getVersion')['product']
    def load(path,width,height,print_media=False):
        cdp.send('Emulation.setDeviceMetricsOverride',{'width':width,'height':height,'deviceScaleFactor':1,'mobile':False})
        cdp.send('Emulation.setEmulatedMedia',{'media':'print' if print_media else 'screen'})
        cdp.send('Page.navigate',{'url':path.resolve().as_uri()})
        deadline=time.monotonic()+15
        while time.monotonic()<deadline:
            if cdp.evaluate("document.readyState==='complete' && [...document.images].every(i=>i.complete&&i.naturalWidth>0)"):break
            time.sleep(.1)
        else:raise RuntimeError('Images did not load: '+str(path))
        cdp.evaluate('document.fonts.ready')
        time.sleep(.2)
    for path in sorted((MEDIA/'social').glob('*.html')):
        if args.topic and not path.name.startswith(args.topic+'-'):continue
        square='square' in path.name
        width,height=(1080,1080) if square else (1600,900)
        load(path,width,height)
        metrics=cdp.evaluate("""(() => ({width:document.documentElement.scrollWidth,height:document.documentElement.scrollHeight,overflow:[...document.querySelectorAll('h1,p,img,figure,footer')].filter(e=>{const r=e.getBoundingClientRect();return r.right>innerWidth+.5||r.bottom>innerHeight+.5||r.left<0||r.top<0}).map(e=>({tag:e.tagName,text:e.textContent.slice(0,60),rect:e.getBoundingClientRect().toJSON()}))}))()""")
        assert not metrics['overflow'],(path.name,metrics)
        png=path.with_suffix('.png')
        png.write_bytes(base64.b64decode(cdp.send('Page.captureScreenshot',{'format':'png','clip':{'x':0,'y':0,'width':width,'height':height,'scale':1}})['data']))
        assert Image.open(png).size==(width,height)
        results['social'].append({'file':png.name,'width':width,'height':height,'clipping':'none'})
        print('Image OK '+png.name,flush=True)
    detector=cv2.QRCodeDetector()
    targets=json.loads((MEDIA/'qr/targets.json').read_text())
    results['qr']=[]
    for item in targets:
        image=cv2.imread(str(MEDIA/'qr'/f'{item["name"]}.png'))
        actual,points,_=detector.detectAndDecode(image)
        assert actual==item['url'],(item,actual)
        results['qr'].append({**item,'decoded':actual})
    for lang in ['ja','en']:
        if args.topic:continue
        path=MEDIA/'print'/f'one-page-{lang}.html'
        load(path,794,1123,True)
        pdf=path.with_suffix('.pdf')
        result=cdp.send('Page.printToPDF',{'printBackground':True,'preferCSSPageSize':True,'displayHeaderFooter':False,'generateTaggedPDF':True,'generateDocumentOutline':True})
        pdf.write_bytes(base64.b64decode(result['data']))
        doc=fitz.open(pdf)
        assert len(doc)==1,(pdf.name,len(doc))
        text=doc[0].get_text()
        assert 'mdEditor' in text and '5garashi.com' in text and VERSION in text
        doc[0].get_pixmap(matrix=fitz.Matrix(2,2)).save(str(path.with_suffix('.png')))
        raster=cv2.imread(str(path.with_suffix('.png')))
        ok,decoded,points,_=detector.detectAndDecodeMulti(raster)
        assert ok and set(decoded)=={x['url'] for x in targets},(pdf.name,decoded)
        # Confirm white PDF margins at all four corners.
        assert all((raster[y,x]==255).all() for y,x in [(0,0),(0,raster.shape[1]-1),(raster.shape[0]-1,0),(raster.shape[0]-1,raster.shape[1]-1)])
        links=[l.get('uri') for l in doc[0].get_links() if l.get('uri')]
        assert {x['url'] for x in targets}.issubset(set(links)),links
        tagged=doc.xref_get_key(doc.pdf_catalog(),'MarkInfo')
        results['pdf'].append({'file':pdf.name,'pages':1,'size_points':[round(doc[0].rect.width,2),round(doc[0].rect.height,2)],'decoded_qr':list(decoded),'clickable_links':links,'white_margins':True,'tagged_pdf':tagged[1],'text_characters':len(text)})
        print('PDF and both QR codes OK '+pdf.name,flush=True)
    (ROOT/'validation/multichannel-results.json').write_text(json.dumps(results,ensure_ascii=False,indent=2),encoding='utf-8')
finally:
    if cdp:
        try:cdp.send('Browser.close')
        except Exception:pass
        cdp.close()
    try:proc.wait(timeout=8)
    except subprocess.TimeoutExpired:proc.terminate()
