"""
File: promotion/tools/serve.py
Description: Loopback preview server with byte ranges for MP4 seeking
Author: 5garashi.com Design Office
Created: 2026-10-13
License: MIT
SPDX-License-Identifier: MIT
"""
import argparse,functools,http.server,re
from pathlib import Path

class RangeHandler(http.server.SimpleHTTPRequestHandler):
    def log_message(self,*args):pass
    def send_head(self):
        path=Path(self.translate_path(self.path))
        if not path.is_file():return super().send_head()
        file=path.open('rb');size=path.stat().st_size
        start,end=0,size-1
        requested=self.headers.get('Range')
        if requested:
            match=re.fullmatch(r'bytes=(\d+)-(\d*)',requested)
            if match:
                start=int(match[1]);end=min(int(match[2]) if match[2] else end,end)
            else:
                file.close();self.send_error(416);return None
        if start>=size or end<start:
            file.close();self.send_error(416);return None
        self.send_response(206 if requested else 200)
        self.send_header('Content-Type',self.guess_type(str(path)))
        self.send_header('Accept-Ranges','bytes')
        self.send_header('Content-Length',str(end-start+1))
        if requested:self.send_header('Content-Range',f'bytes {start}-{end}/{size}')
        self.end_headers();file.seek(start);self.remaining=end-start+1
        return file
    def copyfile(self,source,outputfile):
        if not hasattr(self,'remaining'):return super().copyfile(source,outputfile)
        try:
            while self.remaining:
                data=source.read(min(65536,self.remaining))
                if not data:break
                outputfile.write(data);self.remaining-=len(data)
        except (BrokenPipeError,ConnectionAbortedError,ConnectionResetError):
            pass # A browser may cancel an old media request when seeking or navigating.

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--port',type=int,default=8765)
    args=parser.parse_args()
    root=Path(__file__).resolve().parents[1]
    with http.server.ThreadingHTTPServer(('127.0.0.1',args.port),functools.partial(RangeHandler,directory=str(root))) as server:
        print(f'http://127.0.0.1:{args.port}/',flush=True)
        try:server.serve_forever()
        except KeyboardInterrupt:pass
