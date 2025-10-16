using PerPlayerFarm.Configuration;
using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace PerPlayerFarm.Events.AssetRequested
{
    public sealed class AssetRequested
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private readonly ModConfig _config;

        public AssetRequested(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _config = config;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            PpfTeleporter.AddIfNotExists(e, _translate);
            MapFarm.WarpAdjustmentPropertyFarm(e, _helper, _monitor, _config);
            PpfLogCabin.AddIfNotExists(e);
            MapPpf.AddAndEdit(e, _helper, _monitor, _config);
            FarmEntries.Edit(e, _monitor, _translate);
        }
    }
}