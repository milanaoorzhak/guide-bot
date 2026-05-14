using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Services;

public interface ILikeService
{
    Task<int> GetLikesCountAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<int> GetDislikesCountAsync(Guid attractionId, CancellationToken cancellationToken);
    Task<bool?> GetUserReactionAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
    Task SetReactionAsync(Guid userId, Guid attractionId, bool isLike, CancellationToken cancellationToken);
    Task RemoveReactionAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken);
}

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;

    public LikeService(ILikeRepository likeRepository)
    {
        _likeRepository = likeRepository;
    }

    public async Task<int> GetLikesCountAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        return await _likeRepository.GetLikesCountAsync(attractionId, cancellationToken);
    }

    public async Task<int> GetDislikesCountAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        return await _likeRepository.GetDislikesCountAsync(attractionId, cancellationToken);
    }

    public async Task<bool?> GetUserReactionAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        var like = await _likeRepository.GetUserLikeAsync(userId, attractionId, cancellationToken);
        return like?.IsLike;
    }

    public async Task SetReactionAsync(Guid userId, Guid attractionId, bool isLike, CancellationToken cancellationToken)
    {
        var like = new AttractionLike(userId, attractionId, isLike);
        await _likeRepository.SetLikeAsync(like, cancellationToken);
    }

    public async Task RemoveReactionAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        await _likeRepository.DeleteAsync(userId, attractionId, cancellationToken);
    }
}