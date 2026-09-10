# Local asset-delivery server

Part of [this fork's plan](../../docs/ROADMAP.md); status in
[docs/HANDOFF-2026-09-10.md](../../docs/HANDOFF-2026-09-10.md).

The Free Realms client ships with ~470 MB of base pack files and **streams everything else**
(models, textures, animations, sounds, effects, UI art) over HTTP at runtime from an
"asset delivery" server named by the `AssetDelivery:IndirectServerAddress` launch argument.
Without one it exits at startup.

`server.py` is that server, running locally:

- Serves from `<repo>/asset-cache/` (git-ignored).
- On a miss it fetches from the upstream community server, caches, and serves. The cache
  fills itself as you play; the more you explore, the less you depend on upstream.
- **Overrides**: drop a file in `asset-cache/_overrides/<exact asset name>` and it is served
  instead of the original. This is how loading screens, UI textures, sounds, etc. get swapped
  without touching the exe or repacking anything. Assets the client received as `compressed=1`
  must be served in the same framing (see below); uncompressed ones (.ogg, .txt) are raw.
- `asset-cache/_index.txt` records every `NNN/name` path fetched, i.e. the real directory
  numbers, which we cannot compute yet (see open questions).

## Protocol (reverse-engineered 2026-09-10)

| Request | Response |
|---|---|
| `GET /manifest.crc` | text `"<crc32>,<size>"` of the uncompressed manifest.txt (standard reflected CRC-32) |
| `GET /manifest.txt.z` | framed+zlib manifest: 8-byte header `A1 B2 C3 D4` + big-endian uncompressed size, then zlib stream. Lines: `name[.z],crc32,compressed_size` (162,652 entries, 3.82 GiB total on the community server) |
| `GET /NNN/<name>.z?<crc32>` | the asset; `.z` files use the same 8-byte-header+zlib framing, others are raw |

`NNN` is a 3-digit directory derived from the asset somehow. Every community server
(this one, oxide, oxide-client) simply ignores it when serving. It only matters for pulling a
*complete* mirror from upstream, since upstream 404s without the right directory.

## Usage

```
python tools/asset-server/server.py            # port 20050, cache ../../asset-cache
python tools/asset-server/server.py --upstream http://osfr.editz.dev/assets
```
`run_local.ps1` starts it automatically; `run_client.py` points the client at it.

## Open questions / TODO

- Derive the `NNN` directory function so `mirror.py` can pull all 162k files offline.
  Tried: crc32/adler/djb2/sdbm/fnv/java/elf/oaat over name variants, reflected and
  non-reflected CRC-32, transforms of the manifest crc/size, content CRCs. No match.
  Best next step: ask the OSFR server operator on Discord how the `/NNN/` tree was generated,
  or Ghidra the `AssetDeliveryIndirect` request builder in FreeRealms.exe.
- Some manifest entries 404 on the community server (e.g. `hsg_*` housing items). Track
  which, and look for other mirrors.
