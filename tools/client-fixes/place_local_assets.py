#!/usr/bin/env python3
"""Put game assets that the community streaming server is missing where the client will find them.

The client streams most assets from an HTTP server (tools/asset-server). The community copy of that server is
missing a few files, so some screens draw a fallback. This tool fixes them once, idempotently:

  1. try the streaming server (the real file, if it ever reappears);
  2. otherwise build it from the game's own local files (documented per asset below).

Each result is written twice: as a loose file in client/ (loose files override packed assets) and as an
asset-server override in asset-cache/_overrides/ (served in place of the missing streamed file).

  icon_create_eye_color_bg_mask_64.dds
      Character-creation eye-colour swatch tint mask. Missing from the community streaming server (listed in its
      manifest, absent from every directory), so eye swatches render grey. Fallback: the game's own 32 px version
      of the same mask (Assets_008.pack), scaled to 64 px and saved as DXT5 like the original. Replace with the
      real file if one turns up (other client dumps; ask the OSFR Discord).

  icon_create_hair_*_fg_mask_64.dds (11 files)
      Character-creation hair-style icon tint masks, missing the same way, so those styles' icons ignore the hair
      colour and render white. Same fallback: scale the game's own 32 px mask.

  icon_create_hair_hm_bald_64.dds
      The bald hair-style icon itself (not a mask) is missing from the server. Scaled from the 32 px version.

  A full audit of every character-creation swatch image is in research/character-creation-assets.md.

Usage:  python tools/client-fixes/place_local_assets.py   (needs Pillow for the fallback: pip install Pillow)
"""
import concurrent.futures
import glob
import io
import pathlib
import struct
import sys
import urllib.error
import urllib.request
import zlib

UPSTREAM = "http://opensourcefreerealms.com/assets"
HERE = pathlib.Path(__file__).resolve().parent
REPO = HERE.parent.parent
CLIENT = REPO / "client"
CACHE = REPO / "asset-cache"
OVERRIDES = CACHE / "_overrides"
MAGIC = b"\xa1\xb2\xc3\xd4"

# asset name -> (fallback source in the local packs, size to scale to, DDS pixel format)
ASSETS = {
    "icon_create_eye_color_bg_mask_64.dds": ("icon_create_eye_color_bg_mask_32.dds", (64, 64), "DXT5"),
    "icon_create_hair_hm_bald_64.dds": ("icon_create_hair_hm_bald_32.dds", (64, 64), "DXT5"),
}

# The hair-style icon tint masks that are missing from the streaming server, all fixed the same way.
for _style in ("hf_curly", "hf_pigtail", "hf_ragged", "hm_afro", "hm_dreads", "hm_forward",
               "pf_frosty", "pf_jetson", "pf_mane", "pf_sassy", "pf_silky"):
    ASSETS[f"icon_create_hair_{_style}_fg_mask_64.dds"] = (f"icon_create_hair_{_style}_fg_mask_32.dds", (64, 64), "DXT5")


def fetch(url):
    req = urllib.request.Request(url, headers={"User-Agent": "Sanctuary client-fixes"})
    with urllib.request.urlopen(req, timeout=30) as r:
        return r.read()


def manifest():
    for base in ("http://127.0.0.1:20050", UPSTREAM):
        try:
            raw = fetch(f"{base}/manifest.txt.z")
            data = zlib.decompress(raw[8:]) if raw[:4] == MAGIC else zlib.decompress(raw)
            return {p[0]: p[1] for p in (l.split(",") for l in data.decode("latin1").splitlines()) if len(p) == 3}
        except Exception:
            continue
    return {}


def from_streaming_server(name, crc):
    def probe(d):
        req = urllib.request.Request(f"{UPSTREAM}/{d:03d}/{name}?{crc}", method="HEAD",
                                     headers={"User-Agent": "Sanctuary client-fixes"})
        try:
            with urllib.request.urlopen(req, timeout=20) as r:
                return d if r.status == 200 else None
        except Exception:
            return None

    with concurrent.futures.ThreadPoolExecutor(48) as ex:
        for d in ex.map(probe, range(1000)):
            if d is not None:
                raw = fetch(f"{UPSTREAM}/{d:03d}/{name}?{crc}")
                return zlib.decompress(raw[8:]) if raw[:4] == MAGIC else raw
    return None


def from_local_packs(name):
    """Read a file out of client/Assets_*.pack (chained big-endian groups, per soir20/oxide)."""
    for pack in sorted(glob.glob(str(CLIENT / "Assets_*.pack"))):
        d = open(pack, "rb").read()
        pos = 0
        while True:
            nxt, count = struct.unpack_from(">II", d, pos)
            p = pos + 8
            for _ in range(count):
                (nl,) = struct.unpack_from(">I", d, p); p += 4
                entry = d[p:p + nl].decode("latin1"); p += nl
                off, size, _crc = struct.unpack_from(">III", d, p); p += 12
                if entry == name:
                    return d[off:off + size]
            if nxt == 0:
                break
            pos = nxt
    return None


