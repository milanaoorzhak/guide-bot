using GuideBot;
using GuideBot.DataAccess;
using GuideBot.Entities;
using GuideBot.Infrastructure.DataAccess;
using GuideBot.Services;
using GuideBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new Exception("Bot token not found.");

var commands = new List<ToDoCommand>()
{
    new ToDoCommand()
    {
        Name = "/start",
        Description = "для старта работы программы"
    },
    new ToDoCommand()
    {
        Name = "/help",
        Description = "отображает краткую справочную информацию о том, как пользоваться программой"
    },
    new ToDoCommand()
    {
        Name = "/info",
        Description = "предоставляет информацию о версии программы и дате её создания"
    },
    new ToDoCommand()
    {
        Name = "/addtask",
        Description = "добавляет задачу в список. Пример: /addtask Новая задача"
    },
    new ToDoCommand()
    {
        Name = "/show",
        Description = "отображает списки задач для выбора"
    },
    new ToDoCommand()
    {
        Name = "/report",
        Description = "вывод информации о задачах пользователя. Пример вывода: Статистика по задачам на 01.01.2025 00:00:00. Всего: 10; Завершенных: 7; Активных: 3;"
    },
    new ToDoCommand()
    {
        Name = "/find",
        Description = "возвращает все задачи пользователя, которые начинаются на введенный префикс. Пример команды: /find Имя"
    },
    new ToDoCommand()
    {
        Name = "/exit",
        Description = "для выхода из программы"
    }
};

var toDoBaseFolder = Path.Combine(Environment.CurrentDirectory, "toDos");
var userBaseFolder = Path.Combine(Environment.CurrentDirectory, "users");
var toDoListBaseFolder = Path.Combine(Environment.CurrentDirectory, "toDoLists");

var settings = new ToDoSettings();
using var cts = new CancellationTokenSource();

IUserRepository userRepository = new FileUserRepository(userBaseFolder);
IToDoRepository toDoRepository = new FileToDoRepository(toDoBaseFolder);
IToDoListRepository toDoListRepository = new FileToDoListRepository(toDoListBaseFolder);
IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();

IUserService userService = new UserService(userRepository);
IToDoService toDoService = new ToDoService(settings, toDoRepository);
IToDoListService toDoListService = new ToDoListService(toDoListRepository);
IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);

List<IScenario> scenarios = new()
{
    new AddTaskScenario(userService, toDoService, toDoListService),
    new AddListScenario(userService, toDoListService),
    new DeleteListScenario(userService, toDoListService, toDoService),
    new DeleteTaskScenario(toDoService)
};

var handler = new UpdateHandler(userService, toDoService, toDoListService, toDoReportService, scenarios, contextRepository, cts);
var bot = new TelegramBotClient(token: token, cancellationToken: cts.Token);

try
{
    await bot.SetMyCommands(
            [.. commands.Select(c =>
                new BotCommand
                {
                    Command = c.Name.TrimStart('/'),
                    Description = c.Description
                })
            ]
        );

    bot.StartReceiving(handler, cancellationToken: cts.Token);
    Console.WriteLine("Нажмите клавишу A для выхода");

    var keyInfo = Console.ReadKey(intercept: true);

    if (keyInfo.Key == ConsoleKey.A)
    {
        Console.WriteLine("Выход из программы...");
        cts.Cancel();
        await Task.Delay(500);

        return;
    }
    else
    {
        var me = await bot.GetMe(cts.Token);
        Console.WriteLine($"{me.FirstName} запущен!");
        await Task.Delay(-1);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}