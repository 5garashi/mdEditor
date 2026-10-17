"""File: promotion/tools/showcase.py
Description: Apply the current visual-first bilingual introduction
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.8.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,re,html
P=Path(__file__).resolve().parents[1]
FEATURES=json.loads(Path(__file__).with_name('showcase-features.json').read_text(encoding='utf-8'))
def apply(s,lang):
 en=lang=='en'
 def t(a,b):return b if en else a
 s=s.replace('1.0.10','1.0.12').replace('2026-10-17','2026-10-18').replace('Identity 4.15.0','Identity 4.20.0')
 version=(P/'VERSION').read_text().strip();s=re.sub(r'(Version: )[^\n]+',lambda m:m[1]+version,s,count=1);s=re.sub(r'(<span class="version" data-version>).*?(</span>)',lambda m:m[1]+version+m[2],s)
 title=t('mdEditor — 図も、3Dも、動画も。','mdEditor — Diagrams, 3D and video.')
 s=re.sub(r'<title>.*?</title>','<title>'+title+' | 5garashi.com</title>',s)
 s=re.sub(r'<meta name="description"[^>]*>', '<meta name="description" content="'+t('Markdownに図・数式・3D・地図・動画。書きながら確認し、PDFやWordへ共有。Windows用mdEditor 1.0.12の実演と20機能。','Diagrams, math, 3D, maps and video in Markdown. Edit with a live preview and share PDF or Word. Explore mdEditor 1.0.12 for Windows.')+'">',s)
 s=re.sub(r'(<h1 id="hero-heading">).*?(</h1>)',lambda m:m[1]+t('図も、3Dも、動画も。<br>ひとつのMarkdownから。','Diagrams. 3D. Video.<br>One Markdown document.')+m[2],s,flags=re.S)
 s=re.sub(r'(<p class="lead">).*?(</p>)',lambda m:m[1]+t('書きながら仕上がりを確認。<br>立体を回し、地図を読み、必要な形式で共有。','Write beside a live preview.<br>Rotate models, explore maps and share in the format you need.')+m[2],s,count=1,flags=re.S)
 a=s.index('<figure class="app-figure">');b=s.index('</figure>',a)+len('</figure>')
 s=s[:a]+f'<figure class="app-figure"><a class="image-link" href="assets/hero-3d-1.0.12-{lang}.png"><img src="assets/hero-3d-1.0.12-{lang}.png" width="1264" height="642" alt="'+t('mdEditorの実画面。左にSTLのテキスト、右に赤い5の付いたクリーム色の3D立方体。','Actual mdEditor screen: STL text on the left and a cream 3D cube with a red 5 on the right.')+'"></a><figcaption>'+t('文書の中のSTLモデルを回転・拡大。操作は下の実演で。','Rotate and zoom an STL model inside your document. Watch the demo below.')+'</figcaption></figure>'+s[b:]
 cards=[(t('図・3D・地図・動画','Diagrams, 3D, maps and video'),t('Mermaid・Graphviz・数式から、回転できるSTL、GeoJSON、文書内動画まで。','From Mermaid, Graphviz and math to interactive STL, GeoJSON and embedded video.')),(t('書きながら、仕上がりを見る','Write with the result in view'),t('連動スクロール、別窓プレビュー、ウィンドウ全体で読む表示。','Synchronized scrolling, a detached preview and a full-window viewer.')),(t('取り込み、必要な形式で渡す','Import and share'),t('Word・HTML・メール・CSVなどを開き、PDF・Word・HTML・PNGなどへ保存。','Open Word, HTML, email and CSV; save PDF, Word, HTML, PNG and more.'))]
 s=re.sub(r'<section class="features".*?</section>','<section class="features" aria-label="'+t('できること','Features')+'">'+''.join('<article><h2>'+a+'</h2><p>'+b+'</p></article>' for a,b in cards)+'</section>',s,count=1,flags=re.S)
 s=re.sub(r'(<h2 id="demo-heading">).*?(</h2>)',lambda m:m[1]+t('まず、3Dを回す。そのあと、使い方へ。','Start with rotating 3D. Then see how it works.')+m[2],s,flags=re.S)
 s=re.sub(r'(<output class="player-clock"[^>]*>).*?(</output>)',lambda m:m[1]+('00:00 / 03:09' if en else '00:00 / 03:06')+m[2],s)
 s=s.replace('max="94"','max="190"')
 a=s.index('<details><summary>');b=s.index('</section>',a)
 steps=[t('3Dモデルを回転・拡大し、mdEditorを紹介。','Rotate and zoom the 3D model.'),t('diagram-testを上から下へ。図・数式・コード・3D・地図・動画を見る。','Tour diagram-test from top to bottom: diagrams, math, code, 3D, maps and video.'),t('編集・別窓・ビューアー・目次・取り込み・PDF保存などの主な機能を紹介。','Explore editing, detached preview, viewer, contents, import and PDF export.')]
 s=s[:a]+'<details><summary>'+t('動画の内容を読む','Read the video outline')+'</summary><ol>'+''.join('<li>'+x+'</li>' for x in steps)+'</ol></details><p class="note">'+t('ソース版1.0.12の実画面です。Store配信版番号は未確認。字幕・BGM付き、ナレーションなし。外部メディアと地図は許可して収録しました。','Actual source build 1.0.12. Store build number unverified. Captions and BGM, no narration. Internet media and maps were allowed during recording.')+'</p>'+s[b:]
 # Replace obsolete feature-specific demo caption with accurate credits.
 a=s.index('<p class="note">',s.index('class="player-error"'));b=s.index('</p>',a)+4
 s=s[:a]+'<p class="note">'+t('再生するとBGMが流れます。','BGM starts when you play.')+' Light Steps — 5garashi.com Design Office / Codex, CC BY 4.0; excerpt repeated and faded. Big Buck Bunny © 2008 Blender Foundation, CC BY 3.0 · <a href="https://peach.blender.org/about/">Credits</a>. Map: © OpenStreetMap contributors.</p>'+s[b:]
 s=re.sub(r'<section id="feature-list".*?</section>','',s,flags=re.S)
 section='<section id="feature-list"><h2>'+t('主な20機能','20 main features')+'</h2><ol>'+''.join('<li>'+html.escape(f[lang])+'</li>' for f in FEATURES)+'</ol><p>'+t('文書の形式や読み込み内容によって、引き継がれる情報は異なります。Wordの色・配置や、Notebookの実行結果は再現しません。','Preserved content depends on the format. Word import does not preserve colors or layout; notebook export omits execution output.')+'</p></section>'
 s=s.replace('<section class="demo"',section+'<section class="demo"',1)
 s=s.replace('外部画像は初期設定では確認後に読み込みます。','ネット上の画像・動画・地図は、許可した場合に読み込みます。').replace('Internet images require consent by default.','Internet images, video and maps load only with permission.')
 return s

if __name__=='__main__':
 for lang,name in [('ja','index.html'),('en','en.html')]:
  f=P/name;f.write_text(apply(f.read_text(encoding='utf-8'),lang),encoding='utf-8')
