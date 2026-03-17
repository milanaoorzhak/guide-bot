namespace GuideBot.Scenarios;

public class InMemoryScenarioContextRepository : IScenarioContextRepository
{
    Dictionary<long, ScenarioContext> scenarios = new();
    public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
    {
        return scenarios.FirstOrDefault(x => x.Key == userId).Value;
    }

    public async Task ResetContext(long userId, CancellationToken ct)
    {
        scenarios.Remove(userId);
    }

    public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
    {
        scenarios.Add(userId, context);
    }
}