using System.Security.Claims;
using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Tests.Controllers;

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

}
