using MassTransit;
using Notification.Api.Messages;

namespace EmailProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IBusControl _busControl;
        private readonly Random _random = new Random();

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
                _logger.LogInformation("Email processor running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }

            await _busControl.StopAsync(stoppingToken);
        }
    }

    public class SendEmailHandler : IConsumer<SendEmailNotification>
    {
        private readonly ILogger<SendEmailHandler> _logger;
        private readonly Random _random = new Random();

        public SendEmailHandler(ILogger<SendEmailHandler> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendEmailNotification> context)
        {
            _logger.LogInformation("Processing email notification {NotificationId} for {Recipient}",
                context.Message.NotificationId, context.Message.Recipient);

            // Simulate taking time to process
            await Task.Delay(2000);

            // Simulate a 50% chance of success
            bool success = _random.Next(2) == 0;

            if (success)
            {
                _logger.LogInformation("EMAIL SENT to {Recipient}: {Content}",
                    context.Message.Recipient, context.Message.Content);

                await context.Publish(new EmailNotificationSent
                {
                    NotificationId = context.Message.NotificationId,
                    Recipient = context.Message.Recipient
                });
            }
            else
            {
                _logger.LogWarning("EMAIL FAILED to {Recipient}: {Content}",
                    context.Message.Recipient, context.Message.Content);

                await context.Publish(new EmailNotificationFailed
                {
                    NotificationId = context.Message.NotificationId,
                    Recipient = context.Message.Recipient,
                    RetryCount = context.Message.RetryCount + 1
                });
            }
        }
    }
}