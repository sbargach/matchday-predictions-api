# Matchday Predictions API

Backend for football score predictions with friends: users join leagues, submit predictions before kickoff, and earn points based on real match results.

## Tech Stack
- .NET 9 (ASP.NET Core controllers)
- SQL Server + Dapper data access (stored procedures)
- JWT bearer authentication (BCrypt password hashing)
- OpenTelemetry metrics + Prometheus
- Serilog structured logging

## Authentication & Authorization
The API uses stateless JWT bearer tokens. Typical flow:

1. **Register** (anonymous)  
   `POST /api/users`

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
   `POST /api/auth/login`

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
   - `GET /api/users/me` – current user profile
   - `POST /api/predictions` – only submit predictions for your own user id
   - `GET /api/predictions/{matchId}?userId=...` – only retrieve your own predictions

## Main Endpoints (v1)

- **Auth & Users**
  - `POST /api/users` – create a user (anonymous)
  - `GET /api/users/me` – get current authenticated user
  - `POST /api/auth/login` – obtain JWT token

- **Leagues**
  - `GET /api/leagues` – list leagues
  - `GET /api/leagues/{leagueId}` – league details

- **Matches**
  - `GET /api/matches?leagueId={leagueId}` – matches for a league
  - `GET /api/matches/{matchId}` – match details
  - `POST /api/matches` – create a match (authenticated)

- **Predictions**
  - `POST /api/predictions` – create/update a prediction for a match
  - `GET /api/predictions/{matchId}?userId={userId}` – get prediction for a match/user
*** End Patch`"} ***!
