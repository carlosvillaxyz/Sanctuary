# Playtest notes

Carlos's observations from playing, newest first. Each note: what happened, what the original did (if known),
and whether it is a bug, a missing server feature, or a quality-of-life change. Triage decides which roadmap
phase it lands in; QoL changes must pass the "remaster, not reimagine" rule in ROADMAP.md.

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
  real levelling (persisted, rank table, level-up toast).
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
