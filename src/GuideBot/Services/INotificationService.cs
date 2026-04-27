using GuideBot.Entities;

namespace GuideBot.Services;

public interface INotificationService
{
    Task<bool> ScheduleNotification(
        Guid userId,
        string type,
        string text,
        DateTime scheduledAt,
        CancellationToken ct);

    Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);

    Task MarkNotified(Guid notificationId, CancellationToken ct);
}