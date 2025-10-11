using PerPlayerFarm.Configuration;
using PerPlayerFarm.Events.DayStarted;
using PerPlayerFarm.Events.SaveLoaded;
using StardewModdingAPI;
using StardewValley;


namespace PerPlayerFarm.Utils
{
    internal static class PpfConsoleCommands
    {
        internal static void Register(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            var cmd = helper.ConsoleCommands;
            var translate = helper.Translation;

            // guarantees teleporters
            cmd.Add(
                "ppf.ensure-teleports",
                translate.Get("derexsv.ppf.command.ensure_teleports.description"),
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log(translate.Get("derexsv.ppf.log.warn.world_not_ready"), LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log(translate.Get("derexsv.ppf.log.warn.host_only_teleporters"), LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "all";
                    switch (mode.ToLowerInvariant())
                    {
                        case "all":
                            TeleportItem.Initializer(monitor, translate, config);
                            monitor.Log(translate.Get("derexsv.ppf.log.info.teleporters_all"), LogLevel.Info);
                            break;

                        case "here":
                            {
                                var loc = Game1.currentLocation;
                                if (loc == null) { monitor.Log(translate.Get("derexsv.ppf.log.warn.current_location_null"), LogLevel.Warn); return; }
                                TeleportItem.EnsureIn(loc, monitor, translate, config);
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.teleporter_location",
                                    new { location = loc.NameOrUniqueName ?? loc.Name ?? string.Empty }
                                ), LogLevel.Info);
                                break;
                            }

                        case "farm":
                            {
                                var farm = Game1.getLocationFromName("Farm");
                                if (farm == null) { monitor.Log(translate.Get("derexsv.ppf.log.warn.farm_not_found"), LogLevel.Warn); return; }
                                TeleportItem.EnsureIn(farm, monitor, translate, config);
                                monitor.Log(translate.Get("derexsv.ppf.log.info.teleporter_farm"), LogLevel.Info);
                                break;
                            }

                        case "ppf":
                            {
                                int count = 0;
                                foreach (var loc in Game1.locations.Where(l => (l.NameOrUniqueName ?? l.Name ?? "").StartsWith("PPF_", StringComparison.OrdinalIgnoreCase)))
                                {
                                    TeleportItem.EnsureIn(loc, monitor, translate, config);
                                    count++;
                                }
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.teleporters_ppf_count",
                                    new { count }
                                ), LogLevel.Info);
                                break;
                            }

                        default:
                            {
                                string target = string.Join(' ', args);
                                var loc = Game1.getLocationFromName(target);
                                if (loc == null)
                                {
                                    monitor.Log(translate.Get(
                                        "derexsv.ppf.log.warn.location_not_found",
                                        new { location = target }
                                    ), LogLevel.Warn);
                                    return;
                                }
                                TeleportItem.EnsureIn(loc, monitor, translate, config);
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.teleporter_location",
                                    new { location = loc.NameOrUniqueName ?? loc.Name ?? string.Empty }
                                ), LogLevel.Info);
                                break;
                            }
                    }
                }
            );

            // light cleaning (objects/TF)
            cmd.Add(
                "ppf.clean",
                translate.Get("derexsv.ppf.command.clean.description"),
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log(translate.Get("derexsv.ppf.log.warn.world_not_ready"), LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log(translate.Get("derexsv.ppf.log.warn.host_only_clean"), LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "here";
                    switch (mode.ToLowerInvariant())
                    {
                        case "here":
                            {
                                if (Game1.currentLocation is Farm f)
                                {
                                    PlayerDataInitializer.CleanLocation(f);
                                    monitor.Log(translate.Get(
                                        "derexsv.ppf.log.info.clean_location",
                                        new { location = f.NameOrUniqueName ?? f.Name ?? string.Empty }
                                    ), LogLevel.Info);
                                }
                                else monitor.Log(translate.Get("derexsv.ppf.log.warn.current_location_not_farm"), LogLevel.Warn);
                                break;
                            }

                        case "all":
                            {
                                int n = 0;
                                foreach (var f in Game1.locations.OfType<Farm>())
                                {
                                    PlayerDataInitializer.CleanLocation(f);
                                    n++;
                                }
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.clean_farm_count",
                                    new { count = n }
                                ), LogLevel.Info);
                                break;
                            }

                        case "ppf":
                            {
                                int n = 0;
                                foreach (var f in Game1.locations.OfType<Farm>()
                                         .Where(l => (l.NameOrUniqueName ?? l.Name ?? "").StartsWith("PPF_", StringComparison.OrdinalIgnoreCase)))
                                {
                                    PlayerDataInitializer.CleanLocation(f);
                                    n++;
                                }
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.clean_ppf_count",
                                    new { count = n }
                                ), LogLevel.Info);
                                break;
                            }

                        default:
                            monitor.Log(translate.Get("derexsv.ppf.log.info.clean_usage"), LogLevel.Info);
                            break;
                    }
                }
            );

            // stripper: always removes Farmhouse and Greenhouse if broken, in PPF_*
            cmd.Add(
                "ppf.strip",
                translate.Get("derexsv.ppf.command.strip.description"),
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log(translate.Get("derexsv.ppf.log.warn.world_not_ready"), LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log(translate.Get("derexsv.ppf.log.warn.host_only_buildings"), LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "here";
                    switch (mode.ToLowerInvariant())
                    {
                        case "here":
                            {
                                if (Game1.currentLocation is Farm f && IsPpf(f))
                                {
                                    StripOnce(f, monitor, translate);
                                }
                                else monitor.Log(translate.Get("derexsv.ppf.log.warn.current_location_not_ppf"), LogLevel.Warn);
                                break;
                            }

                        case "all":
                            {
                                int n = 0;
                                foreach (var f in Game1.locations.OfType<Farm>()
                                         .Where(l => IsPpf(l)))
                                {
                                    StripOnce(f, monitor, translate);
                                    n++;
                                }
                                monitor.Log(translate.Get(
                                    "derexsv.ppf.log.info.strip_ppf_count",
                                    new { count = n }
                                ), LogLevel.Info);
                                break;
                            }

                        default:
                            monitor.Log(translate.Get("derexsv.ppf.log.info.strip_usage"), LogLevel.Info);
                            break;
                    }

                    static bool IsPpf(GameLocation loc) =>
                        (loc.NameOrUniqueName ?? loc.Name ?? string.Empty).StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);

                    static void StripOnce(Farm farm, IMonitor mon, ITranslationHelper translate)
                    {
                        int removedHouse = 0, removedGh = 0;

                        for (int i = farm.buildings.Count - 1; i >= 0; i--)
                        {
                            var b = farm.buildings[i];
                            if ((b?.buildingType?.Value) == "Farmhouse")
                            {
                                farm.buildings.RemoveAt(i);
                                removedHouse++;
                            }
                        }

                        bool ghUnlocked = false;
                        try { ghUnlocked = farm.greenhouseUnlocked?.Value == true; } catch { ghUnlocked = true; }
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

                        mon.Log(translate.Get(
                            "derexsv.ppf.log.info.strip_summary",
                            new { farm = farm.Name ?? string.Empty, farmhouses = removedHouse, greenhouses = removedGh, unlocked = ghUnlocked }
                        ), LogLevel.Info);
                    }
                }
            );
        }
    }
}
