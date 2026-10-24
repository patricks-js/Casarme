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
    public WeddingEventStatus Status { get; set; } = WeddingEventStatus.Draft;
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public DateTimeOffset RsvpDeadline { get; set; }
}