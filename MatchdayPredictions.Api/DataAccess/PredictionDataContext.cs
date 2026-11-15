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

    public Task<PredictionWriteResult> CreatePredictionAsync(CreatePredictionRequest request)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_SetPrediction",
            async (conn, timeout) =>
            {
                var status = await conn.QuerySingleAsync<string>(
                    "MatchDayPredictionsApi_SetPrediction",
                    new
                    {
                        request.MatchId,
                        request.UserId,
                        request.HomeGoals,
                        request.AwayGoals
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: timeout);

                return status switch
                {
                    "Created" => PredictionWriteResult.Created,
                    "Updated" => PredictionWriteResult.Updated,
                    "MatchNotFound" => PredictionWriteResult.MatchNotFound,
                    "MatchStarted" => PredictionWriteResult.MatchStarted,
                    "NotLeagueMember" => PredictionWriteResult.NotLeagueMember,
                    _ => throw new InvalidOperationException($"Unexpected prediction result: {status}.")
                };
            });
    }

    public Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetPrediction",
            (conn, timeout) => conn.QuerySingleOrDefaultAsync<MatchPrediction>(
                "MatchDayPredictionsApi_GetPrediction",
                new { MatchId = matchId, UserId = userId },
                commandType: CommandType.StoredProcedure,
                commandTimeout: timeout));
    }
}
