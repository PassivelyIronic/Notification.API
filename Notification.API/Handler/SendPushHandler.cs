using MassTransit;
using Notification.Api.Messages;
using Notification.Api.Services;

namespace Notification.Api.Handlers;

public class SendPushHandler : IConsumer<SendPushNotification>
{
    private readonly IPushService _pushService;
    private readonly ILogger<SendPushHandler> _logger;

    public SendPushHandler(
        IPushService pushService,
        ILogger<SendPushHandler> logger)
    {
        _pushService = pushService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendPushNotification> context)
    {
        _logger.LogInformation("Processing push notification {NotificationId} for {Recipient}",
            context.Message.NotificationId, context.Message.Recipient);

        // Simulate taking time to process one notification at a time
        await Task.Delay(1500);

        bool success = await _pushService.SendAsync(context.Message.Recipient, context.Message.Content);

        if (success)
        {
            await context.Publish(new PushNotificationSent
            {
                NotificationId = context.Message.NotificationId,
                Recipient = context.Message.Recipient
            });
        }
        else
        {
            await context.Publish(new PushNotificationFailed
            {
                NotificationId = context.Message.NotificationId,
                Recipient = context.Message.Recipient,
                RetryCount = context.Message.RetryCount + 1
            });
        }
    }
}