namespace EonWatchesAPI.Factories.Notifications;

public class NotificationFactory
{
    private readonly INotification _notification;
    
    public NotificationFactory(INotification notification)
    {
        _notification = notification;
    }
}