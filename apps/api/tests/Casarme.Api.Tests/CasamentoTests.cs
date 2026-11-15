using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Casarme.Api.Casamentos;
using Casarme.Api.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Casarme.Api.Tests;

public sealed class FixedClock : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(2026, 11, 15, 22, 0, 0, TimeSpan.Zero);
}

public class CasamentoTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(5432)).Build();
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "casarme-jwt-" + Guid.NewGuid());
    private readonly string _secretId = "casarme-tests-" + Guid.NewGuid();
    private WebApplicationFactory<Program> _factory = null!;
    protected string _firstToken = "";
    protected string _secondToken = "";
    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_directory);
        await File.WriteAllTextAsync(Path.Combine(_directory, "appsettings.Development.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(_directory, "Tokens.csproj"), $"<Project Sdk=\"Microsoft.NET.Sdk.Web\"><PropertyGroup><TargetFramework>net10.0</TargetFramework><UserSecretsId>{_secretId}</UserSecretsId></PropertyGroup></Project>");
        _firstToken = await Token("alice", "Alice", "alice@example.test");
        _secondToken = await Token("bob", "Bob", "bob@example.test");
        await _postgres.StartAsync();
        for (var attempt = 0; ; attempt++)
        {
            try { await using var connection = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString()); await connection.OpenAsync(); break; }
            catch (Npgsql.NpgsqlException) when (attempt < 30) { await Task.Delay(500); }
        }
        var configuration = new ConfigurationBuilder().AddJsonFile(Path.Combine(_directory, "appsettings.Development.json")).AddUserSecrets(_secretId).Build();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing")
            .ConfigureServices(services => { services.RemoveAll<TimeProvider>(); services.AddSingleton<TimeProvider>(new FixedClock()); })
            .ConfigureAppConfiguration((_, config) => config.AddConfiguration(configuration).AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:Casarme"] = _postgres.GetConnectionString(), ["Logging:LogLevel:Microsoft.AspNetCore.Authentication"] = "Debug" })));
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CasarmeDbContext>().Database.MigrateAsync();
    }
    private async Task<string> Token(string account, string name, string email)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = _directory, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var arg in new[] { "user-jwts", "create", "--project", "Tokens.csproj", "--output", "token", "--name", account, "--audience", "casarme-tests", "--claim", $"name={name}", "--claim", $"email={email}" }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Assert.True(process.ExitCode == 0, error);
        Assert.False(string.IsNullOrWhiteSpace(output), "dotnet user-jwts returned an empty token: " + error);
        return output.Trim();
    }
    protected HttpClient Client(string? token)
    {
        var client = _factory.CreateClient();
        if (token is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
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
    public async Task DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync(); await _postgres.DisposeAsync(); Directory.Delete(_directory, true);
        // Remove only this fixture's development signing key.
        var appData = Environment.GetEnvironmentVariable("APPDATA");
        var secretPath = string.IsNullOrEmpty(appData)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets", _secretId)
            : Path.Combine(appData, "Microsoft", "UserSecrets", _secretId);
        if (Directory.Exists(secretPath)) Directory.Delete(secretPath, true);
    }
}
