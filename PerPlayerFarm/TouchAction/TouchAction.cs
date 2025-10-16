using Microsoft.Xna.Framework;
using PerPlayerFarm.Utils;
using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.TouchAction
{
    public class TouchAction
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private const string _enterFarmsActionKey = Utils.Constants.EnterFarmsActionKey;

        public TouchAction(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            GameLocation.RegisterTouchAction(_enterFarmsActionKey, this.OnTouchAction);
            GameLocation.RegisterTileAction("PPF_Door", this.OnPpfDoor);
        }

        private void OnTouchAction(GameLocation location, string[] args, Farmer player, Vector2 tile)
        {
            if (args.Length != 6 || args[0] != _enterFarmsActionKey) return;
            // if (!int.TryParse(args[1], out int x) || !int.TryParse(args[2], out int y)) return;
            if (!int.TryParse(args[4], out int targetX) || !int.TryParse(args[5], out int targetY)) return;

            string target = player.IsMainPlayer ? "Farm" : $"PPF_{player.UniqueMultiplayerID}";
            var src = player.TilePoint;
            var w = new StardewValley.Warp(src.X, src.Y, target, targetX, targetY, flipFarmer: false);
            player.warpFarmer(w, player.FacingDirection);
        }

        private bool OnPpfDoor(GameLocation location, string[] args, Farmer player, Microsoft.Xna.Framework.Point tile)
        {
            try
            {
                if (location == null || player == null)
                    return false;

                // encontre a fachada cujo "humanDoor" esteja exatamente neste tile
                foreach (var building in location.buildings)
                {
                    if (building?.buildingType?.Value != Utils.Constants.FacadeBuildingId)
                        continue;

                    // confirma o dono da fachada
                    if (!Events.ButtonPressed.PpfBuildingHelper.TryGetOwnerUid(building, location, out long ownerUid))
                        continue;

                    // tile da porta desta fachada
                    var door = building.humanDoor.Value;
                    int doorX = building.tileX.Value + door.X;
                    int doorY = building.tileY.Value + door.Y;

                    // só reage se estivermos na porta desta fachada
                    if (tile.X != doorX || tile.Y != doorY)
                        continue;

                    // --- casa do DONO ---
                    Farmer? owner = Game1.GetPlayer(ownerUid);
                    if (owner is null)
                        return false;

                    var ownerHome = Utility.getHomeOfFarmer(owner) as StardewValley.Locations.FarmHouse;
                    if (ownerHome == null)
                        return false;

                    string ownerHomeName = owner.homeLocation?.Value
                                           ?? ownerHome.NameOrUniqueName
                                           ?? ownerHome.Name
                                           ?? string.Empty;
                    if (string.IsNullOrEmpty(ownerHomeName))
                        return false;

                    var entry = ownerHome.getEntryLocation();
                    if (entry.X < 0 || entry.Y < 0)
                        entry = ownerHome.GetPlayerBedSpot();
                    if (entry.X < 0 || entry.Y < 0)
                        entry = new Microsoft.Xna.Framework.Point(1, 1);

                    // warp o jogador QUE CLICOU para a casa do DONO
                    var src = player.TilePoint;
                    var w = new StardewValley.Warp(src.X, src.Y, ownerHomeName, entry.X, entry.Y, flipFarmer: false);
                    player.warpFarmer(w, player.FacingDirection);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.warn.ppf_door_failed",
                    new { error = ex.ToString() }
                ), LogLevel.Warn);
                return false;
            }
        }
    }
}
