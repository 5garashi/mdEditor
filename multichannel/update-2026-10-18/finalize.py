"""File: finalize.py
Description: Record verified channel publication and build the distribution kit
Author: 5garashi.com Design Office
Created: 2026-10-18
Version: 1.8.0
License: MIT
SPDX-License-Identifier: MIT
"""
from pathlib import Path
import json,re,shutil,zipfile
B=Path(__file__).resolve().parent;P=B/'repo/promotion';K=P/'multichannel/update-2026-10-18'
URLs={'YouTube JA':'https://www.youtube.com/watch?v=CAi5q1c0cuo','YouTube EN':'https://www.youtube.com/watch?v=valiuj76iG4','TikTok JA':'https://www.tiktok.com/@5garashi_com/video/7697746476302175504','TikTok EN':'https://www.tiktok.com/@5garashi_com/video/7697747556910042375','X':'https://x.com/5garashi_com/status/2111568105919131719','LinkedIn':'https://www.linkedin.com/feed/update/urn:li:activity:7517332938926256128/','Zenn':'https://zenn.dev/5garashi_com/articles/mdeditor-diagrams-math-pdf'}
header='''<!--
ファイル名 / File: PUBLICATION.md
概要 / Description: ソース1.0.12の媒体別公開結果 / Source 1.0.12 channel publication
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-10-18
版 / Version: 1.8.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
-->
'''
sign='\n---\n\n**作成者 / Author**: 5garashi.com設計事務所 / 5garashi.com Design Office\n**最終更新 / Last updated**: 2026-10-18 JST\n'
rows='\n'.join('| '+key+' | [公開先]('+url+') | '+('public、API状態と公開oEmbedを確認' if key.startswith('YouTube') else '誰でも、自社宣伝・AIラベル、公開完了通知と動画URLを確認' if key.startswith('TikTok') else '@5garashi_com、送信後の投稿URL・本文・画像の代替テキストを確認' if key=='X' else 'MOTOMASA IGARASHI、Anyone、Post successfulと公開本文・画像を確認' if key=='LinkedIn' else '連携元の対象1ファイルだけを更新。公開HTMLで新版・新動画・3D画像を確認')+' |' for key,url in URLs.items())
report='''# ソース1.0.12の公開結果 / Publication

2026-10-18 JST。ソース版 **1.0.12**、動画パッケージ **revision 1.1.1**、YouTube公開管理 **revision 3.0.0**。Store配信版番号は未確認です。

3Dモデルが回る実画面から入り、diagram-testの図・数式・コード・地図・動画を見せ、主な20機能の紹介へつなぐ内容を、日英の紹介ページ・動画・画像・PDF・記事・投稿へ展開しました。別窓の背景映り込みを除いた動画を使用しています。

## 動画・記事・SNS

| 媒体 | URL | 確認したこと |
| --- | --- | --- |
'''+rows+'''

## Webと配布資料

| 資料 | 日本語 | English |
| --- | --- | --- |
| 紹介ページ | [開く](https://5garashi.github.io/mdEditor/) | [Open](https://5garashi.github.io/mdEditor/en.html) |
| 資料ページ | [開く](https://5garashi.github.io/mdEditor/resources.html) | [Open](https://5garashi.github.io/mdEditor/resources-en.html) |
| YouTube固定入口 | [開く](https://5garashi.github.io/mdEditor/youtube.html) | [Open](https://5garashi.github.io/mdEditor/youtube-en.html) |
| 1枚紹介PDF | [PDF](one-page-ja.pdf) | [PDF](one-page-en.pdf) |
| 20機能ガイド | [2ページPDF](features-ja.pdf) | [2-page PDF](features-en.pdf) |
| 投稿画像 | [PNG](social-ja.png) | [PNG](social-en.png) |
| 制作事例 | [開く](https://5garashi.github.io/mdEditor/case-study.html) | [Open](https://5garashi.github.io/mdEditor/case-study-en.html) |
| 事務所用カード | [開く](https://5garashi.github.io/mdEditor/office-card-ja.html) | [Open](https://5garashi.github.io/mdEditor/office-card-en.html) |

[配布ZIP](mdEditor-materials-1.0.12.zip)には日英PDF・画像・原稿・サンプル・出典を収録。動画は紹介ページから再生・保存できます。

日英紹介・資料・制作事例・カードは64通り（昼夜、320/640/1280px、文字拡大を含む）のChrome確認が通りました。公開8ページを取得し、日英のMP4・1枚PDF・20機能PDF・画像は原本と配信SHA-256が一致しています。YouTube固定入口も新動画IDを確認しました。Zenn連携元は `7d19cea`、変更は `articles/mdeditor-diagrams-math-pdf.md` だけです。

[5garashi.com](https://www.5garashi.com/)の既存mdEditorリンクがあることを確認しました。案内先の紹介・資料・事務所用カードを更新しています。Google Sites本文の再編集はしていません。

## 確認範囲

新動画は実際のアプリ画面と字幕・BGMを用います。全キー操作、全形式での再出力、異常終了からの復元、実2台目ディスプレイ、全読み上げ環境を再試験したという意味ではありません。PDFはタグなし。SNS再圧縮後の全端末での可読性と音量、OSコントラストテーマは未確認です。

動画・音楽・地図・ロゴの帰属表示を保持。XとLinkedInの画像には代替テキストを付けました。旧投稿・動画は削除せず、YouTubeの旧動画の説明へ最新版リンクを追加しています。本人のLinkedInロゴの未コミット変更は保持しています。

公開用原本の最初のコミットは `9aa3735`。YouTube同期 [39070008936](https://github.com/5garashi/mdEditor/actions/runs/39070008936) と一般公開 [39070125058](https://github.com/5garashi/mdEditor/actions/runs/39070125058) は成功しました。Pagesは既存の `gh-pages` 公開を使用し、日英の紹介と資料を生成しました。

デザイン参照：[Design Office Identity 4.20.0](https://5garashi.github.io/5garashi-identity/)。MITは記事と制作コードへ適用し、第三者素材の権利を上書きしません。
'''
(K/'PUBLICATION.md').write_text(header+report+sign,encoding='utf-8')
for lang in ['ja','en']:
 f=K/f'posts-{lang}.md';body=(B/('x.txt' if lang=='ja' else 'linkedin.txt')).read_text(encoding='utf-8');f.write_text(header.replace('File: PUBLICATION.md',f'File: posts-{lang}.md')+'# 公開原稿 / Published copy\n\n```text\n'+body+'\n```\n\n[公開結果](PUBLICATION.md)\n'+sign,encoding='utf-8')
