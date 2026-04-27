using GuideBot.DataAccess;
using LinqToDB;

namespace GuideBot.Infrastructure.DataAccess;

public class SqlToDoRepository : IToDoRepository
{
    private readonly IDataContextFactory<ToDoDataContext> _factory;

    public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId)
            .ToListAsync(token);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId && i.State == (int)ToDoItemState.Active)
            .ToListAsync(token);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var item = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .FirstOrDefaultAsync(i => i.Id == id, token);

        return item != null ? ModelMapper.MapFromModel(item) : null;
    }

    public async Task AddAsync(ToDoItem item, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(item);
        await dbContext.InsertAsync(model);
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var model = ModelMapper.MapToModel(item);
        await dbContext.UpdateAsync(model);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        await dbContext.ToDoItems
            .Where(i => i.Id == id)
            .DeleteAsync(token);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        return await dbContext.ToDoItems
            .AnyAsync(i => i.UserId == userId && i.Name == name, token);
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        return await dbContext.ToDoItems
            .CountAsync(i => i.UserId == userId && i.State == (int)ToDoItemState.Active, token);
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId)
            .ToListAsync(token);

        return items.Select(ModelMapper.MapFromModel).Where(predicate).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndListAsync(Guid userId, Guid? listId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId && i.ListId == listId)
            .ToListAsync(token);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }

    public async Task DeleteByUserIdAndListAsync(Guid userId, Guid listId, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        await dbContext.ToDoItems
            .Where(i => i.UserId == userId && i.ListId == listId)
            .DeleteAsync(token);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken token)
    {
        using var dbContext = _factory.CreateDataContext();
        var items = await dbContext.ToDoItems
            .LoadWith(i => i.User)
            .LoadWith(i => i.List)
            .LoadWith(i => i.List!.User)
            .Where(i => i.UserId == userId && i.State == (int)ToDoItemState.Active && i.Deadline >= from && i.Deadline < to)
            .ToListAsync(token);

        return items.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
    }
}
