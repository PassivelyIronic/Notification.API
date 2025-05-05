namespace Notification.Api.Services;

public interface IEmailService
{
    Task<bool> SendAsync(string recipient, string content);
}