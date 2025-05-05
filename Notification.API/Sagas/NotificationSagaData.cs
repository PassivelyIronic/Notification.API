using MassTransit;
using Notification.Api.Database;

namespace Notification.Api.Sagas;

public class NotificationSagaData : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;

    public Guid NotificationId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool SendEmail { get; set; }
    public bool SendPush { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public DateTime ScheduledTimeUtc { get; set; }

    public int EmailRetryCount { get; set; }
    public int PushRetryCount { get; set; }

    public bool EmailSent { get; set; }
    public bool PushSent { get; set; }
}