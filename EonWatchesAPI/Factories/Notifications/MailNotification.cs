using System.Net.Mail;

namespace EonWatchesAPI.Factories.Notifications;

public class MailNotification : INotification
{
    private readonly GmailSettings _gmailSettings;

    public MailNotification(GmailSettings gmailSettings)
    {
        _gmailSettings = gmailSettings;
    }

    public void SendNotification(string message)
    {
        MailMessage mailMessage = new MailMessage();





    }
}