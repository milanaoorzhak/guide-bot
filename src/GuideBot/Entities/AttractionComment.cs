namespace GuideBot.Entities;

public class AttractionComment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public string Text { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }

    public string UserName { get; set; }

    public AttractionComment(Guid userId, Guid attractionId, string text)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AttractionId = attractionId;
        Text = text;
        IsApproved = false;
        CreatedAt = DateTime.UtcNow;
    }
}