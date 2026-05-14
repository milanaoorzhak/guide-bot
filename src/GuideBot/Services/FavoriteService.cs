using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface IFavoriteService
{
    Task<IReadOnlyList<Attraction>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
    Task AddFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
    Task RemoveFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
}

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IGuideCatalogService _catalogService;

    public FavoriteService(IFavoriteRepository favoriteRepository, IGuideCatalogService catalogService)
    {
        _favoriteRepository = favoriteRepository;
        _catalogService = catalogService;
    }

    public async Task<IReadOnlyList<Attraction>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _favoriteRepository.GetUserFavoritesAsync(userId, cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        return await _favoriteRepository.IsFavoriteAsync(userId, attractionId, cancellationToken);
    }

    public async Task AddFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        var favorite = new AttractionFavorite(userId, attractionId);
        await _favoriteRepository.AddAsync(favorite, cancellationToken);
    }

    public async Task RemoveFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        await _favoriteRepository.DeleteAsync(userId, attractionId, cancellationToken);
    }
}