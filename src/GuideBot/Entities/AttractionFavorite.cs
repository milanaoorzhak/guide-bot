namespace GuideBot.Entities;

public class AttractionFavorite
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public DateTime CreatedAt { get; set; }

    public AttractionFavorite(Guid userId, Guid attractionId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AttractionId = attractionId;
        CreatedAt = DateTime.UtcNow;
    }
}