# Player and enemy stats from the March 2014 live captures

Every number here was decoded from the authentic packet captures in
`yungcomputerchair/free-realms-re/captures/` (the same files, byte for byte, as
`Open-Source-Free-Realms/OpenSourceFreeRealms/ofrserver/Packets/`). The pcap timestamps date them to
**2014-03-25** and **2014-03-31**, the game's last week, not 2010. Compiled 2026-09-11.
It replaces the provisional curve in `src/Resources/LevelStats.json`.

## Summary

- **Max health depends on job type *and* job level. There is no single player value.** The
  hard-coded 2,500 is not a default. It is the **level 20 value of a non-combat job**: the
  capturing character logged in on Adventurer (rank 20), and Sanctuary copied its stats packet.
- **Combat jobs follow one exponential curve:** `MaxHealth = round(500 x 1.15^(rank - 1))`. It
  reproduces all three combat ranks we observed exactly: rank 13 = 2,675 (Wizard), rank 16 = 4,069
  (Brawler, Medic) and rank 20 = 7,116 (Ninja, Archer). A least-squares fit of those points gives a
  ratio of 1.150008 and a rank-1 value of 499.96. Other data backs it up: another player showed
  575 (= rank 2), Frostfang wolves have 760 (= rank 4), and Feathered Drakes have 16,143
  (= 3 x the rank-18 value).
- **Warrior gets x1.1:** rank 20 gives 7,828 (= round(7,116 x 1.1)). Brawler, Medic, Wizard, Ninja
  and Archer have no multiplier.
- **Non-combat jobs use a separate, much lower table:** rank 1 = 400 (Blacksmith, Demo Derby Driver,
  Soccer Star), rank 5 = 800 (Kart Driver), rank 6 = 800 (Miner), rank 20 = 2,500 (Adventurer,
  Postman). We have no points between 6 and 20. The flat 800 at ranks 5 and 6 suggests steps
  rather than a smooth curve.
- **Regen is derived from max health, with the divisor set by job type:**

  | | Combat jobs | Non-combat jobs |
  |---|---|---|
  | HitPointRegen (per second, out of combat) | `floor(Max/20)` | `floor(Max/100)` |
  | InCombatHitPointRegen (per second) | `floor(Max/100)` | `floor(Max/400)` |

  This holds for every job/rank snapshot we captured, Warrior included (391 and 78 from 7,828).
- **Energy (MaxMana) does not scale:** it is 100 for every job at every rank, with ManaRegen and
  InCombatManaRegen both at 4 per second. Only gear raises it: +10 for Medic and Wizard, +20 and
  +1 regen for Warrior.
- **Gear adds flat max health on top of the base:** +712 for Ninja, Archer and Warrior at rank 20,
  +1,231 for Brawler and +490 for Medic at rank 16, and +76 for Wizard at rank 13. The server sends
  it as a separate stat update right after the job's base stats.
- **The fork's curve (`2500 + 250/level`) is wrong at the low end:** at level 1 it gives 5x the
  real combat value (2,500 vs 500). It is close at level 20 (7,250 vs 7,116) only by coincidence,
  and it has no non-combat branch.
- **Enemies:**

  | Enemy | Max HP | Hits on 0-Defense players | Attack interval |
  |---|---|---|---|
  | Frostfang Growler/Prowler/Howler | 760 | ~25 (inferred) | |
  | Frostfang Alpha | 3,800 (5 x 760) | | |
  | Feathered Drake Defender/Ambusher | 16,143 | 206 (309 on a crit) | ~2.4 s |
  | Bixie Soldier | 260 | 25 | |

  All mobs have 800 mana. Defense 348 cut the drake's 206 to 123 (about 40 %), and
  DamageReductionAmount 10 took off another flat 10, down to 113.
- **Player damage:** a rank 20 Ninja's basic attack hits for 2,739, and the Dragonstrike special
  hits 4 targets for 11,207 each. A rank 20 Archer's basic hits for 3,113 and the Volley special
  for 5,404. An unknown-rank player hit a Bixie Soldier for 215.

