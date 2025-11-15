using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.Repositories.Interfaces;

/// <summary>
/// Defines data operations for football leagues.
/// </summary>
public interface ILeagueRepository
{
    Task<IEnumerable<League>> GetAllAsync();

    Task<League?> GetByIdAsync(int leagueId);
}

