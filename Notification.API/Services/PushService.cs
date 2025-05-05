namespace Notification.Api.Services;

public class PushService : INotificationService
{
    private readonly ILogger<PushService> _logger;
    private readonly Random _random = new();

    public PushService(ILogger<PushService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(string recipient, string content)
    {
        // Simulate a 50% chance of success
        bool isSuccess = _random.Next(2) == 0;

        if (isSuccess)
        {
            _logger.LogInformation("PUSH SENT to {Recipient}: {Content}", recipient, content);
            return Task.FromResult(true);
        }

        _logger.LogWarning("PUSH FAILED to {Recipient}: {Content}", recipient, content);
        return Task.FromResult(false);
    }
}