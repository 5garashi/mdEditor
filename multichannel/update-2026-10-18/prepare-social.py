from pathlib import Path
import json
B=Path(__file__).resolve().parent;P=B/'repo/promotion';A=P/'integration/youtube/automation/release.json';c=json.loads(A.read_text(encoding='utf-8'));c.update(operation='publish',request_id='2026-10-18-source-1.0.12-showcase-3.0.0-publish');A.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
x='''mdEditor 1.0.12を更新。図・3D・地図・動画から、編集と共有まで。日英の実演と資料を公開しました。

JA: https://www.youtube.com/watch?v=CAi5q1c0cuo
EN: https://www.youtube.com/watch?v=valiuj76iG4

Samples / PDF: https://5garashi.github.io/mdEditor/
#mdEditor #Markdown'''
li='''I updated mdEditor, the free Windows Markdown app I develop at 5garashi.com Design Office.

The new demonstration starts with a rotating 3D model, then tours diagrams, equations, code, maps and video in one document. The second half introduces the main editing, viewing, import and sharing features.

Write beside a synchronized live preview, detach it into another window, build a contents list, and share PDF, Word, HTML or PNG. Internet media and maps load only with permission. No ads, no tracking and no account required.

mdEditor 1.0.12を更新しました。回転する3Dから始まり、図・数式・地図・動画を一つの文書で見る実演と、主な20機能の紹介を公開しました。日英の画像・PDF・機能ガイド・サンプルも更新しています。

English demo: https://www.youtube.com/watch?v=valiuj76iG4
日本語の実演: https://www.youtube.com/watch?v=CAi5q1c0cuo
Guide, samples and PDFs: https://5garashi.github.io/mdEditor/en.html
日本語: https://5garashi.github.io/mdEditor/

The recording uses source build 1.0.12. The current Store build number is unverified. Word import does not preserve layout; notebook export omits execution output.

#Markdown #Windows #SoftwareDevelopment'''
(B/'x.txt').write_text(x,encoding='utf-8');(B/'linkedin.txt').write_text(li,encoding='utf-8')
(B/'image-alt.txt').write_text('mdEditor source 1.0.12 on Windows. A cream cube with a red 5 is displayed in the 3D preview. The card introduces diagrams, 3D and video, live editing with synchronized scrolling, detached preview, and document import and export.',encoding='utf-8')
