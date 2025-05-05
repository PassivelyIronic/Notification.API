using System.ComponentModel.DataAnnotations;

namespace Notification.Api.Models;

public class CreateNotificationRequest
{
    [Required]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool SendEmail { get; set; } = true;

    public bool SendPush { get; set; } = false;

    public string TimeZone { get; set; } = "UTC";

    public DateTime? ScheduledTime { get; set; }
}
