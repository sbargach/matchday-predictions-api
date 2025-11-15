CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    SELECT 
        Id,
        UserName,
        DisplayName,
        Email,
        PasswordHash,
        CreatedUtc
    FROM dbo.Users
    WHERE Id = @UserId;
END
GO

