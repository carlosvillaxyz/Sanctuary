# NPC spawns from live Free Realms captures

The original Free Realms servers were authoritative for NPC placement — NPC spawn
positions exist **nowhere** in the client packs and were never reproduced by the
Sanctuary emulator. We recovered them by decoding **authentic live packet captures**
(`captures/`, taken against the real `64.37.170.6` game servers in 2010-era sessions)
and extracting every `AddNpc` world packet.

## How it works

The pipeline is three zero-dependency Python tools (`tools/`):

1. **`pcap_io.py`** — minimal pcap + pcapng reader. Yields UDP datagrams
   `(ts, src_ip, sport, dst_ip, dport, payload)`. No `tshark`/`scapy`/`dpkt` needed.

2. **`soe_decode.py`** — the SOE/UdpLibrary deframer (mirrors Sanctuary's
   `UdpConnection.cs` / `UdpReliableChannel.cs`). For each server→client UDP datagram it:
   - strips the trailing **2 CRC bytes** (negotiated `CrcBytes=2`);
   - applies the FR **`UserSupplied` transform — which is zlib SOFT-COMPRESSION, not a
     cipher**: the region after the preserved header byte(s) is `[flag u8][payload]`,
     `flag==1 ⇒ zlib`. (This confirms the gateway uses **no RC4**.);
   - walks the cooked SOE packet: `Multi(3)`, `Group(25)`, `ZeroEscape(0)`, `Ordered(26/27)`,
     and reassembles the 4 reliable channels (`Reliable1‑4`=9‑12, `Fragment1‑4`=13‑16,
     first fragment carries a 4‑byte BE total length).
   - **Mid‑session captures** are handled by lazily seeding each reliable channel's
     expected sequence id from the first packet seen (several captures start after the
     SOE handshake).
   - Emits the ordered **gateway app‑packet** stream (`[u16 LE opcode][body]`).

3. **`decode_npcs.py`** — unwraps the gateway **op‑5 tunnel carrier**
   (`[u16=5][u8 flag][u32 len][world payload]`), filters world packet
   **op 35 (`BasePlayerUpdate`) / sub‑opcode 2 (`AddNpc`)**, and decodes each one.

   Captures may contain several SOE sessions on different ports; each UDP 4‑tuple is
   deframed by its **own** deframer so reliable sequence numbers never cross sessions.

## AddNpc payload layout (confirmed against the wire)

After `[u16 op=35][u16 sub=2]`:

| field | type | notes |
| --- | --- | --- |
| Guid | u64 | high bits encode entity type |
| NameId | i32 | string‑table id (not a literal name) |
| ModelId | i32 | |
| Unknown | u8 | |
| ChatBubbleFg/Bg/Size | 3 × i32 | |
| Scale | f32 | |
| **Position** | Vec4 (4 × f32) | `X,Y,Z,W` — `W` is the homogeneous coord, always `1.0` |
| **Rotation** | Quat (4 × f32) | yaw‑only `(sin h, 0, cos h, 0)` ⇒ `heading = atan2(rx, rz)` |
| Animation | i32 | |
| … | | attachments / collections follow (not needed for spawns) |

`W==1.0` on every decoded row doubles as a self‑check that field alignment is correct.

## Output (`data/`)

| File | What it is |
| --- | --- |
| `npc_spawns.json` | raw per‑capture `AddNpc` rows (includes interest‑management duplicates) |
| `npc_spawns_unique.json` | spawns deduped by `(guid, x, y, z)`, with the list of source captures |
| `npc_spawns_unique.csv` | same, flat CSV |

**~3,500 unique NPC spawns** recovered across ~454 distinct models and ~637 name ids,
spanning several FabledRealms zones plus the racing/derby/soccer minigame areas.

## Zone / area labelling

The overworld is the single seamless zone **`FabledRealms`** (confirmed by the op43
`ZoneDetails` packet in the session‑start captures — the only zone name on the wire).
Recognisable sub‑locations are *areas* defined spatially in the client's
`custom/FabledRealmsAreas.xml` (1,279 sphere/box volumes; a copy lives at
`data/FabledRealmsAreas.xml`).

`tools/label_areas.py` assigns each spawn the area volume that contains it (**99%** land
inside one) and derives a macro **region** from the area‑name prefix, biased toward the
populous "real place" prefixes so incidental ambiance bubbles don't win. Output:
`data/npc_spawns_labeled.{json,csv}`. Region breakdown:

| region | spawns | region | spawns |
| --- | ---: | --- | ---: |
| snowhill | 2393 | racetrack | 75 |
| thewild | 563 | soccercrowd/field | 76 |
| sanctuary | 233 | hotspring | 16 |
| desert | 151 | misc (mine, blackspore, …) | ~14 |

## Map

`tools/render_map.py` draws a zero‑dependency SVG of every spawn coloured by region,
with the 3600–3699 "creature" model family outlined in red to highlight the wild mob
grounds. Output: `data/npc_map.svg` (render to PNG with
`rsvg-convert -o npc_map.png npc_map.svg`).

**Orientation.** Free Realms' in-game compass is **North = world +X**, **East = world +Z**
(derived from the cardinal region layout — Snowhill N / Briarwood E / Seaside S /
Blackspore W — against the area coordinates in `FabledRealmsAreas.xml`). The renderer
therefore plots screen-up = +X and screen-right = +Z, which puts Snowhill in the north as
expected. Note that FR's macro-regions are **not** arranged on a clean orthogonal cross
(e.g. Briarwood "east" and Blackspore "west" sit only ~78° apart as seen from the world
center), so this is a best-fit compass rather than an exact axis alignment.

Coverage is **pocketed (~6.5% of the bounding box)** — captures follow the players' paths,
so towns/hubs are dense but there is no blanket map coverage. Combat is identifiable:
mixed‑species packs of 3600‑range creature models cluster at wilderness hotspots
(`(-100,200)`, `(-1500,400)`, `(-800,960)`), distinct from the unique town NPCs.

## Regenerating

```sh
cd tools && python3 decode_npcs.py    # captures -> data/npc_spawns*.json/csv
python3 label_areas.py                # + region/area labels
python3 render_map.py                 # + data/npc_map.svg
```

## Still open

- `NameId`/`ModelId` are string‑table / asset ids. Mapping them to human‑readable
  names requires the client string tables (`*.dir`/locale packs) — a future pass.
- Per‑spawn zone attribution currently comes from which capture saw it; tying each
  capture to its `ZoneDetails` (op 43) name would label spawns by zone explicitly.
