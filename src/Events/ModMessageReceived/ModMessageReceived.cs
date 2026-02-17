using PerPlayerFarm.Config;
using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.ModMessageReceived
{
    public static class ModMessageReceived
    {
        public static void OnModMessageReceived(
            IModHelper helper,
            IMonitor monitor,
            IManifest modManifest,
            string msgFarmhandFinishedCharacterSetup,
            HashSet<long> knownFarmhandIds,
            string saveKey,
            ModConfig config,
            object? sender,
            ModMessageReceivedEventArgs e
        )
        {
            // if (e.FromModID == modManifest.UniqueID && e.Type == msgFarmhandConnect)
            // {
            //     if (Context.IsMainPlayer) return;
            //     var payload = e.ReadAs<CharacterSetupFinishedPayload>();
            //     Game1.chatBox.addMessage($"validName: {payload.Name} {payload.FarmName} {payload.UniqueMultiplayerId}", Microsoft.Xna.Framework.Color.White);
            //     Game1.addHUDMessage(new HUDMessage($"validName: {payload.Name} {payload.FarmName} {payload.UniqueMultiplayerId}", HUDMessage.newQuest_type));
            //     MapHelpers.EnsurePlayerMap(payload.UniqueMultiplayerId, payload.Name, monitor, knownFarmhandIds);
            //     knownFarmhandIds.Add(payload.UniqueMultiplayerId);
            // }

            if (e.FromModID == modManifest.UniqueID && e.Type == msgFarmhandFinishedCharacterSetup)
            {
                var payload = e.ReadAs<CharacterSetupFinishedPayload>();

                foreach (var f in Game1.getOnlineFarmers())
                {
                    if (f.UniqueMultiplayerID == payload.UniqueMultiplayerId)
                    {
                        MapHelpers.EnsurePlayerMap(payload.UniqueMultiplayerId, payload.Name, helper, monitor, knownFarmhandIds);
                        knownFarmhandIds.Add(payload.UniqueMultiplayerId);

                        if (Context.IsMainPlayer && config.WarpToPPFOnHouseExit)
                            WarpHelper.UpdateHomeExitWarp(monitor, payload.UniqueMultiplayerId);

                        break;
                    }
                }

                if (knownFarmhandIds.Count > 0 && Context.IsMainPlayer)
                    helper.Data.WriteSaveData(saveKey, knownFarmhandIds);
            }
        }
    }
}