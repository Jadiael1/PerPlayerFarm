using StardewModdingAPI;
using StardewValley;

namespace PerPlayerFarm.Helpers
{
    public static class MapHelpers
    {
        public static void EnsurePlayerMap(long uid, string? displayName, IModHelper helper, IMonitor monitor, HashSet<long> knownFarmhandIds)
        {
            string name = $"PPF_{uid}";
            var existing = Game1.getLocationFromName(name);

            string desiredDisplayName = string.IsNullOrWhiteSpace(displayName)
                ? helper.Translation.Get("ppf.map.farm_display_name_fallback", new { uid })
                : helper.Translation.Get("ppf.map.farm_display_name", new { name = displayName });

            if (existing is Farm existingFarm)
            {
                if (!string.Equals(existingFarm.DisplayName, desiredDisplayName, StringComparison.Ordinal))
                    existingFarm.DisplayName = desiredDisplayName;
                return;
            }

            var loc = new Farm($"Maps/{name}", name)
            {
                IsOutdoors = true,
                IsFarm = true
            };
            loc.isAlwaysActive.Value = true;
            loc.AddDefaultBuildings(true);
            loc.DisplayName = desiredDisplayName;
            Game1.locations.Add(loc);
            // var farm = Game1.getFarm();
            // string? farmMapAsset = farm?.mapPath?.Value;
            // monitor.Log($"1Farm map atual: {farmMapAsset}", LogLevel.Debug);
        }
    }
}