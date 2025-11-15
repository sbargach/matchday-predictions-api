CREATE PROCEDURE dbo.MatchDayPredictionsApi_SetPrediction
    @MatchId   INT,
    @UserId    INT,
    @HomeGoals TINYINT,
    @AwayGoals TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LeagueId INT;

    SELECT @LeagueId = lm.LeagueId
    FROM dbo.LeagueMatches AS lm
    WHERE lm.MatchId = @MatchId;

    IF @LeagueId IS NULL
        RETURN;

    UPDATE p
       SET PredictedHomeGoals = @HomeGoals,
           PredictedAwayGoals = @AwayGoals,
           UpdatedAt          = SYSUTCDATETIME()
    FROM dbo.Predictions AS p
    WHERE p.LeagueId = @LeagueId
      AND p.MatchId  = @MatchId
      AND p.UserId   = @UserId;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.Predictions
            (LeagueId, MatchId, UserId, PredictedHomeGoals, PredictedAwayGoals)
        VALUES
            (@LeagueId, @MatchId, @UserId, @HomeGoals, @AwayGoals);
    END
END
GO

