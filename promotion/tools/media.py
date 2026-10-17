"""
ファイル名 / File: promotion/tools/media.py
概要 / Description: Add bilingual captions to real-time Windows recordings; no music
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import json, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
WORK=ROOT.parent.parent/'work'
ASSETS=ROOT/'assets'
CAPTIONS=[
(0,3,'mdEditor — WindowsでMarkdownを編集','mdEditor — Markdown on Windows'),
(3,9,'「開く」から、図と数式のサンプルを選択','Open a sample with diagrams and math'),
(9,13,'見出しを編集すると、右のプレビューも更新','Edit the heading. The preview updates.'),
(13,17,'数式はKaTeXで表示','Preview the equation with KaTeX'),
(17,23,'Mermaidの文字を変更。図にも反映','Edit a Mermaid label. The diagram updates.'),
(23,25,'Markdownを保存','Save the Markdown document'),
(25,30,'「PDFに保存」で、保存先を選択','Choose Save as PDF, then a destination'),
(30,35,'保存完了。図・数式を含むPDFを書き出し','PDF saved — including the diagram and equation'),
(35,40,'mdEditor · Microsoft Store / 5garashi.com','mdEditor · Microsoft Store / 5garashi.com')]

def stamp(sec,ass=False):
    return f'0:00:{sec:02}.00' if ass else f'00:00:{sec:02}.000'

info=[]
for lang,col in [('ja',2),('en',3)]:
    # Bottom caption band uses ink/paper, 7.14:1; original app pixels are not recolored.
    ass='''[Script Info]
ScriptType: v4.00+
PlayResX: 1280
PlayResY: 720
WrapStyle: 2
[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Meiryo,24,&H00235738,&H00235738,&H00CBEFF9,&H00CBEFF9,-1,0,0,0,100,100,0,0,1,0,0,2,32,32,24,1
[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
'''
    note=('File: ' if lang=='en' else 'ファイル名 / File: ')+f'promotion/assets/demo-{lang}.vtt\n'
    note+=('Description: ' if lang=='en' else '概要 / Description: ')+('Windows demonstration captions\n' if lang=='en' else 'Windows実演の字幕 / Windows demonstration captions\n')
    note+=('Author: 5garashi.com Design Office\n' if lang=='en' else '作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office\n')
    note+=('Created: ' if lang=='en' else '作成日 / Created: ')+'2026-10-13\n'
    note+=('Version: ' if lang=='en' else '版 / Version: ')+(ROOT/'VERSION').read_text().strip()+'\n'
    note+=('License: ' if lang=='en' else 'ライセンス / License: ')+'MIT\nSPDX-License-Identifier: MIT\n'
    vtt='WEBVTT\n\nNOTE\n'+note+'\n'
    for start,end,ja,en in CAPTIONS:
        text=ja if lang=='ja' else en
        ass+=f'Dialogue: 0,{stamp(start,True)},{stamp(end,True)},Default,,0,0,0,,{text}\n'
        vtt+=f'{stamp(start)} --> {stamp(end)}\n{text}\n\n'
    (ASSETS/f'demo-{lang}.vtt').write_text(vtt,encoding='utf-8',newline='\n')
    (WORK/f'captions-{lang}.ass').write_text(ass,encoding='utf-8-sig',newline='\n')
    raw=WORK/f'raw-{lang}.mp4'
    output=ASSETS/f'mdEditor-demo-{lang}.mp4'
    subprocess.run(['ffmpeg','-y','-i',str(raw),'-vf',f'pad=1280:720:8:0:color=0xF9EFCB,ass=captions-{lang}.ass','-an','-t','40','-r','20','-c:v','libx264','-preset','medium','-crf','20','-pix_fmt','yuv420p','-metadata','title=mdEditor Windows demonstration','-metadata','artist=5garashi.com Design Office','-metadata','copyright=Copyright 2026 5garashi.com Design Office; captions MIT; official logos CC BY-ND 4.0','-metadata','comment=Recorded with unmodified mdEditor source build 1.0.5; silent; real-time Windows capture','-movflags','+faststart',str(output)],cwd=WORK,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE,check=True)
    subprocess.run(['ffmpeg','-y','-ss','20','-i',str(output),'-frames:v','1',str(ASSETS/f'video-poster-{lang}.png')],stdout=subprocess.DEVNULL,stderr=subprocess.PIPE,check=True)
    result=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration:stream=codec_type,width,height','-of','json',str(output)]))
    assert float(result['format']['duration'])==40
    assert all(s['codec_type']!='audio' for s in result['streams'])
    info.append({'file':output.name,**result})
    print('Encoded '+output.name,flush=True)
(ROOT/'validation/media-results.json').write_text(json.dumps(info,indent=2),encoding='utf-8')
