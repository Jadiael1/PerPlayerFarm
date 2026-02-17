using Microsoft.Xna.Framework;
using PerPlayerFarm.Config;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace PerPlayerFarm.Helpers
{
    public static class WarpHelper
    {
        public static void UpdateHomeExitWarp(IMonitor monitor, long uid)
        {
            if (!Context.IsWorldReady || !Context.IsMainPlayer) return;

            Farmer? farmer = Game1.GetPlayer(uid);

            if (farmer is null || farmer.IsMainPlayer) return;

            if (Utility.getHomeOfFarmer(farmer) is not Cabin home) return;

            string locName = $"PPF_{farmer.UniqueMultiplayerID}";
            GameLocation? targetLocation = Game1.getLocationFromName(locName);
            if (targetLocation is null) return;

            Building? farmHouse = targetLocation.buildings
                .FirstOrDefault(b => string.Equals(b.buildingType?.Value, "Farmhouse", StringComparison.OrdinalIgnoreCase));

            if (farmHouse is null) return;

            Point doorOffset = farmHouse.getPointForHumanDoor();
            int exitX = doorOffset.X;
            int exitY = doorOffset.Y + 1;

            string targetName = targetLocation.NameOrUniqueName;

            IEnumerable<Warp>? entranceWarps = home.warps.Where(w => string.Equals(w.TargetName, "Farm", StringComparison.OrdinalIgnoreCase) || string.Equals(w.TargetName, targetName, StringComparison.OrdinalIgnoreCase));

            if (entranceWarps is null)
            {
                //Log
                monitor.Log($"For some unknown reason, this house has no exit to the farm.", LogLevel.Info);
                return;
            }

            foreach (var entranceWarp in entranceWarps)
            {
                entranceWarp.TargetName = targetName;
                entranceWarp.TargetX = exitX;
                entranceWarp.TargetY = exitY;
            }
        }

        public static void SetupFarmHousePPFDoorWarp(
            IModHelper helper,
            IMonitor monitor,
            object? s,
            ButtonPressedEventArgs e
        )
        {
            if (Game1.currentLocation is not Farm farm)
                return;

            var locName = farm.NameOrUniqueName;
            if (locName is null || !locName.StartsWith("PPF_", StringComparison.Ordinal))
                return;

            var cursor = e.Cursor ?? helper.Input.GetCursorPosition();
            Point targetTile = cursor.GrabTile.ToPoint();

            Building? house = farm.buildings.FirstOrDefault(b => string.Equals(b.buildingType?.Value, "Farmhouse", StringComparison.OrdinalIgnoreCase));
            if (house is null)
                return;

            Point doorTile = house.getPointForHumanDoor();

            // prevents action at a distance greater than 2
            // if (!(Math.Abs(targetTile.X - doorTile.X) <= 2 && Math.Abs(targetTile.Y - doorTile.Y) <= 2))
            if (Math.Abs(targetTile.X - doorTile.X) + Math.Abs(targetTile.Y - doorTile.Y) > 2)
                return;

            // Logic to obtain the interior of the current PPF owner's house.
            if (!long.TryParse(locName.AsSpan("PPF_".Length), out long ownerId)) return;

            var ppfFarmer = Game1.getAllFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == ownerId) ?? Game1.GetPlayer(ownerId);
            if (ppfFarmer is null) return;

            if (Game1.getLocationFromName("Farm") is not Farm mainFarm) return;
            var cabin = mainFarm.buildings
                .Select(b => b.GetIndoors() as Cabin)
                .FirstOrDefault(c => c != null && c.HasOwner && c.OwnerId == ownerId);
            if (cabin is null) return;

            helper.Input.Suppress(e.Button);
            // Warp
            Game1.warpFarmer(cabin.NameOrUniqueName, 2, 9, false);
        }


        public static List<WarpLocations>? ParseWarpString(string inputString, int groupSize = 5)
        {
            try
            {
                var elements = inputString
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

                if (elements.Count % groupSize != 0)
                {
                    return null;
                }

                var locationList = elements
                    .Select((element, index) => new { element, index })
                    .GroupBy(x => x.index / groupSize)
                    .Select(group => group.Select(x => x.element).ToArray())
                    .Select(groupOfFive =>
                    {
                        return new WarpLocations
                        {
                            X = int.TryParse(groupOfFive[0], out int x) ? x : 0,
                            Y = int.TryParse(groupOfFive[1], out int y) ? y : 0,
                            TargetName = groupOfFive[2],
                            TargetX = int.TryParse(groupOfFive[3], out int targetX) ? targetX : 0,
                            TargetY = int.TryParse(groupOfFive[4], out int targetY) ? targetY : 0
                        };
                    })
                    .ToList();
                return locationList;
            }
            catch
            {
                return null;
            }
        }

        public static string BuildWarpString(List<WarpLocations> list)
        {
            var stringSegments = list.Select(loc => $"{loc.X} {loc.Y} {loc.TargetName} {loc.TargetX} {loc.TargetY}");
            return string.Join(" ", stringSegments);
        }

        public static void WarpToCorrectFarm(
            IModHelper helper,
            IMonitor monitor,
            string enterFarmsTouchActionKey,
            ModConfig config,
            GameLocation location,
            string[] args,
            Farmer who,
            Vector2 tile
        )
        {
            if (!config.RedirectFarmhandsToPPF || args.Length != 6 || !string.Equals(args[0], enterFarmsTouchActionKey, StringComparison.Ordinal))
                return;

            if (!int.TryParse(args[1], out int warpSourceX)) return;
            if (!int.TryParse(args[2], out int warpSourceY)) return;
            string warpTargetName = args[3];
            if (String.IsNullOrEmpty(warpTargetName)) return;
            if (!int.TryParse(args[4], out int warpTargetX)) return;
            if (!int.TryParse(args[5], out int warpTargetY)) return;

            if (Context.IsMainPlayer)
            {
                Game1.warpFarmer(warpTargetName, warpTargetX, warpTargetY, flip: false);
                return;
            }

            string locName = $"PPF_{who.UniqueMultiplayerID}";
            GameLocation? targetLocation = Game1.getLocationFromName(locName);
            if (targetLocation is null) return;

            Game1.warpFarmer(targetLocation.NameOrUniqueName, warpTargetX, warpTargetY, flip: false);
        }


        public static void InvalidateFarmEntranceMaps(IModHelper helper)
        {
            helper.GameContent.InvalidateCache("Maps/BusStop");
            helper.GameContent.InvalidateCache("Maps/FarmCave");
            helper.GameContent.InvalidateCache("Maps/Backwoods");
            helper.GameContent.InvalidateCache("Maps/Forest");
        }

    }
}