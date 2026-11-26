using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using MatchdayPredictions.Api.IntegrationTests.Infrastructure;
using MatchdayPredictions.Api.IntegrationTests.Utilities;
using MatchdayPredictions.Api.Models;
using MatchdayPredictions.Api.Models.Api;
using Xunit;

namespace MatchdayPredictions.Api.IntegrationTests;

[Collection("integration-tests")]
public class MatchesAndPredictionsTests
{
    private readonly IntegrationTestFixture _fixture;

    public MatchesAndPredictionsTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Matches_can_be_created_and_fetched()
    {
        await _fixture.ResetDatabaseAsync();

        var seeder = _fixture.CreateSeeder();
        var leagueId = await seeder.InsertLeagueAsync("Premier League", "EPL");

        var api = new TestApiClient(_fixture.CreateClient());
        var manager = await api.CreateUserAndLoginAsync("manager");

        using var _ = api.ApplyBearer(manager.Token);

        var kickoff = TestClock.TruncateToSecond(DateTime.UtcNow.AddHours(1));
        var createResponse = await api.PostJsonAsync("/api/v1/matches", new CreateMatchRequest
        {
            LeagueId = leagueId,
            HomeTeam = "Arsenal",
            AwayTeam = "Chelsea",
            KickoffUtc = kickoff
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedMatchResponse>();
        created.Should().NotBeNull();
        var matchId = created!.MatchId;
        createResponse.Headers.Location?.AbsolutePath.ToLowerInvariant()
            .Should().Be($"/api/v1/matches/{matchId}");

        var getResponse = await api.GetAsync($"/api/v1/matches/{matchId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var match = await getResponse.Content.ReadFromJsonAsync<Match>();
        match.Should().NotBeNull();
        match!.MatchId.Should().Be(matchId);
        match.LeagueId.Should().Be(leagueId);
        match.HomeTeam.Should().Be("Arsenal");
        match.AwayTeam.Should().Be("Chelsea");

        var listResponse = await api.GetAsync($"/api/v1/matches?leagueId={leagueId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var matches = await listResponse.Content.ReadFromJsonAsync<List<Match>>();
        matches.Should().ContainSingle(m => m.MatchId == matchId);
    }

    [Fact]
    public async Task Concurrent_match_creates_have_distinct_ids()
    {
        await _fixture.ResetDatabaseAsync();

        var seeder = _fixture.CreateSeeder();
        var firstLeague = await seeder.InsertLeagueAsync("First League", "ONE");
        var secondLeague = await seeder.InsertLeagueAsync("Second League", "TWO");
        var kickoff = TestClock.TruncateToSecond(DateTime.UtcNow.AddHours(1));

        var api = new TestApiClient(_fixture.CreateClient());
        var user = await api.CreateUserAndLoginAsync("manager");
        using var _ = api.ApplyBearer(user.Token);

        var responses = await Task.WhenAll(
            api.PostJsonAsync("/api/v1/matches", new CreateMatchRequest
            {
                LeagueId = firstLeague,
                HomeTeam = "Ajax",
                AwayTeam = "PSV",
                KickoffUtc = kickoff
            }),
            api.PostJsonAsync("/api/v1/matches", new CreateMatchRequest
            {
                LeagueId = secondLeague,
                HomeTeam = "AZ",
                AwayTeam = "Feyenoord",
                KickoffUtc = kickoff
            }));

        responses.Should().OnlyContain(response => response.StatusCode == HttpStatusCode.Created);

        var firstId = await seeder.GetMatchIdAsync(firstLeague, "Ajax", "PSV", kickoff);
        var secondId = await seeder.GetMatchIdAsync(secondLeague, "AZ", "Feyenoord", kickoff);
        firstId.Should().NotBe(secondId);
    }

    [Fact]
    public async Task Matches_requires_authentication()
    {
        await _fixture.ResetDatabaseAsync();

        var response = await new TestApiClient(_fixture.CreateClient())
            .GetAsync("/api/v1/matches?leagueId=1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Predictions_respect_user_guardrails()
    {
        await _fixture.ResetDatabaseAsync();

        var seeder = _fixture.CreateSeeder();
        var leagueId = await seeder.InsertLeagueAsync("Bundesliga", "GER");
        var kickoff = TestClock.TruncateToSecond(DateTime.UtcNow.AddHours(2));
        var matchId = await seeder.InsertMatchAsync(leagueId, "Bayern", "Dortmund", kickoff);

        var api = new TestApiClient(_fixture.CreateClient());
        var userOne = await api.CreateUserAndLoginAsync("userone");
        var userTwo = await api.CreateUserAndLoginAsync("usertwo");

        using var _ = api.ApplyBearer(userOne.Token);

        var forbiddenResponse = await api.PostJsonAsync("/api/v1/predictions", new CreatePredictionRequest
        {
            MatchId = matchId,
            UserId = userTwo.Profile.Id,
            HomeGoals = 2,
            AwayGoals = 1
        });

        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await seeder.InsertLeagueMemberAsync(leagueId, userOne.Profile.Id);

        var createdResponse = await api.PostJsonAsync("/api/v1/predictions", new CreatePredictionRequest
        {
            MatchId = matchId,
            UserId = userOne.Profile.Id,
            HomeGoals = 2,
            AwayGoals = 1
        });

        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getResponse = await api.GetAsync($"/api/v1/predictions/{matchId}?userId={userOne.Profile.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var prediction = await getResponse.Content.ReadFromJsonAsync<MatchPrediction>();
        prediction.Should().NotBeNull();
        prediction!.UserId.Should().Be(userOne.Profile.Id);
        prediction.MatchId.Should().Be(matchId);
        prediction.HomeGoals.Should().Be(2);
        prediction.AwayGoals.Should().Be(1);
    }

    [Fact]
    public async Task Predictions_require_membership_and_close_at_kickoff()
    {
        await _fixture.ResetDatabaseAsync();

        var seeder = _fixture.CreateSeeder();
        var leagueId = await seeder.InsertLeagueAsync("Eredivisie", "NED");
        var futureMatchId = await seeder.InsertMatchAsync(
            leagueId, "Ajax", "PSV", TestClock.TruncateToSecond(DateTime.UtcNow.AddHours(1)));
        var pastMatchId = await seeder.InsertMatchAsync(
            leagueId, "Feyenoord", "AZ", TestClock.TruncateToSecond(DateTime.UtcNow.AddMinutes(-5)));

        var api = new TestApiClient(_fixture.CreateClient());
        var user = await api.CreateUserAndLoginAsync("predictor");
        using var _ = api.ApplyBearer(user.Token);

        var request = new CreatePredictionRequest
        {
            MatchId = futureMatchId,
            UserId = user.Profile.Id,
            HomeGoals = 2,
            AwayGoals = 1
        };

        (await api.PostJsonAsync("/api/v1/predictions", request)).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);

        await seeder.InsertLeagueMemberAsync(leagueId, user.Profile.Id);

        (await api.PostJsonAsync("/api/v1/predictions", request)).StatusCode
            .Should().Be(HttpStatusCode.Created);

        (await api.PostJsonAsync("/api/v1/predictions", request with { HomeGoals = 3 })).StatusCode
            .Should().Be(HttpStatusCode.OK);

        var savedResponse = await api.GetAsync($"/api/v1/predictions/{futureMatchId}?userId={user.Profile.Id}");
        var saved = await savedResponse.Content.ReadFromJsonAsync<MatchPrediction>();
        saved!.HomeGoals.Should().Be(3);

        (await api.PostJsonAsync("/api/v1/predictions", request with { MatchId = pastMatchId })).StatusCode
            .Should().Be(HttpStatusCode.Conflict);

        (await api.PostJsonAsync("/api/v1/predictions", request with { MatchId = int.MaxValue })).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Concurrent_predictions_for_one_user_leave_one_row()
    {
        await _fixture.ResetDatabaseAsync();

        var seeder = _fixture.CreateSeeder();
        var leagueId = await seeder.InsertLeagueAsync("Eredivisie", "NED");
        var matchId = await seeder.InsertMatchAsync(
            leagueId, "Ajax", "PSV", TestClock.TruncateToSecond(DateTime.UtcNow.AddHours(1)));

        var api = new TestApiClient(_fixture.CreateClient());
        var user = await api.CreateUserAndLoginAsync("predictor");
        await seeder.InsertLeagueMemberAsync(leagueId, user.Profile.Id);
        using var _ = api.ApplyBearer(user.Token);

        var request = new CreatePredictionRequest
        {
            MatchId = matchId,
            UserId = user.Profile.Id,
            HomeGoals = 2,
            AwayGoals = 1
        };

        var responses = await Task.WhenAll(
            api.PostJsonAsync("/api/v1/predictions", request),
            api.PostJsonAsync("/api/v1/predictions", request));

        responses.Select(response => response.StatusCode)
            .Should().BeEquivalentTo(new[] { HttpStatusCode.Created, HttpStatusCode.OK });
        (await seeder.CountPredictionsAsync(matchId, user.Profile.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Create_match_with_invalid_body_returns_validation_error()
    {
        await _fixture.ResetDatabaseAsync();

        var api = new TestApiClient(_fixture.CreateClient());
        var manager = await api.CreateUserAndLoginAsync("manager");

        using var _ = api.ApplyBearer(manager.Token);

        var response = await api.PostJsonAsync("/api/v1/matches", new CreateMatchRequest
        {
            LeagueId = 0,
            HomeTeam = "",
            AwayTeam = "",
            KickoffUtc = DateTime.MinValue
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("Validation failed");
        body.Errors.Should().NotBeEmpty();
    }

    private sealed record ValidationErrorResponse(string Message, IEnumerable<ValidationError> Errors);
    private sealed record ValidationError(string Field, IEnumerable<string> Errors);
    private sealed record CreatedMatchResponse(int MatchId);
}
