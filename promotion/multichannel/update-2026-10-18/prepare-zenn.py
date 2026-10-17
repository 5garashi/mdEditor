from pathlib import Path
import sys,re
B=Path(__file__).resolve().parent;sys.path.insert(0,str(B/'repo/promotion/tools'))
from showcase_article import article
f=B/'zenn/articles/mdeditor-diagrams-math-pdf.md';s=f.read_text(encoding='utf-8');front,body=s.split('\n---',1);front=front.replace('Version: 1.4.0','Version: 1.5.0')
intro=body.split('## 2026-10-17更新:')[0].strip()
new=article('ja').split('\n',1)[1].lstrip();s=front+'\n---\n\n'+intro+'\n\n## 2026-10-18更新：図も、3Dも、動画も\n\n'+new+'\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: 2026-10-18 JST\n'
f.write_text(s,encoding='utf-8');(B/'repo/promotion/integration/zenn/mdeditor-diagrams-math-pdf.md').write_text(s,encoding='utf-8')
print('Prepared only the existing mdEditor article; preserved title, publication settings and personal introduction')
