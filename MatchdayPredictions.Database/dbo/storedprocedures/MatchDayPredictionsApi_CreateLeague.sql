CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateLeague
    @Name NVARCHAR(100),
    @Code NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SET TRANSACTION ISOLATION LEVEL SNAPSHOT;

    INSERT INTO dbo.Leagues (Name, Code)
    SELECT @Name, @Code
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Leagues WHERE Code = @Code);
END
GO
