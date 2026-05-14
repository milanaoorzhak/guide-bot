using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface IRouteRepository
{
    Task<IReadOnlyList<Route>> GetUserRoutesAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Route>> GetThematicRoutesAsync(CancellationToken cancellationToken);
    Task<Route?> GetByIdAsync(Guid routeId, CancellationToken cancellationToken);
    Task AddAsync(Route route, CancellationToken cancellationToken);
    Task UpdateAsync(Route route, CancellationToken cancellationToken);
    Task DeleteAsync(Guid routeId, CancellationToken cancellationToken);
    Task AddAttractionAsync(RouteAttraction routeAttraction, CancellationToken cancellationToken);
    Task RemoveAttractionAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Attraction>> GetRouteAttractionsAsync(Guid routeId, CancellationToken cancellationToken);
}