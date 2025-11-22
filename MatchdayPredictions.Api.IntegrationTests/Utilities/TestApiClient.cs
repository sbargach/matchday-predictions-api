using System.Net.Http.Headers;
using System.Net.Http.Json;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.IntegrationTests.Utilities;

public sealed class TestApiClient
{
    private readonly HttpClient _client;

    public TestApiClient(HttpClient client)
    {
        _client = client;
    }

    public Task<HttpResponseMessage> GetAsync(string url)
        => _client.GetAsync(url);

    public Task<HttpResponseMessage> PostJsonAsync<T>(string url, T payload)
        => _client.PostAsJsonAsync(url, payload);

    public Task<HttpResponseMessage> RegisterUserAsync(CreateUserRequest request)
        => _client.PostAsJsonAsync("/api/users", request);

    public async Task<LoginResult?> LoginAsync(string username, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        });

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResult>();
    }

    public async Task<User?> GetCurrentUserAsync(string token)
    {
        using var _ = ApplyBearer(token);
        var response = await _client.GetAsync("/api/users/me");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<User>();
    }

    public async Task<AuthenticatedUser> CreateUserAndLoginAsync(string prefix)
    {
        var username = $"{prefix}_{Guid.NewGuid():N}".Substring(0, 18);
        const string password = "Str0ngP@ssw0rd!";

        var register = await RegisterUserAsync(new CreateUserRequest
        {
            Username = username,
            DisplayName = prefix,
            Email = $"{username}@example.com",
            Password = password
        });

        register.EnsureSuccessStatusCode();

        var login = await LoginAsync(username, password)
                    ?? throw new InvalidOperationException("Login failed during test setup.");

        var profile = await GetCurrentUserAsync(login.Token)
                      ?? throw new InvalidOperationException("Failed to fetch current user after login.");

        return new AuthenticatedUser(login.Token, profile);
    }

    public IDisposable ApplyBearer(string token)
    {
        var original = _client.DefaultRequestHeaders.Authorization;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new HeaderReset(() => _client.DefaultRequestHeaders.Authorization = original);
    }

    public sealed record LoginResult(string Token);

    public sealed record AuthenticatedUser(string Token, User Profile);

    private sealed class HeaderReset : IDisposable
    {
        private readonly Action _reset;
        public HeaderReset(Action reset) => _reset = reset;
        public void Dispose() => _reset();
    }
}
