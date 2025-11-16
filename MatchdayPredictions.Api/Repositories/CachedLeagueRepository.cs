using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MatchdayPredictions.Api.Repositories;

/// <summary>
/// Caching decorator for <see cref="ILeagueRepository"/> that caches
/// league queries in memory to reduce database load.
/// </summary>
public sealed class CachedLeagueRepository : ILeagueRepository
{
    private readonly ILeagueRepository _inner;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan AllLeaguesCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SingleLeagueCacheDuration = TimeSpan.FromMinutes(5);

    public CachedLeagueRepository(ILeagueRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public Task<IEnumerable<League>> GetAllAsync()
    {
        return _cache.GetOrCreateAsync(
            "leagues:all",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = AllLeaguesCacheDuration;
                return await _inner.GetAllAsync();
            })!;
    }

    public Task<League?> GetByIdAsync(int leagueId)
    {
        var cacheKey = $"leagues:{leagueId}";

        return _cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SingleLeagueCacheDuration;
                return await _inner.GetByIdAsync(leagueId);
            })!;
    }
}

