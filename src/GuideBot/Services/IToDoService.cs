namespace GuideBot;

public interface IToDoService
{
    IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
    IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
    ToDoItem? Add(ToDoUser user, string name);
    void MarkAsCompleted(Guid id);
    void Delete(Guid id);
    IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
}