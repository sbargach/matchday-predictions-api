using MatchdayPredictions.Api.DataAccess;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using Serilog.Events;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            Log.Information("Starting {Application} v{Version}",
                typeof(Program).Assembly.GetName().Name,
                typeof(Program).Assembly.GetName().Version?.ToString());

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            ConfigureServices(builder);

            var app = BuildApp(builder);

            ConfigureMiddleware(app);

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<MatchdayPredictionsSettings>(
            builder.Configuration.GetSection("MatchdayPredictions"));

        builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt"));

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value!.Errors.Count > 0)
                        .Select(e => new
                        {
                            Field = e.Key,
                            Errors = e.Value!.Errors.Select(err => err.ErrorMessage)
                        });

                    return new BadRequestObjectResult(new
                    {
                        Message = "Validation failed",
                        Errors = errors
                    });
                };
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMemoryCache();

        ConfigureJwt(builder);
        ConfigureOpenTelemetry(builder);

        builder.Services.AddScoped<IUserDataContext, UserDataContext>();
        builder.Services.AddScoped<ILeagueDataContext, LeagueDataContext>();
        builder.Services.AddScoped<IMatchDataContext, MatchDataContext>();
        builder.Services.AddScoped<IPredictionDataContext, PredictionDataContext>();

        builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
        builder.Services.AddScoped<LeagueRepository>();
        builder.Services.AddScoped<ILeagueRepository>(sp =>
        {
            var inner = sp.GetRequiredService<LeagueRepository>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            return new CachedLeagueRepository(inner, cache);
        });
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IMatchRepository, MatchRepository>();
    }

    private static void ConfigureJwt(WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetRequiredSection("Jwt");
        var jwtSettings = jwtSection.Get<JwtSettings>()
                          ?? throw new InvalidOperationException("Jwt configuration section is missing or invalid.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new InvalidOperationException("Jwt:Key must be configured.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);
        var key = new SymmetricSecurityKey(keyBytes);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();
    }

    private static void ConfigureOpenTelemetry(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IMetricsProvider, MetricsProvider>();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddRuntimeInstrumentation();
                metrics.AddMeter("MatchdayPredictions.Api");
            });
    }

    private static WebApplication BuildApp(WebApplicationBuilder builder)
    {
        return builder.Build();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();

        app.UseRouting();

        app.UseHttpMetrics();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapMetrics();
    }
}
