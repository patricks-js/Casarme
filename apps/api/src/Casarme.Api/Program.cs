using Casarme.Api.Data;
using Casarme.Api.Status;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<CasarmeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Casarme"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", CasarmeDbContext.Schema)));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CasarmeDbContext>("postgres");

var app = builder.Build();

app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapStatus();

app.Run();
