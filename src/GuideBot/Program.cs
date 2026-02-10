using GuideBot;
using GuideBot.DataAccess;
using GuideBot.Entities;
using GuideBot.Infrastructure.DataAccess;
using GuideBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

var token = Environment.GetEnvironmentVariable("TUVAN_GUIDE_BOT_TOKEN") ?? throw new Exception("Bot token not found.");

var commands = new List<ToDoCommand>()
{
    new ToDoCommand()
    {
        Name = "/start",
        Description = "для старта работы программы"
    },
    new ToDoCommand()
    {
        Name = "/attractions",
        Description = "Достопримечательности"
    },
    new ToDoCommand()
    {
        Name = "/map",
        Description = "Карта города"
    },
    new ToDoCommand()
    {
        Name = "/search",
        Description = "Поиск"
    },
    new ToDoCommand()
    {
        Name = "/about",
        Description = "О городе"
    },
    new ToDoCommand()
    {
        Name = "/exit",
        Description = "для выхода из программы"
    }
};

var settings = new ToDoSettings();
using var cts = new CancellationTokenSource();

IUserRepository userRepository = new InMemoryUserRepository();
IToDoRepository toDoRepository = new InMemoryToDoRepository();

IUserService userService = new UserService(userRepository);
IToDoService toDoService = new ToDoService(settings, toDoRepository);
IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);

var handler = new UpdateHandler(userService, toDoService, toDoReportService, cts);
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