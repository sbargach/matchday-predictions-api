using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

/// <summary>
/// Defines stored procedure operations for the MatchdayPredictions database.
/// </summary>
public interface IMatchdayPredictionsDataContext
{
    /// <summary>
    /// Executes the stored procedure to insert or update a match prediction.
    /// </summary>
    /// <param name="request">The prediction data to create or update.</param>
    Task CreatePredictionAsync(CreatePredictionRequest request);

    /// <summary>
    /// Executes the stored procedure to retrieve a single prediction for a specific user and match.
    /// </summary>
    /// <param name="matchId">The match ID to search for.</param>
    /// <param name="userId">The user ID associated with the prediction.</param>
    /// <returns>The prediction if found, otherwise null.</returns>
    Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId);

    /// <summary>
    /// Executes the stored procedure to retrieve all leagues.
    /// </summary>
    /// <returns>A collection of all leagues in the system.</returns>
    Task<IEnumerable<League>> GetLeaguesAsync();

    /// <summary>
    /// Executes the stored procedure to retrieve a specific league by its ID.
    /// </summary>
    /// <param name="leagueId">The ID of the league to retrieve.</param>
    /// <returns>The league if found, otherwise null.</returns>
    Task<League?> GetLeagueByIdAsync(int leagueId);
}
