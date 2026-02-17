
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.Saving
{
    public static class Saving
    {
        public static void OnSaving(
            IModHelper helper,
            IMonitor monitor,
            string saveKey,
            HashSet<long> knownFarmhandIds,
            object? s,
            SavingEventArgs e
        )
        {
            if (!Context.IsMainPlayer) return;
            helper.Data.WriteSaveData(saveKey, knownFarmhandIds);
        }
    }
}