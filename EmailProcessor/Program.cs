using EmailProcessor;
using MassTransit;
using Notification.Api.Messages;

var builder = Host.CreateApplicationBuilder(args);

// Dodaj MassTransit
builder.Services.AddMassTransit(config =>
{
    // Zarejestruj konsumenta
    config.AddConsumer<SendEmailHandler>();

    // Skonfiguruj RabbitMQ
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Skonfiguruj endpoint
        cfg.ReceiveEndpoint("email-processor", e =>
        {
            e.ConfigureConsumer<SendEmailHandler>(context);

            // Upewnij si�, �e tylko jeden komunikat jest przetwarzany jednocze�nie
            e.PrefetchCount = 1;
        });
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();