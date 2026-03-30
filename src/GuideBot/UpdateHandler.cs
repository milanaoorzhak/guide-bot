using GuideBot.Helpers;
using GuideBot.Services;
using GuideBot.Scenarios;
using GuideBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot;

public class UpdateHandler : IUpdateHandler
{
    private bool isUserRegistered = false;
    private ToDoUser? currentUser = null;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoListService _toDoListService;
    private readonly IToDoReportService _toDoReportService;
    private readonly IScenarioContextRepository _contextRepository;
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private const int _pageSize = 5;
    public UpdateHandler(
        IUserService userService,
        IToDoService toDoService,
        IToDoListService toDoListService,
        IToDoReportService toDoReportService,
        IEnumerable<IScenario> scenarios,
        IScenarioContextRepository contextRepository,
        CancellationTokenSource cancellationTokenSource)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoListService = toDoListService;
        _toDoReportService = toDoReportService;
        _scenarios = scenarios;
        _contextRepository = contextRepository;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message?.Text?.Trim() == "/cancel")
            {
                if (IsUserRegistered())
                {
                    await _contextRepository.ResetContext(update.Message.From!.Id, cancellationToken);
                    await botClient.SendMessage(update.Message.Chat, "Сценарий отменен.", cancellationToken: cancellationToken);
                }
                return;
            }

            var scenarioContext = await _contextRepository.GetContext(update.Message.From!.Id, cancellationToken);
            if (scenarioContext != null)
            {
                await ProcessScenario(scenarioContext, botClient, update.Message, cancellationToken);
                return;
            }

            ReplyKeyboardMarkup keyboard = null;
            if (!IsUserRegistered())
            {
                keyboard = new(
                new[]
                {
                    new KeyboardButton("/start")
                })
                { ResizeKeyboard = true };
            }
            else
            {
                keyboard = new(
                new[]
                {
                    new KeyboardButton("/addtask"),
                    new KeyboardButton("/show"),
                    new KeyboardButton("/report")
                })
                { ResizeKeyboard = true };
            }

            if (update.Message?.Text == null && update.CallbackQuery == null) return;

            await (update switch
            {
                { Message: { } message } => OnMessage(botClient, update, message, keyboard, cancellationToken),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, cancellationToken),
                _ => OnUnknown(update)
            });
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
        }
        catch (TaskCountLimitException ex)
        {
            await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
        }
        catch (TaskLengthLimitException ex)
        {
            await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
        }
        catch (DuplicateTaskException ex)
        {
            await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
        }
    }

    private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, ReplyKeyboardMarkup keyboard, CancellationToken ct)
    {
        if (message.Text == null) return;

        var userCommand = message.Text;
        if (!string.IsNullOrWhiteSpace(userCommand))
        {
            switch (userCommand!.Trim())
            {
                case "/start":
                    await StartAsync(botClient, update, keyboard, ct);
                    break;
                case "/help":
                    await HelpAsync(botClient, update, keyboard, ct);
                    break;
                case "/info":
                    await InfoAsync(botClient, update, keyboard, ct);
                    break;
                case "/addtask":
                    var addTaskUser = await _userService.GetUserAsync(message.From!.Id, ct);
                    if (addTaskUser != null)
                    {
                        currentUser = addTaskUser;
                        isUserRegistered = true;
                        var addTaskContext = new ScenarioContext(ScenarioType.AddTask);
                        await ProcessScenario(addTaskContext, botClient, message, ct);
                    }
                    else
                    {
                        await botClient.SendMessage(message.Chat, "Пожалуйста, сначала зарегистрируйтесь командой /start", replyMarkup: keyboard, cancellationToken: ct);
                    }
                    break;
                case "/show":
                    var showUser = await _userService.GetUserAsync(message.From!.Id, ct);
                    if (showUser != null)
                    {
                        currentUser = showUser;
                        isUserRegistered = true;
                        await ShowListsAsync(botClient, update, ct);
                    }
                    else
                    {
                        await botClient.SendMessage(message.Chat, "Пожалуйста, сначала зарегистрируйтесь командой /start", replyMarkup: keyboard, cancellationToken: ct);
                    }
                    break;
                case string findtaskInput when findtaskInput.Contains("/find") && findtaskInput.Length > 6:
                    var findUser = await _userService.GetUserAsync(message.From!.Id, ct);
                    if (findUser != null)
                    {
                        currentUser = findUser;
                        isUserRegistered = true;
                        await FindTaskByPrefixAsync(findtaskInput.Substring(6), botClient, update, keyboard, ct);
                    }
                    break;
                case "/report":
                    var reportUser = await _userService.GetUserAsync(message.From!.Id, ct);
                    if (reportUser != null)
                    {
                        currentUser = reportUser;
                        isUserRegistered = true;
                        await ShowUserStatsAsync(botClient, update, keyboard, ct);
                    }
                    else
                    {
                        await botClient.SendMessage(message.Chat, "Пожалуйста, сначала зарегистрируйтесь командой /start", replyMarkup: keyboard, cancellationToken: ct);
                    }
                    break;
                case "/cancel":
                    if (IsUserRegistered())
                    {
                        await _contextRepository.ResetContext(update.Message.From!.Id, ct);
                    }
                    break;
                case "/exit":
                    await botClient.SendMessage(update.Message.Chat, $"Завершение работы...", cancellationToken: ct);
                    _cancellationTokenSource.Cancel();
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat, "Неверная команда! Попробуйте заново!", replyMarkup: keyboard, cancellationToken: ct);
                    break;
            }
        }
    }

    private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery query, CancellationToken ct)
    {
        var callbackUser = await _userService.GetUserAsync(query.From!.Id, ct);
        if (callbackUser == null)
        {
            return;
        }

        currentUser = callbackUser;
        isUserRegistered = true;

        var callbackDto = CallbackDto.FromString(query.Data!);

        switch (callbackDto.Action)
        {
            case "show":
                var toDoListCallback = PagedListCallbackDto.FromString(query.Data!);
                await ShowTasksByListAsync(botClient, update, toDoListCallback.ToDoListId, toDoListCallback.Page, ct);
                break;
            case "showtask":
                var toDoItemCallback = ToDoItemCallbackDto.FromString(query.Data!);
                await ShowTaskAsync(botClient, update, toDoItemCallback.ToDoItemId, ct);
                break;
            case "completetask":
                var completeTaskCallback = ToDoItemCallbackDto.FromString(query.Data!);
                await CompleteTaskAsync(botClient, update, completeTaskCallback.ToDoItemId, ct);
                break;
            case "deletetask":
                var deleteTaskCallback = ToDoItemCallbackDto.FromString(query.Data!);
                var deleteTaskContext = await _contextRepository.GetContext(query.From.Id, ct);
                if (deleteTaskContext != null && deleteTaskContext.CurrentStep == "Approve")
                {
                    deleteTaskContext.Data["CallbackQueryData"] = query.Data;
                    await _contextRepository.SetContext(query.From.Id, deleteTaskContext, ct);
                    var scenario = GetScenario(deleteTaskContext.CurrentScenario);
                    var result = await scenario.HandleMessageAsync(botClient, deleteTaskContext, query.Message!, ct);
                    if (result == ScenarioResult.Completed)
                    {
                        await _contextRepository.ResetContext(query.From.Id, ct);
                    }
                }
                else
                {
                    var newDeleteTaskContext = new ScenarioContext(ScenarioType.DeleteTask);
                    newDeleteTaskContext.Data["CallbackQueryData"] = query.Data;
                    await ProcessScenario(newDeleteTaskContext, botClient, query.Message!, ct);
                }
                break;
            case "show_completed":
                var showCompletedCallback = PagedListCallbackDto.FromString(query.Data!);
                await ShowCompletedTasksAsync(botClient, update, showCompletedCallback.ToDoListId, showCompletedCallback.Page, ct);
                break;
            case "selectlist":
                var selectListContext = await _contextRepository.GetContext(query.From!.Id, ct);
                if (selectListContext != null)
                {
                    selectListContext.Data["CallbackQueryData"] = query.Data;
                    await _contextRepository.SetContext(query.From.Id, selectListContext, ct);
                    var scenario = GetScenario(selectListContext.CurrentScenario);
                    var result = await scenario.HandleMessageAsync(botClient, selectListContext, query.Message!, ct);
                    if (result == ScenarioResult.Completed)
                    {
                        await _contextRepository.ResetContext(query.From.Id, ct);
                    }
                    else
                    {
                        await _contextRepository.SetContext(query.From.Id, selectListContext, ct);
                    }
                }
                break;
            case "deletelist":
                var callbackData = query.Data;
                var deleteListContext = await _contextRepository.GetContext(query.From.Id, ct);
                if (deleteListContext != null && deleteListContext.CurrentStep == "Approve")
                {
                    deleteListContext.Data["CallbackQueryData"] = callbackData;
                    await _contextRepository.SetContext(query.From.Id, deleteListContext, ct);
                    var scenario = GetScenario(deleteListContext.CurrentScenario);
                    var result = await scenario.HandleMessageAsync(botClient, deleteListContext, query.Message!, ct);
                    if (result == ScenarioResult.Completed)
                    {
                        await _contextRepository.ResetContext(query.From.Id, ct);
                    }
                }
                else
                {
                    var newDeleteContext = new ScenarioContext(ScenarioType.DeleteList);
                    newDeleteContext.Data["CallbackQueryData"] = callbackData;
                    await ProcessScenario(newDeleteContext, botClient, query.Message!, ct);
                }
                break;
            case "addlist":
                var addListContext = new ScenarioContext(ScenarioType.AddList);
                await ProcessScenario(addListContext, botClient, query.Message!, ct);
                break;
            case "yes":
            case "no":
                var approveContext = await _contextRepository.GetContext(query.From.Id, ct);
                if (approveContext != null)
                {
                    approveContext.Data["CallbackQueryData"] = query.Data;
                    await _contextRepository.SetContext(query.From.Id, approveContext, ct);
                    var scenario = GetScenario(approveContext.CurrentScenario);
                    var result = await scenario.HandleMessageAsync(botClient, approveContext, query.Message!, ct);
                    if (result == ScenarioResult.Completed)
                    {
                        await _contextRepository.ResetContext(query.From.Id, ct);
                    }
                }
                break;
        }

        await botClient.AnswerCallbackQuery(query.Id, cancellationToken: ct);
    }

    private Task OnUnknown(Update update)
    {
        return Task.CompletedTask;
    }

    async Task StartAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        currentUser = await _userService.GetUserAsync(update!.Message!.From!.Id!, token);
        if (currentUser is null) currentUser = await _userService.RegisterUserAsync(update.Message.From.Id, update!.Message!.From!.Username!, token);

        await botClient.SendMessage(update.Message.Chat, $"Привет, {currentUser.TelegramUserName}! Чем могу помочь?", replyMarkup: keyboard, cancellationToken: token);
    }

    bool IsUserRegistered()
    {
        if (currentUser is not null && !string.IsNullOrWhiteSpace(currentUser.TelegramUserName))
            isUserRegistered = true;

        return isUserRegistered;
    }

    async Task HelpAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        string helpCommandText = "Для работы с программой необходимо ввести одну из команд: \n/start - для старта работы программы\n/help - отображает краткую справочную информацию о том, как пользоваться программой\n/info - предоставляет информацию о версии программы и дате её создания\n/addtask - добавляет задачу в список. Пример: /addtask Новая задача\n/show - отображает списки задач для выбора\n/report - вывод информации о задачах пользователя\n/find - возвращает все задачи пользователя, которые начинаются на введенный префикс. Пример команды: /find Имя\n/cancel - отмена текущего сценария\n/exit - для выхода из программы\n\nДля управления задачами используйте кнопки в интерфейсе.\nЕсли пользователь не зарегистрирован, то ему доступны только команды /help /info! Для регистрации необходимо ввести команду /start";
        await botClient.SendMessage(update.Message.Chat, helpCommandText, replyMarkup: keyboard, cancellationToken: token);
    }

    async Task InfoAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
        await botClient.SendMessage(update.Message.Chat, infoCommandText, replyMarkup: keyboard, cancellationToken: token);
    }

    async Task ShowListsAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var lists = await _toDoListService.GetUserLists(currentUser!.UserId, token);

        var buttons = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("📌Без списка", new PagedListCallbackDto("show", null, 0).ToString())
        };

        foreach (var list in lists)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(list.Name, new PagedListCallbackDto("show", list.Id, 0).ToString()));
        }

        buttons.Add(InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist"));
        buttons.Add(InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist"));

        var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));
        await botClient.SendMessage(update.Message.Chat, "Выберите список", replyMarkup: keyboard, cancellationToken: token);
    }

    async Task ShowTasksByListAsync(ITelegramBotClient botClient, Update update, Guid? listId, int page, CancellationToken token)
    {
        var allTasks = await _toDoService.GetByUserIdAndList(currentUser!.UserId, listId, token);
        var activeTasks = allTasks.Where(t => t.State == ToDoItemState.Active).ToList();

        string listName = listId == null ? "Без списка" : (await _toDoListService.Get(listId.Value, token))?.Name ?? "Неизвестный список";

        var callbackData = new List<KeyValuePair<string, string>>();
        foreach (var task in activeTasks)
        {
            callbackData.Add(new KeyValuePair<string, string>(
                $"({task.State}){task.Name}",
                new ToDoItemCallbackDto("showtask", task.Id).ToString()
            ));
        }

        var pagedListDto = new PagedListCallbackDto("show", listId, page);
        var keyboard = BuildPagedButtons(callbackData, pagedListDto);

        var messageText = activeTasks.Any() ? $"Задачи списка \"{listName}\":" : "Список задач пуст";

        if (update.CallbackQuery != null)
        {
            await botClient.EditMessageText(update.CallbackQuery.Message!.Chat.Id, update.CallbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, cancellationToken: token);
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, messageText, replyMarkup: keyboard, cancellationToken: token);
        }
    }

    async Task PrintTasksAsync(ITelegramBotClient botClient, Update update, IReadOnlyList<ToDoItem> tasks, string message, CancellationToken token)
    {
        if (tasks.Any())
        {
            await botClient.SendMessage(update.Message.Chat, message, cancellationToken: token);
            for (var i = 1; i <= tasks.Count(); i++)
            {
                await botClient.SendMessage(update.Message.Chat, $"{i} - ({tasks[i - 1].State}){tasks[i - 1].Name} - {tasks[i - 1].CreatedAt} - '{tasks[i - 1].Id}'", cancellationToken: token);
            }
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, "Список задач пуст", cancellationToken: token);
        }
    }

    async Task ShowUserStatsAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        var stats = await _toDoReportService.GetUserStatsAsync(currentUser!.UserId, token);
        await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {stats.generatedAt}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};", replyMarkup: keyboard, cancellationToken: token);
    }

    async Task FindTaskByPrefixAsync(string namePrefix, ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(namePrefix)) return;

        var tasks = await _toDoService.FindAsync(currentUser!, namePrefix, token);

        await PrintTasksAsync(botClient, update, tasks, "Вот ваш список задач:", token);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception})");
        return Task.CompletedTask;
    }

    public IScenario GetScenario(ScenarioType scenarioType)
    {
        var scenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenarioType));
        if (scenario == null)
        {
            throw new Exception($"Сценарий не найден");
        }
        return scenario;
    }

    public async Task ProcessScenario(ScenarioContext context, ITelegramBotClient botClient, Message msg, CancellationToken ct)
    {
        var scenario = GetScenario(context.CurrentScenario);
        var result = await scenario.HandleMessageAsync(botClient, context, msg, ct);

        if (result == ScenarioResult.Completed)
        {
            await _contextRepository.ResetContext(msg.From!.Id, ct);
        }
        else
        {
            await _contextRepository.SetContext(msg.From!.Id, context, ct);
        }
    }

    private InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
    {
        var totalPages = (int)Math.Ceiling((double)callbackData.Count / _pageSize);
        var pagedItems = callbackData.GetBatchByNumber(_pageSize, listDto.Page).ToList();

        var buttons = new List<InlineKeyboardButton>();
        foreach (var item in pagedItems)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData(item.Key, item.Value));
        }

        var navigationButtons = new List<InlineKeyboardButton>();
        if (listDto.Page > 0)
        {
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page - 1).ToString()));
        }
        if (listDto.Page < totalPages - 1)
        {
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, listDto.Page + 1).ToString()));
        }

        var keyboard = new List<IEnumerable<InlineKeyboardButton>>();
        keyboard.AddRange(buttons.Select(b => new[] { b }));

        if (navigationButtons.Count > 0)
        {
            keyboard.Add(navigationButtons);
        }

        if (listDto.Action == "show")
        {
            keyboard.Add(new[] { InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные", new PagedListCallbackDto("show_completed", listDto.ToDoListId, 0).ToString()) });
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private async Task ShowTaskAsync(ITelegramBotClient botClient, Update update, Guid toDoItemId, CancellationToken token)
    {
        var task = await _toDoService.Get(toDoItemId, token);
        if (task == null)
        {
            await botClient.SendMessage(update.CallbackQuery!.Message!.Chat.Id, "Задача не найдена", cancellationToken: token);
            return;
        }

        var buttons = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData("✅Выполнить", new ToDoItemCallbackDto("completetask", task.Id).ToString()),
            InlineKeyboardButton.WithCallbackData("❌Удалить", new ToDoItemCallbackDto("deletetask", task.Id).ToString())
        };

        var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }));
        var message = $"Задача: {task.Name}\nСтатус: {task.State}\nСоздана: {task.CreatedAt}\nДедлайн: {task.Deadline}";

        if (update.CallbackQuery != null)
        {
            await botClient.EditMessageText(update.CallbackQuery.Message!.Chat.Id, update.CallbackQuery.Message.MessageId, message, replyMarkup: keyboard, cancellationToken: token);
        }
        else
        {
            await botClient.SendMessage(update.Message!.Chat, message, replyMarkup: keyboard, cancellationToken: token);
        }
    }

    private async Task CompleteTaskAsync(ITelegramBotClient botClient, Update update, Guid toDoItemId, CancellationToken token)
    {
        await _toDoService.MarkAsCompletedAsync(toDoItemId, token);

        var listId = (await _toDoService.Get(toDoItemId, token))?.List?.Id;
        var callbackData = new List<KeyValuePair<string, string>>();
        var tasks = await _toDoService.GetByUserIdAndList(currentUser!.UserId, listId, token);
        var activeTasks = tasks.Where(t => t.State == ToDoItemState.Active).ToList();

        foreach (var task in activeTasks)
        {
            callbackData.Add(new KeyValuePair<string, string>(
                $"({task.State}){task.Name}",
                new ToDoItemCallbackDto("showtask", task.Id).ToString()
            ));
        }

        var pagedListDto = new PagedListCallbackDto("show", listId, 0);
        var keyboard = BuildPagedButtons(callbackData, pagedListDto);

        string listName = listId == null ? "Без списка" : (await _toDoListService.Get(listId.Value, token))?.Name ?? "Неизвестный список";
        var messageText = activeTasks.Any() ? $"Задачи списка \"{listName}\":" : "Список задач пуст";

        await botClient.EditMessageText(update.CallbackQuery!.Message!.Chat.Id, update.CallbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, cancellationToken: token);
    }

    private async Task ShowCompletedTasksAsync(ITelegramBotClient botClient, Update update, Guid? listId, int page, CancellationToken token)
    {
        var allTasks = await _toDoService.GetByUserIdAndList(currentUser!.UserId, listId, token);
        var completedTasks = allTasks.Where(t => t.State == ToDoItemState.Completed).ToList();

        string listName = listId == null ? "Без списка" : (await _toDoListService.Get(listId.Value, token))?.Name ?? "Неизвестный список";

        var callbackData = new List<KeyValuePair<string, string>>();
        foreach (var task in completedTasks)
        {
            callbackData.Add(new KeyValuePair<string, string>(
                $"({task.State}){task.Name}",
                new ToDoItemCallbackDto("showtask", task.Id).ToString()
            ));
        }

        var pagedListDto = new PagedListCallbackDto("show_completed", listId, page);
        var keyboard = BuildPagedButtons(callbackData, pagedListDto);

        var messageText = completedTasks.Any() ? $"Завершенные задачи списка \"{listName}\":" : "Задач нет";

        if (update.CallbackQuery != null)
        {
            await botClient.EditMessageText(update.CallbackQuery.Message!.Chat.Id, update.CallbackQuery.Message.MessageId, messageText, replyMarkup: keyboard, cancellationToken: token);
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, messageText, replyMarkup: keyboard, cancellationToken: token);
        }
    }
}