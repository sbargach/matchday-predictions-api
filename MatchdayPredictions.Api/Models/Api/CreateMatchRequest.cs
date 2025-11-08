namespace MatchdayPredictions.Api.Models.Api
{
    /// <summary>
    /// Represents the request payload for creating a new match.
    /// </summary>
    public sealed class CreateMatchRequest
    {
        /// <summary>
        /// The league in which the match will be played.
        /// </summary>
        public int LeagueId { get; set; }

        /// <summary>
        /// The home team participating in the match.
        /// </summary>
        public string HomeTeam { get; set; } = string.Empty;

        /// <summary>
        /// The away team participating in the match.
        /// </summary>
        public string AwayTeam { get; set; } = string.Empty;

        /// <summary>
        /// The scheduled kickoff time in UTC.
        /// </summary>
        public DateTime KickoffUtc { get; set; }
    }
}
