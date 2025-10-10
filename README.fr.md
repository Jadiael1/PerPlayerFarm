# Per Player Farm (PPF)

> 🌍 README dans d’autres langues : [Português (BR)](README.md) · [English](README.en.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Magyar](README.hu.md) · [Italiano](README.it.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Русский](README.ru.md) · [Türkçe](README.tr.md) · [中文](README.zh.md)

> **Un monde de fermes parallèles, une pour chaque joueur.**
>
> Compatible avec **Stardew Valley 1.6.15** et **SMAPI 4.3.2**.

---

## ✨ Aperçu

**Per Player Farm (PPF)** crée une *ferme séparée par joueur* dans le même fichier de sauvegarde. Chaque fermier (farmhand) obtient sa propre location de type `Farm`, nommée `PPF_<UniqueMultiplayerID>`, avec nettoyage initial, façade de cabane et un flux de voyage rapide entre la **ferme principale** ↔ **PPF** via un **téléporteur exclusif**.

Tout est pensé pour le multijoueur (hôte autoritaire) et fonctionne aussi en écran partagé comme lors des reconnexions.

---

## 📦 Pré-requis

* **Stardew Valley** 1.6.15
* **SMAPI** 4.3.2
* **.NET 6** (pour compiler depuis les sources)
* (Dev) **Pathoschild.Stardew.ModBuildConfig** (NuGet) pour le déploiement/zip automatiques

---

## 🧭 Fonctionnement (vue d’ensemble)

* **Création des PPF**

  * Hôte : au chargement/à la création de la partie, le mod crée/assure toutes les `PPF_*` connues (et enregistre les nouvelles à l’arrivée de joueurs). Les `PPF_*` sont de **vraies `Farm`** (pas de `GameLocation`), ainsi les objets vanilla « uniquement sur une ferme » restent compatibles.
  * Client : crée des *stubs* locaux (aussi des `Farm`) pour conserver warps, UI et voyages même sans l’hôte.

* **Cartes/Assets**

  * `Maps/PPF_*` est un *clone* de `Maps/Farm` avec ajustements : `CanBuildHere = T`, pas de `MailboxLocation`, pas d’actions/warps indésirables.
  * `Data/Buildings` reçoit la **façade** `PPF_CabinFacade` avec tile de porte (`Action: PPF_Door`) et une boîte aux lettres utilisant l’action vanilla.
  * `Data/BigCraftables` ajoute le **téléporteur exclusif** `(BC)DerexSV.PPF_Teleporter`, basé sur le sprite du Mini-Obélisque (personnalisable).

* **Voyage**

  * Interagir avec le **téléporteur PPF** ouvre un **menu de voyage** listant « Ferme de l’hôte » + une entrée par `PPF_*` (avec statut en ligne du propriétaire).
  * La porte de la façade (`PPF_Door`) envoie le propriétaire directement à l’intérieur de sa maison (FarmHouse), conservant le comportement vanilla.

* **Téléporteur : placement intelligent**

  * Tente d’abord la **position préférée** (configurable via `Teleporter.PreferredTileX/PreferredTileY` dans `config.json`). Si elle est bloquée, recherche un tile valide (spirale + balayage de la carte) en utilisant la règle `GameLocation.CanItemBePlacedHere`.
  * Le téléporteur **reste fonctionnel** en cas de déplacement : les événements surveillent et **retaggent** l’objet automatiquement.

* **Nettoyage et retrait vanilla**

  * *Nettoyage initial léger* dans `PPF_*` : enlève mauvaises herbes/pierres/branches, arbres/gazon et resource clumps.
  * *Retrait continu* dans `PPF_*` : **supprime toujours** la **Farmhouse** ; **supprime la serre (Greenhouse) uniquement si elle est cassée** ; **ne touche pas** la **caisse d’expédition** ni la **gamelle**. Si la serre cassée est retirée, les warps vers `Greenhouse` sur la PPF sont effacés.

