# Combat port (slice B3): overworld combat and enemies

Branch `combat-port`, 2026-09-11. Ports Sulphural/main's live-tested overworld combat onto our fork, one subsystem
at a time, re-applied onto our files (never a file copy). Scope is the overworld only: the placed Cobblestone
hooligans and wolves fight back, the player hits them with the equipped weapon's basic and special attacks, gets
knocked out and revives, and kills pay stars through the real levelling path. Dungeons, arenas and zoning
(slices B2/C3) are untouched.

Rule followed throughout: **remaster, not reimagine** - authentic numbers from the March 2014 captures where they
exist (`research/level-stats.md`), Sulphural's live-tested mechanics where they don't, everything invented marked
as ours and kept in JSON.

## Commits

| Commit | What | Sulphural source |
|---|---|---|
| `9c155754` | Packets: op32 combat (AutoAttackTarget 32/1, SingleAttackTarget 32/3, AttackTargetDamage 32/4, AttackAttackerMissed 32/5, AttackProcessed 32/7), HitPointModification 35/35, UpdateHitpoints 35/5, UpdateCharacterState 35/20 + `CharacterStatus`, StartCasting 36/3, MeleeRefresh 36/11, LaunchAndLand 36/4, ShowRespawnWindow 41/125, IsFighting 41/133; `BaseEncounterPacket` writes its two header ints | `e9912598`, `c34648df`, `00c1db34`, CarterW24 PR #76 header fix |
| `23c0ff9d` | `Resources/Enemies.json` + `EnemyDefinitionCollection`, `Resources/CombatSettings.json` + `CombatSettingsCollection`, unit tests | formulas/tiers from `12049172`, `c34648df`, model list from `7086796b` |
| `95f91054` | `Entities/CombatNpc.cs` (aggro, chase, leash, return, attack, death, respawn), `Player.Combat.cs` (health, mitigation, dodge, knockout, revive, regen, energy, op41 combat-state driver), `BaseZone` spawns enemies from data and protects role NPCs | `c34648df`, `a126be4b`, `e8c8a3d4`, `00c1db34` |
| `98ef51dc` | `Combat/CombatEngine.cs` (attack pipeline), `Combat/PlayerDamage.cs` (level scaling), op36/op32/op41 gateway handlers, kill XP through PR #120, `Kill` quest goals, `!combat` admin probe | `00c1db34`, `e9912598`, `2567cc0e`, quest kill credit pattern from `c34648df` |

Every commit message carries the Sulphural hashes it ports (AGPL credit).

## What was ported

**Enemies (`CombatNpc`).** Idle -> Pursuing -> Attacking -> Returning state machine at the zone's 10 Hz tick.
Aggro when a live player comes within `AggroRange` (15 u), chase at `ChaseSpeed` (6) with streamed
`ExpectedSpeed` + run-state positions so the client animates a grounded run, leash back to the post beyond
`LeashRange` (40 u) and heal to full on arrival (no leash check while trading blows at melee range - Sulphural's
live fix), re-aggro only once back inside 80 % of the leash (the anti-bounce fix from `a126be4b`), swing every
`AttackIntervalSeconds` with +/-20 % variance, per-hit `AttackProcessed` so the model plays its bite/swing and the
player sees the number. Death = the 2014 capture's recipe (`RemovePlayerGracefully` Animate=true, hold 2 s,
poof 5017) plus a `RemoveNotifications` so a bow's target lock releases; respawn after `RespawnSeconds` (8) at the
post, re-announced to everyone in view. Red name (Disposition 0 + ActiveProfile 1), crossed-swords cursor 11,
nameplate health bar, `IsInteractable=false` (no talk prompt), never a greeting line.

**Hostility is data.** `BaseZone.TrySpawnNpc` (the Lua `spawnNpcWithGuid` path every placed NPC uses) asks
`Enemies.TryResolve(definition)`: a `Species` match on `NameId`/`ModelId`, else the `HostileModelIds` creature
fallback, minus `FriendlyNpcIds`, minus anything the zone protects (`StartingZone.IsProtectedNpc` = quest giver or
target guid). The Cobblestone Hooligan Wolves (`Npcs.json` 3156-3158, 4164-4166), Hooligans (4154, 4155, 4159-4161)
and Hooligan Archers (4157, 4158) go hostile by species; Samantha, Ricky, Shakey, vendors, cows, squirrels, rabbits
and dogs never match. `!npc spawn <id>` spawns a hostile species as a live enemy anchored where you stand.

