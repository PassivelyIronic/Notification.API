using Notification.Api.Database;

namespace Notification.Api.Models;

public class NotificationDto
{
    public Guid Id { get; set; }

    public string Recipient { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool SendEmail { get; set; }

    public bool SendPush { get; set; }

    public string TimeZone { get; set; } = "UTC";

    public DateTime ScheduledTimeUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    // Map from entity
    public static NotificationDto FromEntity(