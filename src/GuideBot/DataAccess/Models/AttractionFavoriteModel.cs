using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("AttractionFavorite")]
public class AttractionFavoriteModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("AttractionId")]
    public Guid AttractionId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
