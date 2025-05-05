namespace Notification.Api.Database;

public class NotificationEntity
{
    public Guid Id { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool SendEmail { get; set; }

    public bool SendPush { get; set; }

    public string TimeZone { get; set; } = "UTC";

    public DateTime ScheduledTimeUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public NotificationStatus Status { get; set; }

    public int EmailRetryCount { get; set; }

    public int PushRetryCount { get; set; }
}
