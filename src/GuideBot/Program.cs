using System.ComponentModel;
using GuideBot;

int taskCount;
int taskLength;
int min = 0;
int max = 100;
string welcomeText = "Добро пожаловать!";
string menuText = "Доступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /exit";
string usernameNotEnteredMessage = "Имя пользователя не введено! Для работы с командой введите имя пользователя с помошью вызова команды /start";
bool isExit = false;

ToDoUser user = null;
List<ToDoItem> toDoItems = new();


Console.Write(welcomeText);
try
{
    Console.Write("Введите максимально допустимое количество задач: ");
    var taskCountInput = Console.ReadLine();
    taskCount = ParseAndValidateInt(taskCountInput, min, max);

    Console.Write("Введите максимально допустимую длину задачи: ");
    var taskLengthInput = Console.ReadLine();
    taskLength = ParseAndValidateInt(taskLengthInput, min, max);

    while (!isExit)
    {
        PrintUserName();
        Console.WriteLine(menuText);

        var userCommand = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(userCommand))
        {
            switch (userCommand!.Trim())
            {
                case "/start":
                    Start();
                    break;
                case "/help":
                    PrintUserName();
                    Help();
                    break;
                case "/info":
                    PrintUserName();
                    Info();
                    break;
                case string echoInput when echoInput.Contains("/echo") && echoInput.Length > 6:
                    Echo(echoInput);
                    break;
                case "/addtask":
                    CheckTaskCount();
                    PrintUserName();
                    AddTask();
                    break;
                case "/showtasks":
                    PrintUserName();
                    ShowActiveTasks();
                    break;
                case "/removetask":
                    PrintUserName();
                    RemoveTask();
                    break;
                case string completetaskInput when completetaskInput.Contains("/completetask") && completetaskInput.Length > 14:
                    Completetask(completetaskInput.Substring(14));
                    break;
                case "/showalltasks":
                    PrintUserName();
                    ShowAllTasks();
                    break;
                case "/exit":
                    Exit();
                    break;
                default:
                    Console.WriteLine("Неверная команда! Попробуйте заново!");
                    break;
            }
        }
    }
}
catch (ArgumentException ex)
{
    Console.WriteLine($"{ex.Message}");
}
catch (TaskCountLimitException ex)
{
    Console.WriteLine($"{ex.Message}");
}
catch (TaskLengthLimitException ex)
{
    Console.WriteLine($"{ex.Message}");
}
catch (DuplicateTaskException ex)
{
    Console.WriteLine($"{ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}

void Start()
{

    var userName = GetUserName();
    user = new ToDoUser(userName);

    Console.WriteLine($"Привет, {user.TelegramUserName}! Чем могу помочь?");
}

string GetUserName()
{
    string? name;
    do
    {
        Console.Write("Пожалуйста, введите ваше имя:");
        name = Console.ReadLine();
    }
    while (string.IsNullOrWhiteSpace(name));

    return name;
}

void PrintUserName()
{
    if (user is not null && !string.IsNullOrWhiteSpace(user.TelegramUserName)) Console.WriteLine($"{user.TelegramUserName}!");
}

void Help()
{
    string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/echo - данная команда становится доступной после ввода имени. При вводе этой команды с некоторым значением (например, /echo Hello), программа возвращает введенный текст (\"Hello\")\n/addtask - добавляет задачу в список\n/showtasks - отображает список всех добавленных задач со статусом Активна\n/removetask - удаляет задачу по номеру в списке\n/completetask - переводит статус задачи на Завершена. Пример: /completetask 73c7940a-ca8c-4327-8a15-9119bffd1d5e\n/showalltasks - отображает список всех добавленных задач\n/exit - для выхода из программы";
    Console.WriteLine(helpCommandText);
}

void Info()
{
    string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
    Console.WriteLine(infoCommandText);
}

void Echo(string echoInput)
{
    Console.WriteLine(user is not null && !string.IsNullOrWhiteSpace(user.TelegramUserName) ? usernameNotEnteredMessage : echoInput.Substring(6));
}

void CheckTaskCount()
{
    if (toDoItems.Any() && toDoItems.Count == taskCount) throw new TaskCountLimitException(taskCount);
}

void AddTask()
{
    if (user is null && string.IsNullOrWhiteSpace(user?.TelegramUserName))
    {
        Console.WriteLine(usernameNotEnteredMessage);
        return;
    }

    Console.Write("Пожалуйста, введите описание задачи: ");
    var task = Console.ReadLine();

    if (IsValidTaskLength(task) && !IsDuplicateTask(task!))
    {
        toDoItems.Add(new ToDoItem(user!, task!)!);
        Console.WriteLine($"Задача \"{task}\" добавлена.");
    }
}

bool IsValidTaskLength(string? task)
{
    ValidateString(task);

    if (task!.Length > taskLength) throw new TaskLengthLimitException(task.Length, taskLength);

    return true;
}

bool IsDuplicateTask(string task)
{
    foreach (var t in toDoItems)
    {
        if (task.Equals(t.Name)) throw new DuplicateTaskException(task);
    }

    return false;
}

void ShowActiveTasks()
{
    if (toDoItems.Any())
    {
        Console.WriteLine("Вот ваш список активных задач:");
        for (var i = 1; i <= toDoItems.Count(); i++)
        {
            if (toDoItems[i - 1].State == ToDoItemState.Active)
                Console.WriteLine($"{i} - {toDoItems[i - 1].Name} - {toDoItems[i - 1].CreatedAt} - {toDoItems[i - 1].Id}");
        }
    }
    else
    {
        Console.WriteLine("Список задач пуст");
    }
}

void RemoveTask()
{
    if (toDoItems.Any())
    {
        ShowAllTasks();
        Console.Write($"Введите номер задачи для удаления: ");
        var taskNumber = Console.ReadLine();
        if (int.TryParse(taskNumber, out int result) && result > 0 && result <= toDoItems.Count())
        {
            toDoItems.Remove(toDoItems[result - 1]);
            Console.WriteLine($"Задача удалена.");
        }
        else
        {
            Console.WriteLine($"Введен неверный номер для удаления");
        }
    }
    else
    {
        Console.WriteLine("Список задач пуст");
    }
}

void Completetask(string taskId)
{
    Guid id = Guid.Empty;
    if (!string.IsNullOrWhiteSpace(taskId)) Guid.TryParse(taskId, out id);

    var task = toDoItems.FirstOrDefault(t => t.Id == id);
    if (task is not null)
    {
        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.Now;

        Console.WriteLine("Ваша задача переведана в статус Completed");
    }
}

void ShowAllTasks()
{
    if (toDoItems.Any())
    {
        Console.WriteLine("Вот ваш список задач:");
        for (var i = 1; i <= toDoItems.Count(); i++)
        {
            Console.WriteLine($"{i} - ({toDoItems[i - 1].State}){toDoItems[i - 1].Name} - {toDoItems[i - 1].CreatedAt} - {toDoItems[i - 1].Id}");
        }
    }
    else
    {
        Console.WriteLine("Список задач пуст");
    }
}

int ParseAndValidateInt(string? str, int min, int max)
{
    if (!(int.TryParse(str, out int number) && number >= min && number <= max)) throw new ArgumentException($"Это должно быть число от {min} до {max}");

    return number;
}

void ValidateString(string? str)
{
    if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException($"Строка не должна быть пустой");
}

void Exit()
{
    isExit = true;
}