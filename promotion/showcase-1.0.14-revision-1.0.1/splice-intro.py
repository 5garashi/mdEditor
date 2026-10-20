"""File: splice-intro.py
Description: Replace only the real opening frames, retaining subsequent demo timing
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.0.1
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,subprocess,sys,math
B=Path(__file__).resolve().parent;l=sys.argv[1];w=B/f'take1-{l}'
r=json.loads((w/'recording.json').read_text(encoding='utf-8'))
cut=math.ceil(r['remove_intervals'][0][0]*20)/20
out=w/'clean-revision-1.0.1.mp4'
f=f'[0:v]trim=end={cut},setpts=PTS-STARTPTS[a];[1:v]trim=start={cut},setpts=PTS-STARTPTS[b];[a][b]concat=n=2:v=1:a=0[v]'
subprocess.run(['ffmpeg','-y','-i',str(B/f'intro2-{l}/raw.mp4'),'-i',str(w/'clean.mp4'),'-filter_complex',f,'-map','[v]','-an','-r','20','-c:v','libx264','-preset','veryfast','-crf','16','-threads','3',str(out)],check=True,stdout=subprocess.DEVNULL,stderr=(w/'splice.log').open('w'))
print(l,'replacement seconds',cut)
