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
        group.MapGet("/{id:guid}", GetWeddingByIdAsync);
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
            var id = await service.CreateWeddingAsync(command, ct);
            return TypedResults.CreatedAtRoute("GetWeddingById", new { id });
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return TypedResults.InternalServerError(new { error = ex.Message });
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
        catch (ArgumentOutOfRangeException ex)
        {
            return TypedResults.BadRequest(new { error = ex.Message });
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
}

public sealed record CreateWeddingRequest(string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
public sealed record UpdateWeddingRequest(string PartnerOneName, string PartnerTwoName, string Slug, DateOnly PrimaryDate);
