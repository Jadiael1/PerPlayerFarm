using StardewModdingAPI;
using StardewValley;

namespace PerPlayerFarm.Events.SaveLoaded
{
    public static class StripAllBuildingsDefault
    {

        private static bool IsGreenhouseUnlocked(Farm farm)
        {
            try
            {
                return farm?.greenhouseUnlocked?.Value == true;
            }
            catch
            {
                return true;
            }
        }

        private static void StripInFarm(Farm farm, IMonitor monitor, ITranslationHelper translate)
        {
            int removedHouse = 0;
            int removedGh = 0;

            // 1) remove SEMPRE a Farmhouse
            for (int i = farm.buildings.Count - 1; i >= 0; i--)
            {
                var b = farm.buildings[i];
                if ((b?.buildingType?.Value) == "Farmhouse")
                {
                    farm.buildings.RemoveAt(i);
                    removedHouse++;
                }
            }

            bool ghUnlocked = IsGreenhouseUnlocked(farm);
            if (!ghUnlocked)
            {
                for (int i = farm.buildings.Count - 1; i >= 0; i--)
                {
                    var b = farm.buildings[i];
                    if ((b?.buildingType?.Value) == "Greenhouse")
                    {
                        farm.buildings.RemoveAt(i);
                        removedGh++;
                    }
                }

                if (removedGh > 0)
                {
                    for (int i = farm.warps.Count - 1; i >= 0; i--)
                    {
                        if (string.Equals(farm.warps[i].TargetName, "Greenhouse", StringComparison.OrdinalIgnoreCase))
                            farm.warps.RemoveAt(i);
                    }
                }
            }

            if (removedHouse + removedGh > 0)
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.trace.strip_summary",
                    new { farm = farm.Name ?? string.Empty, farmhouses = removedHouse, greenhouses = removedGh, unlocked = ghUnlocked }
                ), LogLevel.Trace);
        }

        private static bool IsPpf(GameLocation loc)
            => (loc.NameOrUniqueName ?? loc.Name ?? string.Empty)
               .StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);

        private static void StripVanillaCabinsFromPpf(Farm farm)
        {
            int removed = 0;
            for (int i = farm.buildings.Count - 1; i >= 0; --i)
            {
                var b = farm.buildings[i];
                if (string.Equals(b.buildingType.Value, "Cabin", StringComparison.Ordinal))
                {
                    farm.buildings.RemoveAt(i);
                    removed++;
                }
            }
        }

        public static void Strip(IMonitor monitor, ITranslationHelper translate)
        {
            if (!Context.IsWorldReady || !Context.IsMainPlayer)
                return;

            foreach (var loc in Game1.locations)
            {
                if (loc is Farm farm && IsPpf(farm))
                {
                    StripInFarm(farm, monitor, translate);
                    StripVanillaCabinsFromPpf(farm);
                }
            }
        }
    }
}
