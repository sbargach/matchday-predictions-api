using MatchdayPredictions.Api.DataAccess;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.Repositories;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Serilog;
using Serilog.Events;

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
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Configure<MatchdayPredictionsSettings>(
            builder.Configuration.GetSection("MatchdayPredictions"));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<IMatchdayPredictionsDataContext, MatchdayPredictionsDataContext>();
        builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
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

        app.UseHttpsRedirection();

        app.MapControllers();
    }
}
