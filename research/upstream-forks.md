# Survey: unmerged work in Sanctuary forks and upstream PRs

Compiled 2026-09-11. Research only; nothing here is merged. Snapshot refs:
upstream `main` = `d42ef77` (2026-09-11), our `quests` = `912fad7`. Our `quests` already contains upstream main up to
`c1f3e2c`, **Sulphural `quest-upstream-v2`**, **PR #120 (ImAlko progression)**, and PR #123's
`RewardBundleEntryCollectionAdd` (it came in with Sulphural's "updates" commit), so none of those appear as candidates.

Scope: 38 forks, 154 branches (151 fetched; the 3 on `carlosvillaxyz` are ours), all 31 unmerged upstream PRs
(fetched via `refs/pull/N/head`, which still works for PRs whose source repo was deleted), the upstream
`minigame` branch, and one non-fork copy (`raisingkaines/FreeRealms-Legacy`). Method is at the end.

---

## 1. Summary

**The big find: `Sulphural/Sanctuary` branch `main`.** `quest-upstream-v2` (the quest system we just adopted) is a
cleaned-up port of *one slice* of this branch. The branch itself is JadenY's integrated server: 267 commits
(2026-05-31 to 2026-08-18), roughly 740 files. The quest port left behind:

- **Combat and enemies:** six job kits (Archer, Brawler, Medic, Ninja, Warrior, Wizard) built from per-weapon
  data. 308 already-placed retail creature spawns turned into aggressive `CombatNpc`s (51 species), with aggro,
  leash, return-to-spawn and respawn. Name-based enemy tiers. 42 walk-through "atlas" dungeons plus authored
  encounters (Frostfang Fury, Tormented Spirits, Bixie Hive, Cracked Claw Caverns, Bandit Hideout, a combat
  tutorial). Boss plates, power-ups, potions, status effects. Death and respawn (pay-to-revive or free revive at the
  nearest warpstone). Kill and EncounterComplete quest goals, which v2 stripped out.
- **NPC behaviour:** a radial menu for NPCs with more than one interaction (quest plus vendor, and so on),
  quest-giver talk gestures, and overhead chat bubbles with real retail greeting lines when you walk up.
- **Economy:** NPC vendor shops, and the real "Spin For The Win!" daily wheel UI with streak and persistence.
- **Levelling:** its own XP and levels (`JobLeveling`, `AwardXp`, XP per kill, the full-screen `JobLevelUp`
  celebration and particle burst). **This overlaps PR #120, which is already in `quests`.**
- **Everything else:** pets (real opcodes, working My Pets panel), gathering (ore veins), the collections panel with
  per-character persistence, a party system and co-op dungeons, the Snow Days event (Trina, the 12 Days panel, the
  band), snowball battles, yo-yo and light-strand prop tricks, Lua-driven spawns, and PR #111 housing merged in.

It is live-tested (commit messages cite live client verification, `/pos` readings and "play-verified" packets).
It is also a monolith: 82 commits behind upstream, forked at `8ab6dd6` (2026-07-14), and it predates the upstream
chat-command refactor. A trial merge conflicts in 116 files against upstream and 167 against `quests`. So adopt it
the way the quests were adopted: **extract one subsystem at a time**, not a branch merge.

**Other important finds**

- **Housing, two independent implementations.**
  - `AlchemyDevelopment/main`: 31 lots (29 enterable), deeds bought from the store turn into houses, placing,
    moving and picking up fixtures, wallpaper, floor and roof per room, lot-data generators, and a 663-line
    `HOUSING.md`. Self-contained.
  - `raisingkaines` PR #111: the full native decoration editor, the housing directory, ratings and interactive
    fixtures. It sits on an old copy of ImAlko's zoning prototype, and upstream closed it as AI-architected.
- **Combat, a cleaner second source.** `CarterW24/combat-main` is Carter's port of the player side of combat onto
  current upstream (six job kits, ability engine, retail wire presentation). It is being upstreamed in slices: #121
  and #124 are merged. It has **no enemy AI** (only a training dummy and mob-archetype data). Closed PRs #74, #76
  and #77 hold his packet serializers. #76 (encounter and minigame packets) is pure additions with **0 conflicts**
  against `quests`.
- **Zoning.** ImAlko PR #116 (generic zoning and instancing) is under active review (changes requested
  2026-09-11). It renames `StartingZone` to `WorldZone`, so anything else touching `StartingZone.cs` should come
  after it, or be ported onto it. Phase 6 (instanced tutorial) and both housing branches need it.
- **Economy/stores:** closed PR #70 (brandonlhill, repo since deleted): in-world merchant shops with buy, sell and
  buyback, and 3,314 lines of wiki-sourced tiered inventories. Only reachable via `refs/pull/70/head`.
