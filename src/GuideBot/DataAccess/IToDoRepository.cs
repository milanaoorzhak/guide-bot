namespace GuideBot.DataAccess;

public interface IToDoRepository
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token);
    Task<ToDoItem?> GetAsync(Guid id, CancellationToken token);
    Task AddAsync(ToDoItem item, CancellationToken token);
    Task UpdateAsync(ToDoItem item, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);
    Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken token);
    Task<int> CountActiveAsync(Guid userId, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken token);
}