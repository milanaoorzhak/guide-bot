using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("Attraction")]
public class AttractionModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("CategoryId")]
    public Guid CategoryId { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("ShortDescription")]
    public string ShortDescription { get; set; } = string.Empty;

    [Column("FullDescription")]
    public string FullDescription { get; set; } = string.Empty;

    [Column("Address")]
    public string Address { get; set; } = string.Empty;

    [Column("PhotoUrl")]
    public string PhotoUrl { get; set; } = string.Empty;

    [Column("MapUrl")]
    public string MapUrl { get; set; } = string.Empty;

    [Association(ThisKey = nameof(CategoryId), OtherKey = nameof(AttractionCategoryModel.Id))]
    public AttractionCategoryModel? Category { get; set; }
}
