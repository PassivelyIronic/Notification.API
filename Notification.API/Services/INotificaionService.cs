namespace Notification.Api.Services;

public interface INotificationService
{
    Task<bool> SendAsync(string recipient, string content);
}