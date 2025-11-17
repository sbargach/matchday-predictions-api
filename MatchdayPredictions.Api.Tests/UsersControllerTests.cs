using System.Security.Claims;
using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MatchdayPredictions.Api.Tests.Controllers;

[TestClass]
public class UsersControllerTests
{
    [TestMethod]
    public async Task GetSelf_WhenUserExists_ReturnsUser()
    {
        var user = new User
        {
            Id = 5,
            UserName = "user5",
            DisplayName = "User Five",
            Email = "user5@example.com"
        };

        var repository = new FakeUserRepository(user);
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<UsersController>();

        var controller = new UsersController(repository, metrics, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContextWithUserId(user.Id)
            }
        };

        var result = await controller.GetSelf();

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBeOfType<User>()
            .Id.ShouldBe(user.Id);
    }

    [TestMethod]
    public async Task GetSelf_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var repository = new FakeUserRepository(null);
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<UsersController>();

        var controller = new UsersController(repository, metrics, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContextWithUserId(99)
            }
        };

        var result = await controller.GetSelf();

        result.ShouldBeOfType<NotFoundResult>();
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
