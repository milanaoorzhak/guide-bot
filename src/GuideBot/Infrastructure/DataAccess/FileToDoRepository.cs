
using System.Text.Json;
using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class FileToDoRepository : IToDoRepository
{
    private readonly string _baseFolder;
    private readonly string _toDoItemIndexFile = $"toDoItemIndexFile.json";

    public FileToDoRepository(string baseFolder)
    {
        _baseFolder = baseFolder;
        if (!Directory.Exists(_baseFolder))
            Directory.CreateDirectory(_baseFolder);
    }

    private async Task BuildIndexFileAsync()
    {
        var indexPath = Path.Combine(_baseFolder, _toDoItemIndexFile);
        if (!File.Exists(indexPath))
        {
            Dictionary<string, string> indexItems = new();
            string[] folders = Directory.GetDirectories(_baseFolder);

            foreach (string folder in folders)
            {
                string[] files = Directory.GetFiles(folder);
                foreach (string file in files)
                {
                    indexItems.Add(Path.GetFileName(file), Path.GetDirectoryName(folder)!);
                }
            }

            var json = JsonSerializer.Serialize(indexItems);
            await File.WriteAllTextAsync(Path.Combine(_baseFolder, _toDoItemIndexFile), json);
        }
    }

    public async Task AddAsync(ToDoItem item, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var fullPath = Path.Combine(_baseFolder, $"{item.User.UserId}");

        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var json = JsonSerializer.Serialize(item);
        await File.WriteAllTextAsync(Path.Combine(fullPath, $"{item.Id}.json"), json);

        await AddIndexToToDoItemIndexFileAsync(item.Id, item.User.UserId);
    }

    public async Task<int> CountActiveAsync(Guid userId, CancellationToken token)
    {
        await BuildIndexFileAsync();
        return (await GetActiveByUserIdAsync(userId, token)).Count();
    }

    public async Task DeleteAsync(Guid id, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var parentFolder = await GetParentFolderNameAsync(id);
        var fullPath = Path.Combine(_baseFolder, parentFolder, $"{id}.json");

        if (File.Exists(fullPath))
            File.Delete(fullPath);
        else
            throw new FileNotFoundException($"File with id {id} not found");

        await DeleteIndexFromToDoItemIndexFileAsync(id);
    }

    public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var userTasks = await GetAllByUserIdAsync(userId, token);
        foreach (var t in userTasks)
        {
            if (name.Equals(t.Name)) throw new DuplicateTaskException(name);
        }

        return false;
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var items = await GetAllByUserIdAsync(userId, token);
        return items
                .Where(predicate)
                .ToList()
                .AsReadOnly();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var items = await GetAllByUserIdAsync(userId, token);
        return items.Where(t => t.State == ToDoItemState.Active).ToList();
    }

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken token)
    {
        await BuildIndexFileAsync();

        List<ToDoItem> toDoItems = new();
        var fullPath = Path.Combine(_baseFolder, $"{userId}");
        if (Directory.Exists(fullPath))
        {
            string[] files = Directory.GetFiles(fullPath);
            foreach (string file in files)
            {
                string json = await File.ReadAllTextAsync(Path.Combine(fullPath, file));
                toDoItems.Add(JsonSerializer.Deserialize<ToDoItem>(json)!);
            }
        }

        return toDoItems;
    }

    public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken token)
    {
        await BuildIndexFileAsync();

        ToDoItem? item = null;

        var parentFolder = await GetParentFolderNameAsync(id);
        var fullPath = Path.Combine(_baseFolder, parentFolder, $"{id}.json");
        if (File.Exists(fullPath))
        {
            string json = await File.ReadAllTextAsync(fullPath);
            item = JsonSerializer.Deserialize<ToDoItem>(json);
        }

        return item;
    }

    public async Task UpdateAsync(ToDoItem item, CancellationToken token)
    {
        await BuildIndexFileAsync();

        var fullPath = Path.Combine(_baseFolder, $"{item.User.UserId}", $"{item.Id}.json");
        if (File.Exists(fullPath))
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(fullPath, json);
        }
    }

    private async Task AddIndexToToDoItemIndexFileAsync(Guid ToDoItemId, Guid userId)
    {
        var index = await GetToDoItemIndexesAsync() ?? new Dictionary<string, string>();

        index[ToDoItemId.ToString()] = userId.ToString();

        var json = JsonSerializer.Serialize(index);
        await File.WriteAllTextAsync(Path.Combine(_baseFolder, _toDoItemIndexFile), json);
    }

    private async Task DeleteIndexFromToDoItemIndexFileAsync(Guid ToDoItemId)
    {
        var index = await GetToDoItemIndexesAsync();
        if (index != null)
        {
            index.Remove(ToDoItemId.ToString());

            var json = JsonSerializer.Serialize(index);
            await File.WriteAllTextAsync(Path.Combine(_baseFolder, _toDoItemIndexFile), json);
        }
    }

    private async Task<Dictionary<string, string>?> GetToDoItemIndexesAsync()
    {
        string json = await File.ReadAllTextAsync(Path.Combine(_baseFolder, _toDoItemIndexFile));
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    private async Task<string> GetParentFolderNameAsync(Guid id)
    {
        var indexItems = await GetToDoItemIndexesAsync();
        return indexItems is not null ? indexItems!.FirstOrDefault(i => i.Key == id.ToString()).Value : throw new NullReferenceException("Indexes is null");
    }
}