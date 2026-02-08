using GuideBot;
using GuideBot.DataAccess;
using GuideBot.Entities;
using GuideBot.Infrastructure.DataAccess;
using GuideBot.Services;
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
        Name = "/showtasks",
        Description = "отображает список всех добавленных задач со статусом Активна"
    },
    new ToDoCommand()
    {
        Name = "/removetask",
        Description = "удаляет задачу по номеру в списке. Пример: /removetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e"
    },
    new ToDoCommand()
    {
        Name = "/completetask ",
        Description = "переводит статус задачи на Завершена. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e"
    },
    new ToDoCommand()
    {
        Name = "/showalltasks",
        Description = "отображает список всех добавленных задач"
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
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}