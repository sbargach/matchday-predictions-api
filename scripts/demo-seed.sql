SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO dbo.Users (UserName, DisplayName, Email, PasswordHash)
VALUES ('demo', 'Demo', 'demo@example.local',
        '$2a$11$S660mvYRwc14ZmEKdhSlKOuxstYi4h4jbi.q5GJtc1JNBYBbMpHqC');

DECLARE @UserId INT = SCOPE_IDENTITY();

INSERT INTO dbo.Leagues (Name, Code, Created)
VALUES ('Eredivisie', 'NED', SYSUTCDATETIME());

DECLARE @LeagueId INT = SCOPE_IDENTITY();

INSERT INTO dbo.LeagueMembers (LeagueId, UserId, Joined)
VALUES (@LeagueId, @UserId, SYSUTCDATETIME());

DECLARE @KickoffUtc DATETIME2(0) = DATEADD(day, 7, SYSUTCDATETIME());

EXEC dbo.MatchDayPredictionsApi_CreateMatch
    @LeagueId = @LeagueId,
    @HomeTeam = 'Ajax',
    @AwayTeam = 'PSV',
    @KickoffUtc = @KickoffUtc;

COMMIT TRANSACTION;
GO
