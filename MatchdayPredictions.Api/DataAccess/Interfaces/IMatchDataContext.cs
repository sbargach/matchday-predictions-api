using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

public interface IMatchDataContext
{
    Task<Match?> GetMatchByIdAsync(int matchId);

    Task<IEnumerable<Match>> GetMatchesByLeagueAsync(int leagueId);

    Task CreateMatchAsync(CreateMatchRequest request);
}

