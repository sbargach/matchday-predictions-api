# Changelog (Database)
This project follows Keep a Changelog and Semantic Versioning.

## [Unreleased]

## [0.5.0] - 2025-11-15
### Changed
- Removed unused stored procedures not referenced by the API.
- Added and wired stored procedures for leagues, matches, predictions, and user lookups to match the API data access layer.
- Ensured `MatchDayPredictionsApi_CreateMatch` generates globally unique match IDs across leagues.

## [0.4.0] - 2025-11-15
### Added
- Changes for JWT authorization

## [0.3.0] - 2025-10-05
### Added
- Added core tables (Leagues, LeagueMembers, LeagueMatches, Predictions) and stored procedures for joining leagues and submitting predictions.

## [0.2.0] - 2025-10-01
### Added
- Add first stored procedures.

## [0.1.0] - 2025-09-30
### Added
- Tables: Users, Leagues, LeagueMembers
