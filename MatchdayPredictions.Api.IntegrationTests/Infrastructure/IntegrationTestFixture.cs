using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string JwtKey = "integration-tests-secret-key-0123456789abcdef";
    private const string JwtIssuer = "MatchdayPredictions.IntegrationTests";
    private const string JwtAudience = "MatchdayPredictions.IntegrationTests";
    private const string DatabaseName = "MatchdayPredictions";
    private const string DatabasePassword = "Str0ngP@ssw0rd!";

    private readonly MsSqlContainer _dbContainer;

    public IntegrationTestFixture()
    {
        var readinessCommand = $"/opt/mssql-tools18/bin/sqlcmd -C -S 127.0.0.1 -U sa -P \"{DatabasePassword}\" -Q \"SELECT 1\"";

        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(DatabasePassword)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted(readinessCommand))
            .Build();
    }

    public CustomWebApplicationFactory Factory { get; private set; } = null!;

    public string ConnectionString { get; private set; } = null!;

    public HttpClient CreateClient() => Factory.CreateClient();

    public CustomWebApplicationFactory CreateFactory(LoginRateLimitSettings? rateLimitSettings = null)
        => new(ConnectionString, JwtKey, JwtIssuer, JwtAudience, rateLimitSettings);

    public DatabaseSeeder CreateSeeder() => new(ConnectionString);

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        ConnectionString = await EnsureDatabaseAsync();

        var migrator = new DatabaseMigrator(ConnectionString);
        await migrator.ApplyMigrationsAsync();

        Factory = CreateFactory();
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

    private async Task<string> EnsureDatabaseAsync()
    {
        var baseBuilder = new SqlConnectionStringBuilder(_dbContainer.GetConnectionString());
        var adminConnectionString = new SqlConnectionStringBuilder(baseBuilder.ConnectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        var databaseConnectionString = new SqlConnectionStringBuilder(baseBuilder.ConnectionString)
        {
            InitialCatalog = DatabaseName
        }.ConnectionString;

        await using var connection = new SqlConnection(adminConnectionString);
        await connection.OpenAsync();

        var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"""
            IF DB_ID('{DatabaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{DatabaseName}];
            END
            """;
        await createCommand.ExecuteNonQueryAsync();

        var optionsCommand = connection.CreateCommand();
        optionsCommand.CommandText = $"""
            ALTER DATABASE [{DatabaseName}] SET ALLOW_SNAPSHOT_ISOLATION ON;
            ALTER DATABASE [{DatabaseName}] SET READ_COMMITTED_SNAPSHOT ON;
            """;
        await optionsCommand.ExecuteNonQueryAsync();

        return databaseConnectionString;
    }
}
