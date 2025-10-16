using Microsoft.Xna.Framework;
using PerPlayerFarm.Utils;
using PerPLayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.UpdateTicked
{
    public class UpdateTicked
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private const string _enterFarmsActionKey = Utils.Constants.EnterFarmsActionKey;
        private Point? _lastTile;

        public UpdateTicked(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Farmer farmer = Game1.player;
            Point currentTile = farmer.TilePoint;

            if (_lastTile.HasValue && _lastTile.Value == currentTile)
                return;

            _lastTile = currentTile;

            GameLocation? location = farmer.currentLocation;
            if (location is null)
                return;

            if (location.Name is "BusStop" or "FarmCave" or "Backwoods" or "Forest")
            {
                bool hasMapProp = location.map.Properties.TryGetValue(_enterFarmsActionKey, out var mapProp);
                if (!hasMapProp)
                {
                    _monitor.Log(_translate.Get("derexsv.ppf.log.warn.map_without_data"), LogLevel.Warn);
                    return;
                }

                List<WarpLocations>? warpsToReplace = ListHelper.ConvertStringForList(mapProp, _monitor, _translate);
                if (warpsToReplace is null)
                {
                    return;
                }

                for (int i = location.warps.Count - 1; i >= 0; i--)
                {
                    bool isWarpTile = warpsToReplace.Any(wtr => wtr.X == location.warps[i].X);
                    if (isWarpTile)
                    {
                        location.warps.Remove(location.warps[i]);
                    }
                }

                string? touchData = location.doesTileHaveProperty(currentTile.X, currentTile.Y, _enterFarmsActionKey, "Back");
                if (string.IsNullOrEmpty(touchData))
                    return;

                List<WarpLocations>? warps = ListHelper.ConvertStringForList(touchData, _monitor, _translate);
                if (warps is null)
                {
                    _monitor.Log(_translate.Get(
                        "derexsv.ppf.log.warn.warp_data_error",
                        new { x = currentTile.X, y = currentTile.Y }
                    ), LogLevel.Warn);
                    return;
                }

                string targetLocationName = $"PPF_{farmer.UniqueMultiplayerID}";
                GameLocation? targetLocation = Game1.getLocationFromName(targetLocationName);
                if (targetLocation is null && !Context.IsMainPlayer)
                {
                    _monitor.Log(_translate.Get(
                        "derexsv.ppf.log.warn.warp_destination_missing",
                        new { location = targetLocationName, x = currentTile.X, y = currentTile.Y }
                    ), LogLevel.Warn);
                    return;
                }

                if (Context.IsMainPlayer)
                {
                    Game1.warpFarmer(warps[0].TargetName, warps[0].TargetX, warps[0].TargetY, flip: false);
                }
                else
                {
                    Game1.warpFarmer(targetLocationName, warps[0].TargetX, warps[0].TargetY, flip: false);
                }
            }
        }

    }
}
