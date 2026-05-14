using GuideBot.DataAccess;
using GuideBot.DataAccess.Models;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlNotificationRepository : INotificationRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlNotificationRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<UserNotification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var notifications = await dataContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        return notifications.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(notification), token: cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.Notifications
            .Where(n => n.Id == notificationId)
            .UpdateAsync(n => new NotificationModel { IsRead = true }, token: cancellationToken);
    }
}