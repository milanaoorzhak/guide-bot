using GuideBot.DataAccess;
using GuideBot.DataAccess.Models;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlRouteRepository : IRouteRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlRouteRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Route>> GetUserRoutesAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var routes = await dataContext.Routes
            .Where(r => r.UserId == userId && !r.IsThematic)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return routes.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Route>> GetThematicRoutesAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var routes = await dataContext.Routes
            .Where(r => r.IsThematic)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return routes.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<Route?> GetByIdAsync(Guid routeId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var route = await dataContext.Routes
            .FirstOrDefaultAsync(r => r.Id == routeId, cancellationToken);

        return route is null ? null : GuideModelMapper.MapFromModel(route);
    }

    public async Task AddAsync(Route route, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(route), token: cancellationToken);
    }

    public async Task UpdateAsync(Route route, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.Routes
            .Where(r => r.Id == route.Id)
            .UpdateAsync(r => new RouteModel
            {
                Name = route.Name,
                Description = route.Description,
                IsThematic = route.IsThematic
            }, token: cancellationToken);
    }

    public async Task DeleteAsync(Guid routeId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.RouteAttractions
            .Where(ra => ra.RouteId == routeId)
            .DeleteAsync(cancellationToken);

        await dataContext.Routes
            .Where(r => r.Id == routeId)
            .DeleteAsync(cancellationToken);
    }

    public async Task AddAttractionAsync(RouteAttraction routeAttraction, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var model = new RouteAttractionModel
        {
            Id = routeAttraction.Id,
            RouteId = routeAttraction.RouteId,
            AttractionId = routeAttraction.AttractionId,
            SortOrder = routeAttraction.SortOrder
        };
        await dataContext.InsertAsync(model, token: cancellationToken);
    }

    public async Task RemoveAttractionAsync(Guid routeId, Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.RouteAttractions
            .Where(ra => ra.RouteId == routeId && ra.AttractionId == attractionId)
            .DeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Attraction>> GetRouteAttractionsAsync(Guid routeId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var attractions = await dataContext.RouteAttractions
            .Where(ra => ra.RouteId == routeId)
            .Join(dataContext.Attractions, ra => ra.AttractionId, a => a.Id, (ra, a) => a)
            .ToListAsync(cancellationToken);

        return attractions.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }
}