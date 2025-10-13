using PerPlayerFarm.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.ReturnedToTitle
{
    public class ReturnedToTitle
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private readonly List<PpfFarmEntry> _clientRegistry;
        private readonly HashSet<string> _clientStubs;

        public ReturnedToTitle(IModHelper helper, IMonitor monitor, List<PpfFarmEntry> clientRegistry, HashSet<string> clientStubs)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _clientRegistry = clientRegistry;
            _clientStubs = clientStubs;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            _clientRegistry.Clear();
            _clientStubs.Clear();
            Peerconnected.Locations.ResetCache();
        }
    }
}