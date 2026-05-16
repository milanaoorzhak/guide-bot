namespace GuideBot.TelegramBot.Dto;

public static class GuideCallbackAction
{
    public const string ShowCategories = "categories";
    public const string ShowCategoryPlaces = "category_places";
    public const string ShowAttraction = "attraction";
    public const string ShowSearchPrompt = "search_prompt";
    public const string BackToMainMenu = "main_menu";
    public const string ShowFavorites = "favorites";
    public const string ShowRoutes = "routes";
    public const string ShowThematicRoutes = "thematic_routes";
    public const string AddToRoute = "add_to_route";
    public const string SelectRoute = "select_route";
    public const string EditRoute = "edit_route";
    public const string DeleteRoute = "delete_route";
    public const string RemoveFromRoute = "remove_from_route";
    public const string ShowEvents = "events";
    public const string AddToFavorites = "add_fav";
    public const string RemoveFromFavorites = "remove_fav";
    public const string Like = "like";
    public const string Dislike = "dislike";
    public const string ShowComments = "comments";
    public const string AddComment = "add_comment";
    public const string ShowUserMenu = "user_menu";
    public const string ShowAdminMenu = "admin_menu";
    public const string ShowStatistics = "stats";
    public const string ManageUsers = "manage_users";
    public const string ApproveComment = "approve_comment";
    public const string RejectComment = "reject_comment";
    public const string AddAttraction = "add_attraction";
    public const string SelectCategoryForNewAttraction = "select_cat_new";
    public const string EditAttraction = "edit_attraction";
    public const string AddEvent = "add_event";
    public const string EditEvent = "edit_event";
    public const string Broadcast = "broadcast";
}

public class GuideCallbackData
{
    public string Action { get; }
    public string? Payload { get; }

    private GuideCallbackData(string action, string? payload)
    {
        Action = action;
        Payload = payload;
    }

    public static string Create(string action, string? payload = null)
    {
        return payload is null ? action : $"{action}|{payload}";
    }

    public static GuideCallbackData Parse(string input)
    {
        var parts = input.Split('|', 2, StringSplitOptions.TrimEntries);
        return new GuideCallbackData(parts[0], parts.Length > 1 ? parts[1] : null);
    }
}
