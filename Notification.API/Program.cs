using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Database;
using Notification.Api.Handlers;
using Notification.Api.Sagas;
using Notification.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("NotificationDb"));

// Add notification services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPushService, PushService>();

// Add MassTransit
builder.Services.AddMassTransit(config =>
{
    // Add message consumers
    config.AddConsumer<CreateNotificationHandler>();
    config.AddConsumer<SendEmailHandler>();
    config.AddConsumer<SendPushHandler>();

    // Configure state machine saga
    config.AddSagaStateMachine<NotificationSaga, NotificationSagaData>()
        .InMemoryRepository();

    // Configure RabbitMQ
    config.UsingRabbitMq((context, cfg) =>
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();