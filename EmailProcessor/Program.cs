using EmailProcessor;
using MassTransit;
using Notification.Api.Messages;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            // Register message consumers
            x.AddConsumer<SendEmailHandler>();

            // Configure RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                // Configure endpoints
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();