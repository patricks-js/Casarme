using Casarme.Api.Casamentos;
using Casarme.Api.Convidados;
using Casarme.Api.Data;
using Casarme.Api.Eventos;
using Casarme.Api.Status;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    // Development signing keys are supplied by dotnet user-jwts, never committed.
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
    {
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["Authentication:Issuer"];
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Authentication:Audience"];
        var jwks = builder.Configuration["Authentication:JwksUrl"];
        if (!string.IsNullOrWhiteSpace(jwks))
            options.ConfigurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                jwks, new JwksRetriever(), new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever { RequireHttps = true });
    }
});
builder.Services.AddAuthorization(options => options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser().RequireClaim("sub").RequireClaim("name").RequireClaim("email").Build());
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddDbContext<CasarmeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Casarme"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", CasarmeDbContext.Schema)));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CasarmeDbContext>("postgres");

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapCasamentos();
app.MapEventos();
app.MapConvidados();
app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapStatus();

app.Run();

public partial class Program { }
