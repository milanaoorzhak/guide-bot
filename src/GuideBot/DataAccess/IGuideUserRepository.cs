using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface IGuideUserRepository
{
    Task<GuideUser?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken);
    Task AddAsync(GuideUser user, CancellationToken cancellationToken);
}
