using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Notification.Api.Database;
using Notification.Api.Models;
using Notification.Api.Messages;
using Microsoft.EntityFrameworkCore;

namespace Notification.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<NotificationController> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Calculate UTC time for scheduled notification
        DateTime scheduledTimeUtc = request.ScheduledTime.HasValue
            ? TimeZoneInfo.ConvertTimeToUtc(request.ScheduledTime.Value,
                TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone))
            : DateTime.UtcNow;

        // Create the command
        var command = new CreateNotification(
            request.Recipient,
            request.Content,
            request.SendEmail,
            request.SendPush,
            request.TimeZone,
            scheduledTimeUtc);

        // Publish the command
        await _publishEndpoint.Publish(command);

        return Accepted();
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await _dbContext.Notifications
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(50)
            .Select(n => NotificationDto.FromEntity(n))
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(Guid id)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
        {
            return NotFound();
        }

        return Ok(NotificationDto.FromEntity(notification));
    }
}