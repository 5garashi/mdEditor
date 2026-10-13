"""
ファイル名 / File: package_identity_handover.py
概要 / Description: 音楽のIdentity引き継ぎZIPを作成 / Package music for Identity handover
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-14
ライセンス / License: MIT（本コードのみ / code only）
SPDX-License-Identifier: MIT
"""
import hashlib,json,zipfile
from pathlib import Path

HERE=Path(__file__).resolve().parent
music=HERE/'music'
files=[music/name for name in ['mdeditor-light-steps-v1.wav','mdeditor-light-steps-v1.mp3','mdeditor-light-steps-v1.mid','score-v1.json','validation-v1.json','OFFICE-IDENTITY-HANDOVER.md']]
files += [HERE/'compose_music.py',HERE/'video/mdEditor-demo-vertical-en-light-steps-v1.mp4']
out=HERE.parents[3]/'deliverables/LightSteps-Identity-handover-v1.0.0.zip'
out.parent.mkdir(exist_ok=True)
assert not out.exists(),'Preserve existing handover packages; create a new version'
expected=json.loads((music/'validation-v1.json').read_text())['wav_sha256']
assert hashlib.sha256((music/'mdeditor-light-steps-v1.wav').read_bytes()).hexdigest()==expected
manifest={'package_version':'1.0.0','date':'2026-10-14','music_license':'not specified','files':[{'path':p.name,'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]}
with zipfile.ZipFile(out,'x',compression=zipfile.ZIP_DEFLATED) as z:
    for p in files:z.write(p,p.name)
    z.writestr('manifest.json',json.dumps(manifest,ensure_ascii=False,indent=2))
with zipfile.ZipFile(out) as z:
    assert z.testzip() is None
    for record in manifest['files']:assert hashlib.sha256(z.read(record['path'])).hexdigest()==record['sha256']
print('Verified handover ZIP:',out,'files:',len(manifest['files'])+1)
