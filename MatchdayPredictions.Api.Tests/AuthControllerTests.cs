using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MatchdayPredictions.Api.Controllers;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.Tests.Fakes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MatchdayPredictions.Api.Tests.Controllers;

[TestClass]
public class AuthControllerTests
{
    [TestMethod]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var userRepository = new FakeUserRepository(null);
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<AuthController>();
        var options = Options.Create(CreateJwtSettings());

        var controller = new AuthController(options, userRepository, metrics, logger);

        var request = new LoginRequest
        {
            Username = "unknown",
            Password = "wrong"
        };

        var result = await controller.Login(request);

        result.ShouldBeOfType<UnauthorizedObjectResult>();
        metrics.ClientErrorCount.ShouldBe(1);
        metrics.FailureCount.ShouldBe(1);
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var password = "P@ssw0rd!";
        var user = new User
        {
            Id = 42,
            UserName = "valid-user",
            DisplayName = "Valid User",
            Email = "valid@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        var userRepository = new FakeUserRepository(user);
        var metrics = new FakeMetricsProvider();
        var logger = new FakeLogger<AuthController>();
        var options = Options.Create(CreateJwtSettings());

        var controller = new AuthController(options, userRepository, metrics, logger);

        var request = new LoginRequest
        {
            Username = user.UserName,
            Password = password
        };

        var result = await controller.Login(request);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldNotBeNull();

        var tokenProperty = okResult.Value.GetType().GetProperty("token");
        tokenProperty.ShouldNotBeNull("Login response should contain a token property.");

        var token = tokenProperty!.GetValue(okResult.Value) as string;
        token.ShouldNotBeNullOrWhiteSpace();

        // Basic sanity check: token should be a JWT that can be read.
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());

        metrics.SuccessCount.ShouldBe(1);
    }

    private static JwtSettings CreateJwtSettings()
    {
        return new JwtSettings
        {
            Key = Convert.ToBase64String(Encoding.UTF8.GetBytes("super-secret-key-for-tests")),
            Issuer = "MatchdayPredictions",
            Audience = "MatchdayPredictionsClient",
            TokenHours = 1
        };
    }
}
