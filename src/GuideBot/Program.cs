string? userName = string.Empty;
string welcomeText = "Добро пожаловать!";
string menuText = "\nВведите одну из команд для дальнейшей работы: /start, /help, /info, /exit.";
string helpCommandText = "Для работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/echo - данная команда становится доступной после ввода имени. При вводе этой команды с некоторым значением (например, /echo Hello), программа возвращает введенный текст (\"Hello\")\n/exit - для выхода из программы";
string infoCommandText = "Dерсия программы 1.0\nДата создания 23.11.2025";
string echoCommandText = "Имя пользователя не введено! Для работы с командой введите имя пользователя с помошью вызова команды /start";
bool isExit = false;

Console.WriteLine(welcomeText);

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
        Console.Write("Введите, пожалуйста, ваше имя: ");
        name = Console.ReadLine();
    }
    while (string.IsNullOrWhiteSpace(name));

    return name;
}

void PrintUserName()
{
    if (!string.IsNullOrWhiteSpace(userName)) Console.WriteLine($"{userName}!");
}