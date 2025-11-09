using System.Diagnostics.Metrics;

namespace MatchdayPredictions.Api.OpenTelemetry;

/// <summary>
/// Default OpenTelemetry metrics provider used to track request flow,
/// reliability indicators and database performance.
/// </summary>
public sealed class MetricsProvider : IMetricsProvider
{
    private static readonly Meter Meter = new("MatchdayPredictions.Api", "1.0.0");

    // Request counters
    private readonly Counter<int> _requestsTotal;
    private readonly Counter<int> _requestsSuccess;
    private readonly Counter<int> _requestsFailed;
    private readonly Counter<int> _requestsClientError;
    private readonly Counter<int> _requestsServerError;

    // Database
    private readonly Counter<int> _databaseFailures;

    // Histograms
    private readonly Histogram<double> _requestDuration;
    private readonly Histogram<double> _dbQueryDuration;

    public MetricsProvider()
    {
        _requestsTotal = Meter.CreateCounter<int>(
            "requests_total",
            description: "Total number of incoming API requests.");

        _requestsSuccess = Meter.CreateCounter<int>(
            "requests_success_total",
            description: "Number of successful API responses.");

        _requestsFailed = Meter.CreateCounter<int>(
            "requests_failed_total",
            description: "Number of failed API responses.");

        _requestsClientError = Meter.CreateCounter<int>(
            "requests_4xx_total",
            description: "Number of client error responses (4xx).");

        _requestsServerError = Meter.CreateCounter<int>(
            "requests_5xx_total",
            description: "Number of server error responses (5xx).");

        _databaseFailures = Meter.CreateCounter<int>(
            "database_failures_total",
            description: "Number of failed database operations.");

        _requestDuration = Meter.CreateHistogram<double>(
            "request_duration_ms",
            unit: "ms",
            description: "Duration of HTTP requests in milliseconds.");

        _dbQueryDuration = Meter.CreateHistogram<double>(
            "db_query_duration_ms",
            unit: "ms",
            description: "Duration of database query execution.");
    }

    public void IncrementRequest() => _requestsTotal.Add(1);
    public void IncrementRequestSuccess() => _requestsSuccess.Add(1);
    public void IncrementRequestFailure() => _requestsFailed.Add(1);
    public void IncrementClientError() => _requestsClientError.Add(1);
    public void IncrementServerError() => _requestsServerError.Add(1);
    public void IncrementDatabaseFailure() => _databaseFailures.Add(1);

    public void RecordRequestDuration(double milliseconds)
    {
        _requestDuration.Record(milliseconds);
    }

    public void RecordDatabaseQueryDuration(double milliseconds)
    {
        _dbQueryDuration.Record(milliseconds);
    }
}
