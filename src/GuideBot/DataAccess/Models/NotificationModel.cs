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

    [Column("Type")]
    public string? Type { get; set; }

    [Column("Text")]
    public string? Text { get; set; }

    [Column("ScheduledAt")]
    public DateTime ScheduledAt { get; set; }

    [Column("IsNotified")]
    public bool IsNotified { get; set; }

    [Column("NotifiedAt")]
    public DateTime? NotifiedAt { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
    public ToDoUserModel? User { get; set; }
}