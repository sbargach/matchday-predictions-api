using System.Collections.Concurrent;
using System.Text.Json;
using MatchdayPredictions.Api.Models.Api;
using Microsoft.Extensions.Options;

namespace MatchdayPredictions.Api.RateLimiting;

public sealed class LoginRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LoginRateLimitOptions _options;
    private readonly ConcurrentDictionary<string, WindowState> _states = new();

    public LoginRateLimitingMiddleware(RequestDelegate next, IOptions<LoginRateLimitOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var window = TimeSpan.FromSeconds(Math.Max(1, _options.WindowSeconds));
        var permitLimit = Math.Max(1, _options.PermitLimit);
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "global";
        var now = DateTime.UtcNow;

        var state = _states.GetOrAdd(partitionKey, _ => new WindowState(now, 0));

        bool allowed;
        lock (state.SyncRoot)
        {
            if (now - state.WindowStart >= window)
            {
                state.WindowStart = now;
                state.Count = 0;
            }

            allowed = state.Count < permitLimit;
            if (allowed)
            {
                state.Count++;
            }
        }

        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(
                ErrorResponse.FromMessage("Too many requests. Please retry shortly."));
            await context.Response.WriteAsync(payload);
            return;
        }

        await _next(context);
    }

    private sealed class WindowState
    {
        public WindowState(DateTime windowStart, int count)
        {
            WindowStart = windowStart;
            Count = count;
        }

        public object SyncRoot { get; } = new();
        public DateTime WindowStart { get; set; }
        public int Count { get; set; }
    }
}

public sealed class LoginRateLimitOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
}
