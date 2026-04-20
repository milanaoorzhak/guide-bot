using GuideBot.DataAccess.Models;
using GuideBot.Entities;

namespace GuideBot.Infrastructure.DataAccess;

internal static class ModelMapper
{
    public static ToDoUser MapFromModel(ToDoUserModel model)
    {
        return new ToDoUser(model.TelegramUserId, model.TelegramUserName ?? string.Empty)
        {
            UserId = model.UserId,
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
            User = model.User != null ? MapFromModel(model.User) : null,
            Name = model.Name,
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
            UserId = entity.User?.UserId ?? Guid.Empty,
            Name = entity.Name,
            ListId = entity.List?.Id,
            CreatedAt = entity.CreatedAt,
            State = (int)entity.State,
            StateChangedAt = entity.StateChangedAt,
            Deadline = entity.Deadline
        };
    }

    public static ToDoList MapFromModel(ToDoListModel model)
    {
        var user = model.User != null ? MapFromModel(model.User) : null;
        return new ToDoList(user!, model.Name ?? string.Empty)
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt
        };
    }

    public static ToDoListModel MapToModel(ToDoList entity)
    {
        return new ToDoListModel
        {
            Id = entity.Id,
            UserId = entity.User?.UserId ?? Guid.Empty,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt
        };
    }
}