* **Synchronisation des warps**

  * Les événements synchronisent la liaison **Cabin → PPF** (la porte de la cabane mène à la PPF du propriétaire et la sortie renvoie vers la façade correspondante).

* **Persistance**

  * La liste des propriétaires/PPF est enregistrée dans `ppf.locations` (données du mod). La recréation est idempotente.

---

## 🧠 Motivation & choix de conception

Stardew Valley est conçu pour le solo ; même avec le multijoueur, la structure standard (ferme partagée, peu de cabanes, argent optionnellement séparé) limite l’avancée indépendante. L’objectif : offrir à chaque invité une ferme dédiée sans casser les flux vanilla (cabanes, Robin, sauvegarde).

### Premiers essais et limites

Le prototype initial imaginait un cycle de *templates* (`PPF_Template`) gérés par l’hôte :

1. La sauvegarde débute avec une `PPF_Template`.
2. L’hôte demande à Robin de construire une cabane dans cette template.
3. Quand un invité rejoint, la `PPF_Template` devient `PPF_<INVITER_UID>`.
4. Une nouvelle `PPF_Template` est donnée à l’hôte pour de futurs invités.

Problèmes rencontrés :

* **Intégration Robin** : modifier le menu vanilla pour permettre des constructions sur cartes personnalisées entre en conflit avec d’autres mods et nécessite de gros hooks UI, vérifs de tiles et coûts.
* **Persistance fragile** : déplacer la cabane du joueur dans une location custom signifie qu’en désactivant le mod, le jeu perd la référence au foyer (spawn/lit invalides).
* **Compatibilité sauvegardes** : convertir `GameLocation` ↔ `Farm` par invité comporte risques (objets perdus, animaux sans maison, quêtes brisées).
* **Gestion des templates** : assurer qu’il y ait toujours une template libre, promouvoir des noms uniques, nettoyer les restes est source d’erreurs, surtout avec de nombreux invités.

### Architecture finale

Pour l’éviter, la cabane vanilla reste sur la ferme principale et la PPF devient une **ferme parallèle** :

* Chaque joueur dispose d’une `PPF_<UID>` (véritable `Farm`), mais sa cabane vanilla demeure chez l’hôte. Si le mod est désactivé, la maison et le spawn restent valides.
* Une **façade (Log Cabin)** est ajoutée à la PPF ; la porte possède une action custom qui renvoie à la vraie cabane. La boîte aux lettres reprend les tiles vanilla (animation de nouveau courrier incluse).
* Un **obélisque personnalisé** (`(BC)DerexSV.PPF_Teleporter`) apparaît sur toutes les fermes, gère les voyages via menu et montre disponibilité/statut en ligne.
* Les warps d’entrée (Bus Stop, Forest, Backwoods, etc.) sont synchronisés pour diriger chaque joueur vers sa `PPF_*`, tout en conservant l’accès à la ferme principale.

Cette architecture reste compatible avec le jeu de base, évite les soucis si le mod est retiré et procure l’espace indépendant recherché.

---

## 🕹️ Utilisation (joueurs)

1. L’**hôte** charge la sauvegarde. Les PPF connues sont créées/garanties.
2. Sur la **ferme principale** ou une **PPF**, interagir avec le téléporteur pour ouvrir le menu de voyage.
3. La **porte de façade** téléporte le propriétaire à l’intérieur de sa cabane réelle (FarmHouse).
4. L’icône flottante de **lettre** apparaît au-dessus de la boîte aux lettres en cas de courrier (pour le propriétaire uniquement).

> Astuce : si la position préférée est bloquée, le mod relocalise automatiquement le téléporteur sur un tile valide proche.

---

## ⌨️ Commandes SMAPI

> Seul l’**hôte** modifie le monde. Exécuter les commandes dans la console SMAPI avec la partie chargée.

