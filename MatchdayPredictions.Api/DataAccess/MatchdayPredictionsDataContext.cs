using System.Data;
using Dapper;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace MatchdayPredictions.Api.DataAccess;

public sealed class MatchdayPredictionsDataContext : IMatchdayPredictionsDataContext
{
    private readonly string _connectionString;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<MatchdayPredictionsDataContext> _logger;
    private readonly IMetricsProvider _metrics;

    public MatchdayPredictionsDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        ILogger<MatchdayPredictionsDataContext> logger,
        IMetricsProvider metrics)
    {
        _logger = logger;
        _metrics = metrics;

        _connectionString = configuration.GetConnectionString("matchdaypredictions") ?? throw new InvalidOperationException("Missing connection string.");

        _retryPolicy = CreateRetryPolicy(settings.Value);
    }

    private async Task<T?> ExecuteDbOperationAsync<T>(string procName, Func<SqlConnection, Task<T?>> operation)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            return await operation(connection);
        }
        catch (SqlException ex)
        {
            _metrics.IncrementDatabaseFailure();
            _logger.LogError(ex, "SQL exception executing stored procedure {Proc}", procName);
            throw;
        }
        finally
        {
            _metrics.RecordDatabaseQueryDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    public async Task CreatePredictionAsync(CreatePredictionRequest request)
    {
        await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_SetPrediction",
                conn => conn.ExecuteAsync(
                    "MatchDayPredictionsApi_SetPrediction",
                    new
                    {
                        request.MatchId,
                        request.UserId,
                        request.HomeGoals,
                        request.AwayGoals
                    },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetPrediction",
                conn => conn.QuerySingleOrDefaultAsync<MatchPrediction>(
                    "MatchDayPredictionsApi_GetPrediction",
                    new { MatchId = matchId, UserId = userId },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<IEnumerable<League>> GetLeaguesAsync()
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetLeagues",
                conn => conn.QueryAsync<League>(
                    "MatchDayPredictionsApi_GetLeagues",
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<League?> GetLeagueByIdAsync(int leagueId)
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetLeagueById",
                conn => conn.QuerySingleOrDefaultAsync<League>(
                    "MatchDayPredictionsApi_GetLeagueById",
                    new { LeagueId = leagueId },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_CreateUser",
                conn => conn.ExecuteAsync(
                    "MatchDayPredictionsApi_CreateUser",
                    new
                    {
                        request.Username,
                        request.Email
                    },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetUserById",
                conn => conn.QuerySingleOrDefaultAsync<User>(
                    "MatchDayPredictionsApi_GetUserById",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<Match?> GetMatchByIdAsync(int matchId)
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetMatchById",
                conn => conn.QuerySingleOrDefaultAsync<Match>(
                    "MatchDayPredictionsApi_GetMatchById",
                    new { MatchId = matchId },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task<IEnumerable<Match>> GetMatchesByLeagueAsync(int leagueId)
    {
        return await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_GetMatchesByLeague",
                conn => conn.QueryAsync<Match>(
                    "MatchDayPredictionsApi_GetMatchesByLeague",
                    new { LeagueId = leagueId },
                    commandType: CommandType.StoredProcedure)));
    }

    public async Task CreateMatchAsync(CreateMatchRequest request)
    {
        await _retryPolicy.ExecuteAsync(() =>
            ExecuteDbOperationAsync(
                "MatchDayPredictionsApi_CreateMatch",
                conn => conn.ExecuteAsync(
                    "MatchDayPredictionsApi_CreateMatch",
                    new
                    {
                        request.LeagueId,
                        request.HomeTeam,
                        request.AwayTeam,
                        request.KickoffUtc
                    },
                    commandType: CommandType.StoredProcedure)));
    }

    private AsyncRetryPolicy CreateRetryPolicy(MatchdayPredictionsSettings settings)
    {
        return Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: settings.MaxRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(settings.RetryDelaySeconds * attempt),
                onRetry: (ex, delay, attempt, context) =>
                    _logger.LogWarning(
                        "SQL retry {Attempt}/{MaxRetries} after {Delay}s. Exception: {Exception}",
                        attempt,
                        settings.MaxRetryCount,
                        delay.TotalSeconds,
                        ex));
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<User>(
                    "MatchDayPredictionsApi_GetUserByUsername",
                    new { Username = username },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception executing stored procedure {Proc}",
                    "MatchDayPredictionsApi_GetUserByUsername");
                throw;
            }
        });
    }

}
