using MatchdayPredictions.Api.Models;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

/// <summary>
/// Defines stored procedure operations for the MatchdayPredictions database.
/// </summary>
public interface IMatchdayPredictionsDataContext
{
    /// <summary>
    /// Executes the stored procedure to insert or update a match prediction.
    /// </summary>
    Task CreatePredictionAsync(CreatePredictionRequest request);

    /// <summary>
    /// Executes the stored procedure to retrieve a single prediction for a user and match.
    /// </summary>
    Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId);
}