CREATE PROCEDURE dbo.MatchDayPredictionsApi_CreateUser
    @UserName     NVARCHAR(50),
    @DisplayName  NVARCHAR(100),
    @Email        NVARCHAR(255),
    @PasswordHash NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.Users (UserName, DisplayName, Email, PasswordHash)
        VALUES (@UserName, @DisplayName, @Email, @PasswordHash);

        SELECT CAST(1 AS BIT) AS Created;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            SELECT CAST(0 AS BIT) AS Created;
        ELSE
            THROW;
    END CATCH
END
GO
