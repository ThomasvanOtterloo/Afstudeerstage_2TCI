namespace EonWatchesAPI.Factories.Notifications;

public record SendEmailRequest(string Subject, string Body, string RecipientEmail);