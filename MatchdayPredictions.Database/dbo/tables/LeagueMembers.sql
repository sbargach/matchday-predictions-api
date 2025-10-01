CREATE TABLE dbo.LeagueMembers
(
    LeagueId INT NOT NULL,
    UserId   INT NOT NULL,
    Joined DATETIME2(0) NOT NULL,

    CONSTRAINT PK_LeagueMembers PRIMARY KEY (LeagueId, UserId),
    CONSTRAINT FK_LeagueMembers_League FOREIGN KEY (LeagueId) REFERENCES dbo.Leagues(Id),
    CONSTRAINT FK_LeagueMembers_User   FOREIGN KEY (UserId)   REFERENCES dbo.Users(Id)
);
GO
