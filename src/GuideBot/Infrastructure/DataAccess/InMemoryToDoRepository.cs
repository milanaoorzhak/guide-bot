using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _toDoItems = new();

    public async Task AddAsync(ToDoItem item, CancellationToken token)
    {
        _toDoItems.Add(item);
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken token)
    {
        return _toDoItems.Count(t => t.User.UserId == userId && t.State == ToDoItemState.Active);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        var item = _toDoItems.FirstOrDefault(item => item.Id == id);
        if (item is null) return;
        _toDoItems.Remove(item!);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken token)
    {
        var userTasks = await GetAllByUserIdAsync(userId, token);
        foreach (var t in userTasks)
        {
            if (name.Equals(t.Name)) throw new DuplicateTaskException(name);
        }

        return false;
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken token)
    {
        return _toDoItems.FirstOrDefault(t => t.Id == id);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token)
    {
        return _toDoItems.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token)
    {
        return _toDoItems.Where(t => t.User.UserId == userId).ToList();
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken token)
    {
        var index = _toDoItems.FindIndex(x => x.Id == item.Id);
        if (index == -1) return;
        _toDoItems[index] = item;
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken token)
    {
        return _toDoItems
                    .Where(x => x.User.UserId == userId)
                    .Where(predicate)
                    .ToList()
                    .AsReadOnly();
    }
}
