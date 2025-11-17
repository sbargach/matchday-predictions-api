using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Repositories;

namespace MatchdayPredictions.Api.Tests.Repositories;

[TestClass]
public class UserRepositoryTests
{
    [TestMethod]
    public async Task CreateUserAsync_WithDisplayName_UsesProvidedDisplayNameAndHashesPassword()
    {
        var (repository, dataContext) = CreateSut();

        var request = new CreateUserRequest
        {
            Username = "user1",
            DisplayName = "User One",
            Email = "user1@example.com",
            Password = "P@ssw0rd!"
        };

        await repository.CreateUserAsync(request);

        dataContext.Username.ShouldBe(request.Username);
        dataContext.DisplayName.ShouldBe(request.DisplayName);
        dataContext.Email.ShouldBe(request.Email);

        dataContext.PasswordHash.ShouldNotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, dataContext.PasswordHash).ShouldBeTrue();
    }

    [TestMethod]
    public async Task CreateUserAsync_WithWhitespaceDisplayName_FallsBackToUsername()
    {
        var (repository, dataContext) = CreateSut();

        var request = new CreateUserRequest
        {
            Username = "user2",
            DisplayName = "   ",
            Email = "user2@example.com",
            Password = "AnotherPassword1!"
        };

        await repository.CreateUserAsync(request);

        dataContext.DisplayName.ShouldBe(request.Username);
    }

    private static (UserRepository Repository, FakeUserDataContext Context) CreateSut()
    {
        var context = new FakeUserDataContext();
        var repository = new UserRepository(context);
        return (repository, context);
    }

    private sealed class FakeUserDataContext : IUserDataContext
    {
        public string? Username { get; private set; }
        public string? DisplayName { get; private set; }
        public string? Email { get; private set; }
        public string? PasswordHash { get; private set; }

        public Task CreateUserAsync(string username, string displayName, string email, string passwordHash)
        {
            Username = username;
            DisplayName = displayName;
            Email = email;
            PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<User?> GetUserByIdAsync(int userId)
            => throw new NotImplementedException();

        public Task<User?> GetUserByUsernameAsync(string username)
            => throw new NotImplementedException();
    }
}
