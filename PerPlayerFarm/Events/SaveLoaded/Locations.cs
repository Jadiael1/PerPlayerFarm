using StardewModdingAPI;
using StardewValley;

namespace PerPlayerFarm.Events.SaveLoaded
{
    public static class Locations
    {
        public static void LoadPpfFarmsForInvited(IMonitor monitor, ITranslationHelper translate)
        {
            if (Context.IsMainPlayer || Game1.player is null) return;

            long uid = Game1.player.UniqueMultiplayerID;
            string locName = $"PPF_{uid}";
            string playerName = $"{Game1.player.displayName}";

            if (Game1.getLocationFromName(locName) is null)
            {
                var loc = new Farm($"Maps/{locName}", locName)
                {
                    IsOutdoors = true,
                    IsFarm = true
                };
                loc.DisplayName = $"{playerName}";
                Game1.locations.Add(loc);
                monitor.Log($"{translate.Get("derexsv.ppf.log.notice.invited_ppf_farm_was_loaded_to_the_invited", new { PPF = locName })}", LogLevel.Info);
            }
        }
    }
}