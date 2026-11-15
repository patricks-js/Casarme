using Casarme.Api.Casamentos;
using Casarme.Api.Convidados;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Data;

public class CasarmeDbContext(DbContextOptions<CasarmeDbContext> options) : DbContext(options)
{
    // Domain schema; the `auth` schema belongs to the authentication service.
    public const string Schema = "app";

    public DbSet<Casamento> Casamentos => Set<Casamento>();
    public DbSet<Noivo> Noivos => Set<Noivo>();
    public DbSet<Evento> Events => Set<Evento>();

    public DbSet<Grupo> Groups => Set<Grupo>();
    public DbSet<Convidado> Guests => Set<Convidado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.Entity<Grupo>().Ignore(x => x.DisplayName);
        modelBuilder.Entity<Grupo>().HasOne(x => x.Casamento).WithMany().HasForeignKey(x => x.CasamentoId);
        modelBuilder.Entity<Grupo>().HasIndex(x => new { x.CasamentoId, x.NameKey }).IsUnique();
        modelBuilder.Entity<Grupo>().Property(x => x.Name).HasMaxLength(200);
        modelBuilder.Entity<Grupo>().Property(x => x.NameKey).HasMaxLength(200);
        modelBuilder.Entity<Convidado>().HasOne(x => x.Grupo).WithMany(x => x.Members).HasForeignKey(x => x.GrupoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Convidado>().Property(x => x.Name).HasMaxLength(200);
        modelBuilder.Entity<Convidado>().Property(x => x.Email).HasMaxLength(254);
        modelBuilder.Entity<Convidado>().Property(x => x.Phone).HasMaxLength(16);
        modelBuilder.Entity<Casamento>().HasIndex(x => x.Identifier).IsUnique();
        modelBuilder.Entity<Casamento>().Property(x => x.Identifier).HasMaxLength(80);
        modelBuilder.Entity<Casamento>().Property(x => x.CoupleName).HasMaxLength(200);
        modelBuilder.Entity<Noivo>().HasIndex(x => x.AccountId).IsUnique();
        modelBuilder.Entity<Noivo>().HasOne(x => x.Casamento).WithMany(x => x.Noivos).HasForeignKey(x => x.CasamentoId);
        modelBuilder.Entity<Evento>().HasOne(x => x.Casamento).WithMany(x => x.Events).HasForeignKey(x => x.CasamentoId);
        modelBuilder.Entity<Evento>().HasIndex(x => x.CasamentoId).IsUnique().HasFilter("\"IsPrincipal\" = true");
        modelBuilder.Entity<Evento>().Property(x => x.StartsAt).HasColumnType("timestamp without time zone");
    }
}
