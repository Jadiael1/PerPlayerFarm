using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.LoadStageChanged
{
    public static class PlayerDataInitializer
    {

        public static void Initializer(LoadStageChangedEventArgs e, IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            if (!Context.IsMainPlayer)
                return;

            if (e.NewStage is LoadStage.CreatedInitialLocations or LoadStage.SaveAddedLocations)
            {
                EnsurePpfPerPlayerFromSaveData(helper, monitor, translate);
                EnsurePerPlayerFarmsFromKnownFarmers(helper, monitor, translate);
            }
        }


        public static void EnsurePpfPerPlayerFromSaveData(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            var data = Peerconnected.Locations.ReadSaveData(helper, monitor, translate);
            foreach (var uid in data.OwnerUids)
            {
                Peerconnected.Locations.LoadInvitedPpfFarmsForHost(uid, monitor, translate);
                Peerconnected.Locations.LoadFacadeCabinInPpfOfInvitedForHost(uid, monitor, translate);
                Peerconnected.Locations.TrackOwner(uid, monitor, helper, translate);
                if (Context.IsMainPlayer)
                    PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(uid);
            }
        }

        private static void EnsurePerPlayerFarmsFromKnownFarmers(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            foreach (var f in Game1.getAllFarmers())
            {
                if (f.IsMainPlayer) continue;
                Peerconnected.Locations.LoadInvitedPpfFarmsForHost(f.UniqueMultiplayerID, monitor, translate, $"Farm {f.displayName}");
                Peerconnected.Locations.LoadFacadeCabinInPpfOfInvitedForHost(f.UniqueMultiplayerID, monitor, translate);
                Peerconnected.Locations.TrackOwner(f.UniqueMultiplayerID, monitor, helper, translate);

            }

            if (!Context.IsMainPlayer)
                return;

            foreach (var farmer in Game1.getAllFarmers())
                PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(farmer.UniqueMultiplayerID);
        }
    }
}