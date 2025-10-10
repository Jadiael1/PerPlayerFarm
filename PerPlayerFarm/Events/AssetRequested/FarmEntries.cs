using PerPlayerFarm.Utils;
using PerPLayerFarm.Types;
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
            if (e.NameWithoutLocale.Name is "Maps/BusStop" or "Maps/FarmCave" or "Maps/Backwoods" or "Maps/Forest")
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
                    List<WarpLocations>? warpsFarm = warps.Where(w => w.TargetName == "Farm").ToList();
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
                        tile.Properties[Utils.Constants.TouchKey] = $"{warpsFarm[i].X} {warpsFarm[i].Y} {warpsFarm[i].TargetName} {warpsFarm[i].TargetX} {warpsFarm[i].TargetY}";
                    }
                    if (warpsFarm.Count > 0)
                    {
                        string resultingString = ListHelper.ConvertListForString(warpsFarm);
                        map.Properties[Utils.Constants.TouchKey] = $"{resultingString}";
                    }
                }, AssetEditPriority.Late);
            }

        }
    }
}