namespace PerPlayerFarm.Utils
{
    public sealed class MailboxState
    {
        public List<string>? MailBackup { get; set; }
        public bool Suppressing { get; set; }
        public bool HadMail { get; set; }
    }
}