namespace GuideBot.TelegramBot.Dto;

public class CallbackDto
{
    public string Action { get; set; }

    public CallbackDto(string action)
    {
        Action = action;
    }
}
