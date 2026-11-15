# Matchday Predictions API

ASP.NET Core API for browsing leagues and matches and submitting score predictions. Users can register, sign in, and create or update their own predictions before kickoff.

## Tech Stack
- .NET 10 (ASP.NET Core controllers)
- SQL Server + Dapper data access (stored procedures)
- JWT bearer authentication (BCrypt password hashing)
- OpenTelemetry metrics + Prometheus
- Serilog structured logging

## Authentication & Authorization
The API uses stateless JWT bearer tokens.

1. **Register** (anonymous)  
   `POST /api/v1/users`

   Body:
   ```json
   {
     "username": "jane.doe",
     "displayName": "Jane",
     "email": "jane@example.com",
     "password": "P@ssw0rd!"
   }
   ```

2. **Login** (anonymous)  
   `POST /api/v1/auth/login`

   Body:
   ```json
   {
     "username": "jane.doe",
     "password": "P@ssw0rd!"
   }
   ```

   Response:
   ```json
   {
     "token": "<jwt-token>"
   }
   ```

3. **Authenticated requests**  
   Send the token in the `Authorization` header:

   ```http
   Authorization: Bearer <jwt-token>
   ```

   The API uses the `NameIdentifier` claim as the current user id to enforce:
   - `GET /api/v1/users/me` — current user profile
   - `POST /api/v1/predictions` — submit only for your own user id and a league you belong to
   - `GET /api/v1/predictions/{matchId}?userId=...` — only retrieve your own predictions

League membership is stored in SQL Server. The API does not yet provide league joining or score calculation.

## Main Endpoints (v1)

- **Auth & Users**
  - `POST /api/v1/users` — create a user (anonymous)
  - `GET /api/v1/users/me` — get current authenticated user
  - `POST /api/v1/auth/login` — obtain JWT token (rate limited)

- **Leagues**
  - `GET /api/v1/leagues` — list leagues
  - `GET /api/v1/leagues/{leagueId}` — league details

- **Matches**
  - `GET /api/v1/matches?leagueId={leagueId}` — matches for a league
  - `GET /api/v1/matches/{matchId}` — match details
  - `POST /api/v1/matches` — create a match (authenticated)

- **Predictions**
  - `POST /api/v1/predictions` — create a prediction (`201`) or update one (`200`) before kickoff; missing matches return `404`, non-members `403`, and closed matches `409`
  - `GET /api/v1/predictions/{matchId}?userId={userId}` — get prediction for a match/user

## Observability & Docs
- OpenTelemetry (metrics) + Prometheus endpoint at `/metrics`
- Health check at `/health`
- Swagger UI enabled in Development at `/swagger`
- Serilog console logging

## Running locally
- API: `dotnet run --project MatchdayPredictions.Api/MatchdayPredictions.Api.csproj`
- Unit tests: `dotnet test MatchdayPredictions.Api.Tests/MatchdayPredictions.Api.Tests.csproj`
- Integration tests: require Docker (SQL Server Testcontainer). Run `dotnet test MatchdayPredictions.Api.IntegrationTests/MatchdayPredictions.Api.IntegrationTests.csproj`

The database project requires Visual Studio SSDT. CLI builds target the API and test projects directly; integration tests apply the SQL scripts to a temporary SQL Server container.
