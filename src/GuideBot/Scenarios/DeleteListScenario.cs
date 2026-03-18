using GuideBot.Entities;
using GuideBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot.Scenarios;

public class DeleteListScenario : IScenario
{
    private readonly IUserService _userService;
    private readonly IToDoListService _toDoListService;
    private readonly IToDoService _toDoService;

    public DeleteListScenario(
        IUserService userService,
        IToDoListService toDoListService,
        IToDoService toDoService)
    {
        _userService = userService;
        _toDoListService = toDoListService;
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteList;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
    {
        switch (context.CurrentStep)
        {
            case null:
                var user = await _userService.GetUserAsync(message.From!.Id, ct);
                if (user == null)
                {
                    await bot.SendMessage(message.Chat.Id, "Пожалуйста, сначала зарегистрируйтесь командой /start", cancellationToken: ct);
                    return ScenarioResult.Completed;
                }
                context.Data["TelegramUserId"] = message.From.Id;
                context.Data["User"] = user;
                context.CurrentStep = "Approve";

                var lists = await _toDoListService.GetUserLists(user.UserId, ct);
                var buttons = new List<InlineKeyboardButton>();

                foreach (var list in lists)
                {
                    var callbackData = new ToDoListCallbackDto("deletelist", list.Id).ToString();
                    buttons.Add(InlineKeyboardButton.WithCallbackData(list.Name, callbackData));
                }

                var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));
                await bot.SendMessage(message.Chat.Id, "Выберете список для удаления:", replyMarkup: keyboard, cancellationToken: ct);
                return ScenarioResult.Transition;

            case "Approve":
                if (context.Data.TryGetValue("CallbackQueryData", out var callbackDataObj) && callbackDataObj is string callbackDataStr)
                {
                    var callbackDto = ToDoListCallbackDto.FromString(callbackDataStr);
                    var toDoList = await _toDoListService.Get(callbackDto.ToDoListId!.Value, ct);

                    if (toDoList == null)
                    {
                        await bot.SendMessage(message.Chat.Id, "Список не найден.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["ToDoList"] = toDoList;
                    context.Data.Remove("CallbackQueryData");
                    context.CurrentStep = "Delete";

                    var approveKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                        }
                    });

                    await bot.SendMessage(message.Chat.Id, $"Подтверждаете удаление списка {toDoList.Name} и всех его задач", replyMarkup: approveKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                }
                return ScenarioResult.Transition;

            case "Delete":
                if (context.Data.TryGetValue("CallbackQueryData", out var approveDataObj) && approveDataObj is string approveData)
                {
                    var toDoList = (ToDoList)context.Data["ToDoList"];
                    context.Data.Remove("CallbackQueryData");

                    if (approveData == "yes")
                    {
                        await _toDoService.DeleteByUserIdAndListAsync(toDoList.User.UserId, toDoList.Id, ct);
                        await _toDoListService.Delete(toDoList.Id, ct);
                        await bot.SendMessage(message.Chat.Id, $"Список \"{toDoList.Name}\" удален.", cancellationToken: ct);
                    }
                    else if (approveData == "no")
                    {
                        await bot.SendMessage(message.Chat.Id, "Удаление отменено.", cancellationToken: ct);
                    }

                    return ScenarioResult.Completed;
                }
                return ScenarioResult.Completed;

            default:
                return ScenarioResult.Completed;
        }
    }
}
