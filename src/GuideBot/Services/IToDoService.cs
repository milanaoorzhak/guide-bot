namespace GuideBot;

public interface IToDoService
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token);
    Task<ToDoItem?> AddAsync(ToDoUser user, string name, CancellationToken token);
    Task MarkAsCompletedAsync(Guid id, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken token);
}