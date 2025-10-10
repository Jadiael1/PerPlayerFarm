# Per Player Farm (PPF)

> 🌍 README más nyelveken: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **A párhuzamos farmok világa – játékosonként egy.**
>
> Kompatibilis a **Stardew Valley 1.6.15** és **SMAPI 4.3.2** verziókkal.

---

## ✨ Áttekintés

**Per Player Farm (PPF)** *játékosonként külön farmot* hoz létre ugyanabban a mentésben. Minden farmhand saját `Farm` típusú locationt kap `PPF_<UniqueMultiplayerID>` néven, kezdeti tisztítással, homlokzat-kunyhóval és egy gyors utazási folyamattal a **fő farm** ↔ **PPF** között a mod **exkluzív teleportere** segítségével.

Minden a multiplayerre (host az autoritás) lett tervezve, és működik split-screen módban és újracsatlakozáskor is.

---

## 📦 Követelmények

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (ha a forráskódból fordítasz)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) az automatikus deploy/zip feladatokhoz

---

## 🧭 Működés (magas szinten)

* **PPF létrehozása**

  * Host: a világ betöltése/új játék indítása során a mod létrehozza/garantálja az összes ismert `PPF_*`-t (és újakat regisztrál, amikor valaki belép). A `PPF_*` **valódi `Farm`** (nem `GameLocation`), így minden „csak farmon” elhelyezhető vanilla tárgy működik.
  * Client: helyi *stubbokat* hoz létre (szintén `Farm`), hogy warp, UI és utazás host nélkül is elérhetőek legyenek.

* **Map/Assets**

  * `Maps/PPF_*` a `Maps/Farm` *klónjaként* indul, módosításokkal: `CanBuildHere = T`, nincs `MailboxLocation`, valamint eltávolítja a nem kívánt actionöket/warpokat.
  * `Data/Buildings` megkapja a **homlokzatot** `PPF_CabinFacade` néven, ajtó-tile-lal (`Action: PPF_Door`) és postafiókkal (vanilla action).
  * `Data/BigCraftables` beilleszti az **exkluzív teleportert**, `(BC)DerexSV.PPF_Teleporter`, amely a Mini-Obelisk sprite-ját használja (később testreszabható).

* **Utazás**

  * A **PPF teleporterrel** interakciókor egy **utazási menü** nyílik: „Host Farm” + minden `PPF_*` bejegyzés (jelzi a tulajdonos online státuszát).
  * A homlokzat ajtaja (`PPF_Door`) a tulajdonost a saját háza (FarmHouse) belsejébe viszi – megtartva a vanilla viselkedést.

* **Teleporter – okos elhelyezés**

  * Először a **preferált pozíciót** próbálja (a `Teleporter.PreferredTileX/PreferredTileY` config mezők alapján). Ha foglalt, érvényes tile-t keres (spirál és map-scan) a `GameLocation.CanItemBePlacedHere` szabály szerint.
  * A teleporter **nem veszti el a használhatóságát** eltávolítás után sem: az események figyelik a lerakást, és **újra címkézik** az objektumot.

* **Tisztítás és vanilla épületek eltávolítása**

  * *Kezdeti könnyű takarítás* `PPF_*`-n: gyomok/kövek/gallyak, fák/fű, resource clumpok eltávolítása.
  * *Folyamatos strip* `PPF_*`-n: **mindig eltávolítja** a **Farmhouse**-t; **csak akkor távolítja el a Greenhouse-t**, ha még törött (nincs feloldva); a **Shipping Bin** és a **Pet Bowl** érintetlen marad. A törött üvegház eltávolításakor a `Greenhouse` warpokat is törli.

* **Warp szinkronizáció**

  * Az események (`PpfWarpHelper` logika) gondoskodnak arról, hogy a **Cabin → PPF** kapcsolat szinkronban maradjon (a kunyhó ajtaja a tulajdonos PPF-jére, a kijárat a megfelelő homlokzatra mutat).

* **Perzisztencia**

  * A tulajdonos/PPF lista `ppf.locations` (mod-save-data) fájlban tárolódik. Az újragenerálás idempotens.

---

## 🧠 Motiváció és tervezési döntések

A Stardew Valley eredetileg solo élménynek készült; még multiban is a standard felállás (közös farm, kevés kunyhó, opcionális külön kassza) szűkös a független előrehaladáshoz. A mod célja: mindenkinek saját, teljes értékű farm, úgy hogy a vanilla folyamatok (cabinok, Robin, mentés) érintetlenek maradjanak.

### Korai prototípusok és hátrányok

Az első koncepció `PPF_Template`-eket képzelt el, amelyeket a host kezel:

1. A mentés egy `PPF_Template`-tel indul.
2. A host Robin-nal kunyhót/épületet építtetne erre a template-re.
3. Amikor egy vendég belép, a `PPF_Template` `PPF_<INVITER_UID>`-dé válik.
4. A host új `PPF_Template`-et kap a következő vendéghez – és így tovább.

Problémák:

* **Robin integrációja:** a vanilla menüt úgy módosítani, hogy testreszabott mapok jelenjenek meg és a fő farmon kívül lehessen építkezni, ütközik más modokkal, mély UI hookokat és tile/cost ellenőrzéseket igényel.
* **Törékeny perzisztencia:** ha a vendég háza teljesen egyedi locationre költözik, a mod kikapcsolásakor a játék elveszíti a hivatkozást (spawn/ágy nélkül maradhat).
* **Kompabilitás mentésekkel:** minden vendégnél `GameLocation` ↔ `Farm` konverzió korrupt mentéseket okozhat (elveszett tárgyak, hajléktalan állatok, bugos küldetések).
* **Sok template karbantartása:** mindig legyen szabad `PPF_Template`, egyedi név, takarítás – hibalehetőség, főleg sok vendéggel.

