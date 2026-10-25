namespace Casarme.Api.DTOs;

public sealed record WeddingResponse(
    Guid Id,
    string PartnerOneName,
    string PartnerTwoName,
    string Slug,
    DateOnly PrimaryDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SlugAvailabilityResponse(string Slug, bool Available);
