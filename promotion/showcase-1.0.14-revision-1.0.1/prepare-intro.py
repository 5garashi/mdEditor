"""File: prepare-intro.py
Description: Prepare a real replacement opening with the product name
Author: 5garashi.com Design Office
Created: 2026-10-20
Version: 1.0.1
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
B=Path(__file__).resolve().parent
s=(B/'record.py').read_text(encoding='utf-8')
s=s.replace("intro=w/('Prototype review.md' if en else '試作品の確認.md')","intro=w/'mdEditor.md'")
s=s.replace("'# '+('Prototype review' if en else '試作品の確認')+'\\n\\n```stl\\n'","'# mdEditor'+'\\n\\n```stl\\n'")
(B/'record.py').write_text(s,encoding='utf-8')
i=s.index('  openfile(sample);')
j=s.index('\n finally:\n  if rec')
s=s[:i]+'''  time.sleep(3);rec.stdin.write(b'q\\n');rec.stdin.flush();rec.wait(timeout=30);rec=None
  shot('opening');print('Replacement intro complete',lang,flush=True)
'''+s[j:]
(B/'record-intro.py').write_text(s,encoding='utf-8')
