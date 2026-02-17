using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.RenderedWorld
{
    public static class RenderedWorld
    {
        public static void OnRenderedWorld(
            MailboxState mailboxState,
            object? s,
            RenderedWorldEventArgs e
        )
        {
            try
            {
                PpfMailboxHelper.DrawOwnerMailboxIconIfNeeded(mailboxState, e.SpriteBatch);
            }
            finally
            {
                PpfMailboxHelper.RestoreMailboxAfterPpfRender(mailboxState);
            }
        }
    }
}