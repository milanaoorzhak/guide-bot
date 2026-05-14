using LinqToDB.Mapping;

namespace GuideBot.DataAccess.Models;

[Table("CityInfo")]
public class CityInfoModel
{
    [PrimaryKey]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("MapUrl")]
    public string MapUrl { get; set; } = string.Empty;
}
