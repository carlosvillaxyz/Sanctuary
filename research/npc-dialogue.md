# NPC dialogue: what the game did, what the client can show, what we have, how to build it

Research notes, 2026-09-10. Read-only survey; nothing here is implemented yet.
Sources: this repo (`src/`, `client/`, `research/wiki/`), `yungcomputerchair/free-realms-re` (PACKETS.md,
catalog.db), `yungcomputerchair/free-realms-ai-decomp`, `Bik182/Free-Realms` (UI dump), `soir20/oxide` +
`soir20/cwa-research` (Clone Wars Adventures, same engine), upstream Sanctuary PRs #109 and #119, Fandom wiki,
2009 reviews. Reference-only repos (no licence): free-realms-re, free-realms-ai-decomp, Bik182 — learn from,
do not copy. oxide is AGPL like us.

## A. What the original game (2009-2014) did

**Short answer: a dedicated full-screen-ish Flash window with the NPC's text typed out teletype-style, the
quest name/description/rewards, and green "Yes, I accept!" / grey "Goodbye" (or "Decline") buttons. Not a
bubble. Chat bubbles were for players and for ambient NPC barks. A few NPCs were voiced (tutorial and some
event NPCs); the general population was text-only.**

Evidence:

1. **Fandom wiki, Sacred Glade (tutorial) page** (`research/wiki/pages/Sacred_Glade.wikitext` lines 14, 40):
   "Michael Tallstrider frantically runs up to you. [...] Click the gray *Goodbye* button to continue." and
   "Click on Michael and learn that the robgoblins chased a pig from Farmer Chug's pen. Click on the green
   *Yes, I accept!* button to receive the Tutorial: Wilbur's Trouble quest." So: click NPC -> window with text
   -> Accept / Goodbye buttons. Also "Michael Tallstrider will have a green circled question mark above his
   head" (turn-in marker; offer marker is the green exclamation per the Quests page).
2. **Client Lua (compiled, `client/UI/ScriptsBase.bin`)** — function/field names recovered from the bytecode:
   - `queststart.lua` / `QuestStartHandler`: window `Main.wndQuestStart` (and `Main.wndQuestEnd`, both load
     `UI\queststart.swf`), functions `showNPCDialog`, `showName`, `showDescription`, `showMembersOnly`,
     `SetNPCDialog`, `SetQuest`, `SetRewardCoin`, `SetRewardExp`, `AddRewardItem`, `PopulateRewards`,
     `AddDialogResponseNodes` / `responseButtons`, `undeclinable` / `noDecline`, `ShowEndScreen`,
     `DismissEndScreen`, and `setTextTeletypeTickTime` driven by the user option
     `MessageOptions.TextTeletypeTickTime` — i.e. the NPC text is typed out letter by letter at a user-set
     speed. Reward data comes from client data sources `BaseClient.Quest.Reward` / `.Reward.Entries`.
   - `dialog.lua` / `DialogHandler`: a *second*, generic NPC conversation window `Main.wndFlashDialog`
     (`client/UI/UiModules/Main/wndDialogNew.xml`, 560x760, loads `UI\dialog.swf`, Lua alias `wndNpcDialog` /
     `swfNpcDialog`) with `SetDialogText(text, actionText)`, `SetChoiceText`, `setDialogResponse(index, ...)`,
     `bgImageSetId` / `imageId` (a background image set and an image id — the "portrait"/scene art slot),
     `SelectOption`, `DialogSelect`, `DialogEnd`, `SetDialogCameraOffset2D`. This is the multi-choice
     conversation window, distinct from the quest offer window.
   - `bub.lua` / `ChatBubbleManager`: over-head chat bubbles, fed by `OnReceiveSay` / `OnReceiveEmote` /
     `OnReceiveWhisper` for the chat channels (WorldSay, GroupSay, AreaSay, Shout, ...). The client binary has
     an `OverHeadChatBubbleElement` class. Bubbles are the *chat* rendering, not the quest-dialogue rendering.
   - `tutorial_dialog.lua`: `wndTutorialDialog` (`tutorial_dialog.swf`) has `OnVoComplete` and
     `tutorialDialogLockForVO` — the tutorial's dialog window waited for a voice-over to finish.