**Player combat state (`Player.Combat.cs`).** Server-authoritative `CurrentHealth` (the client only mirrors it),
initialised by `ApplyLevelStats` on zone entry / job switch / level-up. Incoming hits: dodge roll (base 0 % +
`MeleeAvoidance`), then `Defense / (Defense + 515)`, then flat `DamageReductionAmount`, then
`DamageReductionPercent`; 0 while invulnerable (~42 s after login, ~7 s after a revive - both captured). Out-of-
combat regen = LevelStats `HitPointRegen` (max/20 combat, max/100 non-combat); in-combat (6 s after the last hit)
= max/100 combat, max/400 non-combat (captured). Energy: always 100, +4/s, a special costs its `EnergyCost`.
Knockout at 0: `IsKnockedOut|IsRooted` state, death poof, chat line, the client's native respawn window with a
10 s countdown; enemies drop the target and walk home. Revive: full health + energy, `CharacterStatus.None`,
revive burst, 7 s invulnerable - by the window's buttons (op41/122: "Revive here", free by default, or the nearest
warpstone POI, type 7) or automatically 12 s in. The op41/132+133 "in world combat" flags are driven only from
the tick thread (Sulphural's race fix) and pulse true->false on exit because the client appliers are edge-guarded.

**Attacks (`CombatEngine`).** A toolbar press (op36/10 on any bar but the item bar 2) or a click-attack (op32/1,
32/3) resolves slot 0 = basic / 1 = special from `CombatJobs.json` + the equipped weapon + `CombatAbilities.json`
(upstream PR #121's toolbar wiring is unchanged). Target = the client's selection if it is a live enemy within
reach x `SelectedTargetRangeSlack`, else the nearest live enemy within the job's reach (`BasicAutoTargetReach` 4 u
melee / `AutoTargetReach` 7 u specials, 30 u bow, 20 u wand). Basic swings are paced by `BasicRecastMs` (600);
specials by their energy. Cast = `StartCasting` (animation + cast FX, broadcast) + `MeleeRefresh` (button grey for
the real lock) + `LaunchAndLand` (radial sweep, target only). The hit lands 85 % into the swing (specials 0.4 s)
as `HitPointModification` (number + bar + recoil, and unlike `AttackProcessed` it does not reset the local melee
timer) + the ability's hit FX; `AoeDamage`/`AoeDamageHeal` hit every enemy within `AoeRadius` of the caster and
`HealAmount` heals the caster (Triage 254). Landing a hit puts you in world combat; swinging at air does not.
Ranged "auto-fire" is a client behaviour (it repeats presses while a target is locked); the server only has to
answer each press and release the lock on death, which `CombatNpc.Die` does.

**Kill XP.** `StartingZone.OnNpcKilled` -> `RewardHelper.TryGrantExperience` (upstream PR #120) for the active
combat job, so a level-up fires the full-screen celebration, the toolbar restore and `ApplyLevelStats`. Sulphural's
`JobLeveling`/`AwardXp` were not ported.

**Kill quest goals.** `QuestGoalType.Kill = 3` with `KillNameIds` + `RequiredCount`; credited in
`QuestManager.OnNpcKilled` by NPC `NameId` whichever spawn died, progress in `GoalCount` like Collect, resent on
login, tracker arrow to the nearest living target. Documented in `src/Resources/QUESTS.md`. No quest uses it yet
("Fighting off the Pack" lands with slice C1).

## What was left out and why

| Left out | Why |
|---|---|
| Sulphural's six `*WeaponAbilities.cs` tables (~300 KB of hard-coded per-weapon abilities, traits, name/icon ids) | Code-as-data; our `CombatAbilities.json` already covers the six creation weapons. Add weapons as JSON rows. |
| Job traits (Archer Reflexes/Lucky Shot, Warrior Counterattack/High Morale, Ninja Grace/Shrouded Armor, Medic Shock Paddles, crit rolls) | Depend on the trait tables above and on ability unlock levels (slice B1 leftover). |
| `ProjectileNpc` travelling arrows/bolts, multishot, lag-compensated muzzle | 22 KB of tuning for a visual; bows and wands hit instantly with the cast FX for now. |
| `StatusEffects`, `PowerupSystem`, `CombatOrbAbilities`, `PotionAbilities`, `CombatBuffs`, `CombatCloneConfig` | Items and drops - not needed for the Cobblestone fights; the `CharacterStatus` bitfield they need is in. |
| `ChaseNavigator` obstacle-aware pathing (`a9e68a17`) | Straight-line chases; the wolves' meadow is open ground. Revisit if enemies clip through Cobblestone's walls. |
| Scripted marches, roaming, sticky animations, snowball-only targets, event spawns | Snow Days machinery. |
| `EnemyStatus`-only "Harmless" and `MarchRelentless` flags, boss plates (`EnableBossDisplay`), party XP sharing | Instances/bosses (C2/C3). |
| Dev probes `!lp`, `!abil`, `!fx`, `!anim`, `!ticon`, `!traits`, `!cast` | Not trivially useful; replaced by the admin-only `!combat` (see below). |
| Enemy mana (35/9 `UpdateMana`, mobs show 800) | Cosmetic; the packet class does not exist in our tree. `Energy` is carried in `EnemyStats` for when it does. |
| "Revive here" coin charge | Kept as a tunable (`ReviveHereCoinCost`, default 0): the wiki's recover-in-place was free. |
| Per-ability cooldowns separate from energy (Sulphural's 10/s regen and 5-10 s specials) | The captures say 4/s and specials cost the whole bar, so the bar *is* the cooldown (25 s). |

## Numbers: capture-derived vs Sulphural's vs ours

| Number | Source |
|---|---|
| Player max health 500 x 1.15^(level-1) combat, 400..2,500 non-combat, Warrior x1.1 | **Capture** (`LevelStats.json`, slice B1) |
| Out-of-combat regen max/20 (max/100 non-combat); in-combat max/100 (max/400) | **Capture** |
| Energy 100, +4/s, specials cost 100 | **Capture** |
| Damage reduction Defense/(Defense+515), then flat, then percent | **Capture** (empirical fit, K in 511-521) |
| Login ~42 s / revive ~7 s invulnerability, knockout recover 10 s | **Capture** / wiki |
| Enemy health curve = the player combat curve; Elite x3, Boss x5 | **Capture** (760 wolves = level 4, 16,143 drakes = 3 x level 18, Alpha 3,800 = 5 x 760) |
| Enemy damage 20 x 1.15^(level-1) (25 at level 4, 206 at ~18) | **Fit through the two captured raw hits** |
| Enemy death presentation (Animate, 2 s hold, poof 5017) | **Capture** (dying pack wolf) |
| Aggro 15 u, leash 40 u, melee reach 5 u, chase 6, respawn 8 s, cursor 11 | **Sulphural** live-tested (`c34648df`, `a126be4b`) |
| Basic swing 600 ms recast, damage at 85 %, special lock 0.4 s | **Sulphural** (660 ms capture median; ours uses PR #121's 600) |
| Tier keyword classifier; Weak/Tough multipliers; Elite/Boss damage and XP multipliers | **Sulphural** (`12049172`), Elite/Boss health corrected to the captures |
| Cobblestone wolves level 1 (500 HP, 20 dmg, 15 stars), hooligans level 2 (575 HP, 23 dmg, 17 stars) | **Ours** (plan §B3 asked for 1-2, not Sulphural's flat 3) |
| Player ability damage = `CombatAbilities.json` value at level 5, x1.15 per level (Brawler basic 145 at level 1, ~2,067 at 20 vs the captured 2,739 Ninja / 3,113 Archer) | **Ours** (`DamageReferenceLevel` / `DamageGrowthPerLevel`); Sulphural's rank^2 ease-in dropped |
| Kill XP 15 x 1.10^(level-1) x tier | **Ours** (no kill-XP data survived; 300 stars level 1->2 = ~20 wolves or the newbie quests) |
| Enemy swing 2.4 s | **Capture** (drake cadence) applied to everything; wolves likely swung faster |
| Dodge chance 0 % base | **Ours**: the captures never showed a miss on the 0-avoidance Adventurer |

## Data files and how to tune them

All hot-reload through `ResourceManager`'s watcher, but a reload does **not** re-stat enemies already spawned -
restart the gateway (or `!npc despawn` / `!npc spawn`) to see new enemy numbers.

- `src/Resources/Enemies.json` - `HealthByLevel` / `DamageByLevel` / `XpByLevel` (index = level-1), `Tiers`
  multipliers, `TierKeywords`, `Defaults` (level, tier, aggro, leash, reach, swing, respawn, chase speed, energy),
  `HostileModelIds` (creature-model fallback), `FriendlyNpcIds` (never hostile), `Species` (`NameIds`/`ModelIds`
  -> `Level`, `Tier`, optional `Health`/`Damage`/`Xp` overrides and any default). To make a new NPC fight back add
  its `NameId` as a species; to make a species harder raise `Level` or `Tier`.
- `src/Resources/CombatSettings.json` - `Player` (out-of-combat window, in-combat regen divisors, defense constant,
  knockout/auto-revive/invulnerability timings, revive cost, knockout/revive FX ids, the two op41 flags), `Enemy`
  (death hold/effect, position broadcast distance, attack cursor), `Abilities` (basic damage-delay fraction, special
  lock/delay, dodge animation/chance, `DamageReferenceLevel`, `DamageGrowthPerLevel`, `SelectedTargetRangeSlack`).
  `SendInWorldCombatFlag` is what makes floating damage numbers render in the overworld; it also puts a health bar
  on every nameplate while in combat (a global client switch, traced by Sulphural). Set it `false` for bar-free
  fights without numbers.
- `src/Resources/CombatJobs.json` (PR #121) - per job: reach, `BasicRecastMs`, energy max/regen, weapon -> ability
  mapping. `src/Resources/CombatAbilities.json` - per ability: `EffectType` (`SweepDamage`, `SingleTargetDamage`,
  `AoeDamage`, `AoeDamageHeal`), `Damage`, new `HealAmount`, `AoeRadius`, `EnergyCost`, animation and FX ids.
- `src/Resources/LevelStats.json` (B1) - player health by level; `RankLevels.json` - stars per level.

## Risks

- **First live test of our re-implementation.** Sulphural's numbers were tuned against their client feedback; ours
  are re-applied through a different toolbar (PR #121) and a different `Npc` base. Most likely problems: the special
  slot not greying (MeleeRefresh vs the client's own sweep), the respawn window not showing its buttons (it needs the
  op41 combat state raised first - `Knockout` does this regardless of the flag), enemies clipping through walls
  (no obstacle routing).
- **Health-bar side effect.** With `SendInWorldCombatFlag` on, every nameplate grows a bar while you are in combat
  (6 s after the last hit). Turn it off if that reads worse than losing the damage numbers.
- **Every nameplate bar off / numbers on is not available** - the client couples them (Sulphural traced it).
- **Hot reload does not re-stat live enemies.**
- **Kill goals are untested in game** - no quest uses them until C1.
- **Hooligan barks**: the hooligans' "SCRAM" line (5100399) in `NpcAmbientLines.json` is now inert because enemies
  never greet. A hostile-bark hook (on aggro) would be the faithful place for it.
- **Melee reach** is 4 u for basics; wolves stop at 5 u. If basics whiff while a wolf is chewing on you, raise
  `BasicAutoTargetReach` in `CombatJobs.json` or lower the wolves' `AttackRange` in `Enemies.json`.
- **Branch name.** `combat` was already held by a stalled worktree (`.claude/worktrees/agent-a64885169895f037a`,
  two commits + uncommitted edits, all reviewed and superseded here), so this work is on `combat-port`. To adopt it
  as `combat`: `git worktree remove --force .claude/worktrees/agent-a64885169895f037a`, `git branch -D combat`,
  `git branch -m combat-port combat`.

## In-client test plan

Servers up (`run_local.ps1`), account `test` (admin), Rick Silverstone. `!` is the console prefix.

1. **Baseline.** Log in on Brawler (or `!combat enemies` on any job). HUD shows 500/500 health (level 1) and
   100 energy. Wait a few seconds after login: nothing regens or flickers.
2. **Enemies are hostile.** Warp or walk west of Cobblestone Village to the three Hooligan Wolves around
   (-2100, -46, 460) (`!whereami` to check). They have red names, a health bar, and the crossed-swords cursor on
   hover; no talk cursor, no "!" badge, no greeting bubble. `!combat enemies` lists them: `Hooligan Wolf L1 Normal
   500/500 hp, dmg 20, 15 stars, Idle`.
3. **Aggro and chase.** Walk to ~15 u of a wolf: it turns, runs at you (smooth run, not sliding), stops next to you
   and bites every 2.4 s; each bite shows a floating number (~16-24 at level 1) and your bar drops. Your weapon comes
   out (in-combat state).
4. **Leash.** Run 40+ u from its post: it gives up, walks home, heals to full, and does not bounce back and forth
   at the edge. Walk back in: it aggros again.
5. **Basic attack.** Press `1` with a wolf selected (or click it): swing animation, cooldown sweep on the button,
   the number pops as the swing connects (~145 per hit at level 1), the wolf's bar drops. Mashing `1` gives one
   swing per ~0.6 s, not one per press. Press `1` with no enemy in reach: the swing plays, nothing is hit.
6. **Special.** Press `2`: Leg Sweep animation, energy drops 100 -> 0, the button greys, every wolf within 10 u
   takes ~508, energy refills 4/s (bar counts up, `2` usable again after 25 s).
7. **Kill and stars.** Kill a wolf (4 basics): death animation, body holds ~2 s, poof, target lock releases, a
   stars reward popup for 15 stars appears, the XP meter moves. After ~20 wolves (or `!exp 43 285` to top up)
   the full-screen Brawler level-up plays and health jumps to 575.
8. **Respawn.** ~8 s after a kill a fresh wolf stands at the post with full health and aggros normally.
9. **Knockout.** Let the pack chew (or `!combat hp 30` then take a bite, or `!combat knockout`): at 0 you drop
   into the knocked-out pose, cannot move or attack, a death poof plays, "You have been knocked out!" in chat, and
   the respawn window opens with a 10 s countdown. The wolves lose interest and walk home.
10. **Revive.** Press "Revive here": you stand up at full health and energy with the revive burst, wolves can't
    hurt you for ~7 s (numbers show `Miss`/0 and your bar stays full), then normal combat resumes. Repeat and press
    "Revive at safe location": you appear at the Cobblestone warpstone (-1904, -40, 413). Press nothing: you
    recover in place after 12 s.
11. **Regen.** Take some damage, walk away: no regen for 6 s after the last hit (in-combat regen max/100 = 5/s),
    then +25/s until full. Switch to Adventurer: health 400, regen max/100.
12. **Friendlies unchanged.** Samantha, Ricky, Shakey still show the talk cursor and greet; the cows, squirrels,
    rabbits and dogs in town are untouched; "Introduce Yourself" still plays end to end.
13. **Hooligans.** The Hooligans (4154...) and Hooligan Archers (4157, 4158) near the village are hostile too: level
    2, 575 HP, 23 damage; the archers engage from ~12 u.
14. **Other jobs.** Switch to Archer: `1` hits a wolf from 30 u away (no projectile yet, the hit FX plays on it) and
    the wolf runs at you; Wizard from 20 u; Warrior's basic hits every wolf within 7 u; Medic's `2` (Triage) heals
    you 254 while hitting the pack.
15. **Tuning check.** Edit `Enemies.json` (e.g. Hooligan Wolf `Level: 2`), save, `!npc spawn 3156`: the new wolf
    has 575 HP. Bad JSON logs an error and keeps the old table.

Log lines to watch (`src/Sanctuary.Gateway/bin/Debug/net9.0/Logs`): `Loaded N enemy species`, `Revive here for`,
and any `damage resolution failed` / `Auto-revive failed` errors.
