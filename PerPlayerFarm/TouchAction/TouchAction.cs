using Microsoft.Xna.Framework;
using PerPlayerFarm.Utils;
using PerPLayerFarm.Types;
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
    }
}
