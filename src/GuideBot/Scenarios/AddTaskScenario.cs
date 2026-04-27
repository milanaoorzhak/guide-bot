using GuideBot.Entities;
using GuideBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot.Scenarios;

public class AddTaskScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoListService _toDoListService;
    private static readonly ReplyKeyboardMarkup CancelKeyboard = new ReplyKeyboardMarkup(new[]
    {
        new KeyboardButton("/cancel")
    })
    { ResizeKeyboard = true };

    public AddTaskScenario(
        IUserService userService,
        IToDoService toDoService,
        IToDoListService toDoListService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoListService = toDoListService;
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
                if (user == null)
                {
                    await bot.SendMessage(message.Chat.Id, "Пожалуйста, сначала зарегистрируйтесь командой /start", replyMarkup: CancelKeyboard, cancellationToken: ct);
                    return ScenarioResult.Completed;
                }
                context.Data["TelegramUserId"] = message.From.Id;
                context.Data["User"] = user;
                context.CurrentStep = "Name";
                await bot.SendMessage(message.Chat.Id, "Введите название задачи:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "Name":
                context.Data["TaskName"] = message.Text!;
                context.CurrentStep = "List";

                var lists = await _toDoListService.GetUserLists(((ToDoUser)context.Data["User"]).UserId, ct);
                var buttons = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("📌Без списка", new ToDoListCallbackDto("selectlist", null).ToString())
                };

                foreach (var list in lists)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(list.Name, new ToDoListCallbackDto("selectlist", list.Id).ToString()));
                }

                var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));
                await bot.SendMessage(message.Chat.Id, "Выберите список для задачи:", replyMarkup: keyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "List":
                if (context.Data.TryGetValue("CallbackQueryData", out var callbackDataObj) && callbackDataObj is string callbackData)
                {
                    var callbackDto = ToDoListCallbackDto.FromString(callbackData);
                    context.Data["SelectedListId"] = callbackDto.ToDoListId;
                    context.Data.Remove("CallbackQueryData");
                    context.CurrentStep = "Deadline";
                    await bot.SendMessage(message.Chat.Id, "Введите дедлайн задачи в формате dd.MM.yyyy:", replyMarkup: CancelKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                }
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
                ToDoList? selectedList = null;

                if (context.Data.TryGetValue("SelectedListId", out var listIdObj) && listIdObj is Guid listId)
                {
                    selectedList = await _toDoListService.Get(listId, ct);
                }

                await _toDoService.AddAsync(todoUser!, taskName, deadline, selectedList, ct);
                await bot.SendMessage(message.Chat.Id, $"Задача \"{taskName}\" добавлена.", cancellationToken: ct);
                return ScenarioResult.Completed;

            default:
                return ScenarioResult.Completed;
        }
    }
}
