using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using MatchdayPredictions.Api.IntegrationTests.Infrastructure;
using MatchdayPredictions.Api.IntegrationTests.Utilities;
using MatchdayPredictions.Api.Models.Api;
using Xunit;

namespace MatchdayPredictions.Api.IntegrationTests;

[Collection("integration-tests")]
public class AuthAndUsersTests
{
    private readonly IntegrationTestFixture _fixture;

    public AuthAndUsersTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
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
    public async Task Login_is_rate_limited()
    {
        await _fixture.ResetDatabaseAsync();

        var api = new TestApiClient(_fixture.CreateClient());
        var user = await api.CreateUserAndLoginAsync("ratelimit");

        var responses = new List<HttpStatusCode>();
        for (var i = 0; i < 15; i++)
        {
            var response = await api.LoginRawAsync(user.Profile.UserName, "Str0ngP@ssw0rd!");
            responses.Add(response.StatusCode);
        }

        responses.Take(10).Should().OnlyContain(status => status == HttpStatusCode.OK);
        responses.Skip(10).Should().Contain(HttpStatusCode.TooManyRequests);
    }
}
