using GuideBot.Entities;
using GuideBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot.Scenarios;

public class DeleteTaskScenario : IScenario
{
    private readonly IToDoService _toDoService;

    public DeleteTaskScenario(IToDoService toDoService)
    {
        _toDoService = toDoService;
    }

    public bool CanHandle(ScenarioType scenario)
    {
        return scenario == ScenarioType.DeleteTask;
    }

    public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
    {
        switch (context.CurrentStep)
        {
            case null:
                if (context.Data.TryGetValue("CallbackQueryData", out var callbackDataObj) && callbackDataObj is string callbackDataStr)
                {
                    var callbackDto = ToDoItemCallbackDto.FromString(callbackDataStr);
                    var toDoItem = await _toDoService.Get(callbackDto.ToDoItemId, ct);

                    if (toDoItem == null)
                    {
                        await bot.SendMessage(message.Chat.Id, "Задача не найдена.", cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }

                    context.Data["ToDoItem"] = toDoItem;
                    context.CurrentStep = "Approve";

                    var approveKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✅Да", "yes"),
                            InlineKeyboardButton.WithCallbackData("❌Нет", "no")
                        }
                    });

                    await bot.SendMessage(message.Chat.Id, $"Подтверждаете удаление задачи \"{toDoItem.Name}\"?", replyMarkup: approveKeyboard, cancellationToken: ct);
                    return ScenarioResult.Transition;
                }
                return ScenarioResult.Completed;

            case "Approve":
                if (context.Data.TryGetValue("CallbackQueryData", out var approveDataObj) && approveDataObj is string approveData)
                {
                    var toDoItem = (ToDoItem)context.Data["ToDoItem"];
                    context.Data.Remove("CallbackQueryData");

                    if (approveData == "yes")
                    {
                        await _toDoService.DeleteAsync(toDoItem.Id, ct);
                        await bot.SendMessage(message.Chat.Id, $"Задача \"{toDoItem.Name}\" удалена.", cancellationToken: ct);
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
