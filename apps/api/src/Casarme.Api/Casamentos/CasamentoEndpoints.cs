using System.Security.Claims;
using System.Text.RegularExpressions;
using Casarme.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Casarme.Api.Casamentos;

public record SaveCasamento(string CoupleName, DateOnly Date, string Identifier, string? TimeZone);
public record CasamentoResponse(Guid Id, string CoupleName, DateOnly Date, string Identifier, string TimeZone, Guid PrincipalEventId);

public static class CasamentoEndpoints
{
    private static readonly HashSet<string> _reserved = ["api", "c", "casamento", "login", "logout", "auth", "settings", "health", "recepcao"];

    public static void MapCasamentos(this WebApplication app)
    {
        var group = app.MapGroup("/casamentos").RequireAuthorization();
        group.MapGet("/current", async (ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var account = user.FindFirstValue("sub");
            var noivo = await db.Noivos.Include(x => x.Casamento).ThenInclude(x => x.Events)
                .SingleOrDefaultAsync(x => x.AccountId == account);
            if (noivo is null) return Results.NotFound();
            await Refresh(noivo, user, db, clock);
            return Results.Ok(Response(noivo.Casamento));
        }).Produces<CasamentoResponse>().WithName("GetCurrentCasamento");
        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var noivo = await Find(id, user, db);
            if (noivo is null) return Results.NotFound();
            await Refresh(noivo, user, db, clock);
            return Results.Ok(Response(noivo.Casamento));
        }).Produces<CasamentoResponse>().WithName("GetCasamento");
        group.MapPost("/", async (SaveCasamento request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            var account = user.FindFirstValue("sub")!;
            if (await db.Noivos.AnyAsync(x => x.AccountId == account)) return AccountConflict();
            var now = clock.GetUtcNow();
            var value = new Casamento { CoupleName = request.CoupleName.Trim(), Identifier = request.Identifier, TimeZone = request.TimeZone ?? "America/Sao_Paulo", CreatedAt = now, UpdatedAt = now };
            value.Events.Add(new Evento { StartsAt = request.Date.ToDateTime(TimeOnly.MinValue), CreatedAt = now, UpdatedAt = now });
            value.Noivos.Add(new Noivo { AccountId = account, Name = user.FindFirstValue("name")!, Email = user.FindFirstValue("email")!, CreatedAt = now, UpdatedAt = now });
            db.Casamentos.Add(value);
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" } pg)
            { return pg.ConstraintName?.Contains("AccountId") == true ? AccountConflict() : IdentifierConflict(); }
            return Results.Created($"/casamentos/{value.Id}", Response(value));
        }).Produces<CasamentoResponse>(201).WithName("CreateCasamento");
        group.MapPut("/{id:guid}", async (Guid id, SaveCasamento request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
        {
            var noivo = await Find(id, user, db);
            if (noivo is null) return Results.NotFound();
            var errors = Validate(request);
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            var value = noivo.Casamento;
            value.CoupleName = request.CoupleName.Trim(); value.Identifier = request.Identifier;
            value.TimeZone = request.TimeZone ?? "America/Sao_Paulo"; value.UpdatedAt = clock.GetUtcNow();
            var principal = value.Events.Single(x => x.IsPrincipal);
            principal.StartsAt = request.Date.ToDateTime(TimeOnly.FromDateTime(principal.StartsAt)); principal.UpdatedAt = clock.GetUtcNow();
            UpdateProfile(noivo, user, clock);
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" }) { return IdentifierConflict(); }
            return Results.Ok(Response(value));
        }).Produces<CasamentoResponse>().WithName("UpdateCasamento");
    }

    private static Task<Noivo?> Find(Guid id, ClaimsPrincipal user, CasarmeDbContext db) => db.Noivos.Include(x => x.Casamento).ThenInclude(x => x.Events)
        .SingleOrDefaultAsync(x => x.AccountId == user.FindFirstValue("sub") && x.CasamentoId == id);
    private static IResult AccountConflict() => Results.ValidationProblem(new Dictionary<string, string[]> { ["account"] = ["Esta Conta já tem um Casamento."] }, statusCode: 409);
    private static IResult IdentifierConflict() => Results.ValidationProblem(new Dictionary<string, string[]> { ["identifier"] = ["Este Identificador já está em uso."] }, statusCode: 409);
    private static CasamentoResponse Response(Casamento value)
    {
        var principal = value.Events.Single(x => x.IsPrincipal);
        return new(value.Id, value.CoupleName, DateOnly.FromDateTime(principal.StartsAt), value.Identifier, EventTimeZone.Resolve(principal), principal.Id);
    }
    private static Dictionary<string, string[]> Validate(SaveCasamento request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.CoupleName) || request.CoupleName.Length > 200) errors["coupleName"] = ["Informe o nome do casal (até 200 caracteres)."];
        if (request.Date == default) errors["date"] = ["Informe a data do casamento."];
        if (string.IsNullOrEmpty(request.Identifier) || request.Identifier.Length > 80 || !Regex.IsMatch(request.Identifier, "^[a-z0-9]+(?:-[a-z0-9]+)*$") || _reserved.Contains(request.Identifier)) errors["identifier"] = ["Use um Identificador disponível, com letras minúsculas, números e hífens."];
        var zone = request.TimeZone ?? "America/Sao_Paulo";
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(zone, out _) || !zone.Contains('/')) errors["timeZone"] = ["Informe um fuso horário IANA válido."];
        return errors;
    }
    private static void UpdateProfile(Noivo noivo, ClaimsPrincipal user, TimeProvider clock)
    {
        var name = user.FindFirstValue("name")!; var email = user.FindFirstValue("email")!;
        if (noivo.Name == name && noivo.Email == email) return;
        noivo.Name = name; noivo.Email = email; noivo.UpdatedAt = clock.GetUtcNow();
    }
    private static async Task Refresh(Noivo noivo, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock)
    { UpdateProfile(noivo, user, clock); await db.SaveChangesAsync(); }
}
