```helper.Events.Content.AssetRequested
EventHandler<StardewModdingAPI.Events.AssetRequestedEventArgs> StardewModdingAPI.Events.IContentEvents.AssetRequested
Raised when an asset is being requested from the content pipeline.

The asset isn't necessarily being loaded yet (e.g. the game may be checking if it exists). Mods can register the changes they want to apply using methods on the event arguments. These will be applied when the asset is actually loaded. If the asset is requested multiple times in the same tick (e.g. once to check if it exists and once to load it), SMAPI might only raise the event once and reuse the cached result.
```

```helper.Events.Specialized.LoadStageChanged
EventHandler<StardewModdingAPI.Events.LoadStageChangedEventArgs> StardewModdingAPI.Events.ISpecializedEvents.LoadStageChanged
Raised when the low-level stage in the game's loading process has changed. This is an advanced event for mods which need to run code at specific points in the loading process. The available stages or when they happen might change without warning in future versions (e.g. due to changes in the game's load process), so mods using this event are more likely to break or have bugs. Most mods should use StardewModdingAPI.Events.IGameLoopEvents instead.
```

```helper.Events.Multiplayer.PeerConnected
EventHandler<StardewModdingAPI.Events.PeerConnectedEventArgs> StardewModdingAPI.Events.IMultiplayerEvents.PeerConnected
Raised after a peer connection is approved by the game.
```

```helper.Events.GameLoop.Saving
EventHandler<StardewModdingAPI.Events.SavingEventArgs> StardewModdingAPI.Events.IGameLoopEvents.Saving
Raised before the game begins writing data to the save file (except the initial save creation).
```

```helper.Events.GameLoop.SaveLoaded
EventHandler<StardewModdingAPI.Events.SaveLoadedEventArgs> StardewModdingAPI.Events.IGameLoopEvents.SaveLoaded
Raised after the player loads a save slot and the world is initialized.
```

```helper.Events.GameLoop.DayStarted
EventHandler<StardewModdingAPI.Events.DayStartedEventArgs> StardewModdingAPI.Events.IGameLoopEvents.DayStarted
Raised after the game begins a new day (including when the player loads a save).
```

```helper.Events.Input.ButtonPressed
EventHandler<StardewModdingAPI.Events.ButtonPressedEventArgs> StardewModdingAPI.Events.IInputEvents.ButtonPressed
Raised after the player presses a button on the keyboard, controller, or mouse.
```

```helper.Events.Display.RenderingWorld
EventHandler<StardewModdingAPI.Events.RenderingWorldEventArgs> StardewModdingAPI.Events.IDisplayEvents.RenderingWorld
Raised before the game world is drawn to the screen. This event isn't useful for drawing to the screen, since the game will draw over it.
```

```helper.Events.Display.RenderedWorld
EventHandler<StardewModdingAPI.Events.RenderedWorldEventArgs> StardewModdingAPI.Events.IDisplayEvents.RenderedWorld
Raised after the game world is drawn to the sprite batch, before it's rendered to the screen. Content drawn to the sprite batch at this point will be drawn over the world, but under any active menu, HUD elements, or cursor.
```

```e.NameWithoutLocale
IAssetName AssetRequestedEventArgs.NameWithoutLocale { get; }
The AssetRequestedEventArgs.Name with any locale codes stripped.

For example, if AssetRequestedEventArgs.Name contains a locale like Data/Bundles.fr-FR, this will be the name without locale like Data/Bundles. If the name has no locale, this field is equivalent.
'NameWithoutLocale' não é nulo aqui.
```

```e.Edit
void AssetRequestedEventArgs.Edit(Action<IAssetData> apply, [AssetEditPriority priority = AssetEditPriority.Default], [string? onBehalfOf = null])
Edit the asset after it's loaded.

Usage notes:

• Editing an asset which doesn't exist has no effect. This is applied after the asset is loaded from the game's Content folder, or from any mod's AssetRequestedEventArgs.LoadFrom(Func<object>, AssetLoadPriority, string) or AssetRequestedEventArgs.LoadFromModFile<TAsset>(string, AssetLoadPriority).
• You can apply any number of edits to the asset. Each edit will be applied on top of the previous one (i.e. it'll see the merged asset from all previous edits as its input).
```

```var dict = asset.AsDictionary<string, BigCraftableData>().Data;
IDictionary<string, BigCraftableData> IAssetData<IDictionary<string, BigCraftableData>>.Data { get; }
The content data being read.
'Data' não é nulo aqui.
```

```var dict = asset.AsDictionary<string, BuildingData>().Data;
IDictionary<string, BuildingData> IAssetData<IDictionary<string, BuildingData>>.Data { get; }
The content data being read.
'Data' não é nulo aqui.
```

```ActionTiles
(Campo) List<BuildingActionTile> BuildingData.ActionTiles
A list of tiles which the player can click to trigger an Action map tile property.
'ActionTiles' is not nullable aware.
```

```TileProperties
(Campo) List<BuildingTileProperty> BuildingData.TileProperties
The map tile properties to set.
'TileProperties' is not nullable aware.
```

```loc.objects.Count
O tipo de representante não pôde ser inferido.CS8917
int StardewValley.Network.OverlaidDictionary.Count() (+ 2 sobrecargas)
'Count' não é nulo aqui.
```

```loc.objects.Count()
int StardewValley.Network.OverlaidDictionary.Count() (+ 2 sobrecargas)
```

```e.Added.Count
O tipo de representante não pôde ser inferido.CS8917
(Extensão) int IEnumerable<KeyValuePair<Vector2, StardewValley.Object>>.Count<KeyValuePair<Vector2, StardewValley.Object>>() (+ 1 sobrecarga)
Returns the number of elements in a sequence.

Devoluções:
  The number of elements in the input sequence.
'Count' não é nulo aqui.

Exceções:
  ArgumentNullException
  OverflowException
```

```e.Added.Count()
(Extensão) int IEnumerable<KeyValuePair<Vector2, StardewValley.Object>>.Count<KeyValuePair<Vector2, StardewValley.Object>>() (+ 1 sobrecarga)
Returns the number of elements in a sequence.

Devoluções:
  The number of elements in the input sequence.

Exceções:
  ArgumentNullException
  OverflowException
```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

```

