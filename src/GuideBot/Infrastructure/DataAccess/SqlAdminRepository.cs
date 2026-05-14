using GuideBot.DataAccess;
using GuideBot.DataAccess.Models;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlAdminRepository : IAdminRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlAdminRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<GuideUser>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var users = await dataContext.GuideUsers
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync(cancellationToken);

        return users.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task UpdateUserRoleAsync(Guid userId, GuideUserRole newRole, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.GuideUsers
            .Where(u => u.Id == userId)
            .UpdateAsync(u => new GuideUserModel { Role = (int)newRole }, token: cancellationToken);
    }

    public async Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.GuideUsers.CountAsync(cancellationToken);
    }

    public async Task<int> GetTotalAttractionsCountAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.Attractions.CountAsync(cancellationToken);
    }

    public async Task<int> GetTotalCommentsCountAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.AttractionComments.CountAsync(cancellationToken);
    }

    public async Task<int> GetActiveEventsCountAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.Events.CountAsync(e => e.IsActive, cancellationToken);
    }
}