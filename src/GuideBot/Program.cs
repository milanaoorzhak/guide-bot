string? userName = string.Empty;
string welcomeText = "Добро пожаловать!";
string menuText = "\nДоступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit";
string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/echo - данная команда становится доступной после ввода имени. При вводе этой команды с некоторым значением (например, /echo Hello), программа возвращает введенный текст (\"Hello\")\n/addtask - добавляет задачу в список\n/showtasks - отображает список всех добавленных задач\n/removetask - удаляет задачу по номеру в списке\n/exit - для выхода из программы";
string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
string echoCommandText = "Имя пользователя не введено! Для работы с командой введите имя пользователя с помошью вызова команды /start";
bool isExit = false;
List<string> tasks = new();

Console.Write(welcomeText);

while (!isExit)
{
    Console.WriteLine(!string.IsNullOrWhiteSpace(userName) ? $"\n{userName}, {menuText}" : menuText);

    var userCommand = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(userCommand))
    {
        switch (userCommand!.Trim())
        {
            case "/start":
                userName = GetUserName();
                Console.WriteLine($"Привет, {userName}! Чем могу помочь?");
                break;
            case "/help":
                PrintUserName();
                Console.WriteLine(helpCommandText);
                break;
            case "/info":
                PrintUserName();
                Console.WriteLine(infoCommandText);
                break;
            case string echoInput when echoInput.Contains("/echo") && echoInput.Length > 6:
                Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? echoCommandText : echoInput.Substring(6));
                break;
            case "/addtask":
                PrintUserName();
                AddTask();
                break;
            case "/showtasks":
                PrintUserName();
                ShowTasks();
                break;
            case "/removetask":
                PrintUserName();
                RemoveTask();
                break;
            case "/exit":
                isExit = true;
                break;
        }
    }
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
    if (!string.IsNullOrWhiteSpace(userName)) Console.WriteLine($"{userName}!");
}

void AddTask()
{
    Console.Write("Пожалуйста, введите описание задачи: ");
    var task = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(task))
    {
        tasks.Add(task);
        Console.WriteLine($"Задача \"{task}\" добавлена.");
    }
}

void ShowTasks()
{
    if (tasks.Any())
    {
        Console.WriteLine("Вот ваш список задач:");
        for (var i = 1; i <= tasks.Count(); i++)
        {
            Console.WriteLine($"{i} - {tasks[i - 1]}");
        }
    }
    else
    {
        Console.WriteLine("Список задач пуст");
    }
}

void RemoveTask()
{
    if (tasks.Any())
    {
        ShowTasks();
        Console.Write($"Введите номер задачи для удаления: ");
        var taskNumber = Console.ReadLine();
        if (int.TryParse(taskNumber, out int result) && result > 0 && result <= tasks.Count())
        {
            tasks.Remove(tasks[result - 1]);
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