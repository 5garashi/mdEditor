"""
ファイル名 / File: promotion/tools/browser_check.py
概要 / Description: Check bilingual pages in real Chrome over HTTP and as portable files
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import base64, functools, http.server, json, shutil, socket, subprocess, sys, threading, time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
WORK=ROOT.parent.parent/'work'
sys.path.insert(0,r'C:\miFfplay\appAutoTester')
from autotester.cdp import CdpSession,wait_for_page
from autotester.webview_app import free_port
from serve import RangeHandler
CHROME=Path(r'C:\Program Files\Google\Chrome\Application\chrome.exe')
RESULTS=ROOT/'validation'; RESULTS.mkdir(exist_ok=True)

class QuietHandler(RangeHandler):
    pass

server=http.server.ThreadingHTTPServer(('127.0.0.1',0),functools.partial(QuietHandler,directory=str(ROOT.parent)))
threading.Thread(target=server.serve_forever,daemon=True).start()
origin=f'http://127.0.0.1:{server.server_port}'
results=[]

def keyboard(cdp,key='Tab',code='Tab',vk=9):
    base={'key':key,'code':code,'windowsVirtualKeyCode':vk,'nativeVirtualKeyCode':vk}
    text='\r' if key=='Enter' else ' ' if code=='Space' else ''
    cdp.send('Input.dispatchKeyEvent',{'type':'keyDown' if text else 'rawKeyDown',**base,**({'text':text,'unmodifiedText':text} if text else {})})
    cdp.send('Input.dispatchKeyEvent',{'type':'keyUp',**base})

def screenshot(cdp,name,full=False):
    params={'format':'png','captureBeyondViewport':full}
    if full:
        size=cdp.send('Page.getLayoutMetrics')['cssContentSize']
        params['clip']={'x':0,'y':0,'width':size['width'],'height':size['height'],'scale':1}
    (RESULTS/name).write_bytes(base64.b64decode(cdp.send('Page.captureScreenshot',params)['data']))

try:
    for mode in ['normal','zoom200','text200']:
        profile=WORK/f'chrome-{mode}'; (profile/'Default').mkdir(exist_ok=True,parents=True)
        prefs={}
        # Chromium ChromeZoomLevelPrefs: the default storage partition key is "x".
        if mode=='zoom200': prefs={'partition':{'default_zoom_level':{'x':3.8017840169239308}}}
        if mode=='text200': prefs={'webkit':{'webprefs':{'default_font_size':32,'default_fixed_font_size':26}}}
        (profile/'Default/Preferences').write_text(json.dumps(prefs),encoding='utf-8')
        port=free_port()
        proc=subprocess.Popen([str(CHROME),'--headless','--disable-gpu','--no-first-run','--no-default-browser-check',f'--user-data-dir={profile}',f'--remote-debugging-port={port}','--window-size=1280,900','about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
        cdp=None
        try:
            cdp=CdpSession(wait_for_page(port,timeout=25)['webSocketDebuggerUrl'])
            cdp.send('Page.enable');cdp.send('Runtime.enable');cdp.send('Network.enable')
            failures=[]
            def event(method,params):
                if method=='Network.responseReceived' and params['response']['status']>=400:failures.append(params['response'])
                if method=='Runtime.exceptionThrown':failures.append(params)
            cdp.on_event(event)
            for lang,file in [('ja','index.html'),('en','en.html')]:
                for palette in ['light','dark']:
                    for width in ([1280,320] if mode!='zoom200' else [1280]):
                        name=f'{lang}-{palette}-{mode}-{width}'
                        cdp.send('Emulation.setDeviceMetricsOverride',{'width':width,'height':900,'deviceScaleFactor':1,'mobile':False})
                        cdp.send('Emulation.setEmulatedMedia',{'features':[{'name':'prefers-color-scheme','value':palette}]})
                        cdp.send('Page.navigate',{'url':origin+'/promotion/'+file})
                        deadline=time.monotonic()+15
                        while time.monotonic()<deadline:
                            # Wait for the requested navigation, not the previous page.
                            if cdp.evaluate(f"location.pathname==='/promotion/{file}' && document.readyState==='complete' && document.querySelector('h1') && [...document.images].every(i=>i.complete && i.naturalWidth>0)"):break
                            time.sleep(.1)
                        time.sleep(.3)
                        metrics=cdp.evaluate("""(() => ({width:innerWidth,scrollWidth:document.documentElement.scrollWidth,font:getComputedStyle(document.body).fontSize,logo:document.querySelector('.brand img').currentSrc,background:getComputedStyle(document.body).backgroundColor,brokenImages:[...document.images].filter(i=>!i.naturalWidth).map(i=>i.src),smallTargets:[...document.querySelectorAll('a,summary,button,input')].filter(e=>{const r=e.getBoundingClientRect();return r.width>0&&r.height>0&&(r.width<43.9||r.height<43.9)}).map(e=>({text:e.textContent,width:e.getBoundingClientRect().width,height:e.getBoundingClientRect().height})),overflow:[...document.querySelectorAll('body *')].filter(e=>{const r=e.getBoundingClientRect();return r.width>0&&r.right>innerWidth+1}).map(e=>({tag:e.tagName,cls:e.className,right:e.getBoundingClientRect().right})).slice(0,8)}))()""")
                        assert metrics['scrollWidth']<=metrics['width']+1,(name,metrics)
                        assert not metrics['brokenImages'] and not metrics['smallTargets'],(name,metrics)
                        assert ('reversed' in metrics['logo'])==(palette=='dark'),(name,metrics)
                        if mode=='zoom200': assert metrics['width']==640,(name,metrics)
                        if mode=='text200': assert metrics['font']=='32px',(name,metrics)
                        if mode=='normal':assert metrics['font']=='16px',(name,metrics)
                        screenshot(cdp,name+'.png')
                        if mode=='normal' and width==1280:screenshot(cdp,name+'-full.png',True)
                        keyboard(cdp)
                        assert cdp.evaluate("document.activeElement.classList.contains('skip')"),name
                        focus=cdp.evaluate("getComputedStyle(document.activeElement).outlineWidth")
                        assert focus=='3px',(name,focus)
                        keyboard(cdp,'Enter','Enter',13)
                        assert cdp.evaluate("document.activeElement.id==='main'"),name
                        # Visit page links, player and disclosure via keyboard; activate actual controls.
                        visited=[]
                        for _ in range(34):
                            keyboard(cdp)
                            info=cdp.evaluate("({tag:document.activeElement.tagName,text:document.activeElement.textContent.slice(0,60),id:document.activeElement.id})")
                            visited.append(info)
                            if info['tag']=='SUMMARY':
                                previous=cdp.evaluate("document.querySelector('details').open")
                                keyboard(cdp,'Enter','Enter',13)
                                assert cdp.evaluate("document.querySelector('details').open")!=previous,(name,'Disclosure did not toggle')
                            if info['tag']=='VIDEO':
                                keyboard(cdp,' ','Space',32); time.sleep(.4)
                            if info['tag']=='BUTTON' and info['text'] in ('再生','Play'):
                                keyboard(cdp,'Enter','Enter',13);time.sleep(.4)
                                assert cdp.evaluate("!document.querySelector('video').paused"),(name,'Play failed')
                            if info['tag']=='BUTTON' and info['text'] in ('停止して先頭へ','Stop and restart'):
                                keyboard(cdp,'Enter','Enter',13)
                                assert cdp.evaluate("document.querySelector('video').paused&&document.querySelector('video').currentTime===0"),(name,'Stop failed')
                            if info['tag']=='INPUT':
                                cdp.evaluate("window.seekEvents=[]; for(const e of ['keydown','input','change'])document.activeElement.addEventListener(e,x=>window.seekEvents.push({type:x.type,key:x.key,value:x.target.value}));")
                                keyboard(cdp,'ArrowRight','ArrowRight',39)
                                time.sleep(.2)
                                assert cdp.evaluate("document.querySelector('video').currentTime>0"),(name,'Seek failed',cdp.evaluate("({time:document.querySelector('video').currentTime,seek:document.activeElement.value,ready:document.querySelector('video').readyState,events:window.seekEvents})"))
                        metrics['keyboard_visited']=visited
                        metrics['focus_outline']=focus
                        results.append({'case':name,'metrics':metrics})
                        assert not failures,(name,failures)
                        print('OK '+name,flush=True)
                        if mode=='normal' and palette=='light' and width==1280:
                            download_dir=WORK/f'downloads-{server.server_port}-{lang}'
                            download_dir.mkdir(exist_ok=True)
                            cdp.send('Browser.setDownloadBehavior',{'behavior':'allow','downloadPath':str(download_dir)})
                            for index in range(3):
                                point=cdp.evaluate(f"(() => {{const e=document.querySelectorAll('.sample-download')[{index}];e.scrollIntoView();const r=e.getBoundingClientRect();return {{x:r.left+r.width/2,y:r.top+r.height/2,file:e.download}};}})()")
                                for event_type in ['mousePressed','mouseReleased']:
                                    cdp.send('Input.dispatchMouseEvent',{'type':event_type,'x':point['x'],'y':point['y'],'button':'left','clickCount':1})
                                target=download_dir/point['file']
                                deadline=time.monotonic()+10
                                while not target.exists() and time.monotonic()<deadline:time.sleep(.1)
                                assert target.read_bytes()==(ROOT/'samples'/point['file']).read_bytes(),point
                            results.append({'case':lang+'-http-downloads','result':'Three actual Chrome downloads; exact filenames and bytes match'})
            if mode=='normal':
                for lang,file in [('ja','index.html'),('en','en.html')]:
                    cdp.send('Emulation.setDeviceMetricsOverride',{'width':320,'height':900,'deviceScaleFactor':1,'mobile':False})
                    cdp.send('Emulation.setEmulatedMedia',{'features':[{'name':'forced-colors','value':'active'}]})
                    cdp.send('Page.navigate',{'url':origin+'/promotion/'+file});time.sleep(1)
                    keyboard(cdp);screenshot(cdp,f'{lang}-forced-colors.png')
                    assert cdp.evaluate("matchMedia('(forced-colors:active)').matches")
                    results.append({'case':lang+'-forced-colors','metrics':cdp.evaluate("({border:getComputedStyle(document.querySelector('.button')).borderWidth,focus:getComputedStyle(document.activeElement).outlineWidth,width:innerWidth,scroll:document.documentElement.scrollWidth})")})
                    cdp.send('Emulation.setEmulatedMedia',{'features':[{'name':'prefers-color-scheme','value':'light'}]})
                    cdp.send('Emulation.setEmulatedVisionDeficiency',{'type':'achromatopsia'})
                    screenshot(cdp,lang+'-grayscale.png')
                    cdp.send('Emulation.setEmulatedVisionDeficiency',{'type':'none'})
                # The review copies must work after being moved away from their asset directory.
                isolated=WORK/'portable-review';isolated.mkdir(exist_ok=True)
                for lang,file in [('ja','index.html'),('en','en.html')]:
                    target=isolated/file;shutil.copy2(ROOT/'review'/file,target)
                    cdp.send('Emulation.setDeviceMetricsOverride',{'width':1280,'height':900,'deviceScaleFactor':1,'mobile':False})
                    cdp.send('Page.navigate',{'url':target.as_uri()});time.sleep(2)
                    assert cdp.evaluate("[...document.images].every(i=>i.complete&&i.naturalWidth>0)")
                    assert cdp.evaluate("getComputedStyle(document.body).backgroundColor==='rgb(249, 239, 203)'")
                    assert cdp.evaluate("document.querySelector('video').readyState>=1"),lang+' standalone video'
                    screenshot(cdp,lang+'-standalone.png')
                    results.append({'case':lang+'-standalone-isolated','result':'OK; images, CSS, sample downloads and video embedded'})
                # Read accessible names without claiming a real screen-reader run.
                ax=cdp.send('Accessibility.getFullAXTree')
                named=[{'role':n.get('role',{}).get('value'),'name':n.get('name',{}).get('value')} for n in ax['nodes'] if n.get('role',{}).get('value') in ['link','button','DisclosureTriangle','Video'] and not n.get('ignored')]
                (RESULTS/'accessible-names.json').write_text(json.dumps(named,ensure_ascii=False,indent=2),encoding='utf-8')
        finally:
            if cdp:
                try:cdp.send('Browser.close')
                except Exception:pass
                cdp.close()
            try:proc.wait(timeout=8)
            except subprocess.TimeoutExpired:proc.terminate()
    (RESULTS/'browser-results.json').write_text(json.dumps({'date':'2026-10-13 JST','browser':'Google Chrome','cases':results},ensure_ascii=False,indent=2),encoding='utf-8')
finally:
    server.shutdown();server.server_close()
