CREATE PROCEDURE dbo.MatchDayPredictionsApi_GetPrediction
    @MatchId INT,
    @UserId  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    SELECT 
        p.MatchId,
        p.UserId,
        lm.HomeTeam,
        lm.AwayTeam,
        p.PredictedHomeGoals AS HomeGoals,
        p.PredictedAwayGoals AS AwayGoals,
        lm.KickoffUtc
    FROM dbo.Predictions AS p
    INNER JOIN dbo.LeagueMatches AS lm
        ON p.LeagueId = lm.LeagueId
       AND p.MatchId  = lm.MatchId
    WHERE p.MatchId = @MatchId
      AND p.UserId  = @UserId;
END
GO

