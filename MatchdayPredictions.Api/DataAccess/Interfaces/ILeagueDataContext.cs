using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

public interface ILeagueDataContext
{
    Task<IEnumerable<League>> GetLeaguesAsync();

    Task<League?> GetLeagueByIdAsync(int leagueId);
}

