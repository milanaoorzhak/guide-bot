namespace GuideBot;

public class UserService : IUserService
{
    List<ToDoUser> users = new();

    public ToDoUser? GetUser(long telegramUserId)
    {
        return users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }

    public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
    {
        users.Add(new ToDoUser(telegramUserId, telegramUserName));

        return GetUser(telegramUserId);
    }
}