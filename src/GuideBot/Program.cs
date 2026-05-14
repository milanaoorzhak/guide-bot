using GuideBot;
using GuideBot.DataAccess;
using GuideBot.Infrastructure.DataAccess;
using GuideBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

var token = Environment.GetEnvironmentVariable("TUVAN_GUIDE_BOT_TOKEN")
    ?? throw new Exception("Bot token not found.");

var commands = new List<BotCommand>
{
    new() { Command = "start", Description = "Открыть главное меню" },
    new() { Command = "attractions", Description = "Показать достопримечательности" },
    new() { Command = "map", Description = "Показать карту города" },
    new() { Command = "search", Description = "Поиск места по названию" },
    new() { Command = "about", Description = "Информация о Кызыле" },
    new() { Command = "profile", Description = "Личный кабинет" },
    new() { Command = "favorites", Description = "Избранное" },
    new() { Command = "routes", Description = "Мои маршруты" },
    new() { Command = "events", Description = "События" }
};

using var cts = new CancellationTokenSource();

var connectionString = "Host=localhost;Port=5432;Database=tuva_guide_bot;Username=admin;Password=admin1234!";
var dataContextFactory = new GuideDataContextFactory(connectionString);

await DatabaseInitializer.InitializeAsync(dataContextFactory, cancellationToken: cts.Token);

IGuideUserRepository userRepository = new SqlGuideUserRepository(dataContextFactory);
IGuideCatalogRepository guideCatalogRepository = new SqlGuideCatalogRepository(dataContextFactory);
IFavoriteRepository favoriteRepository = new SqlFavoriteRepository(dataContextFactory);
ILikeRepository likeRepository = new SqlLikeRepository(dataContextFactory);
ICommentRepository commentRepository = new SqlCommentRepository(dataContextFactory);
IRouteRepository routeRepository = new SqlRouteRepository(dataContextFactory);
IEventRepository eventRepository = new SqlEventRepository(dataContextFactory);
INotificationRepository notificationRepository = new SqlNotificationRepository(dataContextFactory);
IAdminRepository adminRepository = new SqlAdminRepository(dataContextFactory);

IGuideUserService userService = new GuideUserService(userRepository);
IGuideCatalogService guideCatalogService = new GuideCatalogService(guideCatalogRepository);
IFavoriteService favoriteService = new FavoriteService(favoriteRepository, guideCatalogService);
ILikeService likeService = new LikeService(likeRepository);
ICommentService commentService = new CommentService(commentRepository);
IRouteService routeService = new RouteService(routeRepository);
IEventService eventService = new EventService(eventRepository);
INotificationService notificationService = new NotificationService(notificationRepository);
IAdminService adminService = new AdminService(adminRepository, commentService, eventService, notificationService);

var handler = new UpdateHandler(
    userService,
    guideCatalogService,
    favoriteService,
    likeService,
    commentService,
    routeService,
    eventService,
    adminService);
var bot = new TelegramBotClient(token: token, cancellationToken: cts.Token);

try
{
    await bot.SetMyCommands(commands, cancellationToken: cts.Token);
    bot.StartReceiving(handler, cancellationToken: cts.Token);

    var me = await bot.GetMe(cts.Token);
    Console.WriteLine($"{me.FirstName} запущен. Нажмите Ctrl+C для выхода.");

    Console.CancelKeyPress += (_, eventArgs) =>
    {
        eventArgs.Cancel = true;
        cts.Cancel();
    };

    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
}
catch (Exception ex)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType()}, {ex.Message}, {ex.StackTrace}, {ex.InnerException}");
}
