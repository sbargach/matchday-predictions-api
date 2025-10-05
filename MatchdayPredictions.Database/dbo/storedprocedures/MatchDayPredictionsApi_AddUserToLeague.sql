CREATE PROCEDURE dbo.MatchDayPredictionsApi_AddUserToLeague
    @UserName NVARCHAR(50),
    @LeagueId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    DECLARE @UserId INT;

    SELECT @UserId = u.Id
    FROM dbo.Users AS u
    WHERE u.UserName = @UserName;

    IF @UserId IS NULL
        RETURN; 

  
    IF NOT EXISTS (SELECT 1 FROM dbo.Leagues WHERE Id = @LeagueId)
        RETURN;

    IF NOT EXISTS (
        SELECT 1 FROM dbo.LeagueMembers
        WHERE LeagueId = @LeagueId AND UserId = @UserId
    )
    BEGIN
        INSERT INTO dbo.LeagueMembers (LeagueId, UserId, Joined)
        VALUES (@LeagueId, @UserId, SYSUTCDATETIME());
    END
END
GO
