# Plan: Cobblestone Village complete (NPX 4.0 new-player path)

Written 2026-09-11 on branch `quests` (@ `8e39fdd6`). Answers Carlos's two questions — "what's the smart move?" and
"why do NPCs feel dead, and what did the original do?" — and lays out play-testable slices. Inputs: `docs/ROADMAP.md`,
`docs/playtest-notes.md`, `research/starting-area.md` (decisive), `research/upstream-forks.md`, `research/npc-dialogue.md`
§F, `src/Resources/QUESTS.md`, the client string table (`research/strings/en_us.json`, sids cited inline), the
community layout (`src/Scripts/Zone/FabledRealms.lua` + `Npcs.json`), the Fandom dump (`research/wiki/`), a read-only
inspection of `sulphural/main`, and a fresh decode of three free-realms-re March-2014 pcaps for NPC movement/chat. Items marked **to verify** were not confirmed live or in code this session.

## 1. Recommendation

**Build Cobblestone Village and Farnum's Farm out first, and take the area's dungeons from Sulphural/main rather than
rebuilding them.** The reasons: (1) the quest chain is data — titles, objectives and nearly all lines for the NPX 4.0
chain are verbatim in the string table, and our quest system already renders them — so the town's remaining cost is
NPCs, enemies and instances, not authoring; (2) the two atlas dungeons the wiki puts next to Farnum's Farm, **Sheep
Watch** (difficulty 1) and **Highroad Hijinx** (difficulty 2, boss Mac, lieutenant Collin), already exist on
`sulphural/main` (`src/Sanctuary.Game/Dungeons/DungeonDefinition.cs` `DungeonCatalog`, activity 119/POI 87 and
45/68, entrances spawned in its `FabledRealms.lua` lines 4404/4440 — as procedural "defeat everything" layouts whose
rosters we author from the wiki), and the two hooligan gates already standing outside town, **Hooligan Bullies!**
(activity 137) and **Hooligan Brawling Club!** (112), are fully authored arenas there (4 brawlers, 2 archers, 1 boss),
along with the whole engine those need — `CombatNpc` aggro/leash/respawn, `EnemyTier`, death and revive, boss plates,
the encounter start panel, kill goals — all live-tested; (3) the one piece
nobody has, the **Cobblestone Showdown** instance (`sg_newbiezone_showdown`, boss Mac), is a small authored arena, and
Sulphural's `EncounterArenaZone` + `DungeonEscortStage` pattern is exactly the tool for authoring it. Rebuilding
combat/enemies/instances ourselves would be months to reach where that branch is now. The port must be subsystem by
subsystem (the branch conflicts in 167 files), in the order below, and each slice must end in something Carlos can
play. Everything invented (HP curve, loot, Showdown waves) is marked as ours, per "remaster, not reimagine".

## 2. Milestone: what "Cobblestone Village complete" means

A brand-new character starts at **Farnum's Farm** (Darkthorne's cavern deferred, see decision D4), and can play
**Cooking 101 -> Gear Up! -> Your First Weapon -> Fighting off the Pack -> One More Gloam -> Where's the Help? ->
Find and Pull the Plug -> Cobblestone Showdown -> Off to the Queen** to the palace door in Sanctuary, reaching
**Brawler level ~5** on the way, with:

- real hit points, energy and abilities that grow with level, gear gated by level;
- hooligans and wolves that aggro, hit back, knock you out and respawn; the Showdown as a real instance with Mac;
- Sheep Watch and Highroad Hijinx enterable from their gates, with the difficulty-gate and a reward;
- authentic coin rewards (ZAM/wiki), no placeholder items;
- a town that reads as 2013: NPCs greet you in bubbles when you walk up, the crash-scene crowd heckles Ricky, the
  teenagers gossip about Sheila, Bartle laments his horse until you bring her back, the odd idle animation and
  wanderer, the talk cursor on NPC bodies;
- no dev-only commands reachable by a player account; journal, tracker and dialogue camera behaving.

## 3. Work plan — play-testable slices, in order

Each slice: goal, borrow vs build (source), data, dependencies, Carlos's test, risk. "S/main" = `sulphural/main`.

