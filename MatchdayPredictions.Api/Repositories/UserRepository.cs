using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMatchdayPredictionsDataContext _context;

        public UserRepository(IMatchdayPredictionsDataContext context)
        {
            _context = context;
        }

        public async Task CreateUserAsync(CreateUserRequest request)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? request.Username
                : request.DisplayName;

            await _context.CreateUserAsync(request.Username, displayName, request.Email, passwordHash);
        }

        public Task<User?> GetByUsernameAsync(string username)
        {
            return _context.GetUserByUsernameAsync(username);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.GetUserByIdAsync(userId);
        }
    }
}
