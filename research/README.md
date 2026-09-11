# Research: what survived of Free Realms, and where

Working notes for rebuilding the server side of Free Realms (SOE, 2009-2014). Compiled 2026-09-10.
Plan that acts on this: [../docs/ROADMAP.md](../docs/ROADMAP.md). Current status:
[../docs/HANDOFF-2026-09-10.md](../docs/HANDOFF-2026-09-10.md).
`wiki/` holds raw wikitext pulled from the Free Realms Fandom wiki via its MediaWiki API
(`https://freerealms.fandom.com/api.php?action=parse&page=<Title>&prop=wikitext`), CC-BY-SA.

## What is recoverable, and from where

| Layer | Status | Source |
|---|---|---|
| Client (v1.910, final 2014 build) + ~3.8 GiB streamed assets | Complete | community CDN (opensourcefreerealms.com); our `tools/asset-server` caches it |
| Item table, models, stores, bundles, collections, POIs, house zones | Complete, already JSON | Sanctuary `src/Resources/*.json`; FabledRealms `data/items.json` (27 MB) |
| All in-game strings (item names, NPC names, quest text, e.g. 8130 = "Welcome to Free Realms!") | **Extracted**: `tools/locale/extract_strings.py` -> `strings/en_us.json` (85,932 strings, git-ignored, regenerable) | client `locale/en_us_data.dat`: `hash	tag	text`, hash = Jenkins lookup2 of `Global.Text.<id>` |
| Job list (17), unlock NPCs, trainer chains, per-level skills/traits for the 6 combat jobs, dungeon list + level tiers, combat rules | Complete as text | Fandom job pages -> `wiki/pages/` |
| Zone/town geography, warpstones, area volumes | Complete | client `custom/FabledRealmsAreas.xml` (1,279 volumes); FabledRealms `zone_areas.json`; `wiki/pages/Locations` etc. |
| NPC placements (guid, NameId, ModelId, position, heading) | 3,521 spawns, all named via the string table. Real towns: **Snowhill ~380** (the raw label says 2,393 but ~2,000 of those are house-instance furniture), Lakeshore 121, Sanctuary ~160, wilds, Sunstone, minigame arenas. **Zero** for Seaside, Merry Vale, Briarwood, Blackspore, Shrouded Glade, Wugachug, Cobblestone, Highroad | `yungcomputerchair/free-realms-re` (live packet captures from 2014-03-25/31, the game's final week — not 2010 as first assumed) -> `npc-spawns/` (`build_snowhill_roster.py`, rosters); analysis in `pilot-town.md` |
| Quest content with verbatim NPC dialogue | 59 quests transcribed -> `wiki/quests/` (the whole `Quest:` namespace). **Plus: the client string table holds the original text of far more quests** in contiguous id blocks (e.g. 36809-36837 = Noisy Neighbors / Noise Permit, word for word). Only the quest -> step -> NPC binding is lost | Fandom + `strings/en_us.json` |
| Packet catalogue | 343 packets / 63 families documented | `yungcomputerchair/free-realms-re` PACKETS.md + catalog.db |
| XP/star curve per level, coin/loot tables, mob stats & AI, dungeon scripts, minigame rules | **Lost.** Must be designed | none |
| Combat ability numbers | Community spreadsheet, not SOE data | Sanctuary `CombatAbilities.json` (cites "the sheet" from the OSFR Discord) |
| `.cdt`, `.dsk`, `.gr2` (Granny) format specs | No public docs | – |

## Starting area (supersedes the Snowhill pilot pick)

See `starting-area.md`. The final build (March 2013 on, "NPX 4.0 / A Hero Rises") started new players in
Darkthorne's Briarwood Caverns tutorial, then Farnum's Farm, then Cobblestone Village (Bartle's chain against Mac's
hooligans), then the Queen in Sanctuary. Sacred Glade -> Crossroads was the 2009 launch flow; 2011-2013 started in
Highroad Vale. Upstream's NPC layout (hand-placed by the community from the final client) fits the 2013 town.
`pilot-town.md` remains useful for Snowhill as a later target. `npc-dialogue.md` covers how NPC conversation worked
in the original client (a Flash window with choices, not bubbles; unvoiced) and how the server drives it.

## Key facts for design

- **Level cap 20 on every job.** Non-members were capped at 4 (Brawler/Blacksmith/Miner opened to 20 in 2012).
  Job XP = "Stars" from quests, minigames, kills, collections. Combat questlines gate at levels 5/10/15/20 with
  repeatable Contract quests from a fixed NPC per job.
- **Combat**: key 1 basic attack, 2 super (100 energy), 3 power-up. Status effects: sleep, silence, stun, root,
  knockback, confuse, poison. Knockout = 10 s wait + invulnerable revive; 10 KOs per wandering battle, 15 per dungeon.
- **The overworld is one seamless zone, `FabledRealms`.** Towns are area volumes, not zones. Dungeons and houses are
  separate instances (client `Resources/SKY/*.xml` is a de-facto instance list).
- **Starter flow**: tutorial zone Sacred Glade (later removed) -> Highroad Junction -> later Cobblestone Village.
  Adventurer is the default job; Brawler is unlocked in the tutorial.
- **Housing**: everyone got a free 1-room Apartment (75 items), later the Wilds Condo. Small Wilds House 549 SC,
  Large 750 SC, ~30 lots total. `Houses.json` already has spawn points and build-area boxes per house.
- **Economy**: Station Cash (real money) for the Marketplace, coins for the Coin Shop (clothes, weapons, pets,
  furniture, potions, recipes). Everything went member/free in Feb 2014 before the March 31 2014 sunset.

## Ecosystem (other projects worth borrowing from)

- `soir20/oxide` + `oxide-client`: Rust server for Clone Wars Adventures (same engine). Has an asset server and a
  client proxy that serves assets straight from pack files; also shows the client accepts `IndirectServerAddress=file://...`.
- `FabledRealmsProject/FabledRealms`: Rust land-walker with `items.json`, `npcs.json`, `journal.json` (quest journal regions/hubs/chapters), `zone_areas.json`.
- `Udaya-X2/FreeRealmsUnpacker` (no license, reference only): read/write `.pack`, `.dat`, repair `.pack.temp`.
  `Udaya-X2/FreeRealmsLocaleTools` (MIT): read/write `en_us_data.dat/.dir`; NameId = Jenkins lookup2 hash of the key.
- `mercish/TCG-Files` AuthBridge: a working Free Realms asset server in JS, incl. CRC/size overrides and generated
  placeholder DDS for assets missing from the community CDN (housing, minigames). Only FR-client-tested reference.
- Upstream Sanctuary open PRs worth cherry-picking: #109 quests, #120 XP/levels, #116 zoning, #111 housing editor.
- Client facts: loose files in the client folder override packed `.z` assets; `LoadingScreen.xml` is a weighted list
  of `loadingscreen*.swf` with tip StringIds; UI is 267 Scaleform window XMLs under `UI/UiModules/Main`.
- Sony's original asset URL was `http://fr.patch.station.sony.com/patch/freerealms/live/assets` (not on Wayback).
- `edenfps/fr-adr-toolkit`, `EDITzDev/ForgeLightToolkit` (Unity importer), `ryanjsims/pydmod` (DME -> glTF): model formats.
- `yungcomputerchair/free-realms-ai-decomp`: Ghidra pseudo-C of the whole client, one file per function.
- Prior emulators: Free Realms Reconnected (dead 2017; Daybreak DMCA'd the client mirror on GitHub in 2016 —
  do not commit client files), Free Realms Sunrise (closed source, alive), OSFR (archived, superseded by Sanctuary).

## Sources
Fandom wiki (pages listed in `wiki/`), tentonhammer.com Wizard guide, mmorpg.com Brawler thread, engadget 2009
job-system interview, Massively OP 2017/2019/2025 articles, GitHub repos named above.
