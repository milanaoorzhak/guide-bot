using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface IAdminService
{
    Task<IReadOnlyList<GuideUser>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task UpdateUserRoleAsync(Guid userId, GuideUserRole newRole, CancellationToken cancellationToken);
    Task<Statistics> GetStatisticsAsync(CancellationToken cancellationToken);
    Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken);
    Task BroadcastNotificationAsync(string message, CancellationToken cancellationToken);
}

public class Statistics
{
    public int TotalUsers { get; set; }
    public int TotalAttractions { get; set; }
    public int TotalComments { get; set; }
    public int ActiveEvents { get; set; }
    public int PendingComments { get; set; }
}

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly ICommentService _commentService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;

    public AdminService(
        IAdminRepository adminRepository,
        ICommentService commentService,
        IEventService eventService,
        INotificationService notificationService)
    {
        _adminRepository = adminRepository;
        _commentService = commentService;
        _eventService = eventService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<GuideUser>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await _adminRepository.GetAllUsersAsync(cancellationToken);
    }

    public async Task UpdateUserRoleAsync(Guid userId, GuideUserRole newRole, CancellationToken cancellationToken)
    {
        await _adminRepository.UpdateUserRoleAsync(userId, newRole, cancellationToken);
    }

    public async Task<Statistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var totalUsers = await _adminRepository.GetTotalUsersCountAsync(cancellationToken);
        var totalAttractions = await _adminRepository.GetTotalAttractionsCountAsync(cancellationToken);
        var totalComments = await _adminRepository.GetTotalCommentsCountAsync(cancellationToken);
        var activeEvents = await _adminRepository.GetActiveEventsCountAsync(cancellationToken);
        var pendingComments = await _commentService.GetPendingCommentsAsync(cancellationToken);

        return new Statistics
        {
            TotalUsers = totalUsers,
            TotalAttractions = totalAttractions,
            TotalComments = totalComments,
            ActiveEvents = activeEvents,
            PendingComments = pendingComments.Count
        };
    }

    public async Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken)
    {
        await _notificationService.SendNotificationAsync(userId, message, cancellationToken);
    }

    public async Task BroadcastNotificationAsync(string message, CancellationToken cancellationToken)
    {
        var users = await _adminRepository.GetAllUsersAsync(cancellationToken);
        foreach (var user in users)
        {
            await _notificationService.SendNotificationAsync(user.Id, message, cancellationToken);
        }
    }
}