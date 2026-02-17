using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.PeerConnected
{
    public static class PeerConnected
    {

        public static void OnPeerConnected(
            IModHelper helper,
            IMonitor monitor,
            HashSet<long> knownFarmhandIds,
            IManifest modManifest,
            object? s,
            PeerConnectedEventArgs e
        )
        {
            // if (!Context.IsMainPlayer) return;

            // long uid = e.Peer.PlayerID;
            // Farmer? farmer = Game1.GetPlayer(uid);
            // if (farmer is null) return;

            // string? displayName = farmer?.displayName;
            // string? name = farmer?.Name;

            // if (displayName is null && name is null) return;
            // if (displayName is not null && displayName.Length <= 0) return;
            // if (name is not null && name.Length <= 0) return;

            // bool online = Game1.getOnlineFarmers().Any(f => f.UniqueMultiplayerID == uid);

            // if (!online) return;

            // var validName = displayName is not null && displayName.Length > 0 ? displayName : name is not null && name.Length > 0 ? name : "unknow";

            // var payload = new CharacterSetupFinishedPayload
            // {
            //     UniqueMultiplayerId = uid,
            //     Name = validName,
            //     FarmName = Game1.player.farmName?.Value ?? "",
            //     IsFarmhand = true,
            // };

            // helper.Multiplayer.SendMessage(
            //         payload,
            //         messageType: msgFarmhandConnect,
            //         modIDs: new[] { modManifest.UniqueID },
            //         playerIDs: new[] { uid }
            //     );


            // Game1.chatBox.addMessage($"validName: {validName} displayName: {displayName} name: {name}", Microsoft.Xna.Framework.Color.White);
            // Game1.addHUDMessage(new HUDMessage($"validName: {validName} displayName: {displayName} name: {name}", HUDMessage.newQuest_type));

        }
    }
}