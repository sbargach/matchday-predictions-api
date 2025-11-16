# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [0.16.0] - 2025-11-16
### Changed
- Update to .NET 10.
- Add Dockerfile
- Add github actions

## [0.15.0] - 2025-11-16
### Added
- Enabled OpenTelemetry tracing for ASP.NET Core and HTTP client requests with OTLP export.
- Introduced an in-memory caching decorator for league queries to reduce database load.
- Added focused unit tests for prediction authorization rules and core model validation.

## [0.14.0] - 2025-11-15
### Changed
- Refactored data access into per-domain contexts (user, league, match, prediction) with a shared SQL base context.
- Aligned API stored procedure usage with the database project and removed unused database operations from the API surface.

## [0.13.0] - 2025-11-15
### Added
- Implemented BCrypt-based password hashing and secure JWT login flow.
- Aligned API user models and repositories with database schema (Users table, GetUserById/GetUserByUsername stored procedures).

## [0.12.0] - 2025-11-10
### Added
- Added model validation for request DTOs.

## [0.11.0] - 2025-11-09
### Added
- Added OpenTelemetry metrics and improved JWT authorization flow across the API.

## [0.10.0] - 2025-11-09
### Added
- Added OpenTelemetry metrics and improved JWT authorization flow across the API.

## [0.9.0] - 2025-11-08
### Added
- Added matches controller and integrated repository.

## [0.8.0] - 2025-11-02
### Added
- Added JWT authentication and secured controllers with authorization.

## [0.7.0] - 2025-10-26
### Added
- Added users controller and integrated repository.

## [0.6.0] - 2025-10-19
### Added
- Added leagues controller and integrated repository.

## [0.5.0] - 2025-10-18
### Added
- Added prediction repository and updated controller with real implementation.

## [0.4.0] - 2025-10-12
### Added
- Added Dapper-based data context with Polly retry policy.

## [0.3.0] - 2025-09-28
### Added
- Add get/post in Predictions controller
- Add model to request an match prediction

## [0.2.0] - 2025-09-27
### Added
- Add MatchPrediction model and first PredictionsController

## [0.1.0] - 2025-09-22
### Added
- Initial solution and API project (MatchdayPredictions.Api)
- NUnit test project (MatchdayPredictions.Api.Tests)
- `.gitignore` for .NET / Visual Studio
- Minimal README
- CHANGELOG file with version history
