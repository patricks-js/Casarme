namespace Casarme.Api.Status;

public record ApiStatus(string Service, string Environment, DateTimeOffset Now);

public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatus(this IEndpointRouteBuilder app)
    {
        app.MapGet("/status", (IHostEnvironment env, TimeProvider clock) =>
                TypedResults.Ok(new ApiStatus("casarme-api", env.EnvironmentName, clock.GetUtcNow())))
            .WithName("GetStatus")
            .WithTags("Status");

        return app;
    }
}
