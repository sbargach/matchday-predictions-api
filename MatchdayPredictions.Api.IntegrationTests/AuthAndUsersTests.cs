using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MatchdayPredictions.Api.IntegrationTests.Infrastructure;
using MatchdayPredictions.Api.IntegrationTests.Utilities;
using MatchdayPredictions.Api.Models.Api;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace MatchdayPredictions.Api.IntegrationTests;

[Collection("integration-tests")]
public class AuthAndUsersTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public AuthAndUsersTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task Register_login_and_read_profile()
    {
        await _fixture.ResetDatabaseAsync();

        var api = new TestApiClient(_fixture.CreateClient());
        var user = await api.CreateUserAndLoginAsync("user");

        var meResponse = await api.GetCurrentUserAsync(user.Token);
        meResponse.Should().NotBeNull();
        meResponse!.UserName.Should().Be(user.Profile.UserName);
        meResponse.DisplayName.Should().Be(user.Profile.DisplayName);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        await _fixture.ResetDatabaseAsync();

        var api = new TestApiClient(_fixture.CreateClient());
        var created = await api.CreateUserAndLoginAsync("user");

        var badLogin = await api.LoginAsync(created.Profile.UserName, "nottherightpassword");
        badLogin.Should().BeNull();
    }

    [Fact]
    public async Task Get_profile_without_token_returns_unauthorized()
    {
        await _fixture.ResetDatabaseAsync();

        var api = new TestApiClient(_fixture.CreateClient());
        await api.CreateUserAndLoginAsync("user");

        var response = await api.GetAsync("/api/v1/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_requests_exceeding_limit_are_rejected()
    {
        await _fixture.ResetDatabaseAsync();

        using var factory = _fixture.CreateFactory(new LoginRateLimitSettings(5, 0, 5));
        var api = new TestApiClient(factory.CreateClient());

        var username = $"ratelimit_{Guid.NewGuid():N}".Substring(0, 18);
        const string password = "Str0ngP@ssw0rd!";

        using (var scope = factory.Services.CreateScope())
        {
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var limit = config.GetValue<int?>("RateLimiting:Login:PermitLimit");
            _output.WriteLine($"Configured permit limit: {limit}");
        }

        var register = await api.RegisterUserAsync(new CreateUserRequest
        {
            Username = username,
            DisplayName = "RateLimiter",
            Email = $"{username}@example.com",
            Password = password
        });

        register.EnsureSuccessStatusCode();

        var loginTasks = Enumerable.Range(0, 10)
            .Select(_ => api.LoginRawAsync(username, password))
            .ToArray();

        var responses = await Task.WhenAll(loginTasks);

        var statusSummary = responses
            .GroupBy(r => r.StatusCode)
            .ToDictionary(g => g.Key, g => g.Count());
        _output.WriteLine($"Rate limit statuses: {string.Join(", ", statusSummary.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(5);
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);

        var rejected = responses.First(r => r.StatusCode == HttpStatusCode.TooManyRequests);
        var error = await rejected.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.Message.Should().Be("Too many requests. Please retry shortly.");
    }
}
