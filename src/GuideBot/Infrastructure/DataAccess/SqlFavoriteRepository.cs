using GuideBot.DataAccess;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlFavoriteRepository : IFavoriteRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlFavoriteRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Attraction>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var favorites = await dataContext.AttractionFavorites
            .Where(f => f.UserId == userId)
            .Join(dataContext.Attractions, f => f.AttractionId, a => a.Id, (f, a) => a)
            .ToListAsync(cancellationToken);

        return favorites.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var favorite = await dataContext.AttractionFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.AttractionId == attractionId, cancellationToken);

        return favorite is not null;
    }

    public async Task AddAsync(AttractionFavorite favorite, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(favorite), token: cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.AttractionFavorites
            .Where(f => f.UserId == userId && f.AttractionId == attractionId)
            .DeleteAsync(cancellationToken);
    }
}