v=K/'verification';v.mkdir(exist_ok=True)
for n in ['live-validation.json','latest-entry-validation.json','zenn-live.json','youtube-published-state.json','tiktok-results.json','x-result.json']:
 shutil.copy2(B/n,v/n)
(v/'linkedin-result.json').write_text(json.dumps({'url':URLs['LinkedIn'],'status':'Post successful; opened published post and observed the new text','author':'MOTOMASA IGARASHI','privacy':'Anyone','image':'social-en.png','alt_text':(B/'image-alt.txt').read_text(encoding='utf-8')},ensure_ascii=False,indent=2),encoding='utf-8')
(K/'linkedin.txt').write_text((B/'linkedin.txt').read_text(encoding='utf-8'),encoding='utf-8');(K/'x.txt').write_text((B/'x.txt').read_text(encoding='utf-8'),encoding='utf-8')
# The existing public report remains as history below this current table.
f=P/'PROMOTION-SYSTEM.md';s=f.read_text(encoding='utf-8');a=s.index('対象アプリは');e=s.index('\n',a);s=s[:a]+'今回の対象はソース版 **1.0.12**、動画パッケージ **revision 1.1.1**、YouTube公開管理 **revision 3.0.0**。2026-10-18 JSTに日英動画と各媒体を公開・確認しました。Store配信版番号は未確認です。文書の版はヘッダー、資料キットは[VERSION](VERSION)、改版内容は[CHANGELOG](CHANGELOG.md)で管理します。'+s[e:]
a=s.index('進行中の新版：');e=s.index('\n',a);s=s[:a]+'最新版：[日英の公開資料と媒体別記録](multichannel/update-2026-10-18/README.md)。3Dから始め、主な20機能へつなぐ日英の紹介・資料ページ、PDF・画像・サンプル、YouTube・TikTok・X・LinkedIn・Zennを更新しました。[動画原本と編集記録](showcase-1.0.12-revision-1.1.1/README.md)には背景映り込みの除去方法も保存しています。'+s[e:]
marker='## 6. 今回の公開結果 — ソース1.0.10';i=s.index(marker)
table='## 6. 今回の公開結果 — ソース1.0.12\n\n2026-10-18 JSTに確認。日英Web・動画・PDF・画像・20機能ガイド・記事・SNSの公開結果と範囲は [詳細記録](multichannel/update-2026-10-18/PUBLICATION.md)へまとめました。\n\n| 媒体 | 公開URL | 状態 |\n| --- | --- | --- |\n'+ '\n'.join('| '+key+' | [開く]('+url+') | 公開確認済み |' for key,url in URLs.items())+'\n| 日英紹介・資料・PDF・画像 | [日本語](https://5garashi.github.io/mdEditor/) / [English](https://5garashi.github.io/mdEditor/en.html) / [資料](https://5garashi.github.io/mdEditor/resources.html) | 原本と配信一致 |\n| 制作事例・事務所カード | [制作事例](https://5garashi.github.io/mdEditor/case-study.html) / [カード](https://5garashi.github.io/mdEditor/office-card-ja.html) | 日英更新済み |\n| 事務所サイト | [5garashi.com](https://www.5garashi.com/) | 既存導線を確認。案内先を更新 |\n\n### 6.1 前回の公開記録 — ソース1.0.10'
s=s[:i]+table+s[i+len(marker):];s=s.replace('Design Office Identity 4.15.0','Design Office Identity 4.20.0').replace('共通チェックリスト（4.17.0）','共通チェックリスト（参照タグ4.20.0）').replace('/blob/v4.17.0/docs/','/blob/v4.20.0/docs/');f.write_text(s,encoding='utf-8')
for relative,note in [
 ('integration/tiktok/README.md','2026-10-18：[日本語動画]('+URLs['TikTok JA']+')と[英語動画]('+URLs['TikTok EN']+')を@5garashi_comから公開。ソース1.0.12、背景除去済みrevision 1.1.1。誰でも、自社宣伝・AIラベルを確認。'),
 ('integration/x/README.md','2026-10-18：[新版の案内]('+URLs['X']+')を@5garashi_comで公開。日英実演リンク、日本語画像と代替テキストを確認。'),
 ('integration/LINKEDIN-FIRST-POST.md','2026-10-18：[新版の案内]('+URLs['LinkedIn']+')をMOTOMASA IGARASHI / Anyoneで公開。日英実演リンク、画像と代替テキスト、公開本文を確認。'),
 ('integration/zenn/README.md','2026-10-18：[既存記事]('+URLs['Zenn']+')を1.0.12の3D画面・新動画・主な20機能へ更新。連携元7d19cea、対象記事1ファイルのみ。公開HTMLで反映確認。')]:
 f=P/relative;s=f.read_text(encoding='utf-8');s=re.sub(r'(Version: )[^\n]+',r'\g<1>1.8.0',s,count=1);pos=s.index('\n\n',s.index('\n# '));s=s[:pos]+ '\n\n'+note+' [詳細記録](https://github.com/5garashi/mdEditor/blob/promotion/site-integration/promotion/multichannel/update-2026-10-18/PUBLICATION.md)。'+s[pos:];s=s.replace('**現在の公開状態 / Current publication:**','**以前の公開状態 / Previous publication:**');f.write_text(s,encoding='utf-8')
