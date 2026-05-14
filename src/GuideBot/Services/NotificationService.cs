using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface INotificationService
{
    Task<IReadOnlyList<UserNotification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IReadOnlyList<UserNotification>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUserNotificationsAsync(userId, cancellationToken);
    }

    public async Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        var notification = new UserNotification(userId, message);
        await _notificationRepository.AddAsync(notification, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
    }
}