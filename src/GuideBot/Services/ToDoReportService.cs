using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;
    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStatsAsync(Guid userId, CancellationToken token)
    {
        var allTasks = await _toDoRepository.GetAllByUserIdAsync(userId, token);
        var allTasksCount = allTasks.Count;
        var activeTasksCount = allTasks.Count(t => t.State == ToDoItemState.Active);

        return new()
        {
            total = allTasksCount,
            completed = allTasksCount - activeTasksCount,
            active = activeTasksCount,
            generatedAt = DateTime.Now
        };
    }
}