using GuideBot.DataAccess.Models;
using LinqToDB;
using LinqToDB.Data;

namespace GuideBot.Infrastructure.DataAccess;

public class GuideDataContext : DataConnection
{
    public ITable<GuideUserModel> GuideUsers => this.GetTable<GuideUserModel>();
    public ITable<AttractionCategoryModel> AttractionCategories => this.GetTable<AttractionCategoryModel>();
    public ITable<AttractionModel> Attractions => this.GetTable<AttractionModel>();
    public ITable<CityInfoModel> CityInfos => this.GetTable<CityInfoModel>();
    public ITable<AttractionFavoriteModel> AttractionFavorites => this.GetTable<AttractionFavoriteModel>();
    public ITable<AttractionLikeModel> AttractionLikes => this.GetTable<AttractionLikeModel>();
    public ITable<AttractionCommentModel> AttractionComments => this.GetTable<AttractionCommentModel>();
    public ITable<RouteModel> Routes => this.GetTable<RouteModel>();
    public ITable<RouteAttractionModel> RouteAttractions => this.GetTable<RouteAttractionModel>();
    public ITable<EventModel> Events => this.GetTable<EventModel>();
    public ITable<NotificationModel> Notifications => this.GetTable<NotificationModel>();

    public GuideDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString)
    {
    }
}
