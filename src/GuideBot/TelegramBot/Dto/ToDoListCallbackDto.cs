namespace GuideBot.TelegramBot.Dto;

public class ToDoListCallbackDto : CallbackDto
{
    public Guid? ToDoListId { get; set; }

    public ToDoListCallbackDto(string action, Guid? toDoListId = null) : base(action)
    {
        ToDoListId = toDoListId;
    }

    public static new ToDoListCallbackDto FromString(string input)
    {
        var parts = input.Split('|');
        var action = parts.Length > 0 ? parts[0] : input;
        Guid? toDoListId = null;

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            if (Guid.TryParse(parts[1], out var guid))
            {
                toDoListId = guid;
            }
        }

        return new ToDoListCallbackDto(action, toDoListId);
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoListId}";
    }
}
