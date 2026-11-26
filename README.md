# Matchday Predictions API

I made this to try a small football predictions app with .NET, SQL Server, and Dapper. People can sign in, browse fixtures, and change their score prediction until kickoff. There is no frontend; Swagger is enough for now.

## Try it

You need Docker. Run this from the repo root:

```sh
docker compose up --build -d
pwsh ./scripts/smoke.ps1
```

The API is at [localhost:8081/swagger](http://localhost:8081/swagger). The demo login is `demo` / `Demo12345`. The seeded Eredivisie league has one match a week in the future. The smoke script signs in, reads the league and match, creates another match, makes and updates a prediction, and checks that a new user cannot predict in a league they have not joined.

The Docker passwords are only for this local demo. To reset its database, run `docker compose down -v` and start it again.

## What I wanted to get right

The prediction cutoff and league membership check happen in the SQL transaction that writes the prediction. The database has a primary key on `(LeagueId, MatchId, UserId)`, so two requests cannot create duplicate predictions. Match creation returns the ID from the database; the API uses it in the `Location` header.

I used stored procedures with Dapper because the writes are small but the rules sit close to the data. The API maps the result to HTTP responses. I have not added indexes just to make the schema look busy. If the data grows, I would start with the actual query plan and logical reads for the league match list and prediction write, then tune what is slow.

Any signed-in user can currently add a match. That is fine for this experiment, but a real league would need an owner or admin rule. League joining and automatic scoring are not built yet.

## Tests

```sh
dotnet test MatchdayPredictions.Api.Tests/MatchdayPredictions.Api.Tests.csproj
dotnet test MatchdayPredictions.Api.IntegrationTests/MatchdayPredictions.Api.IntegrationTests.csproj
```

The integration tests start SQL Server with Testcontainers and apply the database scripts. The Docker smoke script tests the running API and database together over HTTP.
