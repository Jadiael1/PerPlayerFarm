# Per Player Farm (PPF)

> 🌍 README in anderen Sprachen: [Português (BR)](README.md) · [English](README.en.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **Eine Welt aus parallelen Farmen – eine für jeden Spieler.**
>
> Kompatibel mit **Stardew Valley 1.6.15** und **SMAPI 4.3.2**.

---

## ✨ Überblick

**Per Player Farm (PPF)** erstellt eine *eigene Farm pro Spieler* im selben Spielstand. Jeder Farmer (Farmhelfer) erhält eine eigene Location vom Typ `Farm`, genannt `PPF_<UniqueMultiplayerID>`, die bereits gesäubert ist, eine Fassadenhütte besitzt und über einen **exklusiven Teleporter** einen schnellen Wechsel zwischen **Hauptfarm** ↔ **PPF** ermöglicht.

Alles ist für Multiplayer (Host als Autorität) ausgelegt und funktioniert ebenso im Split Screen sowie bei erneuten Verbindungen.

---

## 📦 Voraussetzungen

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (zum Kompilieren aus dem Quellcode)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) für automatischen Deploy/Zip

---

## 🧭 Funktionsweise (High Level)

* **Erstellung der PPF**

  * Host: Beim Laden/Erstellen des Spielstands sorgt der Mod für alle bekannten `PPF_*` (und registriert neue, sobald jemand beitritt). Die `PPF_*` sind **echte `Farm`-Maps** (keine `GameLocation`), wodurch Vanilla-Objekte, die „nur auf Farmen“ platziert werden dürfen, funktionieren.
  * Client: Erstellt lokale *Stubs* (ebenfalls `Farm`), damit Warps, UI und Reisen auch ohne Host verfügbar sind.

* **Karten/Assets**

  * `Maps/PPF_*` basiert als *Kopie* auf `Maps/Farm` mit Anpassungen: `CanBuildHere = T`, ohne `MailboxLocation` und ohne unerwünschte Actions/Warps.
  * `Data/Buildings` erhält die **Fassade** `PPF_CabinFacade` mit Tür-Tile (`Action: PPF_Door`) und einem Briefkasten mit Standard-Action.
  * `Data/BigCraftables` injiziert den **exklusiven Teleporter** `(BC)DerexSV.PPF_Teleporter`, optisch auf Basis des Mini-Obelisken (kann jederzeit angepasst werden).

* **Reisen**

  * Bei Interaktion mit dem **PPF-Teleporter** öffnet sich ein **Reisemenü**, das „Farm des Hosts“ plus einen Eintrag pro `PPF_*` anzeigt (inkl. Online-Status des Besitzers).
  * Die Tür der Fassade (`PPF_Door`) bringt den Besitzer direkt ins *Innere* seiner Hütte (FarmHouse) – das Vanilla-Verhalten bleibt erhalten.

* **Teleporter: intelligente Platzierung**

  * Zuerst wird die **bevorzugte Position** versucht (in der Config via `Teleporter.PreferredTileX/PreferredTileY` definiert). Falls blockiert, sucht der Mod nach einem gültigen Tile (Spirale + Map-Scan) mit der offiziellen Regel `GameLocation.CanItemBePlacedHere`.
  * Der Teleporter **bleibt funktionsfähig**, wenn er entfernt und erneut platziert wird: Events beobachten das Platzieren und **taggen** das Objekt wieder.

* **Säuberung und Entfernen von Vanilla-Strukturen**

  * *Initiale Leichtsäuberung* in `PPF_*`: entfernt Unkraut/Steine/Äste, Bäume/Gras und Resource Clumps.
  * *Kontinuierliches Entfernen* in `PPF_*`: **entfernt immer** das **Farmhouse**; **entfernt das Gewächshaus nur, wenn es zerstört** ist (nicht freigeschaltet); **belässt** **Versandkiste** und **Tiernapf**. Beim Entfernen des zerstörten Gewächshauses werden Warps zur `Greenhouse` in der PPF ebenfalls gelöscht.

* **Warp-Synchronisation**

  * In den Events umgesetzt – sorgt dafür, dass die Verbindung **Cabin → PPF** synchron bleibt (die Tür der Cabin führt in die PPF des Besitzers und zurück zur entsprechenden Fassade).

