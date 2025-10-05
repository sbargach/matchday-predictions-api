CREATE TABLE dbo.Predictions
(
    LeagueId           INT         NOT NULL,
    MatchId            INT         NOT NULL,
    UserId             INT         NOT NULL,
    PredictedHomeGoals TINYINT     NOT NULL,
    PredictedAwayGoals TINYINT     NOT NULL,
    CreatedAt          DATETIME2(0) NOT NULL CONSTRAINT DF_Predictions_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt          DATETIME2(0) NULL,

    CONSTRAINT PK_Predictions PRIMARY KEY (LeagueId, MatchId, UserId),

    CONSTRAINT FK_Predictions_LeagueMatch
        FOREIGN KEY (LeagueId, MatchId)
        REFERENCES dbo.LeagueMatches(LeagueId, MatchId) ON DELETE CASCADE,

    CONSTRAINT FK_Predictions_User
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),

    CONSTRAINT CK_Predictions_Goals
        CHECK (PredictedHomeGoals BETWEEN 0 AND 20
           AND  PredictedAwayGoals BETWEEN 0 AND 20)
);
GO