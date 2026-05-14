using GuideBot.DataAccess;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlLikeRepository : ILikeRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlLikeRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<int> GetLikesCountAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.AttractionLikes
            .CountAsync(l => l.AttractionId == attractionId && l.IsLike, cancellationToken);
    }

    public async Task<int> GetDislikesCountAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        return await dataContext.AttractionLikes
            .CountAsync(l => l.AttractionId == attractionId && !l.IsLike, cancellationToken);
    }

    public async Task<AttractionLike?> GetUserLikeAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var like = await dataContext.AttractionLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.AttractionId == attractionId, cancellationToken);

        return like is null ? null : GuideModelMapper.MapFromModel(like);
    }

    public async Task SetLikeAsync(AttractionLike like, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var existing = await dataContext.AttractionLikes
            .FirstOrDefaultAsync(l => l.UserId == like.UserId && l.AttractionId == like.AttractionId, cancellationToken);

        if (existing is not null)
        {
            existing.IsLike = like.IsLike;
            existing.CreatedAt = like.CreatedAt;
            await dataContext.UpdateAsync(existing, token: cancellationToken);
        }
        else
        {
            await dataContext.InsertAsync(GuideModelMapper.MapToModel(like), token: cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid userId, Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.AttractionLikes
            .Where(l => l.UserId == userId && l.AttractionId == attractionId)
            .DeleteAsync(cancellationToken);
    }
}