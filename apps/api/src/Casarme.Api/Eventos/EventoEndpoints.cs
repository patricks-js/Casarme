using System.Security.Claims;
using Casarme.Api.Casamentos;
using Casarme.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Casarme.Api.Eventos;

public record SaveEvento(string Name, DateTime StartsAt, string Location, bool HasReception, DateTime? RsvpDeadline);
public record EventoResponse(Guid Id, string Name, DateTime StartsAt, string Location, bool HasReception, DateTime? RsvpDeadline, DateTime EffectiveRsvpDeadline, bool IsPrincipal, string Status, string TimeZone);

public static class EventoEndpoints
{
    public static void MapEventos(this WebApplication app)
    {
        var group = app.MapGroup("/casamentos/{casamentoId:guid}/eventos").RequireAuthorization();
        group.MapGet("/", async (Guid casamentoId, ClaimsPrincipal user, CasarmeDbContext db) =>
        {
            var wedding = await FindWedding(casamentoId, user, db);
            if (wedding is null) return Results.NotFound();
            var values = await db.Events.Where(x => x.CasamentoId == casamentoId).OrderBy(x => x.StartsAt).ThenBy(x => x.Id).ToListAsync();
            return Results.Ok(values.Select(Response).ToArray());
        }).Produces<EventoResponse[]>().WithName("ListEventos");
        group.MapPost("/", async (Guid casamentoId, SaveEvento request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var wedding = await FindWedding(casamentoId, user, db);
            if (wedding is null) return Results.NotFound();
            var errors = Validate(request, wedding.TimeZone);
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            var now = clock.GetUtcNow();
            var value = new Evento { Casamento = wedding, IsPrincipal = false, CreatedAt = now, UpdatedAt = now };
            Apply(value, request);
            db.Events.Add(value);
            await db.SaveChangesAsync();
            return Results.Created($"/casamentos/{casamentoId}/eventos/{value.Id}", Response(value));
        }).Produces<EventoResponse>(201).WithName("CreateEvento");
        group.MapPut("/{id:guid}", async (Guid casamentoId, Guid id, SaveEvento request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var value = await Find(casamentoId, id, user, db);
            if (value is null) return Results.NotFound();
            var errors = Validate(request, EventTimeZone.Resolve(value));
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            Apply(value, request);
            value.UpdatedAt = clock.GetUtcNow();
            await db.SaveChangesAsync();
            return Results.Ok(Response(value));
        }).Produces<EventoResponse>().WithName("UpdateEvento");
        group.MapPost("/{id:guid}/cancel", async (Guid casamentoId, Guid id, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var value = await Find(casamentoId, id, user, db);
            if (value is null) return Results.NotFound();
            if (!value.Cancel(clock.GetUtcNow())) return PrincipalConflict();
            await db.SaveChangesAsync();
            return Results.Ok(Response(value));
        }).Produces<EventoResponse>().WithName("CancelEvento");
        group.MapDelete("/{id:guid}", async (Guid casamentoId, Guid id, ClaimsPrincipal user, CasarmeDbContext db) =>
        {
            var value = await Find(casamentoId, id, user, db);
            if (value is null) return Results.NotFound();
            if (value.IsPrincipal) return PrincipalConflict();
            db.Events.Remove(value);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteEvento");
    }

    private static Task<Casamento?> FindWedding(Guid id, ClaimsPrincipal user, CasarmeDbContext db) => db.Casamentos.SingleOrDefaultAsync(x => x.Id == id && x.Noivos.Any(n => n.AccountId == user.FindFirstValue("sub")));
    private static Task<Evento?> Find(Guid weddingId, Guid id, ClaimsPrincipal user, CasarmeDbContext db) => db.Events.Include(x => x.Casamento).SingleOrDefaultAsync(x => x.Id == id && x.CasamentoId == weddingId && x.Casamento.Noivos.Any(n => n.AccountId == user.FindFirstValue("sub")));
    private static IResult PrincipalConflict() => Results.ValidationProblem(new Dictionary<string, string[]> { ["event"] = ["O Evento principal não pode ser cancelado nem apagado."] }, statusCode: 409);
    private static EventoResponse Response(Evento value) => new(value.Id, value.Name, value.StartsAt, value.Location, value.HasReception, value.RsvpDeadline, value.EffectiveRsvpDeadline, value.IsPrincipal, value.CancelledAt is null ? "active" : "cancelled", EventTimeZone.Resolve(value));
    private static void Apply(Evento value, SaveEvento request)
    {
        value.Name = request.Name.Trim(); value.StartsAt = request.StartsAt;
        value.Location = request.Location.Trim(); value.HasReception = request.HasReception; value.RsvpDeadline = request.RsvpDeadline;
    }
    private static Dictionary<string, string[]> Validate(SaveEvento request, string zone)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200) errors["name"] = ["Informe o nome do Evento (até 200 caracteres)."];
        if (string.IsNullOrWhiteSpace(request.Location) || request.Location.Length > 500) errors["location"] = ["Informe o local (até 500 caracteres)."];
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(zone);
        if (!ValidLocal(request.StartsAt, timeZone)) errors["startsAt"] = ["Informe data e horário locais válidos no fuso do Casamento, sem Z ou deslocamento UTC."];
        if (request.RsvpDeadline is { } deadline && !ValidLocal(deadline, timeZone)) errors["rsvpDeadline"] = ["Informe um prazo local válido no fuso do Casamento."];
        return errors;
    }
    private static bool ValidLocal(DateTime value, TimeZoneInfo zone) => value != default && value.Kind == DateTimeKind.Unspecified && !zone.IsInvalidTime(value);
}
