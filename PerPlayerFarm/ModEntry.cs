using PerPlayerFarm.Configuration;
using PerPlayerFarm.Events.AssetRequested;
using PerPlayerFarm.Events.ButtonPressed;
using PerPlayerFarm.Events.DayStarted;
using PerPlayerFarm.Events.LoadStageChanged;
using PerPlayerFarm.Events.ObjectListChanged;
using PerPlayerFarm.Events.OnModMessageReceived;
using PerPlayerFarm.Events.PeerConnected;
using PerPlayerFarm.Events.RenderedWorld;
using PerPlayerFarm.Events.RenderingWorld;
using PerPlayerFarm.Events.ReturnedToTitle;
using PerPlayerFarm.Events.SaveLoaded;
using PerPlayerFarm.Events.Saving;
using PerPlayerFarm.Events.UpdateTicked;
using PerPlayerFarm.Types;
using PerPlayerFarm.Utils;
using StardewModdingAPI;

namespace PerPlayerFarm
{
    public sealed class ModEntry : Mod
    {
        // ButtonPressed, ModMessageReceived, ReturnedToTitle
        private readonly List<PpfFarmEntry> _clientRegistry = new();
        private readonly HashSet<string> _clientStubs = new(StringComparer.OrdinalIgnoreCase);



        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();

            PpfConsoleCommands.Register(helper, this.Monitor, config);
            // RenderingWorld, RenderedWorld
            var mailboxState = new MailboxState();

            _ = new AssetRequested(helper, this.Monitor);
            _ = new Saving(helper);
            _ = new SaveLoaded(helper, this.Monitor);
            _ = new PeerConnected(helper, this.Monitor);
            _ = new LoadStageChanged(helper, this.Monitor);
            _ = new DayStarted(helper, this.Monitor, config);
            _ = new ButtonPressed(helper, this.Monitor, this.ModManifest.UniqueID, _clientRegistry, _clientStubs);
            _ = new ModMessageReceived(helper, this.Monitor, this.ModManifest.UniqueID, _clientRegistry);
            _ = new ReturnedToTitle(helper, this.Monitor, _clientRegistry, _clientStubs);
            _ = new RenderingWorld(helper, this.Monitor, mailboxState);
            _ = new RenderedWorld(helper, this.Monitor, mailboxState);
            _ = new UpdateTicked(helper, this.Monitor);
            _ = new ObjectListChanged(helper, this.Monitor);

        }
    }
}
