using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("AttractionComment")]
public class AttractionCommentModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("UserId")]
    public Guid UserId { get; set; }

    [Column("AttractionId")]
    public Guid AttractionId { get; set; }

    [Column("Text")]
    public string Text { get; set; } = string.Empty;

    [Column("IsApproved")]
    public bool IsApproved { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
