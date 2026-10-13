"""
File: promotion/integration/publish.py
Description: Generate and publish a separate gh-pages branch without merging main
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import argparse, subprocess, tempfile
from pathlib import Path
from build import generate, PROMO

def run(*args, cwd):
    return subprocess.run(args,cwd=cwd,check=True,text=True,encoding='utf-8',capture_output=True).stdout.strip()

if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--push',action='store_true',help='Publish generated output to gh-pages')
    args=parser.parse_args()
    repo=PROMO.parent
    remote=run('git','remote','get-url','origin',cwd=repo)
    if remote.removesuffix('.git')!='https://github.com/5garashi/mdEditor':
        raise RuntimeError('Unexpected remote; no publication performed.')
    if run('git','status','--porcelain',cwd=repo):
        raise RuntimeError('Commit source changes first; no publication performed.')
    sha=run('git','rev-parse','HEAD',cwd=repo)
    # Preserve latest YouTube links when Pages is rebuilt outside the release workflow.
    youtube_branch = 'promotion/youtube-state'
    if run('git','ls-remote','--heads','origin',youtube_branch,cwd=repo):
        run('git','fetch','origin',youtube_branch,cwd=repo)
        state = run('git','show','FETCH_HEAD:state.json',cwd=repo)
        runtime = repo / '.youtube-runtime'
        runtime.mkdir(exist_ok=True)
        (runtime / 'state.json').write_text(state, encoding='utf-8')
    scratch=repo.parent/'work'
    scratch.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory(prefix='mdeditor-pages-',dir=scratch) as tmp:
        output=Path(tmp)
        generate(output)
        (output/'SOURCE-COMMIT.txt').write_text(sha+'\n',encoding='utf-8')
        run('git','init','-b','gh-pages',cwd=output)
        for key in ['user.name','user.email']:
            run('git','config',key,run('git','config',key,cwd=repo),cwd=output)
        run('git','remote','add','origin',remote,cwd=output)
        existing=run('git','ls-remote','--heads','origin','gh-pages',cwd=output)
        if existing:
            run('git','fetch','origin','gh-pages',cwd=output)
            # Preserve publication history and permit only fast-forward pushes.
            run('git','reset','--soft','FETCH_HEAD',cwd=output)
        run('git','add','--all',cwd=output)
        run('git','commit','-m','Publish promotion source '+sha,cwd=output)
        if args.push:
            print(run('git','push','origin','HEAD:gh-pages',cwd=output))
        else:
            print('Generated locally. Add --push to publish gh-pages. Source:',sha)
