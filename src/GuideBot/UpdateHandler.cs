using GuideBot.Services;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace GuideBot;

public class UpdateHandler : IUpdateHandler
{
    private string welcomeText = "Добро пожаловать!";
    private string menuText = "Доступные команды: /start, /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /report, /find, /exit";
    private bool isExit = false;
    private bool isUserRegistered = false;
    private ToDoUser? currentUser = null;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;
    public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoReportService = toDoReportService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        await botClient.SendMessage(update.Message.Chat, welcomeText, token);

        try
        {
            while (!isExit)
            {
                await botClient.SendMessage(update.Message.Chat, menuText, token);

                var userCommand = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(userCommand))
                {
                    switch (userCommand!.Trim())
                    {
                        case "/start":
                            await StartAsync(botClient, update, token);
                            break;
                        case "/help":
                            await HelpAsync(botClient, update, token);
                            break;
                        case "/info":
                            await InfoAsync(botClient, update, token);
                            break;
                        case string addtaskInput when addtaskInput.Contains("/addtask") && addtaskInput.Length > 9:
                            if (IsUserRegistered())
                            {
                                await AddTaskAsync(botClient, update, addtaskInput.Substring(9), token);
                            }
                            break;
                        case "/showtasks":
                            if (IsUserRegistered())
                            {
                                await ShowActiveTasksAsync(botClient, update, token);
                            }
                            break;
                        case string removetaskInput when removetaskInput.Contains("/removetask") && removetaskInput.Length > 12:
                            if (IsUserRegistered())
                            {
                                await RemoveTaskAsync(removetaskInput.Substring(12), botClient, update, token);
                            }
                            break;
                        case string completetaskInput when completetaskInput.Contains("/completetask") && completetaskInput.Length > 14:
                            if (IsUserRegistered())
                            {
                                await CompletetaskAsync(completetaskInput.Substring(14), botClient, update, token);
                            }
                            break;
                        case "/showalltasks":
                            if (IsUserRegistered())
                            {
                                await ShowAllTasksAsync(botClient, update, token);
                            }
                            break;
                        case "/report":
                            if (IsUserRegistered())
                            {
                                await ShowUserStatsAsync(botClient, update, token);
                            }
                            break;
                        case string findtaskInput when findtaskInput.Contains("/find") && findtaskInput.Length > 6:
                            if (IsUserRegistered())
                            {
                                await FindTaskByPrefixAsync(findtaskInput.Substring(6), botClient, update, token);
                            }
                            break;
                        case "/exit":
                            Exit();
                            break;
                        default:
                            await botClient.SendMessage(update.Message.Chat, "Неверная команда! Попробуйте заново!", token);
                            break;
                    }
                }
            }
        }
        catch (ArgumentException ex)
        {
            await HandleErrorAsync(botClient, ex, token);
        }
        catch (TaskCountLimitException ex)
        {
            await HandleErrorAsync(botClient, ex, token);
        }
        catch (TaskLengthLimitException ex)
        {
            await HandleErrorAsync(botClient, ex, token);
        }
        catch (DuplicateTaskException ex)
        {
            await HandleErrorAsync(botClient, ex, token);
        }
    }

    async Task StartAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        currentUser = await _userService.GetUserAsync(update!.Message!.From!.Id!, token);
        if (currentUser is null) currentUser = await _userService.RegisterUserAsync(update.Message.From.Id, update!.Message!.From!.Username!, token);

        await botClient.SendMessage(update.Message.Chat, $"Привет, {currentUser.TelegramUserName}! Чем могу помочь?", token);
    }

    bool IsUserRegistered()
    {
        if (currentUser is not null && !string.IsNullOrWhiteSpace(currentUser.TelegramUserName))
            isUserRegistered = true;

        return isUserRegistered;
    }

    async Task HelpAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/addtask - добавляет задачу в список. Пример: /addtask Новая задача\n/showtasks - отображает список всех добавленных задач со статусом Активна\n/removetask - удаляет задачу по номеру в списке. Пример: /removetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/completetask - переводит статус задачи на Завершена. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/showalltasks - отображает список всех добавленных задач\n/report - вывод информации о задачах пользователя. Пример вывода: Статистика по задачам на 01.01.2025 00:00:00. Всего: 10; Завершенных: 7; Активных: 3;\n/find - возвращает все задачи пользователя, которые начинаются на введенный префикс. Пример команды: /find Имя\n/exit - для выхода из программы\nЕсли пользователь не зарегистрирован, то ему доступны только команды /help /info! Для регистрации необходимо ввести команду /start";
        await botClient.SendMessage(update.Message.Chat, helpCommandText, token);
    }

    async Task InfoAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
        await botClient.SendMessage(update.Message.Chat, infoCommandText, token);
    }

    async Task AddTaskAsync(ITelegramBotClient botClient, Update update, string taskName, CancellationToken token)
    {
        var task = await _toDoService.AddAsync(currentUser!, taskName, token);
        if (task is not null)
            await botClient.SendMessage(update.Message.Chat, $"Задача \"{taskName}\" добавлена.", token);
    }

    async Task ShowAllTasksAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var tasks = await _toDoService.GetAllByUserIdAsync(currentUser!.UserId, token);
        await PrintTasksAsync(botClient, update, tasks, "Вот ваш список задач:", token);
    }

    async Task ShowActiveTasksAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var tasks = await _toDoService.GetActiveByUserIdAsync(currentUser!.UserId, token);
        await PrintTasksAsync(botClient, update, tasks, "Вот ваш список активных задач:", token);
    }

    async Task RemoveTaskAsync(string taskId, ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        Guid.TryParse(taskId, out Guid id);
        if (id == Guid.Empty)
        {
            await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи", token);
            return;
        }

        await _toDoService.DeleteAsync(id, token);
        await botClient.SendMessage(update.Message.Chat, "Ваша задача удалена", token);
    }

    async Task CompletetaskAsync(string taskId, ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        Guid.TryParse(taskId, out Guid id);
        if (id == Guid.Empty)
        {
            await botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи", token);
            return;
        }

        await _toDoService.MarkAsCompletedAsync(id, token);

        await botClient.SendMessage(update.Message.Chat, "Ваша задача переведана в статус Completed", token);
    }

    async Task PrintTasksAsync(ITelegramBotClient botClient, Update update, IReadOnlyList<ToDoItem> tasks, string message, CancellationToken token)
    {
        if (tasks.Any())
        {
            await botClient.SendMessage(update.Message.Chat, message, token);
            for (var i = 1; i <= tasks.Count(); i++)
            {
                await botClient.SendMessage(update.Message.Chat, $"{i} - ({tasks[i - 1].State}){tasks[i - 1].Name} - {tasks[i - 1].CreatedAt} - {tasks[i - 1].Id}", token);
            }
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, "Список задач пуст", token);
        }
    }

    async Task ShowUserStatsAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var stats = await _toDoReportService.GetUserStatsAsync(currentUser!.UserId, token);
        await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {stats.generatedAt}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};", token);
    }

    async Task FindTaskByPrefixAsync(string namePrefix, ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(namePrefix)) return;

        var tasks = await _toDoService.FindAsync(currentUser!, namePrefix, token);

        await PrintTasksAsync(botClient, update, tasks, "Вот ваш список активных задач:", token);
    }

    void Exit()
    {
        isExit = true;
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"HandleError: {exception})");
        return Task.CompletedTask;
    }
}