using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

/// <summary>
/// Defines stored procedure operations for the MatchdayPredictions database.
/// </summary>
public interface IMatchdayPredictionsDataContext
{
    // ----------------------------
    // Predictions
    // ----------------------------

    /// <summary>
    /// Executes the stored procedure to insert or update a match prediction.
    /// </summary>
    /// <param name="request">The prediction data to create or update.</param>
    Task CreatePredictionAsync(CreatePredictionRequest request);

    /// <summary>
    /// Executes the stored procedure to retrieve a prediction for a specific match and user.
    /// </summary>
    /// <param name="matchId">The match ID to search for.</param>
    /// <param name="userId">The user ID associated with the prediction.</param>
    /// <returns>The prediction if found; otherwise null.</returns>
    Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId);


    // ----------------------------
    // Leagues
    // ----------------------------

    /// <summary>
    /// Executes the stored procedure to retrieve all leagues.
    /// </summary>
    /// <returns>A collection of all leagues.</returns>
    Task<IEnumerable<League>> GetLeaguesAsync();

    /// <summary>
    /// Executes the stored procedure to retrieve a single league by its unique identifier.
    /// </summary>
    /// <param name="leagueId">The ID of the league to retrieve.</param>
    /// <returns>The league if found; otherwise null.</returns>
    Task<League?> GetLeagueByIdAsync(int leagueId);


    // ----------------------------
    // Users
    // ----------------------------

    /// <summary>
    /// Executes the stored procedure to create a new user.
    /// </summary>
    /// <param name="request">The user details required for creation.</param>
    Task CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Executes the stored procedure to retrieve a user by their unique ID.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve.</param>
    /// <returns>The user if found; otherwise null.</returns>
    Task<User?> GetUserByIdAsync(int userId);


    // ----------------------------
    // Matches
    // ----------------------------

    /// <summary>
    /// Executes the stored procedure to retrieve a match by its unique ID.
    /// </summary>
    /// <param name="matchId">The match ID.</param>
    /// <returns>The match if found; otherwise null.</returns>
    Task<Match?> GetMatchByIdAsync(int matchId);

    /// <summary>
    /// Executes the stored procedure to retrieve all matches belonging to a specific league.
    /// </summary>
    /// <param name="leagueId">The league ID.</param>
    /// <returns>A collection of matches for the specified league.</returns>
    Task<IEnumerable<Match>> GetMatchesByLeagueAsync(int leagueId);

    /// <summary>
    /// Executes the stored procedure to create a new match.
    /// </summary>
    /// <param name="request">The match details to insert.</param>
    Task CreateMatchAsync(CreateMatchRequest request);
}