### Phase A — fix what's in hand (days, not weeks)

**A1. Housekeeping bundle**
- Goal: dev commands admin-only; authentic rewards; open UI bugs.
- Build: set `RequiredRole => ChatCommandRole.Admin` on `ExperienceChatCommand`, `RewardChatCommand`, `WhereAmI`
  (keep `/help`); `Quests.json` 2563 -> 5 coins, 1801 -> 7 coins, add **Back for More** (94263, Ricky -> Samantha,
  2 coins), strip placeholder items; journal-opens-wrong-page and tracker-after-journal (compare `SelectQuest` /
  journal-state packets against S/main `QuestManager`, which is the live-tested ancestor); dialogue camera = tune the
  focus float after the escape flag in `CommandPacketShowDialog` (npc-dialogue.md §F).
- Data: ZAM rewards in starting-area.md §F/§A. Deps: none.
- Test: log in on a non-admin account, `!exp` refused; replay Introduce Yourself, get 5 coins; open journal, right page.
- Risk: low.

**A2. NPC interaction pack (S/main `25e43664` + `0ff3e8fc`, ~680 lines)**
- Goal: hover cursor on the NPC body, radial menu for vendor+quest NPCs, talk gesture while a line shows, proximity
  greeting bubbles. This is the first liveliness slice (see §4).
- Borrow: `25e43664` (14 files, +560: `Npc.InteractionProviders`, `Interactions/NpcInteractionOption.cs`,
  `ContextIcons.cs`, `QuestManager.GetInteractionOptions`, talk gesture `QuestDialogue.PlayTalkAnimation`, `HasCursor`
  for every interactable) and `0ff3e8fc` (3 files, +118: `Npc.AmbientLineIds` + `TryAmbientGreet`,
  `BaseZone.UpdateAmbientChatter` per-second sweep of each player's visible NPCs, 18 u proximity, 25 s per-NPC
  cooldown, `Npc.SayStringId(id, logged:false)` -> `ChatPacketFromStringId` with `IsChatLogged=false` = bubble only,
  verified live by its author). Its line table is hard-coded in `StartingZone.cs` (`NpcOwnLineIds`,
  `AmbientGreetingIds`); move it to `Npcs.json`/a JSON file when porting.
- Data: greeting sids per NPC (Samantha 94246, Ricky 94388, Bartle 20945/20943, hooligans 5100399, Jones/Jonelle/
  Simone from starting-area.md tables). Deps: none (touches `Npc.cs`, `QuestManager.cs`, `StartingZone.cs`).
- Test: hover Samantha's body -> talk cursor; walk up to Bartle -> "Sheila! They stole my poor Sheila!" bubble, nothing
  in chat log; click Roosey -> radial with Shop + (later) Talk.
- Risk: medium — 4 shared files; S/main `QuestManager` is 2,236 lines vs our 1,078, so port the diff, not the file.

### Phase B — foundations combat needs

**B1. Level scaling**
- Goal: HP, energy, ability unlocks and gear requirements follow job level (today: HP hard-coded 2500 in
  `StartingZone.cs:48,144`; energy 100 from `CombatJobs.json`; `MinProfileRank` present in `ClientItemDefinitions.json`
  but never read; abilities not gated).
- Build (ours; the real curve is lost): `Resources/LevelStats.json` `{level, maxHealth, maxEnergy}` fed into
  `ClientUpdatePacketHitpoints`/`Mana` and `CharacterStat` on zone-ready and on `OnLevelUp`; enforce `MinProfileRank`
  in the equip handler (client already greys the item); ability unlock levels per job from the wiki job pages
  (`research/wiki/pages/Brawler.wikitext` etc.) as an `UnlockLevel` on each `CombatAbilities.json` entry, checked
  in the toolbar builder (`Player.cs` ~L895). S/main already has both halves to copy: `Leveling/JobLeveling.cs`
  (`MaxHealth = 2500 + (level-1)*250`, `MaxMana = 100 + (level-1)*20`, regen 25+3/lvl and 4+1/lvl, applied in
  `Player.RecalculateStats`) and ability gating by rank (`*WeaponAbilities.HasTrait(player, level)` =
  `ActiveProfile.Rank >= traitLevel`, with `JobTraits.Build` sending the "Unlocked at level N" caption). Its numbers
  are JadenY's, not SOE's — keep them as the starting table and tune (D2). `MinProfileRank` is unenforced there too.
