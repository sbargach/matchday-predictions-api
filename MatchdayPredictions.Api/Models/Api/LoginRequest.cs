using System.ComponentModel.DataAnnotations;

namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record LoginRequest
    {
        [Required]
        [MinLength(3)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; init; } = string.Empty;
    }
}
