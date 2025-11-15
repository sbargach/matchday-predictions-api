CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetMatchesByLeague
    @LeagueId INT
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
    WHERE lm.LeagueId = @LeagueId
    ORDER BY lm.KickoffUtc;
END
GO

