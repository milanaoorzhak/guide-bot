using System.ComponentModel;
using GuideBot;

try
{
    int taskCount;
    int taskLength;
    int min = 0;
    int max = 100;
    string? userName = string.Empty;
    string welcomeText = "Добро пожаловать!";
    string menuText = "\nДоступные команды: /start, /help, /info, /echo, /addtask, /showtasks, /removetask, /exit";
    bool isExit = false;
    List<string> tasks = new();

    Console.Write(welcomeText);

    Console.Write("Введите максимально допустимое количество задач: ");
    var taskCountInput = Console.ReadLine();
    taskCount = ParseAndValidateInt(taskCountInput, min, max);

    Console.Write("Введите максимально допустимую длину задачи: ");
    var taskLengthInput = Console.ReadLine();
    taskLength = ParseAndValidateInt(taskLengthInput, min, max);

    while (!isExit)
    {
        Console.WriteLine(!string.IsNullOrWhiteSpace(userName) ? $"\n{userName}, {menuText}" : menuText);

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
                    ShowTasks();
                    break;
                case "/removetask":
                    PrintUserName();
                    RemoveTask();
                    break;
                case "/exit":
                    Exit();
                    break;
            }
        }
    }

    void Start()
    {
        userName = GetUserName();
        Console.WriteLine($"Привет, {userName}! Чем могу помочь?");
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

    void Help()
    {
        string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/echo - данная команда становится доступной после ввода имени. При вводе этой команды с некоторым значением (например, /echo Hello), программа возвращает введенный текст (\"Hello\")\n/addtask - добавляет задачу в список\n/showtasks - отображает список всех добавленных задач\n/removetask - удаляет задачу по номеру в списке\n/exit - для выхода из программы";
        Console.WriteLine(helpCommandText);
    }

    void Info()
    {
        string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
        Console.WriteLine(infoCommandText);
    }

    void Echo(string echoInput)
    {
        string echoCommandText = "Имя пользователя не введено! Для работы с командой введите имя пользователя с помошью вызова команды /start";
        Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? echoCommandText : echoInput.Substring(6));
    }

    void CheckTaskCount()
    {
        if (tasks.Any() && tasks.Count == taskCount) throw new TaskCountLimitException(taskCount);
    }

    void AddTask()
    {
        Console.Write("Пожалуйста, введите описание задачи: ");
        var task = Console.ReadLine();

        if (IsValidTaskLength(task) && !IsDuplicateTask(task))
        {
            tasks.Add(task!);
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
        foreach (var t in tasks)
        {
            if (task.Equals(t)) throw new DuplicateTaskException(task);
        }

        return false;
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