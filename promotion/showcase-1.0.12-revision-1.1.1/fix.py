"""File: fix.py
Description: Isolate the two mdEditor windows throughout the detached-preview segment
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.1.1
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import subprocess,json,shutil,hashlib,sys
B=Path(__file__).resolve().parent;ROOT=B.parents[1]
SRC=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.0'
OUT=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.1'
if not OUT.exists():shutil.copytree(SRC,OUT)
def run(lang):
 v=json.loads((SRC/f'mdEditor-showcase-{lang}-validation.json').read_text(encoding='utf-8'))
 start=next(e['seconds'] for e in v['events'] if e['key']=='detach');end=next(e['seconds'] for e in v['events'] if e['key']=='viewer');duration=v['duration']
 source=SRC/f'mdEditor-showcase-{lang}.mp4';target=OUT/source.name
 # Repeat only a stable second from the actual two-window recording. Crop away
 # every background pixel, including the gap; replace opening/closing transitions.
 filters=f"[0:v]split[base][clip];[clip]trim=start={start+4}:end={start+5},setpts=PTS-STARTPTS,split[l][r];[l]crop=720:642:8:0[left];[r]crop=524:642:748:0[right];[left][right]hstack,scale=1264:642,loop=loop=-1:size=20:start=0,setpts=N/20/TB[safe];[base][safe]overlay=8:0:enable='between(t,{start-.15},{end+.35})'[v]"
 cmd=['ffmpeg','-y','-filter_complex_threads','2','-i',str(source),'-filter_complex',filters,'-map','[v]','-map','0:a','-t',str(duration),'-c:v','libx264','-preset','fast','-crf','18','-threads','3','-pix_fmt','yuv420p','-c:a','copy','-movflags','+faststart',str(target)]
 subprocess.run(cmd,check=True,stdout=subprocess.DEVNULL,stderr=(B/f'fix-{lang}.log').open('w'))
 save_validation(target,v,lang,'landscape',start,end)
 w=ROOT/'work/video-revision-2026-10-18'/('take3-ja' if lang=='ja' else 'take1-en')
 target=OUT/f'mdEditor-showcase-vertical-{lang}.mp4'
 filters=f'[0:v]crop=1264:642:8:0,split=2[full][detail];[full]scale=1000:-2[screen];[detail]crop=720:440:450:120,scale=1000:-2[zoom];[1:v][screen]overlay=40:470[tmp];[tmp][zoom]overlay=40:1080,ass=portrait.ass[v]'
 subprocess.run(['ffmpeg','-y','-filter_complex_threads','2','-i',str(OUT/f'mdEditor-showcase-{lang}.mp4'),'-loop','1','-framerate','20','-i',str(w/'portrait.png'),'-filter_complex',filters,'-map','[v]','-map','0:a','-t',str(duration),'-r','20','-c:v','libx264','-preset','fast','-crf','20','-threads','3','-pix_fmt','yuv420p','-c:a','copy','-movflags','+faststart',str(target)],cwd=w,check=True,stdout=subprocess.DEVNULL,stderr=(B/f'portrait-{lang}.log').open('w'))
 save_validation(target,v,lang,'portrait',start,end)
 print('COMPLETE',lang,flush=True)
def save_validation(file,v,lang,kind,start,end):
 subprocess.run(['ffmpeg','-v','error','-i',str(file),'-f','null','-'],check=True,stdout=subprocess.DEVNULL,stderr=(B/f'decode-{kind}-{lang}.log').open('w'))
 probe=json.loads(subprocess.check_output(['ffprobe','-v','quiet','-show_format','-show_streams','-of','json',str(file)]))
 v=dict(v,probe=probe,sha256=hashlib.sha256(file.read_bytes()).hexdigest(),background_removed=True,background_replacement_seconds=[start-.15,end+.35],method='Actual stable two-window second repeated; gap and all background cropped; opening and closing transitions replaced; original captions and audio preserved',revision='1.1.1')
 file.with_name(file.stem+'-validation.json').write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
if __name__=='__main__':run(sys.argv[1])
