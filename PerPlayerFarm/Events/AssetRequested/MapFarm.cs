using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class MapFarm
    {
        public static void WarpAdjustmentPropertyFarm(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Farm"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    var map = editor.Data;
                    map.Properties["Warp"] = Utils.Constants.PpfDefaultWarpsFarm;
                }, AssetEditPriority.Default);
            }
        }
    }
}