using System.ComponentModel.DataAnnotations;

namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record CreateMatchRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int LeagueId { get; init; }

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string HomeTeam { get; init; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string AwayTeam { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime KickoffUtc { get; init; }
    }
}