* `ppf.ensure-teleports all|here|farm|ppf|<LocationName>`

  * **all** : garantit des téléporteurs dans **toutes** les `PPF_*` + la **ferme principale**.
  * **here** : uniquement dans la location actuelle.
  * **farm** : uniquement sur la ferme principale.
  * **ppf** : uniquement sur toutes les `PPF_*`.
  * **<LocationName>** : garantit dans la location nommée.

* `ppf.clean here|all|ppf`

  * Nettoyage léger (débris/herbes/arbres/resource clumps).

* `ppf.strip here|all`

  * Supprime **Farmhouse** systématiquement et **Greenhouse si cassée** dans `PPF_*` (laisse caisse & gamelle). Supprime les warps vers `Greenhouse` si la serre cassée est retirée.

---

## ⚙️ Configuration & personnalisation

* **Ancrage du téléporteur** : régler `Teleporter.PreferredTileX` / `Teleporter.PreferredTileY` dans `config.json`. Valeurs absentes/invalides reviennent à 74,15 et sont clampées selon la carte.
* **Apparence du téléporteur** : modifier `Texture`/`SpriteIndex` dans le bloc `Data/BigCraftables` d’`AssetRequested`.
* **Nettoyage** : utiliser `ppf.clean` (console) pour re-nettoyer une PPF.

---

## 🧩 Compatibilité

* Conçu pour **1.6.15** / **SMAPI 4.3.2**.
* L’hôte est **autorité** : les clients/stubs ne créent pas de changements permanents.
* Les mods modifiant les **mêmes assets** (`Maps/Farm`, `Maps/BusStop`, `Data/Buildings`, `Data/BigCraftables`) peuvent nécessiter un ordre de chargement spécifique. Le mod emploie `AssetRequested` avec priorités et vise l’idempotence.

---

## 🛠️ Développement

### Build rapide

1. Installer le **.NET 6 SDK**.
2. Vérifier l’ajout du package **Pathoschild.Stardew.ModBuildConfig**.
3. Dans `PerPlayerFarm.csproj`, activer :

   ```xml
   <EnableModDeploy>true</EnableModDeploy>
   <EnableModZip>true</EnableModZip>
   <ModFolderName>PerPlayerFarm</ModFolderName>
   ```
4. (Si besoin) créer `stardewvalley.targets` avec `GamePath`/`GameModsPath`.
5. Compiler :

   ```bash
   dotnet build -c Release
   ```

   ModBuildConfig copie ensuite vers `…/Stardew Valley/Mods/PerPlayerFarm` et peut générer un `.zip`.

### Structure du code

