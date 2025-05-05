namespace Notification.Api.Messages;

public record CreateNotification(
    string Recipient,
    string Content,
    bool SendEmail,
    bool SendPush,
    string TimeZone,
    DateTime ScheduledTimeUtc);

public record SendEmailNotification(
    Guid NotificationId,
    string Recipient,
    string Content,
    int RetryCount);

public record SendPushNotification(
    Guid NotificationId,
    string Recipient,
    string Content,
    int RetryCount);

public record ScheduleNotification(
    Guid NotificationId,
    DateTime ScheduledTimeUtc);