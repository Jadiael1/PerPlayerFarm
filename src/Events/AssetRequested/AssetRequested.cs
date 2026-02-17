using Force.DeepCloner;
using PerPlayerFarm.Config;
using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class AssetRequested
    {
        private static readonly HashSet<string> FarmMapNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Maps/Farm",
            "Maps/Farm_Fishing",
            "Maps/Farm_Foraging",
            "Maps/Farm_Mining",
            "Maps/Farm_Combat",
            "Maps/Farm_FourCorners",
            "Maps/Farm_Island",
            "Maps/Farm_Ranching"
        };

        public static void OnAssetRequested(
            IModHelper helper,
            IMonitor monitor,
            ModConfig config,
            string enterFarmsTouchActionKey,
            object? sender,
            AssetRequestedEventArgs e
        )
        {
            if (config.RedirectFarmhandsToPPF && FarmMapNames.Contains(e.NameWithoutLocale.BaseName))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    var map = editor.Data;
                    var translate = helper.Translation;

                    bool hasMapProp = map.Properties.TryGetValue("Warp", out var mapProp);
                    if (hasMapProp)
                    {
                        List<WarpLocations>? warps = WarpHelper.ParseWarpString(mapProp);
                        if (warps is null || warps.Count == 0) return;

                        foreach (var warp in warps)
                        {
                            // decrements 2 from target y
                            if (warp.TargetName.Equals("Forest", StringComparison.OrdinalIgnoreCase)) warp.TargetY += 2;

                            // decrements 2 from target y
                            if (warp.TargetName.Equals("Backwoods", StringComparison.OrdinalIgnoreCase)) warp.TargetY -= 2;
                        }
                        string newWarpsString = WarpHelper.BuildWarpString(warps);
                        map.Properties["Warp"] = newWarpsString;

                    }
                }, AssetEditPriority.Default);
            }

            if (e.NameWithoutLocale.BaseName.StartsWith("Maps/PPF_", StringComparison.OrdinalIgnoreCase))
            {
                var farm = Game1.getFarm();
                string farmMapAsset = farm?.mapPath?.Value ?? "Maps/Farm";
                e.LoadFrom(
                    () => helper.GameContent.Load<Map>(farmMapAsset).DeepClone(),
                    AssetLoadPriority.Medium
                );
                // e.LoadFrom(
                //     () => helper.ModContent.Load<Map>("assets/maps/Ppf.tmx"),
                //     AssetLoadPriority.Medium
                // );

                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    var map = editor.Data;
                    map.Properties["CanBuildHere"] = "T";
                    map.Properties["IsFarm"] = "T";

                    if (!config.RedirectFarmhandsToPPF) return;

                    bool hasMapProp = map.Properties.TryGetValue("Warp", out var mapProp);
                    if (!hasMapProp)
                    {
                        return;
                    }

                    List<WarpLocations>? warps = WarpHelper.ParseWarpString(mapProp);
                    if (warps is null || warps.Count <= 0)
                    {
                        return;
                    }

                    foreach (var warp in warps)
                    {
                        // increment 2 from target y
                        if (warp.TargetName.Equals("Forest", StringComparison.OrdinalIgnoreCase))
                            warp.TargetY += 2;

                        // decrement 2 from target y
                        if (warp.TargetName.Equals("Backwoods", StringComparison.OrdinalIgnoreCase))
                            warp.TargetY -= 2;
                    }
                    string newWarpsString = WarpHelper.BuildWarpString(warps);
                    map.Properties["Warp"] = newWarpsString;

                }, AssetEditPriority.Default);
            }


            if (config.RedirectFarmhandsToPPF && (e.NameWithoutLocale.IsEquivalentTo("Maps/BusStop") || e.NameWithoutLocale.IsEquivalentTo("Maps/FarmCave") || e.NameWithoutLocale.IsEquivalentTo("Maps/Backwoods") || e.NameWithoutLocale.IsEquivalentTo("Maps/Forest")))
            {
                e.Edit(asset =>
                {
                    IAssetDataForMap editor = asset.AsMap();
                    Map map = editor.Data;
                    Layer back = map.GetLayer("Back");

                    bool hasMapProp = map.Properties.TryGetValue("Warp", out var mapProp);
                    if (!hasMapProp) return;

                    List<WarpLocations>? warps = WarpHelper.ParseWarpString(mapProp);
                    if (warps is null) return;

                    List<WarpLocations>? warpFarms = warps.Where(w => w.TargetName.Equals("Farm", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (warpFarms is null || warpFarms.Count == 0) return;

                    foreach (var warpFarm in warpFarms)
                    {
                        if (warpFarm.X >= back.LayerWidth) warpFarm.X = back.LayerWidth - 1;
                        if (warpFarm.X <= 0) warpFarm.X = 1;
                        if (warpFarm.Y >= back.LayerHeight) warpFarm.Y = back.LayerHeight - 1;
                        if (warpFarm.Y <= 0) warpFarm.Y = 1;

                        Tile? tile = back.Tiles[warpFarm.X, warpFarm.Y];
                        if (tile is null) continue;
                        tile.Properties["TouchAction"] = $"{enterFarmsTouchActionKey} {warpFarm.X} {warpFarm.Y} {warpFarm.TargetName} {warpFarm.TargetX} {warpFarm.TargetY}";
                    }

                    var newWarps = warps.Where(w => w.TargetName != "Farm").ToList();
                    string newWarpsString = WarpHelper.BuildWarpString(newWarps);
                    map.Properties["Warp"] = $"{newWarpsString}";
                }, AssetEditPriority.Late);
            }
        }

    }
}