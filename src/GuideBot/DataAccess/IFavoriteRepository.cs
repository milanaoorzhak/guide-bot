using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface IFavoriteRepository
{
    Task<IReadOnlyList<Attraction>> GetUserFavoritesAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsFavoriteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
    Task AddAsync(AttractionFavorite favorite, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
}