A=P/'integration/youtube/automation';(A/'VERSION').write_text('1.0.4\n',encoding='utf-8')
for name in ['README.md','CHANGELOG.md']:
 f=A/name;s=f.read_text(encoding='utf-8').replace('Version: 1.0.3','Version: 1.0.4')
 if name=='README.md':
  i=s.index('2026-10-17:');s=s[:i]+'2026-10-18：ソース1.0.12・公開管理revision 3.0.0の[日本語動画]('+URLs['YouTube JA']+')と[英語動画]('+URLs['YouTube EN']+')を一般公開済み。字幕・サムネイル・旧動画からの最新版リンク、公開oEmbedを確認しました。\n\n'+s[i:]
 else:s+='\n## 1.0.4 — 2026-10-18\n\n- Publish source 1.0.12 visual-first videos as revision 3.0.0 with captions, thumbnails and previous-video links.\n'
 f.write_text(s,encoding='utf-8')
f=P/'README.md';s=f.read_text(encoding='utf-8');s=s.replace('各媒体の確認結果をまとめます。','各媒体の公開URL・確認結果を記録しました。');s=s.replace('共通チェックリスト（4.17.0）','共通チェックリスト（参照タグ4.20.0）').replace('/blob/v4.17.0/docs/','/blob/v4.20.0/docs/');f.write_text(s,encoding='utf-8')
# Save small reproducible publication helpers, not browser profiles or credentials.
for n in ['prepare.py','prepare-pages.py','prepare-zenn.py','prepare-social.py','verify-live.py','finalize.py']:shutil.copy2(B/n,K/n)
for name in ['diagram-test.md','diagram-test.en.md']:shutil.copy2(P/'samples'/name,K/name)
for n in ['LICENSE','THIRD-PARTY-NOTICES.md']:shutil.copy2(P/n,K/n)
shutil.copytree(P/'samples/import-test-images',K/'import-test-images',dirs_exist_ok=True)
with zipfile.ZipFile(K/'mdEditor-materials-1.0.12.zip','w',zipfile.ZIP_DEFLATED,compresslevel=5) as z:
 for f in K.rglob('*'):
  if f.is_file() and f.suffix not in ['.zip','.py'] and 'verification' not in f.parts:z.write(f,f.relative_to(K))
print('Recorded all public channels and prepared the distribution ZIP')
