using System.Net;
using System.Net.Http.Json;
using Casarme.Api.Casamentos;
using Casarme.Api.Convidados;

namespace Casarme.Api.Tests;

public class ConvidadoTests : CasamentoTests
{
    [Fact]
    public async Task Groups_members_contacts_representatives_and_isolation()
    {
        using var alice = Client(_firstToken); using var bob = Client(_secondToken); using var anonymous = Client(null);
        var created = await alice.PostAsJsonAsync("/casamentos/", new SaveCasamento("Casal", new DateOnly(2027, 1, 1), "groups", null));
        var wedding = (await created.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var root = $"/casamentos/{wedding.Id}";
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(root + "/grupos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync(root + "/grupos")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.PostAsJsonAsync(root + "/grupos", new SaveGrupo("Foreign"))).StatusCode);
        foreach (var invalid in new[] { new SaveConvidado("", null, null, null), new SaveConvidado("Ana", "invalid", null, null), new SaveConvidado("Ana", null, "81999999999", null) })
            Assert.Equal(HttpStatusCode.BadRequest, (await alice.PostAsJsonAsync(root + "/convidados", invalid)).StatusCode);
        async Task<GrupoResponse[]> List() => (await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!;
        async Task<ConvidadoResponse> Add(string name, Guid? group = null, string? phone = null)
        {
            var response = await alice.PostAsJsonAsync(root + "/convidados", new SaveConvidado(name, null, phone, group));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<ConvidadoResponse>())!;
        }
        var ana = await Add("Ana", phone: "+55 (81) 99999-9999");
        Assert.True(ana.IsRepresentative); Assert.Equal("+5581999999999", ana.Phone);
        Assert.Equal("Ana", Assert.Single(await List()).DisplayName);
        Assert.Equal(HttpStatusCode.Created, (await alice.PostAsJsonAsync(root + "/grupos", new SaveGrupo(null))).StatusCode);
        var empty = (await List()).Single(x => x.Id != ana.GroupId);
        Assert.Null(empty.DisplayName); Assert.Empty(empty.Members);
        var bia = await Add("Bia", empty.Id); var carla = await Add("Carla", empty.Id); var dora = await Add("Dora", empty.Id);
        Assert.Equal("Bia", (await List()).Single(x => x.Id == empty.Id).DisplayName);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PutAsJsonAsync(root + $"/grupos/{empty.Id}", new SaveGrupo(" ana "))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.DeleteAsync(root + $"/grupos/{empty.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await alice.PutAsJsonAsync(root + $"/grupos/{empty.Id}/representante", new SetRepresentative(ana.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/grupos/{empty.Id}/representante", new SetRepresentative(dora.Id))).StatusCode);
        Assert.Equal("Dora", (await List()).Single(x => x.Id == empty.Id).DisplayName);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/convidados/{dora.Id}", new SaveConvidado("Dora", "dora@example.test", null, ana.GroupId))).StatusCode);
        var afterMove = await List();
        Assert.True(afterMove.Single(x => x.Id == empty.Id).Members.Single(x => x.Id == bia.Id).IsRepresentative);
        Assert.True(afterMove.Single(x => x.Id == ana.GroupId).Members.Single(x => x.Id == ana.Id).IsRepresentative);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync(root + $"/convidados/{bia.Id}")).StatusCode);
        Assert.True((await List()).Single(x => x.Id == empty.Id).Members.Single(x => x.Id == carla.Id).IsRepresentative);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.PutAsJsonAsync(root + $"/grupos/{empty.Id}", new SaveGrupo("Amigos"))).StatusCode);
        Assert.Equal("Amigos", (await List()).Single(x => x.Id == empty.Id).DisplayName);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync(root + $"/convidados/{carla.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync(root + $"/convidados/{carla.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await alice.DeleteAsync(root + $"/grupos/{empty.Id}")).StatusCode);
        Assert.Single(await List());
        var bobCreated = await bob.PostAsJsonAsync("/casamentos/", new SaveCasamento("Outro", new DateOnly(2027, 1, 1), "other", null));
        var other = (await bobCreated.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var foreignGroupResponse = await bob.PostAsJsonAsync($"/casamentos/{other.Id}/grupos", new SaveGrupo("Ana"));
        var foreign = (await foreignGroupResponse.Content.ReadFromJsonAsync<GrupoResponse>())!;
        Assert.Equal(HttpStatusCode.NotFound, (await alice.PutAsJsonAsync(root + $"/convidados/{ana.Id}", new SaveConvidado("Ana", null, null, foreign.Id))).StatusCode);
        Assert.Single((await List()).Single().Members, x => x.Id == ana.Id);
    }

    [Fact]
    public async Task Concurrent_membership_preserves_one_representative_and_unique_names()
    {
        using var alice = Client(_firstToken);
        var response = await alice.PostAsJsonAsync("/casamentos/", new SaveCasamento("Casal", new DateOnly(2027, 1, 1), "concurrent-groups", null));
        var wedding = (await response.Content.ReadFromJsonAsync<CasamentoResponse>())!;
        var root = $"/casamentos/{wedding.Id}";
        var duplicates = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => alice.PostAsJsonAsync(root + "/grupos", new SaveGrupo("Família"))));
        Assert.Single(duplicates, x => x.StatusCode == HttpStatusCode.Created); Assert.Single(duplicates, x => x.StatusCode == HttpStatusCode.Conflict);
        var group = Assert.Single((await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!);
        var members = await Task.WhenAll(Enumerable.Range(0, 3).Select(i => alice.PostAsJsonAsync(root + "/convidados", new SaveConvidado($"Membro {i}", null, null, group.Id))));
        Assert.All(members, x => Assert.Equal(HttpStatusCode.Created, x.StatusCode));
        group = Assert.Single((await alice.GetFromJsonAsync<GrupoResponse[]>(root + "/grupos"))!);
        Assert.Equal(3, group.Members.Length); Assert.Single(group.Members, x => x.IsRepresentative);
    }
}
