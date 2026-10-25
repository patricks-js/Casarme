using Casarme.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Wedding> Weddings { get; set; }
    public DbSet<WeddingEvent> WeddingsEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wedding>(wedding =>
        {
            wedding.Property(x => x.PartnerOneName).HasMaxLength(WeddingRules.NameMaxLength);
            wedding.Property(x => x.PartnerTwoName).HasMaxLength(WeddingRules.NameMaxLength);
            wedding.Property(x => x.Slug).HasMaxLength(WeddingRules.SlugMaxLength);

            // BR-01, enforced by the database so concurrent requests can't both win
            wedding.HasIndex(x => x.Slug).IsUnique();
            // MVP: one wedding per owner
            wedding.HasIndex(x => x.OwnerId).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
