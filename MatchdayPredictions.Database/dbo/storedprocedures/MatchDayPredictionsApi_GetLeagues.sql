CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetLeagues
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    SELECT 
        Id       AS LeagueId,
        Name,
        Code     AS Country
    FROM dbo.Leagues;
END
GO

