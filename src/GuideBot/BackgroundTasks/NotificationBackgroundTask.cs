using GuideBot.Services;
using Telegram.Bot;

namespace GuideBot.BackgroundTasks;

public class NotificationBackgroundTask : BackgroundTask
{
    private readonly INotificationService _notificationService;
    private readonly ITelegramBotClient _bot;

    public NotificationBackgroundTask(INotificationService notificationService, ITelegramBotClient bot)
        : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
    {
        _notificationService = notificationService;
        _bot = bot;
    }

    protected override async Task Execute(CancellationToken ct)
    {
        var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);

        foreach (var notification in notifications)
        {
            if (notification.User == null) continue;

            try
            {
                await _bot.SendMessage(
                    chatId: notification.User.TelegramUserId,
                    text: notification.Text,
                    cancellationToken: ct);

                await _notificationService.MarkNotified(notification.Id, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке уведомления {notification.Id}: {ex.Message}");
            }
        }
    }
}