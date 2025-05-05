namespace Notification.Api.Database;

public enum NotificationStatus
{
    Pending,
    Scheduled,
    ProcessingEmail,
    ProcessingPush,
    EmailSent,
    EmailFailed,
    PushSent,
    PushFailed,
    Completed,
    Failed
}
