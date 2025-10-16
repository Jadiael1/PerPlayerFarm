# Per Player Farm (PPF)

> 🌍 README in other languages: [Português (BR)](README.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **A world of parallel farms, one for each player.**
>
> Compatible with **Stardew Valley 1.6.15** and **SMAPI 4.3.2**.

---

## ✨ Overview

**Per Player Farm (PPF)** creates a *separate farm per player* within the same save. Every farmhand receives their own location of type `Farm`, named `PPF_<UniqueMultiplayerID>`, with initial cleanup, a cabin facade, and a fast travel flow between the **main farm** ↔ **PPF** via an **exclusive teleporter**.

Everything is designed for multiplayer (host authoritative) and also works in split-screen sessions and reconnects.

---

## 📦 Requirements

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (to build from source)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) for automatic deploy/zip

---

## 🧭 How it works (high level)

* **PPF creation**

  * Host: when loading/creating the world, the mod creates/ensures all known `PPF_*` (and registers new ones when players join). The `PPF_*` are **real `Farm` instances** (not `GameLocation`), so vanilla farm-only items can be placed.
  * Client: creates local *stubs* (also `Farm`) to keep warps, UI, and travel functional even without the host.

* **Maps/Assets**

  * `Maps/PPF_*` starts as a *clone* of `Maps/Farm` with tweaks: `CanBuildHere = T`, no `MailboxLocation`, and no undesired actions/warps.
  * `Data/Buildings` receives the **facade** `PPF_CabinFacade` with door tiles (`Action: PPF_Door`) and a mailbox using the vanilla action.
  * `Data/BigCraftables` injects the **exclusive teleporter** `(BC)DerexSV.PPF_Teleporter`, visually based on the Mini-Obelisk (customizable later).

* **Travel**

  * Interacting with the **PPF teleporter** opens a **travel menu** listing “Host Farm” plus one entry per `PPF_*` (showing the owner’s online status).
  * The facade door (`PPF_Door`) sends the owner straight to the interior of their home (FarmHouse), preserving vanilla behavior.

* **Teleporter: smart placement**

  * Attempts the **preferred position** first (defined via `Teleporter.PreferredTileX/PreferredTileY` in the config). If blocked, it searches for a valid tile (spiral + map scan) using the official `GameLocation.CanItemBePlacedHere` rule.
  * The teleporter **remains useful** when removed and placed again: events watch for placement and **retag** the object automatically.

* **Cleanup & stripping vanilla structures**

  * *Initial light cleanup* in `PPF_*`: removes weeds/stones/branches, trees/grass, and resource clumps.
  * *Continuous stripping* in `PPF_*`: **always removes** the **Farmhouse**; **removes the Greenhouse only if broken** (not unlocked); **never touches** the **Shipping Bin** or **Pet Bowl**. When removing the broken greenhouse, it also clears warps to `Greenhouse`.

* **Warp synchronization**

  * The event utilities (formerly `PpfWarpHelper`) synchronize **Cabin → PPF** connections (the cabin door leads into the owner’s PPF and the exit goes to the matching facade).

* **Persistence**

  * Owner/PPF mappings are stored in `ppf.locations` (mod save-data). Recreating is idempotent.

---

## 🧠 Motivation & design decisions

Stardew Valley was originally built for solo play; even with multiplayer, the standard setup (one shared farm, few cabins, optional split money) is tight for players who want independent progress. The goal here is to give each guest a full-fledged farm without breaking vanilla flows (cabins, Robin, base save).

### Early experiments and drawbacks

The first prototype envisioned a cycle with *templates* (`PPF_Template`) managed by the host:

1. The save starts with a `PPF_Template`.
2. The host would ask Robin to build a cabin on this template.
3. When a guest joined, the `PPF_Template` became `PPF_<INVITER_UID>`.
4. The host got another `PPF_Template` for future guests, repeating the process.

But this flow presented issues:

