using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories;

/// <summary>
/// Handles league data access through the league data context.
/// </summary>
public class LeagueRepository : ILeagueRepository
{
    private readonly ILeagueDataContext _context;

    public LeagueRepository(ILeagueDataContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<League>> GetAllAsync()
        => _context.GetLeaguesAsync();

    public Task<League?> GetByIdAsync(int leagueId)
        => _context.GetLeagueByIdAsync(leagueId);
}

