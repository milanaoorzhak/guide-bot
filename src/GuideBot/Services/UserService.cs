using GuideBot.DataAccess;

namespace GuideBot;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken token)
    {
        return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, token);
    }

    public async Task<ToDoUser?> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken token)
    {
        await _userRepository.AddAsync(new ToDoUser(telegramUserId, telegramUserName), token);

        return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, token);
    }
}