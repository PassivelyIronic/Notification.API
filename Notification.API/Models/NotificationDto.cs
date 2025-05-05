namespace Notification.Api.Models;

using Notification.Api.Database;

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
    public static NotificationDto FromEntity(NotificationEntity entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            Recipient = entity.Recipient,
            Content = entity.Content,
            SendEmail = entity.SendEmail,
            SendPush = entity.SendPush,
            TimeZone = entity.TimeZone,
            ScheduledTimeUtc = entity.ScheduledTimeUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            Status = entity.Status.ToString()
        };
    }
}