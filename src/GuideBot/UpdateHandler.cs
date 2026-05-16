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
    private const string RegisterButton = "Регистрация";
    private const string ProfileButton = "Личный кабинет";
    private const string FavoritesButton = "Избранное";
    private const string MyRoutesButton = "Мои маршруты";
    private const string CreateRouteButton = "Создать маршрут";
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
    private readonly ConcurrentDictionary<long, string> _newPlaceNames = new();
    private readonly ConcurrentDictionary<long, string> _newPlaceDescriptions = new();
    private readonly ConcurrentDictionary<long, string> _newPlaceAddresses = new();
    private readonly ConcurrentDictionary<long, string> _editAttractionFields = new();
    private readonly ConcurrentDictionary<long, string> _editEventFields = new();
    private readonly ConcurrentDictionary<long, string> _eventNames = new();
    private readonly ConcurrentDictionary<long, string> _eventDescriptions = new();
    private readonly ConcurrentDictionary<long, string> _routeNames = new();
    private readonly ConcurrentDictionary<long, string> _notificationMessages = new();

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
            var startUser = await RegisterUserIfNeededAsync(message.From, cancellationToken);
            _conversationStates.TryRemove(telegramUserId, out _);
            await SendMainMenuAsync(botClient, message.Chat.Id, startUser, cancellationToken);
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
            _newPlaceNames.TryRemove(telegramUserId, out _);
            _newPlaceDescriptions.TryRemove(telegramUserId, out _);
            _newPlaceAddresses.TryRemove(telegramUserId, out _);
            _editAttractionFields.TryRemove(telegramUserId, out _);
            _editEventFields.TryRemove(telegramUserId, out _);
            _eventNames.TryRemove(telegramUserId, out _);
            _eventDescriptions.TryRemove(telegramUserId, out _);
            _routeNames.TryRemove(telegramUserId, out _);
            _notificationMessages.TryRemove(telegramUserId, out _);
            var currentUser = await GetCurrentUserAsync(telegramUserId, cancellationToken);
            await SendMainMenuAsync(botClient, message.Chat.Id, currentUser, cancellationToken);
            return;
        }

        if (_conversationStates.TryGetValue(telegramUserId, out var state))
        {
            await HandleConversationStateAsync(botClient, message, state, telegramUserId, cancellationToken);
            return;
        }

        var user = await GetCurrentUserAsync(telegramUserId, cancellationToken);

        if (text == RegisterButton)
        {
            await HandleRegistrationAsync(botClient, message.Chat.Id, user, telegramUserId, cancellationToken);
            return;
        }

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

        if (text == CreateRouteButton && HasRole(user, GuideUserRole.AuthorizedUser))
        {
            _conversationStates[telegramUserId] = ConversationState.AwaitingRouteName;
            await botClient.SendMessage(message.Chat.Id, "Введите название маршрута:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
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
            replyMarkup: BuildMainMenuKeyboard(user),
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
            var telegramUserId = callbackQuery.From.Id;
            var user = await GetCurrentUserAsync(telegramUserId, cancellationToken);
            var chatId = callbackQuery.Message!.Chat.Id;

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
                        await SendAttractionCardAsync(botClient, chatId, attractionId, user, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ShowSearchPrompt:
                    _conversationStates[telegramUserId] = ConversationState.AwaitingSearchQuery;
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
                        chatId,
                        "Главное меню:",
                        replyMarkup: BuildMainMenuKeyboard(user),
                        cancellationToken: cancellationToken);
                    break;

                case GuideCallbackAction.AddToFavorites:
                    if (user is not null && Guid.TryParse(callbackData.Payload, out var favAttractionId))
                    {
                        await _favoriteService.AddFavoriteAsync(user.Id, favAttractionId, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Добавлено в избранное!", cancellationToken: cancellationToken);
                        await SendAttractionCardAsync(botClient, chatId, favAttractionId, user, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.RemoveFromFavorites:
                    if (user is not null && Guid.TryParse(callbackData.Payload, out var unfavAttractionId))
                    {
                        await _favoriteService.RemoveFavoriteAsync(user.Id, unfavAttractionId, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Убрано из избранного.", cancellationToken: cancellationToken);
                        await SendAttractionCardAsync(botClient, chatId, unfavAttractionId, user, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.Like:
                    if (user is not null && Guid.TryParse(callbackData.Payload, out var likeAttractionId))
                    {
                        await _likeService.SetReactionAsync(user.Id, likeAttractionId, true, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Вы поставили лайк!", cancellationToken: cancellationToken);
                        await SendAttractionCardAsync(botClient, chatId, likeAttractionId, user, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.Dislike:
                    if (user is not null && Guid.TryParse(callbackData.Payload, out var dislikeAttractionId))
                    {
                        await _likeService.SetReactionAsync(user.Id, dislikeAttractionId, false, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Вы поставили дизлайк.", cancellationToken: cancellationToken);
                        await SendAttractionCardAsync(botClient, chatId, dislikeAttractionId, user, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ShowComments:
                    if (Guid.TryParse(callbackData.Payload, out var commentsAttractionId))
                    {
                        await ShowCommentsAsync(botClient, chatId, commentsAttractionId, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.AddComment:
                    if (user is not null && Guid.TryParse(callbackData.Payload, out var commentAttractionId))
                    {
                        _usersAwaitingComment[telegramUserId] = commentAttractionId;
                        _conversationStates[telegramUserId] = ConversationState.AwaitingComment;
                        await botClient.SendMessage(chatId, "Введите ваш комментарий:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ShowFavorites:
                    await SendFavoritesAsync(botClient, chatId, user, cancellationToken);
                    break;

                case GuideCallbackAction.ShowRoutes:
                    if (Guid.TryParse(callbackData.Payload, out var routeId))
                    {
                        await ShowRouteDetailsAsync(botClient, chatId, routeId, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ShowEvents:
                    await SendEventsAsync(botClient, chatId, cancellationToken);
                    break;

                case GuideCallbackAction.ShowUserMenu:
                    await SendProfileMenuAsync(botClient, chatId, user, cancellationToken);
                    break;

                case GuideCallbackAction.ShowAdminMenu:
                    await SendAdminMenuAsync(botClient, chatId, cancellationToken);
                    break;

                case GuideCallbackAction.ShowStatistics:
                    await SendAdminMenuAsync(botClient, chatId, cancellationToken);
                    break;

                case GuideCallbackAction.ManageUsers:
                    if (HasRole(user, GuideUserRole.Administrator))
                    {
                        await ShowUserManagementAsync(botClient, chatId, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ShowThematicRoutes:
                    await ShowThematicRoutesAsync(botClient, chatId, cancellationToken);
                    break;

                case GuideCallbackAction.AddAttraction:
                    if (HasRole(user, GuideUserRole.Administrator))
                    {
                        _conversationStates[telegramUserId] = ConversationState.AwaitingNewPlaceName;
                        await botClient.SendMessage(chatId, "Введите название нового места:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                    break;

                case GuideCallbackAction.EditAttraction:
                    if (HasRole(user, GuideUserRole.Administrator) && Guid.TryParse(callbackData.Payload, out var editAttractionId))
                    {
                        _editAttractionFields[telegramUserId] = editAttractionId.ToString();
                        _conversationStates[telegramUserId] = ConversationState.AwaitingEditPlace;
                        var attraction = await _guideCatalogService.GetAttractionAsync(editAttractionId, cancellationToken);
                        if (attraction is not null)
                        {
                            await EditMessageAsync(botClient, callbackQuery,
                                $"Редактирование: {attraction.Name}\n\nТекущее название: {attraction.Name}\nКатегория ID: {attraction.CategoryId}\nОписание: {attraction.ShortDescription}\n\nВведите новое название (или '-' чтобы оставить текущее):",
                                BuildInlineBackToMainKeyboard(), cancellationToken);
                        }
                    }
                    break;

                case GuideCallbackAction.AddEvent:
                    if (HasRole(user, GuideUserRole.Administrator))
                    {
                        _conversationStates[telegramUserId] = ConversationState.AwaitingEventName;
                        await botClient.SendMessage(chatId, "Введите название события:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                    break;

                case GuideCallbackAction.EditEvent:
                    if (HasRole(user, GuideUserRole.Administrator) && Guid.TryParse(callbackData.Payload, out var editEventId))
                    {
                        _editEventFields[telegramUserId] = editEventId.ToString();
                        _conversationStates[telegramUserId] = ConversationState.AwaitingEventName;
                        await botClient.SendMessage(chatId, "Введите новое название события (или '-' чтобы оставить текущее):", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                    break;

                case GuideCallbackAction.ApproveComment:
                    if (HasRole(user, GuideUserRole.Administrator) && Guid.TryParse(callbackData.Payload, out var approveCommentId))
                    {
                        await _commentService.ApproveCommentAsync(approveCommentId, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Комментарий одобрен.", cancellationToken: cancellationToken);
                        await SendPendingCommentsAsync(botClient, chatId, cancellationToken);
                    }
                    break;

                case GuideCallbackAction.Broadcast:
                    if (HasRole(user, GuideUserRole.Administrator))
                    {
                        _conversationStates[telegramUserId] = ConversationState.AwaitingNotificationMessage;
                        await botClient.SendMessage(chatId, "Введите текст уведомления для рассылки:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                    break;

                case GuideCallbackAction.RejectComment:
                    if (HasRole(user, GuideUserRole.Administrator) && Guid.TryParse(callbackData.Payload, out var rejectCommentId))
                    {
                        await _commentService.RejectCommentAsync(rejectCommentId, cancellationToken);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Комментарий отклонён.", cancellationToken: cancellationToken);
                        await SendPendingCommentsAsync(botClient, chatId, cancellationToken);
                    }
                    break;
            }
        }
        finally
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
        }
    }

    private async Task<GuideUser?> RegisterUserIfNeededAsync(User telegramUser, CancellationToken cancellationToken)
    {
        var existingUser = await _userService.GetUserAsync(telegramUser.Id, cancellationToken);
        if (existingUser is not null)
        {
            return existingUser;
        }

        var registeredUser = await _userService.RegisterUserAsync(
            telegramUser.Id,
            telegramUser.Username ?? telegramUser.FirstName,
            GuideUserRole.Guest,
            cancellationToken);
        return registeredUser;
    }

    private async Task HandleRegistrationAsync(ITelegramBotClient botClient, ChatId chatId, GuideUser? user, long telegramUserId, CancellationToken cancellationToken)
    {
        if (user is null)
        {
            user = await RegisterUserIfNeededAsync(new User { Id = telegramUserId, Username = "user", FirstName = "user" }, cancellationToken);
        }

        if (user!.Role == GuideUserRole.Guest)
        {
            await _adminService.UpdateUserRoleAsync(user.Id, GuideUserRole.AuthorizedUser, cancellationToken);
            await botClient.SendMessage(chatId, "Вы успешно зарегистрированы! Теперь вам доступны личный кабинет, избранное и другие функции.", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendMessage(chatId, "Вы уже зарегистрированы.", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
        }
    }

    private async Task SendMainMenuAsync(ITelegramBotClient botClient, ChatId chatId, GuideUser? user, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId,
            "Добро пожаловать в Кызыл! Выбери действие:",
            replyMarkup: BuildMainMenuKeyboard(user),
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
        GuideUser? user,
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

        var likesCount = await _likeService.GetLikesCountAsync(attractionId, cancellationToken);
        var dislikesCount = await _likeService.GetDislikesCountAsync(attractionId, cancellationToken);
        var isFavorite = user is not null && await _favoriteService.IsFavoriteAsync(user.Id, attractionId, cancellationToken);

        var text = $"{attraction.Name}\n\n{attraction.ShortDescription}\n\n{attraction.FullDescription}\n\nАдрес: {attraction.Address}\n\nЛайков: {likesCount} | Дизлайков: {dislikesCount}";

        var keyboardRows = new List<InlineKeyboardButton[]>
        {
            new[]
            {
                InlineKeyboardButton.WithUrl("Открыть на карте", attraction.MapUrl)
            }
        };

        if (user is not null && user.Role >= GuideUserRole.AuthorizedUser)
        {
            var favButton = isFavorite
                ? InlineKeyboardButton.WithCallbackData("❤️ Убрать из избранного", GuideCallbackData.Create(GuideCallbackAction.RemoveFromFavorites, attractionId.ToString()))
                : InlineKeyboardButton.WithCallbackData("♡ В избранное", GuideCallbackData.Create(GuideCallbackAction.AddToFavorites, attractionId.ToString()));

            keyboardRows.Add(new[]
            {
                favButton
            });

            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("👍 Лайк", GuideCallbackData.Create(GuideCallbackAction.Like, attractionId.ToString())),
                InlineKeyboardButton.WithCallbackData("👎 Дизлайк", GuideCallbackData.Create(GuideCallbackAction.Dislike, attractionId.ToString()))
            });

            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("💬 Комментарии", GuideCallbackData.Create(GuideCallbackAction.ShowComments, attractionId.ToString())),
                InlineKeyboardButton.WithCallbackData("✏️ Оставить комментарий", GuideCallbackData.Create(GuideCallbackAction.AddComment, attractionId.ToString()))
            });
        }

        if (HasRole(user, GuideUserRole.Administrator))
        {
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("✏️ Редактировать место", GuideCallbackData.Create(GuideCallbackAction.EditAttraction, attractionId.ToString()))
            });
        }

        keyboardRows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("К категориям", GuideCallbackData.Create(GuideCallbackAction.ShowCategories)),
            InlineKeyboardButton.WithCallbackData("В меню", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
        });

        var keyboard = new InlineKeyboardMarkup(keyboardRows);

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

    private async Task ShowCommentsAsync(ITelegramBotClient botClient, ChatId chatId, Guid attractionId, CancellationToken cancellationToken)
    {
        var comments = await _commentService.GetApprovedCommentsAsync(attractionId, cancellationToken);
        var attraction = await _guideCatalogService.GetAttractionAsync(attractionId, cancellationToken);

        if (comments.Count == 0)
        {
            await botClient.SendMessage(
                chatId,
                $"Комментарии к \"{attraction?.Name ?? "месту"}\":\n\nПока нет комментариев.",
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("← К месту", GuideCallbackData.Create(GuideCallbackAction.ShowAttraction, attractionId.ToString())),
                        InlineKeyboardButton.WithCallbackData("✏️ Оставить", GuideCallbackData.Create(GuideCallbackAction.AddComment, attractionId.ToString()))
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("В меню", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                    }
                }),
                cancellationToken: cancellationToken);
            return;
        }

        var message = $"Комментарии к \"{attraction?.Name ?? "месту"}\":\n\n" +
            string.Join("\n\n", comments.Select(c => $"{c.UserName}: {c.Text}\n_{c.CreatedAt:dd.MM.yyyy HH:mm}_"));

        await botClient.SendMessage(
            chatId,
            message,
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("← К месту", GuideCallbackData.Create(GuideCallbackAction.ShowAttraction, attractionId.ToString())),
                    InlineKeyboardButton.WithCallbackData("✏️ Оставить", GuideCallbackData.Create(GuideCallbackAction.AddComment, attractionId.ToString()))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("В меню", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu))
                }
            }),
            cancellationToken: cancellationToken);
    }

    private async Task SendPendingCommentsAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var pendingComments = await _commentService.GetPendingCommentsAsync(cancellationToken);
        if (pendingComments.Count == 0)
        {
            await botClient.SendMessage(chatId, "Нет комментариев, ожидающих модерации.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        var message = "Комментарии на модерации:\n\n" +
            string.Join("\n\n", pendingComments.Select(c => $"{c.UserName}: {c.Text}\n_{c.CreatedAt:dd.MM.yyyy HH:mm}_"));

        var buttons = pendingComments.Select(c => new[]
        {
            InlineKeyboardButton.WithCallbackData($"✓ {c.UserName}", GuideCallbackData.Create(GuideCallbackAction.ApproveComment, c.Id.ToString())),
            InlineKeyboardButton.WithCallbackData($"✗", GuideCallbackData.Create(GuideCallbackAction.RejectComment, c.Id.ToString()))
        }).ToList();

        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.ShowAdminMenu)) });

        await botClient.SendMessage(chatId, message, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cancellationToken);
    }

    private async Task ShowUserManagementAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var users = await _adminService.GetAllUsersAsync(cancellationToken);
        var message = "Управление пользователями:\n\n" +
            string.Join("\n", users.Select(u => $"{u.TelegramUserName} — {u.Role}"));
        await botClient.SendMessage(chatId, message, replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
    }

    private async Task ShowRouteDetailsAsync(ITelegramBotClient botClient, ChatId chatId, Guid routeId, CancellationToken cancellationToken)
    {
        var route = await _routeService.GetRouteAsync(routeId, cancellationToken);
        if (route is null)
        {
            await botClient.SendMessage(chatId, "Маршрут не найден.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        var attractions = await _routeService.GetRouteAttractionsAsync(routeId, cancellationToken);
        var message = $"{route.Name}\n{route.Description}\n\nМеста в маршруте:\n" +
            string.Join("\n", attractions.Select((a, i) => $"{i + 1}. {a.Name}"));

        await botClient.SendMessage(chatId, message, replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
    }

    private async Task ShowThematicRoutesAsync(ITelegramBotClient botClient, ChatId chatId, CancellationToken cancellationToken)
    {
        var routes = await _routeService.GetThematicRoutesAsync(cancellationToken);
        if (routes.Count == 0)
        {
            await botClient.SendMessage(chatId, "Нет тематических маршрутов.", replyMarkup: BuildInlineBackToMainKeyboard(), cancellationToken: cancellationToken);
            return;
        }

        var buttons = routes.Select(r => new[] { InlineKeyboardButton.WithCallbackData(r.Name, GuideCallbackData.Create(GuideCallbackAction.ShowRoutes, r.Id.ToString())) }).ToList();
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Назад", GuideCallbackData.Create(GuideCallbackAction.BackToMainMenu)) });
        await botClient.SendMessage(chatId, "Тематические маршруты:", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: cancellationToken);
    }

    private static ReplyKeyboardMarkup BuildMainMenuKeyboard(GuideUser? user)
    {
        var buttons = new List<KeyboardButton[]>
        {
            new[] { new KeyboardButton(AttractionsButton), new KeyboardButton(MapButton) },
            new[] { new KeyboardButton(SearchButton), new KeyboardButton(AboutButton) }
        };

        if (user is null || user.Role == GuideUserRole.Guest)
        {
            buttons.Add(new[] { new KeyboardButton(RegisterButton) });
        }
        else
        {
            buttons.Add(new[] { new KeyboardButton(ProfileButton) });
        }

        return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
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
            buttons.Add(new[] { new KeyboardButton(CreateRouteButton) });
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
                    InlineKeyboardButton.WithCallbackData("Пользователи", GuideCallbackData.Create(GuideCallbackAction.ManageUsers))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Модерация комментариев", GuideCallbackData.Create(GuideCallbackAction.ShowComments))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Добавить место", GuideCallbackData.Create(GuideCallbackAction.AddAttraction)),
                    InlineKeyboardButton.WithCallbackData("События", GuideCallbackData.Create(GuideCallbackAction.ShowEvents))
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Добавить событие", GuideCallbackData.Create(GuideCallbackAction.AddEvent)),
                    InlineKeyboardButton.WithCallbackData("Рассылка", GuideCallbackData.Create(GuideCallbackAction.Broadcast))
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
        var chatId = message.Chat.Id;

        switch (state)
        {
            case ConversationState.AwaitingSearchQuery:
                _conversationStates.TryRemove(telegramUserId, out _);
                await SendSearchResultsAsync(botClient, chatId, text, cancellationToken);
                break;

            case ConversationState.AwaitingComment:
                if (_usersAwaitingComment.TryRemove(telegramUserId, out var attractionIdForComment) && user is not null)
                {
                    _conversationStates.TryRemove(telegramUserId, out _);
                    await _commentService.AddCommentAsync(user.Id, attractionIdForComment, text, user.TelegramUserName, cancellationToken);
                    await botClient.SendMessage(chatId, "Комментарий добавлен и ожидает модерации.", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                }
                break;

            case ConversationState.AwaitingRouteName:
                _routeNames[telegramUserId] = text;
                _conversationStates[telegramUserId] = ConversationState.AwaitingRouteDescription;
                await botClient.SendMessage(chatId, "Введите описание маршрута:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;

            case ConversationState.AwaitingRouteDescription:
                if (user is not null && _routeNames.TryGetValue(telegramUserId, out var routeName))
                {
                    _conversationStates.TryRemove(telegramUserId, out _);
                    _routeNames.TryRemove(telegramUserId, out _);
                    await _routeService.CreateRouteAsync(user.Id, routeName, text, false, cancellationToken);
                    await botClient.SendMessage(chatId, "Маршрут создан!", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                }
                break;

            case ConversationState.AwaitingNewPlaceName:
                _newPlaceNames[telegramUserId] = text;
                _conversationStates[telegramUserId] = ConversationState.AwaitingNewPlaceDescription;
                await botClient.SendMessage(chatId, "Введите краткое описание места:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;

            case ConversationState.AwaitingNewPlaceDescription:
                _newPlaceDescriptions[telegramUserId] = text;
                _conversationStates[telegramUserId] = ConversationState.AwaitingNewPlaceAddress;
                await botClient.SendMessage(chatId, "Введите адрес места:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;

            case ConversationState.AwaitingNewPlaceAddress:
                _newPlaceAddresses[telegramUserId] = text;
                if (_newPlaceNames.TryGetValue(telegramUserId, out var name) &&
                    _newPlaceDescriptions.TryGetValue(telegramUserId, out var description))
                {
                    var address = text;
                    _conversationStates.TryRemove(telegramUserId, out _);
                    _newPlaceNames.TryRemove(telegramUserId, out _);
                    _newPlaceDescriptions.TryRemove(telegramUserId, out _);
                    _newPlaceAddresses.TryRemove(telegramUserId, out _);

                    var categories = await _guideCatalogService.GetCategoriesAsync(cancellationToken);
                    var firstCategoryId = categories.FirstOrDefault()?.Id ?? Guid.Empty;

                    await _guideCatalogService.AddAttractionAsync(name, description, description, address, "", "", firstCategoryId, cancellationToken);
                    await botClient.SendMessage(chatId, $"Место \"{name}\" добавлено!", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                }
                break;

            case ConversationState.AwaitingEditPlace:
                if (user is not null && _editAttractionFields.TryGetValue(telegramUserId, out var editIdStr) &&
                    Guid.TryParse(editIdStr, out var editPlaceId))
                {
                    var attraction = await _guideCatalogService.GetAttractionAsync(editPlaceId, cancellationToken);
                    if (attraction is not null)
                    {
                        if (text != "-") attraction.Name = text;
                        attraction.ShortDescription = text != "-" ? $"Название изменено" : attraction.ShortDescription;
                        attraction.FullDescription = text;
                        await _guideCatalogService.UpdateAttractionAsync(attraction, cancellationToken);
                        await botClient.SendMessage(chatId, $"Место \"{attraction.Name}\" обновлено!", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                    }
                }
                _conversationStates.TryRemove(telegramUserId, out _);
                _editAttractionFields.TryRemove(telegramUserId, out _);
                break;

            case ConversationState.AwaitingEventName:
                _eventNames[telegramUserId] = text;
                _conversationStates[telegramUserId] = ConversationState.AwaitingEventDescription;
                await botClient.SendMessage(chatId, "Введите описание события:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;

            case ConversationState.AwaitingEventDescription:
                _eventDescriptions[telegramUserId] = text;
                _conversationStates[telegramUserId] = ConversationState.AwaitingEventLocation;
                await botClient.SendMessage(chatId, "Введите место проведения:", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                break;

            case ConversationState.AwaitingEventLocation:
                if (_eventNames.TryGetValue(telegramUserId, out var evtName) &&
                    _eventDescriptions.TryGetValue(telegramUserId, out var evtDesc))
                {
                    _conversationStates.TryRemove(telegramUserId, out _);
                    _eventNames.TryRemove(telegramUserId, out _);
                    _eventDescriptions.TryRemove(telegramUserId, out _);
                    await _eventService.CreateEventAsync(evtName, evtDesc, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), text, cancellationToken);
                    await botClient.SendMessage(chatId, $"Событие \"{evtName}\" создано!", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
                }
                break;

            case ConversationState.AwaitingNotificationMessage:
                _conversationStates.TryRemove(telegramUserId, out _);
                await _adminService.BroadcastNotificationAsync(text, cancellationToken);
                await botClient.SendMessage(chatId, "Уведомление отправлено всем пользователям.", replyMarkup: BuildBackKeyboard(), cancellationToken: cancellationToken);
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
        AwaitingNewPlaceAddress,
        AwaitingEditPlace,
        AwaitingEventName,
        AwaitingEventDescription,
        AwaitingEventLocation,
        AwaitingNotificationMessage
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
