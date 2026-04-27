using GuideBot.DataAccess.Models;
using GuideBot.Entities;
using GuideBot.Services;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class NotificationService : INotificationService
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public NotificationService(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<bool> ScheduleNotification(
        Guid userId,
        string type,
        string text,
        DateTime scheduledAt,
        CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var exists = await dbContext.Notifications
            .AnyAsync(n => n.UserId == userId && n.Type == type, ct);

        if (exists)
            return false;

        var notification = new NotificationModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Text = text,
            ScheduledAt = scheduledAt,
            IsNotified = false,
            NotifiedAt = null
        };

        await dbContext.InsertAsync(notification);
        return true;
    }

    public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        var models = await dbContext.Notifications
            .LoadWith(n => n.User)
            .Where(n => n.IsNotified == false && n.ScheduledAt <= scheduledBefore)
            .ToListAsync(ct);

        return models.Select(ModelMapper.MapNotificationFromModel).ToList().AsReadOnly();
    }

    public async Task MarkNotified(Guid notificationId, CancellationToken ct)
    {
        using var dbContext = _factory.CreateDataContext();

        await dbContext.Notifications
            .Where(n => n.Id == notificationId)
            .Set(n => n.IsNotified, true)
            .Set(n => n.NotifiedAt, DateTime.UtcNow)
            .UpdateAsync(ct);
    }
}