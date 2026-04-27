using System.Collections.Concurrent;

namespace GuideBot.Scenarios;

public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    ConcurrentDictionary<long, ScenarioContext> scenarios = new();
    public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        return scenarios.TryGetValue(userId, out var context) ? context : null;
    }

    public async Task ResetContext(long userId, CancellationToken ct)
    {
        scenarios.TryRemove(userId, out _);
    }

    public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
    {
        scenarios.AddOrUpdate(userId, context, (_, _) => context);
    }

    public async Task<IReadOnlyList<ScenarioContext>> GetContexts(CancellationToken ct)
    {
        return scenarios.Values.ToList();
    }
}