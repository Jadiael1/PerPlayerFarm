# Per Player Farm (PPF)

> 🌍 README in altre lingue: [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md) · [Magyar](README.hu.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **Un mondo di fattorie parallele, una per ogni giocatore.**
>
> Compatibile con **Stardew Valley 1.6.15** e **SMAPI 4.3.2**.

---

## ✨ Panoramica

**Per Player Farm (PPF)** crea una *fattoria separata per giocatore* nello stesso salvataggio. Ogni giocatore invitato ottiene una location propria di tipo `Farm`, chiamata `PPF_<UniqueMultiplayerID>`, con pulizia iniziale, facciata di cabina e un flusso di viaggio rapido tra **Fattoria principale** ↔ **PPF** tramite un **teletrasportatore esclusivo** del mod.

Tutto è pensato per il multiplayer (host autoritario) e funziona anche in split-screen e riconnessioni.

---

## 📦 Requisiti

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (per compilare dal codice sorgente)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) per deploy/zip automatici

---

## 🧭 Funzionamento (alto livello)

* **Creazione delle PPF**

  * Host: al caricamento/creazione del mondo, il mod crea/garantisce tutte le `PPF_*` conosciute (e registra nuove quando qualcuno entra). Le `PPF_*` sono **vere `Farm`** (non `GameLocation`), quindi gli oggetti vanilla “solo su fattoria” si possono piazzare.
  * Client: crea *stub* locali (sempre `Farm`) per permettere warp, UI e viaggio anche senza l’host.

* **Mappe/Assets**

  * `Maps/PPF_*` nasce come *clone* di `Maps/Farm` con modifiche: `CanBuildHere = T`, senza `MailboxLocation` e senza azioni/warp indesiderati.
  * `Data/Buildings` riceve la **facciata** `PPF_CabinFacade` con tile di porta (`Action: PPF_Door`) e cassetta postale con action vanilla.
  * `Data/BigCraftables` inietta il **teletrasportatore esclusivo** `(BC)DerexSV.PPF_Teleporter`, visivamente basato sul Mini-Obelisk (personalizzabile in seguito).

* **Viaggio**

  * Interagendo con il **teletrasportatore PPF** si apre un **menu di viaggio** con “Fattoria dell’host” + un’entrata per ogni `PPF_*` (indicando se il proprietario è online).
  * La porta della facciata (`PPF_Door`) porta il proprietario direttamente nell’interno della propria casa (FarmHouse), mantenendo il comportamento vanilla.

* **Teletrasportatore: posizionamento intelligente**

  * Tenta per primo la **posizione preferita** (definita via `Teleporter.PreferredTileX/PreferredTileY` nel `config.json`). Se non è possibile, cerca un tile valido (spirale + scansione della mappa) usando la regola `GameLocation.CanItemBePlacedHere`.
  * Il teletrasportatore **resta utile** se rimosso e riposizionato: gli eventi osservano l’aggiunta e **ri-etichettano** l’oggetto.

* **Pulizia e rimozione costruzioni vanilla**

  * *Pulizia iniziale leggera* in `PPF_*`: rimuove detriti (erbacce/pietre/rami), alberi/erba e resource clumps.
  * *Rimozione continua* in `PPF_*`: **rimuove sempre** la **Farmhouse**; **rimuove la Greenhouse solo se è rotta** (non sbloccata); **non tocca** la **Shipping Bin** né la **Pet Bowl**. Togliendo la serra rotta, elimina anche i warp verso `Greenhouse`.

* **Sincronizzazione dei warp**

  * Il codice evento sincronizza la connessione **Cabin → PPF** (la porta della capanna porta nella PPF del proprietario e l’uscita torna alla facciata corrispondente).

* **Persistenza**

  * L’elenco proprietari/PPF è salvato in `ppf.locations` (dati del mod). La ricreazione è idempotente.

---

## 🧠 Motivazione & decisioni di design

Stardew Valley nasce come esperienza single player; anche con il multiplayer, la struttura standard (una fattoria condivisa, poche cabine, denaro opzionalmente separato) è limitante per chi vuole un progresso indipendente. L’obiettivo è dare a ogni ospite una fattoria completa, isolata dall’host, senza rompere i flussi vanilla (cabine, Robin, salvataggio).

