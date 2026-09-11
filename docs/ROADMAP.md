# Roadmap: a playable, single-developer Free Realms

**Status 2026-09-10:** Phase 0 substantially done — the game builds, runs and is playable locally with our
own asset server. See [HANDOFF-2026-09-10.md](HANDOFF-2026-09-10.md) for everything done so far and the
immediate next steps.

Goal: turn the community "walking emulator" into a real single-player-first game, faithful to the 2009-2014
experience where the data survives and our own design where it doesn't. AI-assisted throughout. Upstream
compatibility is not a goal; we track upstream only to cherry-pick fixes.

Guiding rules
- **Remaster, not reimagine.** The reference is the final live build (client v1.910, 2014) as players knew it.
  Keep what made the game the game: layout, NPCs, quests, jobs, pacing, tone. Change only (a) quality-of-life
  improvements and (b) the server-side gaps Sony took with them. Anything that alters the feel or the meta needs
  a stated reason. Model: the 2025 Oblivion remaster — improves what badly needed it, keeps the original intact.
- **The new-player path is the pilot.** Build the game in the order a new player met it: Sacred Glade tutorial,
  then Cobblestone Village (the final build's starting town), then outward along the roads. See
  `research/starting-area.md`.
- **Small vertical slices.** Every phase ends with something you can play in the client, not a framework.
- **Start in one town.** Everything gets built and proven in Cobblestone Village before scaling out.
- **Data over code.** Quests, dialogue, NPCs, drops live in JSON/Lua under `src/Resources` and `src/Scripts`,
  so content can be generated, reviewed and edited without touching C#.
- **Never redistribute the client.** Daybreak DMCA'd a GitHub client mirror in 2016. Our repo holds tools and data only.

## Phase 0 — Foundations (in progress)
- [x] Fork, .NET 10 toolchain, one-command launch (`run_local.ps1`)
- [x] Local asset-delivery server with cache + overrides (`tools/asset-server`)
- [x] Research dump: what data survives and where (`research/`)
- [ ] Derive the asset directory hash so `mirror.py` can pull the full 3.8 GiB offline (open; see asset-server README)
- [ ] Extract the client's locale strings (`en_us_data.dat`) to `research/strings.json` so every NameId resolves to text
      (`Udaya-X2/FreeRealmsLocaleTools`, MIT; NameIds are Jenkins lookup2 hashes, so we can add our own strings too)
- [ ] Review and cherry-pick upstream's open PRs into our fork: #109 quests (`Quests.json`, TalkToNpc/ReachLocation/Collect
      goals, quest log table), #120 XP/level-ups, #116 generic zoning, #111 housing editor. Months of work already drafted.
- [ ] Wire up a debug overlay / chat commands to print your position, nearest NPC guid and NameId (makes content work fast)

## Phase 1 — First impressions (cosmetic, no server code)
- [ ] Replace the Station Cash ad loading screens: the client picks from a weighted list in `LoadingScreen.xml`
      (`loadingscreen*.swf` + tip StringIds). Ours = new XML + our own screens (Claude Design / Stable Diffusion art),
      delivered as loose files in the client folder (loose files override packed assets) or asset-server overrides
- [ ] Strip the marketplace nag/member UI where it is a texture or Scaleform `.gfx` swap
- [ ] Custom login/character-create backdrops
Deliverable: launching the game no longer looks like a dead store.

## Phase 2 — The town lives (Cobblestone Village)
- [ ] Place NPCs from the recovered spawn dump where we have them; hand-place the rest from wiki descriptions
- [ ] NPC click-to-talk: an `Interaction` that opens a dialogue with options (server sends the dialogue packet family
      the client already understands; free-realms-re has the packet catalogue)
- [ ] Dialogue authoring format in JSON, Lua hooks for branching; generate first-draft lines with Claude from the
      wiki's verbatim quotes, edit by hand
- [ ] Three real quests end to end: talk -> objective (collect / kill / go to) -> turn-in -> reward (coins + item)
      using the surviving `journal.json` structure for the quest log
- [ ] Voice lines for the handful of town NPCs (ElevenLabs), served as `.mp3` overrides
Deliverable: a stranger can walk into town, talk to people, and finish a quest chain.

## Phase 3 — Progression
- [ ] Characters start at level 1, not 20 (`ProfileHelper.cs:73` and the profile/rank tables)
- [ ] Stars/XP curve of our own design (the real one is lost); XP from quests and kills; level-up flow
- [ ] Job unlock via trainer NPC dialogue (Brawler first; the wiki has its trainer chain and per-level traits)
- [ ] Abilities gated by level per the wiki tables; damage tuned from the community sheet as a baseline
Deliverable: level a Brawler 1 -> 5 in Cobblestone Village.

## Phase 4 — Things to fight
- [ ] Enemy spawns with respawn, aggro and simple AI in the wilds outside town
- [ ] Loot tables and coin drops (ours; the originals are lost)
- [ ] One dungeon instance (e.g. Robgoblin Trove) using the client's `SKY/*.xml` instance list
Deliverable: the Brawler questline to level 10 is completable.

## Phase 5 — Economy and housing without Station Cash
- [ ] Remove SC from every UI path the server controls; coins only
- [ ] Coin Shop rebuilt from `CoinStoreItems.json`
- [ ] Free starter house, working purchase/placement (`Houses.json` already has zones and build boxes;
      the housing packet handlers are stubs today)
Deliverable: buy furniture, decorate, invite a second character.

## Phase 6 — Scale out
- Sacred Glade tutorial as a separate instanced zone (terrain, gate assets and its voiced NPC lines all survive in
  the streamed assets; needs upstream PR #116 generic zoning)
- Repeat Phase 2-4 per town along the new-player roads: Highroad Junction / Stillwater Crossing (the other two
  post-tutorial exits) -> Sanctuary -> Snowhill (best recovered 2010 data) -> Seaside
- Minigames (kart, derby, soccer, TCG) last: they need the most reverse-engineering and matter least to "playable"

## Content pipeline (cross-cutting)
- **Text**: wiki dump + locale strings -> Claude drafts -> human edit -> JSON
- **Art**: Stable Diffusion (local GPU) for 2D (loading screens, UI, icons, textures); a Free Realms LoRA
  trained on client textures/screenshots is worth trying. 3D is hard (Granny `.gr2` has no open writer) —
  re-texture existing models first, new meshes much later
- **Audio**: your own work + AI for effects; ElevenLabs for NPC voice
- **All of it ships through `asset-cache/_overrides`**, so nothing touches the client install

## Open questions
- Asset directory hash (ask the OSFR server operator on Discord how the `/NNN/` tree was generated)
- Dialogue/quest packet layouts: free-realms-re `catalog.db` for (opcode, sub-opcode); ai-decomp `_sorted/client/journal`,
  `_sorted/game/quest`, `BaseQuestPacket` (op 49), `BaseObjectivePacket` (op 45), `BaseCombatPacket` (op 32) deserialisers;
  oxide `handlers/dialog.rs` for a working dialog design (Clone Wars Adventures, same engine)
- Licensing: Sanctuary/oxide/FabledRealms/fr-adr-toolkit are AGPL (compatible); FreeRealmsUnpacker, free-realms-re,
  ai-decomp, TCG-Files have NO license, so use them as reference only, do not copy code
- Whether the tutorial zone (Sacred Glade) assets still exist client-side (the `SKY/tutorial_02.xml` hint says yes)
