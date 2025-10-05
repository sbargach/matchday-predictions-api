CREATE TABLE dbo.LeagueMatches
(
    LeagueId   INT           NOT NULL,
    MatchId    INT           NOT NULL,  
    HomeTeam   NVARCHAR(100) NOT NULL,
    AwayTeam   NVARCHAR(100) NOT NULL,
    KickoffUtc DATETIME2(0)  NOT NULL,

    CONSTRAINT PK_LeagueMatches PRIMARY KEY (LeagueId, MatchId),
    CONSTRAINT FK_LeagueMatches_League
        FOREIGN KEY (LeagueId) REFERENCES dbo.Leagues(Id) ON DELETE CASCADE
);
GO
