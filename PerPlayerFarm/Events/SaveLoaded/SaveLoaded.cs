using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.SaveLoaded
{
    public sealed class SaveLoaded
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        private readonly ITranslationHelper _translate;

        public SaveLoaded(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Locations.LoadPpfFarmsForInvited(_monitor, _translate);
            StripAllBuildingsDefault.Strip(_monitor, _translate);
        }
    }
}