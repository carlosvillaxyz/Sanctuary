# Starting area: Sacred Glade tutorial, Cobblestone Village, and what "final build" really means

Compiled 2026-09-11 for the "remaster, not reimagine" rebuild. This is research only; no code was changed.

**Inputs**
- the community NPC layout: `src/Scripts/Zone/FabledRealms.lua` + `src/Resources/Npcs.json` (upstream PR #42)
- `src/Resources/Quests.json` (upstream PR #109)
- the client string table `strings/en_us.json`, including the **4.41M-5.10M id range**, which holds the 2012-13 content
- `client/custom/FabledRealmsAreas.xml`, `src/Resources/Models.txt`, and the streamed-asset manifest
- the Fandom wiki (new pages saved to `wiki/npcs/`)
- ZAM and freerealms.com via Wayback (every URL checked with CDX)
- the capture dump in `npc-spawns/`, and GitHub

**Conventions**
- Positions are FabledRealms zone coordinates `x, y, z` plus a heading in radians, as `FabledRealms.lua` uses them.
- Compass directions come from the quest text: **+x = north** and **+z = east**. Sanctuary lies north; the Blackspore
  passage lies east; Wildwood Speedway lies south; Farnum's Farm lies southwest.
- **guid = 100000000000 + npcId** for every community spawn (for example, Samantha is 2045 -> 100000002045). These
  guids are generated, not SOE's.
- **sid** means a string id in `strings/en_us.json`. Dialogue is cited by sid rather than copied here; the table holds
  it word for word. Quest text sits in contiguous blocks.
- **Town proper** is the client volume `TheWild_SmallVillage_3`: a sphere at (-1867, -46, 445), r = 112, which also
  sets the town music.
- **Outskirts** means inside the client volume `Newbiezone`: a sphere at (-2057, -45, 430), r = 415, carrying
  `AdvJournal RegionOverride=1`. This is SOE's own new-player region, and it covers Cobblestone, Farnum's Farm and
  Wildwood Speedway.

## Summary

- **The brief's "final build flow" is wrong in one respect.** The final build (v1.910, March 2013 - March 2014) did
  bring new players to Cobblestone Village, but through **NPX 4.0 "A Hero Rises"** (PC update, 2013-03-15/17), not
  through Sacred Glade:
  - Darkthorne's **Briarwood Caverns** tutorial (instance `sg_npx_cavern_01`)
  - -> the Farnum family's tutorial chain at **Farnum's Farm**
  - -> **Bartle's** liberation of Cobblestone Village from Mac's hooligan blockade (instance `sg_newbiezone_showdown`)
  - -> Queen Valerian in Sanctuary.
- **Sacred Glade -> Crossroads was the April 2009 launch flow.** Sacred Glade was gone by early 2010, and ZAM had
  marked it obsolete by March 2010. From October 2011 to March 2013, new players started in **Highroad Vale / Merigold
  (Highroad Junction)** instead. Crossroads was renamed Cobblestone Village in March 2013. The details are in the table
  below.
- **The community layout matches the 2013 town.**
  - Bartle and Sheila stand west of town; ZAM's Bartle page also says "west of Cobblestone Village".
  - The hooligans carry the NPX 4.0 name ids 5100400-5100403, and the Hooligan Wolves carry 5100200.
  - Five cows stand where the Brawling Ring used to be; ZAM says the ring had become a cow pen by January 2013.
  - Roosey (Aug 2013) and One of Three (Oct 2013) are present.
  - The 2009 Brawling Ring cast (Carlos Brazenfist, Kibler, Mongo's gate) and Majorie are missing. That fits 2013:
    the ring and the pet tutorial had moved to Highroad Vale in 2011.
  - The layout lacks the **dynamic NPX 4.0 pieces**: Mac, Collin, the Gloam Artifacts, the blockade barrier, the
    Showdown entrance, and the Farnum fires and water well.
- **The quest text survives, apart from one gap.** The string table holds all of:
  - the 2009 Sacred Glade tutorial (9 quests, counting Leaving Sacred Glade)
  - the 2009 Crossroads chains: Introduce Yourself -> Call the Crew! -> Back for More, plus the Brawler, Chef and Pet
    Trainer series
  - the 2013 NPX 4.0 chain (sids 4419543-4420119, 5100200-5100394)

  The one gap: **several NPX 4.0 lines are placeholders or hash-only**, for example "If you see this, please tell Erik
  to fill the text box." Rewards, which the string table lacks, are partly on ZAM.
- **Voice:** 26 mp3s for 5 Sacred Glade NPCs (Michael Tallstrider 6, Farmer Chug 5, Ashley Lightwings 6, Cookie 6,
  Flanders 3). There is none for Cobblestone. For NPX 4.0 there are Darkthorne tutorial talk animations but no Darkthorne mp3.
- **Counts:**
  - town proper: 49 spawns, 19 of them named (16 distinct names)
  - rest of Newbiezone: 198 spawns, 77 named
  - attested but missing: 2009 era, 8 NPCs/objects (Carlos, Majorie, Spot, Kibler, the ring gate, Amara, the potion
    vendor, Sam Potts); NPX 4.0, 6+ (Mac, Collin, 3 Gloam Artifacts, the blockade, the Showdown gate, fires and well)
  - Sacred Glade: 0 NPCs placed anywhere
- **Provenance:** **hand-placed** by the community on the OSFR public server using the final client, then compiled
  from an OSFR spreadsheet that is not public. It is not capture data. Where it overlaps the real captures it is within
  about 1 unit.
- **Separate correction:** the "2010" capture dump in `npc-spawns/` was actually recorded on **2014-03-25 and
  2014-03-31**, according to its pcap timestamps.

## Timeline of the new-player experience

String ids were not allocated strictly in time order. The 94k-141k blocks already existed in 2009 (ZAM captured
Ricky Danger in October 2009), and the 2012-13 content moved to the 4.41M+ and 5.10M+ ranges.

| Era | Dates | Tutorial | First town and chain | sid blocks | Evidence |
|---|---|---|---|---|---|
| Beta | 2008 - early 2009 | **Sacred Glade v1** (`sg_tutorial`, 42 chunks). Michael and "Ashley Lightwing" take you to a *female* Farmer Chug; you do a 4-cow roundup, then Gate Keeper Furbin and Tutorial Gates One-Three | Early Crossroads: Samantha's greeting, Carlos's "Trainer: Fighter", Kibler's "Beat da Champ", Simone's cafe with Bartle. Mostly `NOT LIVE` | 25-2118; 20138-24945 | `NOT LIVE` prefixes; low ids |
| **Launch** | Apr 2009 - ~end 2009 | **Sacred Glade v2** (`sg_tutorial_02`, 54 chunks): Wilbur, the pig pen, Ashley's party, Cookie and Flanders | Ashley teleports you to **Samantha in Crossroads** (42205), Merigold in Highroad Junction (116678) or Valerie in Stillwater Crossing (116682). Each town has its own mutually exclusive "Introduce Yourself" | tutorial 41880-48002, 70001-75891; Crossroads 94007-94637, 100150-101048, 103188-103358, 139442-141528 | ZAM Sacred Glade (2009-06-01); ZAM Crossroads (2009-11-01) |
| Town start | ~2010 - Oct 2011 | none. Sacred Glade removed; ZAM tags it "obsolete" by 2010-03 | the same three towns (the wiki says new players "enter Free Realms here"). Exact trigger unknown | same as Launch | ZAM obsolete tags |
| Highroad Vale | Oct 2011 - Mar 2013 | none | **Highroad Vale.** Reggie Skylight greets you (366, 400) and sends you on. The Chatty "dpp" guided path (437348-437493) covers a housing door, the King of the Ring battle gate and Alena Goldenheart's Pet Adventures (436893-436965), then "Take Me There" -> **Merigold** (437403). The King of the Ring and the pet tutorial move here | 366, 376-401, 436893-437739 | freerealmsinsider patch notes 2011-10-28; wiki `Highroad_Vale`; ZAM Kibler page (2012) |
| **Final (NPX 4.0)** | 2013-03-15/17 - 2014-03-31 (PC; never on PS3) | **Briarwood Caverns** (instance `sg_npx_cavern_01`) with Darkthorne: Camera -> Movement -> Basic Combat -> Advanced Combat Tutorial -> Hero's Journal -> The Last Best Hope -> teleport to Farnum's Farm | Farnum's Farm chain (Mae, Cleetus, Pappy) -> **Where's the Help?** -> Bartle in **Cobblestone Village** -> Find and Pull the Plug -> **Cobblestone Showdown** (instance `sg_newbiezone_showdown`, boss Mac) -> Off to the Queen -> Visiting the Job Center | 4419543-4419674, 4420050-4420119, 5100200-5100394 | freerealms.com article 2875; ZAM (2013 notes); freerealmsinsider 2013 |

The name "Crossroads" does not appear anywhere in the string table. The whole table was rewritten
Crossroads -> Cobblestone Village in 2013; sid 431236 still contains the resulting broken script name
`PetTutorial.Cobblestone VillageEnd`. "Crossroads" survives only in area volumes (`Crossroads_*`), the zone file
`sg_crossroads_pet_tutorial.gzne`, and ZAM/wiki page titles.

## Cobblestone Village roster (community layout)

### Town proper (`TheWild_SmallVillage_3`)

| Name | npcId | x, y, z / heading | Model | Role | Era | Wiki | Quests | Dialogue (sid) |
|---|---|---|---|---|---|---|---|---|
| Samantha | 2045 | -1879.2, -45.9, 442.8 / -2.26 | fairy_f_merigold.agr | Quest giver: town greeter, "down the path from the warpstone" (ZAM) | 2009-14 | `npcs/Samantha` (stub, pixie) | Gives Introduce Yourself, Brawler: Bringing Back Brazenfist, Chef: Assistant Needed. Takes Back for More | 94246, 101031, 103188, 94265, 140530, 140522. Beta greetings 21769, 24940. Warpstone tip 47891 |
| Ricky Danger | 2049 | -1909.5, -44.4, 462.8 / 1.69 | fairy_m_ricky_danger.agr | Quest giver: kart driver who crashed into the lamppost by the ring (94360) | 2009-14 | none | Target of Introduce Yourself. Gives Call the Crew!. Leads into Back for More | 94388, 94254, 101035, 94257, 94261, 94262. Crash barks 94013-94058 |
| Simone | 2069 | -1830.9, -42.0, 428.8 / -0.02 | human_f_simone.agr | **Chef trainer** (unlocks the job) at the Crossroads Cafe cooking table | 2009-14; the 2013 job center still sends chefs here (442236) | Chef page | Target of Chef: Assistant Needed. Gives Chef: Crossroads Cafe (string "Cobblestone Village Cafe") and Chef: Delivery Derailed | 140525, 140529, 94274, 94278, 100259, 94290, 103190 |
| Bartle | 2087 | -1860.2, -51.3, 341.1 / -0.11 | chugawug_m_02.adr | **Quest giver (NPX 4.0 hub)**, west of town at the blockade. In beta he was Simone's ingredient merchant | beta; 2013-14 | none (ZAM `FR_Mob:Bartle`) | Target of Where's the Help?. Gives Find and Pull the Plug, Cobblestone Showdown and Off to the Queen | 4420092, 4420097, 4420100, 4420105, 4420108, 4420113, 5100380, 5100382, 5100384. Bark 20945; gossip about him 70047-70048 |
| Sheila | 2088 | -1861.9, -51.2, 344.2 / 0.97 | horse_f.adr | Bartle's pink horse. She **reappears after Cobblestone Showdown** (4420108), so she should spawn conditionally | 2013-14 | none | Showdown reward state | 5101211 |
| Roosey | 2082 | -1842.2, -41.4, 411.2 / -1.38 | fairy_f_merchant_brawler_african_raven.agr | Merchant: Brawler gear | from 2013-08-06 | `npcs/Roosey` | none | name only |
| Gloria | 2057 | -1850.7, -41.1, 477.4 / 2.87 | human_f_freestyle_10.agr | Merchant: Pet Pals, at the Pet Adoption Center | 2013+ wiki | `npcs/Gloria` | none | 101822, 431127 |
| Jennifer | 2058 | -1852.8, -40.9, 476.7 / -2.78 | human_f_freestyle_03.agr | Merchant: pet supplies (ZAM 2009) | 2009-14 | none (ZAM) | Pampered Pets pickup (91471) | 101822 |
| Walker | 2059 | -1845.6, -41.5, 469.8 / -2.04 | fairy_f_freestyle_09.agr | Merchant: **Pet Collars**. ZAM 2009 confirms this; the name sits in the same block as the Highroad and Stillwater collar vendors | 2009-14 | none (ZAM) | none | 116867 |
| Zax, Rascal, Iggy, Dot | 2063, 2062, 2060, 2061 | about -1840, -41, 470-481 | tyrannosaurus_m, dog_small_fluffy, triceratops_m_basic, dog_large_dalmation | Objects: adoption-center pets you can try or buy (101822) | late | pet pages | none | none |
| Cow x5 | 2050-2054 | -1898 to -1910, -43, 476-487 | cow.adr | Ambient: the **cow pen that replaced the Brawling Ring** (ZAM, by Jan 2013) | 2013-14 | none | none | none |
| Cobblestone Village Warpstone | 2254 | -1912.5, -38.5, 408.2 / 1.11 | sg_warpstone_01.adr | Object: warpstone, which also gives the first exploration collection item (ZAM). The server's new-character spawn point (-1904.9, -39.7, 412.6) is 9 u from it | all | Warpstones | Warpstone Woes (NOT LIVE) | 47891 |

**Unnamed spawns in town (30)**
- Townsfolk clusters at (-1852, 413), (-1862, 489), (-1807, 483-501) and (-1766, 457).
- Crash scene next to Ricky: `human_m_racer_repair_guy` 2048 (the "Car Repair Guy", sid 94010) and onlookers 2046
  and 2047.
- A chugawug beside Simone (2070) and a pixie miner (4327).
- Critters, and `invisible_cube_butterflies` 31347.
- **Suspect strays:** `knocker_m_boss` 17596 beside the warpstone, `hsg_int_hum_coffeetable` 31570, and the human +
  `dragon_m_basic` pair 2073/2074.

### Outskirts (inside `Newbiezone`)

| Name | npcId | x, y, z / heading | Model | Role | Quests | Dialogue (sid) |
|---|---|---|---|---|---|---|
| Jones | 2056 | -1895.7, -39.6, 567.5 / -3.02 | human_m_sirwellington.agr | Quest giver (Brawler) at a camp on the road east to Blackspore | Target of Building your Rep. Gives Delinquent Dilemma and Bad Business | 94297, 94302, 94305, 101028, 103353, 46346 |
| Billie JoBob + Billie's Trunk | 4152, 4153 | -1937.1, -37.4, 565.9 / 2.10 | human_m_freestyle_03 / sg_treasure_chest_01 | Quest target (Blacksmith), "outside the passage to Blackspore" / "by the pond" | Target of Blacksmith: Breaking Bank (from Mili Ironcall). Takes the Glistening Scythes (95170-95172). Points you to Silverray (95184) | 94579, 94662, 95136 |
| Jonelle + Jonelle's Mailbag | 4096, 4095 | -1722.3, -31.6, 417.9 / -1.43 | human_f_courier.agr / bw_sack_02 | Quest giver (Chef) on the Sanctuary road north. The mailbag is a minigame object | Target of Delivery Derailed. Gives Package Recovery! and Chef: Queensfields | 94293, 94298, 94301, 100541, 100542, 101044, 101046 |
| Hooligan Brawling Club! + 3 Hooligans + 2 Hooligan Archers | 4156-4161 | gate -1823.0, -33.3, 567.1; mobs -1780 to -1794, 556-572 | sg_spawner_human_01; human_*_hooligan_* | Enemies plus a gate to an instanced battle (75439-75442) | none | 74374 |
| Hooligan x2 | 4154, 4155 | about -1969, -35, 542 | hooligan brawlers | Enemies (name id 5100401 = NPX 4.0 hooligan). They stand where Delinquent Dilemma's "south of Jones" patrol is (94304) | Delinquent Dilemma; NPX 4.0 blockade | 5100399 ("SCRAM" bark) |
| Hooligan Bullies! + 3 Hooligan Wolves | 4162, 4164-4166 | gate -1924.4, -52.2, 275.3; wolves about -1925, -49, 296 | sg_spawner_human_01, wolf_generic | Enemies plus a battle gate (74753-74757). The wolves are the NPX 4.0 **Fighting off the Pack** targets (5100200, 5100203, 5100219) | Fighting off the Pack; Brawler: Night Fights (73005-73016) | none |
| Hooligan Wolf x3 | 3156-3158 | about -2100, -45, 460 | wolf_generic | Enemies | Fighting off the Pack | none |
| **Eddie Eagle Eyes** | 4326 | -2094.7, -37.9, 526.1 / 2.56 | human_m_hooligan_archer_01.agr | Quest giver: Archer bounty contracts, "just south of Cobblestone Village" | Contract: All Around Win (388246), Contract: Bowman for Hire (388258), Contract: No Boundaries on Bounties (388331), Contract: That'll Teach 'Em (388483) | 388554, 388247, 388250, 388262, 388335. **The 2014 capture puts him at -2093.83, -37.99, 525.64, heading 151.5 deg** (0.95 u from the layout) |
| One of Three | 4167 | -2020.2, -33.1, 550.6 / -2.63 | human_m_wraith_white.agr | Enemy from the Super Spooktacular event (2013-10-01); **seasonal** | 5102458 | none |
| Lost Sheep | 23627 | -1963.6, -48.9, 251.2 | ewe.adr | Object: Bo's sheep-shearing (Farnum) | 386531 | 20099 |
| Outlying hooligan camps | 4667-4675; 23623-23626 | about -2023 to -2100, 637-703 (9 mobs); about -1865, -56, 175 (4 mobs, name id 20483) | hooligans | Enemies | Eddie's contracts; NPX 4.0 (Collin's camp is probably one of these) | none |

### Farnum's Farm (next door, about 300 u southwest; on the wiki under Category:Cobblestone Village)

Farnum's Farm is where the final-build player actually arrives. The layout places the **NPX 4.0 hosts** there:
- **Pappy Farnum** 3211 at (-2185, -32, 328): The Last Best Hope turn-in (4419612), One More Gloam (4420081-4420087,
  5100268-5100284), and he starts Where's the Help? (4420089).
- **Mae Farnum** 2538 at (-2113, -57, 209): Cooking 101 / Farming 101 (4420050-4420055); she also sells recipes.
- **Cleetus Farnum** 3232 at (-2078, -56, 201), with model `human_m_andrew.agr`:
  - Gear Up! and Mine and Smelt (4420057-4420063)
  - Your First Weapon (4420065-4420071)
  - Fighting off the Pack (4420073-4420079)
  - wiki: `quests/Your_First_Weapon` (16 coins) and `quests/Fighting_off_the_Pack` (18 coins)

Also placed, with 2009-2013 quests in the string table:
- **Anne** 3235: Investigate the Wolves, Save the Sheep! (Sheep Watch)
- **Andy** 4777
- **Henry** 3234: Hooligan Hassles, Highroad Hijinx, the Wompugg chain
- **Rocky** 3228 (2013 gear)
- **Papa Salt** 3229 and **Houston** 3224: Garden Defense
- **Hartley Harrison** 3230
- **Estus** 3233 and **Vela** 3231 (2013 gear)
- **Aunt Daisy** 3216
- **Riley** 3214
- **Maple Sugarleaf** 3213 (TCG)
- **Tamara** 3210
- **Uncle Irving** 3220, plus Checkers & Chess 3219
- **Bo** 3223
- **Wompugg** 3238
- **Vicki** 3775 (start of the Archer chain)
- Cooking Table 2537 and the Garden Defense tables

### Wildwood Speedway (the Call the Crew! target, about 420 u south)

Shakey 3018 is at (-2246.5, -16.8, 599.3); the capture has him at (-2260.2, -16.6, 594.3) and (-2254.8, -16.7, 576.6).
He wanders, so the layout is 14 u off. The other speedway NPCs sit within 0.3-2 u of the capture:
- Johnny Thunder 3010 (Kart trainer) and Mad Jack 3009 (Derby trainer)
- Linx Redline 3015, T.R. Walker Jr. 3013, Verve McNichols 3012
- Blammo 3011, Jace Wildwheel 3021, Big Paulie 3022, Click 3023
- Hasti 3016 and Lola 3017

## Missing NPCs (attested, not in the layout)

This comes from a whole-world search of `Npcs.json` by `Name` and by model file.

| NPC or object | Era | Attested by | Role and location | Model | Placed? |
|---|---|---|---|---|---|
| **Carlos Brazenfist** | 2009 - Oct 2011 | Wiki; ZAM `FR_Mob:Carlos_Brazenfist`; sids 20671, 140531-140551, 94282-94285, 94587, 100382, 103260, 103322, 431108 | **Brawler trainer**, "near the Brawling Ring to the east" (103260), in front of the ring gate (94585) | `human_m_carlos_brazenfist.agr` (Models 964) | **No.** His model is reused by Fernando Flexsteel (2950) in Blackspore. The 2013 job center sends brawlers to Caitlyn Gravefog instead (442235) |
| **Majorie** + Spot | 2009 - Oct 2011 | Wiki; ZAM 2009 Crossroads page ("at the Adoption Center gate"); sids 78658, 94250-94253, 94357-94358, 101029-101033, 141096-141528, 101968 | **Pet Trainer** "across the street, in front of the Pet Adoption Center". The lesson runs in instance `sg_crossroads_pet_tutorial.gzne` | Spot is probably `dog_large_spot.agr` (1044) | **No** |
| **Kibler** | 2009 - Oct 2011 in town; later Highroad Vale | Wiki; ZAM; sids 46350-46361, 94282, 434614, 434952-434955 | Robgoblin ring announcer at the Brawling Ring | unknown | **No** |
| Brawling Ring gate / King of the Ring! (Mongo) | 2009 - Oct 2011 | Wiki; sids 94584-94585, 436646, 437381 | A gate east of Samantha, next to Ricky's lamppost. Mongo is the boss inside the instance. Replaced by the cow pen | a gate spawner | **No** (and the 2013 layout correctly has cows there) |
| Amara Hearthsong | beta / 2009 | sids 46076-46082, 46449-46469 | Tavern Keeper in Cobblestone. Lending a Hand points to the Medic; her cellar quest is NOT LIVE | unknown | No |
| Potion vendor | 2009 | Wiki "Shops: Potions"; sid 21809 ("Joseph by the wagon") | Potions | unknown | No |
| Sam Potts | beta | sid 94625 (Samantha's list of locals) | Card duelist; she later lives in Sanctuary | none | No (not in `Npcs.json`) |
| **Mac** | 2013-14 | sids 5100344, 5100362, 4420097-4420111; ZAM `FR_Mob:Mac` | Hooligan boss inside **Cobblestone Showdown** (`sg_newbiezone_showdown`), entered from "the cave leading to Blackspore Swamp" | `human_m_mac.agr` (+ `human_hooligan_boss` rigs) | **No** (he is an instance boss; the **instance entrance** is missing too) |
| **Collin** | 2013-14 | sids 5100267, 5100334 ("NPX 4.0 NewbieZone Collin"); ZAM mentions "the camp where you fought Collin" | Hooligan raid leader. His fight is probably "Fight Raid Leader" (4420085) | `human_m_collin.agr` | **No** |
| **Gloam Artifact x3** + the blockade barrier | 2013-14 | sids 5100327-5100329, 5100332, 4420102, 5100336-5100340 ("NPX 4.0 Barriar Grant 1-3"), 1345905211 "Hooligan Blockade Gloam Fire" | "Just north of Cobblestone Village and south by Collin's camp" (ZAM). The barrier seals the road to Sanctuary until the Showdown is won | unknown | **No** |
| Fires + Water Well (Farnum) | 2013-14 | sids 5100342-5100360, 5100394 | A put-out-the-fires task at Farnum after the Gloam attack | unknown | No |
| Gloam Creature ("One More Gloam") and Gloamlings | 2013-14 | sids 5100268, 5100272, 5100374 | "north of Wildwood Speedway" | unknown | No |
| Sacred Glade cast | 2009 | see the tutorial section | | models exist | **None anywhere** |
| Highroad Vale cast (Reggie Skylight, Alena Goldenheart, Chatty guide) | Oct 2011 - Mar 2013 | see the timeline | | | None |

## Unattested NPCs (placed; flag, don't delete)

| Name | Status after the Wayback/string check | Assessment |
|---|---|---|
| Walker, Jennifer | ZAM 2009 confirms them as pet vendors | Keep |
| Bartle, Sheila | Confirmed as the NPX 4.0 quest hub (ZAM plus sids 4420089-4420113) | Keep. Spawn Sheila only after the Showdown |
| Cows x5 | Confirmed: the ring became a cow pen by January 2013 (ZAM) | Keep for 2013 |
| Hooligans, Hooligan Archers, Hooligan Wolves | Confirmed as NPX 4.0 mobs (name ids 51004xx and 5100200) | Keep |
| Billie JoBob | String table; ZAM | Keep |
| Zax, Rascal, Iggy, Dot | Adoption-center pets (101822); the exact pet list is unverified | Keep |
| One of Three | Halloween 2013 | Move to a seasonal event |
| Roosey | 2013 | Keep for 2013-14 |
| `knocker_m_boss` 17596, coffee table 31570, dragon pair 2073/2074 | Nothing attests them | Prune candidates |

## Quest chains that start in Cobblestone Village

Coin rewards below are from ZAM (2009 captures) unless marked otherwise. `Quests.json`'s rewards (30 coins and a
boombox plus a "Ninja's Shadow Blade of Dragonstrike" for Introduce Yourself) are placeholders, and its quest ids
(2563, 1801 ...) are the PR author's own. "Verbatim" means every line is in the string table.

### F. Final build (NPX 4.0, 2013-14): Farnum's Farm -> Cobblestone Village

| Quest (title sid) | Giver -> target | Steps | Dialogue sids | Reward (ZAM) |
|---|---|---|---|---|
| The Last Best Hope (4419563) | Darkthorne (Briarwood Caverns) -> Pappy Farnum | Defend Darkthorne 4419610; talk to Pappy 4419612 | 4419564, 4419565, 4419642, 4419671-4419674 | ? |
| Cooking 101 (4420050); Farming 101 (4420053) | Mae Farnum | Harvest and cook Strong Arm Stew | 4420051-4420052, 4420054-4420055 | ? |
| Gear Up! / Mine and Smelt (4420058, 4420061) | Mae -> Cleetus Farnum | Mine and smelt copper | 4420057, 4420059-4420063 | ? |
| Your First Weapon (4420066) | Cleetus | Forge a Spunky Scrapper Hammer 4420071 | 4420065, 4420067-4420070 | 16 coins (wiki) |
| Fighting off the Pack (4420074, 4420077) | Cleetus | Equip the hammer; knock out 5 Hooligan Wolves (5100203) | 4420073, 4420075-4420079, 5100219 | 18 coins (wiki) |
| One More Gloam (4420082) | Pappy | Fight the raid leader 4420085; defeat the Gloam Creature north of Wildwood 5100268-5100272; return 5100270 | 4420081, 4420083-4420087, 5100274, 5100284 | ? |
| **Where's the Help?** (4420090, 4420093) | Pappy -> **Bartle** (Cobblestone Village) | Talk to Bartle 4420095 | 4420089, 4420091-4420092, 4420094, 5100324 | ? |
| **Find and Pull the Plug** (4420101; also "Pull the Plug" 4420098) | Bartle | Destroy 3 Gloam Artifacts (5100332, 4420102); return 5100286 | 4420097, 4420099-4420100, 5100380 | 24 coins |
| **Cobblestone Showdown** (4420106, 5100367) | Bartle | Defeat Mac inside the Showdown instance (4420110-4420111, 5100372); return 5100292. Afterwards Sheila is back | 4420105, 4420107-4420109, 5100382 | 28 coins. Showdown prize wheel: 26 coins + gear |
| **Off to the Queen** (4420114, 4420117) | Bartle -> Queen Valerian (Royal Palace) | Enter the Royal Palace 4420119; talk to the Queen 5100330 | 4420113, 4420115-4420116, 4420118, 5100384 | 30 coins |
| Visiting the Job Center | Queen -> Sanctuary job center | none | the 442223-442282 job-center hints ("Speak to Simone in Cobblestone Village" 442236) | ? |

Status: titles, objectives and the Farnum, Bartle and Queen lines are verbatim. Darkthorne's cavern script is only
partly in the table: 4419545-4419557 are "please tell Erik to fill the text box" placeholders. ZAM's Briarwood Caverns
page transcribes her live lines.

### A. 2009 new-player chain (Crossroads / Cobblestone)

| # | Quest (title sid) | Giver -> target | Steps (objective sids) | Dialogue sids | Reward |
|---|---|---|---|---|---|
| 0 | Leaving Sacred Glade (42210) | Ashley -> Samantha | 42207, 42211, 47950, 47951 | 42205 | ? |
| 1 | **Introduce Yourself** (94247; description 94360, 94248) | Samantha -> Ricky Danger | 94359 | 94246, 101031, 94388; prerequisite marker 387300 | 5 coins. Verbatim. `Quests.json` 2563 |
| 2 | **Call the Crew!** (94255; description 94256, 94260) | Ricky -> Shakey -> Ricky | Convince Shakey 94487. Conditional: Johnny Thunder 94493 (no Kart job: 94490, 94492); Mad Jack 94501 (no Derby job: 94498, 94500). Return 94511, 94512, 384143 | 94254, 101035, 94519, 94258, 384144, 101038, 384129, 384137, 94257, 94261; journal 139442 | 7 coins. Verbatim. `Quests.json` 1801 |
| 3 | **Back for More** (94263; 94264, 100151) | Ricky -> Samantha | 100150, 101040 | 94262, 94265 | 2 coins. Verbatim. Missing from `Quests.json` |
| 4 | Hub: Samantha offers the job quests (B, C, D) | | | 140530, 140522, 94357-94358, 101029 | |

### B. Brawler (wiki "Trainer Series: Cobblestone Village"; 2009 - Oct 2011)

| Quest | Giver -> target | Steps | Dialogue sids | Reward (ZAM) |
|---|---|---|---|---|
| Brawler: Bringing Back Brazenfist (140531; 140532, 140551) | Samantha -> Carlos (unlocks Brawler) | 140543, 140550 | 140530, 140533, 140537, 73588, 103260 | 6 coins |
| Brawler: Honor Lost (94283; 94284, 94587, 100382) | Carlos | Ring gate 94584 / 94585; defeat Mongo 94586; tell Carlos 94602 | 94282, 103322, 47297, 94285 | Saved by the Bell Brawler pants and boots |
| Brawler: Building your Rep (94295; 94296, 100191) | Carlos -> Jones | 100152, 100153 | 94294, 101048, 103327, 94297 | Brawler's Power Shard of Vitality I; 6 |
| Brawler: Delinquent Dilemma (94303; 94304, 101027) | Jones | 100184, 100554, 100555 (Pilfered Artifact 100558); return 100188, 100556 | 94302, 101028, 94305 | ? |
| Brawler: Bad Business (103354; 103355, 103358) | Jones -> Sabastian Cabet (Blackspore, npc 1552) | 103359, 103360, 46345 | 103353, 46346, 103356 | ? |
| Brawler: Given to Guard (130857) | Sabastian -> Petra; leaves the area | 130858 | 130856, 130859 | ? |

### C. Chef (2009-14; Simone was still the Chef trainer in 2013)

| Quest | Giver -> target | Steps | Dialogue sids | Reward (ZAM) |
|---|---|---|---|---|
| Chef: Assistant Needed (140523; 140524, 140547) | Samantha -> Simone (unlocks Chef) | 140539 | 140522, 140525, 140529 | ? |
| Chef: Crossroads Cafe (string 94275 "Chef: Cobblestone Village Cafe"; 94276, 100247) | Simone | Cooking table 94528; 2 Spiralmint Steaks 94534; return 94536 | 94274, 94278, 100259, 103190 | Amateur Chef shoes and pants |
| Chef: Delivery Derailed (94291; 94292, 100159, 100536) | Simone -> Jonelle | 100158, 94623 | 94290, 101041, 94293 | Amateur Chef Square Shard |
| Chef: Package Recovery! (94299; 94300, 100537) | Jonelle | Mailbag 100538 / 100539; minigame 100540; return 139895 / 139896 | 94298, 101044, 94301, 100541 | ? |
| Chef: Queensfields (100543; 100544, 100549) | Jonelle -> Honey (Queensfields) | 100550, 100551, 71042-71044 | 100542, 101046, 100545 | ? |
| Chef: Anyone Can Cook! (140470) | job finder -> Simone / Auguste / Helena | 140471-140496 | | 6 coins + Chef job |

### D. Pet Trainer (2009 - Oct 2011)

| Quest | Giver | Steps | Dialogue sids | Reward (ZAM) |
|---|---|---|---|---|
| Pet Trainer: Pet Basics / "Pet Tutorial" (94251; 94252, 101032) | Majorie (unlocks Pet Trainer) | Click Spot 94480 / 94481; tutorial 94485 / 94486; return 141096 / 141097 | 94250, 94378, 101029, 101033, 103461, 94253 | 11 coins + Doggy Leather Collar (red) |
| Pet Tutorial: Basics / Happiness / Training (141145, 141150, 141154) | Majorie + Spot, in `sg_crossroads_pet_tutorial` | 141494-141528 | 141144-141162 | ? |
| Pet Trainer: Speak Up! (90019) -> To Sanctuary! (90027) | -> Roscoe (Sanctuary) | 90020, 90028 | 90018, 90021, 90026, 90029 | Speak Up: 38 coins + Pet Trainer Cap |

### E. Other quests here

- **Archer contracts** (Eddie Eagle Eyes, Archer levels 5, 10, 15 and 20).
- **Blacksmith: Breaking Bank** (94577; ends at Billie JoBob). Glistening Scythes goes to Billie as well.
- **Pampered Pets** (91464-91473; Jennifer pickup).
- **Art of Brawling** (139686-139700).
- **Beta / NOT LIVE:**
  - Beat da Champ (21770-21777)
  - Cobblestone Village Cafe v1 (21827-21841)
  - Help Simone (22453-22457)
  - Wondrous Warpstones / Samantha's Mail (46156-46181)
  - Warpstone Woes (77433-77455)
  - What's a Checker? (46350-46361)
  - Basement Brawl (46449-46469)
  - Samantha's "introduce yourself to Simone, Carlos, Aunt Daisy and Sam Potts" (94625-94628; title not found)

## Sacred Glade tutorial (April 2009 launch, `sg_tutorial_02`)

### Roster

| NPC | Name sid | Model (Models.txt id) | Role | Voiced mp3s |
|---|---|---|---|---|
| Michael Tallstrider | 1921 | human_m_michael_tallstrider.agr (448) | Guide. Gives Wilbur's Trouble and unlocks Adventurer (47526) | 6: `MichaelTallstriderDialog569`, `3329`, `3329.5`, `3398`, `3646`, `MIchaelTallstriderDialog3647` |
| Wilbur | 42017 | pig.adr (137) is likely. `pig_m_basic.adr` (3286) is the 2011 Wilds Farm Wilbur | Escort target. Afterwards he follows you like a pet | none |
| Farmer Chug | 359 | Chugawug with texture `chugawug_m_farmer_farmerchug.dds` | Gives Pig Pen Rescue and Perilous Party. Returns in April 2011 on the Wilds Farm (434254-436209) | 5: `FarmerChugDialog3331`, `3333`, `3335`, `3649`, `3651` |
| Ashley Lightwings | 42111 (beta "Ashley Lightwing" 1925) | fairy_f_ashley_lightwings.agr (447) | Party host and job branch. Heals you in the combat tutorial (45597-45598). Radial menu with shop, checkers and chess. Teleports you out | 6: `AshleyLightwingsDialog3337`, `3339`, `3348`, `3454`, `AshleyLIghtwingsDialog3653`, `AshleyLightwingsDIalog3655` |
| Cookie | 42163 | human_f_cookie.agr (507) | Chef tutorial at her chuck wagon, southeast | 6: `CookieDialog3342`, `3346`, `3381`, `3657`, `3659`, `3661` |
| Flanders | 42164 | human_m_flanders.agr (509) | Brawler tutorial at the practice yard, southwest | 3: `FlandersDialog3344`, `3363`, `3663` |
| Robgoblins | Robgoblin Ruffian 42018; Tutorial Special Robgoblin 75549 | robgoblin_f_basic_tutorial.adr (582), robgoblin_m_basic; `robgoblin_carrying_pig` (45347) | Warpstone breakers, Wilbur's captors, the pig-pen gang, the party crashers (encounter NPCs 4550 and 4551) | Barks 41912-41921, 41926, 41932, 42019, 42086-42087, 45870, 46113-46115, 73700 |
| Objects | Broken Warpstone 45494; Sacred Glade Bushel (hay pile) 42181; cooking table; picnic-yard gate; `sg_tutorial_gate_*` | `forestgiant_tutorial.adr` (542, "forest giant at 75% size") is probably Chug's illusion-spell look; `weapon_ar_ag_club_tutoriallog` is the brawler club | | |

The wiki lists Robbie as a former Sacred Glade NPC, but no Robbie lines appear in the tutorial blocks.

### Flow

The wiki and ZAM prefix these titles with "Tutorial:"; the string-table titles don't have it. Coins are from ZAM.

1. **Arrival** (encounter 10).
   - The camera spins (41895). Michael: 41908-41910.
   - The robgoblins break the warpstone: 41915-41921, 41926. Movement hints: 41934, 41935.
   - Tracking quest "Sacred Glade Tutorial" 75679 / 75680; server quest id 534 (41907).
2. **Wilbur's Trouble** (45851; 45852 / 45858). Giver: Michael.
   - Offer 45850. Objectives 45860-45864.
   - Michael 45867. Lead Wilbur to Chug: 47787 / 47788 (reminders 47990, 47992, 71610, 71613, 74146).
   - Chug 41995. Journal 47526.
   - Reward: **Adventurer job + 31 coins**.
3. **Pig Pen Rescue** (41973; 41974). Giver: Farmer Chug.
   - Offer 41972. Illusion potion 42001 / 42002.
   - Scare off 7 robgoblins: 41981 / 41982, 47994, 75421, 42007-42011, 42084.
   - Completion 42088. Return 41993 / 41994. Chug's idle lines 70317, 70364, 70379.
   - Reward: **31 coins**.
4. **Perilous Party** (42095; 42096 / 42116 / 42118). Farmer Chug -> Ashley.
   - Lines 42094, 47998, 42012, 70860. Ashley 42112, 42113.
   - Reward: **6 coins**.
5. **The branch.** Ashley's offer is 42142: "Get Ingredients!" (42143) or "Learn to Fight!" (42144).
   - **Chef**
     1. **Chef Express** (42125; 42126). Ashley -> Cookie. Lines 42124, 42146-42148, 47887, 48002, 72942; Cookie 42165.
        Reward: **Chef job + 2 coins**.
     2. **Stewing to Success** (42173; 42174, 42178). Cookie.
        - Offer 42172. Hay pile 42179-42180, 42199-42200. Bring ingredients 46328.
        - Cook Strong Arm Stew: 42182, 42186, 42189, 42191-42192, 46330. Return 42193 / 42194.
        - Reward: **31 coins**.
     3. **Party Payback** (42120; 42121). Cookie -> Ashley.
        - Bring the stew: 42136, 42137, 46005, 46007, 46100, 46102, 70007, 73222, 75800.
        - Ashley grows (46271, 47958 / 47959), says 42195, and clears out the robgoblins (46331 / 46332, 75804, 75858,
          75891). She finishes with 45889. Journal 47527.
        - Reward: **6 coins**.
   - **Brawler**
     1. **Brawling 101** (42130; 42131). Ashley -> Flanders. Lines 42129, 42150-42152, 47825, 47827, 70001, 72937;
        Flanders 42167. Reward: **Brawler job + 6 coins**.
     2. **The "Hands On" Method** (45307; 45308, 45315). Flanders.
        - Offer, with gear: 45306. Change job 45318 / 45319. Gate beside Ashley 45316, 45320, 73187.
        - Combat encounter 24, "Picnic Pandemonium" (42169, 45280): knock out 5 party crashers (45291, 45321). Ashley
          45444, 45578, and she heals you below 50%. Client scripts `Tutorial.CombatTutorial1-3` (45483-45492, 45566,
          45885, 46400).
        - Flanders 45322. Return 70074 / 70075.
        - Reward: **31 coins**.
6. **Wrap-up.** 45881, 75808. Radial-menu lesson 71840. Talk to Ashley to leave: 42213, 75820.
7. **Leaving Sacred Glade** (42210). Ashley 42205 teleports you to Samantha; the Highroad and Stillwater variants are
   116678 and 116682.
   - You can skip with "Sacred Glade - Exit Quest" (41890 / 41891) or "Skip Tutorial" (712).
   - A ZAM category page also lists "Touring the Glade".

**Beta fragments (Sacred Glade v1):**
- Michael and Ashley meet you: 1914, 1923, 1924.
- The cow roundup and Chug scenes: 278, 309, 311, 320, 339, 358, 871.
- Gate Keeper Furbin 1910; Tutorial Gates One-Three 2008-2018; Tutorial Footrace 2053; the leave prompts 2068, 2070.

### Voiced lines and text availability

- The **26 mp3s** are named `<Npc>Dialog<N>.mp3`, where N is a conversation-node number, not a string id. There are
  two sets, 33xx and 36xx.
- In numeric order the 33xx set follows the tutorial's order: Michael 3329 -> Chug 3331-3335 -> Ashley 3337 / 3339 ->
  Cookie 3342 / Flanders 3344 -> Cookie 3346 -> Ashley 3348 -> ... So each file is most likely one of the offer lines
  above. The 36xx set may be the other tutorial generation or a re-record; **listen to them to map mp3 -> sid**.
- **Text is complete** for all 9 launch quests (title, description, objectives, offer, reminders and completion).
  Rewards are on ZAM.
- **Positions:** none. `FabledRealms.lua` has no tutorial NPCs; the tutorial is a separate zone, and neither
  `Npcs.json` nor the 2014 capture contains any of its NPCs.
- **Zone assets on the CDN:**
  - `sg_tutorial_02` (54 terrain chunks), `sg_tutorial_02.gzne`, `sg_tutorial_02.map`
  - **`sg_tutorialAreas.xml`**: area volumes that should name the pig pen, picnic yard, practice yard and wagon
  - local sky: `client/Resources/SKY/sky_tutorial_02.xml`
- The **NPX 4.0** equivalents are:
  - `sg_npx_cavern_01` (161 files), `sg_npx_cavern_01Areas.xml`
  - `sg_newbiezone_showdown` (25 files)
  - `human_m_mac.agr`, `human_m_collin.agr`
  - `fairy_f_darkthorne_amb_tutorial_dialog_line_*` and `NpxPlayMore.gfx`
- The CDN refuses requests without the right `/NNN/` directory, so none of this is fetched yet.

## Provenance of the community layout

- **PR #42** (yungcomputerchair / Gent Semaj): opened 2026-07-07, squash-merged 2026-07-09 as `052d970`.
  - It "compiled the spreadsheet of community-contributed NPCs" into JSON, with no links.
  - The same 4,253 records went into his FabledRealmsProject/FabledRealms `data/npcs.json` on 2026-07-01 (`476ef99`);
    that README says they are "sourced from OSFR".
  - In the PR comments, GroaxyVRC mentions NPCs "we accidentally logged", and the author says these are the placements
    running on the public server.
  - Later PRs only reshaped the data: #69 (`a28e06a`, guid base) and #88 (`a2accb3`, positions moved into Lua).
  - The spreadsheet itself is not public (most likely on the OSFR Discord). `free-realms-re` holds only the capture
    pipeline.
- **Hand-placed, not captured:**
  - The guids are generated.
  - 380 consecutive-id pairs sit on identical coordinates, i.e. spawn-command doubles.
  - Some names don't fit their models; for example, Autumn Mistflower is spawned as `fish_bbe.adr`.
  - Accidental props: 338 `hsg_*`, about 30 mounts, pickups and a boombox.
  - Zero spawns lie within 0.05 u of a captured spawn, although the capture shows live NPCs never moved. Where both
    sets place the same NPC, positions differ by 0.3-1 u and headings by a few degrees: someone standing on the spot.
- **Era:** all placements were made with the final 2014 client. They include 87 `merchant_*` job-gear vendors (the
  dated ones, e.g. Roosey, Estus and Vela, are Aug-Oct 2013), One of Three (Oct 2013) and gloamed Bixies. Around
  Cobblestone the layout is a faithful 2013 town:
  - the NPX 4.0 Bartle, Sheila and hooligans
  - the cow pen
  - no Brawling Ring cast and no Majorie
  - Samantha, Ricky, Simone and Jonelle, who were still standing in 2013 per ZAM
- **Accuracy where it can be checked:**
  - Eddie Eagle Eyes is 0.95 u from the March 2014 capture.
  - The Wildwood Speedway NPCs are 0.3-2 u off.
  - The town centre has no capture coverage (only 4 captured spawns within 250 u).
- **Verdict:** a memory, screenshot and video reconstruction of the **final 2013-14 world**, done post-shutdown. It is
  careful (about 1 u) where someone had a reference. It is not a live capture of any era, and it omits the scripted
  NPX 4.0 set pieces.

## Manual-research leads

Every Wayback URL below had a 200 capture verified through the CDX API. Where only the original URL is given, prefix it
with `https://web.archive.org/web/<timestamp>/`; the timestamp is in the note beside it. archive.ph returned 429 for us,
so its links are untested. **Local copies:** the ZAM, freerealms.com and freerealmsinsider captures are saved under
`C:\Users\carlo\AppData\Local\Temp\claude\C--Users-carlo-Developer\458eb073-0f00-40ce-b394-30cba2360827\scratchpad\wb\`
(`zam\q_*.txt` for quests, `zam\m_*.txt` for NPCs, `zam\place_*.txt` for places). That folder is temporary, so copy
anything you want to keep.

### Final-build flow (NPX 4.0: Briarwood Caverns, Farnum's Farm, Bartle, Mac)
- **freerealms.com article 2875, "Are You Brave Enough to Be a Hero?"** (March 2013): the announcement.
  https://web.archive.org/web/20130318121729/http://www.freerealms.com:80/article/detail.action?articleId=2875
- **ZAM `FR_Place:Briarwood_Caverns`**: Darkthorne's step-by-step tutorial lines.
  https://web.archive.org/web/20210124053107/https://fr.zam.com/wiki/fr_place:Briarwood_Caverns
- **ZAM quests:**
  - Basic Combat Tutorial (ts 20150914232814); Advanced Combat Tutorial (20150915002337); Hero's Journal
    (20150915025337)
  - Find and Pull the Plug (20150914200053); Cobblestone Showdown (20150914233029); Off to the Queen (20150915024434)
  - all at `http://fr.zam.com/wiki/fr_quest:<Title_with_underscores>`
- **ZAM `FR_Place:Cobblestone_Showdown`** (prize wheel):
  https://web.archive.org/web/20210124055458/https://fr.zam.com/wiki/fr_place:Cobblestone_Showdown
- **freerealmsinsider:**
  - https://web.archive.org/web/20131105014129/http://www.freerealmsinsider.com:80/component/content/article/5712-free-realms-update-adds-hero-rises-content-and-more
  - https://web.archive.org/web/20130316062412/http://www.freerealmsinsider.com:80/forum/f69/exciting-update-march-15th-2013-a-59005/
  - https://web.archive.org/web/20130216072914/http://www.freerealmsinsider.com:80/component/content/article/5706-new-exciting-updates-coming
  - forum threads to look up through CDX: `forum/somebody-help-145/cobblestone-village-questline-59675/`,
    `forum/quests-150/how-get-out-closed-boundaries-cobblestone-village-62041/`,
    `forum/somebody-help-145/advanced-combat-tutorial-big-stuck-problem-65359/`,
    `forum/groups/freerealms-update-group/hero-rises-part-2-1624/`
- **Not captured anywhere:** ZAM "The Last Best Hope" and "Where's the Help". Try:
  - https://archive.ph/https://fr.zam.com/wiki/FR_Quest:The_Last_Best_Hope
  - https://archive.ph/https://fr.zam.com/wiki/FR_Quest:Where's_the_Help
- **YouTube:** "Free Realms Hero Rises Darkthorne Briarwood Caverns", "Free Realms 2013 new character", "Free Realms
  Bartle Mac Cobblestone Showdown", "Free Realms Farnum's Farm Cleetus", "Free Realms last day 2014". Use these for the
  Mac, Collin, Gloam Artifact, barrier and Showdown-entrance positions, and for Darkthorne's cavern lines.

### Cobblestone Village / Crossroads town
- **ZAM Cobblestone Village** (redirects from Crossroads; notes the March 2013 rename):
  https://web.archive.org/web/20150920045458/http://fr.zam.com/wiki/fr_place:Cobblestone_Village
- **ZAM Crossroads, 2009** (Adoption Center with Majorie, Jennifer and Walker; Animal Park):
  https://web.archive.org/web/20091101050421/http://fr.zam.com:80/wiki/fr_place:Crossroads
- **ZAM Crossroads, 2012:** https://web.archive.org/web/20120828211007/http://fr.zam.com:80/wiki/FR_Place:Crossroads
- **ZAM Crossroads exploration collection:**
  https://web.archive.org/web/20200929152049/https://fr.zam.com/wiki/fr_collection%3ACrossroads
- **ZAM NPC pages** (`/wiki/FR_Mob:<Name>`, captured 2009-2021):
  - Samantha: https://web.archive.org/web/20210417073723/https://fr.zam.com/wiki/FR_Mob%3ASamantha
  - Ricky Danger: https://web.archive.org/web/20091003171948/http://fr.zam.com:80/wiki/fr_mob:Ricky_Danger
  - Also captured: Carlos Brazenfist, Majorie, Simone, Jonelle, Billie JoBob, Jennifer, Walker, Jones, Bartle, Eddie
    Eagle Eyes, Kibler, Mongo, Gloamling, Papa Salt, Anne, Shakey, Hooligan Bully, T.R. Walker Jr., Alena Goldenheart
  - ZAM has **no pages** for Roosey, Gloria or Sheila.
- **Positions of Carlos, Majorie and Spot, Kibler and the ring gate (2009-11):** YouTube "Free Realms Crossroads
  Samantha", "Free Realms King of the Ring Mongo", "Free Realms Ricky Danger kart", "Free Realms Cobblestone Village".
- **freerealms.com article 624, "Explore Farnum Farmstead"** (Nov 2009):
  https://web.archive.org/web/20091123062449/http://www.freerealms.com:80/article/detail.action?articleId=624
- **ZAM Farnum's Farm:** https://web.archive.org/web/20210118113111/https://fr.zam.com/wiki/fr_place%3AFarnum%27s_Farm.
  **ZAM Wildwood Speedway:** https://web.archive.org/web/20210513194022/https://fr.zam.com/wiki/FR_Place%3AWildwood_Speedway

### Quests: Introduce Yourself, Call the Crew!, the Brawler / Chef / Pet chains
- ZAM Introduce Yourself: https://web.archive.org/web/20220425200228/https://fr.zam.com/wiki/FR_Quest:Introduce_Yourself
- ZAM Call the Crew!: https://web.archive.org/web/20210117164409/https://fr.zam.com/wiki/FR_Quest%3ACall_the_Crew%21
- The same scheme covers Back for More, Brawler: Bringing Back Brazenfist, Brawler: Honor Lost, Brawler: Building your
  Rep, Chef: Crossroads Cafe, Chef: Delivery Derailed, Pet Tutorial and Pet Trainer: Speak Up. The saved `zam\q_*.txt`
  files carry the timestamps.
- Series indexes: `fr.zam.com/wiki/Crossroads_(FR_Quest_Series)`, `Tutorial_(FR_Quest_Series)`,
  `Farnum's_Farm_%28FR_Quest_Series%29`.
- The Fandom wiki's `Quest:Introduce Yourself` is already in `wiki/quests/`.
- **Still without a text source for rewards:** Delinquent Dilemma, Bad Business, Package Recovery!, Chef: Queensfields,
  Chef: Assistant Needed. Search YouTube for "Free Realms Introduce Yourself quest" and "Free Realms Jones hooligans
  brawler".

### Sacred Glade tutorial
- **ZAM `FR_Place:Sacred_Glade`** (a full walkthrough plus mini-map legend):
  - https://web.archive.org/web/20090601215856/http://fr.zam.com:80/wiki/FR_Place:Sacred_Glade
  - https://web.archive.org/web/20220415195058/https://fr.zam.com/wiki/FR_Place:Sacred_Glade
- **ZAM Sacred Glade quest category:**
  https://web.archive.org/web/20090605175406/http://fr.zam.com:80/wiki/Category:Sacred_Glade_Quests_%28FR%29
- **ZAM Wilbur's Trouble:**
  https://web.archive.org/web/20220409024632/https://fr.zam.com/wiki/fr_quest:Tutorial:_Wilbur%27s_Trouble. The other
  seven quests follow the same pattern (`/web/2022*/https://fr.zam.com/wiki/FR_Quest:Tutorial:_<Name>`).
- **ZAM NPC pages:** Michael Tallstrider, Farmer Chug, Wilbur, Ashley Lightwings, Flanders, Cookie, Robbie.
- **Massively, "Free Realms: A beginner's guide"** (April 2009):
  https://web.archive.org/web/20101127014419/http://massively.joystiq.com:80/2009/04/24/free-realms-a-beginners-guide
- **YouTube** (for NPC positions, pig-pen and picnic-yard layout, voice lines): "Free Realms tutorial Sacred Glade",
  "Free Realms Michael Tallstrider Wilbur", "Free Realms Wilbur's Trouble", "Free Realms Ashley Lightwings robgoblins
  party", "Free Realms 2009 beta tutorial", "Free Realms first 30 minutes 2009".

### Highroad Vale (2011-13 flow, for completeness)
- **ZAM `FR_Place:Highroad_Vale`:** https://web.archive.org/web/20150920110628/http://fr.zam.com/wiki/fr_place:Highroad_Vale
- **freerealmsinsider patch notes, 2011-10-28** (new players go to Merigold):
  https://web.archive.org/web/20190922120258/http://www.freerealmsinsider.com/forum/free-realms-maintenance-160/game-update-summary-friday-october-28-2011-0-a-44942/
- **YouTube:** "Free Realms Highroad Vale Pet Adventures".

### General
- **freerealms.com news index:**
  https://web.archive.org/web/20130410185828/http://www.freerealms.com/article/list.action (April 2013) and
  https://web.archive.org/web/20110401143417/http://www.freerealms.com/article/list.action (April 2011)
- **Update-notes archive:**
  https://web.archive.org/web/20110701025135/http://www.freerealms.com/article/listArchive.action?type=notices&page=2
- **Official blog:**
  - https://web.archive.org/web/20091107075346/http://freerealms.wordpress.com:80/2009/11/02/november-december-updates/
  - https://web.archive.org/web/20100222043010/http://freerealms.wordpress.com:80/2009/12/11/december-update-preview-pt-2/
- **Official forums:** these were barely archived. The one relevant thread is Snow Days update notes 2009-12-14:
  https://web.archive.org/web/20130510145532/http://forums.station.sony.com/freerealms/index.php?threads/snow-days-update-notes-for-december-14th-2009.15922/
- **archive.ph URLs to try:**
  - https://archive.ph/freerealms.com
  - https://archive.ph/fr.zam.com
  - https://archive.ph/fr.fanbyte.com
  - https://archive.ph/freerealms.wikia.com
  - https://archive.ph/freerealmsinsider.com
  - https://archive.ph/forums.station.sony.com
  - https://archive.ph/https://fr.zam.com/wiki/FR_Place:Cobblestone_Village
- **Not verified:** tentonhammer.com and mmorpg.com (their CDX queries timed out; retry with year prefixes).
- **No captures at all:** freerealmsguide, frcompanion, freerealmsinfo, freerealmsunleashed, freerealmsfan.

## Gaps and recommendations

1. **Pick the era, and fix the brief.**
   - "Final build" means **NPX 4.0**: Briarwood Caverns -> Farnum's Farm -> Bartle / Cobblestone Showdown -> Queen.
     The Sacred Glade -> Crossroads flow is **April 2009**.
   - For a "remaster of the final build", rebuild NPX 4.0. The community layout already fits it (Bartle, the hooligans,
     the wolves, the cow pen), and its titles, objectives and most of its dialogue are in the string table.
   - Sacred Glade is the better-documented alternative: complete text, voice and ZAM walkthroughs. It could be offered
     as an optional "classic tutorial".
   - Fix the "Sacred Glade -> Cobblestone" wording in `docs/`.
2. **The spawn point.** In the final build, new characters appeared at **Farnum's Farm** after the cavern (Darkthorne
   teleports them there), not at the Cobblestone warpstone. The server's (-1904.9, -39.7, 412.6) matches neither era;
   in 2009 you arrived at Samantha. Move it once an era is chosen.
3. **NPX 4.0 set pieces to build and place:**
   - Mac (the Showdown instance and its cave entrance toward Blackspore)
   - Collin's camp
   - 3 Gloam Artifacts (north of town and at Collin's camp)
   - the Gloam barrier on the Sanctuary road
   - the Farnum fires and water well, and the Gloam Creature north of Wildwood
   - Sheila's conditional spawn
   - Positions: from video and from `sg_npx_cavern_01Areas.xml`.
4. **2009 set pieces, if you target classic:** Carlos, Majorie with Spot, Kibler, the ring gate (in place of the cow
   pen), the potion vendor, and the whole Sacred Glade zone. The quest text fixes their relative positions: Carlos
   stands east of Samantha by Ricky's lamppost; Majorie stands across the street from the adoption center (Gloria,
   Jennifer and Walker are at about -1848, -41, 474).
5. **Fetch the zone files:**
   - `sg_npx_cavern_01*`, `sg_newbiezone_showdown*`, `sg_tutorial_02*`, `sg_tutorialAreas.xml`,
     `sg_crossroads_pet_tutorial.gzne` and the 26 mp3s.
   - `opensourcefreerealms.com/assets/<NNN>/...` returns 404 without the correct directory (verified).
   - Either let the client request them once through `tools/asset-server` (it caches the right `/NNN/`), or solve the
     NNN function. Probing all 1000 directories per file is possible but impolite to a hobby CDN; that's your call.
6. **Quest data.** Build `Quests.json` entries from the sid blocks above (NPX 4.0 chain F first). Take coin rewards from
   ZAM. Replace the placeholder rewards in `Quests.json` 2563 and 1801 (ZAM: 5 and 7 coins).
7. **Placeholders in the string table.** Darkthorne's cavern lines (4419545-4419557) are unwritten
   "tell Erik" placeholders. Use ZAM's Briarwood Caverns transcript and video for the live wording.
8. **Prune** the strays (`knocker_m_boss` 17596, coffee table 31570, dragon pair 2073/2074) and make One of Three
   seasonal.
9. **Correct the docs:**
   - `npc-spawns/NPC_CAPTURES.md` and `research/README.md` date the captures to 2010; they are March 2014.
   - `research/README.md` says the starter flow was "Sacred Glade (later removed) -> Highroad Junction -> later
     Cobblestone". The real sequence is: Sacred Glade -> three towns (2009); Highroad Vale (2011); Briarwood Caverns ->
     Farnum -> Cobblestone (2013).
10. **Ask the OSFR Discord** for the placement spreadsheet and the references behind the Cobblestone placements.
