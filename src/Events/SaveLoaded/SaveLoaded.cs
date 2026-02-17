using PerPlayerFarm.Config;
using PerPlayerFarm.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.SaveLoaded
{
    public static class SaveLoaded
    {
        public static void OnSaveLoaded(
            IModHelper helper,
            IMonitor monitor,
            HashSet<long> knownFarmhandIds,
            string saveKey,
            ModConfig config,
            object? sender,
            SaveLoadedEventArgs e
        )
        {
            foreach (var f in Game1.getAllFarmers())
            {
                if (f.IsMainPlayer) continue;
                if (f is null || f.displayName is null || (f.displayName is not null && f.displayName.Length <= 0)) continue;
                knownFarmhandIds.Add(f.UniqueMultiplayerID);
            }

            foreach (var uid in knownFarmhandIds)
            {
                if (config.WarpToPPFOnHouseExit) WarpHelper.UpdateHomeExitWarp(monitor, uid);
                MapHelpers.EnsurePlayerMap(uid, Game1.GetPlayer(uid)?.displayName, helper, monitor, knownFarmhandIds);
                knownFarmhandIds.Add(uid);
            }
        }
    }
}