## Data points

### Capture sessions

| Session | Files | Time (UTC) | Capturing character |
|---|---|---|---|
| A | `packets_1.pcapng`, `packets_1_just_gameserver.pcapng`, `p1.pcap`, `possible_character_data.pcap(ng)` (a 1-second slice holding the SendSelf fragments), first half of `p12.pcap`, part of `p3.pcap` | 2014-03-25 15:36-15:43 | "Maÿhem" (UTF-8 bytes `4d 61 c3 bf 68 65 6d`), guid `0x4B52CC6A9573CA91`, member, login #1455, logged in on **Adventurer 20** |
| B | `packets_2.pcapng` = `p2.pcap` (= second half of `p12`), `packets_3`, `packets_soccer_minigame`, `packets_4`, `packets_racing_minigame`, `packets_demoderby_minigame` | 2014-03-31 23:16-23:58 (one continuous session, client port 61759) | same guid, same character. No SendSelf (the capture starts mid-session), but it has 20 job swaps and several zone reloads that resend the full stat list |

### Job ranks (SendSelf of session A: ClientPcProfile.Rank / RankPercent / StarsAvailable)

| Profile id | Job | Rank | % to next | Stars (cumulative) |
|---|---|---|---|---|
| 1 | Adventurer | 20 | 0 | 24,700 |
| 2 | Ninja | 20 | 0 | 24,700 |
| 4 | Postman | 20 | 0 | 24,700 |
| 11 | Medic | 16 | 15 | 16,033 |
| 12 | Wizard | 12, then **13** in session B | 66, then 8 | 9,900, then 10,532 |
| 14 | Miner | 6 | 93 | 2,433 |
| 16 | Blacksmith | 1 | 78 | 234 |
| 32 | Warrior | 20 | 0 | 24,700 |
| 35 | Archer | 20 | 0 | 24,700 |
| 43 | Brawler | 16 | 44 | 16,625 |
| 45 | Chef | 20 | 0 | 24,700 |
| 48 | Kart Driver | 5 | 13 | 1,795 |
| 49 | Demo Derby Driver | 1 | 16 | 50 |
| 52 | Soccer Star | 1 | 67 | 202 |
| 120 | Card Duelist | 4 | 40 | 1,400 |
| 137 | Fisherman | 20 | 0 | 7,225 |

`ActiveProfileId` = 1 (Adventurer).

### Player stats per job and rank (from the job-swap sequences in session B, 23:20-23:40)

"Base" is the stat update the server sends *before* `ActivateProfile`. It carries the job and rank
stats. "Gear" is the update sent right *after* it, which adds equipped items. Every value is exactly
what the client was told, reconstructed by applying the delta updates in order.

| Job | Type | Rank | Base MaxHealth | +Gear | HitPointRegen | InCombat HPRegen | MaxMana (gear) | Defense | Melee crit % / x | Ability crit % / x | HandToHand dmg | WeaponRange | Other gear stats |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Ninja | combat | 20 | **7,116** | 7,828 | 355 (356) | 71 | 100 | 348 | 5 / 1.25 | 3 / 1.5 | 8 | 5 | DamageReductionAmount 10, DamageMultiplier 1.05, run speed 9.8 |
| Archer | combat | 20 | **7,116** | 7,828 | 355 (568) | 71 | 100 | 348 | 5 / 1.25 | 3 / 1.5 (6 / 2.0) | 8 | 30 | MeleeAvoidance 10, DamageMultiplier 1.05, run speed 9.6 |
| Warrior | combat | 20 | **7,828** | 8,540 | 391 | 78 | 100 (120, ManaRegen 5) | 348 | 5 / 1.25 | 3 / 1.5 (6) | 8 | 5 | |
| Brawler | combat | 16 | **4,069** | 5,300 | 203 | 40 | 100 | 295 | 5 / 1.25 | 3 / 1.5 (x 2.0) | 8 | 5 | |
| Medic | combat | 16 | **4,069** | 4,559 | 203 | 40 | 100 (110) | 295 | 5 / 1.25 | 3 / 1.5 (6) | 8 | 5 | DamageMultiplier 1.1 |
| Wizard | combat | 13 | **2,675** | 2,751 | 133 | 26 | 100 (110) | 246 | 5 / 1.25 | 3 / 1.5 (6) | 6 | 30 | DamageReductionAmount 10 |
| Adventurer | non-combat | 20 | **2,500** | - | 25 | 6 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | |
| Postman | non-combat | 20 | **2,500** | - | 25 | 6 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | |
| Miner | non-combat | 6 | **800** | - | 8 | 2 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | (tool: M3Timer/MimicSpeed 139) |
| Kart Driver | non-combat | 5 | **800** | - | 8 | 2 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | |
| Blacksmith | non-combat | 1 | **400** | - | 4 | 1 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | (tool: M3Timer/MimicSpeed 93) |
| Demo Derby Driver | non-combat | 1 | **400** | - | 4 | 1 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | |
| Soccer Star | non-combat | 1 | **400** | - | 4 | 1 | 100 | 0 | 0 / 0 | 0 / 1.0 | 1 | 5 | |

