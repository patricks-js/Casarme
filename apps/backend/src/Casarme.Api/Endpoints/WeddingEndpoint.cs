using Casarme.Api.Common;
using Casarme.Api.Services;

namespace Casarme.Api.Endpoints;

public static class WeddingEndpoint
{
    public static IEndpointRouteBuilder MapWeddingEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/weddings")
            .RequireAuthorization()
            .WithTags("Weddings");

        group.MapPost("/", CreateWeddingAsync);
        group.MapGet("/", ListWeddingsAsync);
        group.MapGet("/slug-availability", CheckSlugAvailabilityAsync);
        group.MapGet("/{id:guid}", GetWeddingByIdAsync).WithName("GetWeddingById");
        group.MapPut("/{id:guid}", UpdateWeddingAsync);
        group.MapDelete("/{id:guid}", DeleteWeddingAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateWeddingAsync(
        ICurrentUser currentUser,
        CreateWeddingRequest request,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        var command = new CreateWeddingCommand(
            ownerId,
            request.PartnerOneName,
            request.PartnerTwoName,
            request.Slug,
            request.PrimaryDate);

        try
        {
            var wedding = await service.CreateWeddingAsync(command, ct);
            return TypedResults.CreatedAtRoute(wedding, "GetWeddingById", new { id = wedding.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetWeddingByIdAsync(
        Guid id,
        ICurrentUser currentUser,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        var wedding = await service.GetByIdAsync(id, ownerId, ct);
        return wedding is not null
            ? TypedResults.Ok(wedding)
            : TypedResults.NotFound();
    }

    private static async Task<IResult> ListWeddingsAsync(
        ICurrentUser currentUser,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        var list = await service.ListByOwnerAsync(ownerId, ct);
        return TypedResults.Ok(list);
    }

    private static async Task<IResult> CheckSlugAvailabilityAsync(
        string slug,
        ICurrentUser currentUser,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        try
        {
            var availability = await service.CheckSlugAvailabilityAsync(slug, ownerId, ct);
            return TypedResults.Ok(availability);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex);
        }
    }

    private static async Task<IResult> UpdateWeddingAsync(
        Guid id,
        UpdateWeddingRequest request,
        ICurrentUser currentUser,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        var command = new UpdateWeddingCommand(
            id,
            ownerId,
            request.PartnerOneName,
            request.PartnerTwoName,
            request.Slug,
            request.PrimaryDate
        );

        try
        {
            await service.UpdateAsync(command, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex);
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteWeddingAsync(
        Guid id,
        ICurrentUser currentUser,
        WeddingService service,
        CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.Id, out var ownerId))
            return TypedResults.Unauthorized();

        try
        {
            await service.DeleteAsync(id, ownerId, ct);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    // `field` lets the web app attach the error to the right form input
    private static IResult BadRequest(ArgumentException ex) =>
        TypedResults.BadRequest(new { error = ex.Message, field = ex.ParamName });
}

public sealed record CreateWeddingRequest(string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
public sealed record UpdateWeddingRequest(string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
