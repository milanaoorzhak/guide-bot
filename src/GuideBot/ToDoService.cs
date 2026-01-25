namespace GuideBot;

public class ToDoService : IToDoService
{
    private readonly ToDoSettings _settings;
    private readonly List<ToDoItem> toDoItems = new();

    public ToDoService(ToDoSettings settings)
    {
        _settings = settings;
    }

    public ToDoItem? Add(ToDoUser user, string name)
    {
        CheckTaskCount();
        if (IsValidTaskLength(name) && !IsDuplicateTask(name))
        {
            var item = new ToDoItem(user, name);
            toDoItems.Add(item);
            return item;
        }

        return null;
    }

    public void Delete(Guid id)
    {
        var item = toDoItems.FirstOrDefault(item => item.Id == id);
        if (item is null) return;
        toDoItems.Remove(item!);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return toDoItems.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return toDoItems.Where(t => t.User.UserId == userId).ToList();
    }

    public void MarkAsCompleted(Guid id)
    {
        var task = toDoItems.FirstOrDefault(t => t.Id == id);
        if (task is not null)
        {
            task.State = ToDoItemState.Completed;
            task.StateChangedAt = DateTime.Now;
        }
    }

    void CheckTaskCount()
    {
        if (toDoItems.Any() && toDoItems.Count == _settings.MaxTaskCount) throw new TaskCountLimitException(_settings.MaxTaskCount);
    }

    bool IsValidTaskLength(string? task)
    {
        ValidateString(task);

        if (task!.Length > _settings.MaxTaskLength) throw new TaskLengthLimitException(task.Length, _settings.MaxTaskLength);

        return true;
    }

    bool IsDuplicateTask(string task)
    {
        foreach (var t in toDoItems)
        {
            if (task.Equals(t.Name)) throw new DuplicateTaskException(task);
        }

        return false;
    }

    void ValidateString(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException($"Строка не должна быть пустой");
    }
}