using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;

namespace MatchdayPredictions.Api.Tests.Repositories;

[TestClass]
public class CachedLeagueRepositoryTests
{
    [TestMethod]
    public async Task GetAllAsync_UsesCacheOnSubsequentCalls()
    {
        const int leagueId = 1;

        var inner = new FakeLeagueRepository
        {
            LeaguesToReturn = new[]
            {
                new League
                {
                    LeagueId = leagueId,
                    Name = "Test League",
                    Country = "Testland"
                }
            }
        };

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var repository = new CachedLeagueRepository(inner, cache);

        var firstResult = await repository.GetAllAsync();
        var cachedResult = await repository.GetAllAsync();

        firstResult.ShouldNotBeNull();
        cachedResult.ShouldBeSameAs(firstResult);
        inner.GetAllCallCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task GetByIdAsync_CachesPerLeagueId()
    {
        const int firstLeagueId = 1;
        const int secondLeagueId = 2;

        var inner = new FakeLeagueRepository
        {
            LeagueToReturn = new League
            {
                LeagueId = firstLeagueId,
                Name = "Test League",
                Country = "Testland"
            }
        };

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var repository = new CachedLeagueRepository(inner, cache);

        var firstResult = await repository.GetByIdAsync(firstLeagueId);
        var cachedResult = await repository.GetByIdAsync(firstLeagueId);
        var otherLeagueResult = await repository.GetByIdAsync(secondLeagueId);

        firstResult.ShouldNotBeNull();
        cachedResult.ShouldBeSameAs(firstResult);
        otherLeagueResult.ShouldNotBeNull();

        inner.GetByIdCallCount.ShouldBe(2);
        inner.GetByIdArguments.ShouldBe(new[] { firstLeagueId, secondLeagueId });
    }
}
