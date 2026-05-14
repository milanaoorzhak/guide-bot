namespace GuideBot.Entities;

public class GuideUser
{
    public Guid Id { get; set; }
    public long TelegramUserId { get; set; }
    public string TelegramUserName { get; set; }
    public GuideUserRole Role { get; set; }
    public DateTime RegisteredAt { get; set; }

    public GuideUser(long telegramUserId, string telegramUserName, GuideUserRole role)
    {
        Id = Guid.NewGuid();
        TelegramUserId = telegramUserId;
        TelegramUserName = telegramUserName;
        Role = role;
        RegisteredAt = DateTime.UtcNow;
    }
}
