using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace PerPlayerFarm.Events.AssetRequested
{
    public sealed class AssetRequested
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;

        public AssetRequested(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            PpfTeleporter.AddIfNotExists(e);
            MapFarm.WarpAdjustmentPropertyFarm(e);
            PpfLogCabin.AddIfNotExists(e);
            MapPpf.AddAndEdit(e, _helper, _monitor);
            FarmEntries.Edit(e, _monitor, _translate);
        }
    }
}