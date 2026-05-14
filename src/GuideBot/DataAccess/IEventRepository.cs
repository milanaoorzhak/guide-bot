using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface IEventRepository
{
    Task<IReadOnlyList<GuideEvent>> GetActiveEventsAsync(CancellationToken cancellationToken);
    Task<GuideEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken);
    Task AddAsync(GuideEvent guideEvent, CancellationToken cancellationToken);
    Task UpdateAsync(GuideEvent guideEvent, CancellationToken cancellationToken);
    Task DeleteAsync(Guid eventId, CancellationToken cancellationToken);
}