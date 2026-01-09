using System.ComponentModel;
using GuideBot;
using Otus.ToDoList.ConsoleBot;

var settings = new ToDoSettings();

IUserService userService = new UserService();
IToDoService toDoService = new ToDoService(settings);

var handler = new UpdateHandler(settings, userService, toDoService);
ConsoleBotClient botClient = new ConsoleBotClient();

try
{
    botClient.StartReceiving(handler);
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}