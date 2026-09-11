# Character creation: swatch images and tints

Audit of every icon the character creator uses, done 2026-09-11 after Carlos reported grey eye-colour swatches,
white hair-style icons and skin-tone swatches that look nothing like the skin they pick.

## How a swatch is drawn

Each row of `client/Resources/CharacterCreateCustomization*.txt` names an **image set** (`ICON_ID`) and a **tint**
(`ICON_TINT`). An image set resolves through `Resources/Images/ImageSetMappings.txt` (type 5 = 32 px, 6 = 64 px,
7 = 128 px, 8/9/10 = the matching tint masks) to file names in `Images/Images.txt`. The client draws the base
image and tints the area the mask marks, using the colour from `Resources/Tints.xml`.

**A missing mask means no tint at all**, which is why an untinted icon looks grey or white rather than wrong-coloured.

## What the audit found

92 image sets are used; 12 are missing a file. Every one is missing from the **community streaming server**: the
file is listed in its manifest but served from no directory (the same failure as the housing furniture). The
client cannot fetch them, so this is how character creation looks for anyone on the community servers.

| Image set | Missing file | Effect |
|---|---|---|
| 6495 | `icon_create_eye_color_bg_mask_64.dds` | all 40 eye-colour swatches grey |
| 6528, 6532, 6534, 6535, 6536, 6538, 6542, 6543, 6546, 6547, 6548 | `icon_create_hair_*_fg_mask_64.dds` (11 files) | those hair styles' icons ignore the hair colour and render white |
| 6576 | `icon_create_hair_hm_bald_64.dds` (the icon itself) | bald style has no icon |

`tools/client-fixes/place_local_assets.py` fixes all twelve: it tries the streaming server first, and otherwise
scales the game's own 32 px version of the same file to 64 px, saving it as DXT5 like the original. Results go
into `client/` as loose files (which override packed assets) and into `asset-cache/_overrides/`. Replace them with
the real files if any turn up in another client dump — worth asking the OSFR Discord.

## Skin-tone swatches: a quirk in Sony's data

Not a missing file. `CharacterCreateCustomizationSkinTone.txt` uses the `ICON_TINT` column for the **skin-tone
number** (1-6), not a colour. The client tints the swatch with it anyway, so the six tones land on `Tints.xml`
ids 1-6: `solidWhite`, then `human_2`..`human_6`, a gold ramp (253,223,132 → 200,151,33). The character's own skin
is unaffected: it comes from a texture chosen through `CharacterCreate/SkinToneMappings.txt` (`skintone1`..`8`),
which is not a standalone file, so the true colours can't be sampled from the client.

This is original behaviour, but it makes the swatches useless for picking a tone, so the same tool applies our
one **deliberate cosmetic change** to the original data: six new tint entries (ids 900-905, a skin ramp from
Ivory to Mahogany) and the swatch rows pointed at them. The original table is kept as
`CharacterCreateCustomizationSkinTone.txt.orig`; delete the tweak from the tool and restore that file to revert.

## Still open

- **Custom names.** The game supported them with moderator approval ("Enter a CUSTOM Name and/or NAME WHEEL Name",
  "Your custom name needs to be approved before it can be used"), and both our client's name screen
  (`characterNameScreen.gfx`: `CustomNameWheel`, `isCustomNum`, `setCustomNameOnly_lua`) and our server
  (`CharacterData.FirstName`/`TemporaryFirstName`, `PacketCheckNameRequest`) support the flow. In game there is no
  visible way to type one. The name screen's Lua only calls `SetCustomNameOnly` in the China environment. Next step
  is decompiling `characterNameScreen.gfx`'s ActionScript to see what gates the custom-name entry.
- **Arrow hold-to-repeat.** The same Flash file has `handleButtonRepeat` / `startButtonRepeat` /
  `buttonRepeatSpeed`, yet holding an arrow does not scroll in game. Same investigation.
- **Mouse wheel** on the name lists: nothing in the Flash suggests wheel support; it would be a new feature.
