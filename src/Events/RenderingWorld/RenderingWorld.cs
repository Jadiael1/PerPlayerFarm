using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PerPlayerFarm.Events.RenderingWorld
{
    public static class RenderingWorld
    {
        public static void OnRenderingWorld(
            MailboxState mailboxState,
            object? sender,
            RenderingWorldEventArgs e
        )
        {
            PpfMailboxHelper.PrepareMailboxForPpfRender(mailboxState);
        }
    }
}