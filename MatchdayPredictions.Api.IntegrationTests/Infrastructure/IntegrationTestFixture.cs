using System.Net.Http;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string JwtKey = "integration-tests-secret-key";
    private const string JwtIssuer = "MatchdayPredictions.IntegrationTests";
    private const string JwtAudience = "MatchdayPredictions.IntegrationTests";

    private readonly MsSqlContainer _dbContainer;

    public IntegrationTestFixture()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Str0ngP@ssw0rd!")
            .WithDatabase("MatchdayPredictions")
            .Build();
    }

    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public string ConnectionString => _dbContainer.GetConnectionString();

    public HttpClient CreateClient() => Factory.CreateClient();

    public DatabaseSeeder CreateSeeder() => new(ConnectionString);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var migrator = new DatabaseMigrator(ConnectionString);
        await migrator.ApplyMigrationsAsync();

        Factory = new CustomWebApplicationFactory(ConnectionString, JwtKey, JwtIssuer, JwtAudience);
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();
        await _dbContainer.DisposeAsync();
    }

    public Task ResetDatabaseAsync()
    {
        var seeder = CreateSeeder();
        return seeder.ClearDataAsync();
    }
}
