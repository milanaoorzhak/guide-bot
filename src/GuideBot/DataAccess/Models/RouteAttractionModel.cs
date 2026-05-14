using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("RouteAttraction")]
public class RouteAttractionModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("RouteId")]
    public Guid RouteId { get; set; }

    [Column("AttractionId")]
    public Guid AttractionId { get; set; }

    [Column("SortOrder")]
    public int SortOrder { get; set; }
}
