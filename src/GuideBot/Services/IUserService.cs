namespace GuideBot;

public interface IUserService
{
    Task<ToDoUser?> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken token);
    Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken token);
}