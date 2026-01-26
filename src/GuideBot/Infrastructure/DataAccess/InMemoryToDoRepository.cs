using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _toDoItems = new();

    public void Add(ToDoItem item)
    {
        _toDoItems.Add(item);
    }

    public int CountActive(Guid userId)
    {
        return _toDoItems.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
    }

    public void Delete(Guid id)
    {
        var item = _toDoItems.FirstOrDefault(item => item.Id == id);
        if (item is null) return;
        _toDoItems.Remove(item!);
    }

    public bool ExistsByName(Guid userId, string name)
    {
        var userTasks = GetAllByUserId(userId);
        foreach (var t in userTasks)
        {
            if (name.Equals(t.Name)) throw new DuplicateTaskException(name);
        }

        return false;
    }

    public ToDoItem? Get(Guid id)
    {
        return _toDoItems.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _toDoItems.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoItems.Where(t => t.User.UserId == userId).ToList();
    }

    public void Update(ToDoItem item)
    {
        var index = _toDoItems.FindIndex(x => x.Id == item.Id);
        if (index == -1) return;
        _toDoItems[index] = item;
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _toDoItems
                    .Where(x => x.User.UserId == userId)
                    .Where(predicate)
                    .ToList()
                    .AsReadOnly();
    }
}
