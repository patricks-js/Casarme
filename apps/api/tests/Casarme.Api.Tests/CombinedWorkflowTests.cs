using System.Net;
using System.Net.Http.Json;
using Casarme.Api.Casamentos;
using Casarme.Api.Convidados;
using Casarme.Api.Eventos;

namespace Casarme.Api.Tests;

public class CombinedWorkflowTests : ApiTestBase
{
    [Fact]
    public async Task Wedding_events_and_guests_coexist_with_account_isolation()
    {
        using var alice = Client(_firstToken);
        using var bob = Client(_secondToken);
        var registration = new SaveCasamento("Alice e Bia", new DateOnly(2027, 6, 12), "combined", "America/Recife");
        var created = await alice.PostAsJsonAsync("/casamentos/", registration);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var wedding = (await created.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var otherCreated = await bob.PostAsJsonAsync("/casamentos/", registration with { Identifier = "other-combined" });
        Assert.Equal(HttpStatusCode.Created, otherCreated.StatusCode);
        var other = (await otherCreated.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var root = $"/casamentos/{wedding.Id}";
        var eventRequest = new SaveEvento("Chá de panela", new DateTime(2027, 5, 1, 15, 0, 0), "Jardim", false, null);
        var eventCreated = await alice.PostAsJsonAsync(root + "/eventos", eventRequest);
        Assert.Equal(HttpStatusCode.Created, eventCreated.StatusCode);
        var occasion = (await eventCreated.Content.ReadFromJsonAsync<EventoResponse>())!;
        var guestCreated = await alice.PostAsJsonAsync(root + "/convidados", new SaveConvidado("Ana", null, "+55 (81) 99999-9999", null));
        Assert.Equal(HttpStatusCode.Created, guestCreated.StatusCode);
        var guest = (await guestCreated.Content.ReadFromJsonAsync<ConvidadoResponse>())!;
        Assert.Equal("+5581999999999", guest.Phone);

        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(root + "/eventos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(root + "/grupos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/casamentos/{other.Id}/eventos/{occasion.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/casamentos/{other.Id}/convidados/{guest.Id}")).StatusCode);
        Assert.Empty((await bob.GetFromJsonAsync<GrupoResponse[]>($"/casamentos/{other.Id}/grupos"))!);

        var ceremony = eventRequest with { Name = "Cerimônia", StartsAt = new DateTime(2027, 6, 13, 18, 30, 0), HasReception = true };
        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync(root + $"/eventos/{wedding.PrincipalEventId}", ceremony)).StatusCode);
        Assert.Equal(new DateOnly(2027, 6, 13), (await alice.GetFromJsonAsync<CasamentoResponse>("/casamentos/current"))!.Date);
        Assert.Equal(HttpStatusCode.OK, (await alice.PutAsJsonAsync(root, registration with { Date = new DateOnly(2027, 6, 14), TimeZone = "America/Manaus" })).StatusCode);
        var events = (await alice.GetFromJsonAsync<EventoResponse[]>(root + "/eventos"))!;
        Assert.All(events, value => Assert.Equal("America/Manaus", value.TimeZone));
        Assert.Equal(new DateTime(2027, 6, 14, 18, 30, 0), events.Single(x => x.IsPrincipal).StartsAt);

        var groupCreated = await alice.PostAsJsonAsync(root + "/grupos", new SaveGrupo("Amigos"));
        Assert.Equal(HttpStatusCode.Created, groupCreated.StatusCode);
        var group = (await groupCreated.Content.ReadFromJsonAsync<GrupoResponse>())!;
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/convidados/{guest.Id}", new SaveConvidado("Ana", "ana@example.test", guest.Phone, group.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await alice.PostAsync(root + $"/eventos/{occasion.Id}/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync(root + $"/eventos/{occasion.Id}")).StatusCode);
        var groups = (await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!;
        var member = Assert.Single(groups.Single(x => x.Id == group.Id).Members);
        Assert.Equal(guest.Id, member.Id);
        Assert.True(member.IsRepresentative);
        Assert.Empty(groups.Single(x => x.Id == guest.GroupId).Members);
        Assert.Single((await alice.GetFromJsonAsync<EventoResponse[]>(root + "/eventos"))!);

        // A failed individual-group creation must leave the existing membership intact.
        var individual = new SaveConvidado("Ana", null, null, null);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PutAsJsonAsync(root + $"/convidados/{guest.Id}", individual)).StatusCode);
        groups = (await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!;
        Assert.Equal(guest.Id, Assert.Single(groups.Single(x => x.Id == group.Id).Members).Id);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/grupos/{guest.GroupId}", new SaveGrupo("Grupo anterior"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/convidados/{guest.Id}", individual)).StatusCode);
        groups = (await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!;
        Assert.Empty(groups.Single(x => x.Id == group.Id).Members);
        var solo = groups.Single(x => x.DisplayName == "Ana");
        Assert.True(Assert.Single(solo.Members).IsRepresentative);
        Assert.Equal(guest.Id, solo.Members[0].Id);
    }
}
