using Casarme.Api.Data;
using Casarme.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Services;

public sealed class WeddingService(AppDbContext dbContext)
{
    public async Task<Guid> CreateWeddingAsync(CreateWeddingCommand command, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.PrimaryDate <= today)
            throw new ArgumentOutOfRangeException(nameof(command.PrimaryDate), "Primary date must be a future date");

        var slugExists = await dbContext.Weddings.AnyAsync(x => x.Slug == command.Slug, ct);
        if (slugExists)
            throw new InvalidOperationException($"The slug '{command.Slug}' is already in use");

        var wedding = new Wedding(
            command.OwnerId,
            command.PartnerOneName,
            command.PartnerTwoName,
            command.Slug,
            command.PrimaryDate);

        dbContext.Weddings.Add(wedding);
        await dbContext.SaveChangesAsync(ct);

        return wedding.Id;
    }

    public async Task<object?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct = default)
    {
        return await dbContext.Weddings
            .AsNoTracking()
            .Where(x => x.Id == id && x.OwnerId == ownerId)
            .Select(x => new { x.Id, x.PartnerOneName, x.PartnerTwoName, x.Slug, x.PrimaryDate })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<object>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await dbContext.Weddings
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.PrimaryDate)
            .Select(x => new { x.Id, x.PartnerOneName, x.PartnerTwoName, x.Slug, x.PrimaryDate })
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(UpdateWeddingCommand command, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.PrimaryDate <= today)
            throw new ArgumentOutOfRangeException(nameof(command.PrimaryDate), "Primary date must be a future date");

        var wedding = await dbContext.Weddings
            .FirstOrDefaultAsync(x => x.Id == command.WeddingId && x.OwnerId == command.OwnerId, ct);
        if (wedding is null)
            throw new KeyNotFoundException($"Wedding with id {command.WeddingId} not found");

        var slugConflict = await dbContext.Weddings
            .AnyAsync(x => x.Slug == command.Slug && x.Id != command.WeddingId, ct);
        if (slugConflict)
            throw new InvalidOperationException($"The slug '{command.Slug}' is already in use");

        wedding.Update(command.PartnerOneName, command.PartnerTwoName, command.Slug, command.PrimaryDate);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid ownerId, CancellationToken ct = default)
    {
        var wedding = await dbContext.Weddings
            .FirstOrDefaultAsync(w => w.Id == id && w.OwnerId == ownerId, ct);

        if (wedding is null)
            throw new KeyNotFoundException("Wedding not found or access denied.");

        dbContext.Weddings.Remove(wedding);
        await dbContext.SaveChangesAsync(ct);
    }
}

public sealed record CreateWeddingCommand(Guid OwnerId, string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
public sealed record UpdateWeddingCommand(Guid WeddingId, Guid OwnerId, string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
