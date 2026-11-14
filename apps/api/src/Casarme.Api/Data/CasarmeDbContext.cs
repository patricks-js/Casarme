using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Data;

public class CasarmeDbContext(DbContextOptions<CasarmeDbContext> options) : DbContext(options)
{
    // Domain schema; the `auth` schema belongs to the authentication service.
    public const string Schema = "app";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
    }
}
