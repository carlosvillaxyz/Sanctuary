#!/usr/bin/env python3
"""Extract the Free Realms client string table to JSON: { "<string id>": "<text>", ... }.

The client's locale/<lang>_data.dat is a UTF-8 (BOM) text file, one entry per line:
    <key>\t<tag>\t<text>
where <key> is usually the Jenkins lookup2 hash of "Global.Text.<id>" (format documented by
Udaya-X2/FreeRealmsLocaleTools, MIT). Some entries carry the numeric id itself as the key. This script
recovers the id for every hashed entry by brute-forcing ids upward until all hashes are matched.

Usage:  python tools/locale/extract_strings.py [--client ../../client] [--lang en_us] [--out ../../research/strings/en_us.json]
Output is git-ignored (it is the game's own text); regenerate it with this script.
"""
import argparse, json, sys, time
from pathlib import Path

MASK = 0xFFFFFFFF

def lookup2(data: bytes, initval: int = 0) -> int:
    """Bob Jenkins' lookup2 hash (1996), 32-bit."""
    a = b = 0x9E3779B9
    c = initval
    length = len(data)
    i = 0
    def mix(a, b, c):
        a = (a - b - c) & MASK; a ^= (c >> 13)
        b = (b - c - a) & MASK; b ^= ((a << 8) & MASK)
        c = (c - a - b) & MASK; c ^= (b >> 13)
        a = (a - b - c) & MASK; a ^= (c >> 12)
        b = (b - c - a) & MASK; b ^= ((a << 16) & MASK)
        c = (c - a - b) & MASK; c ^= (b >> 5)
        a = (a - b - c) & MASK; a ^= (c >> 3)
        b = (b - c - a) & MASK; b ^= ((a << 10) & MASK)
        c = (c - a - b) & MASK; c ^= (b >> 15)
        return a, b, c
    while length - i >= 12:
        a = (a + int.from_bytes(data[i:i+4], 'little')) & MASK
        b = (b + int.from_bytes(data[i+4:i+8], 'little')) & MASK
        c = (c + int.from_bytes(data[i+8:i+12], 'little')) & MASK
        a, b, c = mix(a, b, c)
        i += 12
    c = (c + len(data)) & MASK
    rest = data[i:]
    n = len(rest)
    if n >= 1: a = (a + rest[0]) & MASK
    if n >= 2: a = (a + (rest[1] << 8)) & MASK
    if n >= 3: a = (a + (rest[2] << 16)) & MASK
    if n >= 4: a = (a + (rest[3] << 24)) & MASK
    if n >= 5: b = (b + rest[4]) & MASK
    if n >= 6: b = (b + (rest[5] << 8)) & MASK
    if n >= 7: b = (b + (rest[6] << 16)) & MASK
    if n >= 8: b = (b + (rest[7] << 24)) & MASK
    if n >= 9: c = (c + (rest[8] << 8)) & MASK
    if n >= 10: c = (c + (rest[9] << 16)) & MASK
    if n >= 11: c = (c + (rest[10] << 24)) & MASK
    a, b, c = mix(a, b, c)
    return c

def text_id_hash(string_id: int) -> int:
    return lookup2(f"Global.Text.{string_id}".encode("ascii"))

def read_dat(path: Path):
    """Yield (key:int, tag:str, text:str)."""
    with path.open(encoding="utf-8-sig", newline="") as f:
        for line in f:
            line = line.rstrip("\r\n")
            if not line:
                continue
            parts = line.split("\t", 2)
            if len(parts) != 3:
                continue
            try:
                yield int(parts[0]), parts[1], parts[2]
            except ValueError:
                continue

def main():
    here = Path(__file__).resolve().parent
    ap = argparse.ArgumentParser()
    ap.add_argument("--client", default=str(here.parent.parent / "client"))
    ap.add_argument("--lang", default="en_us")
    ap.add_argument("--out", default=None)
    ap.add_argument("--max-id", type=int, default=6_000_000)
    a = ap.parse_args()
    out = Path(a.out) if a.out else here.parent.parent / "research" / "strings" / f"{a.lang}.json"
    dat = Path(a.client) / "locale" / f"{a.lang}_data.dat"

    by_key = {}
    for key, tag, text in read_dat(dat):
        by_key[key] = (tag, text)
    print(f"{dat.name}: {len(by_key)} entries", flush=True)

    # Entries whose key is a small number are already plain ids (tag tells the client so); everything
    # else is a lookup2 hash of Global.Text.<id>. Brute-force ids until every hash is claimed.
    result = {}
    pending = set(by_key)
    t0 = time.time()
    for sid in range(0, a.max_id):
        h = text_id_hash(sid)
        if h in pending:
            result[str(sid)] = by_key[h][1]
            pending.discard(h)
            if not pending:
                break
        if sid % 500_000 == 0 and sid:
            print(f"  id {sid:,}: {len(result):,} mapped, {len(pending):,} pending ({time.time()-t0:.0f}s)", flush=True)
    # Whatever remains is keyed by its literal id.
    for key in pending:
        result[str(key)] = by_key[key][1]
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(result, ensure_ascii=False, indent=0, sort_keys=True), encoding="utf-8")
    print(f"wrote {out} ({len(result):,} strings; {len(pending):,} kept literal keys) in {time.time()-t0:.0f}s")

if __name__ == "__main__":
    main()
