using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.DataAccess.Repository;

/// <summary>
/// Repository for match-related data operations.
/// Provides a clean abstraction over the data context.
///
/// Methods do not contain business logic and only delegate
/// to the underlying data context.
/// </summary>
public sealed class MatchRepository : IMatchRepository
{
    private readonly IMatchdayPredictionsDataContext _context;

    public MatchRepository(IMatchdayPredictionsDataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a match by its unique ID.
    /// </summary>
    public Task<Match?> GetByIdAsync(int matchId)
    {
        return _context.GetMatchByIdAsync(matchId);
    }

    /// <summary>
    /// Retrieves all matches within a specific league.
    /// </summary>
    public Task<IEnumerable<Match>> GetByLeagueAsync(int leagueId)
    {
        return _context.GetMatchesByLeagueAsync(leagueId);
    }

    /// <summary>
    /// Creates a new match.
    /// </summary>
    public Task CreateAsync(CreateMatchRequest request)
    {
        return _context.CreateMatchAsync(request);
    }
}
