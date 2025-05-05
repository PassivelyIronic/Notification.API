namespace Notification.Api.Services;

public class EmailService : INotificationService
{
    private readonly ILogger<EmailService> _logger;
    private readonly Random _random = new();

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(string recipient, string content)
    {
        // Simulate a 50% chance of success
        bool isSuccess = _random.Next(2) == 0;

        if (isSuccess)
        {
            _logger.LogInformation("EMAIL SENT to {Recipient}: {Content}", recipient, content);
            return Task.FromResult(true);
        }

        _logger.LogWarning("EMAIL FAILED to {Recipient}: {Content}", recipient, content);
        return Task.FromResult(false);
    }
}