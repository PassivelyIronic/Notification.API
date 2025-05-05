using MassTransit;
using Notification.Api.Messages;

namespace Notification.Api.Sagas;

public class NotificationSaga : MassTransitStateMachine<NotificationSagaData>
{
    // States
    public State Pending { get; private set; }
    public State Scheduled { get; private set; }
    public State ProcessingEmail { get; private set; }
    public State ProcessingPush { get; private set; }
    public State Completed { get; private set; }

    // Events
    public Event<NotificationCreated> NotificationCreated { get; private set; }
    public Event<NotificationScheduled> NotificationScheduled { get; private set; }
    public Event<EmailNotificationSent> EmailNotificationSent { get; private set; }
    public Event<EmailNotificationFailed> EmailNotificationFailed { get; private set; }
    public Event<PushNotificationSent> PushNotificationSent { get; private set; }
    public Event<PushNotificationFailed> PushNotificationFailed { get; private set; }

    // Timeout events for scheduled notifications
    public Schedule<NotificationSagaData, NotificationScheduleTimeout> NotificationSchedule { get; private set; }

    public NotificationSaga()
    {
        InstanceState(x => x.CurrentState);

        // Correlate events by notification ID
        Event(() => NotificationCreated, e => e.CorrelateById(m => m.Message.NotificationId));
        Event(() => NotificationScheduled, e => e.CorrelateById(m => m.Message.NotificationId));
        Event(() => EmailNotificationSent, e => e.CorrelateById(m => m.Message.NotificationId));
        Event(() => EmailNotificationFailed, e => e.CorrelateById(m => m.Message.NotificationId));
        Event(() => PushNotificationSent, e => e.CorrelateById(m => m.Message.NotificationId));
        Event(() => PushNotificationFailed, e => e.CorrelateById(m => m.Message.NotificationId));

        // Schedule for notification timing
        Schedule(() => NotificationSchedule,
            saga => saga.NotificationId,
            s => s.Delay = s.ScheduledTimeUtc - DateTime.UtcNow > TimeSpan.Zero ?
                s.ScheduledTimeUtc - DateTime.UtcNow :
                TimeSpan.Zero);

        // Initial state when notification is created
        Initially(
            When(NotificationCreated)
                .Then(context =>
                {
                    context.Saga.NotificationId = context.Message.NotificationId;
                    context.Saga.Recipient = context.Message.Recipient;
                    context.Saga.Content = context.Message.Content;
                    context.Saga.SendEmail = context.Message.SendEmail;
                    context.Saga.SendPush = context.Message.SendPush;
                    context.Saga.TimeZone = context.Message.TimeZone;
                    context.Saga.ScheduledTimeUtc = context.Message.ScheduledTimeUtc;
                    context.Saga.EmailRetryCount = 0;
                    context.Saga.PushRetryCount = 0;
                    context.Saga.EmailSent = false;
                    context.Saga.PushSent = false;
                })
                .TransitionTo(Pending)
                .Schedule(NotificationSchedule,
                    context => new NotificationScheduleTimeout { NotificationId = context.Message.NotificationId },
                    context => context.Message.ScheduledTimeUtc - DateTime.UtcNow > TimeSpan.Zero ?
                        context.Message.ScheduledTimeUtc - DateTime.UtcNow :
                        TimeSpan.Zero)
        );

        // Handle scheduled notification timeout
        During(Pending,
            When(NotificationSchedule.Received)
                .TransitionTo(Scheduled)
                .Then(context =>
                {
                    if (context.Saga.SendEmail)
                    {
                        context.Publish(new SendEmailNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.EmailRetryCount));

                        context.TransitionToState(ProcessingEmail);
                    }
                    else if (context.Saga.SendPush)
                    {
                        context.Publish(new SendPushNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.PushRetryCount));

                        context.TransitionToState(ProcessingPush);
                    }
                    else
                    {
                        // No channels to send to, consider completed
                        context.Publish(new NotificationCompleted
                        {
                            NotificationId = context.Saga.NotificationId,
                            Recipient = context.Saga.Recipient
                        });

                        context.TransitionToState(Completed);
                    }
                })
        );

        // Email notification handling
        During(ProcessingEmail,
            When(EmailNotificationSent)
                .Then(context =>
                {
                    context.Saga.EmailSent = true;

                    // If we need to send push notification, do it now
                    if (context.Saga.SendPush && !context.Saga.PushSent)
                    {
                        context.Publish(new SendPushNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.PushRetryCount));

                        context.TransitionToState(ProcessingPush);
                    }
                    else
                    {
                        // Email sent and no push needed, we're done
                        context.Publish(new NotificationCompleted
                        {
                            NotificationId = context.Saga.NotificationId,
                            Recipient = context.Saga.Recipient
                        });

                        context.TransitionToState(Completed);
                    }
                }),

            When(EmailNotificationFailed)
                .Then(context =>
                {
                    context.Saga.EmailRetryCount = context.Message.RetryCount;

                    // If we haven't exceeded max retries, try again
                    if (context.Saga.EmailRetryCount < 3)
                    {
                        // Use the proper method for delayed publishing
                        var message = new SendEmailNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.EmailRetryCount);

                        // Use SchedulePublish instead of direct Publish with TimeSpan
                        context.SchedulePublish(TimeSpan.FromSeconds(5), message);
                    }
                    else
                    {
                        // Max retries exceeded, move on to push if needed
                        if (context.Saga.SendPush && !context.Saga.PushSent)
                        {
                            context.Publish(new SendPushNotification(
                                context.Saga.NotificationId,
                                context.Saga.Recipient,
                                context.Saga.Content,
                                context.Saga.PushRetryCount));

                            context.TransitionToState(ProcessingPush);
                        }
                        else
                        {
                            // Email failed and no push, mark as failed
                            context.Publish(new NotificationCompleted
                            {
                                NotificationId = context.Saga.NotificationId,
                                Recipient = context.Saga.Recipient
                            });

                            context.TransitionToState(Completed);
                        }
                    }
                })
        );

        // Push notification handling
        During(ProcessingPush,
            When(PushNotificationSent)
                .Then(context =>
                {
                    context.Saga.PushSent = true;

                    // If email was needed but not sent successfully yet, go back to that
                    if (context.Saga.SendEmail && !context.Saga.EmailSent && context.Saga.EmailRetryCount < 3)
                    {
                        context.Publish(new SendEmailNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.EmailRetryCount));

                        context.TransitionToState(ProcessingEmail);
                    }
                    else
                    {
                        // Push sent and email either sent or failed, we're done
                        context.Publish(new NotificationCompleted
                        {
                            NotificationId = context.Saga.NotificationId,
                            Recipient = context.Saga.Recipient
                        });

                        context.TransitionToState(Completed);
                    }
                }),

            When(PushNotificationFailed)
                .Then(context =>
                {
                    context.Saga.PushRetryCount = context.Message.RetryCount;

                    // If we haven't exceeded max retries, try again
                    if (context.Saga.PushRetryCount < 3)
                    {
                        // Use the proper method for delayed publishing
                        var message = new SendPushNotification(
                            context.Saga.NotificationId,
                            context.Saga.Recipient,
                            context.Saga.Content,
                            context.Saga.PushRetryCount);

                        // Use SchedulePublish instead of direct Publish with TimeSpan
                        context.SchedulePublish(TimeSpan.FromSeconds(5), message);
                    }
                    else
                    {
                        // Max retries exceeded, check if email was needed but not sent
                        if (context.Saga.SendEmail && !context.Saga.EmailSent && context.Saga.EmailRetryCount < 3)
                        {
                            context.Publish(new SendEmailNotification(
                                context.Saga.NotificationId,
                                context.Saga.Recipient,
                                context.Saga.Content,
                                context.Saga.EmailRetryCount));

                            context.TransitionToState(ProcessingEmail);
                        }
                        else
                        {
                            // Push failed and email either sent or failed, we're done
                            context.Publish(new NotificationCompleted
                            {
                                NotificationId = context.Saga.NotificationId,
                                Recipient = context.Saga.Recipient
                            });

                            context.TransitionToState(Completed);
                        }
                    }
                })
        );

        // Final state
        During(Completed,
            Ignore(EmailNotificationSent),
            Ignore(EmailNotificationFailed),
            Ignore(PushNotificationSent),
            Ignore(PushNotificationFailed));
    }
}

// Notification Schedule Timeout
public class NotificationScheduleTimeout
{
    public Guid NotificationId { get; set; }
}