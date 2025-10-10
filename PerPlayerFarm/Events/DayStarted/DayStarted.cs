using PerPlayerFarm.Events.SaveLoaded;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.DayStarted
{
    public class DayStarted
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;

        public DayStarted(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

        }
        public void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            PlayerDataInitializer.ClearPpfIfFirstInit(_monitor, _translate);
            if (!Context.IsMainPlayer)
            {
                SaveLoaded.Locations.LoadPpfFarmsForInvited(_monitor, _translate);
            }
            else
            {
                foreach (var farmer in Game1.getAllFarmers())
                {
                    PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(farmer.UniqueMultiplayerID);
                }
                TeleportItem.Initializer(_monitor, _translate);
                TeleportItem.OnDayStartedRetag(sender, e, _monitor, _translate);
            }

            StripAllBuildingsDefault.Strip(_monitor, _translate);
        }
    }
}
