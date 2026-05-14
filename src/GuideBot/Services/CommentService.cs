using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface ICommentService
{
    Task<IReadOnlyList<AttractionComment>> GetApprovedCommentsAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttractionComment>> GetPendingCommentsAsync(CancellationToken cancellationToken);
    Task AddCommentAsync(Guid userId, Guid attractionId, string text, string userName, CancellationToken cancellationToken);
    Task ApproveCommentAsync(Guid commentId, CancellationToken cancellationToken);
    Task RejectCommentAsync(Guid commentId, CancellationToken cancellationToken);
}

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<AttractionComment>> GetApprovedCommentsAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        return await _commentRepository.GetAttractionCommentsAsync(attractionId, cancellationToken);
    }

    public async Task<IReadOnlyList<AttractionComment>> GetPendingCommentsAsync(CancellationToken cancellationToken)
    {
        return await _commentRepository.GetPendingCommentsAsync(cancellationToken);
    }

    public async Task AddCommentAsync(Guid userId, Guid attractionId, string text, string userName, CancellationToken cancellationToken)
    {
        var comment = new AttractionComment(userId, attractionId, text);
        comment.UserName = userName;
        await _commentRepository.AddAsync(comment, cancellationToken);
    }

    public async Task ApproveCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        await _commentRepository.ApproveAsync(commentId, cancellationToken);
    }

    public async Task RejectCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        await _commentRepository.DeleteAsync(commentId, cancellationToken);
    }
}