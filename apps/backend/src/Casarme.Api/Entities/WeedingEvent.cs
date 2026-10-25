using Casarme.Api.Entities.Enums;

namespace Casarme.Api.Entities;

public sealed class WeddingEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid WeddingId { get; init; }
    public string Title { get; set; }
    public string Slug { get; private set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? MapsUrl { get; set; }
    public WeddingEventStatus Status { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset? RsvpDeadline { get; set; }

    public WeddingEvent(Guid weddingId, string title, string slug, string? description, string? location, string? mapsUrl, DateTimeOffset startsAt, DateTimeOffset endsAt, DateTimeOffset? rsvpDeadline)
    {
        WeddingId = weddingId;
        Title = title;
        Slug = slug;
        Description = description;
        Location = location;
        MapsUrl = mapsUrl;
        Status = WeddingEventStatus.Draft;
        StartsAt = startsAt;
        EndsAt = endsAt;
        RsvpDeadline = rsvpDeadline;
    }
}
