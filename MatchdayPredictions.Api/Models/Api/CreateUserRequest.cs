using System.ComponentModel.DataAnnotations;

namespace MatchdayPredictions.Api.Models.Api
{
    public sealed record CreateUserRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public string DisplayName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; init; } = string.Empty;
    }
}
