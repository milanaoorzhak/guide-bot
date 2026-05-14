using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("Notification")]
public class NotificationModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("Message")]
    public string Message { get; set; } = string.Empty;

    [Column("IsRead")]
    public bool IsRead { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
