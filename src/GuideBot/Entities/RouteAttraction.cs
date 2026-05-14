namespace GuideBot.Entities;

public class RouteAttraction
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public Guid AttractionId { get; set; }
    public int SortOrder { get; set; }

    public RouteAttraction(Guid routeId, Guid attractionId, int sortOrder)
    {
        Id = Guid.NewGuid();
        RouteId = routeId;
        AttractionId = attractionId;
        SortOrder = sortOrder;
    }
}