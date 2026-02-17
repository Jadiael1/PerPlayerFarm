using Microsoft.Xna.Framework;
using PerPlayerFarm.Config;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace PerPlayerFarm.Helpers
{
    public static class ChatCommandHelper
    {
        private static DateTime? _ppfUninstallConfirmUntilUtc;
        private static readonly TimeSpan PpfUninstallConfirmWindow = TimeSpan.FromSeconds(30);

        public static void RegisterCommands(
            IModHelper helper,
            IMonitor monitor,
            ModConfig config,
            IManifest modManifest,
            HashSet<long> knownFarmhandIds
        )
        {
            ChatCommands.Register(
                commandName: "ppfclearfarm",
                handler: (command, chatBox) => CmdPpfClearFarm(helper, monitor, config, modManifest, command, chatBox),
                helpDescription: (arg) => HelpDescPpfClearFarm(helper, arg)
            );
            ChatCommands.Register(
                commandName: "ppfuninstall",
                handler: (command, chatBox) => CmdPpfUninstall(helper, monitor, modManifest, command, chatBox, knownFarmhandIds),
                helpDescription: (arg) => HelpDescPpfUninstall(helper, arg)
            );
        }

        private static string HelpDescPpfClearFarm(IModHelper helper, string arg)
        {
            return helper.Translation.Get("ppf.chatcmd.clearfarm.help", new { command = $"/{arg}" });
        }

        private static void CmdPpfClearFarm(IModHelper helper, IMonitor monitor, ModConfig config, IManifest modManifest, string[] command, ChatBox chat)
        {
            if (!Context.IsWorldReady)
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.world_not_ready"));
                return;
            }
            string arg = (command.Length >= 2 ? command[1]?.Trim() : "") ?? "";
            bool clearAll = arg.Equals("all", StringComparison.OrdinalIgnoreCase);

            if (clearAll)
            {
                if (!Context.IsMainPlayer)
                {
                    chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.host_required_all", new { command = "/ppfclearfarm all" }));
                    return;
                }

                int count = 0;

                foreach (var loc in Game1.locations)
                {
                    if (!loc.NameOrUniqueName.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (TryClearFarmLocation(helper, loc, out string? error))
                        count++;
                    else if (!string.IsNullOrEmpty(error))
                        chat.addErrorMessage(error);
                }

                chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.cleared_all_result", new { count = count }));
                return;
            }

            var current = Game1.player.currentLocation;
            if (!current.NameOrUniqueName.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase))
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.must_be_inside_ppf", new { command = "/ppfclearfarm all" }));
                return;
            }

            if (!long.TryParse(current.NameOrUniqueName.AsSpan("PPF_".Length), out long ownerId))
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.owner_id_parse_failed"));
                return;
            }

            if (Game1.player.UniqueMultiplayerID != ownerId && !Context.IsMainPlayer)
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.cannot_clear_other_players"));
                return;
            }


            if (!TryClearFarmLocation(helper, current, out string? errMsg))
            {
                chat.addErrorMessage(errMsg ?? helper.Translation.Get("ppf.chatcmd.clearfarm.clear_failed_generic"));
                return;
            }

            chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.clearfarm.cleared_current_result"));
        }

        private static bool TryClearFarmLocation(IModHelper helper, GameLocation loc, out string? error)
        {
            error = null;

            try
            {
                // Remove objetos colocados (chests, machines, etc.)
                /*
                Weeds, Ervas daninhas, Weeds = 675, 674, 784
                Stone, Pedra, Stone = 450, 343
                Twig, Galho, Twig = 294, 295

            */
                var objectIdsToRemove = new HashSet<string> { "675", "450", "294", "295", "674", "784", "343" };
                var objectKeysToRemove = new List<Vector2>();

                foreach (var kv in loc.Objects.Pairs)
                {
                    var obj = kv.Value;
                    if (obj?.ItemId is not null && objectIdsToRemove.Contains(obj.ItemId))
                        objectKeysToRemove.Add(kv.Key);
                }

                foreach (var key in objectKeysToRemove.Distinct())
                    loc.Objects.Remove(key);
                // loc.Objects?.Clear();

                // Remove terrain features (trees, crops, grass, etc.)
                var terrainFeaturesKeysToRemove = new List<Vector2>();

                foreach (var kv in loc.terrainFeatures.Pairs)
                {
                    var tf = kv.Value;
                    if (tf is null) continue;

                    var typeName = tf.GetType().Name;
                    if (typeName is "Tree" or "Grass")
                        terrainFeaturesKeysToRemove.Add(kv.Key);
                }

                foreach (var pos in terrainFeaturesKeysToRemove.Distinct())
                    loc.terrainFeatures.Remove(pos);
                // loc.terrainFeatures?.Clear();

                // Remove large terrain features (bushes etc.), quando existir
                // loc.largeTerrainFeatures?.Clear();

                // Farm possui resourceClumps (stumps, boulders etc.)
                if (loc is Farm farm)
                    farm.resourceClumps?.Clear();

                // Debris (itens no chão) - existe em muitos GameLocations
                // loc.debris?.Clear();

                return true;
            }
            catch (Exception ex)
            {
                error = helper.Translation.Get("ppf.chatcmd.clearfarm.clear_failed_with_location", new { locationName = loc?.NameOrUniqueName, errorType = ex.GetType().Name, errorMessage = ex.Message });
                return false;
            }
        }


        private static string HelpDescPpfUninstall(IModHelper helper, string arg)
        {
            return helper.Translation.Get("ppf.chatcmd.uninstall.help", new { command = $"/{arg}", seconds = 30 });
        }

        private static void CmdPpfUninstall(
            IModHelper helper,
            IMonitor monitor,
            IManifest modManifest,
            string[] command,
            ChatBox chat,
            HashSet<long> knownFarmhandIds
        )
        {
            if (!Context.IsWorldReady)
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.uninstall.world_not_ready"));
                return;
            }

            if (!Context.IsMainPlayer)
            {
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.uninstall.host_only", new { command = "/ppfuninstall" }));
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.host_only", new { command = "/ppfuninstall" }), HUDMessage.newQuest_type));
                return;
            }

            string arg = (command.Length >= 2 ? command[1]?.Trim() : "") ?? "";
            bool isConfirm = arg.Equals("confirm", StringComparison.OrdinalIgnoreCase);

            DateTime now = DateTime.UtcNow;

            if (!isConfirm)
            {
                _ppfUninstallConfirmUntilUtc = now + PpfUninstallConfirmWindow;

                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.uninstall.warning_remove_mod_and_locations"));
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.warning_remove_mod_and_locations"), HUDMessage.newQuest_type));
                chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.uninstall.preconfirm_move_items_animals"));
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.preconfirm_move_items_animals"), HUDMessage.newQuest_type));
                chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.uninstall.preconfirm_run_confirm", new { command = "/ppfuninstall confirm", seconds = 30 }));
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.preconfirm_run_confirm", new { command = "/ppfuninstall confirm", seconds = 30 }), HUDMessage.newQuest_type));
                return;
            }

            if (_ppfUninstallConfirmUntilUtc is null || now > _ppfUninstallConfirmUntilUtc.Value)
            {
                _ppfUninstallConfirmUntilUtc = null;
                chat.addErrorMessage(helper.Translation.Get("ppf.chatcmd.uninstall.confirmation_expired", new { command = "/ppfuninstall" }));
                Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.confirmation_expired", new { command = "/ppfuninstall" }), HUDMessage.newQuest_type));
                return;
            }

            // confirmation consumed
            _ppfUninstallConfirmUntilUtc = null;

            // Safety: if player is currently inside a PPF_* map, warp out first.
            var current = Game1.player.currentLocation;
            if (current?.NameOrUniqueName?.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase) == true)
            {
                Game1.warpFarmer("Farm", 64, 15, false);
            }

            int removed = 0;
            for (int i = Game1.locations.Count - 1; i >= 0; i--)
            {
                var loc = Game1.locations[i];
                if (loc?.NameOrUniqueName?.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase) == true)
                {
                    Game1.locations.RemoveAt(i);
                    removed++;
                }
            }

            knownFarmhandIds.Clear();
            // clear save data used by your current implementation
            helper.Data.WriteSaveData("ppf.locations", new HashSet<long>());

            monitor.Log($"PPF uninstall executed. Removed {removed} PPF_* location(s).", LogLevel.Info);

            chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.uninstall.complete_removed", new { count = removed }));
            Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.complete_removed", new { count = removed }), HUDMessage.newQuest_type));
            chat.addInfoMessage(helper.Translation.Get("ppf.chatcmd.uninstall.post_save_close_remove_mod"));
            Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("ppf.chatcmd.uninstall.post_save_close_remove_mod"), HUDMessage.newQuest_type));
        }

    }
}