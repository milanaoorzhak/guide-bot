namespace GuideBot.Entities;

public class GuideEvent
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public GuideEvent(string name, string description, DateTime startDate, DateTime? endDate, string location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
}