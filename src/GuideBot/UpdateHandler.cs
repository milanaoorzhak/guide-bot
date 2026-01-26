using System.Runtime.CompilerServices;
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

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        botClient.SendMessage(update.Message.Chat, welcomeText);

        try
        {
            while (!isExit)
            {
                botClient.SendMessage(update.Message.Chat, menuText);

                var userCommand = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(userCommand))
                {
                    switch (userCommand!.Trim())
                    {
                        case "/start":
                            Start(botClient, update);
                            break;
                        case "/help":
                            Help(botClient, update);
                            break;
                        case "/info":
                            Info(botClient, update);
                            break;
                        case string addtaskInput when addtaskInput.Contains("/addtask") && addtaskInput.Length > 9:
                            if (IsUserRegistered())
                            {
                                AddTask(botClient, update, addtaskInput.Substring(9));
                            }
                            break;
                        case "/showtasks":
                            if (IsUserRegistered())
                            {
                                ShowActiveTasks(botClient, update);
                            }
                            break;
                        case string removetaskInput when removetaskInput.Contains("/removetask") && removetaskInput.Length > 12:
                            if (IsUserRegistered())
                            {
                                RemoveTask(removetaskInput.Substring(12), botClient, update);
                            }
                            break;
                        case string completetaskInput when completetaskInput.Contains("/completetask") && completetaskInput.Length > 14:
                            if (IsUserRegistered())
                            {
                                Completetask(completetaskInput.Substring(14), botClient, update);
                            }
                            break;
                        case "/showalltasks":
                            if (IsUserRegistered())
                            {
                                ShowAllTasks(botClient, update);
                            }
                            break;
                        case "/report":
                            if (IsUserRegistered())
                            {
                                ShowUserStats(botClient, update);
                            }
                            break;
                        case string findtaskInput when findtaskInput.Contains("/find") && findtaskInput.Length > 6:
                            if (IsUserRegistered())
                            {
                                FindTaskByPrefix(findtaskInput.Substring(6), botClient, update);
                            }
                            break;
                        case "/exit":
                            Exit();
                            break;
                        default:
                            botClient.SendMessage(update.Message.Chat, "Неверная команда! Попробуйте заново!");
                            break;
                    }
                }
            }
        }
        catch (ArgumentException ex)
        {
            botClient.SendMessage(update.Message.Chat, $"{ex.Message}");
        }
        catch (TaskCountLimitException ex)
        {
            botClient.SendMessage(update.Message.Chat, $"{ex.Message}");
        }
        catch (TaskLengthLimitException ex)
        {
            botClient.SendMessage(update.Message.Chat, $"{ex.Message}");
        }
        catch (DuplicateTaskException ex)
        {
            botClient.SendMessage(update.Message.Chat, $"{ex.Message}");
        }
    }

    void Start(ITelegramBotClient botClient, Update update)
    {
        currentUser = _userService.GetUser(update!.Message!.From!.Id!);
        if (currentUser is null) currentUser = _userService.RegisterUser(update.Message.From.Id, update!.Message!.From!.Username!);

        botClient.SendMessage(update.Message.Chat, $"Привет, {currentUser.TelegramUserName}! Чем могу помочь?");
    }

    bool IsUserRegistered()
    {
        if (currentUser is not null && !string.IsNullOrWhiteSpace(currentUser.TelegramUserName))
            isUserRegistered = true;

        return isUserRegistered;
    }

    void Help(ITelegramBotClient botClient, Update update)
    {
        string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/addtask - добавляет задачу в список. Пример: /addtask Новая задача\n/showtasks - отображает список всех добавленных задач со статусом Активна\n/removetask - удаляет задачу по номеру в списке. Пример: /removetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/completetask - переводит статус задачи на Завершена. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/showalltasks - отображает список всех добавленных задач\n/report - вывод информации о задачах пользователя. Пример вывода: Статистика по задачам на 01.01.2025 00:00:00. Всего: 10; Завершенных: 7; Активных: 3;\n/find - возвращает все задачи пользователя, которые начинаются на введенный префикс. Пример команды: /find Имя\n/exit - для выхода из программы\nЕсли пользователь не зарегистрирован, то ему доступны только команды /help /info! Для регистрации необходимо ввести команду /start";
        botClient.SendMessage(update.Message.Chat, helpCommandText);
    }

    void Info(ITelegramBotClient botClient, Update update)
    {
        string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
        botClient.SendMessage(update.Message.Chat, infoCommandText);
    }

    void AddTask(ITelegramBotClient botClient, Update update, string taskName)
    {
        var task = _toDoService.Add(currentUser!, taskName);
        if (task is not null)
            botClient.SendMessage(update.Message.Chat, $"Задача \"{taskName}\" добавлена.");
    }

    void ShowAllTasks(ITelegramBotClient botClient, Update update)
    {
        var tasks = _toDoService.GetAllByUserId(currentUser!.UserId);
        PrintTasks(botClient, update, tasks, "Вот ваш список задач:");
    }

    void ShowActiveTasks(ITelegramBotClient botClient, Update update)
    {
        var tasks = _toDoService.GetActiveByUserId(currentUser!.UserId);
        PrintTasks(botClient, update, tasks, "Вот ваш список активных задач:");
    }

    void RemoveTask(string taskId, ITelegramBotClient botClient, Update update)
    {
        Guid.TryParse(taskId, out Guid id);
        if (id == Guid.Empty)
        {
            botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
            return;
        }

        _toDoService.Delete(id);
        botClient.SendMessage(update.Message.Chat, "Ваша задача удалена");
    }

    void Completetask(string taskId, ITelegramBotClient botClient, Update update)
    {
        Guid.TryParse(taskId, out Guid id);
        if (id == Guid.Empty)
        {
            botClient.SendMessage(update.Message.Chat, "Некорректный идентификатор задачи");
            return;
        }

        _toDoService.MarkAsCompleted(id);

        botClient.SendMessage(update.Message.Chat, "Ваша задача переведана в статус Completed");
    }

    void PrintTasks(ITelegramBotClient botClient, Update update, IReadOnlyList<ToDoItem> tasks, string message)
    {
        if (tasks.Any())
        {
            botClient.SendMessage(update.Message.Chat, message);
            for (var i = 1; i <= tasks.Count(); i++)
            {
                botClient.SendMessage(update.Message.Chat, $"{i} - ({tasks[i - 1].State}){tasks[i - 1].Name} - {tasks[i - 1].CreatedAt} - {tasks[i - 1].Id}");
            }
        }
        else
        {
            botClient.SendMessage(update.Message.Chat, "Список задач пуст");
        }
    }

    void ShowUserStats(ITelegramBotClient botClient, Update update)
    {
        var stats = _toDoReportService.GetUserStats(currentUser!.UserId);
        botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {stats.generatedAt}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};");
    }

    void FindTaskByPrefix(string namePrefix, ITelegramBotClient botClient, Update update)
    {
        if (string.IsNullOrWhiteSpace(namePrefix)) return;

        var tasks = _toDoService.Find(currentUser!, namePrefix);

        PrintTasks(botClient, update, tasks, "Вот ваш список активных задач:");
    }

    void Exit()
    {
        isExit = true;
    }
}