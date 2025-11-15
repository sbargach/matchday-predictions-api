CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateMatch
    @LeagueId   INT,
    @HomeTeam   NVARCHAR(100),
    @AwayTeam   NVARCHAR(100),
    @KickoffUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    DECLARE @NextMatchId INT;

    -- Ensure MatchId is globally unique across all leagues
    SELECT @NextMatchId = ISNULL(MAX(MatchId), 0) + 1
    FROM dbo.LeagueMatches;

    INSERT INTO dbo.LeagueMatches
        (LeagueId, MatchId, HomeTeam, AwayTeam, KickoffUtc)
    VALUES
        (@LeagueId, @NextMatchId, @HomeTeam, @AwayTeam, @KickoffUtc);
END
GO
