using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Repository;

/// <summary>
/// Defines data operations for football leagues.
/// </summary>
public interface ILeagueRepository
{
    /// <summary>
    /// Retrieves all leagues.
    /// </summary>
    /// <returns>A collection of all leagues.</returns>
    Task<IEnumerable<League>> GetAllAsync();

    /// <summary>
    /// Retrieves a specific league by its ID.
    /// </summary>
    /// <param name="leagueId">The league ID.</param>
    /// <returns>The league if found, otherwise null.</returns>
    Task<League?> GetByIdAsync(int leagueId);
}
