using GuideBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GuideBot.BackgroundTasks;

public class ResetScenarioBackgroundTask : BackgroundTask
{
    private readonly TimeSpan _resetScenarioTimeout;
    private readonly IScenarioContextRepository _scenarioRepository;
    private readonly ITelegramBotClient _bot;

    public ResetScenarioBackgroundTask(
        TimeSpan resetScenarioTimeout,
        IScenarioContextRepository scenarioRepository,
        ITelegramBotClient bot)
        : base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
    {
        _resetScenarioTimeout = resetScenarioTimeout;
        _scenarioRepository = scenarioRepository;
        _bot = bot;
    }

    protected override async Task Execute(CancellationToken ct)
    {
        var contexts = await _scenarioRepository.GetContexts(ct);
        var now = DateTime.UtcNow;

        foreach (var context in contexts)
        {
            if (now - context.CreatedAt > _resetScenarioTimeout)
            {
                var userId = context.Data.ContainsKey("UserId")
                    ? (long)context.Data["UserId"]!
                    : 0;

                if (userId != 0)
                {
                    await _scenarioRepository.ResetContext(userId, ct);

                    await _bot.SendMessage(
                        chatId: new ChatId(userId),
                        text: $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                        cancellationToken: ct);
                }
            }
        }
    }
}