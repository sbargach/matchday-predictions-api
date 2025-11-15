using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.DataAccess.Interfaces;

public interface IUserDataContext
{
    Task CreateUserAsync(string username, string displayName, string email, string passwordHash);

    Task<User?> GetUserByIdAsync(int userId);

    Task<User?> GetUserByUsernameAsync(string username);
}

