using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface IAdminRepository
{
    Task<IReadOnlyList<GuideUser>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task UpdateUserRoleAsync(Guid userId, GuideUserRole newRole, CancellationToken cancellationToken);
    Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken);
    Task<int> GetTotalAttractionsCountAsync(CancellationToken cancellationToken);
    Task<int> GetTotalCommentsCountAsync(CancellationToken cancellationToken);
    Task<int> GetActiveEventsCountAsync(CancellationToken cancellationToken);
}