- Data: `RankLevels.json` (in), wiki job pages, community ability sheet. Deps: A1.
- Test: level-1 character shows ~level-1 HP on the HUD; `!exp` to 5 -> HP rises, a level-5 weapon becomes equippable,
  the level-5 ability appears on the bar.
- Risk: numbers are a design call (D2); the client may cache stats until a profile re-send (S/main fixed a
  "profile re-send wipes the toolbar" bug in `82ec8bf4`/`0b08378f` — take that fix with it).

**B2. Zoning and instances**
- Goal: enter a separate zone (arena/dungeon/tutorial) and come back to the overworld at the entrance.
- Borrow: S/main `ZoneManager.cs` (249 lines) + `Zones/CombatEncounterZone.cs` (750: knockout limit, fail, revive
  lifecycle) + `Zones/EncounterArenaZone.cs` (2,072) and `Player.TeleportToZone(zone, pos, rot, sky, geometryId)`
  (`Player.cs:1560`, saves `StartingZonePosition` for the way back) — the model its 42 dungeons already run on. Its
  instancing is **one shared zone per ActivityId, lazily created, never torn down** (a second player joins the
  running fight): fine for single-player, but a per-character instance and teardown is a small change worth making
  when porting. `Zones/CombatTutorialZone.cs` (328 lines, instanced `bw_briarheart_castle_interior`, op-45
  objective loop, entered via `/tutorial`) is the template for E1. Alternative: ImAlko PR #116
  (`WorldZone`/`HousingZone`/`CombatZone`, `TryGetOrCreateZoneInstance`/`EvictIfEmpty`, 4 conflicts, still
  changing under review) — see D1.
- Data: `Resources/Zones/<zone>.json` (sky, spawn, geometry name) for `sg_newbiezone_showdown` and the dungeon
  zones; assets must stream through `tools/asset-server` (the `/NNN/` directory problem: let the client request them
  once, or solve the hash — starting-area.md §5).
- Deps: none technically; do before anything else that touches `StartingZone.cs`.
- Test: `!zone` (admin) into an empty Showdown arena, walk around, exit back to the road west of Bartle.
- Risk: medium-high — asset fetch for zones never loaded before; two competing zone models (D1).

**B3. Combat and enemies (S/main combat pack; key commits `c34648df`, `7086796b`, `a126be4b`, `12049172`,
`00c1db34`, `325661be`)**
- Goal: hit, be hit, knock out, revive, respawn; kill XP into PR #120.
- Borrow: `Combat/*` (job kits, weapon abilities, status effects, power-ups, potions), `Entities/CombatNpc.cs`,
  `Entities/EnemyTier.cs`, death/respawn (revive at nearest warpstone), `Kill`/`EncounterComplete` quest goals
  (v2 stripped them; re-add to our `QuestGoal`). Reconcile with what we already have from upstream #121
  (`CombatJobs.json`, toolbar/energy in `Player.cs`) — keep #121's toolbar wiring, take S/main's damage engine.
  Optionally CarterW24 PR #76 (encounter/minigame packets, 0 conflicts) first as groundwork.
