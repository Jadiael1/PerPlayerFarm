using PerPlayerFarm.Events.DayStarted;
using PerPlayerFarm.Events.SaveLoaded;
using StardewModdingAPI;
using StardewValley;


namespace PerPlayerFarm.Utils
{
    internal static class PpfConsoleCommands
    {
        internal static void Register(IModHelper helper, IMonitor monitor)
        {
            var cmd = helper.ConsoleCommands;

            // guarantees teleporters
            cmd.Add(
                "ppf.ensure-teleports",
                "Secures PPF teleporters.\n" +
                "Use: ppf.ensure-teleports all|here|farm|ppf|<LocationName>\n" +
                "  all  = all PPF_* and Farm\n" +
                "  here = only in the current location\n" +
                "  farm = only on the main Farm\n" +
                "  ppf  = only in PPF_* (all)\n" +
                "  <LocationName> = exact name of a location",
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log("World is not ready.", LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log("Only the host can guarantee teleporters.", LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "all";
                    switch (mode.ToLowerInvariant())
                    {
                        case "all":
                            TeleportItem.Initializer(monitor);
                            monitor.Log("[PPF] Guaranteed teleporters on all PPF_* and Farm.", LogLevel.Info);
                            break;

                        case "here":
                            {
                                var loc = Game1.currentLocation;
                                if (loc == null) { monitor.Log("Current location is null.", LogLevel.Warn); return; }
                                TeleportItem.EnsureIn(loc, monitor);
                                monitor.Log($"[PPF] Guaranteed teleporter in {loc.NameOrUniqueName ?? loc.Name}.", LogLevel.Info);
                                break;
                            }

                        case "farm":
                            {
                                var farm = Game1.getLocationFromName("Farm");
                                if (farm == null) { monitor.Log("Farm not found.", LogLevel.Warn); return; }
                                TeleportItem.EnsureIn(farm, monitor);
                                monitor.Log("[PPF] Guaranteed teleporter on Farm.", LogLevel.Info);
                                break;
                            }

                        case "ppf":
                            {
                                int count = 0;
                                foreach (var loc in Game1.locations.Where(l => (l.NameOrUniqueName ?? l.Name ?? "").StartsWith("PPF_", StringComparison.OrdinalIgnoreCase)))
                                {
                                    TeleportItem.EnsureIn(loc, monitor);
                                    count++;
                                }
                                monitor.Log($"[PPF] Guaranteed teleporters in {count} PPF_*.", LogLevel.Info);
                                break;
                            }

                        default:
                            {
                                string target = string.Join(' ', args);
                                var loc = Game1.getLocationFromName(target);
                                if (loc == null) { monitor.Log($"Location '{target}' not found.", LogLevel.Warn); return; }
                                TeleportItem.EnsureIn(loc, monitor);
                                monitor.Log($"[PPF] Guaranteed teleporter in{loc.NameOrUniqueName ?? loc.Name}.", LogLevel.Info);
                                break;
                            }
                    }
                }
            );

            // light cleaning (objects/TF)
            cmd.Add(
                "ppf.clean",
                "Cleans debris/grass/trees/farm resources (Ppf Clean Helper).\n" +
                "Use: ppf.clean here|all|ppf",
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log("World is not ready.", LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log("Only the host can clean.", LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "here";
                    switch (mode.ToLowerInvariant())
                    {
                        case "here":
                            {
                                if (Game1.currentLocation is Farm f)
                                {
                                    PlayerDataInitializer.CleanLocation(f);
                                    monitor.Log($"[PPF] Cleaning applied in {f.NameOrUniqueName ?? f.Name}.", LogLevel.Info);
                                }
                                else monitor.Log("Current location is not Farm.", LogLevel.Warn);
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
                                monitor.Log($"[PPF] Cleaning applied to {n} Farms (includes main Farm and PPF_*).", LogLevel.Info);
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
                                monitor.Log($"[PPF] Cleaning applied to {n} PPF_*.", LogLevel.Info);
                                break;
                            }

                        default:
                            monitor.Log("Uso: ppf.clean here|all|ppf", LogLevel.Info);
                            break;
                    }
                }
            );

            // stripper: always removes Farmhouse and Greenhouse if broken, in PPF_*
            cmd.Add(
                "ppf.strip",
                "Removes vanilla buildings in PPF_* (Farmhouse always, Greenhouse if broken).\n" +
                "Use: ppf.strip here|all",
                (name, args) =>
                {
                    if (!Context.IsWorldReady) { monitor.Log("World is not ready.", LogLevel.Warn); return; }
                    if (!Context.IsMainPlayer) { monitor.Log("Only the host can change buildings.", LogLevel.Warn); return; }

                    string mode = args.Length > 0 ? args[0] : "here";
                    switch (mode.ToLowerInvariant())
                    {
                        case "here":
                            {
                                if (Game1.currentLocation is Farm f && IsPpf(f))
                                {
                                    StripOnce(f, monitor);
                                }
                                else monitor.Log("A location atual não é uma PPF_* do tipo Farm.", LogLevel.Warn);
                                break;
                            }

                        case "all":
                            {
                                int n = 0;
                                foreach (var f in Game1.locations.OfType<Farm>()
                                         .Where(l => IsPpf(l)))
                                {
                                    StripOnce(f, monitor);
                                    n++;
                                }
                                monitor.Log($"[PPF] Strip aplicado em {n} PPF_*.", LogLevel.Info);
                                break;
                            }

                        default:
                            monitor.Log("Uso: ppf.strip here|all", LogLevel.Info);
                            break;
                    }

                    static bool IsPpf(GameLocation loc) =>
                        (loc.NameOrUniqueName ?? loc.Name ?? string.Empty).StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);

                    static void StripOnce(Farm farm, IMonitor mon)
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

                        mon.Log($"[PPF] {farm.Name}: strip Farmhouse={removedHouse}, Greenhouse={removedGh} (unlocked={ghUnlocked}).", LogLevel.Info);
                    }
                }
            );
        }
    }
}
