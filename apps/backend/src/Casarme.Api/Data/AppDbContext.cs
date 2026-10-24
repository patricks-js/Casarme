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
    }
}