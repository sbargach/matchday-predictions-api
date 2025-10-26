namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record CreateUserRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}
