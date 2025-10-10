using PerPlayerFarm.Events.Peerconnected;
using StardewModdingAPI;
using StardewModdingAPI.Events;

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

            Locations.LoadInvitedPpfFarmsForHost(e.Peer.PlayerID, _monitor, _translate);
            Locations.LoadFacadeCabinInPpfOfInvitedForHost(e.Peer.PlayerID, _monitor, _translate);
            Locations.TrackOwner(e.Peer.PlayerID, _monitor, _helper, _translate);
            HouseWarpUtils.OverrideDefaultHouseWarpToPPF(e.Peer.PlayerID);
        }
    }
}
