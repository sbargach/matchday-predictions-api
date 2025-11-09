using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.Repositories.Interfaces
{
    /// <summary>
    /// Defines data operations for users.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="request">The user details to insert.</param>
        Task CreateUserAsync(CreateUserRequest request);

        /// <summary>
        /// Retrieves a specific user by ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user if found, otherwise null.</returns>
        Task<User?> GetUserByIdAsync(int userId);
        /// <summary>
        /// Retrieves a specific user by username
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user if found, otherwise null.</returns>
        Task<User?> GetByUsernameAsync(string username);

    }
}
