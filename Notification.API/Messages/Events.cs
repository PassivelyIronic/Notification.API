namespace Notification.Api.Messages;

public class NotificationCreated
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool SendEmail { get; init; }
    public bool SendPush { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public DateTime ScheduledTimeUtc { get; init; }
}

public class NotificationScheduled
{
    public Guid NotificationId { get; init; }
    public DateTime ScheduledTimeUtc { get; init; }
}

public class EmailNotificationSent
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
}

public class EmailNotificationFailed
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
    public int RetryCount { get; init; }
}

public class PushNotificationSent
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
}

public class PushNotificationFailed
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
    public int RetryCount { get; init; }
}

public class NotificationCompleted
{
    public Guid NotificationId { get; init; }
    public string Recipient { get; init; } = string.Empty;
}