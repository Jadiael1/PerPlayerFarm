using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace PerPlayerFarm.Events.LoadStageChanged
{
    public static class PlayerDataInitializer
    {

        public static void Initializer(LoadStageChangedEventArgs e, IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            if (!Context.IsMainPlayer)
                return;

            if (e.NewStage is not LoadStage.SaveAddedLocations)
                return;

            // if (e.NewStage is LoadStage.CreatedInitialLocations or LoadStage.SaveAddedLocations)
            // {
            // }
            EnsurePpfPerPlayerFromSaveData(helper, monitor, translate);
        }


        private static void EnsurePpfPerPlayerFromSaveData(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            var data = Peerconnected.Locations.ReadSaveData(helper, monitor, translate);
            foreach (var uid in data.OwnerUids)
            {
                Peerconnected.Locations.LoadInvitedPpfFarmsForHost(uid, monitor, translate);
                Peerconnected.Locations.LoadFacadeCabinInPpfOfInvitedForHost(uid, monitor, translate);
                Peerconnected.Locations.TrackOwner(uid, monitor, helper, translate);
                PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(uid);
            }
        }

        public static void EnsurePerPlayerFarmsFromKnownFarmers(IModHelper helper, IMonitor monitor, ITranslationHelper translate)
        {
            Types.PpfSaveData data = Peerconnected.Locations.ReadSaveData(helper, monitor, translate);

            var eligible = Utils.EligibleFarmers.Get(helper);
            if (eligible is null)
            {
                return;
            }

            if (data.OwnerUids.Count > 0)
            {
                foreach (var uid in data.OwnerUids)
                {
                    bool isMatching = eligible.Any(euid => euid == uid);
                    if (!isMatching)
                    {
                        var loc = Game1.getLocationFromName($"PPF_{uid}");
                        if (loc is null) continue;
                        Game1.locations.Remove(loc);
                    }
                }
            }

            foreach (long uid in eligible)
            {
                var farmer = Game1.GetPlayer(uid);
                if (farmer is null || farmer.IsMainPlayer)
                {
                    continue;
                }
                string? displayName = farmer?.displayName;
                Peerconnected.Locations.LoadInvitedPpfFarmsForHost(uid, monitor, translate, displayName);
                Peerconnected.Locations.LoadFacadeCabinInPpfOfInvitedForHost(uid, monitor, translate);
                Peerconnected.Locations.TrackOwner(uid, monitor, helper, translate);
                PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(uid);
            }
        }
    }
}