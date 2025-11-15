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

public sealed class MatchDataContext : SqlDataContextBase, IMatchDataContext
{
    public MatchDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        IMetricsProvider metrics,
        ILogger<MatchDataContext> logger)
        : base(configuration, settings, metrics, logger)
    {
    }

    public Task<Match?> GetMatchByIdAsync(int matchId)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetMatchById",
            conn => conn.QuerySingleOrDefaultAsync<Match>(
                "MatchDayPredictionsApi_GetMatchById",
                new { MatchId = matchId },
                commandType: CommandType.StoredProcedure));
    }

    public Task<IEnumerable<Match>> GetMatchesByLeagueAsync(int leagueId)
    {
        return QueryListAsync(
            "MatchDayPredictionsApi_GetMatchesByLeague",
            conn => conn.QueryAsync<Match>(
                "MatchDayPredictionsApi_GetMatchesByLeague",
                new { LeagueId = leagueId },
                commandType: CommandType.StoredProcedure));
    }

    public Task CreateMatchAsync(CreateMatchRequest request)
    {
        return ExecuteAsync(
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
                commandType: CommandType.StoredProcedure));
    }
}

