using Casarme.Api.Casamentos;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Data;

public class CasarmeDbContext(DbContextOptions<CasarmeDbContext> options) : DbContext(options)
{
    // Domain schema; the `auth` schema belongs to the authentication service.
    public const string Schema = "app";

    public DbSet<Casamento> Casamentos => Set<Casamento>();
    public DbSet<Noivo> Noivos => Set<Noivo>();
    public DbSet<Evento> Events => Set<Evento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<Casamento>().HasIndex(x => x.Identifier).IsUnique();
        modelBuilder.Entity<Casamento>().Property(x => x.Identifier).HasMaxLength(80);
        modelBuilder.Entity<Casamento>().Property(x => x.CoupleName).HasMaxLength(200);
        modelBuilder.Entity<Noivo>().HasIndex(x => x.AccountId).IsUnique();
        modelBuilder.Entity<Noivo>().HasOne(x => x.Casamento).WithMany(x => x.Noivos).HasForeignKey(x => x.CasamentoId);
        modelBuilder.Entity<Evento>().HasOne(x => x.Casamento).WithMany(x => x.Events).HasForeignKey(x => x.CasamentoId);
        modelBuilder.Entity<Evento>().HasIndex(x => x.CasamentoId).IsUnique().HasFilter("\"IsPrincipal\" = true");
        modelBuilder.Entity<Evento>().Property(x => x.Name).HasMaxLength(200);
        modelBuilder.Entity<Evento>().Property(x => x.Location).HasMaxLength(500);
        modelBuilder.Entity<Evento>().Property(x => x.RsvpDeadline).HasColumnType("timestamp without time zone");
        modelBuilder.Entity<Evento>().Property(x => x.StartsAt).HasColumnType("timestamp without time zone");
    }
}
