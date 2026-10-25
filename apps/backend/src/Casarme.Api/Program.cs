using Casarme.Api.Common;
using Casarme.Api.Data;
using Casarme.Api.Endpoints;
using Casarme.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var jwksUrl = builder.Configuration["Authentication:JwksUrl"]
    ?? throw new InvalidOperationException("Authentication:JwksUrl is not configured");

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
        jwksUrl,
        new JwksRetriever(),
        new HttpDocumentRetriever { RequireHttps = !builder.Environment.IsDevelopment() });
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// Services
builder.Services.AddScoped<WeddingService>();

var app = builder.Build();

// Unhandled exceptions become a bare 500 problem response, without leaking details
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/openapi");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/status", () => Results.Ok(new { status = "Ok" }));

app.MapGet("/me",
        (ICurrentUser currentUser) =>
            TypedResults.Ok(new { mensagem = "Authorized access!", id = currentUser.Id, email = currentUser.Email, }))
    .RequireAuthorization();

app.MapWeddingEndpoint();

app.Run();
