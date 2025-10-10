using Force.DeepCloner;
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
        public static void AddAndEdit(AssetRequestedEventArgs e, IModHelper helper, IMonitor monitor)
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

                    map.Properties["CanBuildHere"] = "T";

                    map.Properties.Remove("MailboxLocation");
                    map.Properties["Warp"] = Utils.Constants.PpfDefaultWarpsFarm;

                    // Goes through all layers and removes all mailbox, warp and FarmHouse actions.
                    RemovesSpecificActionsFromAllLayers(map, monitor);
                }, AssetEditPriority.Default);
            }

        }
    }
}