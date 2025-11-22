using System.Collections.Generic;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public CustomWebApplicationFactory(
        string connectionString,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience)
    {
        _connectionString = connectionString;
        _jwtKey = jwtKey;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:matchdaypredictions"] = _connectionString,
                ["Jwt:Key"] = _jwtKey,
                ["Jwt:Issuer"] = _jwtIssuer,
                ["Jwt:Audience"] = _jwtAudience,
                ["Jwt:TokenHours"] = "2",
                ["MatchdayPredictions:MaxRetryCount"] = "1",
                ["MatchdayPredictions:RetryDelaySeconds"] = "0"
            };

            configBuilder.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMetricsProvider>();
            services.RemoveAll<TracerProvider>();
            services.RemoveAll<MeterProvider>();
            services.AddSingleton<IMetricsProvider, NoopMetricsProvider>();
        });
    }
}