* **Persistenz**

  * Die Liste der Besitzer/PPFs wird in `ppf.locations` (Save-Data des Mods) gespeichert. Die Neuerstellung ist idempotent.

---

## 🧠 Motivation & Design-Entscheidungen

Stardew Valley wurde primär für Solo-Spiel konzipiert; selbst mit Multiplayer ist die Standardstruktur (eine gemeinsame Farm, wenige Cabins, optional getrenntes Geld) eng für Spieler*innen, die unabhängig vorankommen möchten. Ziel dieses Mods ist es, jedem Gast eine vollwertige Farm zu geben, ohne die Vanilla-Flows (Cabins, Robin, Save) zu brechen.

### Erste Experimente und Gegenargumente

Der erste Prototyp sah einen Zyklus mit *Templates* (`PPF_Template`) vor, die der Host verwaltet:

1. Der Save startet mit einer `PPF_Template`.
2. Der Host baut über Robin eine Cabin in dieser Template.
3. Sobald ein Gast joint, wird die `PPF_Template` zu `PPF_<INVITER_UID>`.
4. Der Host erhält eine neue `PPF_Template` für zukünftige Gäste, der Prozess wiederholt sich.

Dieser Flow brachte allerdings Probleme:

* **Integration mit Robin:** Das Vanilla-Menü so zu erweitern, dass Custom-Maps angezeigt werden und außerhalb der Hauptfarm gebaut werden kann, kollidiert mit anderen Mods und erfordert tiefe UI-Hooks, Tile-Validierung und Kostenlogik.
* **Fragile Persistenz:** Die Hütte eines Gastes auf eine komplett eigene Location zu verschieben führt dazu, dass bei deaktiviertem Mod die Referenz zum Zuhause fehlt – der Gast könnte ohne gültigen Spawn/Couch bleiben.
* **Kompatibilität mit bestehenden Saves:** Das dynamische Konvertieren von `GameLocation` ↔ `Farm` für jeden Gast birgt Risiken (verlorene Items, Tiere ohne Zuhause, kaputte Quests).
* **Verwaltung mehrerer Templates:** Sicherstellen, dass immer eine freie `PPF_Template` existiert, eindeutige Namen vergeben, Reste aufräumen – alles fehleranfällig, besonders bei vielen Gästen, die häufig joinen/quitten.

### Finale Architektur

Um diese Risiken zu vermeiden, bleibt die Cabin in der Hauptfarm und die PPF ist die **parallele Farm**:

* Jeder Spieler erhält eine Location `PPF_<UID>` (echte `Farm`), doch die Vanilla-Cabin verbleibt auf der Hauptfarm. Wird der Mod deaktiviert, existieren Cabin und Spawn weiterhin.
* Eine **Fassade (Log Cabin)** wird in der PPF platziert; die Tür führt mit einer Custom-Action zum Inneren der echten Cabin. Der Briefkasten verhält sich wie im Original (inkl. Animation bei neuer Post).
* Ein **eigener Obelisk** (`(BC)DerexSV.PPF_Teleporter`) erscheint auf allen Farmen. Er verwaltet Reisen zwischen Hauptfarm und PPFs via Menü und zeigt Verfügbarkeit/Online-Status.
* Warps der Farm-Eingänge (Bus Stop, Forest, Backwoods etc.) werden so synchronisiert, dass jeder Spieler in seine `PPF_*` gelangt, die Hauptfarm aber jederzeit erreichbar bleibt.

Diese Architektur bleibt kompatibel mit dem Grundspiel, verhindert Probleme bei deaktiviertem Mod und liefert dennoch den zusätzlichen Raum, der motiviert hat.

---

## 🕹️ Nutzung (Spieler)

1. **Host** lädt den Save. PPFs bekannter Spieler werden erstellt/gesichert.
2. Auf der **Hauptfarm** oder einer **PPF** mit dem Teleporter interagieren; das Menü listet alle Farmen und ob der Besitzer online ist.
3. Die **Fassadentür** teleportiert den Besitzer in das Innere seiner echten Cabin (FarmHouse).
4. Das schwebende **Briefsymbol** erscheint über dem Briefkasten der PPF, wenn Post vorhanden ist (nur für den Besitzer).

