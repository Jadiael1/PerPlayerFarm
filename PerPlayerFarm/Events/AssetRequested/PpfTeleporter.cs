using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.BigCraftables;

namespace PerPlayerFarm.Events.AssetRequested
{
    public static class PpfTeleporter
    {
        public static void AddIfNotExists(AssetRequestedEventArgs e, ITranslationHelper translate)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, BigCraftableData>().Data;

                    if (!dict.ContainsKey(Utils.Constants.PpfTeleporterId))
                    {
                        dict[Utils.Constants.PpfTeleporterId] = new Contents.Itens.PPFTeleporter(translate);
                    }
                }, AssetEditPriority.Early);
            }
        }
    }
}