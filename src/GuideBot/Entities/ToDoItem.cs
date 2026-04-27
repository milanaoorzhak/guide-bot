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
}

public enum ToDoItemState
{
    Active,
    Completed
}