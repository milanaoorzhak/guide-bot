using GuideBot.DataAccess;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlGuideUserRepository : IGuideUserRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlGuideUserRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<GuideUser?> GetByTelegramUserIdAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var user = await dataContext.GuideUsers
            .FirstOrDefaultAsync(x => x.TelegramUserId == telegramUserId, cancellationToken);

        return user is null ? null : GuideModelMapper.MapFromModel(user);
    }

    public async Task AddAsync(GuideUser user, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(user), token: cancellationToken);
    }
}