* **ModEntry** : enregistre les handlers et initialise les gestionnaires.
* **Events/**

  * `AssetRequested/*` : injecte/modifie `Maps/*`, `Data/Buildings`, `Data/BigCraftables`, marque les warps custom.
  * `ButtonPressed/*` : gère téléporteur, porte de façade et menu de voyage.
  * `DayStarted/*` : nettoyage, assurance des téléporteurs et ajustements de warps au début de la journée.
  * `LoadStageChanged/*` : assure les PPF connues lors du chargement (données persistées/joueurs).
  * `ModMessageReceived` : synchronise le registre PPF via messages SMAPI.
  * `ObjectListChanged` : retaggage des Mini-Obelisques placés manuellement.
  * `PeerConnected/*` : l’hôte prépare les ressources PPF pour les nouveaux joueurs et met à jour les warps.
  * `RenderedWorld` / `RenderingWorld` : gèrent/dessinent l’indicateur de courrier sur la façade du propriétaire.
  * `ReturnedToTitle` : nettoie les caches client au retour au menu.
  * `SaveLoaded/*` : charge les PPF pour invités (client) et retire les bâtiments vanilla côté hôte.
  * `Saving` : persiste `ppf.locations` avec la liste UID ↔ PPF.
  * `UpdateTicked` : remplace les warps pour que chaque joueur arrive sur sa `PPF_*`.
* **Utils/**

  * `Constants` : clés/modData partagé(e)s.
  * `ListHelper` : parsing/sérialisation des chaînes de warps.
  * `MailboxState` : état temporaire pour Rendering/RenderedWorld.
  * `PpfConsoleCommands` : commandes `ppf.ensure-teleports`, `ppf.clean`, `ppf.strip`.
* **Types/**

  * `PpfFarmEntry`, `PpfRegistryMessage`, `PpfSaveData`, `WarpLocations` : modèles de données pour multijoueur/persistance.
* **Contents/**

  * `Buildings/LogCabin.cs` : façade `PPF_CabinFacade`.
  * `Itens/PPFTeleporter.cs` : téléporteur exclusif `(BC)DerexSV.PPF_Teleporter`.
* **i18n/**

  * Messages/Logs localisés en `*.json`.
* **Configuration/**

  * `ModConfig` : options lues depuis `config.json`.

### Points de réutilisation clés

Certains helpers interviennent à plusieurs endroits ; toute modification doit considérer chaque usage :

* `TeleportItem.Initializer` (`Events/DayStarted`, `Utils/PpfConsoleCommands`) : assure téléporteurs au cycle quotidien et via commande.
* `HouseWarpUtils.OverrideDefaultHouseWarpToPPF` (`Events/DayStarted`, `Events/LoadStageChanged`, `Events/PeerConnected`) : redirige la sortie des cabanes vers la PPF du propriétaire.
* `Peerconnected.Locations.LoadInvitedPpfFarmsForHost` / `LoadFacadeCabinInPpfOfInvitedForHost` (`Events/LoadStageChanged`, `Events/PeerConnected`) : garantit l’infrastructure PPF côté hôte.
* `Peerconnected.Locations.TrackOwner` (`Events/LoadStageChanged`, `Events/PeerConnected`) : maintient l’association UID ↔ PPF.
* `SaveLoaded.Locations.LoadPpfFarmsForInvited` (`Events/SaveLoaded`, `Events/DayStarted`) : charge la « PPF shadow » pour les clients.
* `StripAllBuildingsDefault.Strip` (`Events/SaveLoaded`, `Events/DayStarted`) : retire continuellement les bâtiments vanilla des PPF.
* `PlayerDataInitializer.CleanLocation` (`Events/DayStarted`, `Utils/PpfConsoleCommands`) : nettoyage initial (réutilisé par `ppf.clean`).
* `ListHelper.ConvertStringForList` (`Events/AssetRequested/FarmEntries`, `Events/UpdateTicked`) : désérialise les warps injectés.
* `PpfBuildingHelper.TryGetOwnerUid` / `GetMailboxTile` (`Events/ButtonPressed`, `Events/RenderedWorld`) : Identifie propriétaire et tiles nécessaires pour interaction/overlay.

---

## 🧯 Dépannage

* **« Le téléporteur n’est pas apparu. »**

  * Utiliser `ppf.ensure-teleports here` dans la location actuelle ; consulter la console SMAPI pour les logs `[PPF]`.
* **« Le clic ouvre le téléporteur vanilla. »**

  * Vérifier qu’il s’agit bien du **téléporteur PPF** (objet exclusif), pas du Mini-Obélisque de base.
* **« Je ne peux pas placer un Mini-Obélisque vanilla sur la PPF. »**

  * Les PPF doivent être de type `Farm`. Ce mod crée `PPF_*` en tant que `Farm`. Pour les anciennes versions (en `GameLocation`), retirer/réactiver le mod pour recréer les locations, ou convertir manuellement.

---

## 🤝 Contributions

Les contributions **sont les bienvenues** (issues, suggestions, patches, PR) ! Pour rester alignés :

* Avant toute grosse fonctionnalité, ouvrir une **issue** détaillant l’idée et attendre un retour.
* Valider le périmètre dans l’issue, puis soumettre le PR correspondant pour éviter toute rework.
* Pour les correctifs mineurs (docs/bugs), un PR direct est accepté, mais un mot dans l’issue correspondante reste conseillé.
* Style : C# (.NET 6, nullable), logs `[PPF]` (Trace/Info/Warn), logique idempotente,
