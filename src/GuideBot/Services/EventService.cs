using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface IEventService
{
    Task<IReadOnlyList<GuideEvent>> GetActiveEventsAsync(CancellationToken cancellationToken);
    Task<GuideEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task<GuideEvent> CreateEventAsync(string name, string description, DateTime startDate, DateTime? endDate, string location, CancellationToken cancellationToken);
    Task UpdateEventAsync(GuideEvent guideEvent, CancellationToken cancellationToken);
    Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken);
}

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IReadOnlyList<GuideEvent>> GetActiveEventsAsync(CancellationToken cancellationToken)
    {
        return await _eventRepository.GetActiveEventsAsync(cancellationToken);
    }

    public async Task<GuideEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return await _eventRepository.GetByIdAsync(eventId, cancellationToken);
    }

    public async Task<GuideEvent> CreateEventAsync(string name, string description, DateTime startDate, DateTime? endDate, string location, CancellationToken cancellationToken)
    {
        var guideEvent = new GuideEvent(name, description, startDate, endDate, location);
        await _eventRepository.AddAsync(guideEvent, cancellationToken);
        return guideEvent;
    }

    public async Task UpdateEventAsync(GuideEvent guideEvent, CancellationToken cancellationToken)
    {
        await _eventRepository.UpdateAsync(guideEvent, cancellationToken);
    }

    public async Task DeleteEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        await _eventRepository.DeleteAsync(eventId, cancellationToken);
    }
}