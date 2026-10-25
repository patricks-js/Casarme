using Casarme.Api.Data;
using Casarme.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Services;

public sealed class WeddingEventService(AppDbContext db)
{
    public async Task<Guid> CreateWeddingEventAsync(CreateWeddingEventCommand command, CancellationToken ct = default)
    {
        var slugExists = await db.WeddingsEvents.AnyAsync(x => x.Slug == command.Slug, ct);
        if (slugExists)
            throw new InvalidOperationException($"The slug '{command.Slug}' is already in use");

        var weddingEvent = new WeddingEvent(
            command.WeddingId,
            command.Title,
            command.Slug,
            command.Description,
            command.Location,
            command.MapsUrl,
            command.StartsAt,
            command.EndsAt,
            command.RsvpDeadline);

        db.WeddingsEvents.Add(weddingEvent);
        await db.SaveChangesAsync(ct);

        return weddingEvent.WeddingId;
    }
}

public sealed record CreateWeddingEventCommand(
    Guid WeddingId,
    string Title,
    string Slug,
    string? Description,
    string? Location,
    string? MapsUrl,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset? RsvpDeadline);
