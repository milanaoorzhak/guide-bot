using GuideBot.Entities;

namespace GuideBot;

public class ToDoItem
{
    public Guid Id { get; set; }
    public ToDoUser User { get; set; }
    public string Name { get; set; }
    public ToDoList? List { get; set; }

    public DateTime CreatedAt { get; set; }
    public ToDoItemState State { get; set; }
    public DateTime? StateChangedAt { get; set; }
    public DateTime Deadline { get; set; }

    public ToDoItem(ToDoUser user, string name, DateTime deadline, ToDoList? list = null)
    {
        Id = Guid.NewGuid();
        User = user;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        State = ToDoItemState.Active;
        Deadline = deadline;
        List = list;
    }
}

public enum ToDoItemState
{
    Active,
    Completed
}