3. **Locale strings** (`client/locale/en_us_data.dat`): "Yes, I accept!" (id 1872662444), "Goodbye"
   (108431211), "Decline" (853532324), "Continue" (2164885961), several "Next". (These are the hash-form ids
   used inside the `.dat`; the wire uses the small numeric ids, see open question 5.)
4. **Voice**: `client/Assets_003.pack` contains 23 files named `<Npc>Dialog<id>.mp3` for exactly five NPCs —
   MichaelTallstrider (5), FarmerChug (5), Cookie (6), AshleyLightwings (5), Flanders (3) — i.e. the Sacred
   Glade tutorial cast, plus `DIA_Raptor_Speak_*.mp3`. Nothing comparable exists for town quest-givers among
   ~12,800 audio names in the packs. Sony's localisation PR ("fully-localized voice over audio") and the
   3.8/5 "Music / Sound FX / Voice Acting" score in the Cheat Code Central review agree with "some voice,
   mostly tutorial and cinematics". So: **voiced tutorial, text-only quests in general**.
5. **Reviews**: MMORPG.com (2009): "Your active quest has green dots leading you around by the nose, and at the
   end of the quest, the final NPC is lit by a shaft of light from heaven" / "The NPC scripting is often pure
   genius and always hilarious". Confirms the breadcrumb trail and the lit turn-in NPC; no mention of voice.
6. **Verbatim quest text** in `research/wiki/quests/*.wikitext` reads as multi-sentence paragraphs with
   coloured keywords (`<font color="#CE7C28">find</font>`), which fits a text panel, not a bubble.

Player-to-player interaction, for contrast, was a radial menu (`CommandPacketInteractionStartWheel`,
`wndBrowserV2`/`AdvancedInteraction.lua` for >N buttons).

## B. What the client can display today (the ceiling without patching the exe)

All of these are driven from the server; the client (v1.910, 2014) contains the UI and the handlers.

### B1. Windows (Scaleform SWF wrapped in window XML, `client/UI/UiModules/Main/`)

| Window XML | SWF | Lua handler | Purpose |
|---|---|---|---|
| `wndQuestStart.xml` (800x768) | `queststart.swf` | `QuestStartHandler` | Quest offer: NPC text (teletype), quest name, description, members-only flag, rewards (coins/XP/items), response buttons, Accept/Decline. Also used as `wndQuestEnd` for turn-in ("end screen"). |
| `wndDialogNew.xml` -> window `wndFlashDialog` (560x760) | `dialog.swf` | `DialogHandler` | Generic NPC conversation: text + action text, N choice buttons, image set/id, camera offset. |
| `wndDialog.xml` (495x336, "Conversation") | none (native widgets) | – | Legacy pre-Flash version: a `BubbleEdit` text box + 5 buttons `btn0..btn4`. Confirms the model "text + up to 5 responses". |
| `wndInteractionPrompt.xml` | `interactionPrompt.swf` | `InteractionPrompt.lua` | The "[E] Talk to <name>" prompt for the closest interactable NPC (`BaseClient.ClosestInteractionTargetData`). |
| `wndQuestHelper.xml`, `wndQuestScreen.xml` (`questjournal.swf`), `wndNotificationQuests.xml` | – | `QuestHelper`, journal | Tracker, journal, toasts. |
| `wndTutorialDialog.xml` | `tutorial_dialog.swf` | `TutorialDialog.lua` | Tutorial dialog with VO wait. |
| `wndNpcMerchant*.xml` | – | – | Vendor window. |

The Bik182 dump has the same 267 XMLs as our local `client/UI/UiModules/Main` (no Lua sources, no SWF
sources). The SWFs live inside the packs (`dialog.swf` in `Assets_004.pack`, `tutorial_dialog.swf` in
`Assets_003.pack`); `queststart.swf` was not found by name in the local packs — see open question 4.

### B2. Packets (opcode / sub-opcode, from catalog.db + Sanctuary + oxide)

**BaseCommandPacket, opcode 26 (0x1a)** — the interaction/dialog channel. Layout `[i16 op=26][i16 sub]`:

