using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for managing match predictions.
    /// </summary>
    public interface IPredictionRepository
    {
        /// <summary>
        /// Adds or updates a match prediction.
        /// </summary>
        Task AddPredictionAsync(CreatePredictionRequest request);

        /// <summary>
        /// Gets a prediction for a specific match and user.
        /// </summary>
        Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId);
    }
}
