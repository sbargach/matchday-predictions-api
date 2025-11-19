using System.Security.Claims;
using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;
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

    [TestMethod]
    public async Task Create_WhenRepositorySucceeds_ReturnsCreatedAndCallsRepository()
    {
        var repository = new CapturingUserRepository();
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<UsersController>();

        var controller = new UsersController(repository, metrics, logger);

        var request = new CreateUserRequest
        {
            Username = "newuser",
            DisplayName = "New User",
            Email = "newuser@example.com",
            Password = "P@ssw0rd!"
        };

        var result = await controller.Create(request);

        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(UsersController.GetSelf));

        repository.CreateCalled.ShouldBeTrue();
        repository.LastRequest.ShouldNotBeNull();
        repository.LastRequest!.Username.ShouldBe(request.Username);
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

    private sealed class CapturingUserRepository : IUserRepository
    {
        public bool CreateCalled { get; private set; }
        public CreateUserRequest? LastRequest { get; private set; }

        public Task CreateUserAsync(CreateUserRequest request)
        {
            CreateCalled = true;
            LastRequest = request;
            return Task.CompletedTask;
        }

        public Task<User?> GetUserByIdAsync(int userId)
            => Task.FromResult<User?>(null);

        public Task<User?> GetByUsernameAsync(string username)
            => Task.FromResult<User?>(null);
    }
}
