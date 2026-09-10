#!/usr/bin/env python3
"""Local Free Realms asset-delivery server.

Serves the client's streamed assets (manifest.crc, manifest.txt.z, /NNN/<name>.z?<crc>)
from a local cache directory. On a cache miss it fetches the file from an upstream asset
server, stores it, and serves it -- so the cache fills itself as you play, and a full
mirror (see mirror.py) makes it fully offline.

Overrides: any file placed in <cache>/_overrides/<name> is served instead of the cached
original. That is how custom loading screens, UI art, textures, etc. get into the client
without touching the exe or the pack files.

Usage:  python server.py [--port 20050] [--cache ../../asset-cache] [--upstream http://opensourcefreerealms.com/assets]
Launch the client with:  AssetDelivery:IndirectServerAddress=http://127.0.0.1:20050
"""
import argparse, logging, sys, threading, urllib.request, urllib.error, zlib, struct
from http.server import ThreadingHTTPServer, BaseHTTPRequestHandler
from pathlib import Path
from urllib.parse import urlsplit, unquote

log = logging.getLogger("asset-server")

class Handler(BaseHTTPRequestHandler):
    protocol_version = "HTTP/1.1"
    cache: Path; upstream: str; overrides: Path; index: Path

    def log_message(self, fmt, *args):  # quieter default logging
        log.debug(fmt, *args)

    def do_GET(self):
        parts = urlsplit(self.path)
        rel = unquote(parts.path.lstrip("/"))
        if not rel or ".." in rel.split("/"):
            return self._send(400, b"bad path")
        name = rel.rsplit("/", 1)[-1]

        override = self.overrides / name
        if override.is_file():
            log.info("OVERRIDE %s", name)
            return self._send_file(override)

        local = self.cache / rel
        if local.is_file():
            return self._send_file(local)

        # cache miss -> upstream
        url = f"{self.upstream}/{rel}" + (f"?{parts.query}" if parts.query else "")
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "Sanctuary asset-server"})
            with urllib.request.urlopen(req, timeout=60) as r:
                data = r.read()
        except urllib.error.HTTPError as e:
            log.warning("MISS %s -> upstream %s", rel, e.code)
            return self._send(e.code, b"")
        except Exception as e:
            log.error("MISS %s -> upstream error %s", rel, e)
            return self._send(502, b"")
        local.parent.mkdir(parents=True, exist_ok=True)
        local.write_bytes(data)
        with self.index.open("a", encoding="utf-8") as f:   # remember which dir each asset lives in
            f.write(f"{rel}\n")
        log.info("FETCHED %s (%d bytes)", rel, len(data))
        self._send(200, data)

    def _send_file(self, p: Path):
        self._send(200, p.read_bytes())

    def _send(self, code, body):
        self.send_response(code)
        self.send_header("Content-Type", "application/octet-stream")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

def main():
    here = Path(__file__).resolve().parent
    ap = argparse.ArgumentParser()
    ap.add_argument("--port", type=int, default=20050)
    ap.add_argument("--cache", default=str(here.parent.parent / "asset-cache"))
    ap.add_argument("--upstream", default="http://opensourcefreerealms.com/assets")
    ap.add_argument("-v", action="store_true")
    a = ap.parse_args()
    logging.basicConfig(level=logging.DEBUG if a.v else logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    Handler.cache = Path(a.cache); Handler.upstream = a.upstream.rstrip("/")
    Handler.overrides = Handler.cache / "_overrides"; Handler.index = Handler.cache / "_index.txt"
    Handler.cache.mkdir(parents=True, exist_ok=True); Handler.overrides.mkdir(exist_ok=True)
    srv = ThreadingHTTPServer(("127.0.0.1", a.port), Handler)
    log.info("asset server on http://127.0.0.1:%d  cache=%s  upstream=%s", a.port, Handler.cache, Handler.upstream)
    srv.serve_forever()

if __name__ == "__main__":
    main()
