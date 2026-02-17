using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace PerPlayerFarm.Events.PeerConnected
{
    public static class HouseWarpUtils
    {
        private const string _facadeOwnerUid = Utils.Constants.FacadeOwnerUid;
        private const string _facadeBuildingId = Utils.Constants.FacadeBuildingId;

        private static bool TryGetOwnerUid(Building building, GameLocation location, out long ownerUid)
        {
            ownerUid = 0;
            if (building.modData.TryGetValue(_facadeOwnerUid, out string? uidText) &&
                long.TryParse(uidText, NumberStyles.Integer, CultureInfo.InvariantCulture, out ownerUid))
            {
                return true;
            }

            string? locName = location.NameOrUniqueName;
            if (locName is not null && locName.StartsWith("PPF_", StringComparison.Ordinal) &&
                long.TryParse(locName.Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out ownerUid))
            {
                return true;
            }

            ownerUid = 0;
            return false;
        }

        public static void OverrideDefaultHouseWarpToPPF(long ownerUid)
        {
            Farmer? farmer = Game1.getAllFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == ownerUid);
            if (farmer is null || farmer.IsMainPlayer)
                return;

            if (Utility.getHomeOfFarmer(farmer) is not Cabin home)
                return;

            string locName = $"PPF_{farmer.UniqueMultiplayerID}";
            GameLocation? targetLocation = Game1.getLocationFromName(locName);
            if (targetLocation is null)
                return;

            Building? facade = targetLocation.buildings
                .FirstOrDefault(b => b is not null &&
                                     b.buildingType?.Value == _facadeBuildingId &&
                                     TryGetOwnerUid(b, targetLocation, out long facadeOwner) &&
                                     facadeOwner == farmer.UniqueMultiplayerID);
            if (facade is null)
                return;

            Point doorOffset = facade.humanDoor.Value;
            int exitX = facade.tileX.Value + doorOffset.X;
            int exitY = facade.tileY.Value + doorOffset.Y + 1;

            string targetName = targetLocation.NameOrUniqueName;

            var entry = home.getEntryLocation();
            Warp? entranceWarp = home.warps.FirstOrDefault(w => w.X == entry.X && w.Y == entry.Y) ?? home.warps.FirstOrDefault();;

            // Warp? entranceWarp = home.warps.FirstOrDefault();
            if (entranceWarp is null)
            {
                entranceWarp = new Warp(entry.X, entry.Y, targetName, exitX, exitY, false);
                home.warps.Add(entranceWarp);
            }
            else
            {
                entranceWarp.TargetName = targetName;
                entranceWarp.TargetX = exitX;
                entranceWarp.TargetY = exitY;
            }
        }

        public static void RestoreDefaultHouseWarp(long ownerUid)
        {
            Farmer? farmer = Game1.getAllFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == ownerUid);
            if (farmer is null || farmer.IsMainPlayer)
                return;

            if (Utility.getHomeOfFarmer(farmer) is not Cabin home)
                return;

            if (Game1.getLocationFromName("Farm") is not Farm farm)
                return;

            Building? cabin = farm.buildings.FirstOrDefault(b => ReferenceEquals(b.indoors.Value, home));
            if (cabin is null)
                return;

            string targetName = farm.NameOrUniqueName ?? farm.Name ?? "Farm";
            Point doorOffset = cabin.humanDoor.Value;
            int exitX = cabin.tileX.Value + doorOffset.X;
            int exitY = cabin.tileY.Value + doorOffset.Y + 1;

            var entry = home.getEntryLocation();
            Warp? entranceWarp = home.warps.FirstOrDefault(w => w.X == entry.X && w.Y == entry.Y) ?? home.warps.FirstOrDefault();
            if (entranceWarp is null)
            {
                entranceWarp = new Warp(entry.X, entry.Y, targetName, exitX, exitY, false);
                home.warps.Add(entranceWarp);
            }
            else
            {
                entranceWarp.TargetName = targetName;
                entranceWarp.TargetX = exitX;
                entranceWarp.TargetY = exitY;
            }
        }

        public static int RestoreAllDefaultHouseWarps()
        {
            int processed = 0;
            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer is null || farmer.IsMainPlayer)
                    continue;

                RestoreDefaultHouseWarp(farmer.UniqueMultiplayerID);
                processed++;
            }

            return processed;
        }
    }
}
