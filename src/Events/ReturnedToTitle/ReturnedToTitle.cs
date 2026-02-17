using PerPlayerFarm.Helpers;
using PerPlayerFarm.Models;
using StardewModdingAPI.Events;

namespace PerPlayerFarm.Events.ReturnedToTitle
{
    public static class ReturnedToTitle
    {
        public static void OnReturnedToTitle(
            HashSet<long> knownFarmhandIds,
            MailboxState mailboxState,
            object? s,
            ReturnedToTitleEventArgs e
        )
        {
            Mailbox.ResetMailboxState(mailboxState);
            knownFarmhandIds.Clear();
        }
    }
}