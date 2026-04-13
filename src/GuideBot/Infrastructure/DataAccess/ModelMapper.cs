using GuideBot.DataAccess.Models;
using GuideBot.Entities;

namespace GuideBot.Infrastructure.DataAccess;

internal static class ModelMapper
{
    public static ToDoUser MapFromModel(ToDoUserModel model)
    {
        return new ToDoUser
        {
            UserId = model.UserId,
            TelegramUserId = model.TelegramUserId,
            TelegramUserName = model.TelegramUserName,
            RegisteredAt = model.RegisteredAt
        };
    }

    public static ToDoUserModel MapToModel(ToDoUser entity)
    {
        return new ToDoUserModel
        {
            UserId = entity.UserId,
            TelegramUserId = entity.TelegramUserId,
            TelegramUserName = entity.TelegramUserName,
            RegisteredAt = entity.RegisteredAt
        };
    }

    public static ToDoItem MapFromModel(ToDoItemModel model)
    {
        return new ToDoItem
        {
            Id = model.Id,
            UserId = model.UserId,
            User = model.User != null ? MapFromModel(model.User) : null,
            Name = model.Name,
            ListId = model.ListId,
            List = model.List != null ? MapFromModel(model.List) : null,
            CreatedAt = model.CreatedAt,
            State = (ToDoItemState)model.State,
            StateChangedAt = model.StateChangedAt,
            Deadline = model.Deadline
        };
    }

    public static ToDoItemModel MapToModel(ToDoItem entity)
    {
        return new ToDoItemModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            ListId = entity.ListId,
            CreatedAt = entity.CreatedAt,
            State = (int)entity.State,
            StateChangedAt = entity.StateChangedAt,
            Deadline = entity.Deadline
        };
    }

    public static ToDoList MapFromModel(ToDoListModel model)
    {
        return new ToDoList
        {
            Id = model.Id,
            UserId = model.UserId,
            Name = model.Name,
            User = model.User != null ? MapFromModel(model.User) : null,
            CreatedAt = model.CreatedAt
        };
    }

    public static ToDoListModel MapToModel(ToDoList entity)
    {
        return new ToDoListModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt
        };
    }
}
