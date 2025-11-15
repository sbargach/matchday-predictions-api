using System.Data;
using Dapper;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MatchdayPredictions.Api.DataAccess;

public sealed class PredictionDataContext : SqlDataContextBase, IPredictionDataContext
{
    public PredictionDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        IMetricsProvider metrics,
        ILogger<PredictionDataContext> logger)
        : base(configuration, settings, metrics, logger)
    {
    }

    public Task CreatePredictionAsync(CreatePredictionRequest request)
    {
        return ExecuteAsync(
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
                commandType: CommandType.StoredProcedure));
    }

    public Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetPrediction",
            conn => conn.QuerySingleOrDefaultAsync<MatchPrediction>(
                "MatchDayPredictionsApi_GetPrediction",
                new { MatchId = matchId, UserId = userId },
                commandType: CommandType.StoredProcedure));
    }
}

