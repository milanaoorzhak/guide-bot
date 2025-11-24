string? userName = string.Empty;

Console.WriteLine("Добро пожаловать!");

while (true)
{
    if (!string.IsNullOrEmpty(userName))
    {
        Console.WriteLine($"\n{userName}, введите одну из команд для дальнейшей работы: /start, /help, /info, /echo, /exit.");
    }
    else
    {
        Console.WriteLine("\nВведите одну из команд для дальнейшей работы: /start, /help, /info, /exit.");
    }

    var userCommand = Console.ReadLine();
    if (!string.IsNullOrEmpty(userCommand))
    {
        switch (userCommand!.Trim())
        {
            case "/start":
                userName = GetUserName();
                break;
            case "/help":
                if (!string.IsNullOrEmpty(userName)) Console.WriteLine($"{userName}!");
                Console.WriteLine("Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/echo - данная команда становится доступной после ввода имени. При вводе этой команды с некоторым значением (например, /echo Hello), программа возвращает введенный текст (\"Hello\")\n/exit - для выхода из программы");
                break;
            case "/info":
                if (!string.IsNullOrEmpty(userName)) Console.WriteLine($"{userName}!");
                Console.WriteLine("Dерсия программы 1.0\nДата создания 23.11.2025");
                break;
            case string echoInput when echoInput.Contains("/echo") && echoInput.Length > 6:
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine($"Имя пользователя не введено! Для работы с командой введите имя пользователя с помошью вызова команды /start");
                }
                else
                {
                    Console.WriteLine(echoInput.Substring(6));
                }
                break;
            case "/exit": return;
        }
    }
}

string GetUserName()
{
    string? name;
    do
    {
        Console.Write("Введите, пожалуйста, ваше имя: ");
        name = Console.ReadLine();
    }
    while (string.IsNullOrEmpty(name));

    return name;
}