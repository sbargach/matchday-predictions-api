using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Tests.Controllers;

[TestClass]
public class LeaguesControllerTests
{
    private static LeaguesController CreateController(
        FakeLeagueRepository repository,
        out FakeMetricsProvider metrics)
    {
        metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<LeaguesController>();
        return new LeaguesController(repository, metrics, logger);
    }

    [TestMethod]
    public async Task GetById_WhenLeagueExists_ReturnsOkAndIncrementsSuccessMetric()
    {
        var repository = new FakeLeagueRepository
        {
            LeagueToReturn = new League
            {
                LeagueId = 10,
                Name = "Premier League",
                Country = "England"
            }
        };

        var controller = CreateController(repository, out var metrics);

        var result = await controller.GetById(10);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var league = okResult.Value.ShouldBeOfType<League>();
        league.LeagueId.ShouldBe(10);

        metrics.RequestCount.ShouldBe(1);
        metrics.SuccessCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(0);
    }

    [TestMethod]
    public async Task GetById_WhenLeagueDoesNotExist_ReturnsNotFoundAndIncrementsClientError()
    {
        var repository = new FakeLeagueRepository
        {
            LeagueToReturn = null
        };

        var controller = CreateController(repository, out var metrics);

        var result = await controller.GetById(42);

        result.ShouldBeOfType<NotFoundResult>();

        metrics.RequestCount.ShouldBe(1);
        metrics.ClientErrorCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task GetById_WhenRepositoryThrows_ReturnsServerErrorAndIncrementsServerErrorMetric()
    {
        var repository = new FakeLeagueRepository
        {
            ThrowOnGetById = true
        };

        var controller = CreateController(repository, out var metrics);

        var result = await controller.GetById(99);

        var objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        metrics.RequestCount.ShouldBe(1);
        metrics.ServerErrorCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(1);
    }
}
