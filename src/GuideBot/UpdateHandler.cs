using System.Collections.Concurrent;
using GuideBot.Entities;
using GuideBot.Services;
using GuideBot.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GuideBot;

public class UpdateHandler : IUpdateHandler
{
    private const string AttractionsButton = "Достопримечательности";
    private const string MapButton = "Карта города";
    private const string SearchButton = "Поиск";
    private const string AboutButton = "О городе";
    private const string BackButton = "Назад";
    private const string ProfileButton = "Личный кабинет";
    private const string FavoritesButton = "Избранное";
    private const string MyRoutesButton = "Мои маршруты";
    private const string EventsButton = "События";
    private const string AddPlaceButton = "Добавить место";
    private const string AdminPanelButton = "Админ-панель";

    private readonly IGuideUserService _userService;
    private readonly IGuideCatalogService _guideCatalogService;
    private readonly IFavoriteService _favoriteService;
    private readonly ILikeService _likeService;
    private readonly ICommentService _commentService;
    private readonly IRouteService _routeService;
    private readonly IEventService _eventService;
    private readonly IAdminService _adminService;
    private readonly ConcurrentDictionary<long, ConversationState> _conversationStates = new();
    private readonly ConcurrentDictionary<long, Guid> _usersAwaitingComment = new();

    public UpdateHandler(
        IGuideUserService userService,
        IGuideCatalogService guideCatalogService,
        IFavoriteService favoriteService,
        ILikeService likeService,
        ICommentService commentService,
        IRouteService routeService,
        IEventService eventService,
        IAdminService adminService)
    {
        _userService = userService;
        _guideCatalogService = guideCatalogService;
        _favoriteService = favoriteService;
        _likeService = likeService;
        _commentService = commentService;
        _routeService = routeService;
        _eventService = eventService;
        _adminService = adminService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.CallbackQuery is not null)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                return;
            }

            if (update.Message?.Text is null)
            {
                return;
            }

            await HandleMessageAsync(botClient, update.Message, cancellationToken);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(botClient, ex, HandleErrorSource.PollingError, cancellationToken);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        Console.WriteLine($"HandleError: {exception}");
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var text = message.Text!.Trim();
        var telegramUserId = message.From!.Id;

        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            await RegisterUserIfNeededAsync(message.From, cancellationToken);
            _conversationStates.TryRemove(telegramUserId, out _);
            await SendMainMenuAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (text.Equals("/attractions", StringComparison.OrdinalIgnoreCase) || text == AttractionsButton)
        {
            _conversationStates.TryRemove(telegramUserId, out _);
            await SendCategoryMenuAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (text.Equals("/map", StringComparison.OrdinalIgnoreCase) || text == MapButton)
        {
            _conversationStates.TryRemove(telegramUserId, out _);
            await SendCityMapAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (text.Equals("/search", StringComparison.OrdinalIgnoreCase) || text == SearchButton)
        {
            _conversationStates[telegramUserId] = ConversationState.AwaitingSearchQuery;
            await botClient.SendMessage(
                message.Chat.Id,
                "Введите название места для поиска:",
                replyMarkup: BuildBackKeyboard(),
                cancellationToken: cancellationToken);
            return;
        }

        if (text.Equals("/about", StringComparison.OrdinalIgnoreCase) || text == AboutButton)
        {
            _conversationStates.TryRemove(telegramUserId, out _);
            await SendCityInfoAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (text == BackButton)
        {
            _conversationStates.TryRemove(telegramUserId, out _);
            _usersAwaitingComment.TryRemove(telegramUserId, out _);
            await SendMainMenuAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (_conversationStates.TryGetValue(telegramUserId, out var state))
        {
            await HandleConversationStateAsync(botClient, message, state, telegramUserId, cancellationToken);
            return;
        }

        var user = await GetCurrentUserAsync(telegramUserId, cancellationToken);

        if (text == ProfileButton || text.Equals("/profile", StringComparison.OrdinalIgnoreCase))
        {
            await SendProfileMenuAsync(botClient, message.Chat.Id, user, cancellationToken);
            return;
        }

        if (text == FavoritesButton || text.Equals("/favorites", StringComparison.OrdinalIgnoreCase))
        {
            await SendFavoritesAsync(botClient, message.Chat.Id, user, cancellationToken);
            return;
        }

        if (text == MyRoutesButton || text.Equals("/routes", StringComparison.OrdinalIgnoreCase))
        {
            await SendUserRoutesAsync(botClient, message.Chat.Id, user, cancellationToken);
            return;
        }

        if (text == EventsButton || text.Equals("/events", StringComparison.OrdinalIgnoreCase))
        {
            await SendEventsAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        if (HasRole(user, GuideUserRole.Expert) && text.Equals("/addplace", StringComparison.OrdinalIgnoreCase))
        {
            _conversationStates[telegramUserId] = ConversationState.AwaitingNewPlaceName;
            await botClient.SendMessage(message.Chat.Id, "Введите название нового места:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        if (HasRole(user, GuideUserRole.Administrator) && text == AdminPanelButton)
        {
            await SendAdminMenuAsync(botClient, message.Chat.Id, cancellationToken);
            return;
        }

        await botClient.SendMessage(
            message.Chat.Id,
            "Выбери действие через меню или начни с команды /start.",
            replyMarkup: BuildMainMenuKeyboard(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data is null)
        {
            return;
        }

        try
        {
            var callbackData = GuideCallbackData.Parse(callbackQuery.Data);

            switch (callbackData.Action)
            {
                case GuideCallbackAction.ShowCategories:
                    await EditMessageAsync(
                        botClient,
                        callbackQuery,
                        "Выбери категорию:",
                        BuildCategoryKeyboard(await _guideCatalogService.GetCategoriesAsync(cancellationToken)),
                        cancellationToken);
                    break;
                case GuideCallbackAction.ShowCategoryPlaces:
                    if (Guid.TryParse(callbackData.Payload, out var categoryId))
                    {
                        await ShowCategoryAttractionsAsync(botClient, callbackQuery, categoryId, cancellationToken);
                    }
                    break;
                case GuideCallbackAction.ShowAttraction:
                    if (Guid.TryParse(callbackData.Payload, out var attractionId))
                    {
                        await SendAttractionCardAsync(botClient, callbackQuery.Message!.Chat.Id, attractionId, cancellationToken);
                    }
                    break;
                case GuideCallbackAction.ShowSearchPrompt:
                    _conversationStates[callbackQuery.From.Id] = ConversationState.AwaitingSearchQuery;
                    await EditMessageAsync(
                        botClient,
                        callbackQuery,
                        "Введите название места для поиска:",
                        BuildInlineBackToMainKeyboard(),
                        cancellationToken);
                    break;
                case GuideCallbackAction.BackToMainMenu:
                    await EditMessageAsync(
                        botClient,
                        callbackQuery,
                        "Добро пожаловать в Кызыл! Выбери действие:",
                        BuildInlineMainMenuKeyboard(),
                        cancellationToken);
                    await botClient.SendMessage(
                        callbackQuery.Message!.Chat.Id,
                        "Главное меню:",
                        replyMarkup: BuildMainMenuKeyboard(),
                        cancellationToken: cancellationToken);
                    break;
            }
        }
        finally
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }
    }

    private async Task RegisterUserIfNeededAsync(User telegramUser, CancellationToken cancellationToken)
    {
        var existingUser = await _userService.GetUserAsync(telegramUser.Id, cancellationToken);
        if (existingUser is not null)
        {
            return;
        }

        await _userService.RegisterUserAsync(
            telegramUser.Id,
            telegramUser.Username ?? telegramUser.FirstName,
            GuideUserRole.Guest,
            cancellationToken);
    }

    private async Task SendMainMenuAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId,
            "Добро пожаловать в Кызыл! Выбери действие:",
            replyMarkup: BuildMainMenuKeyboard(),
            cancellationToken: cancellationToken);
    }

    private async Task SendCategoryMenuAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var categories = await _guideCatalogService.GetCategoriesAsync(cancellationToken);
        await botClient.SendMessage(
            chatId,
            "Выбери категорию:",
            replyMarkup: BuildCategoryKeyboard(categories),
            cancellationToken: cancellationToken);
    }

    private async Task SendCityMapAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var cityInfo = await _guideCatalogService.GetCityInfoAsync(cancellationToken);
        await botClient.SendMessage(
            chatId,
            "Карта Кызыла с основными достопримечательностями:",
            replyMarkup: new InlineKeyboardMarkup(
                new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Открыть в Картах", cityInfo.MapUrl)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                    }
                }),
            cancellationToken: cancellationToken);
    }

    private async Task SendCityInfoAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var cityInfo = await _guideCatalogService.GetCityInfoAsync(cancellationToken);
        var message = $"{cityInfo.Title}\n\n{cityInfo.Description}";

        await botClient.SendMessage(
            chatId,
            message,
            replyMarkup: BuildInlineBackToMainKeyboard(),
            cancellationToken: cancellationToken);
    }

    private async Task SendSearchResultsAsync(ITelegramBotClient botClient, ChatId chatId, string query, CancellationToken cancellationToken)
    {
        var results = await _guideCatalogService.SearchAttractionsAsync(query, cancellationToken);
        if (results.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                $"По запросу '{query}' ничего не найдено.",
                replyMarkup: BuildInlineSearchEmptyKeyboard(),
                cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(
            chatId,
            $"Найдено по запросу '{query}':",
            replyMarkup: BuildAttractionsKeyboard(results, includeSearchPromptButton: true),
            cancellationToken: cancellationToken);
    }

    private async Task ShowCategoryAttractionsAsync(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var category = await _guideCatalogService.GetCategoryAsync(categoryId, cancellationToken);
        var attractions = await _guideCatalogService.GetAttractionsByCategoryAsync(categoryId, cancellationToken);

        if (category is null)
        {
            await EditMessageAsync(
                botClient,
                callbackQuery,
                "Категория не найдена.",
                BuildInlineBackToMainKeyboard(),
                cancellationToken);
            return;
        }

        var message = attractions.Count == 0
            ? $"В категории \"{category.Name}\" пока нет мест."
            : $"{category.Name}\n{category.Description}";

        await EditMessageAsync(
            botClient,
            callbackQuery,
            message,
            BuildAttractionsKeyboard(attractions, includeSearchPromptButton: false, includeCategoryBackButton: true),
            cancellationToken);
    }

    private async Task SendAttractionCardAsync(
        ITelegramBotClient botClient,
        ChatId chatId,
        Guid attractionId,
        CancellationToken cancellationToken)
    {
        var attraction = await _guideCatalogService.GetAttractionAsync(attractionId, cancellationToken);
        if (attraction is null)
        {
            await botClient.SendMessage(
                chatId,
                "Карточка места не найдена.",
                replyMarkup: BuildInlineBackToMainKeyboard(),
                cancellationToken: cancellationToken);
            return;
        }

        var text = $"{attraction.Name}\n\n{attraction.ShortDescription}\n\n{attraction.FullDescription}\n\nАдрес: {attraction.Address}";
        var keyboard = new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("Открыть на карте", attraction.MapUrl)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("К категориям", GuideCallbackData.Create(GuideCallbackAction.ShowCategories)),
                    InlineKeyboardButton.WithCallbackData("В меню", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                }
            });

        try
        {
            await botClient.SendPhoto(
                chatId,
                InputFile.FromUri(attraction.PhotoUrl),
                caption: text,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex) when (
            ex.Message.Contains("failed to get HTTP URL content", StringComparison.OrdinalIgnoreCase) ||
            ex.Message.Contains("wrong type of the web page content", StringComparison.OrdinalIgnoreCase))
        {
            var fallbackText = $"{text}\n\nФото: {attraction.PhotoUrl}";
            await botClient.SendMessage(
                chatId,
                fallbackText,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
    }

    private static ReplyKeyboardMarkup BuildMainMenuKeyboard()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                new[] { new KeyboardButton(AttractionsButton), new KeyboardButton(MapButton) },
                new[] { new KeyboardButton(SearchButton), new KeyboardButton(AboutButton) }
            })
        {
            ResizeKeyboard = true
        };
    }

    private static ReplyKeyboardMarkup BuildBackKeyboard()
    {
        return new ReplyKeyboardMarkup(new[] { new[] { new KeyboardButton(BackButton) } })
        {
            ResizeKeyboard = true
        };
    }

    private static InlineKeyboardMarkup BuildCategoryKeyboard(IReadOnlyList<AttractionCategory> categories)
    {
        var buttons = categories
            .Select(category => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    category.Name,
                    GuideCallbackData.Create(GuideCallbackAction.ShowCategoryPlaces, category.Id.ToString()))
            })
            .ToList();

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
        });

        return new InlineKeyboardMarkup(buttons);
    }

    private static InlineKeyboardMarkup BuildAttractionsKeyboard(
        IReadOnlyList<Attraction> attractions,
        bool includeSearchPromptButton,
        bool includeCategoryBackButton = false)
    {
        var buttons = attractions
            .Select(attraction => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    attraction.Name,
                    GuideCallbackData.Create(GuideCallbackAction.ShowAttraction, attraction.Id.ToString()))
            })
            .ToList();

        if (includeSearchPromptButton)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Новый поиск", GuideCallbackData.Create(GuideCallbackAction.ShowSearchPrompt))
            });
        }

        if (includeCategoryBackButton)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("К категориям", GuideCallbackData.Create(GuideCallbackAction.ShowCategories))
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
        });

        return new InlineKeyboardMarkup(buttons);
    }

    private static InlineKeyboardMarkup BuildInlineBackToMainKeyboard()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                }
            });
    }

    private static InlineKeyboardMarkup BuildInlineMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Достопримечательности", GuideCallbackData.Create(GuideCallbackAction.ShowCategories)),
                    InlineKeyboardButton.WithCallbackData("Поиск", GuideCallbackData.Create(GuideCallbackAction.ShowSearchPrompt))
                }
            });
    }

    private static InlineKeyboardMarkup BuildInlineSearchEmptyKeyboard()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Повторить поиск", GuideCallbackData.Create(GuideCallbackAction.ShowSearchPrompt))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                }
            });
    }

    private static ReplyKeyboardMarkup BuildUserMenuKeyboard(GuideUserRole role)
    {
        var buttons = new List<KeyboardButton[]>();

        if (role >= GuideUserRole.AuthorizedUser)
        {
            buttons.Add(new[] { new KeyboardButton(FavoritesButton), new KeyboardButton(MyRoutesButton) });
        }

        buttons.Add(new[] { new KeyboardButton(EventsButton) });

        if (role >= GuideUserRole.Expert)
        {
            buttons.Add(new[] { new KeyboardButton(AddPlaceButton) });
        }

        if (role >= GuideUserRole.Administrator)
        {
            buttons.Add(new[] { new KeyboardButton(AdminPanelButton) });
        }

        buttons.Add(new[] { new KeyboardButton(BackButton) });

        return new ReplyKeyboardMarkup(buttons.ToArray()) { ResizeKeyboard = true };
    }

    private static InlineKeyboardMarkup BuildAdminKeyboard()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Статистика", GuideCallbackData.Create(GuideCallbackAction.ShowStatistics)),
                    InlineKeyboardButton.WithCallbackData("Управление пользователями", GuideCallbackData.Create(GuideCallbackAction.ManageUsers))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Модерация комментариев", GuideCallbackData.Create(GuideCallbackAction.ShowComments))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                }
            });
    }

    private static async Task EditMessageAsync(
        ITelegramBotClient botClient,
        CallbackQuery callbackQuery,
        string text,
        InlineKeyboardMarkup replyMarkup,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.EditMessageText(
                callbackQuery.Message!.Chat.Id,
                callbackQuery.Message.MessageId,
                text,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException ex) when (
            ex.Message.Contains("there is no text in the message to edit", StringComparison.OrdinalIgnoreCase))
        {
            await botClient.SendMessage(
                callbackQuery.Message!.Chat.Id,
                text,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleConversationStateAsync(ITelegramBotClient botClient, Message message, ConversationState state, long telegramUserId, CancellationToken cancellationToken)
    {
        var text = message.Text!.Trim();
        var user = await GetCurrentUserAsync(telegramUserId, cancellationToken);

        switch (state)
        {
            case ConversationState.AwaitingSearchQuery:
                _conversationStates.TryRemove(telegramUserId, out _);
                await SendSearchResultsAsync(botClient, message.Chat.Id, text, cancellationToken);
                break;
            case ConversationState.AwaitingComment:
                if (_usersAwaitingComment.TryRemove(telegramUserId, out var attractionIdForComment) && user is not null)
                {
                    await _commentService.AddCommentAsync(user.Id, attractionIdForComment, text, user.TelegramUserName, cancellationToken);
                    await botClient.SendMessage(message.Chat.Id, "Комментарий добавлен и ожидает модерации.", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                }
                break;
            case ConversationState.AwaitingRouteName:
                _conversationStates[telegramUserId] = ConversationState.AwaitingRouteDescription;
                await botClient.SendMessage(message.Chat.Id, "Введите описание маршрута:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;
        }
    }

    private async Task SendProfileMenuAsync(ITelegramBotClient botClient, ChatId chatId, GuideUser? user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            await botClient.SendMessage(chatId, "Сначала зарегистрируйтесь /start", cancellationToken: cancellationToken);
            return;
        }

        var roleName = user.Role switch
        {
            GuideUserRole.Guest => "Гость",
            GuideUserRole.AuthorizedUser => "Пользователь",
            GuideUserRole.Expert => "Эксперт",
            GuideUserRole.Moderator => "Модератор",
            GuideUserRole.Administrator => "Администратор",
            _ => "Гость"
        };

        var message = $"Личный кабинет\n\nИмя: {user.TelegramUserName}\nРоль: {roleName}";
        await botClient.SendMessage(chatId, message, replyMarkup: BuildUserMenuKeyboard(user.Role), cancellationToken: cancellationToken);
    }

    private async Task SendFavoritesAsync(ITelegramBotClient botClient, ChatId chatId, GuideUser? user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            await botClient.SendMessage(chatId, "Сначала зарегистрируйтесь /start", cancellationToken: cancellationToken);
            return;
        }

        var favorites = await _favoriteService.GetUserFavoritesAsync(user.Id, cancellationToken);
        if (favorites.Count == 0)
        {
            await botClient.SendMessage(chatId, "У вас пока нет избранного.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        await botClient.SendMessage(chatId, "Ваше избранное:", replyMarkup: BuildAttractionsKeyboard(favorites, includeSearchPromptButton: false), cancellationToken: cancellationToken);
    }

    private async Task SendUserRoutesAsync(ITelegramBotClient botClient, ChatId chatId, GuideUser? user, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            await botClient.SendMessage(chatId, "Сначала зарегистрируйтесь /start", cancellationToken: cancellationToken);
            return;
        }

        var routes = await _routeService.GetUserRoutesAsync(user.Id, cancellationToken);
        if (routes.Count == 0)
        {
            await botClient.SendMessage(chatId, "У вас пока нет маршрутов.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        var buttons = routes.Select(r => new[] { InlineKeyboardButton.WithCallbackData(r.Name, GuideCallbackData.Create(GuideCallbackAction.ShowRoutes, r.Id.ToString())) }).ToList();
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu)) });
        await botClient.SendMessage(chatId, "Ваши маршруты:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cancellationToken);
    }

    private async Task SendEventsAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var events = await _eventService.GetActiveEventsAsync(cancellationToken);
        if (events.Count == 0)
        {
            await botClient.SendMessage(chatId, "Нет активных событий.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        var message = "Активные события:\n\n" + string.Join("\n\n", events.Select(e => $"{e.Name}\n{e.Description}\nДата: {e.StartDate:dd.MM.yyyy}"));
        await botClient.SendMessage(chatId, message, replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
    }

    private async Task SendAdminMenuAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var stats = await _adminService.GetStatisticsAsync(cancellationToken);
        var message = $"Панель администратора\n\nПользователей: {stats.TotalUsers}\nДостопримечательностей: {stats.TotalAttractions}\nКомментариев: {stats.TotalComments}\nСобытий: {stats.ActiveEvents}\nОжидают модерации: {stats.PendingComments}";
        await botClient.SendMessage(chatId, message, replyMarkup: BuildAdminKeyboard(), cancellationToken: cancellationToken);
    }

    private enum ConversationState
    {
        AwaitingSearchQuery,
        AwaitingComment,
        AwaitingRouteName,
        AwaitingRouteDescription,
        AwaitingNewPlaceName,
        AwaitingNewPlaceDescription,
        AwaitingNewPlaceAddress
    }

    private async Task<GuideUser?> GetCurrentUserAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        return await _userService.GetUserAsync(telegramUserId, cancellationToken);
    }

    private bool HasRole(GuideUser? user, GuideUserRole requiredRole)
    {
        return user?.Role >= requiredRole;
    }
}
