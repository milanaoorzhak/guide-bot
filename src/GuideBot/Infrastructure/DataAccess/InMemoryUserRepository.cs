using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    List<ToDoUser> users = new();

    public void Add(ToDoUser user)
    {
        users.Add(user);
    }

    public ToDoUser? GetUser(Guid userId)
    {
        return users.FirstOrDefault(u => u.UserId == userId);
    }

    public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
    {
        return users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }
}