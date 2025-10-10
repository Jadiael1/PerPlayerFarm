using PerPlayerFarm.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.RenderingWorld
{
    public class RenderingWorld
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translate;
        private MailboxState _mailboxState;

        public RenderingWorld(IModHelper helper, IMonitor monitor, MailboxState mailboxState)
        {
            _helper = helper;
            _monitor = monitor;
            _translate = helper.Translation;
            _mailboxState = mailboxState;
            helper.Events.Display.RenderingWorld += OnRenderingWorld;
        }

        private void ResetState()
        {
            _mailboxState.MailBackup = null;
            _mailboxState.Suppressing = false;
            _mailboxState.HadMail = false;
        }

        private static bool IsPerPlayerFarm(string? name)
            => name is not null && name.StartsWith("PPF_", StringComparison.Ordinal);

        public void OnRenderingWorld(object? sender, RenderingWorldEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!IsPerPlayerFarm(Game1.currentLocation?.NameOrUniqueName))
                return;

            if (_mailboxState.Suppressing)
                return;

            try
            {
                _mailboxState.MailBackup = Game1.mailbox.ToList();
                _mailboxState.HadMail = _mailboxState.MailBackup.Count > 0;
                Game1.mailbox.Clear();
                _mailboxState.Suppressing = true;
            }
            catch (Exception ex)
            {
                _monitor.Log($"[PPF] Failed to prepare mailbox view: {ex}", LogLevel.Warn);
                ResetState();
            }
        }
    }
}