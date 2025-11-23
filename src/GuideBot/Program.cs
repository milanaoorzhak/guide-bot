Console.WriteLine("Добро пожаловать!");

while (true)
{
    Console.WriteLine("\nВведите одну из команд для дальнейшей работы: /start, /help, /info, /exit.");

    string? userName;
    var userCommand = Console.ReadLine();

    switch (userCommand)
    {
        case "/start":
            Console.Write("Введите, пожалуйста, ваше имя: ");
            userName = Console.ReadLine();
            break;
        case "/help":
            Console.WriteLine("\nДля работы с программой необходимо ввести одну из комманд: \n/start - для старта работы программы, \n/help - отображает краткую справочную информацию о том, как пользоваться программой, \n/info - предоставляет информацию о версии программы и дате её создания, \n/exit - для выхода из программы");
            break;
        case "/info":
            Console.WriteLine("Dерсия программы 1.0\nДата создания 23.11.2025");
            break;
        case "/echo": break;
        case "/exit": return;
    }
}