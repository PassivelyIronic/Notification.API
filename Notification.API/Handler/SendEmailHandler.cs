using MassTransit;
using Notification.Api.Messages;
using Notification.Api.Services;

namespace Notification.Api.Handlers;

public class SendEmailHandler : IConsumer<SendEmailNotification>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailHandler> _logger;

    public SendEmailHandler(
        IEmailService emailService,
        ILogger<SendEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendEmailNotification> context)
    {
        _logger.LogInformation("Processing email notification {NotificationId} for {Recipient}",
            context.Message.NotificationId, context.Message.Recipient);

        // Simulate taking time to process one notification at a time
        await Task.Delay(2000);

        bool success = await _emailService.SendAsync(context.Message.Recipient, context.Message.Content);

        if (success)
        {
            await context.Publish(new EmailNotificationSent
            {
                NotificationId = context.Message.NotificationId,
                Recipient = context.Message.Recipient
            });
        }
        else
        {
            await context.Publish(new EmailNotificationFailed
            {
                NotificationId = context.Message.NotificationId,
                Recipient = context.Message.Recipient,
                RetryCount = context.Message.RetryCount + 1
            });
        }
    }
}
