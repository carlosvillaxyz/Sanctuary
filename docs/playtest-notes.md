# Playtest notes

Carlos's observations from playing, newest first. Each note: what happened, what the original did (if known),
and whether it is a bug, a missing server feature, or a quality-of-life change. Triage decides which roadmap
phase it lands in; QoL changes must pass the "remaster, not reimagine" rule in ROADMAP.md.

## 2026-09-11 — combat, third session (overworld combat merged from the port)

Working: knockout and the respawn window, wolves aggro on the road and leash back, the two attack buttons, job
health scaling with level, enemies respawning.

### Fixed the same day (commit after this play-test)
- **Health bars on every NPC, forest critters and the car-crash bystanders included.** op41/132 SetInWorldCombat
  is a GLOBAL client switch: it draws the floating damage numbers *and* bars every nameplate in view. Turning it
  off removed the friendly bars and kept the enemy bars (those come from the enemy's own hitpoints) but took the
  damage numbers with it. The numbers now come from op32/7 AttackProcessed instead - the same packet an enemy
  already uses when it hits us - which carries the number, the bar and the hit effect in one, with the global
  switch left off.
- **A camp where only one hooligan fought back and the rest could not be hit.** Sony shipped the display name
  "Hooligan" under two string ids; Enemies.json listed only 5100401, so the three placed under the older 20483
  (23623, 23624, 23626, around archer 23625) spawned as scenery: white name, no bar, no aggro, unhittable. A
  data test over the shipped files now fails if any NPC shares a name with an enemy and is not one - it found a
  second case immediately, the two Mini Necrowart Zombies on model 73 standing beside their hostile twins.
- **No stars for kills.** The kill path never paid experience - the combat commit's message said it did, but the
  change was not in it. A kill now pays the enemy's stars (Enemies.json XpByLevel x tier) into the job that
  landed it, so fighting levels you and fires the level-up celebration.
- **Respawning at 2,500 health at level 1.** The server kept ClientPcData's 2,500 placeholder and a current
  health of 0 all session: the client was told the right numbers at login, the server was not. So the first
  enemy hit knocked the player out instantly and the revive that followed handed back 2,500. Health, regen and
  a full bar are now applied on world entry, which is also what a job switch was accidentally fixing.
- **The blue streak from Leg Sweep stuck to the character for the rest of the session, across job switches.**
  The trail is an attached effect with no end trigger of its own; sent as a cast effect nothing ever stopped it.
  It is now added and pulled by effect tag, like the boombox song.
- **The light attack showed no effects.** It has the standard hit flash, which only plays where a hit lands -
  and the hooligans it was being swung at were not hittable. Should come back with them.

### Still open
- **Enemy AI is each-mob-for-itself.** Carlos: a nearby enemy should be able to alert its neighbours. An enemy
  only hunts players its own zone tile has handed it, so a camp never reacts as a group. Wanted for the combat
  improvement pass, together with the richer 2009-era combat.
- **Combat is basic** compared to the 2009 build Carlos wants to explore once this is solid.
- **Kart racing.** The racing minigame is in the client; its server side was Sony's and would be a project of
  its own rather than a quest fix.

## 2026-09-11 — quests, second session (branch `quests`: Sulphural's quest system + PR #120 levelling)
Working: real quest-offer window with rewards and styled buttons, overhead "!" markers, map and minimap
tracking, tracker panel with "Take Me There", journal, "New Goal" banners, quest chat log, rewards. "Introduce
Yourself" and "Call the Crew" (to Shakey at Wildwood Speedway) play through.
- **Talk cursor only on the "!" icon.** Hovering the NPC's body keeps the purple pointer; hovering the overhead
  "!" shows a hand. Carlos remembers a purple speech-bubble cursor on the NPC itself. Open: the NPC-relevance
  cursor packet is sent but has no visible effect; may need another flag.
- **Journal opens on the wrong page.** First open shows a different quest area until the book tab ("Other
  Quests") is clicked.
- **Tracker glitches after closing the journal.** The quest detail panel needed its arrows clicked to recover.
- **Dialogue camera too close.** Tune the camera focus parameter; compare with the MMOHut footage.
- **No level-up.** Quest XP was display-only in that branch. Fixed the same night: quest XP now uses PR #120's
  real levelling. The rank message was also missing two fields and the full-screen celebration was never sent;
  both fixed. **Verified in client:** replaying the two quests takes Rick to level 2 with the job level-up badge
  animation (brief) and the particle burst.
- **NPCs feel static.** Final-week captures: 3,475 of 3,521 NPCs spawned in the plain idle; ~46 used special idles
  (sleeping dogs, dancing robgoblins). Walking routes and NPC chatter: under investigation in the Cobblestone plan.
- **The `!exp` dev command** (prefix is `!`, not `/`). Carlos: things not in the original game shouldn't be
  player-facing. Proposed: lock dev commands to admin accounts.
- **Coins.** New characters get 999,999,999 coins (dev value in `login.json`). Carlos will recreate his character
  once the economy is designed.

## 2026-09-10 — first playthrough
- All jobs start at level 20. (Fixed on `integrate-upstream`: characters now start at level 1.)
- NPCs cannot be clicked and have no dialogue. (In progress: conversation-window spike, untested.)
- Buying a house fails with "An error occurred while placing your order". (Housing purchase not implemented.)
- Loading screens are low-res Station Cash advertisements. (Phase 1: replace via LoadingScreen.xml.)
- Teleport stones, map, town-entry banners work.

### Character creation
- **Name picker is slow.** Names are chosen from preset lists with up/down arrows: a first name plus a two-part
  last name (prefix + suffix), ~1,300 entries, alphabetical. Reaching a name near the end takes many clicks, and
  holding an arrow does not repeat. Wanted: mouse-wheel scrolling, hold-to-repeat, or type-to-jump.
  _Finding:_ the client also contains a **custom-name screen** (`characterCustomNameScreen.swf`, streamed) next to
  the picker, and the server already stores free-text names and validates their characters. Enabling the game's
  own custom-name path is likely cleaner than editing the picker's Flash. Needs: find what switches it on.
- **Eye colour swatches are grey** (e.g. Human Male "Low Tide", "High Tide", "Dark Tide"): same icon, grey
  background, no colour. _Finding:_ the data is intact — every eye colour deliberately uses icon 6495 with a
  per-colour tint (`CharacterCreateCustomizationEyeColor.txt`, ICON_TINT column). Grey means the tint is not
  being applied. Bug; cause not yet located.

### NPC interaction (reference: MMOHut video, likely 2009-2010 build — Harold, "Brawler: Hewey's Escape!")
- Hovering a talkable NPC changed the cursor to a speech icon. _Finding:_ `Cursors.txt` has `cursor_talk` (5) and
  `cursor_interaction_talk` (13); the server sends a cursor id per NPC and currently sends 0 (default pointer).
- Clicking zoomed the camera in on the NPC and showed the quest-offer window: the NPC's line in a speech balloon,
  a quest card with "Show Details", green "Yes, I accept!" and "No! I decline." The line also went to the chat log.
  _Finding:_ that window is `queststart.swf`, present in the streamed assets. Its server packet (quest family,
  opcode 49) has not been reverse-engineered by anyone yet.
- Minor NPCs spoke in over-head chat bubbles, like player chat. Keep that split: bubbles for ambient NPCs,
  windows for quest and story NPCs.

### Housing and economy
- Housing purchase fails. Carlos remembers a free starter Apartment: "a floating box with a little furniture".
  _Finding:_ the real house interiors survive as zones in the streamed assets: `hsg_hum_condo` (the Wilds Condo,
  the free starter home from Jan 2012, which replaced the Apartment), economy, deluxe, Snowhill, Seaside beach
  house, Blackspore, night club, Christmas houses, plus 24 empty lots. The server has 31 house definitions with
  spawn points and build areas. Upstream PR #111 drafts the housing editor and directory. About 20 furniture
  assets so far 404 on the community CDN.
- The economy was built around Station Cash microtransactions, which no longer exist. Needs a design decision.
