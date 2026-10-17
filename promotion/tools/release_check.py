"""
File: promotion/tools/release_check.py
Description: Check packaged files and portable playback without external assets
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import base64,functools,hashlib,http.server,json,subprocess,sys,threading,time,urllib.request,zipfile
from html.parser import HTMLParser
from pathlib import Path
from build import ROOT,VERSION
from serve import RangeHandler
sys.path.insert(0,r'C:\miFfplay\appAutoTester')
from autotester.cdp import CdpSession,wait_for_page
from autotester.webview_app import free_port
WORK=ROOT.parent.parent/'work'
OUT=ROOT.parent.parent/'deliverables'
staging=WORK/'release-preview';staging.mkdir(exist_ok=True)
with zipfile.ZipFile(OUT/f'mdEditor-promotion-{VERSION}.zip') as z:
    for name in z.namelist():
        assert (staging/name).resolve().is_relative_to(staging.resolve())
    z.extractall(staging)
manifest=json.loads((staging/'manifest.json').read_text())
for item in manifest['files']:
    file=staging/item['path']
    assert hashlib.sha256(file.read_bytes()).hexdigest()==item['sha256']
assert not (staging/'assets/mdEditor-intro-en.mp4').exists()
assert not (staging/'assets/video-poster.png').exists()

class Links(HTMLParser):
    def __init__(self):super().__init__();self.links=[]
    def handle_starttag(self,tag,attrs):
        for key,value in attrs:
            if key in ('href','src','srcset','poster') and value and not value.startswith(('https:','http:','data:','#')):self.links.append(value)

server=http.server.ThreadingHTTPServer(('127.0.0.1',0),functools.partial(RangeHandler,directory=str(staging)))
threading.Thread(target=server.serve_forever,daemon=True).start()
origin=f'http://127.0.0.1:{server.server_port}'
summary=[]
try:
    for lang,file in [('ja','index.html'),('en','en.html')]:
        parser=Links();parser.feed((staging/file).read_text(encoding='utf-8'))
        for path in parser.links:
            with urllib.request.urlopen(origin+'/'+path) as r:assert r.status==200 and r.read()==(staging/path).read_bytes()
        req=urllib.request.Request(origin+f'/assets/mdEditor-demo-{lang}.mp4',headers={'Range':'bytes=0-99'})
        with urllib.request.urlopen(req) as r:assert r.status==206 and len(r.read())==100
        summary.append({'page':file,'local_links':len(parser.links),'HTTP':'200','MP4_ranges':'206'})
    port=free_port();profile=WORK/'release-browser'
    proc=subprocess.Popen([r'C:\Program Files\Google\Chrome\Application\chrome.exe','--headless','--disable-gpu','--no-first-run',f'--user-data-dir={profile}',f'--remote-debugging-port={port}','--window-size=1280,900','about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
    cdp=None
    try:
        cdp=CdpSession(wait_for_page(port,timeout=20)['webSocketDebuggerUrl']);cdp.send('Page.enable');cdp.send('Runtime.enable')
        version=cdp.send('Browser.getVersion')
        def press(key,code,vk,text=''):
            base={'key':key,'code':code,'windowsVirtualKeyCode':vk,'nativeVirtualKeyCode':vk}
            cdp.send('Input.dispatchKeyEvent',{'type':'keyDown' if text else 'rawKeyDown',**base,**({'text':text,'unmodifiedText':text} if text else {})})
            cdp.send('Input.dispatchKeyEvent',{'type':'keyUp',**base})
        for lang,file in [('ja','index.html'),('en','en.html')]:
            portable=WORK/f'isolated-release-{lang}.html';portable.write_bytes((ROOT/'review'/file).read_bytes())
            cdp.send('Page.navigate',{'url':portable.as_uri()})
            deadline=time.monotonic()+15
            while time.monotonic()<deadline:
                if cdp.evaluate("document.querySelector('video')?.readyState>=1 && !document.querySelector('.player-controls')?.hidden"):break
                time.sleep(.1)
            cdp.evaluate("document.querySelector('[data-play]').focus()")
            press('Enter','Enter',13,'\r');time.sleep(.4)
            assert cdp.evaluate("!document.querySelector('video').paused")
            cdp.evaluate("document.querySelector('[data-stop]').focus()")
            press('Enter','Enter',13,'\r');time.sleep(.2)
            assert cdp.evaluate("document.querySelector('video').paused&&document.querySelector('video').currentTime===0")
            cdp.evaluate("document.querySelector('input[type=range]').focus()")
            press('ArrowRight','ArrowRight',39);time.sleep(.3)
            assert cdp.evaluate("document.querySelector('video').currentTime>0")
            cdp.evaluate("document.querySelector('[data-fullscreen]').focus()")
            press('Enter','Enter',13,'\r');time.sleep(.3)
            assert cdp.evaluate("document.fullscreenElement?.classList.contains('video-stage')")
            press('Enter','Enter',13,'\r');time.sleep(.3)
            assert cdp.evaluate("!document.fullscreenElement")
            downloads=cdp.evaluate("[...document.querySelectorAll('.sample-download')].map(a=>({file:a.download,uri:a.href}))")
            for d in downloads:
                assert base64.b64decode(d['uri'].split(',',1)[1])==(ROOT/'samples'/d['file']).read_bytes()
            summary.append({'portable':lang,'keyboard':'play, stop, seek, full screen and exit OK','embedded_samples':'all three match source bytes'})
        (ROOT/'validation/release-results.json').write_text(json.dumps({'date':'2026-10-13 JST','browser':version,'packaged_files':len(manifest['files']),'results':summary},ensure_ascii=False,indent=2),encoding='utf-8')
    finally:
        if cdp:
            try:cdp.send('Browser.close')
            except Exception:pass
            cdp.close()
        try:proc.wait(timeout=8)
        except subprocess.TimeoutExpired:proc.terminate()
finally:server.shutdown();server.server_close()
print('Release ZIP links, checksums, byte ranges and isolated review playback: OK')
