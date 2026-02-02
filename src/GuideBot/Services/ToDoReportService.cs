using GuideBot.DataAccess;

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
        var allTasksCount = (await _toDoRepository.GetAllByUserIdAsync(userId, token)).Count();
        var activeTasksCount = await _toDoRepository.CountActiveAsync(userId, token);

        return new()
        {
            total = allTasksCount,
            completed = allTasksCount - activeTasksCount,
            active = activeTasksCount,
            generatedAt = DateTime.Now
        };
    }
}