> Tipp: Falls der Teleporter an der bevorzugten Position blockiert ist, wählt der Mod automatisch ein passendes Feld in der Nähe.

---

## ⌨️ SMAPI-Konsolenbefehle

> Nur der **Host** kann die Welt verändern. Befehle im SMAPI-Konsolenfenster mit geladenem Spiel ausführen.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: garantiert Teleporter in **allen** `PPF_*` sowie auf der **Hauptfarm**.
  * **here**: nur in der aktuellen Location.
  * **farm**: nur auf der Hauptfarm.
  * **ppf**: nur in allen `PPF_*`.
  * **<LocationName>**: exakt genannter Bereich.

* `ppf.clean here|all|ppf`

  * Entfernt leichte Verschmutzung/Unkraut/Bäume/Ressourcen.

* `ppf.strip here|all`

  * Entfernt **Farmhouse** immer und **Greenhouse nur, wenn kaputt**; Versandkiste und Tiernapf bleiben. Warps zur `Greenhouse` werden gelöscht, wenn das defekte Gewächshaus entfernt wird.

---

## ⚙️ Konfiguration & Personalisierung

* **Teleporter-Anker**: `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` in `config.json` einstellen. Bei fehlenden/ungültigen Werten greift der Standard (74,15) und wird auf die Mapmaße geclamped.
* **Teleporter-Optik**: Im Block `Data/BigCraftables` von `AssetRequested` `Texture`/`SpriteIndex` ändern.
* **Säuberung**: Mit `ppf.clean` (Konsole) PPFs erneut säubern.

---

## 🧩 Kompatibilität

