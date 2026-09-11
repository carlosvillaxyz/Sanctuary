# Pilot town: which one to rebuild first

Compiled 2026-09-10 from the recovered 2010 packet-capture spawns (`npc-spawns/`), the client string table
(`strings/en_us.json`), and the Fandom wiki dump (`wiki/`). Method and data caveats are at the bottom.

## Recommendation: Snowhill

Snowhill is the only candidate where all three legs exist at once: **positions** (the 2010 captures were
mostly taken in and around Snowhill), **wiki coverage** (77 pages in `Category:Snowhill`, the largest of the
eight after Seaside), and **dialogue** (the only town with more than one quest transcript, and the client
string table carries the rest of its quest text verbatim). It is also a genuine hub: four job unlocks
(Warrior, Blacksmith, Miner, Card Duelist), the Brawler/Medic/Ninja/Chef/Postman/Wizard trainer chains all
route through it, and Highroad Junction (the original starter town) is "south of Snowhill", so a new player's
second stop was Snowhill.

Runner-up is **Lakeshore** (121 spawns, 26 named NPCs with positions, 9 of them with wiki pages) as the
first scale-out target after Snowhill: it is small, has a job trainer (Ty, Ninja) and all six 2013 job-gear
merchants placed, and no dialogue to transcribe beyond what the string table gives.

Every other candidate has **zero** recovered positions. Cobblestone Village and Highroad Junction are the
best new-player fit narratively but would have to be laid out by hand from screenshots; Seaside has by far
the richest wiki (175 pages) but no positions and is the largest town in the game.

## Comparison

Positional counts are spawns inside the town's own area volumes from `FabledRealmsAreas.xml`, after
removing house-instance furniture that the capture labeller mis-attributed (see caveats). "Named NPCs" are
distinct (name, model) pairs that are not generic ambience ("Human", "Dog", mounts, event props).

| Town | Spawns w/ position | Named NPCs placed | Wiki `Category:` pages | Quest transcripts w/ dialogue | Size (area volumes) | New-player centrality |
|---|---:|---:|---:|---:|---:|---|
| **Snowhill** | 254 NPC + 38 creature + 22 harvest + 70 prop | **82** (31 with wiki page, 8 quest givers) | **77** | **3 town quests** (Noisy Neighbors, Noise Permit, Misplaced Meeps) + 2 event quests (Winter Party, Glide Training 3) | 76 | High: 4 job unlocks, warpstone, 11 dungeons, Snow Days event; 2nd town after Highroad |
| Lakeshore | 50 NPC + 46 harvest + 25 prop | 26 (9 with wiki page, 1 quest giver) | 22 | 0 (2 Glide Training quests only send you there) | 51 | Medium: Ninja unlock (Ty), Wizard camp (Fizzlesticks), Thunder Falls Speedway |
| Stillwater Crossing | 2 (an antelope and a troll, edge of volume) | 0 | 9 | 0 (Introduce Yourself mentions it) | 55 | Medium: Brawler + Chef unlock, Pet Trainer |
| Wugachug | 0 | 0 | 26 | 1 (A Sweetwater Climb) | 5 | Medium: Chef chain, Sweetwater Climb dungeon |
| Seaside | 0 | 0 | **175** | 5 (Glide Training x4 start there, Horrible Hiccups, Scaredy Pants, Half a Dozen) | 78 | High: Medic unlock, pet adoption, busiest social hub |
| Cobblestone Village | 0 (no volume named Cobblestone; 17 "Crossroads" volumes at (-1834, 446)) | 0 | 11 | 0 (Introduce Yourself mentions it) | 17 | Highest: post-tutorial spawn point 2011-2014; Brawler/Chef/Pet trainers |
| Highroad Junction | 0 | 0 | 12 | 0 | 51 | Highest (2009-2011): original starter town; Brawler unlock (Harold) |
| Merry Vale | 0 | 0 | 30 | 1 (Half a Dozen) | 56 | Medium: Festival of Hearts, Chef chain (Jarvie) |

Where the positions sit (world units; compass North = +X, East = +Z per free-realms-re):

