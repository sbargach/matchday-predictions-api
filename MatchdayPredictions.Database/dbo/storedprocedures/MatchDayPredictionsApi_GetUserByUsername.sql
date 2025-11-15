CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetUserByUsername
    @UserName NVARCHAR(50)
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
    WHERE UserName = @UserName;
END
GO
