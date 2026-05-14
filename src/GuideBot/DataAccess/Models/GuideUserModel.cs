using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("GuideUser")]
public class GuideUserModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("TelegramUserId")]
    public long TelegramUserId { get; set; }

    [Column("TelegramUserName")]
    public string TelegramUserName { get; set; } = string.Empty;

    [Column("Role")]
    public int Role { get; set; }

    [Column("RegisteredAt")]
    public DateTime RegisteredAt { get; set; }
}
