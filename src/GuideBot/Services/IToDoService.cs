using GuideBot.Entities;

namespace GuideBot;

public interface IToDoService
{
    Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token);
    Task<ToDoItem?> AddAsync(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken token);
    Task MarkAsCompletedAsync(Guid id, CancellationToken token);
    Task DeleteAsync(Guid id, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken token);
    Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken token);
    Task DeleteByUserIdAndListAsync(Guid userId, Guid listId, CancellationToken token);
}