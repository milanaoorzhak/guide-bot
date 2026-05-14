using GuideBot.DataAccess.Models;
using GuideBot.Entities;

namespace GuideBot.Infrastructure.DataAccess;

internal static class GuideModelMapper
{
    public static GuideUser MapFromModel(GuideUserModel model)
    {
        return new GuideUser(model.TelegramUserId, model.TelegramUserName, (GuideUserRole)model.Role)
        {
            Id = model.Id,
            RegisteredAt = model.RegisteredAt
        };
    }

    public static GuideUserModel MapToModel(GuideUser entity)
    {
        return new GuideUserModel
        {
            Id = entity.Id,
            TelegramUserId = entity.TelegramUserId,
            TelegramUserName = entity.TelegramUserName,
            Role = (int)entity.Role,
            RegisteredAt = entity.RegisteredAt
        };
    }

    public static AttractionCategory MapFromModel(AttractionCategoryModel model)
    {
        return new AttractionCategory
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            SortOrder = model.SortOrder
        };
    }

    public static Attraction MapFromModel(AttractionModel model)
    {
        return new Attraction
        {
            Id = model.Id,
            CategoryId = model.CategoryId,
            Name = model.Name,
            ShortDescription = model.ShortDescription,
            FullDescription = model.FullDescription,
            Address = model.Address,
            PhotoUrl = model.PhotoUrl,
            MapUrl = model.MapUrl
        };
    }

    public static CityInfo MapFromModel(CityInfoModel model)
    {
        return new CityInfo
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            MapUrl = model.MapUrl
        };
    }

    public static AttractionFavorite MapFromModel(AttractionFavoriteModel model)
    {
        return new AttractionFavorite(model.UserId, model.AttractionId)
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt
        };
    }

    public static AttractionFavoriteModel MapToModel(AttractionFavorite entity)
    {
        return new AttractionFavoriteModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AttractionId = entity.AttractionId,
            CreatedAt = entity.CreatedAt
        };
    }

    public static AttractionLike MapFromModel(AttractionLikeModel model)
    {
        return new AttractionLike(model.UserId, model.AttractionId, model.IsLike)
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt
        };
    }

    public static AttractionLikeModel MapToModel(AttractionLike entity)
    {
        return new AttractionLikeModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AttractionId = entity.AttractionId,
            IsLike = entity.IsLike,
            CreatedAt = entity.CreatedAt
        };
    }

    public static AttractionComment MapFromModel(AttractionCommentModel model)
    {
        return new AttractionComment(model.UserId, model.AttractionId, model.Text)
        {
            Id = model.Id,
            IsApproved = model.IsApproved,
            CreatedAt = model.CreatedAt
        };
    }

    public static AttractionCommentModel MapToModel(AttractionComment entity)
    {
        return new AttractionCommentModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AttractionId = entity.AttractionId,
            Text = entity.Text,
            IsApproved = entity.IsApproved,
            CreatedAt = entity.CreatedAt
        };
    }

    public static Route MapFromModel(RouteModel model)
    {
        return new Route(model.UserId, model.Name, model.Description ?? "", model.IsThematic)
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt
        };
    }

    public static RouteModel MapToModel(Route entity)
    {
        return new RouteModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            Description = entity.Description,
            IsThematic = entity.IsThematic,
            CreatedAt = entity.CreatedAt
        };
    }

    public static GuideEvent MapFromModel(EventModel model)
    {
        return new GuideEvent(model.Name, model.Description, model.StartDate, model.EndDate, model.Location ?? "")
        {
            Id = model.Id,
            IsActive = model.IsActive,
            CreatedAt = model.CreatedAt
        };
    }

    public static EventModel MapToModel(GuideEvent entity)
    {
        return new EventModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Location = entity.Location,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt
        };
    }

    public static UserNotification MapFromModel(NotificationModel model)
    {
        return new UserNotification(model.UserId, model.Message)
        {
            Id = model.Id,
            IsRead = model.IsRead,
            CreatedAt = model.CreatedAt
        };
    }

    public static NotificationModel MapToModel(UserNotification entity)
    {
        return new NotificationModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Message = entity.Message,
            IsRead = entity.IsRead,
            CreatedAt = entity.CreatedAt
        };
    }
}
