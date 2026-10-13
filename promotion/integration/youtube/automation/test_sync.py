"""File: test_sync.py
Description: Check duplicate prevention, recovery and publication boundaries without APIs.
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import copy
import tempfile
import unittest
from pathlib import Path
import sync


class Store:
    def __init__(self):
        self.data = {'videos': {}, 'latest': {}}
        self.saved = []
        self.fail = False

    def save(self):
        if self.fail:
            raise RuntimeError('State unavailable')
        self.saved.append(copy.deepcopy(self.data))


class API:
    def __init__(self, channel):
        self.owner = channel
        self.uploads = 0
        self.fail_upload = False
        self.fail_captions = False
        self.status = 'private'
        self.current_snippet = {'channelId': channel, 'description': 'Publication: demo-en 1.0.0'}
        self.metadata_calls = []

    def channel(self): return [self.owner]
    def upload(self, v, snippet):
        self.uploads += 1
        if self.fail_upload: raise RuntimeError('Upload response lost')
        return 'test-video-id'
    def video(self, video_id, channel):
        if channel != self.owner: raise RuntimeError('Wrong channel')
        return {'snippet': self.current_snippet, 'status': {'privacyStatus': self.status},
                'processingDetails': {'processingStatus': 'succeeded'}}
    def metadata(self, video_id, snippet):
        self.metadata_calls.append((video_id, dict(snippet)))
        self.current_snippet = dict(snippet)
    def thumbnail(self, video_id, v): pass
    def captions(self, video_id, v):
        if self.fail_captions: raise RuntimeError('Captions failed')
    def privacy(self, video_id, video, target): self.status = target


class SyncTests(unittest.TestCase):
    def setUp(self):
        self.config = sync.load_config()
        self.config['videos'] = [self.config['videos'][0]]
        self.config['videos'][0]['target_privacy'] = 'unlisted'
        self.store = Store()
        self.api = API(self.config['channel_id'])

    def test_retry_metadata_does_not_upload_again(self):
        self.api.fail_captions = True
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'sync')
        self.assertEqual(self.store.saved[-1]['videos']['demo-en/1.0.0']['id'], 'test-video-id')
        self.api.fail_captions = False
        sync.sync(self.config, self.store, self.api, 'sync')
        self.assertEqual(self.api.uploads, 1)
        self.assertEqual(self.api.status, 'private')

    def test_unknown_upload_blocks_retry(self):
        self.api.fail_upload = True
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'sync')
        with self.assertRaisesRegex(RuntimeError, 'Unresolved'):
            sync.sync(self.config, self.store, self.api, 'sync')
        self.assertEqual(self.api.uploads, 1)

    def test_failed_reservation_never_uploads(self):
        self.store.fail = True
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'sync')
        self.assertEqual(self.api.uploads, 0)

    def test_wrong_channel_never_uploads(self):
        self.api.owner = 'another-channel'
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'sync')
        self.assertEqual(self.api.uploads, 0)

    def test_publish_requires_complete_sync(self):
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'publish')
        self.assertEqual(self.api.uploads, 0)

    def test_unlisted_not_in_public_latest(self):
        sync.sync(self.config, self.store, self.api, 'sync')
        sync.sync(self.config, self.store, self.api, 'publish')
        self.assertEqual(self.api.status, 'unlisted')
        self.assertEqual(self.store.data['latest'], {})

    def test_public_latest_and_same_video(self):
        self.config['videos'][0]['target_privacy'] = 'public'
        sync.sync(self.config, self.store, self.api, 'sync')
        sync.sync(self.config, self.store, self.api, 'publish')
        self.assertEqual(self.store.data['latest']['en']['id'], 'test-video-id')
        self.assertEqual(self.api.uploads, 1)

    def test_changed_metadata_cannot_publish_without_sync(self):
        sync.sync(self.config, self.store, self.api, 'sync')
        self.config['videos'][0]['title'] = 'Changed title'
        with self.assertRaisesRegex(RuntimeError, 'Assets changed'):
            sync.sync(self.config, self.store, self.api, 'publish')

    def test_changed_media_requires_new_revision(self):
        sync.sync(self.config, self.store, self.api, 'sync')
        self.store.data['videos']['demo-en/1.0.0']['media_sha256'] = 'different'
        with self.assertRaisesRegex(RuntimeError, 'MP4 changed'):
            sync.sync(self.config, self.store, self.api, 'sync')

    def test_recover_unknown_upload_by_existing_id(self):
        self.api.fail_upload = True
        with self.assertRaises(RuntimeError): sync.sync(self.config, self.store, self.api, 'sync')
        sync.sync(self.config, self.store, self.api, 'sync', ('demo-en/1.0.0', 'recovered-video'))
        self.assertEqual(self.api.uploads, 1)
        self.assertEqual(self.store.data['videos']['demo-en/1.0.0']['id'], 'recovered-video')

    def test_management_header_not_in_uploaded_description(self):
        self.assertNotIn('SPDX', sync.snippet(self.config['videos'][0])['description'])

    def test_previous_video_gets_latest_link_and_is_not_deleted(self):
        self.config['videos'][0]['target_privacy'] = 'public'
        self.store.data['latest']['en'] = {'id': 'previous-id'}
        sync.sync(self.config, self.store, self.api, 'sync')
        sync.sync(self.config, self.store, self.api, 'publish')
        old_id, text = self.api.metadata_calls[-1]
        self.assertEqual(old_id, 'previous-id')
        self.assertIn('Latest demo: https://www.youtube.com/watch?v=test-video-id', text['description'])

    def test_generated_latest_links_exclude_external_state_ids(self):
        import latest_pages
        import json
        with tempfile.TemporaryDirectory() as directory:
            p = Path(directory)
            state = p / 'state.json'
            state.write_text(json.dumps({'latest': {'en': {'id': 'invalid<script>'}}}))
            with self.assertRaises(ValueError): latest_pages.generate_latest(p, state)

    def test_generated_latest_page_and_landing_link(self):
        import latest_pages
        import json
        with tempfile.TemporaryDirectory() as directory:
            p = Path(directory)
            (p/'en.html').write_text('<header><nav></nav></header>')
            state = p/'state.json'
            state.write_text(json.dumps({'latest': {'en': {'id': 'AbCdEf12345',
                'title': 'Demo <title>', 'revision': '1.0.0'}}}))
            latest_pages.generate_latest(p, state)
            page = (p/'youtube-en.html').read_text(encoding='utf-8')
            self.assertIn('watch?v=AbCdEf12345', page)
            self.assertIn('Demo &lt;title&gt;', page)
            self.assertIn('youtube-en.html', (p/'en.html').read_text())


if __name__ == '__main__':
    unittest.main()
