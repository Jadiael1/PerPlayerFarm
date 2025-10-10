using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.ObjectListChanged
{
    public class ObjectListChanged
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;

        private const string PlacedKey = Utils.Constants.TeleportPlacedKey;
        private const string PlacedMainFarmKey = Utils.Constants.TeleportPlacedMainFarmKey;
        private const string TeleportModDataKey = Utils.Constants.TeleportModDataKey;
        private const string TeleportQualifiedItemId = Utils.Constants.TeleportQualifiedItemId;
        private const string TeleportModDataMainFarm = Utils.Constants.TeleportModDataMainFarm;
        private const string TeleportModDataPpf = Utils.Constants.TeleportModDataPpf;

        public ObjectListChanged(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            try
            {
                if (!Context.IsWorldReady || !Context.IsMainPlayer)
                    return;

                var loc = e.Location;
                if (loc == null) return;

                string name = loc.NameOrUniqueName ?? loc.Name ?? string.Empty;
                bool isFarm = name.Equals("Farm", StringComparison.OrdinalIgnoreCase);
                bool isPpf = name.StartsWith("PPF_", StringComparison.OrdinalIgnoreCase);
                if (!isFarm && !isPpf)
                    return;

                if (e.Added == null || e.Added.Count() == 0)
                    return;

                foreach (var pair in e.Added)
                {
                    var obj = pair.Value;
                    if (obj == null) continue;
                    if (!obj.bigCraftable.Value) continue;

                    bool isMiniObelisk = string.Equals(obj.QualifiedItemId, TeleportQualifiedItemId, StringComparison.Ordinal)
                        || string.Equals(obj.QualifiedItemId, "(BC)Mini-Obelisk", StringComparison.OrdinalIgnoreCase)
                        || obj.ParentSheetIndex == 238;

                    if (!isMiniObelisk) continue;

                    // re-label to maintain usefulness after placement
                    obj.modData[TeleportModDataKey] = isFarm ? TeleportModDataMainFarm : TeleportModDataPpf;
                    loc.modData[isFarm ? PlacedMainFarmKey : PlacedKey] = "1";

                    _monitor?.Log($"[PPF] Mini-Obelisk placed manually in {name} was tagged for teleport.", LogLevel.Trace);
                }
            }
            catch (Exception ex)
            {
                _monitor?.Log($"[PPF] OnObjectListChanged failed: {ex}", LogLevel.Warn);
            }
        }

    }
}