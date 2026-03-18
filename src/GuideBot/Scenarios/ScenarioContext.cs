namespace GuideBot.Scenarios;

public class ScenarioContext(ScenarioType scenario)
{
    public ScenarioType CurrentScenario { get; set; } = scenario;
    public string? CurrentStep { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}