### Primi esperimenti e controindicazioni

Il prototipo iniziale prevedeva un ciclo di *template* (`PPF_Template`) gestiti dall’host:

1. Il salvataggio inizia con una `PPF_Template`.
2. L’host, dalla falegnameria di Robin, costruisce una cabina su quella template.
3. Quando un invitato entra, la `PPF_Template` diventa `PPF_<INVITER_UID>`.
4. L’host ottiene una nuova `PPF_Template` per futuri invitati, e così via.

Problemi riscontrati:

* **Integrazione con Robin:** modificare il menu vanilla per elencare mappe custom e consentire costruzioni fuori dalla fattoria principale va in conflitto con altri mod e richiede hook profondi su UI, validazione tile e costi.
* **Persistenza fragile:** spostare la casa dell’invitato su una location completamente custom significa che, disattivando il mod, il gioco perde la reference dell’abitazione; l’invitato resta senza punto di spawn/letto valido.
* **Compatibilità con salvataggi:** convertire `GameLocation` ↔ `Farm` per giocatore comporta rischi di corruzione (oggetti persi, animali senza casa, quest rotte).
* **Gestione di molte template:** garantire sempre una `PPF_Template` vuota, promuovere nomi univoci, ripulire i residui rischia di portare a inconsistenze, specialmente con molti invitati che entrano/escono.

### Architettura finale

Per evitare questi problemi, le cabine restano nella fattoria principale e la PPF funge da **fattoria parallela**:

* Ogni giocatore ha una `PPF_<UID>` (vera `Farm`), ma la cabina vanilla rimane sul main farm. Se il mod viene disattivato, casa e spawn restano validi.
* Nella PPF viene aggiunta una **facciata** (Log Cabin); la porta ha una action personalizzata che teletrasporta all’interno della cabina reale. Il tile della casella postale replica quello vanilla (animazione nuova posta).
* Un **obelisco personalizzato** (`(BC)DerexSV.PPF_Teleporter`) compare su tutte le fattorie, gestisce il viaggio con menu, indicando disponibilità e stato online.
* Le entrate della fattoria (Bus Stop, Forest, Backwoods, ecc.) sono sincronizzate: ciascun giocatore viene indirizzato alla propria `PPF_*`, mentre la fattoria principale resta accessibile.

Questa architettura mantiene la compatibilità col gioco base, evita problemi se il mod viene rimosso e offre lo spazio indipendente progettato.

---

## 🕹️ Utilizzo (giocatori)

1. L’**host** carica il salvataggio. Le PPF note vengono create/garantite.
2. Nella **fattoria principale** o in una **PPF**, interagisci con il teletrasportatore per aprire il menu di viaggio.
3. La **porta della facciata** porta il proprietario all’interno della propria cabina (FarmHouse).
4. L’icona **lettera** sopra la cassetta della PPF compare se c’è posta (solo per il proprietario).

> Suggerimento: se la posizione preferita del teletrasportatore è bloccata, il mod lo riposiziona automaticamente su un tile valido vicino.

---

## ⌨️ Comandi SMAPI

> Solo l’**host** può modificare il mondo. Esegui i comandi nella console SMAPI a gioco caricato.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all**: garantisce teletrasportatori in **tutte** le `PPF_*` e nella **fattoria principale**.
  * **here**: solo nella location attuale.
  * **farm**: solo nella fattoria principale.
  * **ppf**: solo in tutte le `PPF_*`.
  * **<LocationName>**: teletrasporta specificamente nella location indicata.

* `ppf.clean here|all|ppf`

  * Pulizia leggera di detriti/alberi/risorse.

* `ppf.strip here|all`

  * Rimuove **Farmhouse** sempre e **Greenhouse se rotta** (in `PPF_*`); lascia Shipping Bin/Pet Bowl. Se elimina la serra rotta, toglie anche i warp verso `Greenhouse`.

---

## ⚙️ Configurazione & personalizzazione

