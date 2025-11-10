using System.ComponentModel.DataAnnotations;

namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record CreatePredictionRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int MatchId { get; init; }

        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; init; }

        [Range(0, 20)]
        public int HomeGoals { get; init; }

        [Range(0, 20)]
        public int AwayGoals { get; init; }
    }
}
