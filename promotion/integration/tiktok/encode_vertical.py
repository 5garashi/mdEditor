"""
File: promotion/integration/tiktok/encode_vertical.py
Description: Compose an accessible vertical demo from actual Windows capture
Author: 5garashi.com Design Office
Created: 2026-10-14
License: MIT
SPDX-License-Identifier: MIT
"""
import json, subprocess
from pathlib import Path

HERE=Path(__file__).resolve().parent
WORK=HERE.parents[3]/'work/tiktok-v3'
OUT=HERE/'video'; OUT.mkdir(exist_ok=True)
steps=[
 (0,3,'Markdown on Windows'),
 (3,9,'Open a Markdown document'),
 (9,14,'Edit the heading'),
 (14,20,'Preview the updated text\nand equation'),
 (20,25,'Preview the Mermaid diagram'),
 (25,31,'Open the preview\nin a separate window'),
 (31,34,'Save the Markdown'),
 (34,43,'Export the diagram and math\nas a PDF'),
 (43,49,'Try mdEditor — free\non Microsoft Store')]
def stamp(n,ass=False):
    return f'0:00:{n:02d}.00' if ass else f'00:00:{n:02d}.000'
ass='''[Script Info]
ScriptType: v4.00+
PlayResX: 1080
PlayResY: 1920
WrapStyle: 2
[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Segoe UI,54,&H00235738,&H00235738,&H00CBEFF9,&H00CBEFF9,-1,0,0,0,100,100,0,0,1,0,0,5,80,130,0,1
[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
'''
# ink/paper contrast 7.14:1. Branding uses the app's unchanged official logo.
vtt='WEBVTT\n\nNOTE\nFile: demo-en.vtt\nAuthor: 5garashi.com Design Office\nCreated: 2026-10-14\nLicense: MIT\nSPDX-License-Identifier: MIT\n\n'
for a,b,t in steps:
    caption=t.replace('\n',r'\N')
    ass+=f'Dialogue: 0,{stamp(a,True)},{stamp(b,True)},Default,,0,0,0,,{{\\pos(515,1450)}}{caption}\n'
    vtt+=f'{stamp(a)} --> {stamp(b)}\n{t}\n\n'
(WORK/'captions.ass').write_text(ass,encoding='utf-8-sig')
(OUT/'demo-en.vtt').write_text(vtt,encoding='utf-8')
font="C\\:/Windows/Fonts/seguisb.ttf"
vf=("scale=896:960,pad=1080:1920:72:350:color=0xF9EFCB,"
    f"drawtext=fontfile='{font}':text='mdEditor':fontsize=90:fontcolor=0x385723:x=80:y=130,"
    f"drawtext=fontfile='{font}':text='Free Markdown editor for Windows':fontsize=42:fontcolor=0x385723:x=80:y=250,"
    f"drawtext=fontfile='{font}':text='Get it from Microsoft Store':fontsize=48:fontcolor=0x385723:x=80:y=1640,"
    f"drawtext=fontfile='{font}':text='5garashi.com Design Office':fontsize=42:fontcolor=0x385723:x=80:y=1720,ass=captions.ass")
output=OUT/'mdEditor-demo-vertical-en.mp4'
subprocess.run(['ffmpeg','-y','-i',str(WORK/'raw-en.mp4'),'-vf',vf,'-an','-t','49','-r','20','-c:v','libx264','-crf','20','-preset','medium','-pix_fmt','yuv420p','-movflags','+faststart','-metadata','title=mdEditor Demo — Markdown, separate preview and PDF','-metadata','artist=5garashi.com Design Office','-metadata','comment=Actual Windows source build 1.0.5; silent; no simulated app UI','-metadata','copyright=Captions MIT; official logo CC BY-ND 4.0',str(output)],cwd=WORK,check=True,stdout=subprocess.DEVNULL,stderr=(WORK/'encode.log').open('w'))
probe=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration:stream=codec_type,width,height','-of','json',str(output)]))
assert probe['streams'][0]['width']==1080 and probe['streams'][0]['height']==1920
assert float(probe['format']['duration'])==49
assert not any(s['codec_type']=='audio' for s in probe['streams'])
subprocess.run(['ffmpeg','-y','-ss','29','-i',str(output),'-frames:v','1',str(OUT/'poster-en.png')],check=True,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
record=json.loads((WORK/'recording.json').read_text())
(OUT/'validation.json').write_text(json.dumps({'encoding':probe,'capture':record,'published':False},indent=2),encoding='utf-8')
print('Encoded vertical demo: 1080x1920, 49 seconds, burnt captions and separate VTT')
