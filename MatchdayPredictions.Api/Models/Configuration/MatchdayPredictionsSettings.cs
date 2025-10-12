namespace MatchdayPredictions.Api.Models.Configuration;

/// <summary>
/// Global configuration settings for the MatchdayPredictions API.
/// This single class can hold any application-wide setting, such as database, cache, or API configuration.
/// </summary>
public sealed class MatchdayPredictionsSettings
{
    /// <summary>
    /// The maximum number of retry attempts for transient SQL errors (e.g., timeouts, deadlocks).
    /// Default: 3.
    /// </summary>
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>
    /// The base delay (in seconds) before retrying a failed SQL operation.
    /// Each retry increases the delay linearly (e.g., 2s, 4s, 6s).
    /// Default: 2 seconds.
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 2;
}
