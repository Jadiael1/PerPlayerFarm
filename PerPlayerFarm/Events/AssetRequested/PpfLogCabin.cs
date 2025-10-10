using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class PpfLogCabin
    {
        public static void AddIfNotExists(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, BuildingData>().Data;
                    if (!dict.ContainsKey(Utils.Constants.FacadeBuildingId))
                    {
                        dict[Utils.Constants.FacadeBuildingId] = new Contents.Buildings.PPFLogCabin();
                    }
                }, AssetEditPriority.Early);
            }
        }
    }
}