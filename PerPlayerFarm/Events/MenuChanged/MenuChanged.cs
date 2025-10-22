
using System.Globalization;
using PerPlayerFarm.Configuration;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace PerPlayerFarm.Events.MenuChanged
{
    public sealed class MenuChanged
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private readonly ModConfig _config;

        public MenuChanged(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _config = config;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        // recalculates the new exit of the cabin facade.
        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsMainPlayer) return;

            if (e.OldMenu is CarpenterMenu && e.NewMenu is null)
            {
                foreach (var loc in Game1.locations.OfType<Farm>()
                         .Where(l => (l.NameOrUniqueName ?? l.Name ?? "").StartsWith("PPF_", StringComparison.OrdinalIgnoreCase)))
                {
                    long ownerUid = 0;

                    var facade = loc.buildings.FirstOrDefault(b => b?.buildingType?.Value == Utils.Constants.FacadeBuildingId);
                    if (facade != null && facade.modData.TryGetValue(Utils.Constants.FacadeOwnerUid, out var uidText) &&
                        long.TryParse(uidText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                    {
                        ownerUid = parsed;
                    }
                    else
                    {
                        long.TryParse((loc.NameOrUniqueName ?? loc.Name ?? "").Substring(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out ownerUid);
                    }

                    if (ownerUid == 0)
                    {
                        return;
                    }

                    PerPlayerFarm.Events.PeerConnected.HouseWarpUtils.OverrideDefaultHouseWarpToPPF(ownerUid);
                }
            }
        }

    }
}