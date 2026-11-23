# StudyForge

AI-powered study, quiz, and practice platform.

StudyForge converts learner-provided study material into interactive practice activities, including multiple-choice, true/false, fill-in-the-blank, short-answer, mixed tests, and flashcards.

## Project status

Implementation follows the approved 21-day plan in [docs/PROJECT_PLAN.md](docs/PROJECT_PLAN.md).

- [x] Day 1 — Requirements, roles, MVP scope, acceptance criteria
- [x] Day 2 — Use-case, activity, sequence, and ERD diagrams
- [ ] Day 3 — .NET solution and layered project scaffold; SQL Server, EF Core, and Swagger configuration
- [ ] Day 4 onward — Authentication and feature implementation

## Architecture

`StudyForge.Web` → HTTPS/JSON → `StudyForge.API` → Application services and validation → Domain → Infrastructure / EF Core / SQL Server.

External AI credentials and other secrets must remain server-side and must never be committed.

## Technology

- .NET 10 / ASP.NET Core
- C#
- SQL Server and Entity Framework Core
- ASP.NET Core Web API
- Swagger / OpenAPI
- Responsive frontend (HTML, CSS, Bootstrap 5, JavaScript)

## Getting started

Day 3 setup instructions will be documented in [docs/SETUP.md](docs/SETUP.md) once the solution scaffold is committed.

## Scope note

This repository tracks implementation evidence. A planned feature is not considered complete until it has been implemented and tested.
