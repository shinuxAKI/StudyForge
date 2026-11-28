# StudyForge

AI-powered study, quiz, and practice platform.

StudyForge converts learner-provided study material into interactive practice activities: multiple-choice, true/false, fill-in-the-blank, short-answer, mixed tests, and flashcards.

## Project status
Implementation follows the 21-day plan in docs/PROJECT_PLAN.md and the project proposal.

- [x] Day 1 — Requirements, roles, MVP scope, acceptance criteria
- [x] Day 2 — Use-case, activity, sequence, and ERD diagrams
- [x] Day 3 — API/Web/Domain/Application/Infrastructure/Tests solution scaffold; SQLite, EF Core, and Swagger configuration
- [x] Day 4 — Identity registration/login, JWT access tokens, rotating refresh tokens, and role authorization code
- [x] Day 5 — Subjects, study-material CRUD, and owner-scoped access code
- [x] Day 6 — Question sets, question types, and validation code
- [x] Day 7 — Persistence/API integration and initial scoring unit tests added; database migration and full verification still pending
- [x] Day 8 — Test attempts, server-side scoring, persisted answers, and result review code
- [x] Day 9 — Strict JSON Schema AI generation, server-side validation, and safe unavailable-AI response code

## Important verification status
The SQLite provider is configured in the application. The local EF migration, build, automated test run, SQLite integration test, and live AI provider request must still be executed before the integration gate can be considered passed.

## Architecture
StudyForge.Web → HTTPS/JSON → StudyForge.API → Application contracts/services → Domain → Infrastructure / EF Core / SQLite.
The browser never receives AI provider credentials or JWT signing secrets.

## Technology
- .NET 10 / ASP.NET Core
- SQLite and Entity Framework Core
- ASP.NET Core Identity and JWT bearer authentication
- Swagger / OpenAPI
- Responsive frontend (HTML, CSS, Bootstrap 5, JavaScript)

## Setup and verification
- Local setup: docs/SETUP.md
- Day 4 authentication: docs/DAY-4-AUTHENTICATION.md
- Days 5–9 implementation: docs/DAY-5-TO-9.md
- Testing checklist: docs/TESTING.md
- Project plan: docs/PROJECT_PLAN.md
