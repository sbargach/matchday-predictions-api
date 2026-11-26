CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateMatch
    @LeagueId   INT,
    @HomeTeam   NVARCHAR(100),
    @AwayTeam   NVARCHAR(100),
    @KickoffUtc DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @NextMatchId INT;

    BEGIN TRANSACTION;

    SELECT @NextMatchId = ISNULL(MAX(MatchId), 0) + 1
    FROM dbo.LeagueMatches WITH (TABLOCKX, HOLDLOCK);

    INSERT INTO dbo.LeagueMatches
        (LeagueId, MatchId, HomeTeam, AwayTeam, KickoffUtc)
    VALUES
        (@LeagueId, @NextMatchId, @HomeTeam, @AwayTeam, @KickoffUtc);

    COMMIT TRANSACTION;
    SELECT @NextMatchId AS MatchId;
END
GO
