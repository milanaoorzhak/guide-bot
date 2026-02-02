using GuideBot.DataAccess;

namespace GuideBot;

public class ToDoService : IToDoService
{
    private readonly ToDoSettings _settings;
    private readonly IToDoRepository _toDoRepository;

    public ToDoService(ToDoSettings settings, IToDoRepository toDoRepository)
    {
        _settings = settings;
        _toDoRepository = toDoRepository;
    }

    public async Task<ToDoItem?> AddAsync(ToDoUser user, string name, CancellationToken token)
    {
        await CheckTaskCountAsync(user.UserId, token);
        if (IsValidTaskLength(name) && !await _toDoRepository.ExistsByNameAsync(user.UserId, name, token))
        {
            var item = new ToDoItem(user, name);
            await _toDoRepository.AddAsync(item, token);
            return item;
        }

        return null;
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        await _toDoRepository.DeleteAsync(id, token);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token)
    {
        return await _toDoRepository.GetActiveByUserIdAsync(userId, token);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token)
    {
        return await _toDoRepository.GetAllByUserIdAsync(userId, token);
    }

    public async Task MarkAsCompletedAsync(Guid id, CancellationToken token)
    {
        var task = await _toDoRepository.GetAsync(id, token);

        if (task is null) return;

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.Now;

        await _toDoRepository.UpdateAsync(task, token);
    }

    async Task CheckTaskCountAsync(Guid userId, CancellationToken token)
    {
        var userTasks = await _toDoRepository.GetAllByUserIdAsync(userId, token);
        if (userTasks.Any() && userTasks.Count == _settings.MaxTaskCount) throw new TaskCountLimitException(_settings.MaxTaskCount);
    }

    bool IsValidTaskLength(string? task)
    {
        ValidateString(task);

        if (task!.Length > _settings.MaxTaskLength) throw new TaskLengthLimitException(task.Length, _settings.MaxTaskLength);

        return true;
    }

    void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException($"Строка не должна быть пустой");
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken token)
    {
        return await _toDoRepository.FindAsync(user.UserId, item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase), token);
    }
}