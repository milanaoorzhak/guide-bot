using GuideBot.DataAccess;

namespace GuideBot.Services;

public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;
    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        var allTasksCount = _toDoRepository.GetAllByUserId(userId).Count();
        var activeTasksCount = _toDoRepository.CountActive(userId);

        return new()
        {
            total = allTasksCount,
            completed = allTasksCount - activeTasksCount,
            active = activeTasksCount,
            generatedAt = DateTime.Now
        };
    }
}