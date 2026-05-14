using GuideBot.Entities;

namespace GuideBot.Services;

public interface IGuideUserService
{
    Task<GuideUser?> GetUserAsync(long telegramUserId, CancellationToken cancellationToken);
    Task<GuideUser?> RegisterUserAsync(long telegramUserId, string telegramUserName, GuideUserRole role, CancellationToken cancellationToken);
}
