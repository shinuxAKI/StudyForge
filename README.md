# StudyForge

AI-powered study, quiz, and practice platform.

StudyForge converts learner-provided study material into interactive practice activities: multiple-choice, true/false, fill-in-the-blank, short-answer, mixed tests, and flashcards.

## Project status

Implementation follows the 21-day plan in [docs/PROJECT_PLAN.md](docs/PROJECT_PLAN.md) and the project proposal.

- [x] Day 1 — Requirements, roles, MVP scope, acceptance criteria
- [x] Day 2 — Use-case, activity, sequence, and ERD diagrams
- [x] Day 3 — API/Web/Domain/Application/Infrastructure/Tests solution scaffold; SQL Server, EF Core, and Swagger configuration
- [x] Day 4 — Identity registration/login, JWT access tokens, rotating refresh tokens, and role authorization code
- [ ] Local verification — EF migration, build, automated tests, and API endpoint checks still need to be run

## Architecture

`StudyForge.Web` → HTTPS/JSON → `StudyForge.API` → Application contracts/services → Domain → Infrastructure / EF Core / SQL Server.

The browser does not communicate directly with the AI provider. External AI credentials and all signing secrets remain server-side.

## Technology

- .NET 10 / ASP.NET Core
- C#
- SQL Server and Entity Framework Core
- ASP.NET Core Identity and JWT bearer authentication
- Swagger / OpenAPI
- Responsive frontend (HTML, CSS, Bootstrap 5, JavaScript)

## Setup and verification

- [Local setup](docs/SETUP.md)
- [Day 4 authentication](docs/DAY-4-AUTHENTICATION.md)
- [Testing checklist](docs/TESTING.md)
- [Project plan](docs/PROJECT_PLAN.md)

## Scope note

A feature is not considered tested or complete until its local build and relevant checks have actually passed. The current branch contains implementation code; it has not yet been verified on a local .NET 10 machine.
