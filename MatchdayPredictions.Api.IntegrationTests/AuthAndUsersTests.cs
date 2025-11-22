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
}
