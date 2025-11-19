using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Tests.Fakes;

internal sealed class FakePredictionRepository : IPredictionRepository
{
    public bool AddPredictionCalled { get; private set; }
    public bool GetPredictionCalled { get; private set; }
    public CreatePredictionRequest? LastRequest { get; private set; }
    public MatchPrediction? PredictionToReturn { get; set; }

    public Task AddPredictionAsync(CreatePredictionRequest request)
    {
        AddPredictionCalled = true;
        LastRequest = request;
        return Task.CompletedTask;
    }

    public Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
    {
        GetPredictionCalled = true;
        return Task.FromResult(PredictionToReturn);
    }
}
