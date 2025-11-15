using System.Data;
using Dapper;
using MatchdayPredictions.Api.DataAccess.Interfaces;
using MatchdayPredictions.Api.Models.Api;
using MatchdayPredictions.Api.Models.Configuration;
using MatchdayPredictions.Api.OpenTelemetry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace MatchdayPredictions.Api.DataAccess;

public sealed class UserDataContext : SqlDataContextBase, IUserDataContext
{
    public UserDataContext(
        IConfiguration configuration,
        IOptions<MatchdayPredictionsSettings> settings,
        IMetricsProvider metrics,
        ILogger<UserDataContext> logger)
        : base(configuration, settings, metrics, logger)
    {
    }

    public Task CreateUserAsync(string username, string displayName, string email, string passwordHash)
    {
        return ExecuteAsync(
            "MatchDayPredictionsApi_CreateUser",
            conn => conn.ExecuteAsync(
                "MatchDayPredictionsApi_CreateUser",
                new
                {
                    UserName = username,
                    DisplayName = displayName,
                    Email = email,
                    PasswordHash = passwordHash
                },
                commandType: CommandType.StoredProcedure));
    }

    public Task<User?> GetUserByIdAsync(int userId)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetUserById",
            conn => conn.QuerySingleOrDefaultAsync<User>(
                "MatchDayPredictionsApi_GetUserById",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure));
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        return QueryAsync(
            "MatchDayPredictionsApi_GetUserByUsername",
            conn => conn.QuerySingleOrDefaultAsync<User>(
                "MatchDayPredictionsApi_GetUserByUsername",
                new { UserName = username },
                commandType: CommandType.StoredProcedure));
    }
}

