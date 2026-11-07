"""ファイル名 / File: test_redate.py
概要 / Description: 紹介ページの日付の置き換えが、フォルダー名の中の日付（リンク先）を壊さないことを確かめる例集
作成者 / Author: 5garashi.com設計事務所 / 5garashi.com Design Office
作成日 / Created: 2026-11-07
版 / Version: 1.0.0
ライセンス / License: MIT
SPDX-License-Identifier: MIT
"""
import prepare
# 2026-11-01 の一括置き換えで、resources-zh-CN.html の PDF へのリンクが存在しないフォルダーを指した（直したもの）。
CASES = [
    ('最終更新 2026-11-01', '最終更新 2026-11-07'),
    ('multichannel/update-2026-11-01/one-page-zh-CN.pdf', 'multichannel/update-2026-11-01/one-page-zh-CN.pdf'),
    ('Version 1.14.0, 2026-11-01, update-2026-11-01/x.png', 'Version 1.15.0, 2026-11-07, update-2026-11-01/x.png'),
]
def test_redate():
    for given, expected in CASES:
        assert prepare.redate(given) == expected, given
if __name__ == '__main__':
    test_redate(); print('ok', len(CASES))
