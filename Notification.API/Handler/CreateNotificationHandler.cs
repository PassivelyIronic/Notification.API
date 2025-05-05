using MassTransit;
using Notification.Api.Database;
using Notification.Api.Messages;

namespace Notification.Api.Handlers;

public class CreateNotificationHandler : IConsumer<CreateNotification>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CreateNotificationHandler> _logger;

    public CreateNotificationHandler(
        AppDbContext dbContext,
        ILogger<CreateNotificationHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CreateNotification> context)
    {
        _logger.LogInformation("Creating notification for {Recipient}", context.Message.Recipient);

        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Recipient = context.Message.Recipient,
            Content = context.Message.Content,
            SendEmail = context.Message.SendEmail,
            SendPush = context.Message.SendPush,
            TimeZone = context.Message.TimeZone,
            ScheduledTimeUtc = context.Message.ScheduledTimeUtc,
            CreatedAtUtc = DateTime.UtcNow,
            Status = NotificationStatus.Pending,
            EmailRetryCount = 0,
            PushRetryCount = 0
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        await context.Publish(new NotificationCreated
        {
            NotificationId = notification.Id,
            Recipient = notification.Recipient,
            Content = notification.Content,
            SendEmail = notification.SendEmail,
            SendPush = notification.SendPush,
            TimeZone = notification.TimeZone,
            ScheduledTimeUtc = notification.ScheduledTimeUtc
        });
    }
}