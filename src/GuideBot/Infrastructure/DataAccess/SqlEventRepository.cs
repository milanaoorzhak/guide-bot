using GuideBot.DataAccess;
using GuideBot.DataAccess.Models;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlEventRepository : IEventRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlEventRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<GuideEvent>> GetActiveEventsAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var events = await dataContext.Events
            .Where(e => e.IsActive)
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);

        return events.Select(GuideModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<GuideEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var guideEvent = await dataContext.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        return guideEvent is null ? null : GuideModelMapper.MapFromModel(guideEvent);
    }

    public async Task AddAsync(GuideEvent guideEvent, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(guideEvent), token: cancellationToken);
    }

    public async Task UpdateAsync(GuideEvent guideEvent, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.Events
            .Where(e => e.Id == guideEvent.Id)
            .UpdateAsync(e => new EventModel
            {
                Name = guideEvent.Name,
                Description = guideEvent.Description,
                StartDate = guideEvent.StartDate,
                EndDate = guideEvent.EndDate,
                Location = guideEvent.Location,
                IsActive = guideEvent.IsActive
            }, token: cancellationToken);
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.Events
            .Where(e => e.Id == eventId)
            .DeleteAsync(cancellationToken);
    }
}