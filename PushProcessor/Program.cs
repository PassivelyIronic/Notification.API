using MassTransit;
using Notification.Api.Messages;
using PushProcessor;

var builder = Host.CreateApplicationBuilder(args);

// Dodaj MassTransit
builder.Services.AddMassTransit(config =>
{
    // Zarejestruj konsumenta
    config.AddConsumer<SendPushHandler>();

    // Skonfiguruj RabbitMq
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Skonfiguruj endpoint
        cfg.ReceiveEndpoint("push-processor", e =>
        {
            e.ConfigureConsumer<SendPushHandler>(context);

            // Upewnij siê, ¿e tylko jeden komunikat jest przetwarzany jednoczeœnie
            e.PrefetchCount = 1;
        });
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();