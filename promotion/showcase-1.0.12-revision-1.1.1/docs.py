from pathlib import Path
import json,shutil
B=Path(__file__).resolve().parent;ROOT=B.parents[1];P=ROOT/'deliverables/mdEditor-showcase-1.0.12-revision-1.1.1'
f=P/'README.md';s=f.read_text(encoding='utf-8').replace('Version: 1.1.0','Version: 1.1.1').replace('Package version: 1.1.0','Package version: 1.1.1')
s=s.replace('## 動画 / Videos','## 背景の映り込み対策 / Background isolation\n\n別窓プレビュー紹介の全区間（開く途中・閉じた直後を含む）は、実録画で安定して表示された1秒を繰り返し、mdEditorの2つのウィンドウだけを切り出した映像へ差し替えました。ウィンドウ間の隙間も除去し、背後のVS Codeを含めません。日英・横縦の4本に反映しています。音声・字幕・動画の長さは維持しています。\n\nThe detached-preview segment uses a repeated stable second from the real recording, cropped to the two mdEditor windows. Background pixels and opening/closing transitions are excluded in all four videos.\n\n制作手順は前版の収録・編集コードに続けて `fix.py` を使います。前版の録画は作業原本として保持しています。\n\n## 動画 / Videos')
f.write_text(s,encoding='utf-8');(P/'VERSION').write_text('1.1.1\n',encoding='utf-8')
f=P/'CHANGELOG.md';s=f.read_text(encoding='utf-8').replace('Version: 1.1.0','Version: 1.1.1');i=s.index('## 1.1.0');s=s[:i]+'## 1.1.1 — 2026-10-18\n\n- 別窓の隙間・開閉途中に映った背景を、2ウィンドウだけを切り出した実録画へ置換。日英・横縦の4本を更新。字幕・BGM・長さを維持。\n- Isolate both mdEditor windows throughout the detached-preview segment, including opening and closing transitions.\n\n'+s[i:];f.write_text(s,encoding='utf-8')
f=P/'FEATURE-COVERAGE.md';s=f.read_text(encoding='utf-8').replace('Version: 1.1.0','Version: 1.1.1');s=s.replace('実2台目への移動は未試験','実2台目への移動は未試験。背景除去のため、安定した1秒の実録画を繰り返して紹介');f.write_text(s,encoding='utf-8')
shutil.copy2(B/'fix.py',P/'fix.py')
p=ROOT/'audit-source/promotion'
for n in ['README.md','PROMOTION-SYSTEM.md','CHANGELOG.md']:
 f=p/n;s=f.read_text(encoding='utf-8').replace('Version: 1.7.0','Version: 1.7.1')
 if n!='CHANGELOG.md':
  s=s.replace('showcase-1.0.12-revision-1.1.0/','showcase-1.0.12-revision-1.1.1/').replace('revision 1.1.0','revision 1.1.1')
  marker='肌色の部分マスクを廃止し、ファイルダイアログの区間をカットしました。' if n=='PROMOTION-SYSTEM.md' else '肌色の長方形を使わず、ファイル選択ダイアログの区間をカットしました。'
  s=s.replace(marker,marker+' 別窓の開閉と隙間に映った背景も、アプリの2ウィンドウだけを切り出した映像へ差し替えました。')
 else:
  i=s.index('## 1.7.0');s=s[:i]+'## 1.7.1 — 2026-10-18\n\n- 動画revision 1.1.1：日英の別窓プレビュー紹介で、隙間と開閉途中の背景を除去。横縦の4本と確認記録を更新。\n- Remove background exposure during detached-preview opening, viewing and closing in all four videos.\n\n'+s[i:]
 f.write_text(s,encoding='utf-8')
(p/'VERSION').write_text('1.7.1\n',encoding='utf-8')
