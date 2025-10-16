using PerPlayerFarm.Utils;
using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class FarmEntries
    {
        public static void Edit(AssetRequestedEventArgs e, IMonitor monitor, ITranslationHelper translate)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/BusStop") || e.NameWithoutLocale.IsEquivalentTo("Maps/FarmCave") || e.NameWithoutLocale.IsEquivalentTo("Maps/Backwoods") || e.NameWithoutLocale.IsEquivalentTo("Maps/Forest"))
            {
                e.Edit(asset =>
                {
                    IAssetDataForMap editor = asset.AsMap();
                    Map map = editor.Data;
                    Layer back = map.GetLayer("Back");

                    bool hasMapProp = map.Properties.TryGetValue("Warp", out var mapProp);
                    if (!hasMapProp)
                    {
                        monitor.Log($"{translate.Get("derexsv.ppf.log.notice.map_has_no_warp_prop")}", LogLevel.Trace);
                        return;
                    }
                    List<WarpLocations>? warps = ListHelper.ConvertStringForList(mapProp, monitor, translate);
                    if (warps is null)
                    {
                        return;
                    }
                    List<WarpLocations>? warpsFarm = warps.Where(w => w.TargetName.Equals("Farm", StringComparison.OrdinalIgnoreCase)).ToList();
                    if (warpsFarm is null)
                    {
                        return;
                    }

                    for (int i = 0; i < warpsFarm.Count; i++)
                    {
                        if (warpsFarm[i].X >= back.LayerWidth)
                        {
                            warpsFarm[i].X = back.LayerWidth - 1;
                        }
                        if (warpsFarm[i].X <= 0)
                        {
                            warpsFarm[i].X = 1;
                        }
                        if (warpsFarm[i].Y >= back.LayerHeight)
                        {
                            warpsFarm[i].Y = back.LayerHeight - 1;
                        }
                        if (warpsFarm[i].Y <= 0)
                        {
                            warpsFarm[i].Y = 1;
                        }
                        Tile? tile = back.Tiles[warpsFarm[i].X, warpsFarm[i].Y];
                        if (tile is null)
                        {
                            monitor.Log($"{translate.Get("derexsv.ppf.log.notice.null_tile_at_position_xy", new { tyleX = warpsFarm[i].X, tyleY = warpsFarm[i].Y })}", LogLevel.Warn);
                            continue;
                        }
                        if (tile.Properties.ContainsKey("Action")) tile.Properties.Remove("Action");
                        if (tile.Properties.ContainsKey("TouchAction")) tile.Properties.Remove("TouchAction");
                        tile.Properties["TouchAction"] = $"{Utils.Constants.EnterFarmsActionKey} {warpsFarm[i].X} {warpsFarm[i].Y} {warpsFarm[i].TargetName} {warpsFarm[i].TargetX} {warpsFarm[i].TargetY}";
                    }
                    if (warpsFarm.Count > 0)
                    {
                        // string resultingString = ListHelper.ConvertListForString(warpsFarm);
                        // map.Properties[Utils.Constants.EnterFarmsActionKey] = $"{resultingString}";
                        var newWarps = warps.Where(w => w.TargetName != "Farm").ToList();
                        string newWarpsString = ListHelper.ConvertListForString(newWarps);
                        map.Properties["Warp"] = $"{newWarpsString}";
                    }
                }, AssetEditPriority.Late);
            }
        }
    }
}