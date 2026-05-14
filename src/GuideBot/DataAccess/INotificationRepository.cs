using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface INotificationRepository
{
    Task<IReadOnlyList<UserNotification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
}