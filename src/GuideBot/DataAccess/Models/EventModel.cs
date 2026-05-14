using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("Event")]
public class EventModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("StartDate")]
    public DateTime StartDate { get; set; }

    [Column("EndDate")]
    public DateTime? EndDate { get; set; }

    [Column("Location")]
    public string? Location { get; set; }

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