* **Robin integration:** Extending the vanilla carpenter menu to expose custom maps and allow building outside the main farm conflicts with other mods and requires deep UI hooks, tile validation, and cost handling.
* **Fragile persistence:** Moving the guest’s cabin to a totally custom location means that disabling the mod leaves the game without a reference to the guest’s home; they’d lose a valid spawn/bed.
* **Save compatibility:** Dynamically converting `GameLocation` ↔ `Farm` per guest risks corruption (lost items, homeless animals, broken quests).
* **Managing multiple templates:** Ensuring an always-available `PPF_Template`, promoting unique names, cleaning leftovers—prone to errors, especially with many guests joining/leaving.

### Final architecture

To avoid these pitfalls, guest cabins stay on the main farm while the PPF acts as a **parallel farm**:

* Each player gets a `PPF_<UID>` location (real `Farm`), but their vanilla cabin remains on the main farm. If the mod is disabled, the cabin and spawn remain valid.
* A **facade cabin** is added to the PPF; the door has a custom action warping to the real cabin interior. Mailbox tiles mirror vanilla (including new-mail animation).
* A **custom obelisk** (`(BC)DerexSV.PPF_Teleporter`) appears on all farms, providing menu-based travel between the main farm and every PPF, showing availability and online status.
* Farm entry warps (Bus Stop, Forest, Backwoods, etc.) are synchronized so each player lands in their `PPF_*`, while the main farm remains accessible.

This architecture keeps compatibility with the base game, avoids issues if the mod is removed, and still delivers the personal space that inspired the project.

---

## 🕹️ How to use (players)

1. **Host** loads the save normally. Known PPFs are created/ensured.
2. On the **main farm** or a **PPF**, interact with the PPF teleporter to open the travel menu.
3. The **facade door** teleports the owner into the interior of their real cabin (FarmHouse).
4. The floating **mail icon** appears above the PPF mailbox when mail is waiting (only for the owner).

> Tip: If the preferred teleporter spot is blocked, the mod automatically relocates it to a nearby valid tile.

---

## ⌨️ SMAPI console commands

> Only the **host** can change the world. Run commands in the SMAPI console with the game loaded.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: guarantees teleporters in **all** `PPF_*` and the **main farm**.
  * **here**: guarantees only in the current location.
  * **farm**: guarantees only on the main farm.
  * **ppf**: guarantees only in all `PPF_*`.
  * **<LocationName>**: guarantees specifically in the named location.

* `ppf.clean here|all|ppf`

  * Performs light cleanup of debris/grass/trees/resource clumps.

* `ppf.strip here|all`

  * Removes **Farmhouse** always and **Greenhouse if broken** in `PPF_*` (keeps Shipping Bin/Pet Bowl). Removes warps to `Greenhouse` when the broken greenhouse is gone.

---

## ⚙️ Configuration & customization

* **Teleporter anchor:** adjust `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` in `config.json`. Missing/invalid values fall back to 74,15 and clamp to the map.
* **Teleporter visuals:** change `Texture`/`SpriteIndex` in the `Data/BigCraftables` block inside `AssetRequested`.
* **Cleanup:** use `ppf.clean` (console) to reapply light cleanup on PPFs.

---

## 🧩 Compatibility

