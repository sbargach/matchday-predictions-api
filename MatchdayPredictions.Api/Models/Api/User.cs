namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record User
    {
        public int UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}
