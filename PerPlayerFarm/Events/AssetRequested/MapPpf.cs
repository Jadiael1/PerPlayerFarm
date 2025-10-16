using Force.DeepCloner;
using PerPlayerFarm.Configuration;
using PerPlayerFarm.Utils;
using PerPLayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using xTile;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class MapPpf
    {

        private static void RemovesSpecificActionsFromAllLayers(Map map, IMonitor monitor)
        {
            foreach (var layer in map.Layers)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                    for (int y = 0; y < layer.LayerHeight; y++)
                    {
                        var t = layer.Tiles[x, y];
                        if (t?.Properties is null) continue;

                        if (t.Properties.TryGetValue("Action", out var pv))
                        {
                            var action = pv?.ToString() ?? "";
                            if (action == "Mailbox" || action.StartsWith("Warp ") && action.Contains("FarmHouse"))
                                t.Properties.Remove("Action");
                        }
                    }
            }
        }
        public static void AddAndEdit(AssetRequestedEventArgs e, IModHelper helper, IMonitor monitor, ModConfig config)
        {
            if (e.NameWithoutLocale.BaseName.StartsWith("Maps/PPF_", StringComparison.OrdinalIgnoreCase))
            {
                e.LoadFrom(
                    () => helper.GameContent.Load<Map>("Maps/Farm").DeepClone(),
                    AssetLoadPriority.Medium
                );
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    var map = editor.Data;
                    var translate = helper.Translation;

                    map.Properties["CanBuildHere"] = "T";

                    map.Properties.Remove("MailboxLocation");

                    bool hasMapProp = map.Properties.TryGetValue("Warp", out var mapProp);
                    if (hasMapProp)
                    {
                        List<WarpLocations>? warps = ListHelper.ConvertStringForList(mapProp, monitor, translate);
                        if (warps is not null && warps.Count > 0)
                        {
                            foreach (var warp in warps)
                            {
                                // decrements 2 from target y
                                if (warp.TargetName == "Forest")
                                {
                                    warp.TargetY -= 2;
                                }
                                // increments 1 from target y
                                if (warp.TargetName == "Backwoods")
                                {
                                    warp.TargetY += 1;
                                }
                            }
                            string newWarpsString = ListHelper.ConvertListForString(warps);
                            map.Properties["Warp"] = newWarpsString;
                        }
                        else
                        {
                            map.Properties["Warp"] = config.FarmMapWarpProperty;
                        }
                    }
                    else
                    {
                        map.Properties["Warp"] = config.FarmMapWarpProperty;
                    }
                    // Goes through all layers and removes all mailbox, warp and FarmHouse actions.
                    RemovesSpecificActionsFromAllLayers(map, monitor);
                }, AssetEditPriority.Default);
            }
        }
    }
}