* Designed for **1.6.15** / **SMAPI 4.3.2**.
* Host is **authoritative**: clients/stubs do not make permanent changes.
* Mods editing the **same assets** (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`) may require load-order adjustments. The mod uses prioritized `AssetRequested` handlers and aims to stay idempotent.

---

## 🛠️ Development

### Quick build

1. Install **.NET 6 SDK**.
2. Ensure the **Pathoschild.Stardew.ModBuildConfig** package is referenced.
3. In `PerPlayerFarm.csproj`, enable:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (If needed) create `stardewvalley.targets` in your user directory with `GamePath`/`GameModsPath`.
5. Build:

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig copies the mod to `…/Stardew Valley/Mods/PerPlayerFarm` and can produce a `.zip`.

### Code structure

* **ModEntry**: registers handlers and initializes manager classes.
* **Events/**

  * `AssetRequested/*`: inject/edit `Maps/*`, `Data/Buildings`, `Data/BigCraftables`, and mark custom warps.
  * `ButtonPressed/*`: handles teleporter interaction, facade door, travel menu.
  * `DayStarted/*`: performs cleanup, teleporter assurance, warp adjustments at day start.
  * `LoadStageChanged/*`: ensures known PPFs when the save loads (using persisted data/known farmers).
  * `ModMessageReceived`: syncs the PPF registry via SMAPI messages.
  * `ObjectListChanged`: retags manual Mini-Obelisk placements to keep teleports functional.
  * `PeerConnected/*`: host creates/ensures PPF assets for new players and updates house warps.
  * `RenderedWorld` / `RenderingWorld`: manage and draw the mailbox UI overlay for the owner’s facade.
  * `ReturnedToTitle`: clears client caches when returning to the main menu.
  * `SaveLoaded/*`: loads the guest’s PPF (client) and strips vanilla structures in host PPFs.
  * `Saving`: persists `ppf.locations` with known UIDs.
  * `TouchAction`: handles custom touch-action warps so each player lands in their `PPF_*`.
* **Utils/**

  * `Constants`: shared keys/modData names.
  * `ListHelper`: warp-string parsing/serialization.
  * `MailboxState`: temporary state used during Rendering/RenderedWorld.
  * `PpfConsoleCommands`: console commands `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: data models for multiplayer/persistence.
* **Contents/**

  * `Buildings/LogCabin.cs`: facade building (`PPF_CabinFacade`).
  * `Itens/PPFTeleporter.cs`: exclusive teleporter `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * Localized log strings in `*.json`.
* **Configuration/**

  * `ModConfig`: options loaded from `config.json`.

### Key reuse points

Some helpers are used across multiple flows; modifications should consider every caller:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): ensures teleporters both on day start and via command.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): repoints default cabin warps to the owner’s PPF.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): ensures the host has the guest PPF infrastructure.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): keeps the UID ↔ PPF mapping persisted.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): loads the “shadow” PPF for clients.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): removes vanilla buildings continuously from PPFs.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): initial cleanup reused by `ppf.clean`.
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/TouchAction`): deserializes injected warp strings.
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`): determines owner and relevant tiles for interactions/overlays.

---

## 🧯 Troubleshooting

* **“The teleporter didn’t appear.”**

  * Run `ppf.ensure-teleports here` in the current location; check the SMAPI console for `[PPF]` logs about placement attempts.
* **“Clicking teleporter opened the vanilla one.”**

  * Make sure it’s the **PPF teleporter** (exclusive item), not the vanilla Mini-Obelisk.
* **“I can’t place the vanilla Mini-Obelisk on the PPF.”**

  * PPFs must be `Farm` locations. This mod already creates `PPF_*` as `Farm`. If you migrated from an older version (with `GameLocation`), remove/re-add the mod to rebuild the locations, or convert manually.

---

## 🤝 Contributions

Contributions are **welcome** (issues, suggestions, patches, PRs)! To stay aligned:

* Before building a large feature, **open an issue** outlining the idea and wait for feedback.
* Agree on scope in the issue, then submit the PR referencing it—this avoids rework.
* For small fixes (docs/bugs), direct PRs are fine, but it’s still recommended to mention the related issue.
* Code style: C# (.NET 6, nullable enabled), `[PPF]` logs with Trace/Info/Warn, idempotent logic, host-authoritative multiplayer.

> **Important (licensing):** This mod is **not open-source**. **Forks or modified distributions are not permitted** without the author’s permission. Pull requests and patches are welcome here, subject to review.

---

## 📜 License

**All rights reserved.**

* You may **install and play** the mod.
* You may **propose improvements** via issues/PRs in this official repository.
* You **may not** publish forks, create derivative mods, or redistribute modified versions without explicit consent from the author.

---

## 🙌 Credits

* **Author:** DerexSV
* **Frameworks:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **Community:** thanks to testers and the Stardew Valley modding community.

---

## 📫 Support

Open an issue with SMAPI logs and a scenario description (host/client, location, reproduction steps). Provide short repro cases with other mods disabled whenever possible.
