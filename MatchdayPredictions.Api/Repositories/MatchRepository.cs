using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories;

/// <summary>
/// Repository for match-related data operations.
/// </summary>
public sealed class MatchRepository : IMatchRepository
{
    private readonly IMatchDataContext _context;

    public MatchRepository(IMatchDataContext context)
    {
        _context = context;
    }

    public Task<Match?> GetByIdAsync(int matchId)
        => _context.GetMatchByIdAsync(matchId);

    public Task<IEnumerable<Match>> GetByLeagueAsync(int leagueId)
        => _context.GetMatchesByLeagueAsync(leagueId);

    public Task CreateAsync(CreateMatchRequest request)
        => _context.CreateMatchAsync(request);
}