- **Nobody has touched** character-creation custom names, eye-colour swatches, loading screens, mail, crafting, or
  a free starter home. Those stay ours to build. (PR #25 is a name *profanity* blocklist, not custom names.)

### Ranked adoption list

Score = Value (1-5, for a single-player, faithful, playable revival) x Maturity (1 prototype, 2 working,
3 live-tested) / Conflict (1 clean or 2 or fewer trivial files, 2 moderate, 3 heavy or needs extraction).
Conflict counts come from `git merge-tree` against `quests` @ `912fad7`.

| # | Candidate | Area | V | M | C | Score | Why / caveat |
|---|---|---|---|---|---|---|---|
| 1 | **Sulphural/main: NPC interaction pack** (`25e43664` radial menu + talk animation, `0ff3e8fc` chat bubbles) | NPC dialogue/behaviour | 4 | 3 | 2 | 6.0 | About 680 lines, sits directly on the quest system we have. Fixes the "vendor who also gives a quest loses its shop" bug and `NpcRelevance.HasCursor` for every non-quest interactable. Touches `Npc.cs`, `QuestManager.cs`, `StartingZone.cs`, the interaction handlers. |
| 2 | **Sulphural/main: combat and enemies pack** (`CombatNpc`, `DungeonDefinition`, `EncounterArenaZone`, `EnemyTier`, death/respawn, Kill/EncounterComplete goals; key commits `c34648df`, `7086796b`, `a126be4b`, `12049172`, `00c1db34`, `325661be`) | Combat, enemies, dungeons | 5 | 3 | 3 | 5.0 | The only branch with working enemies, aggro AI, respawn and death. Large extraction (Combat/ ~5.6k lines, Zones/ ~13k). Can be combined with Carter's player-side port (#4): combat-v2 already merged the two lineages once. |
| 3 | **AlchemyDevelopment/main housing** (`c301cee2`) | Housing buy/place | 5 | 2 | 2 | 5.0 | Independent of #116. 6 conflicts (EF snapshots, `Npc.cs`, the interact handlers, `PacketClientFinishedLoadingHandler`). Caveats from its own message: 48 `Models.txt` rows must also go into the **client** copy or house surfaces have no collision; lots 18/29/31/32 unverified; debug scaffolding (`SANCTUARY_AUTO_HOUSE`, every surface item granted on entry) still present. |
| 4 | **Sulphural/main: collections panel and persistence** (`8075c83f` collections part) | Collections | 3 | 3 | 2 | 4.5 | Real `CollectionStart`/`CollectionAddEntry` packets, payout to the collection's own job, `DbCharacterCollection`. The commit also carries the scare quest goals and prop tricks; cherry-pick selectively. |
| 5 | **Sulphural/main: level-up presentation** (`JobLevelUp` full-screen UI, delayed `PFX_levelup_big`, toolbar-safe profile re-send, XP per kill; `28ed42d0`, `82ec8bf4`, `0b08378f`) | Levelling | 3 | 3 | 2 | 4.5 | **Overlaps PR #120 (in quests).** Take only the presentation and "profile re-send wipes the toolbar" fixes and feed them from #120's XP/rank tables. Don't take `JobLeveling`. |
| 6 | **Sulphural/main: daily wheel** (`accf0cd0`, `6136c73a`) | Economy | 3 | 3 | 2 | 4.5 | The real `game_wheel.gfx` flow (op143 RepeatingActivity + op26/12 StartFlashGame + op39/14 payload). About 2.9k lines across 38 files, with migrations. Needs part of the minigame/activity plumbing. |
| 7 | **CarterW24/combat-main** | Combat (player side) | 4 | 2 | 2 | 4.0 | Current with upstream, 3 commits. 13 conflicts, mostly add/add against the #121 versions of the same files. Pairs with #2 for enemies. Note: 21.8k of its lines are `CombatAbilities.json`/`CombatJobs.json`. |
| 8 | **ImAlko PR #116 generic zoning** | Zoning/instances | 4 | 2 | 2 | 4.0 | Foundation for the tutorial instance, housing and dungeons. 4 conflicts (`Player.cs`, `BaseZone.cs`, `IZone.cs`, `WorldZone.cs`). Still changing under review; decide whether to take it now or wait for the upstream merge. |
| 9 | **PR #70 merchant shops** (brandonlhill) | Economy/stores | 4 | 2 | 2 | 4.0 | Buy, sell and buyback at in-world merchant NPCs, `MerchantItems.json` (wiki-sourced tier splits). 5 conflicts. Fold into #1's provider model (Sulphural's own `1a1decb9` vendor wiring is thinner). |
| 10 | **Sulphural/main: gathering** (`6fdea50f`) | Jobs (Miner) | 2 | 2 | 1 | 4.0 | 285 lines, 4 files. First pass only: hand-placed ore veins near spawn, not the real mines. |
| 11 | **PR #38 safe teleport to nearest warpstone** (StimpyDev) | QoL | 2 | 2 | 1 | 4.0 | 1 file. Sulphural's death system has the same "nearest warpstone" logic, so take only one. |
| 12 | **PR #76 encounter/minigame packet library** (CarterW24) | Packets for dungeons/minigames | 2 | 2 | 1 | 4.0 | 17 files, pure additions, **0 conflicts**. Includes the fix that makes `BaseEncounterPacket` write its 2 missing header ints. Useful groundwork for #2. |
| 13 | **Sulphural/main: pets** (`41f83788`, `86a1e2b6`) | Pets | 3 | 2 | 2 | 3.0 | Real opcodes and panel refresh. Depends on its pet DB/migrations (`AddPets`). |
| 14 | **edenfps fishing (PR #31)** | Minigame/job | 3 | 2 | 2 | 3.0 | Full cast-bite-fight-catch loop, six holes, rods, lures, persistence. Built on the upstream `minigame` branch; 8 conflicts. Open since 2026-07-04 with no review. Its ambient fish are WIP. |
| 15 | **ImAlko PR #119 NPC action framework** | NPC behaviour | 3 | 1 | 1 | 3.0 | 317 lines: `IAction` (OnStart/OnTick), Sequential/Parallel/Wait/Timeout, `MoveToAction`, exposed to Lua. Closed as "too much for now". Good substrate for scripted NPCs; 1 conflict (`Npc.cs`). |
| 16 | **raisingkaines PR #111 housing editor/directory** | Housing | 4 | 2 | 3 | 2.7 | Richest editor (move/rotate/scale/tint, pickup-all, directory, ratings, teleporters, safe logout). But 14k lines, built on ImAlko's *old* zoning prototype (collides with #116), and upstream rejected it as AI-architected. Pick this *or* #3. Sulphural/main has it merged, with tests. |
| 17 | Sulphural/main: Snow Days and snowball battles | Seasonal content | 2 | 3 | 3 | 2.0 | Real op207 ProgressiveQuest (12 Days panel) RE is notable. Seasonal, so later. |
| 18 | `yungcomputerchair/minigame` (upstream `minigame` + main, 2026-08-02) | Minigame infra | 2 | 1 | 1 | 2.0 | Activity list, matchmaking queues, minigame start/payload. Prerequisite for fishing and the wheel. |
| 19 | AlchemyDevelopment `fix/store-bundle-serialization` / PR #101 | Store UI | 2 | 2 | 1 | 2.0 | Real per-category counts in the marketplace tree. 0 conflicts. Low value if SC goes away. |
| 20 | FreeRealms-Legacy player-to-player trading | Trading | 1 | 2 | 1 | 2.0 | About 2.7k lines, clean (1 conflict: `Program.cs`). Pointless solo; keep for a later multiplayer mode. |
| 21 | StimpyDev PR #82 friend-invite fixes | Social | 1 | 2 | 1 | 2.0 | 11 lines, 0 conflicts. |
| 22 | raisingkaines `trading-card-game` | Minigame (TCG) | 2 | 1 | 2 | 1.0 | One-commit TCG launch bridge. Prototype. |
| 23 | brennengreen `feat/minigame-combat-quest-protocol` | Protocol | 1 | 1 | 2 | 0.5 | Recognises and routes ops 32/33/34/49/76, plus pcap tooling. Superseded for combat and quests. |
| 24 | cillow19 `soccer` | Minigame | 1 | 1 | 3 | 0.3 | WIP soccer zone and ball physics mixed with an unrelated Node web side project. |

**Suggested order:** 1 → 12 → 2 (+7) → 5 → 9 → 3 (or take 8 first and then 16) → 4 → 6 → the rest. Take #8 early if
you intend to follow upstream's zone layout, because every later port touches `StartingZone.cs`.

---

## 2. Per-fork details

Branches with nothing ahead, only copies of upstream WIP branches (`guild`, `minigame`), or only commits already
merged upstream are grouped at the end of this section. "Ahead/behind" is against upstream `main`. "Conflicts" are
files that conflict in a trial merge into upstream (U) and into `quests` (Q). Line counts exclude CRLF-only noise
where it mattered.

### Sulphural/Sanctuary (JadenY; repo pushed 2026-09-11)

**`main`** (tip 2026-08-18; 267 ahead / 82 behind; 252 patch-unique; conflicts U116 / Q167)
- **What it is:** JadenY's integrated server, described in the Summary. Its initial import `326c954d` (2026-06-27)
  brought "housing, pets, transformations, mounts, coin store, NPC vendors". The rest is iterative.
  - Combat: `Combat/*WeaponAbilities.cs` for six jobs, `ProjectileNpc`, native op35/62 projectiles, LaunchAndLand
    cooldown radials, traits and dodge.
  - Enemies and dungeons: `CombatNpc` and `DungeonDefinition` (42 atlas dungeons and 54 encounter arenas),
    authored arenas, Battle Starter NPCs, party and co-op dungeons, death and respawn.
  - Quests: a fuller `QuestManager` (2,236 lines) with Kill/EncounterComplete/scare/counted-talk goals.
  - NPCs and content: radial NPC menus, chat bubbles, vendors, pets, daily wheel, gathering, collections panel,
    Snow Days, snowball arena.
  - Scripting and tooling: Lua spawning of the whole roster, and ~50 admin chat commands (still under
    `Sanctuary.Gateway/ChatCommands`, the pre-#93 layout).
  - Housing: PR #111 merged on 2026-08-15, with 1.3k lines of housing tests.
  - Data: large JSON (`ClientItemDefinitions`, `StoreBundles`, `CoinStoreItems`); most of the 1.5M-line diff is
    reformatting.
- **Features:** combat, mobs/AI, dungeons, death, levelling, NPC behaviour/dialogue, vendors, daily wheel, pets,
  gathering, collections, party, seasonal events, housing, Lua spawns.
- **Maturity:** live-tested. Many commits are explicit live bug-fix loops, e.g. "Fix bow not re-firing after an
  overworld kill", "In-encounter KO/revive wire matches the play-verified build packet-for-packet". It also still
  holds probe and debug commands (`!lp`, `!abil`, `!fx`).
- **Overlap with `quests`:**
  - Quests: v2 is the upstreamed descendant of this code, so porting a subsystem means reconciling with v2's
    `QuestManager` and `QuestGoal`.
  - PR #120: direct overlap on XP and levels (`JobLeveling`, `Player.AwardXp`).
  - PR #116: it has its own instanced-arena `ZoneManager`.
  - Housing: it carries PR #111.

**`quest-upstream-v2`**: adopted (in `quests`).
**`quest-data-model`** (= PR #109) and `pr109`, `pr96`, `pr-27`, `consumables-only`: PR staging branches. All
merged upstream or subsumed by v2. `consumables-only` is 52 commits ahead but only 30 net new lines.
**`feature/transformations`** (2026-06-27, 1 commit): the original monolithic import. Superseded by `main`.
**`guild`, `minigame`**: copies of upstream WIP branches.

### CarterW24/combat-main (Carter Weakley; repo pushed 2026-09-11)

**`combat-main`** (default; tip 2026-09-11; 3 ahead / 45 behind; 52 files, +21.8k (about 19k is JSON); conflicts U11 / Q13)
- **What it does:** `6d97a02d` "Combat restoration: six-job kits, ability engine, retail wire presentation":
  - Weapon-mapped basics and specials; sweep, AoE, summon and buff verbs.
  - Heal, energy steal, DoT, multi-hit, crits, an energy pool with regen, projectiles.
  - Packet families for StartCasting, LaunchAndLand, SetDefinition, HitPointModification, LaunchProjectile,
    SetAnimation, composite effects, knockback, disposition, mana/hitpoint/scale updates and IsFighting.
  - Data in `CombatAbilities.json`, `CombatJobs.json` and `CombatArchetypes.json`.
  - WieldType resolution on equip, job switch and spawn; the toolbar syncs on equip and job switch.
  - Admin tools `!hp`, `!dummy`, `!testweapons`. `8908ab98` re-aligns with upstream #121 and `4b78a7cc` is #124.
- **Features:** player combat, job ability toolbars. Enemies exist only as archetype data (Snow Wolf, Evil Wolf and
  others with aggro and leash ranges); there is no AI loop.
- **Maturity:** working. It is being upstreamed piecemeal, and two slices have passed review (#121, #124).
- **Overlap:** `Player.cs`, the resource manager and the ability handler. Both combat JSONs are add/add against the
  #121 versions. No overlap with #120's XP.

**`combat`** (2026-07-15, 9 commits): Frostfang Fury encounter (waves, alpha flee, heart buff, exit door), the loot
wheel end to end, red hostile nameplates. Live-faithful and video-matched. Superseded: merged into Sulphural/main
on 2026-07-09 and 2026-07-16.
**`combat-v2`** (2026-07-26, 160 ahead): Sulphural/main as of 2026-07-16 plus Carter's projectiles, Warrior kit,
EnemyTier and group encounter entry. A sibling of Sulphural/main; use `main` instead.
**`pr1-playerupdate-packets`, `pr3-encounter-minigame-packets`, `pr4-ability-combat-packets`**: sources of closed
PRs #74, #76 and #77 (see section 3). **`pr5-combat-toolbar`** is #121 (merged). **`fix-weaponless-toolbar`** is #124
(merged). **`pr2-addnpc-namecolor`** is merged. **`guild`, `minigame`**: upstream copies.

### ImAlko/Sanctuary (Alexander Koldy)

- **`feature/progression`** is PR #120. **Already in `quests`.**
- **`major-feature/zones`** is PR #116 (tip 2026-09-06; 18 ahead / 8 behind; 17 files +824/-525; conflicts U1 / Q4).
  - It introduces generic zones with an inheritance model (`WorldZone`, `HousingZone`, `CombatZone`), a
    `ZoneManager` that creates and disposes instances, safer entity registration, `!zone ls`, and a rename of
    `StartingZone.cs` to `WorldZone.cs`.
  - Maturity: working, and heavily reviewed (30 review comments; changes requested 08-20, 08-25, 08-30 and 09-11).
    ImAlko has an open question about `SendWelcomeInfo` firing on every WorldZone teleport.
  - Overlap: the zone layer and `Player.cs`. PR #111 (housing) is blocked on it.
- **`major-feature/zones-testing`** (2026-08-30): an older iteration of #116.
- **`prototype/npc-behavior`** is PR #119 (closed draft, 2026-08-23). Action-based NPC behaviour exposed to Lua.
  Prototype; 1 conflict (`Npc.cs`).
- **`pathfinding`** (#91), **`feature/drop-tables-and-rewards`** (#113), **`bugfix/facial-hair`** (#108) and
  **`feature/more-mounts-1`** (#115) are merged. **`bugfix/coinstore`** is #107, closed as a duplicate. It adds
  tints to max-level combat armour and cat/dog ears (9 residual lines).

### raisingkaines/Sanctuary (Raising Kaines)

- **`housing`** is PR #111 (tip 2026-08-20; 5 ahead / 45 behind; 97 files, about 14.4k; conflicts U2 / Q3 on
  `Player.cs`, `StartingZone.cs` and `Gateway/Program.cs`).
  - What it adds:
    - Per-character home ownership and the client's native decoration UI (edit grants, fixture inventory,
      furniture score).
    - The full fixture workflow: place, move, rotate, scale, tint, save, pick up, pick up all.
    - Interactive fixtures: teleporters, elevators, gumball machines, fireworks, pools.
    - The housing directory and ratings (published, featured, friends, votes).
    - Housing zones for Club House, the Seaside and Snowhill lots, the Wilds lot and Snowhill Lodge.
    - Safe logout: saves the overworld position when you disconnect inside a house.
    - Following a friend into their house.
  - It includes ImAlko's zoning v1 prototype (`60250126`, `80efc147`).
  - Maturity: working, and tested on the author's server ("Adjustments made after testing"). Upstream response:
    "very likely not going to get reviewed … core system architected by AI"; now blocked on #116.
  - Stacked split branches: `housing-ownership-two-homes`, `housing-instance-lifecycle`,
    `housing-persistence-schema`, `housing-editor-packet-contracts`, `housing-editor-catalog-data`,
    `housing-editor-runtime` (= closed PR #118), `housing-directory-ratings`, `housing-store-catalog-tints` (top
    of stack, 12 commits, same net content as `housing`), `housing-editor-directory-pr116` (the stack rebased onto
    #116) and `housing-full-archive` (= `housing`).
- **`shop-selling-item-fixes`** (2026-08-15): restricts coin-shop sales of house fixtures to their owners; bundles
  a 1.6k-line `HouseOwnershipService`. Only makes sense together with the housing work.
- **`trading-card-game`** (2026-07-03; 1 commit; 26 files, +7.5k): the TCG launch flow through the gateway
  (activity launch, matchmaking state, a TCG detail UI patch, a generic minigame start-screen bridge). Prototype.
- **`guild`**: the old upstream guild branch plus one fix. Superseded by upstream Guilds (`633ca76`).
  **`fix/afk-disconnection`** is #117 (merged).

### raisingkaines/FreeRealms-Legacy (non-fork copy, one "Initial commit", 2026-09-10)

- **`main`** is upstream at `d24be9d` (2026-08-24, the closest match) plus a **player-to-player trading system**:
  `Trading/TradeManager.cs` (1,282 lines), `TradeCommitter`, `TradeTransferPlanner`, `BaseTradePacket` (op 59 with
  all 18 sub-ops), a trade interaction and handlers. There are also small `UdpConnection`, `GatewayConnection` and
  `Player` changes, and a README rebrand. 18 files, +2.7k.
  - Maturity: working. It has timeouts, validation and a trading-card item type guard.
  - Overlap: none except DI registration in `Program.cs`. Because the history is unrelated, it must be applied as
    a patch from `d24be9d`.
  - AGPL-3.0 LICENSE retained.
- **`minigame`**: an old snapshot of upstream `minigame` (pre-2026-06-24). Nothing new.

### AlchemyDevelopment/Sanctuary (Jacob Plunkett)

- **`main`** / **`feature/housing`** (tip 2026-08-22 / 2026-08-21; 83 / 80 files, about 15.9k; conflicts U3 / Q6).
  - What it adds:
    - Lot data for all 31 lots in `Houses.json` (zone, sky, geometry, build areas, re-texturable surface groups,
      rooms). 29 are enterable.
    - `HouseZone` (1.4k lines) and `HousingManager`, with the housing packet handlers: enter and leave, place,
      move, pick up, and per-room wallpaper, floor and roof.
    - Persistence for houses, fixtures and surfaces (EF migrations for SQLite and MySQL).
    - Deed resolution, so store-bought lots become houses.
    - `tools/` generators that rebuild the lot data from client assets, and `HOUSING.md`, which also records
      approaches that failed.
    - Tests for house definitions and deeds.
  - Maturity: working. The author calls it the WIP integrated branch that is being split into PRs; debug
    scaffolding is still present, and a client-side `Models.txt` patch is required.
  - Overlap: `Npc.cs`, the interaction handlers, `PacketClientFinishedLoadingHandler` and the EF snapshots. It is
    independent of #116.
- **`housing/data`** (2026-08-16): only the lot data and generators (7 files, +5.7k). Useful on its own for either
  housing implementation.
- **`fix/store-bundle-serialization`** (2026-08-03) / **`feature/store-category-counts`** (= closed PR #101):
  computes per-category item counts for the SC store category tree. 0 conflicts. It also adds a `TODOs.md`.
- **`minigame`**: upstream copy.

### edenfps/Sanctuary (eden)

- **`fishing`** is PR #31 (tip 2026-07-03; 2 ahead / 128 behind; 97 files, +13.7k, of which 7.4k is
  `ClientActivityDefinitions.json`; conflicts U7 / Q8).
  - It is built on upstream's `minigame` WIP commit `bc0982b`.
  - The server drives the whole fishing loop (opcode 138): cast, school, bite, fight, reel, catch banner and item
    grant, with catches persisted to the DB.
  - Content and gear: per-hole fish tables for all six holes, the Fish Finder, treasure and junk catches, and
    multiplayer sync. Rods set cast-distance tiers; lures add +10%; Treasure Magnet.
  - Tooling and docs: `tp`/`pos` commands, and four docs on RE notes, handoff, wiki data and the remaining plan.
  - Maturity: working core loop; the ambient fish scenery is WIP. Open PR with no reviews.
  - Overlap: `StartingZone.cs`, `BaseZone.cs`, the ability handler and `RewardBundleBase`.
- **`fix/coin-store-list-jump-after-purchase`** (2026-06-27): writes the `ClientItemDefinition` after the
  `ClientItem` in item-add packets, so the store list stops jumping to the top. It probably overlaps what upstream
  already serialises; check in game before taking it. **`main`**: reverted experiments, net zero.

### cillow19/Sanctuary (Cat / cillow19)

- **`soccer`** (tip 2026-09-10; 154 ahead / 31 behind; 9 conflicts): a soccer zone and `SoccerZone`, the
  `BaseSoccerPacket` client config and game state, ball physics (WIP, "test" commits), and a `!soccer` command. It
  is bundled with the author's `dev` history: WebAPI admin endpoints, a chat-log reader, and a separate Node.js
  `free-realms-server` / `free-realms-website` account and manifest service. Prototype.
- **`dev`** / **`add-admin-endpoints`**: admin WebAPI endpoints and the chat-log endpoint. Server-admin tooling;
  no gameplay value.
- **`add-yoyo-weapons`** (2026-07-12): yo-yo store bundles, texture alias and models, with the animation found by
  trial. Small data change; Sulphural/main has yo-yo tricks too.
- **`consumables-refactor`** (2026-08-24): JadenY's ability refactor plus an abstract `ConsumableAbility`. Largely
  merged upstream (`Helpers/Abilities`).
- **`add-referee`** (merged as #110), **`silly-string`** (merged via consumables), **`admin-commands(-main)`**
  (superseded by the #93 command refactor), `contributing-md`, `test`, `revert-*`, `minigame`: nothing to adopt.

### amuralle/Sanctuary (Alex M)

- **`collections/quest-reward`** is PR #123. Already in `quests`.
- **`collections-dev-rig`** (2026-06-28): `DevPacketLabService` (1.3k lines, a packet experimentation service),
  collection nodes JSON and `docs/COLLECTIONS_AND_PACKET_LAB.md`. Dev tooling that predates the merged collections.
  Could help with RE work; not a game feature.
- **`collections-foundation`**, **`collections-review-fixes`**, **`fix/native-collection-updates`**,
  **`fix/collection-node-mounted-interaction`**, **`fix/reward-bundle-item-packet`**,
  **`collections/native-collection-integration`**, **`npc-visible-notifications`**: pre-merge iterations of #86,
  #94, #96 and #104. Merged.

### MyRealms/Sanctuary-Mainline

- **`minigames-Mining` / `-cooking` / `-derby` / `-micro-games` / `-racing` / `-smelting`, `playgrounds,`**: all
  identical (8 commits: README and LICENSE rebrand, image deletions). **No code despite the names.**
- **`playgrounds---snowshill-2021-historical`** (2026-07-24): a single `Npcs.json` with 4,663 entries *including
  spawn positions and headings* (upstream's current `Npcs.json` has no positions; spawns live in Lua). A data
  reference for NPC placement; compare it with `research/npc-spawns/` before using. No code.
- **`Chrome-Drake-Mount-And-LawnChair-Adding`** (#85), **`mounts-and-rides-fix`** (#106), **`teleport-fix`** (#97):
  merged.

### brennengreen/Sanctuary

- **`feat/minigame-combat-quest-protocol`** (2026-07-04; 1 commit; 17 files, +781; Q4): packet base classes with
  sub-op enums, and gateway routing for Combat (32), VehicleRace (33), DemolitionDerby (34), Quest (49) and Soccer
  (76), from live pcap analysis. Includes `research/packet-analysis/` pcapng tooling and `MINIGAME_RE_NOTES.md`.
  Protocol layer only. Superseded for combat and quests; the race, derby and soccer enums are still unique.
- **`guild`, `minigame`**: upstream copies.

### StimpyDev/Sanctuary

- **`fix/friend-accept-ignore-check`** is PR #82 (open; 3 files, +11/-3; 0 conflicts). Friend invites are kept
  until the DB accept succeeds, and you can no longer ignore friends from the interaction menu.

### strophiccat/Sanctuary

- **`main`, `guilds`**: guild functionality (PRs #32, #33, #45). Superseded by upstream Guilds.
- **`ignoring`** (2026-07-08): ignore-list fixes and a reusable system-chat helper (4 files; 3 conflicts). Probably
  superseded by upstream #81 ("tell echo on ignore") and the chat-helper refactor.

### yungcomputerchair/Sanctuary (Gent Semaj, upstream maintainer)

`lua`, `commands`, `npcs`, `guild`, `docker`, `cicd`, `devtools`, `friends`, `g9`, `license`, `membership`, `perf`,
`profiles`, `resources`, `scshop`: all merged upstream (their residual diffs are conflict artefacts).
**`minigame`** (2026-08-02) is upstream `minigame` merged with main plus an unknown-field deserialisation fix, making
it the most current copy of the minigame infrastructure (49 files, +9.7k; 1 conflict against `quests`).

### Other forks with code ahead but nothing worth adopting

- **EDITzDev/main** (2026-07-15): database-layer refactor (factories, the `SqLite` to `Sqlite` rename). Already
  upstream.
- **jisham318/main** (2024-10-22): MySQL timestamp columns and a null-player crash fix. Ancient.
- **MarkCiliaVincenti/net9locking** is PR #4: a backported `Lock`. Obsolete on .NET 9/10.
- **thisisme133/`claude/convert-to-cpp23-…`** (2025-11): an AI port of the UDP library to C++23. Not applicable.
- **lyszt/elixir** (2026-01): an Elixir/Phoenix rewrite ("ryujin_core"). Not applicable.
- **claytonsulby/docker-freerealms** (2026-02): Unraid docker notes only.

### Forks with nothing ahead of upstream

GekkoQuest, Isalobo2, SWNestoras/OSFR-With-NO-Chicken, FreeRealmsClover/sanc-fork (`main`; its `minigame` is an
upstream copy), mihaubuhai, lysskiss, EagleArrow22, ckcircuitwitch/Free-Realms-Sanctuary, Gmjjr,
deansawyer2026-rgb, SengokuNadeko, Alziibun, hehehenrtque, shenderson62, nmclaren, seleniumdrop,
RealmerFree (`main`, `chicken`), BasedJumper, plus the `main` branches of StimpyDev, amuralle, raisingkaines,
ImAlko, CarterW24, brennengreen, cillow19 and MyRealms.

Related non-fork repos checked and empty or irrelevant: `deansawyer2026-rgb/Sanctuary-Pets` (README only),
`deansawyer2026-rgb/OSFR-Client` (empty), `strophiccat/PortableRealms` (empty), `MyRealms/MyRealms` (README),
`MyRealms/FreeRealmsJS` (JS, separate project), `raisingkaines/Robbie-` (Rust Discord moderation bridge).

---

## 3. Unmerged upstream PRs

State as of 2026-09-11. "Q" = conflicting files against `quests`.

| PR | Author / branch | State (updated) | Size | Review | What it does | Maturity | Q | Verdict |
|---|---|---|---|---|---|---|---|---|
| #120 | ImAlko `feature/progression` | open (09-11) | +571/-221, 18 files | 3 author comments, no maintainer review yet | XP (stars), level bar, level-ups, rewards architecture | live-tested ("Play testing and bug fixing") | 0 | **Already in `quests`** |
| #116 | ImAlko `major-feature/zones` | open (09-11) | +824/-526, 17 files | changes requested x4 (latest 09-11) | Generic zoning and instancing, StartingZone to WorldZone | working | 4 | Rank 8 |
| #111 | raisingkaines `housing` | open (09-11) | +14,386/-1,510, 98 files | maintainer: won't review as is (AI-architected); blocked on #116 | Housing editor, directory, ratings, ownership, zoning fixes | working | 3 | Rank 16 (or via Sulphural/main) |
| #109 | Sulphural `quest-data-model` | open (09-11) | +2,677, 17 files | changes requested (08-11) | Quest definitions, JSON loading, CharacterQuests table | working | 5 | Already covered by v2 in `quests` |
| #123 | amuralle `collections/quest-reward` | open draft (09-11) | +15, 1 file | none | `RewardBundleEntryCollectionAdd` (type 0xA) | working | 0 | **Already in `quests`** |
| #82 | StimpyDev `fix/friend-accept-ignore-check` | open (09-11) | +11/-3 | author owes fixes | Friend invite persistence, block ignoring friends | working | 0 | Rank 21 |
| #31 | edenfps `fishing` | open (07-23) | +3,861 (+7.4k data), 51-97 files | none | Fishing minigame end to end | working | 8 | Rank 14 |
| #119 | ImAlko `prototype/npc-behavior` | closed draft (08-25) | +317/-32, 13 files | "good ideas but too much for now" | NPC action framework (Lua to C#) | prototype | 1 | Rank 15 |
| #118 | raisingkaines `housing-editor-runtime` | closed (08-20) | +12,947 | none | A slice of #111 | working | 3 | Covered by #111 |
| #112 | raisingkaines `shop-sell-all-inventory` | closed (08-15) | +8/-6 | approved, then withdrawn | Treat qty <= 0 as "sell all" | n/a | 0 | Skip: the client never sends it; the real sell bug was ResellValue = 0 in JSON |
| #107 | ImAlko `bugfix/coinstore` | closed (08-10) | +9 | dup of #108 | Tints for max-level armour, ears | working | 2 | Optional data nicety |
| #105 | MyRealms `mounts-duplicate-fix` | closed draft | +17/-9 | superseded | Dismount before summoning | – | 0 | Merged as #106 |
| #102 | Sulphural `quest-upstream-v2` | closed (08-06) | +7,034 | "too much to review; keep the branch" | Quest system and Take Me There | live-tested | 0 | **Adopted** |
| #101 | AlchemyDevelopment `feature/store-category-counts` | closed (08-03) | +33/-4 | closed without comment | SC store category counts | working | 0 | Rank 19 |
| #98 | GroaxyVRC `main` (repo deleted) | closed (08-02) | +3/-3 | – | Mount disposal | – | 0 | Superseded by #106 |
| #95 | amuralle revert | closed | – | – | Revert of #94 | – | – | n/a |
| #80 | raisingkaines `enforcer-job-profile` | closed (07-16) | +828 | changes requested (deployment/AI files) | Enforcer job, red nameplate | working | 0 | Superseded: upstream `aea3bcd` added Enforcer |
| #78 | raisingkaines `referee-job-profile` | closed (07-16) | +161/-26 | changes requested | Referee profile alias | – | 3 | Superseded by #110 |
| #77 | CarterW24 `pr4-ability-combat-packets` | closed (08-30) | +239/-14 | none | AbilitySetDefinition, StartCasting, AttackProcessed, EnableBossDisplay | working | 3 | Contained in combat-main |
| #76 | CarterW24 `pr3-encounter-minigame-packets` | closed (08-30) | +908, 17 files | "cross-reference with minigame branch" | Encounter family (offer popup, state, GO!, HUD, IsFighting); minigame start, over, knock-out, loot wheel, score rows; op45/47 goals; op62 combat ruleset; RewardBundle; `BaseEncounterPacket` header fix | working | **0** | Rank 12 |
| #74 | CarterW24 `pr1-playerupdate-packets` | closed (08-30) | +364, 10 files | "remove the annotations, then good to merge" | 10 op35 serializers (SetAnimation, HP mod, knockback, disposition…) | working | 3 | Contained in combat-main |
| #70 | brandonlhill `pr/merchants` (repo deleted) | closed (07-14) | +5,380/-28, 30 files | closed under the no-AI policy; screenshots show it running | In-world merchant shops, buy/sell/buyback, wiki-sourced inventories | working (play-tested by the author) | 5 | Rank 9 |
| #65 | yungcomputerchair `docker` | closed | +105/-56 | – | Docker layer caching | – | 4 | Superseded by #90 |
| #50 | StimpyDev `spam-fix` | closed | +10/-2 | "over-engineered" | Limit pending friend requests | – | 2 | Skip |
| #45, #33, #32 | strophiccat guilds | closed | – | "recreate on the guild branch" | Guilds | – | 34 | Superseded by upstream Guilds |
| #38 | StimpyDev `teleport-safety` | closed (07-07) | +48/-2 | "why does it bring other commits" | Safe teleport to the nearest warpstone POI | working | 1 | Rank 11 |
| #25 | StimpyDev `profane-filter` | closed (06-27) | +37/-19 | – | Configurable profanity name blocklist | working | 1 | Skip (single-player) |
| #9 | strophiccat `fix/style-cards` | closed (2025-12) | +25/-8 | – | Clean-shaven style card | – | 2 | Superseded by #108 |
| #4 | MarkCiliaVincenti `net9locking` | closed (2024) | +15/-14 | declined | Backport Lock | – | 1 | Obsolete |

Upstream branch `minigame` (EDITz WIP, 2026-07-18; 49 files, +9.7k): activity definitions, matchmaking queue
listing, activity join/launch, minigame start/payload/end, `ClientActivityDefinitions.json`. Prototype
infrastructure that fishing and the daily wheel both lean on. Prefer the `yungcomputerchair/minigame` copy, which
is merged with main.

### Overlap with PR #120 (levelling), already in `quests`

- **Sulphural/main**: its own XP and levels (`Leveling/JobLeveling.cs`, `Player.AwardXp`, a rank loop,
  `ClientUpdatePacketJobLevelUp`), XP from kills (`CombatNpc`) and from collections. Porting its combat must route
  kill XP through #120's API instead. Its level-up *presentation* (rank 5) is worth grafting onto #120.
- **CarterW24/combat-v2** and **CarterW24/combat**: the same lineage as Sulphural's, with Frostfang granting job XP
  "through the real leveling system" of that branch.
- **CarterW24/combat-main**: no XP code, so no overlap.
- **ImAlko drop tables (#113, merged)**: the base #120 builds on.

---

## 4. Method

1. `gh api repos/Open-Source-Free-Realms/Sanctuary/forks --paginate` gave 38 forks. `gh api repos/<fork>/branches`
   on each gave 154 branches.
2. Every fork's branches were fetched into the local repo as remote-tracking refs, with no checkout and no change to
   the working tree:
   `git fetch --no-tags https://github.com/<fork>.git '+refs/heads/*:refs/remotes/survey/<owner>/*'`.
   Unmerged PR heads came from `+refs/pull/N/head:refs/remotes/survey-pr/prN`, which covers PRs whose source repo
   was deleted (#70, #98). FreeRealms-Legacy went into `refs/remotes/survey-legacy/*`. `git fetch upstream`
   advanced `upstream/main` from `eefc77d` to `d42ef77` and refreshed `upstream/minigame`.
3. Per ref (script in the session scratchpad):
   - Ahead/behind via `git rev-list --count`.
   - Patch-unique commits via `git cherry upstream/main <ref>`, to spot squash-merged work.
   - Net new content via `git merge-tree --write-tree upstream/main <ref>` diffed against `upstream/main`. This is
     what the branch would still add today, so branches already merged show roughly 0.
   - Conflict counts via the same trial merge against `upstream/main`, `quests` (@ `912fad7`) and `pr-120`.
   Line counts include large JSON data (`ClientItemDefinitions.json` alone accounts for ~1.27M of the biggest
   diffs, mostly a line-ending or format change). They are indicative, not a measure of code volume.
4. Commit messages and bodies, PR descriptions, reviews and comments
   (`gh api repos/.../pulls/N`, `/reviews`, `/issues/N/comments`) were used to judge maturity. Signs of "live-tested"
   were explicit live-client verification, "play testing" or "after testing" commits, or screenshots.
5. Non-fork copies: repo and code search (`namespace Sanctuary.Game`, `Sanctuary.Packet` in csproj), plus a scan
   of every fork owner's non-fork repos for FR-related names. Only `raisingkaines/FreeRealms-Legacy` turned out to
   be a real copy. It is a single squashed commit, so it was matched to its closest upstream commit by minimum diff
   size (`d24be9d`), and its merge risk was checked with a synthetic commit (`git commit-tree -p d24be9d`, an
   unreferenced object).
6. Features nobody has worked on were checked by grepping all commit subjects on every surveyed ref (not in
   `upstream/main` or `quests`) for eye, name, loading screen, starter, deed, mail, craft, vendor, pet and similar.

Cleanup, if the survey refs are no longer wanted:
`git for-each-ref --format='delete %(refname)' refs/remotes/survey refs/remotes/survey-pr refs/remotes/survey-legacy | git update-ref --stdin`
