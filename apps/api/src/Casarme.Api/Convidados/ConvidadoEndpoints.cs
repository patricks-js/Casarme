using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Casarme.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Casarme.Api.Convidados;

public record SaveGrupo(string? Name);
public record SaveConvidado(string Name, string? Email, string? Phone, Guid? GroupId);
public record SetRepresentative(Guid GuestId);
public record ConvidadoResponse(Guid Id, Guid GroupId, string Name, string? Email, string? Phone, bool IsRepresentative);
public record GrupoResponse(Guid Id, string? Name, string? DisplayName, ConvidadoResponse[] Members);

public static class ConvidadoEndpoints
{
    public static void MapConvidados(this WebApplication app)
    {
        var routes = app.MapGroup("/casamentos/{casamentoId:guid}").RequireAuthorization();
        routes.MapGet("/grupos", async (Guid casamentoId, ClaimsPrincipal user, CasarmeDbContext db) =>
        {
            if (!await Authorized(casamentoId, user, db)) return Results.NotFound();
            var groups = await db.Groups.Include(x => x.Members).Where(x => x.CasamentoId == casamentoId).ToListAsync();
            return Results.Ok(groups.OrderBy(x => x.DisplayName).ThenBy(x => x.Id).Select(Response));
        }).Produces<GrupoResponse[]>().WithName("ListGrupos");
        routes.MapPost("/grupos", (Guid casamentoId, SaveGrupo request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups =>
            {
                if (request.Name?.Length > 200) return Invalid("name", "Use até 200 caracteres.");
                var group = new Grupo { CasamentoId = casamentoId, Name = Clean(request.Name), CreatedAt = clock.GetUtcNow() };
                groups.Add(group); db.Groups.Add(group);
                return Results.Created($"/casamentos/{casamentoId}/grupos/{group.Id}", Response(group));
            })).Produces<GrupoResponse>(201).WithName("CreateGrupo");
        routes.MapPut("/grupos/{id:guid}", (Guid casamentoId, Guid id, SaveGrupo request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups =>
            {
                var group = groups.SingleOrDefault(x => x.Id == id);
                if (group is null) return Results.NotFound();
                if (request.Name?.Length > 200) return Invalid("name", "Use até 200 caracteres.");
                group.Name = Clean(request.Name); return Results.NoContent();
            })).WithName("UpdateGrupo");
        routes.MapDelete("/grupos/{id:guid}", (Guid casamentoId, Guid id, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups =>
            {
                var group = groups.SingleOrDefault(x => x.Id == id);
                if (group is null) return Results.NotFound();
                if (group.Members.Count > 0) return Invalid("group", "Apague ou mova os Convidados antes de apagar o Grupo.", 409);
                groups.Remove(group); db.Groups.Remove(group); return Results.NoContent();
            })).WithName("DeleteGrupo");
        routes.MapPut("/grupos/{id:guid}/representante", (Guid casamentoId, Guid id, SetRepresentative request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups =>
            {
                var group = groups.SingleOrDefault(x => x.Id == id);
                if (group is null || !group.Members.Any(x => x.Id == request.GuestId)) return Results.NotFound();
                foreach (var member in group.Members) { member.IsRepresentative = member.Id == request.GuestId; member.UpdatedAt = clock.GetUtcNow(); }
                return Results.NoContent();
            })).WithName("SetGrupoRepresentative");
        routes.MapPost("/convidados", (Guid casamentoId, SaveConvidado request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups => SaveGuest(casamentoId, null, request, groups, db, clock))).Produces<ConvidadoResponse>(201).WithName("CreateConvidado");
        routes.MapPut("/convidados/{id:guid}", (Guid casamentoId, Guid id, SaveConvidado request, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups => SaveGuest(casamentoId, id, request, groups, db, clock))).WithName("UpdateConvidado");
        routes.MapDelete("/convidados/{id:guid}", (Guid casamentoId, Guid id, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock) =>
            Mutate(casamentoId, user, db, clock, groups =>
            {
                var guest = groups.SelectMany(x => x.Members).SingleOrDefault(x => x.Id == id);
                if (guest is null) return Results.NotFound();
                guest.Grupo.Members.Remove(guest); db.Guests.Remove(guest); return Results.NoContent();
            })).WithName("DeleteConvidado");
    }

