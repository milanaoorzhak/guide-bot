using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public class GuideCatalogService : IGuideCatalogService
{
    private readonly IGuideCatalogRepository _guideCatalogRepository;

    public GuideCatalogService(IGuideCatalogRepository guideCatalogRepository)
    {
        _guideCatalogRepository = guideCatalogRepository;
    }

    public Task<IReadOnlyList<AttractionCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return _guideCatalogRepository.GetCategoriesAsync(cancellationToken);
    }

    public Task<AttractionCategory?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return _guideCatalogRepository.GetCategoryAsync(categoryId, cancellationToken);
    }

    public Task<IReadOnlyList<Attraction>> GetAttractionsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return _guideCatalogRepository.GetAttractionsByCategoryAsync(categoryId, cancellationToken);
    }

    public Task<IReadOnlyList<Attraction>> SearchAttractionsAsync(string query, CancellationToken cancellationToken)
    {
        return _guideCatalogRepository.SearchAttractionsAsync(query, cancellationToken);
    }

    public Task<Attraction?> GetAttractionAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        return _guideCatalogRepository.GetAttractionAsync(attractionId, cancellationToken);
    }

    public async Task<CityInfo> GetCityInfoAsync(CancellationToken cancellationToken)
    {
        return await _guideCatalogRepository.GetCityInfoAsync(cancellationToken)
            ?? new CityInfo
            {
                Title = "О городе Кызыл",
                Description = "Информация о городе пока не добавлена.",
                MapUrl = "https://yandex.ru/maps/?text=%D0%9A%D1%8B%D0%B7%D1%8B%D0%BB"
            };
    }
}
