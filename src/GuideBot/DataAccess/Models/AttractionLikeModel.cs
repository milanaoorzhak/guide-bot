using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("AttractionLike")]
public class AttractionLikeModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("AttractionId")]
    public Guid AttractionId { get; set; }

    [Column("IsLike")]
    public bool IsLike { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
