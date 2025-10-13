using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace PerPlayerFarm.Events.DayStarted
{
    public static class PlayerDataInitializer
    {

        private static readonly string _cleanKey = Utils.Constants.CleanKey;

        public static void ClearPpfIfFirstInit(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            var eligible = Utils.EligibleFarmers.Get(helper);

            if (eligible is not null)
            {
                foreach (long uid in eligible)
                {
                    var farmer = Game1.GetPlayer(uid);
                    if (farmer is not null && !farmer.IsMainPlayer)
                    {
                        string locName = $"PPF_{uid}";
                        var loc = Game1.getLocationFromName(locName);
                        if (loc is null)
                        {
                            continue;
                        }
                        Farm farmLoc = (Farm)loc;
                        CleanIfFirstInit(farmLoc, monitor, translate);
                    }
                }
            }
        }

        private static void CleanIfFirstInit(Farm loc, IMonitor monitor, ITranslationHelper translate)
        {
            if (!Context.IsMainPlayer || loc is null)
                return;

            if (loc.modData.TryGetValue(_cleanKey, out var v) && v == "1")
            {
                monitor.Log(translate.Get(
                    "derexsv.ppf.log.trace.cleaning_already_applied",
                    new { location = loc.Name ?? string.Empty }
                ), LogLevel.Trace);
                return;
            }

            CleanLocation(loc);
            loc.modData[_cleanKey] = "1";
            monitor.Log(translate.Get(
                "derexsv.ppf.log.trace.cleaning_applied",
                new { location = loc.Name ?? string.Empty }
            ), LogLevel.Trace);
        }

        public static void CleanLocation(Farm loc)
        {
            /*
                Weeds, Ervas daninhas, Weeds = 675, 674, 784
                Stone, Pedra, Stone = 450, 343
                Twig, Galho, Twig = 294, 295

            */
            var objectIdsToRemove = new HashSet<string> { "675", "450", "294", "295", "674", "784", "343" };
            var objectKeysToRemove = new List<Vector2>();

            foreach (var layer in loc.objects)
            {
                foreach (var kv in layer)
                {
                    var obj = kv.Value;
                    if (obj != null && obj.ItemId != null && objectIdsToRemove.Contains(obj.ItemId))
                    {
                        objectKeysToRemove.Add(kv.Key);
                    }
                }
            }
            foreach (var key in objectKeysToRemove.Distinct())
            {
                loc.objects.Remove(key);
            }
            // loc.objects.Clear();

            var terrainFeaturesIdsToRemove = new HashSet<string> { "Tree", "Grass" };
            var terrainFeaturesKeysToRemove = new List<Vector2>();
            foreach (var layer in loc.terrainFeatures)
            {
                foreach (var kv in layer)
                {
                    var tf = kv.Value;
                    if (tf == null) continue;
                    var name = tf.GetType().Name;
                    if (terrainFeaturesIdsToRemove.Contains(name))
                    {
                        terrainFeaturesKeysToRemove.Add(kv.Key);
                    }
                }
            }
            foreach (var pos in terrainFeaturesKeysToRemove.Distinct())
            {
                loc.terrainFeatures.Remove(pos);
            }
            // loc.terrainFeatures.Clear();

            // loc.largeTerrainFeatures?.Clear();

            if (loc is Farm farm)
            {
                farm.resourceClumps.Clear();
            }
        }
    }
}
