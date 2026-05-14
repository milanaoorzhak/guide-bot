using GuideBot.DataAccess;
using GuideBot.Entities;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlCommentRepository : ICommentRepository
{
    private readonly IDataContextFactory<GuideDataContext> _factory;

    public SqlCommentRepository(IDataContextFactory<GuideDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<AttractionComment>> GetAttractionCommentsAsync(Guid attractionId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var comments = await dataContext.AttractionComments
            .Where(c => c.AttractionId == attractionId && c.IsApproved)
            .Join(dataContext.GuideUsers, c => c.UserId, u => u.Id, (c, u) => new { Comment = c, UserName = u.TelegramUserName })
            .OrderByDescending(x => x.Comment.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(x =>
        {
            var comment = GuideModelMapper.MapFromModel(x.Comment);
            comment.UserName = x.UserName;
            return comment;
        }).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AttractionComment>> GetPendingCommentsAsync(CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var comments = await dataContext.AttractionComments
            .Where(c => !c.IsApproved)
            .Join(dataContext.GuideUsers, c => c.UserId, u => u.Id, (c, u) => new { Comment = c, UserName = u.TelegramUserName })
            .OrderByDescending(x => x.Comment.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(x =>
        {
            var comment = GuideModelMapper.MapFromModel(x.Comment);
            comment.UserName = x.UserName;
            return comment;
        }).ToList().AsReadOnly();
    }

    public async Task AddAsync(AttractionComment comment, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.InsertAsync(GuideModelMapper.MapToModel(comment), token: cancellationToken);
    }

    public async Task ApproveAsync(Guid commentId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        var comment = await dataContext.AttractionComments
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment is not null)
        {
            comment.IsApproved = true;
            await dataContext.UpdateAsync(comment, token: cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid commentId, CancellationToken cancellationToken)
    {
        using var dataContext = _factory.CreateDataContext();
        await dataContext.AttractionComments
            .Where(c => c.Id == commentId)
            .DeleteAsync(cancellationToken);
    }
}