using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public class GuideUserService : IGuideUserService
{
    private readonly IGuideUserRepository _guideUserRepository;

    public GuideUserService(IGuideUserRepository guideUserRepository)
    {
        _guideUserRepository = guideUserRepository;
    }

    public Task<GuideUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        return _guideUserRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
    }

    public async Task<GuideUser?> RegisterUserAsync(
        long telegramUserId,
        string telegramUserName,
        GuideUserRole role,
        CancellationToken cancellationToken)
    {
        var user = new GuideUser(telegramUserId, telegramUserName, role);
        await _guideUserRepository.AddAsync(user, cancellationToken);
        return await _guideUserRepository.GetByTelegramUserIdAsync(telegramUserId, cancellationToken);
    }
}
