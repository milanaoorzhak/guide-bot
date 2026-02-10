using GuideBot.Services;
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
    private readonly IToDoReportService _toDoReportService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    public UpdateHandler(
        IUserService userService,
        IToDoService toDoService,
        IToDoReportService toDoReportService,
        CancellationTokenSource cancellationTokenSource)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoReportService = toDoReportService;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
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

            var userCommand = update.Message.Text;
            if (!string.IsNullOrWhiteSpace(userCommand))
            {
                switch (userCommand!.Trim())
                {
                    case "/start":
                        await StartAsync(botClient, update, keyboard, cancellationToken);
                        break;
                    case "/attractions":
                        await AttractionsAsync(botClient, update, keyboard, cancellationToken);
                        break;
                    case "/map":
                        await MapAsync(botClient, update, keyboard, cancellationToken);
                        break;
                    case string addtaskInput when addtaskInput.Contains("/search") && addtaskInput.Length > 8:
                        if (IsUserRegistered())
                        {
                            await SearchAsync(botClient, update, addtaskInput.Substring(8), keyboard, cancellationToken);
                        }
                        break;
                    case "/about":
                        await AboutAsync(botClient, update, keyboard, cancellationToken);
                        break;
                    case "/exit":
                        await botClient.SendMessage(update.Message.Chat, $"Завершение работы...", cancellationToken: cancellationToken);
                        _cancellationTokenSource.Cancel();
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, "Неверная команда! Попробуйте заново!", replyMarkup: keyboard, cancellationToken: cancellationToken);
                        break;
                }
            }
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

    async Task StartAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        currentUser = await _userService.GetUserAsync(update!.Message!.From!.Id!, token);
        if (currentUser is null) currentUser = await _userService.RegisterUserAsync(update.Message.From.Id, update!.Message!.From!.Username!, token);

        await botClient.SendMessage(update.Message.Chat, $"Добро пожаловать в Кызыл! Выбери действие: [Меню]: [Достопримечательности] [Карта города] [Поиск] [О городе]", replyMarkup: keyboard, cancellationToken: token);
    }

    bool IsUserRegistered()
    {
        if (currentUser is not null && !string.IsNullOrWhiteSpace(currentUser.TelegramUserName))
            isUserRegistered = true;

        return isUserRegistered;
    }

    async Task AttractionsAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/addtask - добавляет задачу в список. Пример: /addtask Новая задача\n/showtasks - отображает список всех добавленных задач со статусом Активна\n/removetask - удаляет задачу по номеру в списке. Пример: /removetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/completetask - переводит статус задачи на Завершена. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/showalltasks - отображает список всех добавленных задач\n/report - вывод информации о задачах пользователя. Пример вывода: Статистика по задачам на 01.01.2025 00:00:00. Всего: 10; Завершенных: 7; Активных: 3;\n/find - возвращает все задачи пользователя, которые начинаются на введенный префикс. Пример команды: /find Имя\n/exit - для выхода из программы\nЕсли пользователь не зарегистрирован, то ему доступны только команды /help /info! Для регистрации необходимо ввести команду /start";
        await botClient.SendMessage(update.Message.Chat, helpCommandText, replyMarkup: keyboard, cancellationToken: token);
    }

    async Task MapAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
        await botClient.SendMessage(update.Message.Chat, infoCommandText, replyMarkup: keyboard, cancellationToken: token);
    }

    async Task SearchAsync(ITelegramBotClient botClient, Update update, string taskName, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        var task = await _toDoService.AddAsync(currentUser!, taskName, token);
        if (task is not null)
            await botClient.SendMessage(update.Message.Chat, $"Задача \"{taskName}\" добавлена.", replyMarkup: keyboard, cancellationToken: token);
    }

    async Task AboutAsync(ITelegramBotClient botClient, Update update, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        var tasks = await _toDoService.GetActiveByUserIdAsync(currentUser!.UserId, token);
        await PrintTasksAsync(botClient, update, tasks, "Вот ваш список активных задач:", keyboard, token);
    }

    async Task PrintTasksAsync(ITelegramBotClient botClient, Update update, IReadOnlyList<ToDoItem> tasks, string message, ReplyKeyboardMarkup keyboard, CancellationToken token)
    {
        if (tasks.Any())
        {
            await botClient.SendMessage(update.Message.Chat, message, replyMarkup: keyboard, cancellationToken: token);
            for (var i = 1; i <= tasks.Count(); i++)
            {
                await botClient.SendMessage(update.Message.Chat, $"{i} - ({tasks[i - 1].State}){tasks[i - 1].Name} - {tasks[i - 1].CreatedAt} - '{tasks[i - 1].Id}'", cancellationToken: token);
            }
        }
        else
        {
            await botClient.SendMessage(update.Message.Chat, "Список задач пуст", replyMarkup: keyboard, cancellationToken: token);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception})");
        return Task.CompletedTask;
    }
}