using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories.Interfaces;

namespace MatchdayPredictions.Api.Tests.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly User? _user;

    public FakeUserRepository(User? user)
    {
        _user = user;
    }

    public Task CreateUserAsync(CreateUserRequest request) => Task.CompletedTask;

    public Task<User?> GetUserByIdAsync(int userId) => Task.FromResult<User?>(_user);

    public Task<User?> GetByUsernameAsync(string username) => Task.FromResult<User?>(_user);
}

