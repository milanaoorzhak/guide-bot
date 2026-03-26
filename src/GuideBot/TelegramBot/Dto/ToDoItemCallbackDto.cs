namespace GuideBot.TelegramBot.Dto;

public class ToDoItemCallbackDto : CallbackDto
{
    public Guid ToDoItemId { get; set; }

    public ToDoItemCallbackDto(string action, Guid toDoItemId) : base(action)
    {
        ToDoItemId = toDoItemId;
    }

    public static new ToDoItemCallbackDto FromString(string input)
    {
        var parts = input.Split('|');
        var action = parts.Length > 0 ? parts[0] : input;
        Guid toDoItemId = Guid.Empty;

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            if (Guid.TryParse(parts[1], out var guid))
            {
                toDoItemId = guid;
            }
        }

        return new ToDoItemCallbackDto(action, toDoItemId);
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{ToDoItemId}";
    }
}
