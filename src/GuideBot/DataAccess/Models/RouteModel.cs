using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("Route")]
public class RouteModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    public string? Description { get; set; }

    [Column("IsThematic")]
    public bool IsThematic { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