- Snowhill town proper: x 70..330, z 280..470 (entrance at x ~30..100, z ~300..380; Town Hall/clock at ~(300, 410); stage at ~(270, 460); Big J's at ~(150, 390)).
- Snowhill outskirts with positions: Diamondback Raceway (x ~0, y 102, z 470), Frost Ridge kart starting grid (x 785..805, y 86, z 365..415), soccer field (-200, 290), Perry's Pastures / Ree Peatpants (-54, 332), Frostfang wolves (100..150, 170..210), Hot Springs (250..300, 210..300).
- Lakeshore: x -820..-590, z 800..1050 (merchant row at ~(-760, 840..860), farm at ~(-810, 935)).

## Snowhill roster

Position = first recorded spawn (x, y, z) and heading in degrees; the CSV (`npc-spawns/snowhill_roster.csv`)
has every duplicate and the area-volume name. Sorted: wiki-documented quest givers, then other wiki NPCs, then
NPCs known only from the captures.

| NPC | Role (wiki / job pages) | Position (x, y, z) / heading | Wiki page | Dialogue on record | NameId / model |
|---|---|---|---|---|---|
| Candi Ivy | Snow Days event quest giver (10 quests, by the Gifting Tree) | (230.2, 24.4, 401.0) / -128.97 | [yes](https://freerealms.fandom.com/wiki/Candi%20Ivy) | - | 419832 / `human_f_santa.adr` |
| Frostpetal | Quest giver: Meet the Mayor!, Ninja: Return to the Mentor (right of town entrance) | (72.9, 20.6, 331.2) / -131.26 | [yes](https://freerealms.fandom.com/wiki/Frostpetal) | - | 5448 / `fairy_f_valinda.adr` |
| Lucca De'Flor | Chef trainer (5 quests; cooking table at 2 Icecrest Ct) | (200.2, 23.0, 352.1) / -15.27 | [yes](https://freerealms.fandom.com/wiki/Lucca%20De%27Flor) | - | 23788 / `fairy_m_farmer_01.adr` |
| Mayor Crystalline | Mayor; Brawler / Medic / Card Duelist quest hub (front of Town Hall) | (299.4, 27.4, 409.0) / -67.47 | [yes](https://freerealms.fandom.com/wiki/Mayor%20Crystalline) | - | 182 / `fairy_f_lorina.adr` |
| Momma Meepster | 8-Bit Month event: Misplaced Meeps (near Big J's) | (127.7, 22.6, 423.2) / -136.08 | [yes](https://freerealms.fandom.com/wiki/Momma%20Meepster) | Misplaced Meeps transcript | 5102405 / `penguin_baby_8bit_01.adr` |
| Ree Peatpants | Ninja contract giver (4 repeatable contracts; south of town) | (-54.1, 5.2, 332.0) / 76.24 | [yes](https://freerealms.fandom.com/wiki/Ree%20Peatpants) | - | 388454 / `fairy_m_sakka.agr` |
| Tevin | Brawler trainer: Way of the Brawler | (303.5, 27.2, 410.3) / -61.35 | [yes](https://freerealms.fandom.com/wiki/Tevin) | - | 79022 / `fairy_m_freestyle_08.agr` |
| Tiger the Party Animal | Event party host / merchant (on the stage) | (309.2, 27.3, 465.0) / 13.59 | [yes](https://freerealms.fandom.com/wiki/Tiger%20the%20Party%20Animal) | A Luau / Party for Pets / Winter Party transcripts | 420171 / `human_m_tiger_the_party_animal_holiday.agr` |
| Arci Joan | NPC beside Easy Penguin Defense minigame | (293.9, 25.4, 355.4) / -1.24 | [yes](https://freerealms.fandom.com/wiki/Arci%20Joan) | - | 391188 / `fairy_f_03.adr` |
| Assistant Chef Edward | Merchant (Chef supplies) | (193.7, 22.7, 364.0) / 29.11 | [yes](https://freerealms.fandom.com/wiki/Assistant%20Chief%20Edward) | - | 90184 / `fairy_m_02.adr` |
| Bert | Penguin postman | (210.7, 28.7, 434.6) / -133.34 | [yes](https://freerealms.fandom.com/wiki/Bert) | - | 177 / `penguin_bert.adr` |
| Big J | Owner of Big J's cafe | (146.6, 23.5, 386.1) / 177.25 | [yes](https://freerealms.fandom.com/wiki/Big%20J) | - | 6375 / `fairy_m_02.adr` |
| Captain Ironsides | Pirate NPC (TCG character) | (124.6, 23.8, 352.9) / 146.47 | [yes](https://freerealms.fandom.com/wiki/Captain%20Ironsides) | - | 427724 / `human_m_pirate_01.agr` |
| Chip Numbwing | Merchant (Fisherman) | (130.8, 25.1, 392.6) / 176.29 | [yes](https://freerealms.fandom.com/wiki/Chip%20Numbwing) | - | 408638 / `fairy_m_chip_numbwing.agr` |
| Clara Chatterhag | Starts Noisy Neighbors | (274.1, 26.4, 458.6) / -130.82 | [yes](https://freerealms.fandom.com/wiki/Clara%20Chatterhag) | Noisy Neighbors transcript | 36808 / `fairy_f_02.adr` |
| Cragara | Mysterious Merchant (roaming) | (130.9, 28.7, 290.9) / -5.56 | [yes](https://freerealms.fandom.com/wiki/Cragara) | wiki quotes (shop open / closed) | 440941 / `sogg_f_vendor.adr` |
| Dempsy | Merchant (Brawler gear, by Post Office) | (237.1, 26.2, 436.1) / 79.57 | [yes](https://freerealms.fandom.com/wiki/Dempsy) | - | 5102200 / `human_m_merchant_brawler_african.agr` |
| Ernie | Penguin postman | (226.0, 26.8, 431.3) / 44.85 | [yes](https://freerealms.fandom.com/wiki/Ernie) | - | 179 / `penguin_ernie.adr` |
| Fladnag | Merchant (Wizard gear, by Post Office) | (230.9, 25.2, 430.0) / -176.17 | [yes](https://freerealms.fandom.com/wiki/Fladnag) | - | 5102517 / `fairy_m_merchant_wizard_caucasian_volcano.agr` |
| Fritti Bluebelle |  | (286.8, 27.5, 406.3) / -60.69 | [yes](https://freerealms.fandom.com/wiki/Fritti%20Bluebelle) | wiki quote | 90186 / `fairy_f_freestyle_11.agr` |
| Jonathon Forkpath | Quest giver (Defenders Medal; wiki spells "Johnathon") | (224.4, 23.6, 405.2) / 7.77 | [yes](https://freerealms.fandom.com/wiki/Johnathon%20Forkpath) | - | 92578 / `fairy_m_old_01.adr` |
| Kiel | Idle NPC | (139.7, 23.6, 393.9) / -98.49 | [yes](https://freerealms.fandom.com/wiki/Kiel) | - | 6156 / `fairychild_m_pointyhair.adr` |
| Mr. Twinkle | Idle NPC outside Big J's | (151.6, 23.4, 386.3) / 177.85 | [yes](https://freerealms.fandom.com/wiki/Mr.%20Twinkle) | - | 3070 / `fairy_m_01.adr` |
| Reba | Merchant (Postman gear, by Post Office) | (215.3, 28.2, 441.7) / -157.08 | [yes](https://freerealms.fandom.com/wiki/Reba) | - | 5102420 / `human_f_merchant_postman_caucasian.agr` |
| Roland Sporeling | Diamondback Raceway host (above Frostfang Caverns) | (-0.6, 102.3, 472.9) / -15.28 | [yes](https://freerealms.fandom.com/wiki/Roland%20Sporeling) | wiki quote | 432170 / `human_m_roland_spore.agr` |
| Senari | Idle NPC | (302.2, 26.5, 396.4) / -111.42 | [yes](https://freerealms.fandom.com/wiki/Senari) | - | 4726 / `human_f_explorer_caucasian01.agr` |
| Skye | Merchant (Chef gear, by cooking table) | (203.8, 23.1, 352.8) / -13.64 | [yes](https://freerealms.fandom.com/wiki/Skye) | - | 5102505 / `fairy_f_merchant_chef_asian_phoenix.agr` |
| Sonja | Merchant (Warrior gear) | (233.4, 23.6, 387.9) / 62.0 | [yes](https://freerealms.fandom.com/wiki/Sonja) | - | 5102437 / `human_f_merchant_warrior_caucasian.agr` |
| Spratt | Merchant (Medic gear) | (228.2, 23.5, 385.2) / -71.2 | [yes](https://freerealms.fandom.com/wiki/Spratt) | - | 5102292 / `fairy_f_merchant_medic_asian_cupid.agr` |
| Steele | Warrior trainer (Town Hall, blocks until lvl 5) | (319.5, 26.1, 430.2) / -51.04 | [yes](https://freerealms.fandom.com/wiki/Steele) | - | 47830 / `fairy_m_warrior_01.agr` |
| Tommy | Merchant (Archer gear, Aug 2013) | (235.0, 23.7, 384.3) / 32.21 | [yes](https://freerealms.fandom.com/wiki/Tommy) | - | 5101985 / `human_m_merchant_archer_asian.agr` |
| Annabelle | Idle NPC | (245.1, 24.8, 470.1) / 153.34 | no | - | 21849 / `human_f_mary.agr` |
| Autumn Mistflower | NPC (also Npcs.json id 1198) | (304.8, 27.3, 414.8) / -84.07 | no | - | 394914 / `fairy_f_autumn_mistflower.agr` |
| Brian |  | (272.8, 29.0, 435.3) / -41.22 | no | - | 3427 / `humanchild_m_tshirtwithbaggypants.adr` |
| Brittany |  | (295.2, 24.5, 279.0) / -128.52 | no | - | 3062 / `fairy_f_02.adr` |
| Calvin Coldcastle | Idle NPC | (120.4, 22.1, 378.2) / 128.96 | no | - | 420397 / `human_m_snowhill.adr` |
| Carly Collision |  | (263.3, 11.0, 309.6) / 128.92 | no | - | 19371 / `human_f_amber_violet_incar.agr` |
| Cinn | Farmer NPC | (283.1, 27.7, 426.8) / -109.77 | no | - | 4554 / `fairy_m_farmer_01.adr` |
| Coin Farmer Lubag | Farming NPC | (101.2, 22.8, 330.0) / -16.42 | no | - | 429152 / `chugawug_farmer_m.adr` |
| Connor Rush |  | (805.0, 86.2, 415.0) / 0.0 | no | - | 19364 / `human_m_verve_mcnichols_incar.agr` |
| Crafty Robgoblin | Snow Days event | (29.2, 22.6, 354.8) / -118.4 | no | - | 420583 / `robgoblin_m_reindeerantlers.adr` |
| Downey | Idle NPC | (228.3, 24.0, 371.5) / -1.54 | no | - | 90117 / `fairy_f_freestyle_11.agr` |
| Drill Sergeant Dewey | Warrior job unlock (front of Town Hall) | (317.7, 26.5, 424.6) / -64.66 | no | - | 22435 / `fairy_royalguard_m.adr` |
| Edward |  | (263.8, 23.9, 281.1) / -10.49 | no | - | 39812 / `fairy_f_01.adr` |
| Everett | Miner NPC | (322.8, 25.6, 378.5) / -106.58 | no | - | 420809 / `human_m_miner_01.adr` |
| Freddy MacIsaac | Idle NPC | (261.2, 26.9, 433.3) / -118.83 | no | - | 385662 / `human_m_snowhill.adr` |
| Garrison Gold | Card Duelist job unlock | (192.6, 23.3, 353.1) / -0.32 | no | - | 38092 / `fairy_m_tcg_rick_garfield.agr` |
| Gerold | Brawler quest giver: Growler Encroachment / The Growler Report (south of town) | (-207.1, -19.1, 290.6) / -80.17 | no | - | 104054 / `human_m_gerold.agr` |
| Grog | Chugawug NPC | (293.0, 28.2, 296.2) / -112.17 | no | - | 3224 / `chugawug_m_01.adr` |
| Holly Singsong | Idle NPC | (259.4, 27.0, 437.5) / -126.99 | no | - | 46947 / `fairy_f_01.adr` |
| Jet Madedge |  | (360.1, 6.0, 291.1) / -91.67 | no | - | 19368 / `human_m_racer_johnny_thunder_incar.agr` |
| Jet Swiftpass |  | (800.0, 86.2, 390.0) / 0.0 | no | - | 19363 / `human_m_racer_johnny_thunder_incar.agr` |
| Joel |  | (270.7, 28.0, 437.3) / 137.25 | no | - | 3429 / `humanchild_m_tshirtwithbeltbig.adr` |
| Julie |  | (293.3, 24.9, 277.5) / 58.93 | no | - | 3310 / `fairy_f_03.adr` |
| Layla Octane |  | (790.0, 86.2, 390.0) / 0.0 | no | - | 19361 / `fairy_f_mustang_alli_incar.agr` |
| Lealin |  | (258.3, 22.1, 261.6) / 140.97 | no | - | 5454 / `fairy_m_01.adr` |
| Lily Leadfoot |  | (805.0, 86.2, 365.0) / 0.0 | no | - | 19358 / `fairy_m_sparks_blitzwing_incar.agr` |
| Linnie | Idle NPC | (284.3, 27.6, 399.6) / -90.38 | no | - | 139451 / `fairy_f_freestyle_06.agr` |
| Little J |  | (117.5, 22.4, 431.4) / 168.41 | no | - | 5449 / `fairychild_m_pointyhair.adr` |
| Loryn | Postman trainer (4 quests) | (198.0, 29.0, 429.0) / 89.44 | no | - | 178 / `fairy_m_01.adr` |
| Mai Redline |  | (795.0, 86.2, 415.0) / 0.0 | no | - | 19362 / `fairy_f_mustang_alli_incar.agr` |
| Marcie |  | (170.9, 23.2, 406.3) / 0.13 | no | - | 36807 / `humanchild_f_skirtandtunic.adr` |
| Mikko Ragerunner |  | (251.8, 11.0, 294.1) / 128.92 | no | - | 19367 / `human_f_amber_violet_incar.agr` |
| Milus Featherfeet | Elder pixie NPC | (192.9, 28.1, 417.4) / 64.09 | no | - | 72171 / `fairy_m_old_02.adr` |
| Morninglory | Warrior trainer | (261.8, 49.0, 211.6) / 20.05 | no | - | 16826 / `fairy_f_morninglory.agr` |
| Murphy |  | (263.2, 24.2, 243.4) / -22.42 | no | - | 3063 / `fairy_m_02.adr` |
| Nikki | Idle NPC | (57.7, 30.6, 363.6) / 27.21 | no | - | 429539 / `human_f_snowhill.adr` |
| Nina the Wrecker |  | (265.9, 11.0, 172.7) / 40.11 | no | - | 19370 / `human_f_amber_violet_incar.agr` |
| Robgoblin Builder | Snow Days event | (25.7, 22.7, 353.3) / 90.41 | no | - | 420570 / `robgoblin_m_xmas.adr` |
| Scarlet Shadeveil | Idle NPC | (316.7, 26.4, 416.4) / 6.89 | no | - | 36923 / `fairy_f_01.adr` |
| Screamin' Steven |  | (785.0, 86.2, 415.0) / 0.0 | no | - | 19344 / `human_m_racer_johnny_thunder_incar.agr` |
| Sol |  | (100.0, 61.8, 541.4) / 174.64 | no | - | 7054 / `humanchild_m_tshirtwithbeltbig.adr` |
| Spindle | Dwarf NPC | (286.8, 26.3, 384.7) / -11.17 | no | - | 420500 / `dwarf_m_edwingoldstory.adr` |
| Tad Slopeslider | Idle NPC | (230.1, 21.1, 322.4) / 48.18 | no | - | 117257 / `human_m_freestyle_06.agr` |
| Tanda T. Toes | Idle NPC | (225.8, 26.6, 446.7) / -0.02 | no | - | 91660 / `human_f_freestyle_06.agr` |
| Tucker Thunderjolt |  | (360.1, 15.0, 215.0) / -51.57 | no | - | 19365 / `human_m_samuel_bunker_incar.agr` |
| Vaelen Warpwatcher | Warpstone druid (warp NPC) | (56.2, 31.5, 374.9) / 152.16 | no | - | 130731 / `fairy_druid_m.adr` |
| Willie | Idle NPC | (290.1, 27.7, 295.0) / 52.42 | no | - | 36827 / `human_m_snowhill.adr` |
| Willow Slamdash |  | (249.9, 11.0, 186.3) / 40.11 | no | - | 19366 / `fairy_f_willow_slamdash_incar.agr` |
| Wintara |  | (98.9, 61.8, 541.3) / -175.52 | no | - | 7053 / `fairychild_f_sleevesandshorts.adr` |
| Yoink | Robgoblin banker | (197.6, 28.4, 420.2) / 105.1 | no | - | 441346 / `robgoblin_m_banker.adr` |
| Zoomer Rex |  | (785.0, 86.2, 365.0) / 0.0 | no | - | 19359 / `human_m_antonio_taurino_incar.agr` |

### Wiki says they are in Snowhill, but no position recovered

| NPC | Role | Why missing | Wiki page |
|---|---|---|---|
| Sorin, Buren, Cronyn, Therin, Trixi, Brody Sparfist | Miner/Blacksmith trainers inside the Singing Crystal Mines (Therin = Miner unlock) | Mine interior is a separate instance; no capture went in | Sorin, Buren, Cronyn yes; Therin/Trixi/Brody no |
| Smitty | Blacksmith job unlock | Not in captures (may be inside/at the Smithy) | no |
| Flynn, Snowi | Stage caretaker (Noisy Neighbors / Noise Permit), stage pixie | Not in captures despite the stage being captured; possibly removed after 2009-2010 | yes |
| Silversnow | Elder at the Crystal Barrier | Barrier area not captured | yes |
| Hank Fisticuffs, Foreman Hetfield, Vaal, Wayland | Merchants (Hetfield under the playground; Vaal/Wayland at the mine crafting station, added Aug/Oct 2013) | Post-2010 additions or outside captured paths | yes |
| Valinda, Carrie, Morgulg, Yaren Sunstare, Michi, Jeni Shortfuse, Sampson, Fastvi Frostflutter | Wizard/Ninja/Brawler/Warrior chain NPCs | Not in captures; Morgulg moves with quest state | Valinda, Carrie, Morgulg, Yaren yes; others no |
| Umari, Abominable Snowman, Baron von Darkcheat, Madam Zelda, Bruce, Pear Jam | Dungeon boss / seasonal boss / TCG storyline / 2009 stage act / actually in the Wilds Roadhouse | Instance, event-only, or not actually in town | yes |

Note that some of these do have a model in `src/Resources/Npcs.json` (Therin = id 1201/1202, model 46
`dwarf_m_therin.adr`) and all have a string id in `strings/en_us.json` (listed in the roster builder output),
so they can be placed by hand once a screenshot or video fixes the spot.

## Dialogue: what exists and where

1. **Verbatim quest transcripts (wiki)** — `wiki/quests/`: Noisy Neighbors (Clara Chatterhag -> Flynn),
   Noise Permit (Flynn), Misplaced Meeps (Momma Meepster), Winter Party (Tiger the Party Animal),
   Glide Training: Confidence Is Key (Troy in Seaside sends you to the Snowhill launch pads).
   The wiki's `Quest:` namespace has exactly 59 pages total and all are now local; every other quest
   name on the NPC pages (Meet the Mayor!, Key to the City, Status Reports, Snowhill Secret Society,
   the Brawler/Ninja/Chef/Postman/Wizard chains, the ten Snow Days quests) has no wiki page.
2. **Wiki quote boxes** on NPC pages: Flynn, Fritti Bluebelle, Foreman Hetfield, Roland Sporeling, Cragara,
   Bruce (plus his Thugawug Sneak! battle lines), Umari, Abominable Snowman.
3. **The client string table has the rest.** `strings/en_us.json` (85,932 strings, ids recovered by
   brute-forcing `Global.Text.{id}` through Jenkins lookup2) contains quest names, journal summaries,
   objective labels and the NPC lines themselves, in contiguous id ranges per quest. Examples:
   36809-36837 = Noisy Neighbors + Noise Permit (matches the wiki word for word);
   5102405-5102410 = Misplaced Meeps; 7063 = Frostpetal's greeter speech ("Hi there, I'm Frostpetal, the
   town greeter for Snowhill Village..."); 7201/7576 = the "Did Frostpetal send you?" replies; 93195 = Mayor
   Crystalline's Key to the City reward speech; 36723 = the Bruce dressing-room quest summary; 72796 =
   Flynn's "Have you found the permit yet?" idle line; 24344 = a delivery objective naming Ryn at the hot
   springs, the Mayor at the Clocktower and Loryn at the Post Office.
   What is lost is the binding (which string id is which quest step, and who says it). Adjacency plus the
   NPC name in the text restores most of it. `strings/snowhill_strings.tsv` is a first cut: every string
   mentioning Snowhill or a Snowhill NPC name, sorted by id so the ranges are visible.

## Gaps

- Positions: the 28 NPCs in the table above (most important: Smitty, Therin, Flynn, Silversnow, Jeni
  Shortfuse, Yaren Sunstare). Mine interior and Town Hall interior are uncaptured.
- Captures are from late 2010 with Snow Days decorations up (Candi Ivy in a Santa model, holiday mannequins,
  Robgoblin Builder); the eight 2013 job-gear merchants (Tommy, Dempsy, Spratt, Reba, Sonja, Skye, Fladnag,
  Momma Meepster) are ALSO present, so at least one capture is from 2013-2014. Decide which year Snowhill
  should represent; the roster CSV keeps the source capture names per spawn.
- Quest->NPC->string binding for everything except the three transcribed quests.
- Quest rewards, star/coin values, level gates (only the wiki's "blocks partway until lvl N" hints).
- NPC idle/greeting lines: the string table has them (e.g. 72796) but nothing says which NPC owns which.
- Shop inventories: the 2013 merchants' stock is listed on their wiki pages; the older "Shops" list on the
  Snowhill page (Blacksmith clothing, Brawler & Ninja weapons, Fancy Pet Sweaters...) has no NPC names.
- Interiors (Town Hall, Post Office, Big J's, Smithy) — nothing beyond POI names.
- Mob packs (Frostfang Growler/Prowler/Howler/Alpha at (95..155, 170..210)) have positions but no stats.

## Sources for Carlos to dig up manually

Generic, for the whole town:

- YouTube: `"Free Realms" Snowhill 2009`, `"Free Realms" Snowhill 2010 gameplay`, `Free Realms Snowhill tour`,
  `Free Realms "Snowhill" quest`, `Free Realms Warrior "Drill Sergeant Dewey"`, `Free Realms Blacksmith Smitty`,
  `Free Realms "Singing Crystal Mines"`, `Free Realms "Snow Days" 2009`, `Free Realms "Snowhill stage" Bruce`,
  `Free Realms Sunrise Snowhill` (the live private server; its town will be the closest thing to a reference).
- Wayback, official site (news, quest guides, the "Snow Days" pages):
  `https://web.archive.org/web/2010*/freerealms.com/*`,
  `https://web.archive.org/web/2009*/http://www.freerealms.com/article/*`,
  `https://web.archive.org/web/*/freerealms.com/*snowhill*`,
  `https://web.archive.org/web/2013*/https://www.freerealms.com/*`.
- Wayback, official blog: `https://web.archive.org/web/2010*/freerealms.wordpress.com/*` (the wiki cites
  `freerealms.wordpress.com/2009/12/11/december-update-preview-pt-2` for Snow Days).
- Wayback, ZAM database (had per-NPC and per-quest pages with coordinates and dialogue):
  `https://web.archive.org/web/2011*/fr.zam.com/*`, `https://web.archive.org/web/*/fr.zam.com/wiki/*Snowhill*`,
  `https://web.archive.org/web/*/freerealms.zam.com/*`, `https://web.archive.org/web/*/freerealms.allakhazam.com/*`
  (try all three hostnames; ZAM moved it).
- Wayback, SOE forums: `https://web.archive.org/web/2010*/forums.station.sony.com/freerealms/*`.
- Fandom: `https://freerealms.fandom.com/wiki/Category:Snowhill`,
  `https://freerealms.fandom.com/wiki/Category:Quest_Givers`, `https://freerealms.fandom.com/wiki/Category:Merchant`,
  `https://freerealms.fandom.com/wiki/Category:Quest` (59 pages, all local),
  `https://freerealms.fandom.com/wiki/Special:WhatLinksHere/Snowhill` (pages that mention Snowhill but are
  not in the category), `https://freerealms.fandom.com/wiki/Special:Search?query=Snowhill&ns0=1&ns112=1`.
- Other wikis that existed: `https://web.archive.org/web/*/freerealms.wikia.com/wiki/Snowhill` (older
  revisions of the same wiki sometimes had more), `https://web.archive.org/web/*/freerealmsinsider.com/*`,
  BrawlWiki (Roland Spore's site, cited for the Diamondback Raceway).
- The client: `client/locale/en_us_data.dat` (already extracted), `UI/UiModules/Main/*.xml` tip strings, and
  the `Resources/SKY/*.xml` instance list for the mine/Town Hall interiors.

Per NPC lacking dialogue (search strings; drop the quotes if YouTube returns nothing):

- **Candi Ivy** — YouTube: `"Free Realms" "Candi Ivy"`, `Free Realms Snowhill Candi Ivy`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Candi%20Ivy*`, `https://web.archive.org/web/*/freerealms.com/*Candi%20Ivy*`; [wiki](https://freerealms.fandom.com/wiki/Candi%20Ivy); strings: grep `Candi Ivy` in `strings/snowhill_strings.tsv` (name id 419832).
- **Frostpetal** — YouTube: `"Free Realms" "Frostpetal"`, `Free Realms Snowhill Frostpetal`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Frostpetal*`, `https://web.archive.org/web/*/freerealms.com/*Frostpetal*`; [wiki](https://freerealms.fandom.com/wiki/Frostpetal); strings: grep `Frostpetal` in `strings/snowhill_strings.tsv` (name id 5448).
- **Lucca De'Flor** — YouTube: `"Free Realms" "Lucca De'Flor"`, `Free Realms Snowhill Lucca De'Flor`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Lucca%20De%27Flor*`, `https://web.archive.org/web/*/freerealms.com/*Lucca%20De%27Flor*`; [wiki](https://freerealms.fandom.com/wiki/Lucca%20De%27Flor); strings: grep `Lucca De'Flor` in `strings/snowhill_strings.tsv` (name id 23788).
- **Mayor Crystalline** — YouTube: `"Free Realms" "Mayor Crystalline"`, `Free Realms Snowhill Mayor Crystalline`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Mayor%20Crystalline*`, `https://web.archive.org/web/*/freerealms.com/*Mayor%20Crystalline*`; [wiki](https://freerealms.fandom.com/wiki/Mayor%20Crystalline); strings: grep `Mayor Crystalline` in `strings/snowhill_strings.tsv` (name id 182).
- **Ree Peatpants** — YouTube: `"Free Realms" "Ree Peatpants"`, `Free Realms Snowhill Ree Peatpants`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Ree%20Peatpants*`, `https://web.archive.org/web/*/freerealms.com/*Ree%20Peatpants*`; [wiki](https://freerealms.fandom.com/wiki/Ree%20Peatpants); strings: grep `Ree Peatpants` in `strings/snowhill_strings.tsv` (name id 388454).
- **Tevin** — YouTube: `"Free Realms" "Tevin"`, `Free Realms Snowhill Tevin`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Tevin*`, `https://web.archive.org/web/*/freerealms.com/*Tevin*`; [wiki](https://freerealms.fandom.com/wiki/Tevin); strings: grep `Tevin` in `strings/snowhill_strings.tsv` (name id 79022).
- **Arci Joan** — YouTube: `"Free Realms" "Arci Joan"`, `Free Realms Snowhill Arci Joan`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Arci%20Joan*`, `https://web.archive.org/web/*/freerealms.com/*Arci%20Joan*`; [wiki](https://freerealms.fandom.com/wiki/Arci%20Joan); strings: grep `Arci Joan` in `strings/snowhill_strings.tsv` (name id 391188).
- **Assistant Chef Edward** — YouTube: `"Free Realms" "Assistant Chef Edward"`, `Free Realms Snowhill Assistant Chef Edward`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Assistant%20Chef%20Edward*`, `https://web.archive.org/web/*/freerealms.com/*Assistant%20Chef%20Edward*`; [wiki](https://freerealms.fandom.com/wiki/Assistant%20Chief%20Edward); strings: grep `Assistant Chef Edward` in `strings/snowhill_strings.tsv` (name id 90184).
- **Bert** — YouTube: `"Free Realms" "Bert"`, `Free Realms Snowhill Bert`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Bert*`, `https://web.archive.org/web/*/freerealms.com/*Bert*`; [wiki](https://freerealms.fandom.com/wiki/Bert); strings: grep `Bert` in `strings/snowhill_strings.tsv` (name id 177).
- **Big J** — YouTube: `"Free Realms" "Big J"`, `Free Realms Snowhill Big J`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Big%20J*`, `https://web.archive.org/web/*/freerealms.com/*Big%20J*`; [wiki](https://freerealms.fandom.com/wiki/Big%20J); strings: grep `Big J` in `strings/snowhill_strings.tsv` (name id 6375).
- **Captain Ironsides** — YouTube: `"Free Realms" "Captain Ironsides"`, `Free Realms Snowhill Captain Ironsides`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Captain%20Ironsides*`, `https://web.archive.org/web/*/freerealms.com/*Captain%20Ironsides*`; [wiki](https://freerealms.fandom.com/wiki/Captain%20Ironsides); strings: grep `Captain Ironsides` in `strings/snowhill_strings.tsv` (name id 427724).
- **Chip Numbwing** — YouTube: `"Free Realms" "Chip Numbwing"`, `Free Realms Snowhill Chip Numbwing`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Chip%20Numbwing*`, `https://web.archive.org/web/*/freerealms.com/*Chip%20Numbwing*`; [wiki](https://freerealms.fandom.com/wiki/Chip%20Numbwing); strings: grep `Chip Numbwing` in `strings/snowhill_strings.tsv` (name id 408638).
- **Dempsy** — YouTube: `"Free Realms" "Dempsy"`, `Free Realms Snowhill Dempsy`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Dempsy*`, `https://web.archive.org/web/*/freerealms.com/*Dempsy*`; [wiki](https://freerealms.fandom.com/wiki/Dempsy); strings: grep `Dempsy` in `strings/snowhill_strings.tsv` (name id 5102200).
- **Ernie** — YouTube: `"Free Realms" "Ernie"`, `Free Realms Snowhill Ernie`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Ernie*`, `https://web.archive.org/web/*/freerealms.com/*Ernie*`; [wiki](https://freerealms.fandom.com/wiki/Ernie); strings: grep `Ernie` in `strings/snowhill_strings.tsv` (name id 179).
- **Fladnag** — YouTube: `"Free Realms" "Fladnag"`, `Free Realms Snowhill Fladnag`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Fladnag*`, `https://web.archive.org/web/*/freerealms.com/*Fladnag*`; [wiki](https://freerealms.fandom.com/wiki/Fladnag); strings: grep `Fladnag` in `strings/snowhill_strings.tsv` (name id 5102517).
- **Jonathon Forkpath** — YouTube: `"Free Realms" "Jonathon Forkpath"`, `Free Realms Snowhill Jonathon Forkpath`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Jonathon%20Forkpath*`, `https://web.archive.org/web/*/freerealms.com/*Jonathon%20Forkpath*`; [wiki](https://freerealms.fandom.com/wiki/Johnathon%20Forkpath); strings: grep `Jonathon Forkpath` in `strings/snowhill_strings.tsv` (name id 92578).
- **Kiel** — YouTube: `"Free Realms" "Kiel"`, `Free Realms Snowhill Kiel`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Kiel*`, `https://web.archive.org/web/*/freerealms.com/*Kiel*`; [wiki](https://freerealms.fandom.com/wiki/Kiel); strings: grep `Kiel` in `strings/snowhill_strings.tsv` (name id 6156).
- **Mr. Twinkle** — YouTube: `"Free Realms" "Mr. Twinkle"`, `Free Realms Snowhill Mr. Twinkle`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Mr.%20Twinkle*`, `https://web.archive.org/web/*/freerealms.com/*Mr.%20Twinkle*`; [wiki](https://freerealms.fandom.com/wiki/Mr.%20Twinkle); strings: grep `Mr. Twinkle` in `strings/snowhill_strings.tsv` (name id 3070).
- **Reba** — YouTube: `"Free Realms" "Reba"`, `Free Realms Snowhill Reba`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Reba*`, `https://web.archive.org/web/*/freerealms.com/*Reba*`; [wiki](https://freerealms.fandom.com/wiki/Reba); strings: grep `Reba` in `strings/snowhill_strings.tsv` (name id 5102420).
- **Senari** — YouTube: `"Free Realms" "Senari"`, `Free Realms Snowhill Senari`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Senari*`, `https://web.archive.org/web/*/freerealms.com/*Senari*`; [wiki](https://freerealms.fandom.com/wiki/Senari); strings: grep `Senari` in `strings/snowhill_strings.tsv` (name id 4726).
- **Skye** — YouTube: `"Free Realms" "Skye"`, `Free Realms Snowhill Skye`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Skye*`, `https://web.archive.org/web/*/freerealms.com/*Skye*`; [wiki](https://freerealms.fandom.com/wiki/Skye); strings: grep `Skye` in `strings/snowhill_strings.tsv` (name id 5102505).
- **Sonja** — YouTube: `"Free Realms" "Sonja"`, `Free Realms Snowhill Sonja`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Sonja*`, `https://web.archive.org/web/*/freerealms.com/*Sonja*`; [wiki](https://freerealms.fandom.com/wiki/Sonja); strings: grep `Sonja` in `strings/snowhill_strings.tsv` (name id 5102437).
- **Spratt** — YouTube: `"Free Realms" "Spratt"`, `Free Realms Snowhill Spratt`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Spratt*`, `https://web.archive.org/web/*/freerealms.com/*Spratt*`; [wiki](https://freerealms.fandom.com/wiki/Spratt); strings: grep `Spratt` in `strings/snowhill_strings.tsv` (name id 5102292).
- **Steele** — YouTube: `"Free Realms" "Steele"`, `Free Realms Snowhill Steele`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Steele*`, `https://web.archive.org/web/*/freerealms.com/*Steele*`; [wiki](https://freerealms.fandom.com/wiki/Steele); strings: grep `Steele` in `strings/snowhill_strings.tsv` (name id 47830).
- **Tommy** — YouTube: `"Free Realms" "Tommy"`, `Free Realms Snowhill Tommy`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Tommy*`, `https://web.archive.org/web/*/freerealms.com/*Tommy*`; [wiki](https://freerealms.fandom.com/wiki/Tommy); strings: grep `Tommy` in `strings/snowhill_strings.tsv` (name id 5101985).
- **Annabelle** — YouTube: `"Free Realms" "Annabelle"`, `Free Realms Snowhill Annabelle`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Annabelle*`, `https://web.archive.org/web/*/freerealms.com/*Annabelle*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Annabelle); strings: grep `Annabelle` in `strings/snowhill_strings.tsv` (name id 21849).
- **Autumn Mistflower** — YouTube: `"Free Realms" "Autumn Mistflower"`, `Free Realms Snowhill Autumn Mistflower`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Autumn%20Mistflower*`, `https://web.archive.org/web/*/freerealms.com/*Autumn%20Mistflower*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Autumn%20Mistflower); strings: grep `Autumn Mistflower` in `strings/snowhill_strings.tsv` (name id 394914).
- **Brian** — YouTube: `"Free Realms" "Brian"`, `Free Realms Snowhill Brian`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Brian*`, `https://web.archive.org/web/*/freerealms.com/*Brian*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Brian); strings: grep `Brian` in `strings/snowhill_strings.tsv` (name id 3427).
- **Brittany** — YouTube: `"Free Realms" "Brittany"`, `Free Realms Snowhill Brittany`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Brittany*`, `https://web.archive.org/web/*/freerealms.com/*Brittany*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Brittany); strings: grep `Brittany` in `strings/snowhill_strings.tsv` (name id 3062).
- **Calvin Coldcastle** — YouTube: `"Free Realms" "Calvin Coldcastle"`, `Free Realms Snowhill Calvin Coldcastle`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Calvin%20Coldcastle*`, `https://web.archive.org/web/*/freerealms.com/*Calvin%20Coldcastle*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Calvin%20Coldcastle); strings: grep `Calvin Coldcastle` in `strings/snowhill_strings.tsv` (name id 420397).
- **Carly Collision** — YouTube: `"Free Realms" "Carly Collision"`, `Free Realms Snowhill Carly Collision`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Carly%20Collision*`, `https://web.archive.org/web/*/freerealms.com/*Carly%20Collision*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Carly%20Collision); strings: grep `Carly Collision` in `strings/snowhill_strings.tsv` (name id 19371).
- **Cinn** — YouTube: `"Free Realms" "Cinn"`, `Free Realms Snowhill Cinn`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Cinn*`, `https://web.archive.org/web/*/freerealms.com/*Cinn*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Cinn); strings: grep `Cinn` in `strings/snowhill_strings.tsv` (name id 4554).
- **Coin Farmer Lubag** — YouTube: `"Free Realms" "Coin Farmer Lubag"`, `Free Realms Snowhill Coin Farmer Lubag`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Coin%20Farmer%20Lubag*`, `https://web.archive.org/web/*/freerealms.com/*Coin%20Farmer%20Lubag*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Coin%20Farmer%20Lubag); strings: grep `Coin Farmer Lubag` in `strings/snowhill_strings.tsv` (name id 429152).
- **Connor Rush** — YouTube: `"Free Realms" "Connor Rush"`, `Free Realms Snowhill Connor Rush`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Connor%20Rush*`, `https://web.archive.org/web/*/freerealms.com/*Connor%20Rush*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Connor%20Rush); strings: grep `Connor Rush` in `strings/snowhill_strings.tsv` (name id 19364).
- **Crafty Robgoblin** — YouTube: `"Free Realms" "Crafty Robgoblin"`, `Free Realms Snowhill Crafty Robgoblin`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Crafty%20Robgoblin*`, `https://web.archive.org/web/*/freerealms.com/*Crafty%20Robgoblin*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Crafty%20Robgoblin); strings: grep `Crafty Robgoblin` in `strings/snowhill_strings.tsv` (name id 420583).
- **Downey** — YouTube: `"Free Realms" "Downey"`, `Free Realms Snowhill Downey`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Downey*`, `https://web.archive.org/web/*/freerealms.com/*Downey*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Downey); strings: grep `Downey` in `strings/snowhill_strings.tsv` (name id 90117).
- **Drill Sergeant Dewey** — YouTube: `"Free Realms" "Drill Sergeant Dewey"`, `Free Realms Snowhill Drill Sergeant Dewey`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Drill%20Sergeant%20Dewey*`, `https://web.archive.org/web/*/freerealms.com/*Drill%20Sergeant%20Dewey*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Drill%20Sergeant%20Dewey); strings: grep `Drill Sergeant Dewey` in `strings/snowhill_strings.tsv` (name id 22435).
- **Edward** — YouTube: `"Free Realms" "Edward"`, `Free Realms Snowhill Edward`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Edward*`, `https://web.archive.org/web/*/freerealms.com/*Edward*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Edward); strings: grep `Edward` in `strings/snowhill_strings.tsv` (name id 39812).
- **Everett** — YouTube: `"Free Realms" "Everett"`, `Free Realms Snowhill Everett`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Everett*`, `https://web.archive.org/web/*/freerealms.com/*Everett*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Everett); strings: grep `Everett` in `strings/snowhill_strings.tsv` (name id 420809).
- **Freddy MacIsaac** — YouTube: `"Free Realms" "Freddy MacIsaac"`, `Free Realms Snowhill Freddy MacIsaac`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Freddy%20MacIsaac*`, `https://web.archive.org/web/*/freerealms.com/*Freddy%20MacIsaac*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Freddy%20MacIsaac); strings: grep `Freddy MacIsaac` in `strings/snowhill_strings.tsv` (name id 385662).
- **Garrison Gold** — YouTube: `"Free Realms" "Garrison Gold"`, `Free Realms Snowhill Garrison Gold`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Garrison%20Gold*`, `https://web.archive.org/web/*/freerealms.com/*Garrison%20Gold*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Garrison%20Gold); strings: grep `Garrison Gold` in `strings/snowhill_strings.tsv` (name id 38092).
- **Gerold** — YouTube: `"Free Realms" "Gerold"`, `Free Realms Snowhill Gerold`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Gerold*`, `https://web.archive.org/web/*/freerealms.com/*Gerold*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Gerold); strings: grep `Gerold` in `strings/snowhill_strings.tsv` (name id 104054).
- **Grog** — YouTube: `"Free Realms" "Grog"`, `Free Realms Snowhill Grog`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Grog*`, `https://web.archive.org/web/*/freerealms.com/*Grog*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Grog); strings: grep `Grog` in `strings/snowhill_strings.tsv` (name id 3224).
- **Holly Singsong** — YouTube: `"Free Realms" "Holly Singsong"`, `Free Realms Snowhill Holly Singsong`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Holly%20Singsong*`, `https://web.archive.org/web/*/freerealms.com/*Holly%20Singsong*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Holly%20Singsong); strings: grep `Holly Singsong` in `strings/snowhill_strings.tsv` (name id 46947).
- **Jet Madedge** — YouTube: `"Free Realms" "Jet Madedge"`, `Free Realms Snowhill Jet Madedge`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Jet%20Madedge*`, `https://web.archive.org/web/*/freerealms.com/*Jet%20Madedge*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Jet%20Madedge); strings: grep `Jet Madedge` in `strings/snowhill_strings.tsv` (name id 19368).
- **Jet Swiftpass** — YouTube: `"Free Realms" "Jet Swiftpass"`, `Free Realms Snowhill Jet Swiftpass`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Jet%20Swiftpass*`, `https://web.archive.org/web/*/freerealms.com/*Jet%20Swiftpass*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Jet%20Swiftpass); strings: grep `Jet Swiftpass` in `strings/snowhill_strings.tsv` (name id 19363).
- **Joel** — YouTube: `"Free Realms" "Joel"`, `Free Realms Snowhill Joel`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Joel*`, `https://web.archive.org/web/*/freerealms.com/*Joel*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Joel); strings: grep `Joel` in `strings/snowhill_strings.tsv` (name id 3429).
- **Julie** — YouTube: `"Free Realms" "Julie"`, `Free Realms Snowhill Julie`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Julie*`, `https://web.archive.org/web/*/freerealms.com/*Julie*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Julie); strings: grep `Julie` in `strings/snowhill_strings.tsv` (name id 3310).
- **Layla Octane** — YouTube: `"Free Realms" "Layla Octane"`, `Free Realms Snowhill Layla Octane`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Layla%20Octane*`, `https://web.archive.org/web/*/freerealms.com/*Layla%20Octane*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Layla%20Octane); strings: grep `Layla Octane` in `strings/snowhill_strings.tsv` (name id 19361).
- **Lealin** — YouTube: `"Free Realms" "Lealin"`, `Free Realms Snowhill Lealin`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Lealin*`, `https://web.archive.org/web/*/freerealms.com/*Lealin*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Lealin); strings: grep `Lealin` in `strings/snowhill_strings.tsv` (name id 5454).
- **Lily Leadfoot** — YouTube: `"Free Realms" "Lily Leadfoot"`, `Free Realms Snowhill Lily Leadfoot`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Lily%20Leadfoot*`, `https://web.archive.org/web/*/freerealms.com/*Lily%20Leadfoot*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Lily%20Leadfoot); strings: grep `Lily Leadfoot` in `strings/snowhill_strings.tsv` (name id 19358).
- **Linnie** — YouTube: `"Free Realms" "Linnie"`, `Free Realms Snowhill Linnie`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Linnie*`, `https://web.archive.org/web/*/freerealms.com/*Linnie*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Linnie); strings: grep `Linnie` in `strings/snowhill_strings.tsv` (name id 139451).
- **Little J** — YouTube: `"Free Realms" "Little J"`, `Free Realms Snowhill Little J`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Little%20J*`, `https://web.archive.org/web/*/freerealms.com/*Little%20J*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Little%20J); strings: grep `Little J` in `strings/snowhill_strings.tsv` (name id 5449).
- **Loryn** — YouTube: `"Free Realms" "Loryn"`, `Free Realms Snowhill Loryn`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Loryn*`, `https://web.archive.org/web/*/freerealms.com/*Loryn*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Loryn); strings: grep `Loryn` in `strings/snowhill_strings.tsv` (name id 178).
- **Mai Redline** — YouTube: `"Free Realms" "Mai Redline"`, `Free Realms Snowhill Mai Redline`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Mai%20Redline*`, `https://web.archive.org/web/*/freerealms.com/*Mai%20Redline*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Mai%20Redline); strings: grep `Mai Redline` in `strings/snowhill_strings.tsv` (name id 19362).
- **Marcie** — YouTube: `"Free Realms" "Marcie"`, `Free Realms Snowhill Marcie`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Marcie*`, `https://web.archive.org/web/*/freerealms.com/*Marcie*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Marcie); strings: grep `Marcie` in `strings/snowhill_strings.tsv` (name id 36807).
- **Mikko Ragerunner** — YouTube: `"Free Realms" "Mikko Ragerunner"`, `Free Realms Snowhill Mikko Ragerunner`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Mikko%20Ragerunner*`, `https://web.archive.org/web/*/freerealms.com/*Mikko%20Ragerunner*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Mikko%20Ragerunner); strings: grep `Mikko Ragerunner` in `strings/snowhill_strings.tsv` (name id 19367).
- **Milus Featherfeet** — YouTube: `"Free Realms" "Milus Featherfeet"`, `Free Realms Snowhill Milus Featherfeet`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Milus%20Featherfeet*`, `https://web.archive.org/web/*/freerealms.com/*Milus%20Featherfeet*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Milus%20Featherfeet); strings: grep `Milus Featherfeet` in `strings/snowhill_strings.tsv` (name id 72171).
- **Morninglory** — YouTube: `"Free Realms" "Morninglory"`, `Free Realms Snowhill Morninglory`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Morninglory*`, `https://web.archive.org/web/*/freerealms.com/*Morninglory*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Morninglory); strings: grep `Morninglory` in `strings/snowhill_strings.tsv` (name id 16826).
- **Murphy** — YouTube: `"Free Realms" "Murphy"`, `Free Realms Snowhill Murphy`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Murphy*`, `https://web.archive.org/web/*/freerealms.com/*Murphy*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Murphy); strings: grep `Murphy` in `strings/snowhill_strings.tsv` (name id 3063).
- **Nikki** — YouTube: `"Free Realms" "Nikki"`, `Free Realms Snowhill Nikki`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Nikki*`, `https://web.archive.org/web/*/freerealms.com/*Nikki*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Nikki); strings: grep `Nikki` in `strings/snowhill_strings.tsv` (name id 429539).
- **Nina the Wrecker** — YouTube: `"Free Realms" "Nina the Wrecker"`, `Free Realms Snowhill Nina the Wrecker`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Nina%20the%20Wrecker*`, `https://web.archive.org/web/*/freerealms.com/*Nina%20the%20Wrecker*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Nina%20the%20Wrecker); strings: grep `Nina the Wrecker` in `strings/snowhill_strings.tsv` (name id 19370).
- **Robgoblin Builder** — YouTube: `"Free Realms" "Robgoblin Builder"`, `Free Realms Snowhill Robgoblin Builder`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Robgoblin%20Builder*`, `https://web.archive.org/web/*/freerealms.com/*Robgoblin%20Builder*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Robgoblin%20Builder); strings: grep `Robgoblin Builder` in `strings/snowhill_strings.tsv` (name id 420570).
- **Scarlet Shadeveil** — YouTube: `"Free Realms" "Scarlet Shadeveil"`, `Free Realms Snowhill Scarlet Shadeveil`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Scarlet%20Shadeveil*`, `https://web.archive.org/web/*/freerealms.com/*Scarlet%20Shadeveil*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Scarlet%20Shadeveil); strings: grep `Scarlet Shadeveil` in `strings/snowhill_strings.tsv` (name id 36923).
- **Screamin' Steven** — YouTube: `"Free Realms" "Screamin' Steven"`, `Free Realms Snowhill Screamin' Steven`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Screamin%27%20Steven*`, `https://web.archive.org/web/*/freerealms.com/*Screamin%27%20Steven*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Screamin%27%20Steven); strings: grep `Screamin' Steven` in `strings/snowhill_strings.tsv` (name id 19344).
- **Sol** — YouTube: `"Free Realms" "Sol"`, `Free Realms Snowhill Sol`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Sol*`, `https://web.archive.org/web/*/freerealms.com/*Sol*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Sol); strings: grep `Sol` in `strings/snowhill_strings.tsv` (name id 7054).
- **Spindle** — YouTube: `"Free Realms" "Spindle"`, `Free Realms Snowhill Spindle`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Spindle*`, `https://web.archive.org/web/*/freerealms.com/*Spindle*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Spindle); strings: grep `Spindle` in `strings/snowhill_strings.tsv` (name id 420500).
- **Tad Slopeslider** — YouTube: `"Free Realms" "Tad Slopeslider"`, `Free Realms Snowhill Tad Slopeslider`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Tad%20Slopeslider*`, `https://web.archive.org/web/*/freerealms.com/*Tad%20Slopeslider*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Tad%20Slopeslider); strings: grep `Tad Slopeslider` in `strings/snowhill_strings.tsv` (name id 117257).
- **Tanda T. Toes** — YouTube: `"Free Realms" "Tanda T. Toes"`, `Free Realms Snowhill Tanda T. Toes`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Tanda%20T.%20Toes*`, `https://web.archive.org/web/*/freerealms.com/*Tanda%20T.%20Toes*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Tanda%20T.%20Toes); strings: grep `Tanda T. Toes` in `strings/snowhill_strings.tsv` (name id 91660).
- **Tucker Thunderjolt** — YouTube: `"Free Realms" "Tucker Thunderjolt"`, `Free Realms Snowhill Tucker Thunderjolt`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Tucker%20Thunderjolt*`, `https://web.archive.org/web/*/freerealms.com/*Tucker%20Thunderjolt*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Tucker%20Thunderjolt); strings: grep `Tucker Thunderjolt` in `strings/snowhill_strings.tsv` (name id 19365).
- **Vaelen Warpwatcher** — YouTube: `"Free Realms" "Vaelen Warpwatcher"`, `Free Realms Snowhill Vaelen Warpwatcher`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Vaelen%20Warpwatcher*`, `https://web.archive.org/web/*/freerealms.com/*Vaelen%20Warpwatcher*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Vaelen%20Warpwatcher); strings: grep `Vaelen Warpwatcher` in `strings/snowhill_strings.tsv` (name id 130731).
- **Willie** — YouTube: `"Free Realms" "Willie"`, `Free Realms Snowhill Willie`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Willie*`, `https://web.archive.org/web/*/freerealms.com/*Willie*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Willie); strings: grep `Willie` in `strings/snowhill_strings.tsv` (name id 36827).
- **Willow Slamdash** — YouTube: `"Free Realms" "Willow Slamdash"`, `Free Realms Snowhill Willow Slamdash`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Willow%20Slamdash*`, `https://web.archive.org/web/*/freerealms.com/*Willow%20Slamdash*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Willow%20Slamdash); strings: grep `Willow Slamdash` in `strings/snowhill_strings.tsv` (name id 19366).
- **Wintara** — YouTube: `"Free Realms" "Wintara"`, `Free Realms Snowhill Wintara`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Wintara*`, `https://web.archive.org/web/*/freerealms.com/*Wintara*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Wintara); strings: grep `Wintara` in `strings/snowhill_strings.tsv` (name id 7053).
- **Yoink** — YouTube: `"Free Realms" "Yoink"`, `Free Realms Snowhill Yoink`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Yoink*`, `https://web.archive.org/web/*/freerealms.com/*Yoink*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Yoink); strings: grep `Yoink` in `strings/snowhill_strings.tsv` (name id 441346).
- **Zoomer Rex** — YouTube: `"Free Realms" "Zoomer Rex"`, `Free Realms Snowhill Zoomer Rex`; Wayback: `https://web.archive.org/web/*/fr.zam.com/*Zoomer%20Rex*`, `https://web.archive.org/web/*/freerealms.com/*Zoomer%20Rex*`; [Fandom search](https://freerealms.fandom.com/wiki/Special:Search?query=Zoomer%20Rex); strings: grep `Zoomer Rex` in `strings/snowhill_strings.tsv` (name id 19359).

## Method and caveats

- `npc-spawns/npc_spawns_labeled.json` is yungcomputerchair/free-realms-re's decode of 2010-era `AddNpc`
  packets (3,521 rows deduped by guid+xyz; 3,470 distinct guids; 2,366 rows if deduped by name+model+rounded
  position, so ~1/3 are the same NPC re-sent across sessions). Labels come from `FabledRealmsAreas.xml`
  ambience volumes, which is why "snowhill" claims 2,393 rows: **2,015 of them are `hsg_*` housing furniture
  from a player-house instance whose local coordinates (x 320..345, z 385..410, y stacked -10..29) fall inside
  `SnowHill_Bed_15`.** Real Snowhill is ~380 rows. The "sanctuary" region likewise has 70 housing rows.
  NPC_CAPTURES.md already flags that zone attribution is positional only.
- Names: `NameId` in `AddNpc` is a `Global.Text.{id}` string id; the `.dat` stores entries under the Jenkins
  lookup2 hash of that key (format from Udaya-X2/FreeRealmsLocaleTools, MIT). `strings/en_us.json` maps
  id -> text for all 85,932 hashed entries (0 unmapped) plus the 2,079 entries that carry their id inline.
  All 3,521 spawns resolve. `src/Resources/Npcs.json` has names for 1,659 of 4,253 entries but only 51 of
  the 637 spawn NameIds appear in it, so the string table is the real name source.
- Models: `src/Resources/Models.txt` (id -> .adr/.agr).
- Town membership: point-in-volume against every `FabledRealmsAreas.xml` volume whose name matches the town
  (sphere or box). Volumes are ambience zones, not town borders, so edge NPCs (Ree Peatpants, Gerold) were
  added by hand and a few Wilds NPCs near Snowhill's entrance may be missing.
- Wiki content is CC-BY-SA; page URLs are embedded in each `.wikitext` header.
- Files written by this pass: `npc-spawns/{npc_spawns_unique,npc_spawns_labeled}.json`, `npc_spawns_labeled.csv`,
  `NPC_CAPTURES.md`, `npc_spawns_named.json` (every spawn with name + model file + region/area),
  `candidate_town_rosters.json`, `snowhill_roster.csv`, `snowhill_roster_table.md`, `build_snowhill_roster.py`;
  `strings/en_us.json`, `strings/snowhill_strings.tsv`; `wiki/categories/*.txt` (category members for the
  eight towns), `wiki/npcs/*.wikitext` (88 NPC/POI pages for Snowhill and Lakeshore), 7 more `wiki/quests/` (now 59 = the whole namespace).
  `strings/snowhill_strings.tsv` matches on NPC names too, so short names (Bert, Grog, Cinn, Nikki) pull in
  unrelated strings; filter by id range once a quest's block is found.
