namespace GuideBot;

public class ToDoUser
{
    public Guid UserId { get; set; }
    public long TelegramUserId { get; set; }
    public string TelegramUserName { get; set; }
    public DateTime RegisteredAt { get; set; }

    public ToDoUser(long telegramUserId, string telegramUserName)
    {
        UserId = Guid.NewGuid();
        TelegramUserId = telegramUserId;
        TelegramUserName = telegramUserName;
        RegisteredAt = DateTime.UtcNow;
    }
}