#!/bin/bash
set -euo pipefail

sqlcmd=/opt/mssql-tools18/bin/sqlcmd
server=db
password="$MSSQL_SA_PASSWORD"

for attempt in {1..30}; do
    if "$sqlcmd" -C -S "$server" -U sa -P "$password" -Q "SELECT 1" -b -o /dev/null 2>/dev/null; then
        break
    fi
    if [ "$attempt" -eq 30 ]; then
        echo "SQL Server did not become ready" >&2
        exit 1
    fi
    sleep 2
done

exists=$("$sqlcmd" -C -S "$server" -U sa -P "$password" -h -1 -W -Q \
    "SET NOCOUNT ON; SELECT CASE WHEN DB_ID('MatchdayPredictions') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')
if [ "$exists" = "1" ]; then
    seeded=$("$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -h -1 -W -Q \
        "SET NOCOUNT ON; SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'demo') THEN 1 ELSE 0 END" | tr -d '[:space:]')
    if [ "$seeded" = "1" ]; then
        echo "Demo database already exists"
        exit 0
    fi
    "$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b -i /setup/demo-seed.sql
    echo "Demo database ready"
    exit 0
fi

"$sqlcmd" -C -S "$server" -U sa -P "$password" -b -Q "CREATE DATABASE MatchdayPredictions"
"$sqlcmd" -C -S "$server" -U sa -P "$password" -b -Q \
    "ALTER DATABASE MatchdayPredictions SET ALLOW_SNAPSHOT_ISOLATION ON; ALTER DATABASE MatchdayPredictions SET READ_COMMITTED_SNAPSHOT ON"

for name in Users Leagues LeagueMatches Predictions LeagueMembers; do
    "$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b -i "/schema/tables/$name.sql"
done

for file in /schema/storedprocedures/*.sql; do
    "$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b -i "$file"
done

"$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b \
    -i /schema/security/matchdaypredictions_api.sql
"$sqlcmd" -C -S "$server" -U sa -P "$password" -b \
    -Q "CREATE LOGIN matchday_api WITH PASSWORD = 'LocalApi_12345!'"
"$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b \
    -Q "CREATE USER matchday_api FOR LOGIN matchday_api; ALTER ROLE db_matchdaypredictionsapi ADD MEMBER matchday_api"
"$sqlcmd" -C -S "$server" -U sa -P "$password" -d MatchdayPredictions -b \
    -i /setup/demo-seed.sql

echo "Demo database ready"
