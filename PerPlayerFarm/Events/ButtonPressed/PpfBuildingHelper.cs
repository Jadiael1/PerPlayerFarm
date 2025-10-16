using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;

namespace PerPlayerFarm.Events.ButtonPressed
{
    internal static class PpfBuildingHelper
    {
        internal const string _facadeOwnerUid = Utils.Constants.FacadeOwnerUid;

        internal static bool TryGetOwnerUid(Building building, GameLocation location, out long ownerUid)
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

        internal static Point GetMailboxTile(Building building)
        {
            var door = building.humanDoor.Value;
            int mailboxX = building.tileX.Value + door.X + 2;
            int mailboxY = building.tileY.Value + door.Y + 1;
            return new Point(mailboxX, mailboxY);
        }
    }
}