| sub | Name | Dir | Fields (what is known) |
|---|---|---|---|
| 3 | `CommandPacketShowDialog` | s2c | catalog: "large: dword@+0x10; i32@+0x14=-1; vec@+0x18..; multiple Vec3 + flags". oxide (CWA, same engine) decodes it as `EnterDialog { dialog_message_id u32, speaker_animation_id i32, speaker_guid u64, enable_escape bool, unk4 f32, choices: Vec<{button_id u32, unk2 u32, button_text_id u32, unk4 u32, unk5 u32}>, camera_placement Pos, look_at Pos, change_player_pos bool, new_player_pos Pos, unk8 f32, hide_players bool, unk10 bool, unk11 bool, zoom f32, speaker_sound_id u32 }`. Field order must be verified against FR (see open question 1). |
| 4 | `CommandPacketEndDialog` | s2c | no body. |
| 6 | `PacketDialogResponse` | c2s | `i32 response index` (-1 default). oxide calls it `AdvanceDialog { button_id u32 }`. |
| 8 | `CommandPacketInteractRequest` | c2s | `u64 guid, u8`. Sent when the player clicks an NPC / presses the interact key. **Already handled.** |
| 9 | `CommandPacketInteractionList` | s2c | `u64 guid, bool autoSelectIfSingle, List<InteractionData>, string contextName, bool isQuestObjective, bool isHidden` (oxide names; ours: `Unknown`, `Name`, `Unknown2`). `InteractionData`: `id, iconId, buttonTextId, type, param1, param2, tooltipId` (oxide adds `param3, sortOrder`; ours serialises 7 ints — check). **Already implemented for players.** |
| 10 | `CommandPacketInteractionSelect` | c2s | `u64 guid, i32 id`. **Already handled.** |
| 11 | `CommandPacketInteractionStartWheel` | s2c | radial menu. |
| 20 | `FreeInteractionNpc` | s2c | releases the NPC/player from an interaction lock (oxide `FreeInteractNpc`). |
| 22 | `CommandPacketMoveAndInteract` | c2s | client walks to target then interacts. |
| 28 | `CommandPacketPlayDialogEffect` | s2c | `3 dwords` — plays an effect/animation on the speaker during a dialog. |
| 18 | `CommandPacketSetChatBubbleColor` | c2s | player bubble colours. |

**BaseChatPacket, opcode 15** — what makes a bubble:

| sub | Name | Notes |
|---|---|---|
| 1 | `PacketChat` | channel + free text; `Npc.Say()` uses it with `ChatChannel.WorldSay` -> bubble + chat log. |
| 4 | `ChatPacketFromStringId` | `speakerGuid u64, stringId i32, isEmote, isChatLogged, hasColor, colorId, targetGuid, ownerGuid, elapsedTime`. `Npc.SayLocalized()` uses it. oxide sends the same thing (`SendStringId`) only to players within `CHAT_BUBBLE_VISIBLE_RADIUS`, confirming it renders as an over-head bubble. |

**BaseQuestPacket, opcode 49 (0x31)**, all s2c, sub-op as `i32`: 1 `QuestInfoPacket`, 2 `QuestReplyPacket`,
3 `QuestAddPacket`, 4 `QuestCompletePacket`, 5 `QuestFailedPacket`, 6 `QuestAbandonedPacket`,
7 `QuestObjectiveAddedPacket`, 8 `QuestObjectiveActivatedPacket`, 9 `QuestObjectiveUpdatePacket`,
10 `QuestObjectiveCompletePacket`, 11 `QuestObjectiveFailedPacket`, 12 `CompletedQuestCountUpdatePacket`;
cwa-research also lists `QuestEndPacket`, `QuestEndReplyPacket`, `QuestStartBreadcrumbPacket`. **No field
layouts are recovered anywhere** (catalog: `subop_verified`, no fields; decomp has no named deserialisers;
cwa-research pages are headings only). The quest-offer window (`wndQuestStart`) is almost certainly opened by
`QuestInfoPacket` (sub 1) carrying quest id, title/description/giver-dialogue string ids, rewards and the
response-button list (the Lua handler's `SetQuest`, `SetNPCDialog`, `SetRewardCoin/Exp`, `AddRewardItem`,
`AddDialogResponseNodes` map 1:1 onto such a payload). `CommandPacketQuestAbandon` (26/23) is the c2s side.
Neither Sanctuary nor oxide implements any op-49 packet.

