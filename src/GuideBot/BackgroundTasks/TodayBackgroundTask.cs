using GuideBot.DataAccess;
using GuideBot.Services;

namespace GuideBot.BackgroundTasks;

public class TodayBackgroundTask : BackgroundTask
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private readonly IToDoRepository _toDoRepository;

    public TodayBackgroundTask(
        INotificationService notificationService,
        IUserRepository userRepository,
        IToDoRepository toDoRepository)
        : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
        _toDoRepository = toDoRepository;
    }

    protected override async Task Execute(CancellationToken ct)
    {
        var users = await _userRepository.GetUsers(ct);
        var today = DateTime.UtcNow.Date;

        foreach (var user in users)
        {
            var todayItems = await _toDoRepository.GetActiveWithDeadline(
                user.UserId,
                today,
                today.AddDays(1),
                ct);

            if (todayItems.Count == 0) continue;

            var taskList = string.Join("\n", todayItems.Select((t, i) => $"{i + 1}. {t.Name}"));
            var text = $"Сегодня у вас запланированы задачи:\n{taskList}";

            await _notificationService.ScheduleNotification(
                user.UserId,
                $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}",
                text,
                today.AddHours(9),
                ct);
        }
    }
}