using PerPlayerFarm.Config;
using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace PerPlayerFarm.Events.MenuChanged
{
    public static class MenuChanged
    {
        public static void OnMenuChanged(
            IModHelper helper,
            IMonitor monitor,
            IManifest modManifest,
            string msgFarmhandFinishedCharacterSetup,
            HashSet<long> knownFarmhandIds,
            ModConfig config,
            object? sender,
            MenuChangedEventArgs e
        )
        {
            if (e.OldMenu is CharacterCustomization && e.NewMenu is not CharacterCustomization)
            {
                var payload = new CharacterSetupFinishedPayload
                {
                    UniqueMultiplayerId = Game1.player.UniqueMultiplayerID,
                    Name = Game1.player.Name,
                    FarmName = Game1.player.farmName?.Value ?? "",
                    IsFarmhand = !Context.IsMainPlayer,
                };

                if (Game1.getLocationFromName($"PPF_{Game1.player.UniqueMultiplayerID}") is not null || Context.IsMainPlayer)
                    return;

                foreach (var f in Game1.getOnlineFarmers())
                {
                    if (Game1.player.UniqueMultiplayerID == f.UniqueMultiplayerID) continue;

                    helper.Multiplayer.SendMessage(
                        payload,
                        messageType: msgFarmhandFinishedCharacterSetup,
                        modIDs: new[] { modManifest.UniqueID },
                        playerIDs: new[] { f.UniqueMultiplayerID }
                    );
                }

                MapHelpers.EnsurePlayerMap(payload.UniqueMultiplayerId, payload.Name, helper, monitor, knownFarmhandIds);
                knownFarmhandIds.Add(payload.UniqueMultiplayerId);
            }

            if (e.OldMenu is CarpenterMenu && e.NewMenu is not CarpenterMenu)
            {
                if (!config.WarpToPPFOnHouseExit) return;

                foreach (var f in Game1.getAllFarmers())
                {
                    if (f is null || f.IsMainPlayer) continue;
                    WarpHelper.UpdateHomeExitWarp(monitor, f.UniqueMultiplayerID);
                }
            }
        }
    }
}