* **Ancoraggio teletrasportatore**: modifica `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` nel `config.json`. Valori mancanti/invalidi tornano a 74,15 e sono clamped alle dimensioni della mappa.
* **Aspetto teletrasportatore**: cambia `Texture`/`SpriteIndex` nel blocco `Data/BigCraftables` di `AssetRequested`.
* **Pulizia**: usa `ppf.clean` (console) per ripulire nuovamente le PPF.

---

## 🧩 Compatibilità

* Progettato per **1.6.15** / **SMAPI 4.3.2**.
* L’host è **autorità**: i client/stub non applicano cambi permanenti.
* Mod che modificano gli **stessi asset** (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`) potrebbero richiedere un ordine di caricamento specifico. Il mod usa `AssetRequested` prioritari ed è pensato per essere idempotente.

---

## 🛠️ Sviluppo

### Build rapida

1. Installa il **.NET 6 SDK**.
2. Assicurati che il pacchetto **Pathoschild.Stardew.ModBuildConfig** sia presente.
3. In `PerPlayerFarm.csproj`, abilita:

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Se necessario) crea `stardewvalley.targets` con i percorsi `GamePath`/`GameModsPath`.
5. Compila:

   ```bash
   dotnet build -c Release
   ```

   Il ModBuildConfig copia nella cartella `…/Stardew Valley/Mods/PerPlayerFarm` e può generare uno `.zip`.

### Struttura del codice

* **ModEntry**: registra gli handler e inizializza i manager.
* **Events/**

  * `AssetRequested/*`: inietta/modifica `Maps/*`, `Data/Buildings`, `Data/BigCraftables`, segna i warp custom.
  * `ButtonPressed/*`: gestisce teletrasportatore, porta di facciata e menu di viaggio.
  * `DayStarted/*`: pulizia, teletrasportatore e aggiustamenti warp all’inizio della giornata.
  * `LoadStageChanged/*`: garantisce le PPF note al caricamento del salvataggio (dati persistiti/giocatori).
  * `ModMessageReceived`: sincronizza il registro PPF tramite messaggi SMAPI.
  * `ObjectListChanged`: retagga i Mini-Obelisk posizionati manualmente.
  * `PeerConnected/*`: l’host garantisce le risorse PPF per i nuovi giocatori e aggiorna i warp della casa.
  * `RenderedWorld` / `RenderingWorld`: gestiscono e disegnano l’indicatore di posta sopra la facciata del proprietario.
  * `ReturnedToTitle`: pulisce le cache client al ritorno al menu principale.
  * `SaveLoaded/*`: carica la PPF dell’invitato (client) e rimuove costruzioni vanilla nelle PPF dell’host.
  * `Saving`: salva `ppf.locations` con gli UID conosciuti.
  * `UpdateTicked`: sostituisce i warp perché ogni giocatore arrivi nella propria `PPF_*`.
* **Utils/**

  * `Constants`: chiavi/modData condivise.
  * `ListHelper`: parsing/serializzazione delle stringhe warp.
  * `MailboxState`: stato temporaneo usato da Rendering/RenderedWorld.
  * `PpfConsoleCommands`: comandi `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations`: modelli dati per multiplayer/persistenza.
* **Contents/**

  * `Buildings/LogCabin.cs`: facciata PPF (`PPF_CabinFacade`).
  * `Itens/PPFTeleporter.cs`: teletrasportatore esclusivo `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * File `*.json` con tutti i messaggi di log/localizzazione.
* **Configuration/**

  * `ModConfig`: opzioni di configurazione lette dal `config.json` di SMAPI.

### Punti di riuso chiave

Alcune utility appaiono in più contesti; modificandole, considera tutti gli usi:

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`): garantisce/reattiva i teletrasportatori sia nel ciclo iniziale della giornata sia via comando.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`): sostituisce i warp standard delle cabane con l’uscita verso la PPF del proprietario.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`): assicurano l’infrastruttura PPF lato host per ciascun invitato.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`): mantiene la mappatura UID ↔ PPF persistita.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`): carica la “shadow PPF” per i client.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`): rimuove le costruzioni vanilla continuamente nelle PPF.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfCon
