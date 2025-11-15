using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IUserDataContext _context;

    public UserRepository(IUserDataContext context)
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
        => _context.GetUserByUsernameAsync(username);

    public Task<User?> GetUserByIdAsync(int userId)
        => _context.GetUserByIdAsync(userId);
}

