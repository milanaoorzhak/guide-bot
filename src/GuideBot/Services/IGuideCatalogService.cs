using GuideBot.Entities;

namespace GuideBot.Services;

public interface IGuideCatalogService
{
    Task<IReadOnlyList<AttractionCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<AttractionCategory?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Attraction>> GetAttractionsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Attraction>> SearchAttractionsAsync(string query, CancellationToken cancellationToken);
    Task<Attraction?> GetAttractionAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<CityInfo> GetCityInfoAsync(CancellationToken cancellationToken);
}
