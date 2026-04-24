using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("ToDoItem")]
public class ToDoItemModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("ListId")]
    public Guid? ListId { get; set; }

    [Column("Name")]
    public string? Name { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("State")]
    public int State { get; set; }

    [Column("StateChangedAt")]
    public DateTime? StateChangedAt { get; set; }

    [Column("Deadline")]
    public DateTime Deadline { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
    public ToDoUserModel? User { get; set; }

    [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
    public ToDoListModel? List { get; set; }
}
