using System.Data;
using Dapper;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace MatchdayPredictions.Api.DataAccess;

public class MatchdayPredictionsDataContext : IMatchdayPredictionsDataContext
{
    private readonly string _connectionString;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<MatchdayPredictionsDataContext> _logger;

    public MatchdayPredictionsDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        ILogger<MatchdayPredictionsDataContext> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string.");
        _retryPolicy = CreateRetryPolicy(settings.Value);
    }

    public async Task CreatePredictionAsync(CreatePredictionRequest request)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "MatchDayPredictionsApi_SetPrediction",
                    new
                    {
                        request.MatchId,
                        request.UserId,
                        request.HomeGoals,
                        request.AwayGoals
                    },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception executing stored procedure {Proc}",
                    "MatchDayPredictionsApi_SetPrediction");
                throw;
            }
        });
    }

    public async Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<MatchPrediction>(
                    "MatchDayPredictionsApi_GetPrediction",
                    new { MatchId = matchId, UserId = userId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception executing stored procedure {Proc}",
                    "MatchDayPredictionsApi_GetPrediction");
                throw;
            }
        });
    }

    public async Task<IEnumerable<League>> GetLeaguesAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                return await connection.QueryAsync<League>(
                    "MatchDayPredictionsApi_GetLeagues",
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception executing stored procedure {Proc}",
                    "MatchDayPredictionsApi_GetLeagues");
                throw;
            }
        });
    }

    public async Task<League?> GetLeagueByIdAsync(int leagueId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                return await connection.QuerySingleOrDefaultAsync<League>(
                    "MatchDayPredictionsApi_GetLeagueById",
                    new { LeagueId = leagueId },
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception executing stored procedure {Proc}",
                    "MatchDayPredictionsApi_GetLeagueById");
                throw;
            }
        });
    }

    private AsyncRetryPolicy CreateRetryPolicy(MatchdayPredictionsSettings settings)
    {
        return Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: settings.MaxRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(settings.RetryDelaySeconds * attempt),
                onRetry: (ex, delay, attempt, context) => OnRetry(ex, delay, attempt, context, settings.MaxRetryCount));
    }

    private void OnRetry(Exception exception, TimeSpan delay, int attempt, Context context, int maxRetries)
    {
        _logger.LogWarning(
            "SQL retry {Attempt}/{MaxRetries} after {Delay}s. Exception: {Exception}",
            attempt,
            maxRetries,
            delay.TotalSeconds,
            exception);
    }
}
