using GuideBot.Entities;

namespace GuideBot.DataAccess;

public interface ICommentRepository
{
    Task<IReadOnlyList<AttractionComment>> GetAttractionCommentsAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttractionComment>> GetPendingCommentsAsync(CancellationToken cancellationToken);
    Task AddAsync(AttractionComment comment, CancellationToken cancellationToken);
    Task ApproveAsync(Guid commentId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid commentId, CancellationToken cancellationToken);
}