using MassTransit;
using Notification.Api.Messages;

namespace PushProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBusControl _busControl;

        public Worker(ILogger<Worker> logger, IBusControl busControl)
        {
            _logger = logger;
            _busControl = busControl;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.StartAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Push processor running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }

            await _busControl.StopAsync(stoppingToken);
        }
    }

    public class SendPushHandler : IConsumer<SendPushNotification>
    {
        private readonly ILogger<SendPushHandler> _logger;
        private readonly Random _random = new Random();

        public SendPushHandler(ILogger<SendPushHandler> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendPushNotification> context)
        {
            _logger.LogInformation("Processing push notification {NotificationId} for {Recipient}",
                context.Message.NotificationId, context.Message.Recipient);

            // Simulate taking time to process
            await Task.Delay(1500);

            // Simulate a 50% chance of success
            bool success = _random.Next(2) == 0;

            if (success)
            {
                _logger.LogInformation("PUSH SENT to {Recipient}: {Content}",
                    context.Message.Recipient, context.Message.Content);

                await context.Publish(new PushNotificationSent
                {
                    NotificationId = context.Message.NotificationId,
                    Recipient = context.Message.Recipient
                });
            }
            else
            {
                _logger.LogWarning("PUSH FAILED to {Recipient}: {Content}",
                    context.Message.Recipient, context.Message.Content);

                await context.Publish(new PushNotificationFailed
                {
                    NotificationId = context.Message.NotificationId,
                    Recipient = context.Message.Recipient,
                    RetryCount = context.Message.RetryCount + 1
                });
            }
        }
    }
}