### Végleges architektúra

Hogy elkerüljük ezeket, a vendég kunyhója a fő farmon marad, a PPF csak **párhuzamos farmként** szolgál:

* Minden játékos kap egy `PPF_<UID>` locationt (valódi `Farm`), de a vanilla kunyhó a fő farmon marad. A mod kikapcsolásakor is lesz háza és spawnja.
* A PPF-re **homlokzat-kunyhó** kerül, az ajtó egyéni actionnel a valódi kunyhó belsejébe vezet. A postafiók a vanilla viselkedést követi (új levél animáció).
* Egy **egyedi obeliszk** (`(BC)DerexSV.PPF_Teleporter`) minden farmon megjelenik, menüvel lehet ugrálni, mutatva az elérhetőséget/online státuszt.
* A farm bejáratai (Bus Stop, Forest, Backwoods stb.) mind átirányítanak a játékos saját `PPF_*`-jére, miközben a fő farm továbbra is elérhető.

Így a játék alapja megmarad, mod kikapcsolásakor nincs gond, mégis biztosított a különálló terület.

---

## 🕹️ Használat (játékos)

1. A **host** betölti a mentést. A mod létrehozza/garantálja az ismert PPF-eket.
2. A **fő farmon** vagy bármely **PPF**-en interakció a teleporterrel megnyitja az utazási menüt.
3. A **homlokzati ajtó** a tulajdonost a valós kunyhójába (FarmHouse) teleportálja.
4. A **lebegő levél ikon** a PPF postafiókja fölött jelzi az új leveleket (csak tulajdonosnak).

> Tipp: ha az előnyben részesített teleporter pozíció foglalt, a mod automatikusan áthelyezi egy közeli érvényes tile-ra.

---

## ⌨️ SMAPI konzolparancsok

> Csak a **host** változtathat a világon. Parancsokat SMAPI konzolban, betöltött játéknál futtass.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: garantálja a teleporter(eke)t **minden** `PPF_*` és a **fő farm** helyszínen.
  * **here**: csak az aktuális locationön.
  * **farm**: csak a fő farmon.
  * **ppf**: csak az összes `PPF_*` locationön.
  * **<LocationName>**: pontos név alapján biztosítja a teleportert.

* `ppf.clean here|all|ppf`

  * Könnyű takarítást végez (gyomok/fák/akadályok).

* `ppf.strip here|all`

  * **Farmhouse-t** mindig eltávolítja, **Greenhouse-t** csak ha törött; Shipping Bin/Pet Bowl marad. A törött üvegház eltávolításkor törli a `Greenhouse` warpokat.

---

## ⚙️ Konfiguráció & testreszabás

* **Teleporter koordináták:** állítsd be a `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` mezőt a `config.json` fájlban. Hiányzó/érvénytelen érték 74,15-re esik vissza és clampelődik a map méretéhez.
* **Teleporter kinézet:** módosítható `Texture`/`SpriteIndex` a `Data/BigCraftables` blokkon belül (`AssetRequested`).
* **Tisztítás:** a `ppf.clean` parancs újból lehívható PPF-ek takarításához.

---

## 🧩 Kompatibilitás

* Készült **1.6.15** / **SMAPI 4.3.2** verzióhoz.
* Host az autoritás: kliensek/stubbok nem írnak maradandó változásokat.
* Ha más mod ugyanazokat az asseteket módosítja (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`), előfordulhat, hogy a betöltési sorrenden állítani kell. A mod prioritásos `AssetRequested` hookokat használ, igyekszik idempotensen viselkedni.

---

## 🛠️ Fejlesztés

### Gyors build

1. Telepítsd a **.NET 6 SDK**-t.
2. Győződj meg róla, hogy a **Pathoschild.Stardew.ModBuildConfig** csomag a projekt része.
3. A `PerPlayerFarm.csproj`-ban engedélyezd:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Ha kell) hozd létre a `stardewvalley.targets` fájlt a `GamePath`/`GameModsPath` beállításával.
5. Fordítsd:

   ```bash
   dotnet build -c Release
   ```

   A ModBuildConfig ezután bemásolja `…/Stardew Valley/Mods/PerPlayerFarm` alá, és `.zip`-et is készíthet.

### Kódfelépítés

* **ModEntry**: event handlerek regisztrálása, menedzserek inicializálása.
* **Events/**

  * `AssetRequested/*`: `Maps/*`, `Data/Buildings`, `Data/BigCraftables` injektálása/szerkesztése, warpok jelölése.
  * `ButtonPressed/*`: teleporter, homlokzat ajtó és utazási menü kezelése.
  * `DayStarted/*`: takarítás, teleporter biztosítás, warp igazítás nap elején.
  * `LoadStageChanged/*`: ismert PPF-ek garantálása mentés betöltésekor (mentett adatok / ismert farmerek alapján).
  * `ModMessageReceived`: PPF-regiszter szinkronizálása SMAPI üzenetekkel.
  * `ObjectListChanged`: kézzel elhelyezett Mini-Obeliszk címkézése teleporter funkcióhoz.
  * `PeerConnected/*`: host biztosítja PPF erőforrásokat új játékosokhoz, frissíti a kunyhó warpokat.
  * `RenderedWorld` / `RenderingWorld`: a tulajdonos fa
