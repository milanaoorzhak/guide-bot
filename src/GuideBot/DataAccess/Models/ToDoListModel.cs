using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("ToDoList")]
public class ToDoListModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("Name")]
    public string? Name { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
    public ToDoUserModel? User { get; set; }
}
