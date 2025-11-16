using System.Security.Claims;
using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.OpenTelemetry;
using MatchdayPredictions.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MatchdayPredictions.Api.Tests;

[TestClass]
public class PredictionsControllerTests
{
    [TestMethod]
    public async Task AddPrediction_ForDifferentUser_ReturnsForbid()
    {
        // Arrange
        var repository = new FakePredictionRepository();
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<PredictionsController>();

        var controller = new PredictionsController(repository, metrics, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContextWithUserId(userId: 1)
            }
        };

        var request = new CreatePredictionRequest
        {
            MatchId = 10,
            UserId = 2,
            HomeGoals = 1,
            AwayGoals = 0
        };

        // Act
        var result = await controller.AddPrediction(request);

        // Assert
        result.ShouldBeOfType<ForbidResult>();
        repository.AddPredictionCalled.ShouldBeFalse();
    }

    [TestMethod]
    public async Task AddPrediction_ForCurrentUser_CallsRepositoryAndReturnsCreated()
    {
        var repository = new FakePredictionRepository();
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<PredictionsController>();

        var controller = new PredictionsController(repository, metrics, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContextWithUserId(userId: 5)
            }
        };

        var request = new CreatePredictionRequest
        {
            MatchId = 42,
            UserId = 5,
            HomeGoals = 3,
            AwayGoals = 1
        };

        var result = await controller.AddPrediction(request);

        result.ShouldBeOfType<CreatedAtActionResult>();
        repository.AddPredictionCalled.ShouldBeTrue();
        repository.LastRequest?.MatchId.ShouldBe(42);
        repository.LastRequest?.UserId.ShouldBe(5);
    }

    [TestMethod]
    public async Task GetPrediction_ForDifferentUser_ReturnsForbid()
    {
        var repository = new FakePredictionRepository();
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<PredictionsController>();

        var controller = new PredictionsController(repository, metrics, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContextWithUserId(userId: 3)
            }
        };

        var result = await controller.GetPrediction(matchId: 7, userId: 4);

        result.ShouldBeOfType<ForbidResult>();
        repository.GetPredictionCalled.ShouldBeFalse();
    }

    private static HttpContext CreateHttpContextWithUserId(int userId)
    {
        var context = new DefaultHttpContext();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return context;
    }

    private sealed class FakePredictionRepository : IPredictionRepository
    {
        public bool AddPredictionCalled { get; private set; }
        public bool GetPredictionCalled { get; private set; }
        public CreatePredictionRequest? LastRequest { get; private set; }

        public Task AddPredictionAsync(CreatePredictionRequest request)
        {
            AddPredictionCalled = true;
            LastRequest = request;
            return Task.CompletedTask;
        }

        public Task<MatchPrediction?> GetPredictionAsync(int matchId, int userId)
        {
            GetPredictionCalled = true;
            return Task.FromResult<MatchPrediction?>(null);
        }
    }

    private sealed class FakeMetricsProvider : IMetricsProvider
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

    private sealed class FakeLogger<T> : ILogger<T>
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
}
