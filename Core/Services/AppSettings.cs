namespace Core.Services
{
    public class AppSettings
    {
        private AppSettings()
        {
        }

        public static AppSettings Instance { get; } = new AppSettings();

        public string MailSmtpUsername { get; set; }
        public string MailSmtpPassword { get; set; }
        public string MailSmtpServer { get; set; }
        public int MailSenderPort { get; set; }
        public bool MailEnableSsl { get; set; }
        public string WebInterfaceUrl { get; set; }
    }
}