- Data: S/main's numbers are formulas, not tables — `CombatNpc.InitializeFromLevel`: `MaxHp = (350 + level*200) *
  tier`, damage `(20 + level*15) * tier`, XP `(25 + level*8) * tier`; `EnemyTiers.FromName` keyword tiers (Boss
  x5.5, Elite x2.7, Tough x1.9, Weak x0.55); every overworld enemy is `WorldEnemyLevel = 3`; aggro 15 u, leash 40,
  respawn 8 s (`StartingZone.RespawnWorldEnemy`). Any `Npcs.json` entry whose model is in
  `DungeonCatalog.EnemyModelIds` and is not a vendor/quest NPC spawns hostile (`StartingZone.IsWorldEnemyDefinition`),
  so the placed Hooligan Wolves (3156-3158, 4164-4166, name 5100200), Hooligans (4154-4155, 4159-4161) and Hooligan
  Archers (4157-4158) go hostile automatically. Move the formulas into `Resources/Enemies.json` per species (data over
  code) and give the newbie-zone mobs level 1-2, not 3. Route `AwardXp` through #120's API, not S/main's
  `JobLeveling`.
- Deps: B1 (HP must scale before enemies hit), B2 for anything instanced.
- Test: as a level-1 Brawler, fight the wolves at (-2100, -46, 460): they aggro at range, chase, leash back, you get
  knocked out and revive at the Cobblestone warpstone, a kill gives stars and levels you.
- Risk: high — the largest extraction (Combat ~5.6k lines, Zones ~13k); S/main's `Player.cs` differs heavily; must
  drop its own XP system and dev probes (`!lp`, `!abil`, `!fx`).

### Phase C — the NPX 4.0 content

**C1. Farnum's Farm chain (data + small scripts)**
- Goal: Cooking 101 / Farming 101 (Mae 2538), Gear Up! / Mine and Smelt, Your First Weapon, Fighting off the Pack
  (Cleetus 3232), One More Gloam and Where's the Help? (Pappy 3211) play in order; new characters spawn at the farm.
- Build: `Quests.json` entries from sids 4420050-4420095 / 5100268-5100324 (starting-area.md §F has every id);
  harvest/cook/mine/smelt as `Collect` goals on placed crop/ore/table props until crafting exists (D3); Your First
  Weapon grants the Spunky Scrapper Hammer on turn-in; Fighting off the Pack = `Kill` x5 Hooligan Wolf (5100203);
  One More Gloam = Collin's camp fight (4420085, Collin `human_m_collin.agr` Models 476, place at one of the outlying
  hooligan camps 4667-4675 — **position to verify from video**) + a Gloam Creature north of Wildwood (5100268;
  model **to verify**, Gloamling 5100374 is the fallback); the farm fires + water well (5100342-5100394) as a
  collect-and-return step. Move the new-character spawn to the farm (Pappy at -2185, -32, 328).
- Borrow: S/main gathering `6fdea50f` (ore veins, 285 lines) for Mine and Smelt.
- Deps: B3 (Kill goals). Test: new character appears at Farnum's, completes the chain to "Talk to Bartle", is
  level 2-3 with the hammer equipped. Risk: crafting stand-ins must not read as fake — keep the retail text, add
  the minigames later.

**C2. Bartle's chain + the missing set pieces**
- Goal: Find and Pull the Plug (3 Gloam Artifacts 5100327-5100329, "just north of Cobblestone Village and south by
  Collin's camp"), Cobblestone Showdown (instance, Mac), Off to the Queen (palace 4420119, Queen 5100330); the
  Gloam barrier on the Sanctuary road (5100336-5100340) drops after the Showdown; Sheila (2088) spawns only after.
- Build: artifacts as attackable `CombatNpc` props (or interact-to-destroy if an "object HP" model isn't in B3);
  barrier/Sheila as per-character conditional spawns (`Npc` visibility by quest state — S/main's per-player
  notification refresh in `QuestManager` is the pattern); Showdown entrance = an `EncounterEntryNpc` at "the cave
  leading to Blackspore Swamp" (east road; **position to verify from video**); Mac `human_m_mac.agr` (Models 477).
  Author the Showdown as a `DungeonDefinition` with 2-3 hooligan waves then Mac (`MainBoss=true`) — our design,
  labelled as such; 28 coins + the prize wheel (26 coins + gear, ZAM) once the loot wheel is ported.
- Deps: B2, B3, C1. Test: destroy 3 artifacts, enter the cave, beat Mac, come out to Sheila standing by Bartle and
  the road north open; "Off to the Queen" tracks to Sanctuary. Risk: instance asset fetch; Showdown layout unknown
  (video only).

**C3. Area dungeons from S/main**
- Goal: Sheep Watch (gate -2114, -57, 279; wolves + Alpha pair; 122 stars) and Highroad Hijinx (gate -2182, -37,
  141; hooligans, Collin, Mac; bonus 4 weapon racks 75548; 118 stars) enterable, gated by difficulty (2 = "level 3+
  recommended", 3+ requires a combat job level), rewarded per the wiki (`research/wiki/npcs/Sheep_Watch.wikitext`,
  `Highroad_Hijinx.wikitext`). Also the two placed wandering-encounter gates — Hooligan Bullies! (74753-74757, gate
  4162) and Hooligan Brawling Club! (75439-75442, gate 4156) — as overworld battle areas (46010 "wandering encounter").
  Cracked Claw Caverns (gate -2267, -29, 896) is the next one over, on the Blackspore side.
- Borrow: the `DungeonCatalog` entries (Sheep Watch = activity 119 / POI 87, zone `sg_sheep_watch`, r=300,
  procedural layout; Highroad Hijinx = 45 / 68, `sg_highroad_hijinx`; Hooligan Bullies = 137 and Brawling Club = 112
  in `bw_random_encounter_02`, authored: 4 brawlers 700 HP, 2 archers 800, boss 1500), `spawnDungeonEntrance`
  (an invisible model-511 `Npc`, cursor 11, `SendDungeonOffer` -> `EncounterStatePacket` x3 -> GO! button ->
  `EncounterParticipantRequestEntranceHandler`), `EncounterEntryNpc` (the wandering "Battle Starter"), the Battles
  start panel (`ba09a1d0`), end-screen flow (`cc9ac19e`), loot wheel (CarterW24 `combat` -> S/main). Sheep Watch and
  Hijinx are "defeat every enemy" stubs there: author their rosters from the wiki (sheep + wolves + Alpha Male/Female;
  hooligans + Hooligan-At-Arms + Collin + Mac, 4 weapon racks) using the Bixie Hive entry (escort stages, bonus
  props, `MainBoss`) as the worked example, and hand-place spawn points in `Scripts/Zone/sg_sheep_watch.lua` the way
  `sg_bixie_hive.lua` does. Dungeon data is C# in S/main; extract to `Resources/Dungeons.json` as we port.
- Deps: B2, B3. Test: click the Sheep Watch gate, get the start panel, clear the wolves, get the wheel, exit at the
  gate. Risk: low-medium once B2/B3 exist.

**C4. Vendors and the pet corner**
- Goal: Roosey (Brawler gear), Gloria/Jennifer/Walker (pets, supplies, collars), Mae's recipes, a potion vendor
  ("Joseph by the wagon", 21809 — **to place**) sell for coins.
- Borrow: S/main `Resources/NpcVendors.json` (keyed by NPC guid: `Items`, `ItemCosts`, `Bundles`, `SubTextNameId`)
  + `Resources/NpcVendorCollection.cs`, wired in its `StartingZone.cs:239-300` (cursor 17, a Merchant
  `InteractionProviders` entry that sends `CoinStoreItemListPacket` + `CoinStoreMerchantListPacket`), or PR #70
  `MerchantItems.json` (wiki-sourced tiers) folded into the same provider. Deps: A2 (radial). Test: buy a potion, use it in a fight.
  Risk: low; economy redesign later may reprice everything.

### Phase D — the town lives (see §4 for evidence)

**D1. Ambient conversations and barks (Lua + data)**
- Goal: the crash-scene chorus (94013, 94023, 94026, 94028, 94035, 94041, 94054, 94055, 94058 — Ricky, Car Repair
  Guy 2048, onlookers 2046/2047), the teenager gossip about Sheila (70047-70052, plus the pie/fashion-show sets
  70061-70071) on the townsfolk cluster nearest Bartle, Bartle's laments (20943-20945) that switch off after the
  Showdown, hooligan "SCRAM" (5100399), Simone/Jonelle idle lines.
- Build: `Resources/AmbientChatter.json` `{ participants: [guid...], lines: [{speaker, sid}], intervalSec, radius }`
  played round-robin by a zone-level ticker using the A2 bubble path, at the captured cadence (one line every 4-5 s,
  `TargetGuid = 0`, loop while a player is within ~30 u); `npc:sayLocalized` already exists; add a `bubbleOnly`
  flag and fire the 3101 talk gesture with each line. Deps: A2. Test: stand by Ricky for a minute, the crowd heckles
  him in order, nothing in the chat log. Risk: low; lines with `<font>` markup must be excluded (client shows raw
  tags).

**D2. Idle animations, wanderers, scripted movement**
- Goal: match the captures (§4b): critters and unnamed townsfolk wander 15-25 m at speed 3.0 with ~1 s
  `PlayerUpdatePacketUpdatePosition` + `ExpectedSpeed`; NPCs with a captured special idle get it (`Npc.Animation`
  is already serialised in `AddNpc`); `SetLookAt` toward an approaching player; crowd gestures via
  `PlayerUpdatePacketSetAnimation` play-once; a few scripted movers (Shakey near his kart; the tag kids 70023-70040
  — **location to verify**); Wilbur/pet-style followers later.
- Borrow: ImAlko PR #119 (`refs/remotes/survey-pr/pr119` = `d42dc7c5`, same base as ours). Our branch already has
  its scripting half (`src/Sanctuary.Scripting`, per-NPC `Scripts/Npc/*.lua`, `registerCallback("second")`,
  `npc:say/sayLocalized/moveTo`, `welcomer.lua`); the missing half is `Sanctuary.Core/Actions/*` (Instant, Delegate,
  Sequential, Parallel, Wait, Timeout), `Game/Actions/ActionManager.cs` + `MoveToAction.cs` (zone `Pathfinder` +
  `PathFollower`) and Lua `npc:runBehavior{ {say=...}, {wait=...}, {moveTo={x,y,z}}, {sequential=...} }`. Add an
  `anim` step that sends `PlayerUpdatePacketSetAnimation` (S/main's talk gesture uses `AnimationId 3105, PlayType 1`
  then a timed reset). Data: `research/npc-spawns/npc_spawns_unique.json` animation values per model
  (**to verify** the ~46 non-default rows and map them to placed models). Test: the adoption-centre dogs lie down,
  Shakey ambles near his kart, a kid runs past. Risk: none of the real routines survive; keep them few and plausible.

**D3. Prune and season** — remove strays (`knocker_m_boss` 17596, coffee table 31570, dragon pair 2073/2074), make
One of Three (4167) an October-only spawn. Test: walk the town, nothing odd by the warpstone.

### Phase E — optional / later
- **E1. Briarwood Caverns tutorial** (`sg_npx_cavern_01`, Darkthorne): assets not fetched, 4419545-4419557 are "tell
  Erik" placeholders (use ZAM's transcript), needs B2. Do after C2 so the arrival at Farnum has somewhere to go.
- **E2. Cooking/smithing minigames** to replace the C1 stand-ins; **E3. Sacred Glade "classic start"**; **E4. loot
  wheel + daily wheel** (S/main `accf0cd0`); **E5. Queen Valerian + job centre** in Sanctuary (the milestone ends at
  the palace door).

## 4. NPC liveliness — findings

**Question:** did the original NPCs have routines, walking paths, or talk to each other?

| Source | Evidence | Reading |
|---|---|---|
| (a) 2014 final-week spawn dump (`research/npc-spawns/npc_spawns_labeled.json`, 3,521 `AddNpc` rows) | `animation` = the **initial idle id**: 1 for 3,475; the rest are real idles — 3101 talking (10, e.g. Fisherman, Seating Area), 1152 wounded/toppled (7), 2902 teleporters (5), 3200 robgoblin dance (4), 5403 sleeping dog (3), 2140 Lazy Chugawug (2), 2100 boom-box/child (3), 8661 mounted (2), 1301 yeti/wolf (3). In the Cobblestone box (x -2300..-1700, z 150..720; 107 spawns) **every row is 1**. `StandAnimId`/`WalkAnimId` are -1 for nearly all. | The spawn animation says little: Cobblestone NPCs spawn in the plain idle, and (b) shows they still moved and gestured. |
| (b) the pcaps themselves (free-realms-re `captures/`, 3 sessions decoded this session: `p1`, `packets_3`, `packets_4`, 1,038 NPCs, ~15 min) | **Movement is streamed**: op125 `PlayerUpdatePacketUpdatePosition` to NPC guids, 5,397 packets to 223 NPCs at a median 1.01 s; **120 NPCs moved > 2 m**. Named movers: "Mrs. B" 1,481 m of path in 391 s, "Speedy" 1,150 m, "Buddy" 1,063 m, six Soccer Players ~110 m each, a mounted Skullz Roughrider 2,257 m; Rabbits/Squirrels/Deer/Dogs wander 15-40 m. `ExpectedSpeed` 3.0 (walk) / 6.0 / 12.5-15 (mounts). In the Cobblestone/Speedway box the squirrels, rabbits and miners wander 15-25 m. **Gestures**: op35/8 `SetAnimation` play-once to 114 NPCs (3101 "talk" x262, cheer sets 3302/3306/3316 on Soccer Fans, miner swings 2100/2110/2120), plus flags=3 idle sets paired with `UpdateIdleAnim` (5403 to a Dog, 2100 to a Human Child); `SetLookAt` NPC->player 62x; `ThoughtBubble` on pets. **Talk**: op15/4 `ChatPacketFromStringId` 147 total — two "Smart Kid"s alternating 5 lines every 5.0 s with target 0 ("The ice is frozen, I assure you." / "Did you check the color?"), another pair alternating 6 lines every 4 s ("We need to stockpile some supplies!" / "You think so?"), player-targeted greetings (Captain Speedway 3 lines, Florina's adoption-centre welcome, Hot Dog Stand Clerk), a "Pet Owner" repeating "Stand, [pet]!" every ~10 s. | **All three exist and are common**: wildlife and a few named NPCs walk; NPCs gesture and turn to look at you; NPC pairs hold timed alternating conversations; greeters bark at nearby players. |
| (c) string table | **NPC-to-NPC exchanges are authored**: teenagers about Bartle's "wife" (70047-70052: "Poor Bartle. He lost his wife during some bandit raid." / "No, doofus! Sheila is his horse."), the crash chorus around Ricky (94023 "Isn't this like, the third kart you've busted this month?", 94028, 94035, 94058) with Ricky answering (94026, 94054), kids playing tag with Carl (70023-70040 — a *moving* group), the pie/fashion-show teens (70061-70071), Karin's wand (70057-70060). Single barks: Bartle 20943-20945, hooligans 5100399, cats 70097-70099. The layout places the crash crowd (2046/2047/2048) beside Ricky, so the scene was staged. | Ambient chatter was scripted, multi-speaker, and location-bound; bubbles, not windows (Carlos's memory + npc-dialogue.md §A). |
| (d) wiki / reviews | Sacred Glade: "Michael Tallstrider frantically runs up to you"; Wilbur follows you; Shakey "wanders" (the community layout has him 14 u from his captured spots); MMORPG.com 2009: "The NPC scripting is often pure genius and always hilarious." | Scripted movement for set pieces (tutorial, escorts); the humour was in the lines. |

**What "lively" should mean for a faithful remaster** — the captures set the bar, so this is restoration, not
invention. In order of authenticity and cost:
(1) proximity greeting bubbles with retail lines, exactly the Captain Speedway / Florina pattern (A2);
(2) the staged NPC-pair and group conversations, alternating one line every 4-5 s with no target, bubble only (D1 —
the crash chorus, the Sheila teens, the pie/fashion-show teens);
(3) gesture animations: the 3101 "talk" gesture while an NPC speaks, `SetLookAt` toward the player who walks up, the
cheer/work gestures on crowds and miners (A2 + D2);
(4) **wildlife and townsfolk wander** — the captured rabbits, squirrels, dogs and miners around Cobblestone drift
15-25 m at walk speed 3.0 on ~1 s position updates; named quest-givers stay put (D2);
(5) the special idles from the spawn dump on the models that carry them — sleeping dogs at the adoption centre,
the Lazy Chugawug, talking seated groups (D2);
(6) a few scripted movers and set pieces (Shakey near his kart, the tag kids, Wilbur-style followers) via the action
framework (D2);
(7) state-driven changes — Sheila absent then back, Bartle's lament stopping, the barrier gone, hooligans respawning
— which is what made the 2013 town feel alive (C2).
Daily schedules or NPCs walking between buildings are not attested and stay out unless Carlos wants them as an
opt-in "reimagine" layer.

## 5. Decisions Carlos needs to make

| # | Decision | Default |
|---|---|---|
| D1 | Zone/instance base: port S/main's `ZoneManager`/`EncounterArenaZone` (proven, carries the dungeons) or adopt PR #116 `WorldZone` first and re-base the arenas on it (upstream direction, still churning). | S/main's model now; revisit if #116 merges upstream. Upstream compatibility is not a goal. |
| D2 | HP/energy curve numbers (lost). | Own table, linear-ish, tuned by feel in B1; document as ours. |
| D3 | Farnum's cooking/mining/smithing before the minigames exist: collect/talk stand-ins with retail text, or block the chain until the minigames are built. | Stand-ins now, minigames in E2. |
| D4 | Start new characters at Farnum's Farm now (cavern later) or build Briarwood Caverns first. | Farnum's Farm now. |
| D5 | Dev `!` commands: admin-only or remove. | Admin-only; `/help` shows nothing to players. |
| D6 | Showdown/dungeon rewards: fixed coins per wiki, or port the loot wheel first. | Fixed coins first, wheel in E4. |
| D7 | World state (barrier, Sheila, Bartle's lines) per character vs global. | Per character (single-player first, and it is how the game did it). |
| D8 | Seasonal: One of Three off until October; keep Roosey (2013). | Yes. |

## 6. Risks

- **The S/main port is the critical path** (B2/B3): a monolith 82 commits behind upstream, 167 conflicting files,
  its own XP, zoning and housing. Mitigation: extract by subsystem, never merge; keep #120 XP and #121 toolbars;
  drop its dev probes.
- **Zone assets** for `sg_newbiezone_showdown`, the dungeons and the cavern have never been streamed; the CDN needs
  the right `/NNN/` directory. Mitigation: request once through the client via `tools/asset-server`, or resolve the
  directory hash (asset-server README).
- **Set-piece positions** (Mac's cave, Collin's camp, artifacts, barrier, fires/well) exist only in videos.
- **Capture coverage is pocketed**: the three decoded sessions (~15 min) never enter Cobblestone proper, so the town's
  own barks/movers are inferred from the string table and neighbouring areas; the four undownloaded pcaps (~150 MB:
  `p2`, `p12`, `packets_2`, the minigame captures) may hold more and are worth a targeted decode later.
- **Text gaps**: Darkthorne placeholders; several NPX rewards unknown (The Last Best Hope, One More Gloam, Where's
  the Help?).
- **Design choices we own** (HP curve, Showdown waves, loot) must be labelled so they don't get mistaken for retail.
- **Reference-only repos** (free-realms-re, ai-decomp): learn from, never copy code. S/main and PR #119 are AGPL.
- **Quest goals**: adding `Kill`/`EncounterComplete` re-opens `QuestGoal`/`QuestManager`; regression-test the two
  working chains after every slice.
- **Code-as-data in S/main**: dungeons (`DungeonCatalog`, 1,846 lines of C#), enemy stats (formulas), ambient lines
  and vendor wiring (`StartingZone.cs`, 3,017 lines there vs our 1,166) all live in code. Porting verbatim would
  break the "data over code" rule; budget the JSON extraction into B3/C3/A2 rather than deferring it.
- **Sizes**: against `quests`, S/main differs by +5,489/-950 in just `Npc.cs`, `QuestManager.cs`, `StartingZone.cs`,
  `Player.cs`; `src/Sanctuary.Game` as a whole +32,690/-2,903 over 129 files. Every slice must be a re-implementation
  guided by its diff, not a file copy.
