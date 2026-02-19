using System.Text.Json;
using GuideBot.DataAccess;

namespace GuideBot.Infrastructure.DataAccess;

public class FileUserRepository : IUserRepository
{
    private readonly string _baseFolder;

    public FileUserRepository(string baseFolder)
    {
        _baseFolder = baseFolder;
        if (!Directory.Exists(_baseFolder))
        {
            Directory.CreateDirectory(_baseFolder);
        }
    }

    public async Task AddAsync(ToDoUser user, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(user);
        await File.WriteAllTextAsync(Path.Combine(_baseFolder, $"{user.UserId}.json"), json);
    }

    public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken token)
    {
        ToDoUser? user = null;
        var fullPath = Path.Combine(_baseFolder, userId.ToString());
        if (File.Exists(fullPath))
        {
            string json = await File.ReadAllTextAsync(fullPath);
            user = JsonSerializer.Deserialize<ToDoUser>(json);
        }

        return user;
    }

    public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken token)
    {
        var users = new List<ToDoUser>();
        string[] files = Directory.GetFiles(Path.Combine(_baseFolder));
        foreach (string file in files)
        {
            string json = await File.ReadAllTextAsync(Path.Combine(_baseFolder, file));
            users.Add(JsonSerializer.Deserialize<ToDoUser>(json)!);
        }

        return users.FirstOrDefault(u => u.TelegramUserId == telegramUserId);
    }
}