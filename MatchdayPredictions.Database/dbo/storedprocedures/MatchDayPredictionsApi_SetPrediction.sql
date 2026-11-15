CREATE PROCEDURE dbo.MatchDayPredictionsApi_SetPrediction
    @MatchId   INT,
    @UserId    INT,
    @HomeGoals TINYINT,
    @AwayGoals TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @LeagueId INT;
    DECLARE @KickoffUtc DATETIME2(0);

    BEGIN TRANSACTION;

    SELECT @LeagueId = lm.LeagueId,
           @KickoffUtc = lm.KickoffUtc
    FROM dbo.LeagueMatches AS lm WITH (UPDLOCK, HOLDLOCK)
    WHERE lm.MatchId = @MatchId;

    IF @LeagueId IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'MatchNotFound' AS Result;
        RETURN;
    END

    IF @KickoffUtc <= SYSUTCDATETIME()
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'MatchStarted' AS Result;
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.LeagueMembers
        WHERE LeagueId = @LeagueId AND UserId = @UserId
    )
    BEGIN
        ROLLBACK TRANSACTION;
        SELECT 'NotLeagueMember' AS Result;
        RETURN;
    END

    IF EXISTS (
        SELECT 1
        FROM dbo.Predictions WITH (UPDLOCK, HOLDLOCK)
        WHERE LeagueId = @LeagueId AND MatchId = @MatchId AND UserId = @UserId
    )
    BEGIN
        UPDATE dbo.Predictions
           SET PredictedHomeGoals = @HomeGoals,
               PredictedAwayGoals = @AwayGoals,
               UpdatedAt = SYSUTCDATETIME()
         WHERE LeagueId = @LeagueId AND MatchId = @MatchId AND UserId = @UserId;

        COMMIT TRANSACTION;
        SELECT 'Updated' AS Result;
        RETURN;
    END

    INSERT INTO dbo.Predictions
        (LeagueId, MatchId, UserId, PredictedHomeGoals, PredictedAwayGoals)
    VALUES
        (@LeagueId, @MatchId, @UserId, @HomeGoals, @AwayGoals);

    COMMIT TRANSACTION;
    SELECT 'Created' AS Result;
END
GO

