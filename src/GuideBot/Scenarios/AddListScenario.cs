using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot.Scenarios;

public class AddListScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoListService _toDoListService;
    private static readonly ReplyKeyboardMarkup CancelKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton("/cancel")
    })
    { ResizeKeyboard = true };

    public AddListScenario(
        IUserService userService,
        IToDoListService toDoListService)
    {
        _userService = userService;
        _toDoListService = toDoListService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.AddList;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
    {
        switch (context.CurrentStep)
        {
            case null:
                var user = await _userService.GetUserAsync(message.From!.Id, ct);
                context.Data["TelegramUserId"] = message.From.Id;
                context.Data["User"] = user!;
                context.CurrentStep = "Name";
                await bot.SendMessage(message.Chat.Id, "Введите название списка:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "Name":
                var todoUser = (ToDoUser)context.Data["User"];
                await _toDoListService.Add(todoUser, message.Text!, ct);
                await bot.SendMessage(message.Chat.Id, $"Список \"{message.Text}\" добавлен.", cancellationToken: ct);
                return ScenarioResult.Completed;

            default:
                return ScenarioResult.Completed;
        }
    }
}
