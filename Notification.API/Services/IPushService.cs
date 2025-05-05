namespace Notification.Api.Services;

public interface IPushService
{
    Task<bool> SendAsync(string recipient, string content);
}