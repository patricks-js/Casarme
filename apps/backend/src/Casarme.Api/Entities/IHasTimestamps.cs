namespace Casarme.Api.Entities;

/// <summary>Filled automatically by <see cref="Data.AppDbContext"/> on save (NFR-03).</summary>
public interface IHasTimestamps
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
