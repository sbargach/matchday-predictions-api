using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Tests.Controllers;

[TestClass]
public class MatchesControllerTests
{
    private static MatchesController CreateController(
        FakeMatchRepository repository,
        out FakeMetricsProvider metrics)
    {
        metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<MatchesController>();
        return new MatchesController(repository, metrics, logger);
    }

    [TestMethod]
    public async Task GetById_WhenMatchDoesNotExist_ReturnsNotFoundAndIncrementsClientError()
    {
        var repository = new FakeMatchRepository
        {
            MatchToReturn = null
        };

        var controller = CreateController(repository, out var metrics);

        var result = await controller.GetById(123);

        result.ShouldBeOfType<NotFoundResult>();

        metrics.RequestCount.ShouldBe(1);
        metrics.ClientErrorCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task Create_WhenRepositoryThrows_ReturnsServerErrorAndIncrementsServerErrorMetric()
    {
        var repository = new FakeMatchRepository
        {
            ThrowOnCreate = true
        };

        var controller = CreateController(repository, out var metrics);

        var request = new CreateMatchRequest
        {
            LeagueId = 7,
            HomeTeam = "Home Team",
            AwayTeam = "Away Team",
            KickoffUtc = DateTime.UtcNow
        };

        var result = await controller.Create(request);

        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        metrics.RequestCount.ShouldBe(1);
        metrics.ServerErrorCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(1);
    }
}
