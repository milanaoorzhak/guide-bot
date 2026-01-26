using System.Runtime.CompilerServices;
using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> toDoItems = new();

    public void Add(ToDoItem item)
    {
        toDoItems.Add(item);
    }

    public int CountActive(Guid userId)
    {
        return toDoItems.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).Count();
    }

    public void Delete(Guid id)
    {
        var item = toDoItems.FirstOrDefault(item => item.Id == id);
        if (item is null) return;
        toDoItems.Remove(item!);
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
        return toDoItems.FirstOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return toDoItems.Where(t => t.User.UserId == userId && t.State == ToDoItemState.Active).ToList();
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return toDoItems.Where(t => t.User.UserId == userId).ToList();
    }

    public void Update(ToDoItem item)
    {
        var index = toDoItems.FindIndex(x => x.Id == item.Id);
        if (index == -1) return;
        toDoItems[index] = item;
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return toDoItems
                    .Where(x => x.User.UserId == userId)
                    .Where(predicate)
                    .ToList()
                    .AsReadOnly();
    }
}
