namespace MatchdayPredictions.Api.Models.Api;

/// <summary>
/// Standard error response shape for the API.
/// </summary>
public sealed record ErrorResponse(string Message, IEnumerable<FieldError> Errors)
{
    public static ErrorResponse FromValidation(string message, IEnumerable<FieldError> errors)
        => new(message, errors);

    public static ErrorResponse FromMessage(string message)
        => new(message, Array.Empty<FieldError>());
}

public sealed record FieldError(string Field, IEnumerable<string> Messages);
