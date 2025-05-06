using System.Net.Mail;
using Microsoft.Extensions.Options;


namespace EonWatchesAPI.Factories.Notifications;

public class MailNotification : INotification
{
    private readonly GmailSettings _gmailSettings;

    public MailNotification(IOptions<GmailSettings> gmailSettings)
    {
        _gmailSettings = gmailSettings.Value;
    }

    public async Task SendNotification(SendEmailRequest message)
    {
        MailMessage mailMessage = new MailMessage
        {
            From = new MailAddress(_gmailSettings.email),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(message.RecipientEmail);
        
        Console.WriteLine("Using sender: " + _gmailSettings.email);
        Console.WriteLine("Sending to: " + message.RecipientEmail);
        
        try
        {
            using var smtpClient = new SmtpClient(_gmailSettings.Host, _gmailSettings.Port)
            {
                Credentials = new System.Net.NetworkCredential(_gmailSettings.email, _gmailSettings.password),
                EnableSsl = true,
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }
}