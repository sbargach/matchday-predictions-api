using System.Data;
using Dapper;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace MatchdayPredictions.Api.DataAccess;

/// <summary>
/// Base class for SQL data contexts that encapsulates connection handling,
/// retry logic and metrics.
/// </summary>
public abstract class SqlDataContextBase
{
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger _logger;

    protected string ConnectionString { get; }
    protected IMetricsProvider Metrics { get; }

    protected SqlDataContextBase(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        IMetricsProvider metrics,
        ILogger logger)
    {
        _logger = logger;
        Metrics = metrics;

        ConnectionString = configuration.GetConnectionString("matchdaypredictions")
                            ?? throw new InvalidOperationException("Missing connection string.");

        _retryPolicy = CreateRetryPolicy(settings.Value);
    }

    protected Task ExecuteAsync(string procName, Func<SqlConnection, Task> operation)
        => _retryPolicy.ExecuteAsync(() => ExecuteInternalAsync(procName, async conn =>
        {
            await operation(conn);
            return 0;
        }));

    protected Task<T?> QueryAsync<T>(string procName, Func<SqlConnection, Task<T?>> operation)
        => _retryPolicy.ExecuteAsync(() => ExecuteInternalAsync(procName, operation));

    protected Task<IEnumerable<T>> QueryListAsync<T>(string procName, Func<SqlConnection, Task<IEnumerable<T>>> operation)
        => _retryPolicy.ExecuteAsync(() => ExecuteInternalAsync(procName, operation));

    private async Task<T> ExecuteInternalAsync<T>(string procName, Func<SqlConnection, Task<T>> operation)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await using var connection = new SqlConnection(ConnectionString);
            return await operation(connection);
        }
        catch (SqlException ex)
        {
            Metrics.IncrementDatabaseFailure();
            _logger.LogError(ex, "SQL exception executing stored procedure {Proc}", procName);
            throw;
        }
        finally
        {
            Metrics.RecordDatabaseQueryDuration(sw.Elapsed.TotalMilliseconds);
        }
    }

    private AsyncRetryPolicy CreateRetryPolicy(MatchdayPredictionsSettings settings)
    {
        return Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: settings.MaxRetryCount,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(settings.RetryDelaySeconds * attempt),
                onRetry: (ex, delay, attempt, _) =>
                    _logger.LogWarning(
                        "SQL retry {Attempt}/{MaxRetries} after {Delay}s. Exception: {Exception}",
                        attempt,
                        settings.MaxRetryCount,
                        delay.TotalSeconds,
                        ex));
    }
}

