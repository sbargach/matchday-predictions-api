CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetLeagueById
    @LeagueId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    SELECT 
        Id       AS LeagueId,
        Name,
        Code     AS Country
    FROM dbo.Leagues
    WHERE Id = @LeagueId;
END
GO

