CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateUser
    @UserName     NVARCHAR(50),
    @DisplayName  NVARCHAR(100),
    @Email        NVARCHAR(255),
    @PasswordHash NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    -- Prevent duplicate usernames / emails
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = @UserName OR Email = @Email)
        RETURN;

    INSERT INTO dbo.Users (UserName, DisplayName, Email, PasswordHash)
    VALUES (@UserName, @DisplayName, @Email, @PasswordHash);
END
GO
