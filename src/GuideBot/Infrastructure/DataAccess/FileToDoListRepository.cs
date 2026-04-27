using System.Text.Json;
using GuideBot.DataAccess;
using GuideBot.Entities;

namespace GuideBot.Infrastructure.DataAccess;

public class FileToDoListRepository : IToDoListRepository
{
    private readonly string _baseFolder;

    public FileToDoListRepository(string baseFolder)
    {
        _baseFolder = baseFolder;
        if (!Directory.Exists(_baseFolder))
        {
            Directory.CreateDirectory(_baseFolder);
        }
    }

    public async Task Add(ToDoList list, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(list);
        await File.WriteAllTextAsync(Path.Combine(_baseFolder, $"{list.Id}.json"), json, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        var fullPath = Path.Combine(_baseFolder, $"{id}.json");
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
    {
        var userLists = await GetByUserId(userId, ct);
        return userLists.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
    {
        ToDoList? list = null;
        var fullPath = Path.Combine(_baseFolder, $"{id}.json");
        if (File.Exists(fullPath))
        {
            string json = await File.ReadAllTextAsync(fullPath, ct);
            list = JsonSerializer.Deserialize<ToDoList>(json);
        }

        return list;
    }

    public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
    {
        var toDoLists = new List<ToDoList>();
        string[] files = Directory.GetFiles(_baseFolder);
        foreach (string file in files)
        {
            string json = await File.ReadAllTextAsync(Path.Combine(_baseFolder, file), ct);
            var list = JsonSerializer.Deserialize<ToDoList>(json);
            if (list != null && list.User.UserId == userId)
            {
                toDoLists.Add(list);
            }
        }

        return toDoLists;
    }
}
