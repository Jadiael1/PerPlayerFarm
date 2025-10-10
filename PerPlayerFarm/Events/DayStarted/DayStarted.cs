using PerPlayerFarm.Configuration;
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
        private readonly ModConfig _config;

        public DayStarted(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _config = config;
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
                TeleportItem.Initializer(_monitor, _translate, _config);
                TeleportItem.OnDayStartedRetag(sender, e, _monitor, _translate);
            }

            StripAllBuildingsDefault.Strip(_monitor, _translate);
        }
    }
}
