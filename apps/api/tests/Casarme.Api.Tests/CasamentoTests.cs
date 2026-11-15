using System.Net;
using System.Net.Http.Json;
using Casarme.Api.Casamentos;
using Casarme.Api.Data;
using Casarme.Api.Eventos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Casarme.Api.Tests;

public class CasamentoTests : ApiTestBase
{
    [Fact]
    public async Task Registration_edit_profile_and_isolation()
    {
        using var anonymous = Client(null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/casamentos/current")).StatusCode);
        using var invalid = Client("invalid.token.value");
        Assert.Equal(HttpStatusCode.Unauthorized, (await invalid.GetAsync("/casamentos/current")).StatusCode);
        using var alice = Client(_firstToken); using var bob = Client(_secondToken);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.GetAsync("/casamentos/current")).StatusCode);
        var request = new SaveCasamento("Alice e Bia", new DateOnly(2027, 6, 12), "alice-e-bia", null);
        foreach (var identifier in new[] { "api", "c", "login", "UPPER", "bad/slash" })
            Assert.Equal(HttpStatusCode.BadRequest, (await alice.PostAsJsonAsync("/casamentos/", request with { Identifier = identifier })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await alice.PostAsJsonAsync("/casamentos/", request with { TimeZone = "invalid/zone" })).StatusCode);
        var created = await alice.PostAsJsonAsync("/casamentos/", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var value = (await created.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        Assert.Equal("America/Sao_Paulo", value.TimeZone);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PostAsJsonAsync("/casamentos/", request with { Identifier = "another" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await bob.PostAsJsonAsync("/casamentos/", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/casamentos/{value.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync($"/casamentos/{value.Id}", request)).StatusCode);
        var edit = request with { CoupleName = "Alice e Beatriz", Date = new DateOnly(2028, 1, 10), Identifier = "alice-e-beatriz", TimeZone = "America/Recife" };
        var edited = await alice.PutAsJsonAsync($"/casamentos/{value.Id}", edit);
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        var result = (await edited.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        Assert.Equal(edit.Date, result.Date); Assert.Equal(edit.Identifier, result.Identifier); Assert.Equal(edit.CoupleName, result.CoupleName); Assert.Equal(edit.TimeZone, result.TimeZone);
        using var updatedAlice = Client(await Token("alice", "Alice Nova", "new@example.test"));
        Assert.Equal(HttpStatusCode.OK, (await updatedAlice.GetAsync("/casamentos/current")).StatusCode);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasarmeDbContext>();
        var noivo = await db.Noivos.SingleAsync();
        Assert.Equal(new FixedClock().GetUtcNow(), noivo.CreatedAt);
        Assert.Equal("Alice Nova", noivo.Name); Assert.Equal("new@example.test", noivo.Email);
        Assert.Single(await db.Casamentos.ToListAsync()); Assert.Single(await db.Events.ToListAsync());
        Assert.Equal(edit.Date.ToDateTime(TimeOnly.MinValue), (await db.Events.SingleAsync()).StartsAt);
    }
    [Fact]
    public async Task Concurrent_registration_is_atomic()
    {
        using var alice = Client(_firstToken); using var bob = Client(_secondToken);
        var request = new SaveCasamento("Casal", new DateOnly(2027, 1, 1), "same", null);
        var responses = await Task.WhenAll(alice.PostAsJsonAsync("/casamentos/", request), bob.PostAsJsonAsync("/casamentos/", request));
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasarmeDbContext>();
        Assert.Single(await db.Casamentos.ToListAsync()); Assert.Single(await db.Noivos.ToListAsync()); Assert.Single(await db.Events.ToListAsync());
    }
    [Fact]
    public async Task Concurrent_registration_for_same_account_creates_only_one_wedding()
    {
        using var first = Client(_firstToken); using var second = Client(_firstToken);
        var request = new SaveCasamento("Casal", new DateOnly(2027, 1, 1), "first", null);
        var responses = await Task.WhenAll(first.PostAsJsonAsync("/casamentos/", request), second.PostAsJsonAsync("/casamentos/", request with { Identifier = "second" }));
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, x => x.StatusCode == HttpStatusCode.Conflict);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CasarmeDbContext>();
        Assert.Single(await db.Casamentos.ToListAsync()); Assert.Single(await db.Noivos.ToListAsync()); Assert.Single(await db.Events.ToListAsync());
    }
    [Fact]
    public async Task Eventos_lifecycle_validation_and_account_isolation()
    {
        using var alice = Client(_firstToken); using var bob = Client(_secondToken); using var anonymous = Client(null);
        var wedding = (await (await alice.PostAsJsonAsync("/casamentos/", new SaveCasamento("Casal", new DateOnly(2027, 6, 12), "events", "America/Recife"))).Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var path = $"/casamentos/{wedding.Id}/eventos";
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(path)).StatusCode);
        var request = new SaveEvento("Chá", new DateTime(2027, 2, 10, 15, 30, 0), "Jardim", true, null);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PostAsJsonAsync(path, request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(path)).StatusCode);
        foreach (var invalid in new[] { request with { Name = " " }, request with { Location = "" }, request with { StartsAt = default }, request with { StartsAt = DateTime.UtcNow }, request with { RsvpDeadline = DateTime.UtcNow } })
            Assert.Equal(HttpStatusCode.BadRequest, (await alice.PostAsJsonAsync(path, invalid)).StatusCode);
        var created = await alice.PostAsJsonAsync(path, request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var value = (await created.Content.ReadFromJsonAsync<EventoResponse>())!;
        Assert.False(value.IsPrincipal); Assert.Equal("active", value.Status); Assert.Equal(request.StartsAt, value.EffectiveRsvpDeadline); Assert.Equal("America/Recife", value.TimeZone);
        var item = $"{path}/{value.Id}";
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PutAsJsonAsync(item, request)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PostAsync(item + "/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync(item)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.PutAsJsonAsync($"/casamentos/{Guid.NewGuid()}/eventos/{value.Id}", request)).StatusCode);
        var edit = request with { Name = "Chá de panela", StartsAt = new DateTime(2027, 3, 12, 16, 0, 0), HasReception = false, RsvpDeadline = new DateTime(2027, 3, 1, 12, 0, 0), Location = "Salão" };
        var edited = (await (await alice.PutAsJsonAsync(item, edit)).Content.ReadFromJsonAsync<EventoResponse>())!;
        Assert.Equal(edit.Name, edited.Name); Assert.Equal(edit.StartsAt, edited.StartsAt); Assert.Equal(edit.Location, edited.Location); Assert.False(edited.HasReception); Assert.Equal(edit.RsvpDeadline, edited.EffectiveRsvpDeadline);
        var principalPath = $"{path}/{wedding.PrincipalEventId}";
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PostAsync(principalPath + "/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.DeleteAsync(principalPath)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync(principalPath, request with { StartsAt = new DateTime(2027, 6, 12, 18, 30, 0) })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync($"/casamentos/{wedding.Id}", new SaveCasamento("Casal", new DateOnly(2027, 6, 13), "events", "America/Recife"))).StatusCode);
        var cancelled = (await (await alice.PostAsync(item + "/cancel", null)).Content.ReadFromJsonAsync<EventoResponse>())!;
        Assert.Equal("cancelled", cancelled.Status);
        var stillCancelled = (await (await alice.PutAsJsonAsync(item, request)).Content.ReadFromJsonAsync<EventoResponse>())!;
        Assert.Equal("cancelled", stillCancelled.Status); Assert.True(stillCancelled.HasReception);
        var list = (await alice.GetFromJsonAsync<EventoResponse[]>(path))!;
        Assert.Equal(2, list.Length); Assert.Equal(value.Id, list[0].Id); Assert.Equal("cancelled", list[0].Status); Assert.True(list[1].IsPrincipal);
        Assert.Equal(new DateTime(2027, 6, 13, 18, 30, 0), list[1].StartsAt);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync(item)).StatusCode);
        Assert.Single((await alice.GetFromJsonAsync<EventoResponse[]>(path))!);
    }
}
