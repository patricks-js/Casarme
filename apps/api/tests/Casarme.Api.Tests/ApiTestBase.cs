using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Casarme.Api.Casamentos;
using Casarme.Api.Data;
using Casarme.Api.Eventos;
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

public abstract class ApiTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(5432)).Build();
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "casarme-jwt-" + Guid.NewGuid());
    private readonly string _secretId = "casarme-tests-" + Guid.NewGuid();
    protected WebApplicationFactory<Program> _factory = null!;
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
    protected async Task<string> Token(string account, string name, string email)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = _directory, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var arg in new[] { "user-jwts", "create", "--project", "Tokens.csproj", "--output", "token", "--name", account, "--audience", "casarme-tests", "--claim", $"name={name}", "--claim", $"email={email}" }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        Assert.True(process.ExitCode == 0, error);
        Assert.False(string.IsNullOrWhiteSpace(output), "dotnet user-jwts must return a signed token.");
        return output.Trim();
    }
    protected HttpClient Client(string? token)
    {
        var client = _factory.CreateClient();
        if (token is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
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