These values are the same for every job and rank: MaxMana 100, ManaRegen 4, InCombatManaRegen 4,
MeleeChanceToHit 100, MaxMovementSpeed 8.0 (base), and every multiplier stat at 1.0 except those
listed. MeleeAttackIntervalMs is 1000 for Ninja, Archer and Brawler, 0 for Wizard, Medic and
Warrior (see the last caveat), and 2000 for non-combat jobs. EquippedMeleeWeaponDamage is 0 for
combat jobs and 1 for non-combat jobs.

**Session A login (Adventurer 20)**: the 38/7 stat update carried exactly the values in
`ClientPcData`'s constructor today (MaxHealth 2500, HitPointRegen 25, MaxMana 100, ManaRegen 4,
InCombatHitPointRegen 6, InCombatManaRegen 4, MeleeAttackIntervalMs 2000, ...). That is where
Sanctuary's defaults come from.

### Other players seen in the captures (max HP on the wire; rank unknown)

| Player (job per AddPc) | Max HP seen | Reading |
|---|---|---|
| Candi Pearllake (Adventurer) | 400, then 575 after a job swap | 400 = non-combat rank 1-4. 575 = combat rank 2 (500 x 1.15) or rank 1 + gear |
| Violet Vapordream (Blacksmith) | 400 | non-combat, low rank |
| Vapor (Adventurer) | 2,500 | non-combat rank 20 |
| Sareh (Adventurer), Caleb Stormorbit (Brawler) | 400 | non-combat low rank (AddPc's profile id may be stale) |
| ronaldo suares (Brawler) | 400 and 800, alternating (swapping jobs) | non-combat values |
| Mario Hernandez (Brawler) | 3,537, energy 120 | combat rank 15 is 3,538; could be off by one or include gear |

Across all players we only ever saw 400, 800 and 2,500 for non-combat jobs.

### Enemies (op 35/35 HitPointModification and op 32/7 AttackProcessed)

| Enemy (string id, model) | Max HP | Where it fits the player curve | Damage it dealt | Killed by |
|---|---|---|---|---|
| Frostfang Growler (104067, m176), Prowler (116023, m177), Howler (115837, m177) | **760** | = combat rank 4 exactly | vs Ninja 20 (Defense 348, DRA 10): 5 per hit, 3 while Defense was buffed to 462, 12 on a crit. Raw about 25 (inferred) | one Ninja basic hit (2,739) |
| Frostfang Alpha (423045, m176) | **3,800** | 5 x rank 4 | vs Ninja: 9 | Dragonstrike (11,207) |
| Feathered Drake Defender (439211, m3978), Ambusher (m3976) | **16,143** | 3 x rank 18 (3 x 5,381) exactly | **206** raw (vs Adventurer, Defense 0), **309** crit (x1.5), 123 vs Defense 348, 113 vs Defense 348 + DRA 10. One hit every ~2.4 s. Killed the 2,500-HP Adventurer in 13 hits | 6 Ninja basics (2,739) or 6 Archer basics (3,113) |
| Bixie Soldier (440711, m220), session A | **260** | none (0.52 x rank 1) | **25** vs Adventurers with 400 and 2,500 max HP; 22 once | 2 hits of 215 from Candi Pearllake |

Mob mana (35/9): 100 / 800 / 800 on every Frostfang, so all mobs have 800 mana. We found no mob
*level* anywhere on the wire (AddNpc has none), so the "rank 4 / rank 18" readings are inferred from
the HP values.

### Player damage and effects

| Event | Numbers |
|---|---|
| Ninja 20 basic (slot def 4895 Flame Flash, ability 5994) | 2,739 per hit. 3,925 while a buff gave DamageAddition +1,186 (and Defense 462). The crit under that buff was 5,294 = 2,739 x 1.5 + 1,186 |
| Ninja 20 special (slot 4899 Dragonstrike, 100 energy) | 11,207 to each of 4 targets |
| Archer 20 basic (slot 5060 Barrage, ability 4598) | 3,113 per hit |
| Archer 20 special (slot 5061 Volley, 100 energy) | 5,404 |
| "Gift of Health" loot pickup | MaxHealth x4/3 for 15 s (7,828 to 10,411), with regen 473 and in-combat regen 94 |
| Knockout revive, and login | DamageReductionPercent = 100 (invulnerable) for ~7 s after revive and ~42 s after login |
| Health potion (Adventurer) | +394 instantly, then a heal-over-time |
| Regen ticks | HP regen and energy regen both fire once per ~1.0 s. Energy climbs 4 per second from 0 after a special |

## Method

1. Cloned `free-realms-re` (scratch only), and deframed every capture with its `tools/pcap_io.py`
   + `tools/soe_decode.py`, using one deframer per UDP 4-tuple and both directions. Then unwrapped
   the gateway op-5 tunnel (`[u16 5][u8][u32 len][world packet]`).
2. Decoded these world packets. Their layouts come from Sanctuary's serializers, and each was
   confirmed on the wire:
   - **op 12 `PacketSendSelfToClient`** (session A, 204,329 bytes) gives the `ClientPcData`
     header, then 16 `ClientPcProfile`s (Rank at the documented offset; parsing all 16 lands exactly
     on `ActiveProfileId`), then later the 69-entry `Dictionary<CharacterStatId, CharacterStat>`
     (`[key][id][type 0=int/1=float][value]`). **Every stat value in SendSelf is 0.** The real
     stats arrive right after login via 38/7.
   - **op 38/7 `ClientUpdatePacketUpdateStat`** `[u64 guid][count][id, type, value]...`. These are
     **deltas**: at login and zone-in the full list is sent, and after that only changed stats.
     The tables above were rebuilt by applying them in order.
   - **op 38/1 Hitpoints** `[cur][max]` and **38/13 Mana** `[cur][max][u8 showOverhead]`.
   - **op 38/21 `ActivateProfile`**: a length-prefixed `ClientPcProfile` (Rank etc.), then
     attachments.
   - **op 35/35 `HitPointModification`**
     `[u64 source][u64 target][u8][i32 targetMaxHP][i32 targetHPAfter][i32 delta][u8 crit]`.
     Note: upstream PR #74 labels these Guid/Guid2/Unknown2-4. Field meanings were pinned by
     matching regen ticks (+71/+356) against the self HP track.
   - **op 32/7 `CombatPacketAttackProcessed`** (mob melee)
     `[u64 attacker][u64 attacker][u64 target][i32 damage][i32 targetMax][i32 attackId: 5409 normal, 5622 crit, 5631 killing blow][i32 flags][u16][i32 targetHPBefore]`.
   - **op 36/3** ability execute `[u64 caster][u64 target][u64][i32 abilityId]...`, used to label
     hits.
   - **op 35/1 AddPc / 35/2 AddNpc** for names (NameId resolved via `research/strings/en_us.json`).
3. **The old OSFR `ofrserver/Customize/PacketSendSelfToClient.json` is a hand-edited decode of
   this same session-A packet.** Its `Unknown` 665338923 is our LaunchTicket, and its
   `AccountBirthday` 1261854072, pets, mounts and titles all match. It was then edited: name set to
   "Open Source Free Realms", guid set to 1, hair and eye colours changed, position nudged, every
   combat and trade job forced to JobLevel 20 and Adventurer forced to 1 (live: Adventurer 20,
   others 1-20), XP zeroed. Its `CharacterStats` are all 0, which is faithful to the wire, since
   SendSelf carries no stat values. **So that file contains no usable level or health data.** Its
   `Class: 1` = active profile Adventurer.
4. **Useful sequencing for the server**, as observed:
   - **Zone-in:** 38/1 (100, 0) and 38/13 (100, 0) placeholders, then the 38/7 full stat list,
     then 38/1 (max, max), then 38/13 (max, max) twice.
   - **Job swap:** 38/7 job/rank deltas, then 38/21 ActivateProfile, then 38/7 gear deltas, then a
     35/35 self-broadcast (source = target = self, delta 0) carrying the new max and current HP,
     which keeps roughly the same fraction. Swaps that change energy also send 38/1 and 38/13.

The decoding scripts were throwaway (session scratchpad). With the layouts above they are about
150 lines of Python on top of `free-realms-re/tools`.

## Recommendation

Replace `LevelStats.json` with two branches keyed on job type. Put the Warrior multiplier and
per-job gear on top.

- **Combat jobs** (Brawler, Ninja, Archer, Warrior, Medic, Wizard):
  `MaxHealth = round(500 x 1.15^(rank-1))` x job multiplier (Warrior 1.1, others 1.0; round again).
  `HitPointRegen = floor(Max/20)`, `InCombatHitPointRegen = floor(Max/100)`.
  Defense = 246 / 295 / 348 at ranks 13 / 16 / 20. That is about +16 per rank from 13 to 16 and
  about +13 per rank from 16 to 20. Below 13 it is unknown. Suggested interpolation: linear
  `50 + 16.3 x (rank-1)` up to 13, then the observed points.
  Crit 5 % / x1.25 melee, 3 % / x1.5 ability. HandToHand 6 at rank 13 and 8 at 16+.
- **Non-combat jobs** (Adventurer, Postman, Miner, Blacksmith, Chef, Kart and Derby Driver, Soccer
  Star, Card Duelist, Fisherman, ...): observed 400 at rank 1, 800 at ranks 5-6, 2,500 at rank 20,
  with `HitPointRegen = floor(Max/100)` and `InCombatHitPointRegen = floor(Max/400)`. Defense 0,
  crit 0.
- **Energy:** 100 max and 4/s regen for everyone at every rank. Gear adds.
- **Mobs:** use the same curve: `round(500 x 1.15^(level-1)) x multiplier`. Observed multipliers
  are 1 for normal mobs, 3 for the drakes and 5 for the Alpha ("elite/boss"). Mob mana is 800. Mob
  melee damage 25 (low level) to 206 (level ~18 elite) raw. Defense mitigation fits
  `Defense / (Defense + ~515)` and is then minus DamageReductionAmount flat. This is an empirical
  fit, bounded to K in [511, 521] by the drake hits.

Proposed table. **Bold = observed in the captures. Everything else is formula or interpolation.**
In the combat columns the formula is exact at all 3 observed ranks. In the non-combat column, ranks
2-4 and 7-19 are **linear interpolation (rounded to 25)**. The real table may be stepped (ranks 5
and 6 are both 800).

| Rank | Combat MaxHealth | Regen | In-combat regen | Warrior (x1.1) | Non-combat MaxHealth | Regen | In-combat regen |
|---|---|---|---|---|---|---|---|
| 1 | 500 | 25 | 5 | 550 | **400** | 4 | 1 |
| 2 | 575 | 28 | 5 | 632 | 500 *(interp)* | 5 | 1 |
| 3 | 661 | 33 | 6 | 727 | 600 *(interp)* | 6 | 1 |
| 4 | 760 | 38 | 7 | 836 | 700 *(interp)* | 7 | 1 |
| 5 | 875 | 43 | 8 | 963 | **800** | 8 | 2 |
| 6 | 1,006 | 50 | 10 | 1,107 | **800** | 8 | 2 |
| 7 | 1,157 | 57 | 11 | 1,273 | 925 *(interp)* | 9 | 2 |
| 8 | 1,330 | 66 | 13 | 1,463 | 1,050 *(interp)* | 10 | 2 |
| 9 | 1,530 | 76 | 15 | 1,683 | 1,175 *(interp)* | 11 | 2 |
| 10 | 1,759 | 87 | 17 | 1,935 | 1,275 *(interp)* | 12 | 3 |
| 11 | 2,023 | 101 | 20 | 2,225 | 1,400 *(interp)* | 14 | 3 |
| 12 | 2,326 | 116 | 23 | 2,559 | 1,525 *(interp)* | 15 | 3 |
| 13 | **2,675** | **133** | **26** | 2,943 | 1,650 *(interp)* | 16 | 4 |
| 14 | 3,076 | 153 | 30 | 3,384 | 1,775 *(interp)* | 17 | 4 |
| 15 | 3,538 | 176 | 35 | 3,892 | 1,900 *(interp)* | 19 | 4 |
| 16 | **4,069** | **203** | **40** | 4,476 | 2,025 *(interp)* | 20 | 5 |
| 17 | 4,679 | 233 | 46 | 5,147 | 2,125 *(interp)* | 21 | 5 |
| 18 | 5,381 | 269 | 53 | 5,919 | 2,250 *(interp)* | 22 | 5 |
| 19 | 6,188 | 309 | 61 | 6,807 | 2,375 *(interp)* | 23 | 5 |
| 20 | **7,116** | **355** | **71** | **7,828** (regen **391** / **78**) | **2,500** | **25** | **6** |

Energy for every row: 100, regen 4 (in combat 4).

Side note on XP: every rank-20 job reports 24,700 cumulative stars (Fisherman 7,225), and rank 1 to
2 takes 300 stars (234 = 78 %, 50 = 16 %, 202 = 67 %). `RankLevels.json` (100 to reach rank 2,
10,450 total to 20) does not match. The per-rank spans don't fit a simple linear or geometric rule
from these points, so treat them as a table to fill in later.

## Caveats

- **One capturing character.** Combat base HP is observed only at ranks 13, 16 and 20 (plus an
  external rank-2-looking 575). The 500 x 1.15 curve fits those points to the HP, and it also
  predicts the mob HP values. It is still a fit, not a formula read from server code.
- **The non-combat curve between ranks 6 and 20 is unknown.** The linear fill above is a
  placeholder, and a stepped table (e.g. changes every 4-5 ranks) is just as consistent with the
  data.
- **Gear bonuses are item-specific, and their source is not on the wire.** The item definitions in
  the captures (35/37) carry no combat stats. The +712 / +1,231 / +490 / +76 figures belong to this
  character's outfits only.
- **Warrior's x1.1 is one data point** (rank 20). We assumed it applies at every rank, probably from
  a trait.
- **Mob levels are inferred.** "Frostfang = level 4" and "drake = 3 x level 18" come from the HP
  values alone, and the Bixie Soldier's 260 fits no multiplier on the curve.
- **The defense formula is an empirical fit** from three damage pairs. Mob raw damage for
  Frostfangs (25) is inferred, not observed.
- **Mario Hernandez's 3,537 is 1 below the rank-15 prediction (3,538).** His rank and gear are
  unknown.
- The 38/7 stat packets are deltas. Any value not re-sent at a job swap was carried over from the
  previous job, which is why MeleeAttackIntervalMs reads 0 for Medic, Wizard and Warrior. Treat that
  as "unchanged", not necessarily authoritative.
