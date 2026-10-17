"""File: materials.py
Description: Refresh promotion drafts and map all twenty requested features to footage
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.0.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,subprocess,sys,shutil
B=Path(__file__).resolve().parent;ROOT=B.parents[1];OUT=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.0';OUT.mkdir(exist_ok=True)
features=json.loads((B/'features.json').read_text(encoding='utf-8'))
s=(ROOT/'work/video-showcase/materials.py').read_text(encoding='utf-8')
s=s.replace("B=Path(__file__).resolve().parent;ROOT=B.parents[1]",f"B=Path({str(B)!r});ROOT=B.parents[1]")
s=s.replace("deliverables/mdEditor-showcase-1.0.12'","deliverables/mdEditor-showcase-1.0.12-revision-1.1.0'").replace("('en','take2'),('ja','take1')","('en','take1'),('ja','take3')")
s=s.replace('2026-10-17','2026-10-18').replace('4.19.0','4.20.0')
s=s.replace('最初にdiagram-test.mdを上から下へ。図の連続表示、3Dの拡大・回転、詳細地図、文書内動画を見てから、新機能と編集・保存の操作を紹介します。','冒頭は3Dモデルを回しながらmdEditorを紹介。そのあとdiagram-testを上から下へ見せ、編集・表示・取り込み・保存などの主な機能を実画面で紹介します。')
s=s.replace('Begin with a top-to-bottom tour of diagram-test.en.md. Explore diagrams, zoom and rotate 3D, and load maps and video. Then follow the new features and real editing and saving operations.','Open with a rotating 3D model and a brief mdEditor introduction. Tour diagram-test.en.md, then explore the main editing, viewing, import and export features.')
s=s.replace('GitHub形式の注意書き・数式・HTML・YAML','編集・連動スクロール・別窓プレビュー・目次').replace('GitHub-style alerts, math, HTML and YAML cards','Live editing, synced scrolling, detached preview and contents')
s=s.replace('後半で新機能とWord取り込み・PDF保存を紹介。','後半で主な機能を紹介。肌色のマスクは使いません。').replace('Then see new features, Word import and PDF export.','Then explore the main features, with no colored masks.')
for lang,take in [('ja','take3'),('en','take1')]:
 w=B/f'{take}-{lang}'
 if (w/'model.png').exists():shutil.copy2(w/'model.png',w/'model-after.png')
(B/'build-materials.py').write_text(s,encoding='utf-8');subprocess.run([sys.executable,'-X','utf8',str(B/'build-materials.py')],check=True)
for lang in ['ja','en']:
 f=OUT/('index.html' if lang=='ja' else 'en.html')
 if not f.exists():continue
 text=f.read_text(encoding='utf-8');title='主な機能' if lang=='ja' else 'Main features'
 text=text.replace('</main>',f'<section><h2>{title}</h2><ol>'+''.join(f'<li>{e[lang]}</li>' for e in features)+'</ol></section></main>');f.write_text(text,encoding='utf-8')
 notes=OUT/f'posts-{lang}.md';text=notes.read_text(encoding='utf-8');text=text.replace('新機能と編集・保存を実演します。','編集・表示・取り込み・保存まで、主な機能を紹介します。').replace('then explore new features and real editing and saving.','then explore its main editing, viewing, import and export features.');notes.write_text(text,encoding='utf-8')
print('Updated materials with all',len(features),'features')
