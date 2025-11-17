using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace MatchdayPredictions.Api.Tests.Fakes;

internal sealed class FakeMetricsProvider : IMetricsProvider
{
    public int RequestCount { get; private set; }
    public int SuccessCount { get; private set; }
    public int FailureCount { get; private set; }
    public int ClientErrorCount { get; private set; }
    public int ServerErrorCount { get; private set; }

    public void IncrementRequest() => RequestCount++;
    public void IncrementRequestSuccess() => SuccessCount++;
    public void IncrementRequestFailure() => FailureCount++;
    public void IncrementClientError() => ClientErrorCount++;
    public void IncrementServerError() => ServerErrorCount++;
    public void IncrementDatabaseFailure() { }
    public void RecordRequestDuration(double milliseconds) { }
    public void RecordDatabaseQueryDuration(double milliseconds) { }
}

internal sealed class FakeLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}

