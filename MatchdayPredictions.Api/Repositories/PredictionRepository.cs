using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories;

/// <summary>
/// Repository implementation for handling match predictions.
/// </summary>
public class PredictionRepository : IPredictionRepository
{
    private readonly IPredictionDataContext _context;

    public PredictionRepository(IPredictionDataContext context)
    {
        _context = context;
    }

    public Task AddPredictionAsync(CreatePredictionRequest request)
        => _context.CreatePredictionAsync(request);

    public Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
        => _context.GetPredictionAsync(matchId, userId);
}

