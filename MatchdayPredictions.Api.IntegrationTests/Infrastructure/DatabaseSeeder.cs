using Dapper;
using Microsoft.Data.SqlClient;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class DatabaseSeeder
{
    private readonly string _connectionString;

    public DatabaseSeeder(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ClearDataAsync()
    {
        const string sql = """
            DELETE FROM dbo.Predictions;
            DELETE FROM dbo.LeagueMembers;
            DELETE FROM dbo.LeagueMatches;
            DELETE FROM dbo.Leagues;
            DELETE FROM dbo.Users;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql);
    }

    public async Task<int> InsertLeagueAsync(string name, string code)
    {
        const string sql = """
            INSERT INTO dbo.Leagues (Name, Code, Created)
            VALUES (@Name, @Code, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """;

        await using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql, new { Name = name, Code = code });
    }

    public async Task<int> InsertMatchAsync(int leagueId, string homeTeam, string awayTeam, DateTime kickoffUtc)
    {
        const string insertSql = """
            EXEC dbo.MatchDayPredictionsApi_CreateMatch
                @LeagueId = @LeagueId,
                @HomeTeam = @HomeTeam,
                @AwayTeam = @AwayTeam,
                @KickoffUtc = @KickoffUtc;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(insertSql, new { LeagueId = leagueId, HomeTeam = homeTeam, AwayTeam = awayTeam, KickoffUtc = kickoffUtc });

        const string selectSql = """
            SELECT MatchId
            FROM dbo.LeagueMatches
            WHERE LeagueId = @LeagueId
              AND HomeTeam = @HomeTeam
              AND AwayTeam = @AwayTeam
              AND KickoffUtc = @KickoffUtc;
            """;

        return await connection.ExecuteScalarAsync<int>(selectSql, new { LeagueId = leagueId, HomeTeam = homeTeam, AwayTeam = awayTeam, KickoffUtc = kickoffUtc });
    }

    public async Task<int> GetMatchIdAsync(int leagueId, string homeTeam, string awayTeam, DateTime kickoffUtc)
    {
        const string sql = """
            SELECT MatchId
            FROM dbo.LeagueMatches
            WHERE LeagueId = @LeagueId
              AND HomeTeam = @HomeTeam
              AND AwayTeam = @AwayTeam
              AND KickoffUtc = @KickoffUtc;
            """;

        await using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql, new { LeagueId = leagueId, HomeTeam = homeTeam, AwayTeam = awayTeam, KickoffUtc = kickoffUtc });
    }

    public async Task<int> GetUserIdAsync(string username)
    {
        const string sql = "SELECT Id FROM dbo.Users WHERE UserName = @Username;";

        await using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
    }
}