* Entwickelt für **1.6.15** / **SMAPI 4.3.2**.
* Host ist **Autorität**: Clients/Stubs führen keine permanenten Änderungen aus.
* Mods, die **dieselben Assets** verändern (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`), benötigen eventuell eine abgestimmte Lade-Reihenfolge. Der Mod nutzt `AssetRequested` mit Prioritäten und bemüht sich um Idempotenz.

---

## 🛠️ Entwicklung

### Schneller Build

1. **.NET 6 SDK** installieren.
2. Paket **Pathoschild.Stardew.ModBuildConfig** ins Projekt aufnehmen.
3. In `PerPlayerFarm.csproj` aktivieren:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Bei Bedarf) `stardewvalley.targets` im Benutzerverzeichnis erstellen und `GamePath`/`GameModsPath` setzen.
5. Kompilieren:

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig kopiert anschließend nach `…/Stardew Valley/Mods/PerPlayerFarm` und kann ein `.zip` erzeugen.

### Code-Struktur

* **ModEntry**: registriert Handler und initialisiert Manager.
* **Events/**

  * `AssetRequested/*`: injiziert/bearbeitet `Maps/*`, `Data/Buildings`, `Data/BigCraftables`, markiert angepasste Warps.
  * `ButtonPressed/*`: verarbeitet Interaktionen mit Teleporter, Fassadentür und Reisemenü.
  * `DayStarted/*`: Reinigung, Teleporter-Garantie, Warp-Anpassungen zum Tagesbeginn.
  * `LoadStageChanged/*`: stellt bekannte PPFs beim Save-Laden sicher (Persistenz & bekannte Farmer).
  * `ModMessageReceived`: synchronisiert das PPF-Register per SMAPI-Nachrichten.
  * `ObjectListChanged`: retaggt manuell platzierte Mini-Obelisken.
  * `PeerConnected/*`: Host richtet Ressourcen für neue Spieler ein und aktualisiert Haus-Warps.
  * `RenderedWorld` / `RenderingWorld`: verwalten/zeichnen das Briefsymbol über der Fassadenhütte.
  * `ReturnedToTitle`: leert Client-Caches beim Zurückkehren ins Hauptmenü.
  * `SaveLoaded/*`: lädt PPFs für eingeladene Clients und entfernt Vanilla-Bauten auf Host-PPFs.
  * `Saving`: speichert `ppf.locations` mit der UID-Liste.
  * `TouchAction`: verarbeitet benutzerdefinierte Touch-Actions, damit jeder Spieler in seine `PPF_*` gelangt.
* **Utils/**

  * `Constants`: gemeinsame Keys/ModData.
  * `ListHelper`: (De-)Serialisierung der Warp-Strings.
  * `MailboxState`: temporärer Zustand für Rendering/RenderedWorld.
  * `PpfConsoleCommands`: Konsolenbefehle `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: Datenmodelle für Multiplayer/Persistenz.
* **Contents/**

  * `Buildings/LogCabin.cs`: Fassaden-Gebäude `PPF_CabinFacade`.
  * `Itens/PPFTeleporter.cs`: exklusiver Teleporter `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * Lokalisierte Nachrichten in `*.json`.
* **Configuration/**

  * `ModConfig`: Optionen aus `config.json`.

### Wichtige Wiederverwendungen

Einige Helfer werden mehrfach genutzt; Änderungen sollten alle Aufrufe berücksichtigen:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): stellt Teleporter sowohl im Tageszyklus als auch per Kommando sicher.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): ersetzt Standard-Hauswarps durch PPF-Ausgänge.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): sorgt dafür, dass Hosts PPFs der Gäste bereitstellen.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): speichert UID ↔ PPF im Save.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): lädt PPF-Stubs für Clients.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): entfernt Vanilla-Gebäude kontinuierlich aus PPFs.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): initiale Säuberung, auch per `ppf.clean`.
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/TouchAction`): deserialisiert injizierte Warps.
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`): bestimmt Besitzer/Tiles für Interaktion und Overlay.

---

## 🧯 Fehlerbehebung

* **„Der Teleporter ist nicht erschienen.“**

  * `ppf.ensure-teleports here` in der aktuellen Location ausführen; SMAPI-Konsole nach `[PPF]`-Logs durchsuchen.
* **„Beim Klick öffnet sich der Vanilla-Teleporter.“**

  * Prüfen, ob es der **PPF-Teleporter** (exklusives Item) ist, nicht der Vanilla-Mini-Obelisk.
* **„Ich kann keinen Mini-Obelisk in der PPF platzieren.“**

  * PPFs müssen `Farm`-Maps sein. Dieser Mod erstellt `PPF_*` als `Farm`. Bei Migration von alten Versionen (mit `GameLocation`) Mod entfernen/neu aktivieren oder manuell konvertieren.

---

## 🤝 Beiträge

Beiträge **sind willkommen** (Issues, Vorschläge, Patches, PRs)! Bitte beachte:

* Vor größeren Features eine **Issue** eröffnen und Feedback abwarten.
* Umfang in der Issue klären und erst danach PR einreichen, der sich darauf bezieht.
* Kleinere Fixes (Docs/Bugs) können direkt als PR kommen, dennoch ist ein Hinweis in der passenden Issue sinnvoll.
* Stil: C# (.NET 6, Nullable enabled), Logs mit `[PPF]`, idempotente Logik, Multiplayer-Kontext beachten.

> **Wichtig (Lizenz):** Dieser Mod ist **keine freie Software**. **Modifizierte Kopien oder Forks zur Weiterverbreitung sind ohne Zustimmung des Autors verboten.** Pull Requests und Patches werden hier gern geprüft.

---

## 📜 Lizenz

**Alle Rechte vorbehalten.**

* Du darfst den Mod **installieren und nutzen**.
* Du darfst **Verbesserungen** über Issues/PRs im offiziellen Repository vorschlagen.
* **Nicht erlaubt:** Forks veröffentlichen, abgeleitete Mods erstellen oder modifizierte Versionen ohne ausdrückliche Zustimmung des Autors verbreiten.

---

## 🙌 Credits

* **Autor:** DerexSV
* **Frameworks:** SMAPI, Pathoschild.Stardew.ModBuildConfig
* **Community:** Dank an Tester*innen und die Stardew-Valley-Modding-Community.

---

## 📫 Support

Eröffne eine *Issue* mit SMAPI-Logs und Szenariobeschreibung (Host/Client, Map, Reproduktionsschritte). Idealerweise mit kurzem Repro und deaktivierten anderen Mods.