**BaseUiPacket, opcode 47 (0x2f)**, `[i16 op][u8 sub]`: sub 7 `ExecuteScriptPacket { string script, List<int> params }`
(implemented in `src/Sanctuary.Packet/BaseUiPacket/ExecuteScriptPacket.cs`), plus `ExecuteScriptWithStringParams`,
`CinematicStartLookAt`, `SelectQuest`, `ObjectiveTargetUpdate`, `UiMessage`, `StartTimer`, `LoadingScreen`
(names from cwa-research; catalog confirms an inline sub-dispatcher with cases 1-16 that fire Lua/UI handlers,
e.g. "QuestHandler:NotifyNonmemberQuestLimitReached"). **ExecuteScript is the escape hatch**: it calls any global
client Lua function by name with int params, so anything `DialogHandler` / `QuestStartHandler` expose as
globals can be driven without the native packet — e.g. oxide sends `UIGlobal.DialogDisableInteraction` before
`EnterDialog` and `UIGlobal.DialogEnableInteraction` after `ExitDialog` "to prevent the UI from breaking".

**BasePlayerUpdatePacket 35 / AddNpc (sub 2)** already carries per-NPC `ChatBubbleForegroundColor`,
`ChatBubbleBackgroundColor`, `ChatBubbleSize`, `InteractRange`, `IsInteractable`, `ImageSetId`, `NameId`,
`SubTextNameId` — so bubble styling and clickability are per-NPC data we already send.

### B3. What that means for a "Bethesda-style" window

Everything needed exists client-side: a modal window with the speaker's line, up to N selectable responses
(5 in the legacy XML; the Flash version takes a list), an image slot, a camera move to frame the speaker
(`camera_placement`/`look_at`/`zoom` in ShowDialog, `SetDialogCameraOffset2D` in Lua), a speaker animation and
sound id, and a c2s response index. A "Next" is simply a dialog with one choice whose handler sends the next
`ShowDialog`. Voice = `speaker_sound_id` (a client sound-definition id) or `CommandPacketPlaySoundIdOnTarget`
(26/39); new lines would be `.mp3` overrides served by `tools/asset-server`.

## C. What Sanctuary already implements (our fork, branch `integrate-upstream`)

- **Clicking an NPC**: client sends `CommandPacketInteractRequest` (26/8) -> `CommandPacketInteractRequestHandler`
  -> `zone.TryGetEntity(guid)` -> `entity.OnInteract(player)`. For `CollectionNode` it runs the collection
  logic; for `Player` it sends a `CommandPacketInteractionList` (Inspect / Add-Remove Friend / Ignore / Guild
  invite); for **`Npc.OnInteract` it is an empty method** (`src/Sanctuary.Game/Entities/Npc.cs:98`).
- **Interaction menu**: `InteractionManager` auto-registers every `IInteraction` in the Game assembly by
  reflection (`Id = IInteraction.UniqueId++`, `IconId`, `ButtonText` string id). Selecting a button sends
  `CommandPacketInteractionSelect` (26/10) -> `interaction.OnInteract(player, target)` where target may be a
  Player *or an Npc* (the handler already looks in `VisibleNpcs`). So NPC-specific interactions slot straight in.
- **NPC speech**: `Npc.Say(string)` -> `PacketChat` WorldSay; `Npc.SayLocalized(int)` -> `ChatPacketFromStringId`;
  both broadcast to `VisiblePlayers`, i.e. **chat bubble + chat log only**. Exposed to Lua as `npc:say()`,
  `npc:sayLocalized()`, `npc:moveTo()`; scripts register `registerCallback("second"|"tick", fn)`
  (`src/Scripts/Npc/welcomer.lua` says string 8130 every 10-20 s).
- **Quest data (PR #109, merged)**: `src/Resources/Quests.json`, `QuestDefinition`, `QuestGoal` (types
  TalkToNpc / ReachLocation / Collect), `QuestDialogueLine { TextId, ResponseTextId }`, per-NPC
  `TargetDialogueIds` / `TargetResponseIds`, `GiverDialogueId`, rewards, `DbCharacterQuest` table,
  `QuestDefinitionCollection` (index by giver / goal target, `TryGetNpcInteractRange`). **No packets, no
  handlers, no UI** — the model anticipates exactly the "line + response caption" pair the client windows want.
- **PR #119 (closed prototype)**: an `ActionManager` with Instant/Timeout/Wait/Sequential/Parallel actions
  ticked per NPC, `npc:say` wrapped as `InstantAction`. Useful pattern for "walk over, turn, speak" sequences;
  nothing about dialog windows.
