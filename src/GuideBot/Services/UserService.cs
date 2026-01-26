using GuideBot.DataAccess;

namespace GuideBot;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public ToDoUser? GetUser(long telegramUserId)
    {
        return _userRepository.GetUserByTelegramUserId(telegramUserId);
    }

    public ToDoUser? RegisterUser(long telegramUserId, string telegramUserName)
    {
        _userRepository.Add(new ToDoUser(telegramUserId, telegramUserName));

        return _userRepository.GetUserByTelegramUserId(telegramUserId);
    }
}