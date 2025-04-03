namespace EonWatchesAPI.Factories.Notifications
{
    public class GmailSettings
    {
        public const string GmailOptionsKey = "GmailOptions";
        public string Host { get; set; }
        public int Port { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
