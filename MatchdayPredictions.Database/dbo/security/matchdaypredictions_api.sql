CREATE ROLE [db_matchdaypredictionsapi]
GO

GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_CreateUser] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetUserById] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetUserByUsername] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_CreateMatch] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetLeagues] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetLeagueById] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetMatchById] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetMatchesByLeague] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_SetPrediction] TO [db_matchdaypredictionsapi]
GO
GRANT EXECUTE ON [dbo].[MatchDayPredictionsApi_GetPrediction] TO [db_matchdaypredictionsapi]
GO
