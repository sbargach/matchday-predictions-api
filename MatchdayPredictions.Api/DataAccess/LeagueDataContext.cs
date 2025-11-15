using System.Data;
using Dapper;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MatchdayPredictions.Api.DataAccess;

public sealed class LeagueDataContext : SqlDataContextBase, ILeagueDataContext
{
    public LeagueDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        IMetricsProvider metrics,
        ILogger<LeagueDataContext> logger)
        : base(configuration, settings, metrics, logger)
    {
    }

    public Task<IEnumerable<League>> GetLeaguesAsync()
    {
        return QueryListAsync(
            "MatchDayPredictionsApi_GetLeagues",
            conn => conn.QueryAsync<League>(
                "MatchDayPredictionsApi_GetLeagues",
                commandType: CommandType.StoredProcedure));
    }

    public Task<League?> GetLeagueByIdAsync(int leagueId)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetLeagueById",
            conn => conn.QuerySingleOrDefaultAsync<League>(
                "MatchDayPredictionsApi_GetLeagueById",
                new { LeagueId = leagueId },
                commandType: CommandType.StoredProcedure));
    }
}

