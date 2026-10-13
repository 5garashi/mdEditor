"""File: sync.py
Description: Release selected YouTube assets with durable duplicate-upload protection.
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[4]
CONFIG = Path(__file__).with_name('release.json')
STATE_BRANCH = 'promotion/youtube-state'


def asset(path):
    resolved = (ROOT / path).resolve()
    if not resolved.is_relative_to(ROOT) or not resolved.is_file():
        raise ValueError('Asset must be an existing file within this repository.')
    return resolved


def sha(path):
    h = hashlib.sha256()
    with path.open('rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            h.update(chunk)
    return h.hexdigest()


def description(v):
    text = asset(v['description_file']).read_text(encoding='utf-8')
    return re.sub(r'^<!--.*?-->\s*', '', text, count=1, flags=re.S)


def load_config(path=CONFIG):
    config = json.loads(path.read_text(encoding='utf-8'))
    if config['channel_id'] != 'UCI_NiKGav2fQZu0IRMxQUeg':
        raise ValueError('Expected 5garashi.com channel; review code before changing channel.')
    keys = set()
    for v in config['videos']:
        if not re.fullmatch(r'[a-z0-9-]+', v['key']) or v['key'] in keys:
            raise ValueError('Video keys must be unique lower-case identifiers.')
        keys.add(v['key'])
        if not re.fullmatch(r'\d+\.\d+\.\d+', v['revision']):
            raise ValueError('Use a three-number video revision.')
        if v['target_privacy'] not in ('private', 'unlisted', 'public'):
            raise ValueError('Unsupported target privacy.')
        if not v['title'] or len(v['title']) > 100 or '<' in v['title'] or '>' in v['title']:
            raise ValueError('YouTube title must contain 1–100 valid characters.')
        text = description(v)
        if len(text) + 100 > 5000 or '<' in text or '>' in text:
            raise ValueError('Description exceeds YouTube limits.')
        for key in ('media', 'thumbnail', 'captions'):
            asset(v[key])
        if asset(v['media']).suffix.lower() != '.mp4':
            raise ValueError('Use an MP4 video.')
        if not asset(v['captions']).read_text(encoding='utf-8-sig').startswith('WEBVTT'):
            raise ValueError('Use timestamped WebVTT captions.')
    return config


class GitState:
    """State contains public IDs and hashes only. Each save must reach origin first."""
    def __init__(self):
        self.parent = None
        remote = self.git('remote', 'get-url', 'origin')
        if remote.removesuffix('.git') != 'https://github.com/5garashi/mdEditor':
            raise RuntimeError('Unexpected origin repository.')
        exists = self.git('ls-remote', '--heads', 'origin', STATE_BRANCH)
        if exists:
            self.git('fetch', 'origin', STATE_BRANCH)
            self.parent = self.git('rev-parse', 'FETCH_HEAD')
            self.data = json.loads(self.git('show', self.parent + ':state.json'))
        else:
            self.data = {'schema': 1, 'videos': {}, 'latest': {}}

    def git(self, *args, input=None):
        p = subprocess.run(['git', *args], cwd=ROOT, input=input, text=True,
                           encoding='utf-8', capture_output=True)
        if p.returncode:
            # Never echo credential-bearing git configuration or remote errors.
            raise RuntimeError('Git state operation failed; no automatic upload retry.')
        return p.stdout.strip()

    def save(self):
        blob = self.git('hash-object', '-w', '--stdin', input=json.dumps(self.data, ensure_ascii=False))
        tree = self.git('mktree', input='100644 blob ' + blob + '\tstate.json\n')
        args = ['commit-tree', tree, '-m', 'Record YouTube release state']
        if self.parent:
            args.extend(['-p', self.parent])
        commit = self.git(*args)
        # Normal fast-forward push: races or lost remote updates stop the run.
        self.git('push', 'origin', commit + ':refs/heads/' + STATE_BRANCH)
        self.parent = commit


class YouTube:
    def __init__(self):
        from google.oauth2.credentials import Credentials
        from googleapiclient.discovery import build
        raw = os.environ.get('YOUTUBE_CREDENTIALS_JSON')
        if not raw:
            raise RuntimeError('Set YOUTUBE_CREDENTIALS_JSON via GitHub Secrets first.')
        creds = Credentials.from_authorized_user_info(json.loads(raw))
        self.service = build('youtube', 'v3', credentials=creds, cache_discovery=False)

    def channel(self):
        items = self.service.channels().list(part='id', mine=True).execute()['items']
        return [item['id'] for item in items]

    def video(self, video_id, channel):
        result = self.service.videos().list(
            part='snippet,status,processingDetails', id=video_id).execute()['items']
        if len(result) != 1 or result[0]['snippet']['channelId'] != channel:
            raise RuntimeError('Video is missing or belongs to another channel.')
        return result[0]

    def upload(self, v, snippet):
        from googleapiclient.http import MediaFileUpload
        request = self.service.videos().insert(
            part='snippet,status', notifySubscribers=False,
            body={'snippet': snippet, 'status': {'privacyStatus': 'private',
                                                'selfDeclaredMadeForKids': False}},
            media_body=MediaFileUpload(str(asset(v['media'])), mimetype='video/mp4',
                                       chunksize=8 * 1024 * 1024, resumable=True))
        response = None
        while response is None:
            _, response = request.next_chunk(num_retries=0)
        return response['id']

    def metadata(self, video_id, snippet):
        self.service.videos().update(part='snippet',
            body={'id': video_id, 'snippet': snippet}).execute(num_retries=0)

    def thumbnail(self, video_id, v):
        from googleapiclient.http import MediaFileUpload
        self.service.thumbnails().set(videoId=video_id,
            media_body=MediaFileUpload(str(asset(v['thumbnail'])), mimetype='image/png')).execute(num_retries=0)

    def captions(self, video_id, v):
        from googleapiclient.http import MediaFileUpload
        name = 'mdEditor ' + v['language']
        tracks = self.service.captions().list(part='snippet', videoId=video_id).execute()['items']
        ours = [t for t in tracks if t['snippet']['name'] == name
                and t['snippet']['language'] == v['language']]
        if len(ours) > 1:
            raise RuntimeError('Multiple managed caption tracks; resolve in Studio.')
        media = MediaFileUpload(str(asset(v['captions'])), mimetype='application/octet-stream')
        if ours:
            self.service.captions().update(part='snippet', body={'id': ours[0]['id'],
                'snippet': {'isDraft': False}}, media_body=media).execute(num_retries=0)
        else:
            self.service.captions().insert(part='snippet', body={'snippet': {
                'videoId': video_id, 'language': v['language'], 'name': name,
                'isDraft': False}}, media_body=media).execute(num_retries=0)

    def privacy(self, video_id, video, target):
        allowed = ('privacyStatus', 'license', 'embeddable', 'publicStatsViewable',
                   'selfDeclaredMadeForKids', 'containsSyntheticMedia')
        status = {k: val for k, val in video['status'].items() if k in allowed}
        status['privacyStatus'] = target
        self.service.videos().update(part='status', body={'id': video_id,
                                    'status': status}).execute(num_retries=0)


def snippet(v):
    return {'title': v['title'], 'description': description(v)
            + '\nPublication: ' + v['key'] + ' ' + v['revision'],
            'categoryId': v['category_id'], 'defaultLanguage': v['language'], 'tags': v['tags']}


def metadata_sha(v):
    # Normalize text newlines so Windows recovery and Linux Actions agree.
    payload = json.dumps(v, sort_keys=True).encode() + description(v).encode()
    payload += asset(v['thumbnail']).read_bytes()
    payload += asset(v['captions']).read_text(encoding='utf-8-sig').encode()
    return hashlib.sha256(payload).hexdigest()


def sync(config, state, api, action, recover=None):
    if config['channel_id'] not in api.channel():
        raise RuntimeError('OAuth account does not own the configured channel.')
    for v in config['videos']:
        key = v['key'] + '/' + v['revision']
        digest = sha(asset(v['media']))
        record = state.data['videos'].get(key)
        if record and record['media_sha256'] != digest:
            raise RuntimeError('MP4 changed: increment revision before uploading.')
        if recover and recover[0] == key:
            if not record or record.get('id') or record['phase'] != 'upload-reserved':
                raise RuntimeError('Only an unresolved reservation can be recovered.')
            candidate = api.video(recover[1], config['channel_id'])
            marker = 'Publication: ' + v['key'] + ' ' + v['revision']
            if marker not in candidate['snippet']['description']:
                raise RuntimeError('Recovery video does not contain the release marker.')
            record.update(id=recover[1], phase='uploaded')
            state.save()
        if record and not record.get('id'):
            raise RuntimeError('Unresolved upload. Check Studio; recover the ID. Do not re-upload.')
        if not record:
            if action != 'sync':
                raise RuntimeError('Synchronize the non-public video before publishing.')
            # Reusing identical bytes at a new revision is almost certainly accidental.
            if any(r['media_sha256'] == digest for r in state.data['videos'].values()):
                raise RuntimeError('Identical MP4 already recorded; use its existing revision.')
            record = {'media_sha256': digest, 'id': None, 'phase': 'upload-reserved'}
            state.data['videos'][key] = record
            state.save()  # Persist before the first upload request, including on errors.
            record['id'] = api.upload(v, snippet(v))
            record['phase'] = 'uploaded'
            state.save()  # Persist the ID immediately, before metadata/caption operations.
        video_id = record['id']
        video = api.video(video_id, config['channel_id'])
        if action == 'sync':
            # Preserve unmanaged mutable snippet values; never touch privacy here.
            merged = {k: val for k, val in video['snippet'].items()
                      if k in ('title', 'description', 'categoryId', 'tags',
                               'defaultLanguage', 'defaultAudioLanguage')}
            merged.update(snippet(v))
            api.metadata(video_id, merged)
            api.thumbnail(video_id, v)
            api.captions(video_id, v)
            record['phase'] = 'ready'
            record['metadata_sha256'] = metadata_sha(v)
            state.save()
        elif action == 'publish':
            expected = metadata_sha(v)
            if record['phase'] not in ('ready', 'published') or record.get('metadata_sha256') != expected:
                raise RuntimeError('Assets changed or incomplete; run sync again first.')
            if video.get('processingDetails', {}).get('processingStatus') != 'succeeded':
                raise RuntimeError('YouTube processing not complete. Retry publish later.')
            api.privacy(video_id, video, v['target_privacy'])
            # Confirm actual privacy; do not assume the API/project allows public visibility.
            verified = api.video(video_id, config['channel_id'])
            if verified['status']['privacyStatus'] != v['target_privacy']:
                raise RuntimeError('Visibility differs from requested setting; check Studio/API project.')
            record['phase'] = 'published'
            if v['target_privacy'] == 'public':
                previous = state.data['latest'].get(v['language'])
                if previous and previous['id'] != video_id:
                    record['previous_id'] = previous['id']
                state.data['latest'][v['language']] = {
                    'id': video_id, 'url': 'https://www.youtube.com/watch?v=' + video_id,
                    'revision': v['revision'], 'title': v['title']}
            state.save()
            if v['target_privacy'] == 'public' and record.get('previous_id'):
                old = api.video(record['previous_id'], config['channel_id'])
                old_snippet = {k: val for k, val in old['snippet'].items()
                               if k in ('title', 'description', 'categoryId', 'tags',
                                        'defaultLanguage', 'defaultAudioLanguage')}
                old_description = re.sub(r'\nLatest demo: https://www.youtube.com/watch\?v=[A-Za-z0-9_-]+',
                                         '', old_snippet.get('description', ''))
                old_snippet['description'] = old_description + '\nLatest demo: https://www.youtube.com/watch?v=' + video_id
                if len(old_snippet['description']) > 5000:
                    raise RuntimeError('Old description too long; add latest link in Studio.')
                api.metadata(record['previous_id'], old_snippet)
                # Retry is safe: old video is retained and its title/privacy never change.
        print(v['key'] + ': https://www.youtube.com/watch?v=' + video_id + ' (' + record['phase'] + ')')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['plan', 'sync', 'publish'])
    parser.add_argument('--recover-key')
    parser.add_argument('--recover-id')
    args = parser.parse_args()
    config = load_config()
    if args.action == 'plan':
        for v in config['videos']:
            print(v['key'], v['revision'], sha(asset(v['media'])), v['target_privacy'])
        print('enabled:', config['enabled'])
        return
    if not config['enabled']:
        raise RuntimeError('Release disabled. Complete OAuth setup before enabling release.json.')
    if bool(args.recover_key) != bool(args.recover_id):
        raise ValueError('Supply both recovery key and video ID.')
    state = GitState()
    recovery = (args.recover_key, args.recover_id) if args.recover_key else None
    sync(config, state, YouTube(), args.action, recovery)
    runtime = ROOT / '.youtube-runtime'
    runtime.mkdir(exist_ok=True)
    (runtime / 'state.json').write_text(json.dumps(state.data, ensure_ascii=False, indent=2), encoding='utf-8')


if __name__ == '__main__':
    try:
        main()
    except Exception as error:
        # Google exception strings can contain request URLs. Log only a safe category.
        if isinstance(error, (ValueError, RuntimeError)):
            print(str(error), file=sys.stderr)
        else:
            print('YouTube operation failed (' + type(error).__name__ + '). Check Studio and the durable state; credentials are not logged.', file=sys.stderr)
        sys.exit(1)
