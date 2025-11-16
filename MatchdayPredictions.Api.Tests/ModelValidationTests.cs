using System.ComponentModel.DataAnnotations;
using MatchdayPredictions.Api.Models.Api;

namespace MatchdayPredictions.Api.Tests;

[TestClass]
public class ModelValidationTests
{
    [TestMethod]
    public void CreateUserRequest_WithTooShortUsername_IsInvalid()
    {
        var model = new CreateUserRequest
        {
            Username = "ab",
            DisplayName = "Valid Name",
            Email = "user@example.com",
            Password = "validpassword"
        };

        var results = ValidateModel(model);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(CreateUserRequest.Username)));
    }

    [TestMethod]
    public void LoginRequest_WithMissingPassword_IsInvalid()
    {
        var model = new LoginRequest
        {
            Username = "validuser",
            Password = ""
        };

        var results = ValidateModel(model);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(LoginRequest.Password)));
    }

    [TestMethod]
    public void CreateUserRequest_WithInvalidEmail_IsInvalid()
    {
        var model = new CreateUserRequest
        {
            Username = "validuser",
            DisplayName = "Valid Name",
            Email = "not-an-email",
            Password = "validpassword"
        };

        var results = ValidateModel(model);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(CreateUserRequest.Email)));
    }

    [TestMethod]
    public void CreatePredictionRequest_WithNegativeMatchId_IsInvalid()
    {
        var model = new CreatePredictionRequest
        {
            MatchId = 0,
            UserId = 1,
            HomeGoals = 1,
            AwayGoals = 1
        };

        var results = ValidateModel(model);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(CreatePredictionRequest.MatchId)));
    }

    [TestMethod]
    public void CreatePredictionRequest_WithTooManyGoals_IsInvalid()
    {
        var model = new CreatePredictionRequest
        {
            MatchId = 1,
            UserId = 1,
            HomeGoals = 25,
            AwayGoals = 0
        };

        var results = ValidateModel(model);

        results.ShouldContain(r => r.MemberNames.Contains(nameof(CreatePredictionRequest.HomeGoals)));
    }

    private static IList<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
