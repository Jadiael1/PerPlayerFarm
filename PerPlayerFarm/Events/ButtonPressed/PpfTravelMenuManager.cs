using Microsoft.Xna.Framework;
using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.ButtonPressed
{
    public sealed class PpfTravelMenuManager
    {
        private const string RegistryMessageType = Utils.Constants.RegistryMessageType;
        private const string RegistryRequestType = Utils.Constants.RegistryRequestType;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly string _modUniqueId;
        private readonly List<PpfFarmEntry> _clientRegistry;
        private readonly HashSet<string> _clientStubs;
        private readonly ITranslationHelper _translate;

        public PpfTravelMenuManager(IModHelper helper, IMonitor monitor, string modUniqueId, List<PpfFarmEntry> clientRegistry, HashSet<string> clientStubs)
        {
            _helper = helper;
            _monitor = monitor;
            _modUniqueId = modUniqueId;
            _clientRegistry = clientRegistry;
            _clientStubs = clientStubs;
            _translate = helper.Translation;
        }

        internal void BroadcastRegistry(long? toPlayerId = null)
        {
            if (!Context.IsMainPlayer)
                return;

            var farms = BuildRegistry();
            var payload = new PpfRegistryMessage { Farms = farms };

            if (toPlayerId.HasValue)
            {
                _helper.Multiplayer.SendMessage(payload, RegistryMessageType, new[] { _modUniqueId }, new[] { toPlayerId.Value });
                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.trace.registry_sent",
                    new { player = toPlayerId.Value, count = farms.Count }
                ), LogLevel.Trace);
            }
            else
            {
                _helper.Multiplayer.SendMessage(payload, RegistryMessageType, new[] { _modUniqueId });
                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.trace.registry_broadcast",
                    new { count = farms.Count }
                ), LogLevel.Trace);
            }
        }

        internal void RequestRegistryFromHost()
        {
            if (Context.IsMainPlayer)
                return;

            _helper.Multiplayer.SendMessage(new { }, RegistryRequestType, new[] { _modUniqueId });
            _monitor.Log(_translate.Get("derexsv.ppf.log.trace.registry_requested"), LogLevel.Trace);
        }

        internal void OnPerPlayerFarmEnsured(long uid, bool created)
        {
            if (!Context.IsMainPlayer)
                return;

            if (created)
                BroadcastRegistry();
        }

        internal bool TryHandleTeleportItem(ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return false;

            if (!e.Button.IsActionButton())
                return false;

            var location = Game1.currentLocation;
            // if (!IsAnyFarm(location))
            //     return false;

            Vector2 tile = e.Cursor.GrabTile;

            if (!location.objects.TryGetValue(tile, out var obj))
                return false;

            if (!IsTeleportObject(obj))
                return false;

            _helper.Input.Suppress(e.Button);

            if (Game1.activeClickableMenu != null)
                return true;

            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                Game1.showRedMessage(_translate.Get("derexsv.ppf.travelmenu.error.cannot_travel"));
                return true;
            }

            OpenTravelMenu();
            return true;
        }

        internal void OpenTravelMenu(int page = 0)
        {
            if (!Context.IsWorldReady)
                return;

            List<PpfFarmEntry> farms = Context.IsMainPlayer
                ? BuildRegistry()
                : new List<PpfFarmEntry>(_clientRegistry);

            if (!Context.IsMainPlayer && farms.Count == 0)
            {
                RequestRegistryFromHost();
                Game1.showRedMessage(_translate.Get("derexsv.ppf.travelmenu.info.syncing"));
                return;
            }

            farms = farms.Where(f => f.Available).ToList();
            if (farms.Count == 0)
            {
                Game1.showRedMessage(_translate.Get("derexsv.ppf.travelmenu.info.none"));
                return;
            }

            const int pageSize = 6;
            int totalPages = Math.Max(1, (int)Math.Ceiling(farms.Count / (double)pageSize));
            page = Math.Clamp(page, 0, totalPages - 1);

            var slice = farms.Skip(page * pageSize).Take(pageSize).ToList();
            var responses = new List<Response>();

            foreach (var entry in slice)
            {
                string label = entry.DisplayName;
                if (entry.OwnerId != 0)
                {
                    label = entry.OwnerOnline
                        ? _translate.Get("derexsv.ppf.travelmenu.label.online", new { name = entry.DisplayName })
                        : _translate.Get("derexsv.ppf.travelmenu.label.offline", new { name = entry.DisplayName });
                }

                responses.Add(new Response(entry.InternalName, label));
            }

            if (totalPages > 1)
            {
                if (page > 0)
                    responses.Add(new Response("__ppf_prev", _translate.Get("derexsv.ppf.travelmenu.prev_page")));
                if (page < totalPages - 1)
                    responses.Add(new Response("__ppf_next", _translate.Get("derexsv.ppf.travelmenu.next_page")));
            }

            string prompt = _translate.Get("derexsv.ppf.travelmenu.prompt.choose_farm");
            Game1.playSound("smallSelect");
            Game1.currentLocation?.createQuestionDialogue(prompt, responses.ToArray(), (Farmer _, string answer) =>
            {
                if (answer == "__ppf_prev")
                {
                    OpenTravelMenu(page - 1);
                    return;
                }

                if (answer == "__ppf_next")
                {
                    OpenTravelMenu(page + 1);
                    return;
                }

                if (string.IsNullOrEmpty(answer))
                {
                    return;
                }

                Game1.exitActiveMenu();
                TravelTo(answer);
            });
        }

        private void TravelTo(string internalName)
        {
            if (Game1.eventUp || Game1.CurrentEvent != null)
            {
                Game1.showRedMessage(_translate.Get("derexsv.ppf.travelmenu.error.cannot_travel"));
                return;
            }

            if (!Context.IsMainPlayer)
                EnsureClientStub(internalName);

            var location = Game1.getLocationFromName(internalName);

            int tileX = Game1.player.TilePoint.X;
            int tileY = Game1.player.TilePoint.Y;

            (tileX, tileY) = ClampToLocation(location, tileX, tileY, 64, 15);

            Game1.warpFarmer(internalName, tileX, tileY, false);
        }

        private void EnsureClientStub(string internalName)
        {
            if (Context.IsMainPlayer)
                return;

            if (_clientStubs.Contains(internalName))
                return;

            if (Game1.getLocationFromName(internalName) != null)
            {
                _clientStubs.Add(internalName);
                return;
            }

            var loc = new Farm($"Maps/{internalName}", internalName)
            {
                IsOutdoors = true,
                IsFarm = true
            };

            Game1.locations.Add(loc);
            _clientStubs.Add(internalName);
            _monitor.Log(_translate.Get(
                "derexsv.ppf.log.trace.client_shadow_created",
                new { location = internalName }
            ), LogLevel.Trace);
        }

        private static (int X, int Y) ClampToLocation(GameLocation? location, int x, int y, int fallbackX, int fallbackY)
        {
            var layer = location?.Map?.GetLayer("Back");
            if (layer is null)
                return (fallbackX, fallbackY);

            x = Math.Clamp(x, 0, layer.LayerWidth - 1);
            y = Math.Clamp(y, 0, layer.LayerHeight - 1);

            return (x, y);
        }

        private static bool IsTeleportObject(StardewValley.Object obj)
        {
            if (obj is null || !obj.bigCraftable.Value)
                return false;

            // only the mod's exclusive big craftable
            return string.Equals(
                obj.QualifiedItemId,
                "(BC)DerexSV.PPF_Teleporter",
                StringComparison.Ordinal
            );
        }

        private static bool IsAnyFarm(GameLocation? loc)
        {
            if (loc is null)
                return false;

            string name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;
            return name.Equals("Farm", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);
        }

        private List<PpfFarmEntry> BuildRegistry()
        {
            var farms = new List<PpfFarmEntry>();

            GameLocation? mainFarm = Game1.getLocationFromName("Farm");
            string hostName = Game1.MasterPlayer?.displayName ?? Game1.player?.displayName ?? Game1.MasterPlayer?.Name ?? Game1.player?.Name ?? "Host";

            farms.Add(new PpfFarmEntry
            {
                InternalName = "Farm",
                DisplayName = $"Farm {hostName}",
                OwnerId = 0,
                Available = mainFarm != null,
                OwnerOnline = true
            });

            foreach (var farmer in Game1.getAllFarmers())
            {
                if (farmer.IsMainPlayer)
                    continue;

                string internalName = $"PPF_{farmer.UniqueMultiplayerID}";
                GameLocation? loc = Game1.getLocationFromName(internalName);
                bool online = Game1.getOnlineFarmers().Any(f => f.UniqueMultiplayerID == farmer.UniqueMultiplayerID);

                farms.Add(new PpfFarmEntry
                {
                    InternalName = internalName,
                    DisplayName = $"Farm {farmer.displayName}",
                    OwnerId = farmer.UniqueMultiplayerID,
                    Available = loc != null,
                    OwnerOnline = online
                });
            }

            return farms;
        }
    }
}
