using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("AttractionCategory")]
public class AttractionCategoryModel
{
    [PrimaryKey]
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("SortOrder")]
    public int SortOrder { get; set; }
}
