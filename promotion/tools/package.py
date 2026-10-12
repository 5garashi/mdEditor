"""
File: promotion/tools/package.py
Description: Package verified public pages and portable review files, excluding old slides
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
from build import ROOT, VERSION
import hashlib, json, zipfile
OUT=ROOT.parent.parent/'deliverables';OUT.mkdir(exist_ok=True)
public=[ROOT/name for name in ['index.html','en.html','style.css','player.js','VERSION','LICENSE','README.md','CHANGELOG.md','PRIVACY.md','THIRD-PARTY-NOTICES.md','IDENTITY-CHECK.md','RECORDING.md','IMPLEMENTATION.md']]
public+=list((ROOT/'samples').glob('*.md'))+list((ROOT/'licenses').glob('*'))
public+=[p for p in (ROOT/'assets').iterdir() if p.name not in ['mdEditor-intro-en.mp4','video-poster.png']]
assert all(p.is_file() for p in public)
manifest={'kit_version':VERSION,'app_source_version':'1.0.5','app_source_commit':'6440a2edbe0c570de9fffe8636e7e0233711d978','files':[{'path':p.relative_to(ROOT).as_posix(),'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(public)]}
(ROOT/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
public.append(ROOT/'manifest.json')
with zipfile.ZipFile(OUT/f'mdEditor-promotion-{VERSION}.zip','w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted(public):z.write(p,p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(OUT/f'mdEditor-review-{VERSION}.zip','w',zipfile.ZIP_DEFLATED) as z:
    for p in sorted((ROOT/'review').glob('*.html')):z.write(p,('ja.html' if p.name=='index.html' else 'en.html'))
for p in [OUT/f'mdEditor-promotion-{VERSION}.zip',OUT/f'mdEditor-review-{VERSION}.zip']:
    with zipfile.ZipFile(p) as z:assert z.testzip() is None
    print(str(p)+' ('+str(p.stat().st_size)+' bytes)')
