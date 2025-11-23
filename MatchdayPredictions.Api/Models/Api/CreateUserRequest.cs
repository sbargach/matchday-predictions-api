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
        [MinLength(8)]
        [MaxLength(100)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$", ErrorMessage = "Password must contain upper, lower, and digit.")]
        public string Password { get; init; } = string.Empty;
    }
}
