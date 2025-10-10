using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.LoadStageChanged
{
    public class LoadStageChanged
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;

        public LoadStageChanged(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
        }

        private void OnLoadStageChanged(object? sender, LoadStageChangedEventArgs e)
        {
            PlayerDataInitializer.Initializer(e, _helper, _monitor, _translate);
        }
    }
}