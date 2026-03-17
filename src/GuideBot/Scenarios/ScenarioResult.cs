namespace GuideBot.Scenarios;

public enum ScenarioResult
{
    /// <summary>
    /// Переход к следующему шагу. Сообщение обработано, но сценарий еще не завершен
    /// </summary>
    Transition,

    /// <summary>
    /// Сценарий завершен
    /// </summary>
    Completed
}