CREATE PROCEDURE dbo.MatchDayPredictionsApi_SetPrediction
    @UserName  NVARCHAR(50),
    @LeagueId  INT,
    @MatchId   INT,
    @HomeGoals TINYINT,
    @AwayGoals TINYINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT;

    SELECT @UserId = u.Id
    FROM dbo.Users AS u
    WHERE u.UserName = @UserName;

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
