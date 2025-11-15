CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetMatchById
    @MatchId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    SELECT 
        lm.MatchId,
        lm.LeagueId,
        lm.HomeTeam,
        lm.AwayTeam,
        lm.KickoffUtc
    FROM dbo.LeagueMatches AS lm
    WHERE lm.MatchId = @MatchId;
END
GO