- **Not implemented**: `CommandPacketShowDialog`/`EndDialog`/`DialogResponse`/`PlayDialogEffect`,
  `FreeInteractionNpc`, every `BaseQuestPacket`, `SelectQuest`/`ObjectiveTargetUpdate` UI packets.
  `PacketReaderExtensions.cs` only names them for logging.

## D. Implementation sketch for our fork

### D1. Two rendering paths, pick per line

| Path | Client window | Packet | Use for |
|---|---|---|---|
| **Conversation** | `wndFlashDialog` (`dialog.swf`) | `CommandPacketShowDialog` 26/3 + `PacketDialogResponse` 26/6 + `EndDialog` 26/4 | Skyrim-style talk: line, 1-5 choices, Next, camera on the speaker, optional animation/sound. |
| **Quest offer / turn-in** | `wndQuestStart` / `wndQuestEnd` (`queststart.swf`) | `QuestInfoPacket` 49/1 (+ `QuestAddPacket` 49/3 on accept, `QuestCompletePacket` 49/4 on turn-in) | The last node of a conversation that offers or completes a quest: shows rewards and Accept/Decline. |
| **Bark** | over-head bubble | `ChatPacketFromStringId` 15/4 (exists) | Ambient one-liners, greetings while walking past. |

Do the Conversation path first; the ShowDialog layout has a working reference implementation (oxide) and the
window handles everything the owner asked for. The Quest window needs a packet layout nobody has recovered yet
(open question 2) — until then, "Accept quest" can be a conversation choice whose server handler adds the
quest and sends only the tracker/journal packets.

### D2. Packets to add (`src/Sanctuary.Packet/BaseCommandPacket/`)

- `CommandPacketShowDialog : BaseCommandPacket, ISerializablePacket` (sub 3) with the oxide `EnterDialog`
  field order as the first guess: `int DialogTextId; int SpeakerAnimationId; ulong SpeakerGuid; bool EnableEscape;
  float Unk4; List<DialogChoice> Choices; Vector4 CameraPlacement; Vector4 LookAt; bool ChangePlayerPos;
  Vector4 NewPlayerPos; float Unk8; bool HidePlayers; bool Unk10; bool Unk11; float Zoom; int SpeakerSoundId`.
  `DialogChoice { int ButtonId; int Unk2; int ButtonTextId; int Unk4; int Unk5 }`.
- `CommandPacketEndDialog` (sub 4, empty), `CommandPacketPlayDialogEffect` (sub 28, 3 ints),
  `FreeInteractionNpc` (sub 20, empty).
- `PacketDialogResponse : IDeserializable` (sub 6, `int ResponseIndex`) + `PacketDialogResponseHandler` in
  `src/Sanctuary.Gateway/Handlers/BaseCommandPacket/` and a case in `BaseCommandPacketHandler`.
- Optional: `ExecuteScriptPacket("UIGlobal.DialogDisableInteraction")` before / `...EnableInteraction` after,
  as oxide does; verify those globals exist in FR (`ScriptsBase.bin` has `DisableDialogClose` /
  `EnableDialogClose` / `SetInteractionEnabled` / `CloseAllDialogs`, which are the FR equivalents).

### D3. Data model — `src/Resources/Dialogues/<npc-or-topic>.json`

```json
{
  "id": "cobblestone/farmer_chug",
  "npcGuid": 100000002045,
  "camera": { "offset": [1.2, 1.6, 2.5], "lookAtHeight": 1.5, "zoom": 25 },
  "start": "greet",
  "nodes": {
    "greet": {
      "textId": 94246,                      // or "text": "literal" -> server-side custom string id (open q. 5)
      "animationId": 0, "soundId": 0,
      "choices": [
        { "textId": 1872662444, "next": "offer",  "if": "!hasQuest(2563)" },
        { "textId": 2651139138, "next": "rumor" },
        { "textId": 108431211,  "end": true }
      ]
    },
    "rumor": { "textId": 94388, "choices": [ { "textId": 2651139138, "next": "greet" } ] },
    "offer": { "quest": 2563, "textId": 94246 }   // -> quest window / acceptQuest action
  }
}
```

