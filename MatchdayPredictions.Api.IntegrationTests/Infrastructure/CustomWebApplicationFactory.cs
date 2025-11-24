using System.Collections.Generic;
using System.Globalization;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly LoginRateLimitSettings? _loginRateLimitSettings;

    public CustomWebApplicationFactory(
        string connectionString,
        string jwtKey,
        string jwtIssuer,
        string jwtAudience,
        LoginRateLimitSettings? loginRateLimitSettings = null)
    {
        _connectionString = connectionString;
        _jwtKey = jwtKey;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
        _loginRateLimitSettings = loginRateLimitSettings;
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

            if (_loginRateLimitSettings is not null)
            {
                overrides["RateLimiting:Login:PermitLimit"] = _loginRateLimitSettings.PermitLimit.ToString(CultureInfo.InvariantCulture);
                overrides["RateLimiting:Login:QueueLimit"] = _loginRateLimitSettings.QueueLimit.ToString(CultureInfo.InvariantCulture);
                overrides["RateLimiting:Login:WindowSeconds"] = _loginRateLimitSettings.WindowSeconds.ToString(CultureInfo.InvariantCulture);
            }

            configBuilder.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMetricsProvider>();
            services.RemoveAll<TracerProvider>();
            services.RemoveAll<MeterProvider>();
            services.AddSingleton<IMetricsProvider, NoopMetricsProvider>();
            services.AddSingleton(_ => Sdk.CreateMeterProviderBuilder().Build());
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuer = false;
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                options.TokenValidationParameters.SignatureValidator = (token, parameters) => new JsonWebToken(token);
            });

        });
    }
}

public sealed record LoginRateLimitSettings(int PermitLimit, int QueueLimit, int WindowSeconds);
