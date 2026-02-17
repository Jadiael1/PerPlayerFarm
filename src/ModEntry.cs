using PerPlayerFarm.Config;
using StardewModdingAPI;
using PerPlayerFarm.Events.LoadStageChanged;
using PerPlayerFarm.Events.Saving;
using PerPlayerFarm.Models;
using PerPlayerFarm.Events.RenderedWorld;
using PerPlayerFarm.Events.RenderingWorld;
using PerPlayerFarm.Events.AssetRequested;
using PerPlayerFarm.Events.MenuChanged;
using PerPlayerFarm.Events.ModMessageReceived;
using PerPlayerFarm.Events.ReturnedToTitle;
using PerPlayerFarm.Events.PeerConnected;
using PerPlayerFarm.Events.SaveLoaded;
using PerPlayerFarm.Events.ButtonPressed;
using StardewValley;
using PerPlayerFarm.Helpers;
using PerPlayerFarm.Events.GameLaunched;

namespace PerPlayerFarm
{

    public sealed class ModEntry : Mod
    {
        private HashSet<long> KnownFarmhandIds = new();
        private const string SaveKey = "ppf.locations"; // "ppf.known-farmhands";
        private const string MsgFarmhandFinishedCharacterSetup = "FarmhandFinishedCharacterSetup";
        private MailboxState _mailboxState = new();
        private const string EnterFarmsTouchActionKey = "DerexSV.PerPlayerFarm.EnterFarms";

        public override void Entry(IModHelper helper)
        {
            var config = helper.ReadConfig<ModConfig>();

            GameLocation.RegisterTouchAction($"{EnterFarmsTouchActionKey}", (locs, args, who, tile) =>
                WarpHelper.WarpToCorrectFarm(helper, Monitor, EnterFarmsTouchActionKey, config, locs, args, who, tile));

            helper.Events.Specialized.LoadStageChanged += (s, e) =>
                LoadStageChanged.OnLoadStageChanged(helper, Monitor, SaveKey, KnownFarmhandIds, s, e);

            helper.Events.GameLoop.Saving += (s, e) =>
                Saving.OnSaving(helper, Monitor, SaveKey, KnownFarmhandIds, s, e);

            helper.Events.Content.AssetRequested += (s, e) =>
                AssetRequested.OnAssetRequested(helper, Monitor, config, EnterFarmsTouchActionKey, s, e);

            helper.Events.Display.MenuChanged += (s, e) =>
                MenuChanged.OnMenuChanged(helper, Monitor, this.ModManifest, MsgFarmhandFinishedCharacterSetup, KnownFarmhandIds, config, s, e);

            helper.Events.Multiplayer.ModMessageReceived += (s, e) =>
                ModMessageReceived.OnModMessageReceived(helper, Monitor, this.ModManifest, MsgFarmhandFinishedCharacterSetup, KnownFarmhandIds, SaveKey, config, s, e);

            helper.Events.Display.RenderingWorld += (s, e) =>
                RenderingWorld.OnRenderingWorld(_mailboxState, s, e);

            helper.Events.Display.RenderedWorld += (s, e) =>
                RenderedWorld.OnRenderedWorld(_mailboxState, s, e);

            helper.Events.GameLoop.ReturnedToTitle += (s, e) =>
                ReturnedToTitle.OnReturnedToTitle(KnownFarmhandIds, _mailboxState, s, e);

            helper.Events.Multiplayer.PeerConnected += (s, e) =>
                PeerConnected.OnPeerConnected(helper, Monitor, KnownFarmhandIds, this.ModManifest, s, e);

            helper.Events.GameLoop.SaveLoaded += (s, e) =>
                SaveLoaded.OnSaveLoaded(helper, Monitor, KnownFarmhandIds, SaveKey, config, s, e);

            helper.Events.Input.ButtonPressed += (s, e) =>
                ButtonPressed.OnButtonPressed(helper, Monitor, config, s, e);

            helper.Events.GameLoop.GameLaunched += (s, e) =>
                GameLaunched.OnGameLaunched(helper, Monitor, config, ModManifest, s, e);

            ChatCommandHelper.RegisterCommands(helper, Monitor, config, ModManifest, KnownFarmhandIds);
        }
    }
}
