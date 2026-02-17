using StardewModdingAPI;
using StardewModdingAPI.Events;
using PeerLocations = PerPlayerFarm.Events.Peerconnected.Locations;
using HouseWarpHelper = PerPlayerFarm.Events.PeerConnected.HouseWarpUtils;
using TeleporterManager = PerPlayerFarm.Events.DayStarted.TeleportItem;

namespace PerPlayerFarm.Events.SaveLoaded
{
    public sealed class SaveLoaded
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        private readonly ITranslationHelper _translate;

        public SaveLoaded(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            var data = PeerLocations.ReadSaveData(_helper, _monitor, _translate);
            if (data.IsDisabled)
            {
                if (Context.IsMainPlayer)
                {
                    PeerLocations.RemoveAllPpfFarms();
                    TeleporterManager.RemoveAllTeleporters();
                    HouseWarpHelper.RestoreAllDefaultHouseWarps();
                }

                return;
            }

            Locations.LoadPpfFarmsForInvited(_monitor, _translate);
            StripAllBuildingsDefault.Strip(_monitor, _translate);
            if (Context.IsMainPlayer)
            {
                LoadStageChanged.PlayerDataInitializer.EnsurePerPlayerFarmsFromKnownFarmers(_helper, _monitor, _translate);
            }
        }
    }
}
