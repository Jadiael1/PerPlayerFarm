using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.OnModMessageReceived
{
    public class ModMessageReceived
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly string _modUniqueId;
        private readonly ITranslationHelper _translate;
        private readonly List<PpfFarmEntry> _clientRegistry;
        private const string _registryMessageType = Utils.Constants.RegistryMessageType;
        private const string _registryRequestType = Utils.Constants.RegistryRequestType;

        public ModMessageReceived(IModHelper helper, IMonitor monitor, string modUniqueId, List<PpfFarmEntry> clientRegistry)
        {
            _helper = helper;
            _monitor = monitor;
            _modUniqueId = modUniqueId;
            _translate = helper.Translation;
            _clientRegistry = clientRegistry;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
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

        internal void BroadcastRegistry(long? toPlayerId = null)
        {
            if (!Context.IsMainPlayer)
                return;

            var farms = BuildRegistry();
            var payload = new PpfRegistryMessage { Farms = farms };

            if (toPlayerId.HasValue)
            {
                _helper.Multiplayer.SendMessage(payload, _registryMessageType, new[] { _modUniqueId }, new[] { toPlayerId.Value });
                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.trace.registry_sent",
                    new { player = toPlayerId.Value, count = farms.Count }
                ), LogLevel.Trace);
            }
            else
            {
                _helper.Multiplayer.SendMessage(payload, _registryMessageType, new[] { _modUniqueId });
                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.trace.registry_broadcast",
                    new { count = farms.Count }
                ), LogLevel.Trace);
            }
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != _modUniqueId)
                return;

            if (e.Type == _registryMessageType)
            {
                var payload = e.ReadAs<PpfRegistryMessage>();
                _clientRegistry.Clear();
                if (payload?.Farms != null)
                    _clientRegistry.AddRange(payload.Farms);

                _monitor.Log(_translate.Get(
                    "derexsv.ppf.log.trace.registry_received",
                    new { count = _clientRegistry.Count }
                ), LogLevel.Trace);
                return;
            }

            if (e.Type == _registryRequestType && Context.IsMainPlayer)
            {
                BroadcastRegistry(e.FromPlayerID);
            }
        }

    }
}
