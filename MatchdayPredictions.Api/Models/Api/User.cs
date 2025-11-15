namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record User
    {
        public int Id { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime CreatedUtc { get; init; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string PasswordHash { get; init; } = string.Empty;
    }
}
