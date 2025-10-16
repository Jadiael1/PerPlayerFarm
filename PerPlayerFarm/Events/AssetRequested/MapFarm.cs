using PerPlayerFarm.Configuration;
using PerPlayerFarm.Utils;
using PerPLayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class MapFarm
    {
        public static void WarpAdjustmentPropertyFarm(AssetRequestedEventArgs e, IModHelper helper, IMonitor monitor, ModConfig config)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Farm"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    var map = editor.Data;
                    var translate = helper.Translation;

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
                }, AssetEditPriority.Default);
            }
        }
    }
}