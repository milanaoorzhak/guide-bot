namespace GuideBot.Entities;

public class AttractionLike
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public bool IsLike { get; set; }
    public DateTime CreatedAt { get; set; }

    public AttractionLike(Guid userId, Guid attractionId, bool isLike)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AttractionId = attractionId;
        IsLike = isLike;
        CreatedAt = DateTime.UtcNow;
    }
}