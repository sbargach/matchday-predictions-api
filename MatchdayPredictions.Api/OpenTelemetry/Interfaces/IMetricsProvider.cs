namespace MatchdayPredictions.Api.OpenTelemetry;

/// <summary>
/// Provides OpenTelemetry metrics for API performance, reliability and database health.
/// </summary>
public interface IMetricsProvider
{
    // Request-level metrics
    void IncrementRequest();
    void IncrementRequestSuccess();
    void IncrementRequestFailure();
    void IncrementClientError();   // 4xx responses
    void IncrementServerError();   // 5xx responses

    // Database failure metrics
    void IncrementDatabaseFailure();

    // Latency
    void RecordRequestDuration(double milliseconds);
    void RecordDatabaseQueryDuration(double milliseconds);
}
