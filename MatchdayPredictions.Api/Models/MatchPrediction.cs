using System.Text.Json.Serialization;

namespace MatchdayPredictions.Api.Models;

/// <summary>
/// Represents a prediction for a given football match.
/// </summary>
public sealed record MatchPrediction
{
    /// <summary>
    /// Unique match identifier.
    /// </summary>
    [JsonPropertyName("matchId")]
    public int MatchId { get; init; }

    /// <summary>
    /// Name of the home team.
    /// </summary>
    [JsonPropertyName("homeTeam")]
    public required string HomeTeam { get; init; }

    /// <summary>
    /// Name of the away team.
    /// </summary>
    [JsonPropertyName("awayTeam")]
    public required string AwayTeam { get; init; }

    /// <summary>
    /// Predicted goals for the home team (nullable until a prediction is made).
    /// </summary>
    [JsonPropertyName("homeGoals")]
    public int? HomeGoals { get; init; }

    /// <summary>
    /// Predicted goals for the away team (nullable until a prediction is made).
    /// </summary>
    [JsonPropertyName("awayGoals")]
    public int? AwayGoals { get; init; }

    /// <summary>
    /// Scheduled kickoff in UTC.
    /// </summary>
    [JsonPropertyName("kickoffUtc")]
    public DateTime KickoffUtc { get; init; }
}
