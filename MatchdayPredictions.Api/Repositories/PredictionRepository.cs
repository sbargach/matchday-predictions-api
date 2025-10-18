using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories
{
    /// <summary>
    /// Repository implementation for handling match predictions.
    /// </summary>
    public class PredictionRepository : IPredictionRepository
    {
        private readonly IMatchdayPredictionsDataContext _context;

        public PredictionRepository(IMatchdayPredictionsDataContext context)
        {
            _context = context;
        }

        public async Task AddPredictionAsync(CreatePredictionRequest request)
        {
            await _context.CreatePredictionAsync(request);
        }

        public async Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
        {
            return await _context.GetPredictionAsync(matchId, userId);
        }
    }
    }
