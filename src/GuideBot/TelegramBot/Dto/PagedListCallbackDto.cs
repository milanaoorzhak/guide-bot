namespace GuideBot.TelegramBot.Dto;

public class PagedListCallbackDto : ToDoListCallbackDto
{
    public int Page { get; set; }

    public PagedListCallbackDto(string action, Guid? toDoListId = null, int page = 0) : base(action, toDoListId)
    {
        Page = page;
    }

    public static new PagedListCallbackDto FromString(string input)
    {
        var parts = input.Split('|');
        var action = parts.Length > 0 ? parts[0] : input;
        Guid? toDoListId = null;
        int page = 0;

        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            if (Guid.TryParse(parts[1], out var guid))
            {
                toDoListId = guid;
            }
        }

        if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
        {
            if (int.TryParse(parts[2], out var parsedPage))
            {
                page = parsedPage;
            }
        }

        return new PagedListCallbackDto(action, toDoListId, page);
    }

    public override string ToString()
    {
        return $"{base.ToString()}|{Page}";
    }
}
