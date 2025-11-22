using MatchdayPredictions.Api.OpenTelemetry;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class NoopMetricsProvider : IMetricsProvider
{
    public void IncrementClientError() { }
    public void IncrementDatabaseFailure() { }
    public void IncrementRequest() { }
    public void IncrementRequestFailure() { }
    public void IncrementRequestSuccess() { }
    public void IncrementServerError() { }
    public void RecordDatabaseQueryDuration(double milliseconds) { }
    public void RecordRequestDuration(double milliseconds) { }
}
