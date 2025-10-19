using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Repository;

/// <summary>
/// Handles stored procedure calls for league data.
/// </summary>
public class LeagueRepository : ILeagueRepository
{
    private readonly IMatchdayPredictionsDataContext _context;

    public LeagueRepository(IMatchdayPredictionsDataContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<League>> GetAllAsync()
        => await _context.GetLeaguesAsync();

    /// <inheritdoc />
    public async Task<League?> GetByIdAsync(int leagueId)
        => await _context.GetLeagueByIdAsync(leagueId);
}
