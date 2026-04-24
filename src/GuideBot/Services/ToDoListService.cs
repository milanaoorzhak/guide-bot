using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot;

public class ToDoListService : IToDoListService
{
    private readonly IToDoListRepository _toDoListRepository;

    public ToDoListService(IToDoListRepository toDoListRepository)
    {
        _toDoListRepository = toDoListRepository;
    }

    public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым");

        if (name.Length > 10)
            throw new ArgumentException("Размер имени списка не может быть больше 10 символом");

        if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
            throw new ArgumentException("Название списка должно быть уникально в рамках одного ToDoUser");

        var list = new ToDoList(user, name)
        {
            Id = Guid.NewGuid()
        };
        await _toDoListRepository.Add(list, ct);
        return list;
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        await _toDoListRepository.Delete(id, ct);
    }

    public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
    {
        return await _toDoListRepository.Get(id, ct);
    }

    public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
    {
        return await _toDoListRepository.GetByUserId(userId, ct);
    }
}
