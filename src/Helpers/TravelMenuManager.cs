using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewValley;

namespace PerPlayerFarm.Helpers
{
    public static class TravelMenuManager
    {
        public static void OpenTravelMenu(IModHelper helper, IMonitor monitor, int page = 0)
        {
            if (!Context.IsWorldReady)
                return;

            List<PpfFarmEntry> farms = BuildRegistry(helper)
               .Where(f => f.Available)
               .ToList();

            if (farms.Count == 0)
            {
                Game1.showRedMessage(helper.Translation.Get("ppf.travel_menu.no_farms_available"));
                return;
            }
            OpenTravelMenu(helper, farms, page, monitor);
        }

        private static void OpenTravelMenu(IModHelper helper, List<PpfFarmEntry> farms, int page, IMonitor monitor)
        {
            const int pageSize = 6;
            int totalPages = Math.Max(1, (int)Math.Ceiling(farms.Count / (double)pageSize));
            page = Math.Clamp(page, 0, totalPages - 1);

            var slice = farms.Skip(page * pageSize).Take(pageSize).ToList();
            var responses = new List<Response>();

            foreach (var entry in slice)
            {
                var translateMenuStatus = helper.Translation.Get(entry.OwnerOnline ? "ppf.travel_menu.status.online" : "ppf.travel_menu.status.offline");
                string label = entry.OwnerId == 0 ? entry.DisplayName : helper.Translation.Get("ppf.travel_menu.entry_status", new { displayName = entry.DisplayName, status = translateMenuStatus });
                responses.Add(new Response(entry.InternalName, label));
            }

            if (totalPages > 1)
            {
                if (page > 0) responses.Add(new Response("__ppf_prev", helper.Translation.Get("ppf.travel_menu.nav.prev")));
                if (page < totalPages - 1) responses.Add(new Response("__ppf_next", helper.Translation.Get("ppf.travel_menu.nav.next")));
            }

            responses.Add(new Response("__ppf_cancel", helper.Translation.Get("ppf.travel_menu.nav.cancel")));

            Game1.playSound("smallSelect");

            Game1.currentLocation?.createQuestionDialogue(helper.Translation.Get("ppf.travel_menu.prompt.choose_farm"), responses.ToArray(), (Farmer _, string answer) =>
            {
                if (string.IsNullOrEmpty(answer) || answer == "__ppf_cancel") return;
                if (answer == "__ppf_prev")
                {
                    Game1.delayedActions.Add(new DelayedAction(1, () => OpenTravelMenu(helper, monitor, page - 1)));
                    return;
                }
                if (answer == "__ppf_next")
                {
                    Game1.delayedActions.Add(new DelayedAction(1, () => OpenTravelMenu(helper, monitor, page + 1)));
                    return;
                }
                Game1.delayedActions.Add(new DelayedAction(1, () => TravelTo(helper, answer)));
            });
        }

        private static void TravelTo(IModHelper helper, string internalName)
        {
            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                Game1.showRedMessage(helper.Translation.Get("ppf.travel_menu.travel.not_now"));
                return;
            }

            var location = Game1.getLocationFromName(internalName);
            if (location is null)
            {
                Game1.showRedMessage(helper.Translation.Get("ppf.travel_menu.travel.location_not_available"));
                return;
            }

            int tileX = Game1.player.TilePoint.X;
            int tileY = Game1.player.TilePoint.Y;

            (tileX, tileY) = ClampToLocation(location, tileX, tileY, 64, 15);
            Game1.warpFarmer(internalName, tileX, tileY, false);
        }

        private static (int X, int Y) ClampToLocation(GameLocation? location, int x, int y, int fallbackX, int fallbackY)
        {
            var layer = location?.Map?.GetLayer("Back");
            if (layer is null) return (fallbackX, fallbackY);
            x = Math.Clamp(x, 0, layer.LayerWidth - 1);
            y = Math.Clamp(y, 0, layer.LayerHeight - 1);
            return (x, y);
        }

        private static List<PpfFarmEntry> BuildRegistry(IModHelper helper)
        {
            var farms = new List<PpfFarmEntry>();
            var mainFarm = Game1.getLocationFromName("Farm");
            string hostName = Game1.MasterPlayer?.displayName ?? helper.Translation.Get("ppf.travel_menu.farm_label.host_fallback_name");
            farms.Add(new PpfFarmEntry
            {
                InternalName = "Farm",
                DisplayName = helper.Translation.Get("ppf.travel_menu.farm_label.main_farm", new { hostName = hostName }),
                OwnerId = Game1.MasterPlayer?.UniqueMultiplayerID ?? 0,
                Available = mainFarm != null,
                OwnerOnline = true
            });
            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer.IsMainPlayer) continue;
                if (farmer is null || farmer.displayName is null || (farmer.displayName is not null && farmer.displayName.Length <= 0)) continue;
                string internalName = $"PPF_{farmer.UniqueMultiplayerID}";
                var loc = Game1.getLocationFromName(internalName);
                bool online = Game1.getOnlineFarmers().Any(f => f.UniqueMultiplayerID == farmer.UniqueMultiplayerID);
                farms.Add(new PpfFarmEntry
                {
                    InternalName = internalName,
                    DisplayName = helper.Translation.Get("ppf.travel_menu.farm_label.player_farm", new { playerName = farmer.displayName }),
                    OwnerId = farmer.UniqueMultiplayerID,
                    Available = loc != null,
                    OwnerOnline = online
                });
            }
            return farms;
        }

    }
}