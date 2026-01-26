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

    public ToDoItem? Add(ToDoUser user, string name)
    {
        CheckTaskCount(user.UserId);
        if (IsValidTaskLength(name) && !_toDoRepository.ExistsByName(user.UserId, name))
        {
            var item = new ToDoItem(user, name);
            _toDoRepository.Add(item);
            return item;
        }

        return null;
    }

    public void Delete(Guid id)
    {
        _toDoRepository.Delete(id);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _toDoRepository.GetActiveByUserId(userId);
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoRepository.GetAllByUserId(userId);
    }

    public void MarkAsCompleted(Guid id)
    {
        var task = _toDoRepository.Get(id);

        if (task is null) return;

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.Now;

        _toDoRepository.Update(task);
    }

    void CheckTaskCount(Guid userId)
    {
        var userTasks = _toDoRepository.GetAllByUserId(userId);
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

    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        return _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
    }
}