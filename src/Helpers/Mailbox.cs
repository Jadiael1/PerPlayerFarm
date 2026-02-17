using PerPlayerFarm.Models;

namespace PerPlayerFarm.Helpers
{
    public static class Mailbox
    {
        public static void ResetMailboxState(MailboxState mailboxState)
        {
            mailboxState.MailBackup = null;
            mailboxState.Suppressing = false;
            mailboxState.HadMail = false;
        }
    }
}