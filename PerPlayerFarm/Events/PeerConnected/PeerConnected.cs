using PerPlayerFarm.Events.Peerconnected;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.PeerConnected
{
    public sealed class PeerConnected
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;

        public PeerConnected(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            long uid = e.Peer.PlayerID;
            var farmer = Game1.GetPlayer(uid);
            string? displayName = farmer?.displayName;

            Locations.LoadInvitedPpfFarmsForHost(uid, _monitor, _translate, displayName);
            Locations.LoadFacadeCabinInPpfOfInvitedForHost(uid, _monitor, _translate);
            Locations.TrackOwner(uid, _monitor, _helper, _translate);
            HouseWarpUtils.OverrideDefaultHouseWarpToPPF(uid);
        }
    }
}
