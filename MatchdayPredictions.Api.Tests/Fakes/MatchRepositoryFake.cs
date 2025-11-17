using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Tests.Fakes;

internal sealed class FakeMatchRepository : IMatchRepository
{
    public Match? MatchToReturn { get; set; }
    public IEnumerable<Match> MatchesToReturn { get; set; } = Array.Empty<Match>();

    public bool ThrowOnGetById { get; set; }
    public bool ThrowOnGetByLeague { get; set; }
    public bool ThrowOnCreate { get; set; }

    public int GetByIdCallCount { get; private set; }
    public int GetByLeagueCallCount { get; private set; }
    public int CreateCallCount { get; private set; }

    public List<int> GetByIdArguments { get; } = new();
    public List<int> GetByLeagueArguments { get; } = new();

    public Task<Match?> GetByIdAsync(int matchId)
    {
        GetByIdCallCount++;
        GetByIdArguments.Add(matchId);

        ThrowIf(ThrowOnGetById, nameof(GetByIdAsync));

        return Task.FromResult(MatchToReturn);
    }

    public Task<IEnumerable<Match>> GetByLeagueAsync(int leagueId)
    {
        GetByLeagueCallCount++;
        GetByLeagueArguments.Add(leagueId);

        ThrowIf(ThrowOnGetByLeague, nameof(GetByLeagueAsync));

        return Task.FromResult(MatchesToReturn);
    }

    public Task CreateAsync(CreateMatchRequest request)
    {
        CreateCallCount++;

        ThrowIf(ThrowOnCreate, nameof(CreateAsync));

        return Task.CompletedTask;
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
