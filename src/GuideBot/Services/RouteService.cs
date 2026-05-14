using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface IRouteService
{
    Task<IReadOnlyList<Route>> GetUserRoutesAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Route>> GetThematicRoutesAsync(CancellationToken cancellationToken);
    Task<Route?> GetRouteAsync(Guid routeId, CancellationToken cancellationToken);
    Task<Route> CreateRouteAsync(Guid userId, string name, string description, bool isThematic, CancellationToken cancellationToken);
    Task UpdateRouteAsync(Route route, CancellationToken cancellationToken);
    Task DeleteRouteAsync(Guid routeId, CancellationToken cancellationToken);
    Task AddAttractionToRouteAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken);
    Task RemoveAttractionFromRouteAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Attraction>> GetRouteAttractionsAsync(Guid routeId, CancellationToken cancellationToken);
}

public class RouteService : IRouteService
{
    private readonly IRouteRepository _routeRepository;

    public RouteService(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<IReadOnlyList<Route>> GetUserRoutesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _routeRepository.GetUserRoutesAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Route>> GetThematicRoutesAsync(CancellationToken cancellationToken)
    {
        return await _routeRepository.GetThematicRoutesAsync(cancellationToken);
    }

    public async Task<Route?> GetRouteAsync(Guid routeId, CancellationToken cancellationToken)
    {
        return await _routeRepository.GetByIdAsync(routeId, cancellationToken);
    }

    public async Task<Route> CreateRouteAsync(Guid userId, string name, string description, bool isThematic, CancellationToken cancellationToken)
    {
        var route = new Route(userId, name, description, isThematic);
        await _routeRepository.AddAsync(route, cancellationToken);
        return route;
    }

    public async Task UpdateRouteAsync(Route route, CancellationToken cancellationToken)
    {
        await _routeRepository.UpdateAsync(route, cancellationToken);
    }

    public async Task DeleteRouteAsync(Guid routeId, CancellationToken cancellationToken)
    {
        await _routeRepository.DeleteAsync(routeId, cancellationToken);
    }

    public async Task AddAttractionToRouteAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken)
    {
        var attractions = await _routeRepository.GetRouteAttractionsAsync(routeId, cancellationToken);
        var routeAttraction = new RouteAttraction(routeId, attractionId, attractions.Count + 1);
        await _routeRepository.AddAttractionAsync(routeAttraction, cancellationToken);
    }

    public async Task RemoveAttractionFromRouteAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken)
    {
        await _routeRepository.RemoveAttractionAsync(routeId, attractionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Attraction>> GetRouteAttractionsAsync(Guid routeId, CancellationToken cancellationToken)
    {
        return await _routeRepository.GetRouteAttractionsAsync(routeId, cancellationToken);
    }
}