- `textId` = string id shown by the client; `choices[].textId` = button caption id. A choice with `next`
  sends another `ShowDialog`; `end` sends `EndDialog` + `FreeInteractionNpc`; `quest` hands over to the quest
  path. A node with exactly one choice is a "Next" page.
- Conditions/effects as small strings evaluated server-side (`hasQuest`, `questGoalDone`, `hasItem`, `level`,
  `flag`) and actions (`giveItem`, `giveCoins`, `startQuest`, `completeGoal`, `setFlag`, `lua:<fn>`).
- Load in `ResourceManager` next to `Quests.json`; index by `npcGuid`. `QuestGoal.ConversationFor(npcGuid)`
  (from PR #109) already yields `[ {TextId, ResponseTextId} ]` for talk-goals — render those as a single-choice
  chain, so quests need no separate authoring.

### D4. Server flow

1. `Npc.OnInteract(player)`: build a `CommandPacketInteractionList` for the NPC — button "Talk" (string id of
   "Talk"/"Speak", icon TBD) via a new `TalkInteraction : IInteraction`, plus vendor/trainer buttons later.
   Send with `autoSelectIfSingle = true` so one button opens the dialog immediately (oxide field name; our
   `List.Unknown`). If the Lua script for the NPC defines `onInteract`, fire that first and let it veto.
2. `TalkInteraction.OnInteract(player, npc)`: `DialogueRunner.Start(player, npc)` — pick the dialogue by
   `npcGuid` (quest-goal conversation takes priority over the idle tree), evaluate the start node, send
   `ShowDialog` with `SpeakerGuid = npc.Guid`, camera computed from the NPC's position + heading + `camera`
   offsets, `Choices` numbered 0..n-1 (`ButtonId = index`, `ButtonTextId = choice.textId`). Store the
   `(dialogueId, nodeKey)` on the `Player` as `ActiveDialogue`.
3. `PacketDialogResponseHandler`: look up `player.ActiveDialogue`, map `ResponseIndex` to the choice, run its
   effects, then `next` -> step 2 again, or `end` -> `EndDialog` + `FreeInteractionNpc` + clear. Any
   `InteractRequest`/teleport/zone change also clears it.
4. Quest path: the `offer` node calls `QuestManager.Offer(player, questId)`; until `QuestInfoPacket` is
   decoded, offer = a ShowDialog page whose Accept choice runs `startQuest`.

### D5. Lua hooks (`NpcUserData` / `ScriptContext`)

- New NPC events: `registerCallback("interact", fn(npc, player))` (return `true` to suppress the default
  Talk menu), `registerCallback("dialogueChoice", fn(npc, player, dialogueId, nodeKey, choiceIndex))`.
- New NPC methods: `npc:showDialog(player, { textId=..., choices={ {textId=..., id=1}, ... }, camera={...},
  animId=..., soundId=... })`, `npc:endDialog(player)`, `npc:playDialogEffect(player, a, b, c)`,
  `npc:startDialogue(player, "cobblestone/farmer_chug", "greet")`. JSON `lua:<fn>` actions call into the
  NPC's script with `(npc, player)`. Keep the JSON tree as the default; Lua is for branching that needs state.

### D6. Mapping onto what the client supports

| Owner wants | Client mechanism |
|---|---|
| Click NPC -> window opens | InteractRequest -> InteractionList(auto-select) -> ShowDialog; or skip the list and send ShowDialog straight from `Npc.OnInteract` (test both; oxide goes through the list). |
| NPC line in a window | `ShowDialog.DialogTextId` -> `DialogHandler.SetDialogText` (teletype not confirmed for this window; it is for `wndQuestStart`). |
| Selectable options | `ShowDialog.Choices[]` -> buttons; `PacketDialogResponse.ResponseIndex` back. Up to 5 safely (legacy XML); Flash version likely more. |
| Next | one choice; server sends next page. |
| Camera on speaker | `CameraPlacement` / `LookAt` / `Zoom` / `HidePlayers`. |
| Portrait | `bgImageSetId` / `imageId` in Lua; which ShowDialog field feeds them is unknown (open q. 1). Possibly `AddNpc.ImageSetId`. |
| Voice | `SpeakerSoundId` or `CommandPacketPlaySoundIdOnTarget`; author lines as `.mp3` overrides (roadmap already plans ElevenLabs). |
| Accept/Decline with rewards | `wndQuestStart` via `QuestInfoPacket` once decoded; interim: dialog choice. |

## E. Open questions

1. **Exact FR layout of `CommandPacketShowDialog` (26/3).** Only the CWA (oxide) layout is known; the catalog
   notes agree in shape ("dword@+0x10; i32@+0x14=-1; vec@+0x18; multiple Vec3 + flags"). Plan: implement the
   oxide order, send one to the real client, watch `wndFlashDialog` open; bisect fields if it doesn't. The
   decomp has no named deserialiser (ctor `0x00a99bd0`); a Ghidra look at that function would settle it and
   reveal which field carries the image set/portrait.
2. **`QuestInfoPacket` (49/1) and friends have no recovered field layouts** anywhere (catalog, decomp,
   cwa-research, oxide). Needed for the real quest-offer window with rewards. Sources to try: the 2010 pcaps in
   free-realms-re `captures/` (grep for op 49), or Ghidra on ctor `FUN_00c7be00` / dispatcher `FUN_00c7cd40`.
3. **Does `wndFlashDialog` type text teletype-style and show a portrait?** `setTextTeletypeTickTime` is only in
   `QuestStartHandler`; `dialog.swf` may render differently. Needs a live test.
4. **`queststart.swf` was not found by name in the local packs** (`dialog.swf`, `tutorial_dialog.swf` were).
   Check the streamed asset manifest (`asset-cache/_index.txt` has only 2 lines) — if it is missing from the
   community CDN the quest window will be blank, same class of problem as the placeholder DDS work.
5. **String-id spaces.** The wire uses small ids (8130, 3902, 94246...) while `en_us_data.dat` keys rows by a
   32-bit hash (1193909402 = "Welcome to Free Realms!"); the `.dir` maps hash -> offset. The voiced files are
   named with small ids (`FarmerChugDialog3331.mp3`), so the small id is the canonical one and the mapping
   lives in the client's string table loader (FreeRealmsLocaleTools handles both). Confirm how to add *new*
   lines: either ship a patched `en_us_data.dat/.dir` as a loose-file override, or send free text via
   `PacketChat`-style fields where the dialog packets accept strings (they appear not to; ShowDialog is id-based).
6. **Who plays the speaker sound?** `SpeakerSoundId` is a client sound-definition id; adding new VO means
   either reusing an existing definition id and overriding its `.mp3` on the asset server, or finding the
   sound-definition table format.
7. **Interaction lock.** oxide disables interaction during a dialog and sends `FreeInteractNpc` after; the FR
   Lua has `SetInteractionEnabled` / `DisableDialogClose`. Verify what happens if the player walks away or gets
   a second InteractRequest mid-dialog.

## F. Corrections from a working implementation (2026-09-11)

We adopted Sulphural's `quest-upstream-v2` branch (github.com/Sulphural/Sanctuary), which gets the dialogue and
quest UI right in the live client. What it taught us, versus our own first attempt:

- **Button art is data, not defaults.** Each `CommandPacketShowDialog` response is
  `{Id, ActionType, LabelTextId, Param1, Param2}`: `Param1` is the button's icon image id (check, X, return
  arrow, plus) and `Param2` is the button background image set (e.g. the green button). We sent zeros there and
  the client drew its "OOPS" placeholder banner.
- **Let the client frame the camera.** They send zero camera vectors and put a focus parameter (1.0) in the float
  after the escape flag; the client frames the speaker itself. Tune that parameter, not hand-built camera
  positions, if the shot is too tight.
- **Quest offers are not dialogue.** Offers and turn-ins use `QuestInfoPacket` (opcode 49, sub 1): the quest
  card, "Show Details", rewards, and the green accept / tan decline buttons. `ShowDialog` is for the lines in
  between ("You got it!").
- **The NPC talks.** A talk animation plays on the speaker while a line is shown, and stops after.
- **Markers.** Overhead quest icons come from `PlayerUpdatePacketAddNotifications` / `RemoveNotifications`
  (image set per quest state), refreshed per player on every quest state change; map and minimap tracking comes
  from `ObjectiveTargetUpdatePacket`; the tracker panel from the quest add / objective packets and `SelectQuest`.
- **Method.** Recover a packet's layout from the client decompilation and the 2010 captures, test it live,
  iterate; keep content in data (`Quests.json`, authoring guide in `src/Resources/QUESTS.md`).
