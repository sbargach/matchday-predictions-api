using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Tests.Fakes;

internal sealed class FakeLeagueRepository : ILeagueRepository
{
    public int GetAllCallCount { get; private set; }
    public int GetByIdCallCount { get; private set; }
    public List<int> GetByIdArguments { get; } = new();

    public IEnumerable<League> LeaguesToReturn { get; set; } = Array.Empty<League>();
    public League? LeagueToReturn { get; set; }

    public bool ThrowOnGetAll { get; set; }
    public bool ThrowOnGetById { get; set; }

    public Task<IEnumerable<League>> GetAllAsync()
    {
        GetAllCallCount++;

        ThrowIf(ThrowOnGetAll, nameof(GetAllAsync));

        return Task.FromResult(LeaguesToReturn);
    }

    public Task<League?> GetByIdAsync(int leagueId)
    {
        GetByIdCallCount++;
        GetByIdArguments.Add(leagueId);

        ThrowIf(ThrowOnGetById, nameof(GetByIdAsync));

        return Task.FromResult(LeagueToReturn);
    }

    private static void ThrowIf(bool condition, string operationName)
    {
        if (!condition)
        {
            return;
        }

        throw new InvalidOperationException($"{operationName} failure for testing.");
    }
}
