using Casarme.Api.Data;
using Casarme.Api.DTOs;
using Casarme.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Casarme.Api.Services;

public sealed class WeddingService(AppDbContext dbContext)
{
    public async Task<WeddingResponse> CreateWeddingAsync(CreateWeddingCommand command, CancellationToken ct = default)
    {
        var (partnerOneName, partnerTwoName) = Validate(command.PartnerOneName, command.PartnerTwoName, command.Slug, command.PrimaryDate);

        var hasWedding = await dbContext.Weddings.AnyAsync(x => x.OwnerId == command.OwnerId, ct);
        if (hasWedding)
            throw new InvalidOperationException("The user already has a wedding");

        var slugExists = await dbContext.Weddings.AnyAsync(x => x.Slug == command.Slug, ct);
        if (slugExists)
            throw new InvalidOperationException($"The slug '{command.Slug}' is already in use");

        var wedding = new Wedding(
            command.OwnerId,
            partnerOneName,
            partnerTwoName,
            command.Slug,
            command.PrimaryDate);

        dbContext.Weddings.Add(wedding);
        await SaveChangesAsync(command.Slug, ct);

        return ToResponse(wedding);
    }

    public async Task<WeddingResponse?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct = default)
    {
        var wedding = await dbContext.Weddings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == ownerId, ct);

        return wedding is null ? null : ToResponse(wedding);
    }

    public async Task<IReadOnlyCollection<WeddingResponse>> ListByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await dbContext.Weddings
            .AsNoTracking()
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.PrimaryDate)
            .Select(x => new WeddingResponse(x.Id, x.PartnerOneName, x.PartnerTwoName, x.Slug, x.PrimaryDate, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(ct);
    }

    /// <summary>A slug is available when it is well formed and not used by another owner's wedding.</summary>
    public async Task<SlugAvailabilityResponse> CheckSlugAvailabilityAsync(string slug, Guid ownerId, CancellationToken ct = default)
    {
        if (!WeddingRules.IsValidSlug(slug))
            throw new ArgumentException("Slug must be 3-60 lowercase letters, digits or hyphens", nameof(slug));

        var taken = await dbContext.Weddings.AnyAsync(x => x.Slug == slug && x.OwnerId != ownerId, ct);
        return new SlugAvailabilityResponse(slug, !taken);
    }

    public async Task UpdateAsync(UpdateWeddingCommand command, CancellationToken ct = default)
    {
        var (partnerOneName, partnerTwoName) = Validate(command.PartnerOneName, command.PartnerTwoName, command.Slug, command.PrimaryDate);

        var wedding = await dbContext.Weddings
            .FirstOrDefaultAsync(x => x.Id == command.WeddingId && x.OwnerId == command.OwnerId, ct);
        if (wedding is null)
            throw new KeyNotFoundException($"Wedding with id {command.WeddingId} not found");

        var slugConflict = await dbContext.Weddings
            .AnyAsync(x => x.Slug == command.Slug && x.Id != command.WeddingId, ct);
        if (slugConflict)
            throw new InvalidOperationException($"The slug '{command.Slug}' is already in use");

        wedding.Update(partnerOneName, partnerTwoName, command.Slug, command.PrimaryDate);
        await SaveChangesAsync(command.Slug, ct);
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

    private static (string PartnerOneName, string PartnerTwoName) Validate(
        string? partnerOneName, string? partnerTwoName, string? slug, DateOnly primaryDate)
    {
        var one = partnerOneName?.Trim();
        if (string.IsNullOrEmpty(one) || one.Length > WeddingRules.NameMaxLength)
            throw new ArgumentException("Partner one name is required (max 100 characters)", "PartnerOneName");

        var two = partnerTwoName?.Trim();
        if (string.IsNullOrEmpty(two) || two.Length > WeddingRules.NameMaxLength)
            throw new ArgumentException("Partner two name is required (max 100 characters)", "PartnerTwoName");

        if (!WeddingRules.IsValidSlug(slug))
            throw new ArgumentException("Slug must be 3-60 lowercase letters, digits or hyphens", "Slug");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (primaryDate <= today)
            throw new ArgumentOutOfRangeException("PrimaryDate", "Primary date must be a future date");

        return (one, two);
    }

    // The unique indexes are the source of truth when two requests race past the checks above
    private async Task SaveChangesAsync(string slug, CancellationToken ct)
    {
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw pg.ConstraintName == "IX_Weddings_OwnerId"
                ? new InvalidOperationException("The user already has a wedding", ex)
                : new InvalidOperationException($"The slug '{slug}' is already in use", ex);
        }
    }

    private static WeddingResponse ToResponse(Wedding x) =>
        new(x.Id, x.PartnerOneName, x.PartnerTwoName, x.Slug, x.PrimaryDate, x.CreatedAt, x.UpdatedAt);
}

public sealed record CreateWeddingCommand(Guid OwnerId, string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
public sealed record UpdateWeddingCommand(Guid WeddingId, Guid OwnerId, string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
