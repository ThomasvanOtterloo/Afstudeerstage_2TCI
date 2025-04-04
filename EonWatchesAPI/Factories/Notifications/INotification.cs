namespace EonWatchesAPI.Factories.Notifications;

public interface INotification
{
    public Task SendNotification(SendEmailRequest message);

}