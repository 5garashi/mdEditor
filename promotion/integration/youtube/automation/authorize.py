"""File: authorize.py
Description: Obtain local user OAuth consent without logging tokens.
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import argparse
import os
from pathlib import Path


def main():
    from google_auth_oauthlib.flow import InstalledAppFlow
    parser = argparse.ArgumentParser()
    parser.add_argument('--client', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists():
        raise RuntimeError('Credential output already exists; choose a new filename.')
    flow = InstalledAppFlow.from_client_secrets_file(str(args.client),
        scopes=['https://www.googleapis.com/auth/youtube.force-ssl'])
    credentials = flow.run_local_server(port=0, access_type='offline', prompt='consent',
        authorization_prompt_message='Authorize the YouTube account in your browser.',
        success_message='Authorization completed. You can close this tab.')
    if not credentials.refresh_token:
        raise RuntimeError('No refresh token received; no credentials saved.')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    # Exclusive creation, no credentials on stdout. Use a private user-owned folder.
    fd = os.open(args.output, os.O_WRONLY | os.O_CREAT | os.O_EXCL, 0o600)
    with os.fdopen(fd, 'w', encoding='utf-8') as stream:
        stream.write(credentials.to_json())
    print('Credentials saved locally. Transfer with gh secret set; do not paste them into chat.')


if __name__ == '__main__':
    main()