    private static IResult SaveGuest(Guid casamentoId, Guid? id, SaveConvidado request, List<Grupo> groups, CasarmeDbContext db, TimeProvider clock)
    {
        var guest = id.HasValue ? groups.SelectMany(x => x.Members).SingleOrDefault(x => x.Id == id) : null;
        if (id.HasValue && guest is null) return Results.NotFound();
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200) errors["name"] = ["Informe o nome (até 200 caracteres)."];
        var email = Clean(request.Email);
        if (email is not null && (email.Length > 254 || !MailAddress.TryCreate(email, out var address) || address.Address != email)) errors["email"] = ["Informe um e-mail válido."];
        var phone = Clean(request.Phone);
        if (phone is not null)
        {
            phone = Regex.Replace(phone, "[ ()-]", "");
            if (!Regex.IsMatch(phone, @"^\+[1-9][0-9]{7,14}$")) errors["phone"] = ["Informe o telefone com + e DDI (ex.: +5581999999999)."];
        }
        if (errors.Count > 0) return Results.ValidationProblem(errors);
        var target = request.GroupId.HasValue ? groups.SingleOrDefault(x => x.Id == request.GroupId) : null;
        if (request.GroupId.HasValue && target is null) return Results.NotFound();
        var now = clock.GetUtcNow();
        if (target is null)
        {
            target = new Grupo { CasamentoId = casamentoId, Name = request.Name.Trim(), CreatedAt = now };
            groups.Add(target); db.Groups.Add(target);
        }
        if (guest is null) { guest = new Convidado { CreatedAt = now }; db.Guests.Add(guest); }
        if (guest.GrupoId != target.Id)
        {
            guest.Grupo?.Members.Remove(guest); guest.IsRepresentative = false;
            guest.JoinedOrder = target.Members.Select(x => x.JoinedOrder).DefaultIfEmpty(0).Max() + 1;
            guest.Grupo = target; guest.GrupoId = target.Id; target.Members.Add(guest);
        }
        guest.Name = request.Name.Trim(); guest.Email = email; guest.Phone = phone; guest.UpdatedAt = now;
        target.Normalize(now);
        return id.HasValue ? Results.NoContent() : Results.Created($"/casamentos/{casamentoId}/convidados/{guest.Id}", GuestResponse(guest));
    }

    private static async Task<IResult> Mutate(Guid casamentoId, ClaimsPrincipal user, CasarmeDbContext db, TimeProvider clock, Func<List<Grupo>, IResult> change)
    {
        if (!await Authorized(casamentoId, user, db)) return Results.NotFound();
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Serialize membership and representative decisions within this Casamento.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM app.\"Casamentos\" WHERE \"Id\" = {casamentoId} FOR UPDATE");
        var groups = await db.Groups.Include(x => x.Members).Where(x => x.CasamentoId == casamentoId).ToListAsync();
        var result = change(groups);
        if (result is IStatusCodeHttpResult { StatusCode: >= 400 }) return result;
        foreach (var group in groups) group.Normalize(clock.GetUtcNow());
        if (groups.Where(x => x.NameKey is not null).GroupBy(x => x.NameKey).Any(x => x.Count() > 1)) return Invalid("name", "Já existe um Grupo com esse nome neste Casamento.", 409);
        try { await db.SaveChangesAsync(); await transaction.CommitAsync(); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23505" }) { return Invalid("name", "Já existe um Grupo com esse nome neste Casamento.", 409); }
        return result;
    }
    private static Task<bool> Authorized(Guid casamentoId, ClaimsPrincipal user, CasarmeDbContext db) => db.Noivos.AnyAsync(x => x.CasamentoId == casamentoId && x.AccountId == user.FindFirstValue("sub"));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IResult Invalid(string field, string message, int status = 400) => Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] }, statusCode: status);
    private static ConvidadoResponse GuestResponse(Convidado value) => new(value.Id, value.GrupoId, value.Name, value.Email, value.Phone, value.IsRepresentative);
    private static GrupoResponse Response(Grupo value) => new(value.Id, value.Name, value.DisplayName, value.Members.OrderBy(x => x.JoinedOrder).ThenBy(x => x.Id).Select(GuestResponse).ToArray());
}
