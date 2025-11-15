using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

public interface IPredictionDataContext
{
    Task CreatePredictionAsync(CreatePredictionRequest request);

    Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId);
}

