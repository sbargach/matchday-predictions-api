namespace MatchdayPredictions.Api.Models;

/// <summary>
/// Represents a football match with participating teams,
/// league information and kickoff time.
/// </summary>
public sealed record Match
{
    /// <summary>
    /// The unique identifier of the match.
    /// </summary>
    public int MatchId { get; init; }

    /// <summary>
    /// The league this match belongs to.
    /// </summary>
    public int LeagueId { get; init; }

    /// <summary>
    /// The name of the home team.
    /// </summary>
    public string HomeTeam { get; init; } = string.Empty;

    /// <summary>
    /// The name of the away team.
    /// </summary>
    public string AwayTeam { get; init; } = string.Empty;

    /// <summary>
    /// The kickoff time in UTC.
    /// </summary>
    public DateTime KickoffUtc { get; init; }
}
