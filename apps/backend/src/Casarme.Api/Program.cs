using Casarme.Api.Common;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.Run();