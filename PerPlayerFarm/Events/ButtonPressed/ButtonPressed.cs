using Microsoft.Xna.Framework;
using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;

namespace PerPlayerFarm.Events.ButtonPressed
{
    public class ButtonPressed
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private const string _facadeBuildingId = Utils.Constants.FacadeBuildingId;
        private readonly string _modManifest;
        private readonly PpfTravelMenuManager _travelMenuManager;
        private readonly List<PpfFarmEntry> _clientRegistry;
        private readonly HashSet<string> _clientStubs;
        public ButtonPressed(IModHelper helper, IMonitor monitor, string modManifest, List<PpfFarmEntry> clientRegistry, HashSet<string> clientStubs)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _modManifest = modManifest;
            _clientRegistry = clientRegistry;
            _clientStubs = clientStubs;
            _travelMenuManager = new PpfTravelMenuManager(helper, monitor, modManifest, clientRegistry, clientStubs);
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }
        public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton())
                return;

            if (_travelMenuManager.TryHandleTeleportItem(e))
                return;

            GameLocation? location = Game1.currentLocation;
            if (location?.buildings is null || location.buildings.Count == 0)
                return;

            var grabTile = Game1.player.GetGrabTile();
            int targetX = (int)grabTile.X;
            int targetY = (int)grabTile.Y;

            foreach (var building in location.buildings)
            {
                if (building?.buildingType?.Value != _facadeBuildingId)
                    continue;

                if (!PpfBuildingHelper.TryGetOwnerUid(building, location, out long ownerUid))
                    continue;

                // var doorOffset = building.humanDoor.Value;
                // int doorX = building.tileX.Value + doorOffset.X;
                // int doorY = building.tileY.Value + doorOffset.Y;

                Point mailboxTile = PpfBuildingHelper.GetMailboxTile(building);

                // bool targetIsDoor = targetX == doorX && targetY == doorY;
                bool targetIsMailbox = targetX == mailboxTile.X && targetY == mailboxTile.Y;


                /*
                if (targetIsDoor)
                {
                    Farmer? owner = Game1.GetPlayer(ownerUid);
                    if (owner is null)
                        continue;

                    string homeName = owner.homeLocation.Value;
                    if (string.IsNullOrEmpty(homeName))
                        continue;

                    GameLocation? homeLocation = Game1.getLocationFromName(homeName);
                    if (homeLocation is not FarmHouse farmHouse)
                        continue;

                    Point entry = farmHouse.getEntryLocation();
                    if (entry.X < 0 || entry.Y < 0)
                        entry = farmHouse.GetPlayerBedSpot();

                    if (entry.X < 0 || entry.Y < 0)
                        entry = new Point(1, 1);

                    Game1.warpFarmer(homeName, entry.X, entry.Y, false);
                    _helper.Input.Suppress(e.Button);
                    return;
                }
                */

                if (targetIsMailbox)
                {
                    if (Game1.player.UniqueMultiplayerID != ownerUid)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
                        _helper.Input.Suppress(e.Button);
                        return;
                    }

                    var tileLocation = new Location(targetX, targetY);
                    location.performAction("Mailbox", Game1.player, tileLocation);
                    _helper.Input.Suppress(e.Button);
                    return;
                }
            }
        }
    }
}