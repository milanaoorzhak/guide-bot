using GuideBot.DataAccess;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlGuideCatalogRepository : IGuideCatalogRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlGuideCatalogRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<AttractionCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var categories = await dataContext.AttractionCategories
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return categories.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<AttractionCategory?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var category = await dataContext.AttractionCategories
            .FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);

        return category is null ? null : GuideModelMapper.MapFromModel(category);
    }

    public async Task<IReadOnlyList<Attraction>> GetAttractionsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var attractions = await dataContext.Attractions
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return attractions.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Attraction>> SearchAttractionsAsync(string query, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var normalizedQuery = query.Trim().ToLowerInvariant();

        var attractions = await dataContext.Attractions
            .Where(x =>
                Sql.Lower(x.Name ?? string.Empty).Contains(normalizedQuery) ||
                Sql.Lower(x.ShortDescription ?? string.Empty).Contains(normalizedQuery) ||
                Sql.Lower(x.FullDescription ?? string.Empty).Contains(normalizedQuery))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return attractions.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<Attraction?> GetAttractionAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var attraction = await dataContext.Attractions
            .FirstOrDefaultAsync(x => x.Id == attractionId, cancellationToken);

        return attraction is null ? null : GuideModelMapper.MapFromModel(attraction);
    }

    public async Task<CityInfo?> GetCityInfoAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var cityInfo = await dataContext.CityInfos
            .FirstOrDefaultAsync(cancellationToken);

        return cityInfo is null ? null : GuideModelMapper.MapFromModel(cityInfo);
    }

    public async Task AddAttractionAsync(Attraction attraction, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var model = GuideModelMapper.MapToModel(attraction);
        await dataContext.InsertAsync(model, token: cancellationToken);
    }

    public async Task UpdateAttractionAsync(Attraction attraction, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var model = GuideModelMapper.MapToModel(attraction);
        await dataContext.UpdateAsync(model, token: cancellationToken);
    }
}
