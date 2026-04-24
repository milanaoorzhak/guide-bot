using GuideBot.DataAccess;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlUserRepository : IUserRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var user = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(u => u.UserId == userId, token);

        return user != null ? ModelMapper.MapFromModel(user) : null;
    }

    public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var user = await dbContext.ToDoUsers
            .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, token);

        return user != null ? ModelMapper.MapFromModel(user) : null;
    }

    public async Task AddAsync(ToDoUser user, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(user);
        await dbContext.InsertAsync(model);
    }
}