def scaled(source_bytes, size, pixel_format):
    from PIL import Image  # only needed for the fallback
    image = Image.open(io.BytesIO(source_bytes)).convert("RGBA").resize(size, Image.Resampling.BICUBIC)
    out = io.BytesIO()
    image.save(out, "DDS", pixel_format=pixel_format)
    return out.getvalue()


def framed(data):
    """The streaming format: 8-byte header (magic + big-endian uncompressed size) + zlib stream."""
    return MAGIC + struct.pack(">I", len(data)) + zlib.compress(data, 6)


# --- Data tweak: skin-tone swatch colours -------------------------------------------------------------------
# Sony's CharacterCreateCustomizationSkinTone.txt reuses the ICON_TINT column for the skin-tone NUMBER (1-6), and
# the client tints the swatch with it, so all six land on Tints.xml ids 1-6 ("solidWhite", "human_2".."human_6"),
# a gold ramp that looks nothing like the skin it selects. The character itself is unaffected: its skin comes from
# a texture, not this tint. This is the only deliberate change to the original data here, and it is cosmetic:
# new tint entries with a skin ramp, and the swatches pointed at them. Delete the block and restore the .orig
# file to go back. See research/character-creation-assets.md.
SKIN_TINT_BASE_ID = 900
SKIN_TONE_COLOURS = {
    1: ("skinswatch_ivory", 0.97, 0.88, 0.80),
    2: ("skinswatch_pearl", 0.93, 0.80, 0.69),
    3: ("skinswatch_gold", 0.87, 0.71, 0.55),
    4: ("skinswatch_bronze", 0.76, 0.58, 0.42),
    5: ("skinswatch_copper", 0.62, 0.44, 0.31),
    6: ("skinswatch_mahogany", 0.46, 0.31, 0.22),
}


def fix_skin_tone_swatches():
    tints = CLIENT / "Resources" / "Tints.xml"
    table = CLIENT / "Resources" / "CharacterCreateCustomizationSkinTone.txt"

    if not tints.exists() or not table.exists():
        print("client-fixes: skin-tone files not found, skipped")
        return

    text = tints.read_text(encoding="latin1")
    if "skinswatch_ivory" not in text:
        rows = "".join(
            f'    <TintVal id="{SKIN_TINT_BASE_ID + tone - 1}" name="{name}" r="{r:.6f}" g="{g:.6f}" b="{b:.6f}" />\n'
            for tone, (name, r, g, b) in sorted(SKIN_TONE_COLOURS.items()))
        tints.write_text(text.replace("</Tints>", rows + "</Tints>"), encoding="latin1")
        print(f"client-fixes: added {len(SKIN_TONE_COLOURS)} skin-swatch tints to Tints.xml")

    lines = table.read_text(encoding="latin1").splitlines(keepends=True)
    changed = False
    out = []
    for line in lines:
        parts = line.rstrip("\r\n").split("^")
        if len(parts) > 2 and parts[0].isdigit() and parts[2].isdigit():
            tone = int(parts[2])
            if tone in SKIN_TONE_COLOURS:
                new = str(SKIN_TINT_BASE_ID + tone - 1)
                if parts[2] != new:
                    parts[2] = new
                    changed = True
                    line = "^".join(parts) + ("\r\n" if line.endswith("\r\n") else "\n")
        out.append(line)

    if changed:
        backup = table.with_suffix(table.suffix + ".orig")
        if not backup.exists():
            backup.write_bytes(table.read_bytes())
        table.write_text("".join(out), encoding="latin1")
        print("client-fixes: skin-tone swatches now use the skin ramp (original saved as .orig)")


def main():
    if not CLIENT.exists():
        raise SystemExit(f"client folder not found: {CLIENT}")

    fix_skin_tone_swatches()
    todo = [a for a in ASSETS if not (CLIENT / a).exists() or not (OVERRIDES / (a + ".z")).exists()]
    if not todo:
        print("client-fixes: all assets in place")
        return
    names = manifest()
    for asset in todo:
        data, how = None, ""
        if (CLIENT / asset).exists():
            data, how = (CLIENT / asset).read_bytes(), "existing loose file"
        else:
            streamed = asset + ".z" if asset + ".z" in names else asset
            if streamed in names:
                data = from_streaming_server(streamed, names[streamed])
                how = "streaming server"
            if data is None:
                source, size, fmt = ASSETS[asset]
                src = from_local_packs(source)
                if src is None:
                    print(f"client-fixes: {asset}: fallback source {source} not in local packs, skipped")
                    continue
                data, how = scaled(src, size, fmt), f"scaled from local {source}"
        (CLIENT / asset).write_bytes(data)
        OVERRIDES.mkdir(parents=True, exist_ok=True)
        (OVERRIDES / (asset + ".z")).write_bytes(framed(data))
        print(f"client-fixes: placed {asset} ({len(data)} bytes, {how})")


if __name__ == "__main__":
    sys.exit(main())
