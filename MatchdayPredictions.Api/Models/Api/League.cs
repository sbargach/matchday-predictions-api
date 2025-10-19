namespace MatchdayPredictions.Api.Models.Api
{
    /// <summary>
    /// Represents a football league.
    /// </summary>
    public sealed class League
    {
        public int LeagueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
