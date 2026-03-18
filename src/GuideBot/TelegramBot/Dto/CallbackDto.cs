namespace GuideBot.TelegramBot.Dto;

public class CallbackDto
{
    public string Action { get; set; }

    public CallbackDto(string action)
    {
        Action = action;
    }

    public static CallbackDto FromString(string input)
    {
        var parts = input.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var action = parts.Length > 0 ? parts[0] : input;
        return new CallbackDto(action);
    }

    public override string ToString()
    {
        return Action;
    }
}
