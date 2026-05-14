namespace GuideBot.Entities;

public class Route
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsThematic { get; set; }
    public DateTime CreatedAt { get; set; }

    public Route(Guid userId, string name, string description, bool isThematic = false)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name;
        Description = description;
        IsThematic = isThematic;
        CreatedAt = DateTime.UtcNow;
    }
}