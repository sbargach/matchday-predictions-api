using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.Repositories.Interfaces
{
    /// <summary>
    /// Provides data access operations for match entities.
    /// </summary>
    public interface IMatchRepository
    {
        /// <summary>Retrieves a match by its unique identifier.</summary>
        Task<Match?> GetByIdAsync(int matchId);

        /// <summary>Retrieves all matches within a specific league.</summary>
        Task<IEnumerable<Match>> GetByLeagueAsync(int leagueId);

        /// <summary>Creates a new match.</summary>
        Task CreateAsync(CreateMatchRequest request);
    }
}
