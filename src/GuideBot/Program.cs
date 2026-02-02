using System.ComponentModel;
using GuideBot;
using GuideBot.DataAccess;
using GuideBot.Infrastructure.DataAccess;
using GuideBot.Services;
using Otus.ToDoList.ConsoleBot;

int min = 0;
int max = 100;

var settings = new ToDoSettings();
using var tokenSource = new CancellationTokenSource();

IUserRepository userRepository = new InMemoryUserRepository();
IToDoRepository toDoRepository = new InMemoryToDoRepository();

IUserService userService = new UserService(userRepository);
IToDoService toDoService = new ToDoService(settings, toDoRepository);
IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);

var handler = new UpdateHandler(userService, toDoService, toDoReportService, tokenSource);
ConsoleBotClient botClient = new ConsoleBotClient();

try
{
    Console.Write("Введите максимально допустимое количество задач: ");
    settings.MaxTaskCount = ParseAndValidateInt(Console.ReadLine(), min, max);

    Console.Write("Введите максимально допустимую длину задачи: ");
    settings.MaxTaskLength = ParseAndValidateInt(Console.ReadLine(), min, max);

    botClient.StartReceiving(handler, tokenSource.Token);
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}

int ParseAndValidateInt(string? str, int min, int max)
{
    if (!(int.TryParse(str, out int number) && number >= min && number <= max)) throw new ArgumentException($"Это должно быть число от {min} до {max}");

    return number;
}