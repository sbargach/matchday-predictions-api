using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MatchdayPredictions.Api.Models;

/// <summary>Request payload to create a prediction for a match.</summary>
public sealed record CreatePredictionRequest
{
    /// <summary>User identifier submitting the prediction.</summary>
    [Required, JsonPropertyName("userId")]
    public required int UserId { get; init; }

    /// <summary>Unique match identifier.</summary>
    [Range(1, int.MaxValue), JsonPropertyName("matchId")]
    public required int MatchId { get; init; }

    /// <summary>Home team name.</summary>
    [Required, JsonPropertyName("homeTeam")]
    public required string HomeTeam { get; init; }

    /// <summary>Away team name.</summary>
    [Required, JsonPropertyName("awayTeam")]
    public required string AwayTeam { get; init; }

    /// <summary>Predicted goals for the home team (0–20 guard).</summary>
    [Range(0, 20), JsonPropertyName("homeGoals")]
    public required int HomeGoals { get; init; }

    /// <summary>Predicted goals for the away team (0–20 guard).</summary>
    [Range(0, 20), JsonPropertyName("awayGoals")]
    public required int AwayGoals { get; init; }

    /// <summary>Scheduled kickoff time in UTC (used to lock predictions at kickoff).</summary>
    [JsonPropertyName("kickoffUtc")]
    public required DateTime KickoffUtc { get; init; }
}
