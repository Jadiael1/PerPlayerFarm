# Per Player Farm

**Nexus page:** https://www.nexusmods.com/stardewvalley/mods/38441

If you want to understand more about what inspired me and the workflow for building this MOD, read here:
https://jadiael.dev/blog/3

Per Player Farm creates a separate farm space for each farmhand in multiplayer.

Instead of everyone sharing only the main Farm, each invited player can have a personal `PPF_*` farm, with proper entrance redirection and owner-aware mailbox behavior. This keeps multiplayer progression organized while preserving a familiar gameplay flow.

---

## Compatibility

- **Stardew Valley:** `1.6.15`
- **SMAPI:** `4.4.0`

---

## Requirements

- SMAPI 4.4.0+
- Stardew Valley 1.6.15
- Generic Mod Config Menu (optional, recommended)

---

## Installation

1. Install **SMAPI 4.4.0+**
2. Extract this mod into your `Mods` folder

---

## Load Order / Recommended Setup

- Install the same version of this mod for host and all farmhands.
- Keep host and farmhand config values synchronized.
- If you change the configuration values, all players must have the same settings and everyone must restart the game before continuing in multiplayer mode.

---

## Multiplayer Requirements (Important)

For consistent multiplayer behavior:

- The host and all farmhands should install this mod
- The host and farmhands should use the **same config values**

Different config values may cause inconsistent warps/entrances and unexpected behavior between players.

---

## Configuration

- `WarpToPPFOnHouseExit` (default: `true`)  
  Redirects farmhands to their own PPF farm when leaving their cabin.

- `RedirectFarmhandsToPPF` (default: `true`)  
  Redirects farm entrances so farmhands go to their assigned PPF farm.

- `OpenTravelMenuKey` (default: `K`)  
  Key used to open the travel menu.

- `DefaultPPFMapType` (default: `null`)  
  Optional override for the default map used when creating PPF farms.  
  Leave empty/null to keep normal behavior.

  Examples:  
  `Maps/Farm`, `Maps/Farm_Fishing`, `Maps/Farm_Foraging`, `Maps/Farm_Mining`, `Maps/Farm_Combat`, `Maps/Farm_FourCorners`, `Maps/Farm_Island`, `Maps/Farm_Ranching`

### Travel Menu Key Behavior

- Default key: **`K`**
- The travel menu can only be opened while in:
  - the main `Farm`, or
  - a `PPF_*` farm

---

## In-Game Chat Commands

Open chat with **`T`**, then type:

- `/ppfclearfarm`  
  Clears placed objects and selected terrain features in the current PPF farm.  
  - Guest players: only on **their own** assigned PPF farm  
  - Host player: can use it on any PPF farm

- `/ppfclearfarm all`  
  **Host only.** Clears all loaded PPF farms.

- `/ppfuninstall`  
  **Host only.** Starts the uninstall confirmation flow (2-step).

- `/ppfuninstall confirm`  
  **Host only.** Confirms uninstall and removes PPF data created by this mod.

---

## Before Uninstalling

Before confirming uninstall, move important items/animals from PPF farms back to the main Farm.

After uninstall:

1. Save the game
2. Close the game
3. Remove the mod from your `Mods` folder

---

## Known Limitations

- Host and farmhands should keep the same config values.
- Different config values between players may cause inconsistent entrance/warp behavior.

---

## Recommended Companion Mod

To move buildings and animals between farms, use:

- https://www.nexusmods.com/stardewvalley/mods/20606  
  (**Relocate Buildings And Farm Animals**)

---

## Translations

This mod currently includes:

- English (`default.json`)
- Portuguese (Brazil) (`pt.json`)

You can add new languages by creating additional i18n files.

---

## FAQ

**Q: Is this mod for single-player?**  
A: It is designed for multiplayer use.

**Q: Do all players need the same config?**  
A: Yes. Keep host/farmhand config aligned for consistent behavior.

**Q: Why can’t a guest clear another player’s PPF farm?**  
A: By design, guests can only clear their own assigned PPF farm.

---

## Reporting Issues

If you find a bug, please open an issue here:  
https://github.com/Jadiael1/PerPlayerFarm/issues or here https://www.nexusmods.com/stardewvalley/mods/38441?tab=posts

Please include:

- Stardew Valley version
- SMAPI version
- Per Player Farm version
- Multiplayer role (host or farmhand)
- Steps to reproduce
- SMAPI log: https://smapi.io/log

---

## Source Code

- https://github.com/Jadiael1/PerPlayerFarm

## Support / Donate

- https://jadiael.dev/donate