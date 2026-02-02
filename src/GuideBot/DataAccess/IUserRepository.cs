namespace GuideBot.DataAccess;

public interface IUserRepository
{
    Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken token);
    Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken token);
    Task AddAsync(ToDoUser user, CancellationToken token);
}