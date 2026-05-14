using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface ILikeRepository
{
    Task<int> GetLikesCountAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<int> GetDislikesCountAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<AttractionLike?> GetUserLikeAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
    Task SetLikeAsync(AttractionLike like, CancellationToken cancellationToken);
    Task DeleteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
}