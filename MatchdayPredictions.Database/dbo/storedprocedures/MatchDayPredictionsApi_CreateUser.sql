CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateUser
    @UserName    NVARCHAR(50),
    @DisplayName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    INSERT INTO dbo.Users (UserName, DisplayName)
    SELECT @UserName, @DisplayName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = @UserName);

END
GO
