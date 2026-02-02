using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new();

    public async Task AddAsync(ToDoUser user, CancellationToken token)
    {
        _users.Add(user);
    }

    public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken token)
    {
        return _users.FirstOrDefault(u => u.UserId == userId);
    }

    public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken token)
    {
        return _users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }
}