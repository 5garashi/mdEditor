"""
ファイル名 / File: promotion/tools/media-check.py
概要 / Description: 新しい媒体素材の色・記事リンク・配布ZIPを検証 / Verify media content and packages
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-13
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import hashlib,json,re,zipfile
from pathlib import Path
import fitz
from build import ROOT,VERSION
MEDIA=ROOT/'multichannel'
OUT=ROOT.parent.parent/'deliverables'
def lum(h):
    rgb=[int(h[i:i+2],16)/255 for i in (1,3,5)]
    linear=[v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4 for v in rgb]
    return sum(v*w for v,w in zip(linear,[.2126,.7152,.0722]))
ratios=[]
for color in ['#385723','#7B4A21','#8C1B0D']:
    ratio=1.05/(lum(color)+.05)
    assert ratio>=7
    ratios.append({'foreground':color,'background':'#FFFFFF','ratio':round(ratio,4),'method':'WCAG sRGB relative luminance'})
article_links=[]
for article in (MEDIA/'articles').glob('*.md'):
    text=article.read_text(encoding='utf-8')
    assert '5garashi.com' in text and 'Mermaid' in text and 'KaTeX' in text
    assert 'SPDX-License-Identifier: MIT' in text
    # Verify the copyable TeX has single command backslashes, as in the tested samples.
    formula=next(line for line in text.splitlines() if line.startswith('$$P='))
    assert formula.count(chr(92))==4
    for target in re.findall(r'\]\(([^)]+)\)',text):
        if target.startswith(('https:','http:')):continue
        path=(article.parent/target).resolve()
        assert path.is_file(),(article.name,target)
        article_links.append({'article':article.name,'relative_target':target})
post_lengths=[]
for posts in (MEDIA/'social').glob('posts-*.md'):
    text=posts.read_text(encoding='utf-8')
    shorts=re.findall(r'### (?:短文|Short copy)\n\n(.*?)\n\nhttps:',text,re.S)
    assert len(shorts)==3
    post_lengths.append({'file':posts.name,'short_body_characters':[len(x) for x in shorts]})
    for target in re.findall(r'\]\(([^)]+)\)',text):
        if target.startswith(('https:','http:')):continue
        assert (posts.parent/target).is_file(),target
packages=[]
for name in [f'mdEditor-multichannel-{VERSION}.zip',f'mdEditor-promotion-{VERSION}.zip']:
    file=OUT/name
    with zipfile.ZipFile(file) as z:
        assert z.testzip() is None
        assert sum(x.startswith('multichannel/social/') and x.endswith('.png') for x in z.namelist())==12
        assert sum(x.startswith('multichannel/articles/') and x.endswith('.md') for x in z.namelist())==2
        for lang in ['ja','en']:
            path=f'multichannel/print/one-page-{lang}.pdf'
            pdf=fitz.open(stream=z.read(path),filetype='pdf')
            assert len(pdf)==1
            assert '1.2.0' in pdf[0].get_text()
        for item in article_links:
            resolved=(Path('multichannel/articles')/item['relative_target'])
            parts=[]
            for part in resolved.parts:
                if part=='..':parts.pop()
                elif part!='.':parts.append(part)
            assert '/'.join(parts) in z.namelist(),resolved
    packages.append({'file':name,'bytes':file.stat().st_size,'sha256':hashlib.sha256(file.read_bytes()).hexdigest(),'ZIP_integrity':'OK'})
(ROOT/'validation/multichannel-content.json').write_text(json.dumps({'date':'2026-10-13 JST','version':VERSION,'new_contrasts':ratios,'article_links':article_links,'post_lengths':post_lengths,'packages':packages},ensure_ascii=False,indent=2),encoding='utf-8')
print('Print contrasts, article links and media ZIP contents: OK')
