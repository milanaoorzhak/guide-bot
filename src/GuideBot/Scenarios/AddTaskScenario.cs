using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot.Scenarios;

public class AddTaskScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private static readonly ReplyKeyboardMarkup CancelKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton("/cancel")
    })
    { ResizeKeyboard = true };

    public AddTaskScenario(
        IUserService userService,
        IToDoService toDoService)
    {
        _userService = userService;
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
    {
        switch (context.CurrentStep)
        {
            case null:
                var user = await _userService.GetUserAsync(message.From!.Id, ct);
                context.Data["TelegramUserId"] = message.From.Id;
                context.CurrentStep = "Name";
                await bot.SendMessage(message.Chat.Id, "Введите название задачи:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "Name":
                context.Data["TaskName"] = message.Text!;
                context.CurrentStep = "Deadline";
                await bot.SendMessage(message.Chat.Id, "Введите дедлайн задачи в формате dd.MM.yyyy:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "Deadline":
                if (!DateTime.TryParseExact(message.Text!, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var deadline))
                {
                    await bot.SendMessage(message.Chat.Id, "Неверный формат даты. Пожалуйста, введите дату в формате dd.MM.yyyy:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                }

                var telegramUserId = (long)context.Data["TelegramUserId"];
                var todoUser = await _userService.GetUserAsync(telegramUserId, ct);
                var taskName = (string)context.Data["TaskName"];
                await _toDoService.AddAsync(todoUser!, taskName, deadline, ct);
                await bot.SendMessage(message.Chat.Id, $"Задача \"{taskName}\" добавлена.", cancellationToken: ct);
                return ScenarioResult.Completed;

            default:
                return ScenarioResult.Completed;
        }
    }
}