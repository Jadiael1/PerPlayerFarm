using PerPlayerFarm.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.LoadStageChanged
{
    public static class LoadStageChanged
    {
        public static void OnLoadStageChanged(
            IModHelper helper,
            IMonitor monitor,
            string saveKey,
            HashSet<long> knownFarmhandIds,
            object? s,
            LoadStageChangedEventArgs e
        )
        {
            if (!Context.IsMainPlayer) return;
            if (e.NewStage is not LoadStage.SaveAddedLocations) return;

            var loaded = helper.Data.ReadSaveData<HashSet<long>>(saveKey) ?? new();

            foreach (var uid in loaded)
            {
                MapHelpers.EnsurePlayerMap(uid, Game1.GetPlayer(uid)?.displayName, helper, monitor, knownFarmhandIds);
                knownFarmhandIds.Add(uid);
            }
        }
    }
}