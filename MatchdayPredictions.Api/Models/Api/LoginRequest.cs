namespace MatchdayPredictions.Api.Models.Api
{
    /// <summary>
    /// Represents a login request with username and password.
    /// </summary>
    public sealed class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
