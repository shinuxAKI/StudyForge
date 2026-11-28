# StudyForge implementation plan

## Source of truth
This plan follows the StudyForge project proposal. The repository records implementation evidence; a task is only marked implemented when its artifacts exist, and tested only after its checks have actually run.

## 21-day roadmap
1. Freeze requirements, roles, acceptance criteria, and MVP scope.
2. Design use-case, activity, sequence, and ERD diagrams.
3. Create API/Web/Domain/Application/Infrastructure/Tests projects; configure SQL Server, EF Core, and Swagger.
4. Implement registration, login, JWT, refresh tokens, and role policies.
5. Build subjects, study-material CRUD, and ownership rules.
6. Build question-set/question models and validation.
7. Week 1 integration gate: API workflows, migrations, and automated tests.
8. Implement test engine, scoring, and answer persistence.
9. Integrate AI question generation with strict JSON schema and fallback.
10. Implement multiple choice, true/false, fill-in and short-answer flows.
11. Implement flashcards, review state and mistake review.
12. Build dashboard, progress statistics and test history.
13. Add SignalR/live updates and notifications if required.
14. Week 2 integration gate: generation → test → score → review workflow.
15. Build responsive landing, login and study-material screens.
16. Build test builder, test player and question navigation.
17. Build results, flashcards, progress and admin screens.
18. Security, API, unit/integration and edge-case testing.
19. Usability, accessibility, mobile layout and performance pass.
20. Complete documentation, screenshots, ERD/UML, test evidence and installation guide.
21. Final demo and package source code, database/scripts, PDF, tests, and presentation.

## Milestone status
- Day 1 — Requirements and MVP scope documented in `docs/requirements/MVP_SCOPE.md`.
- Day 2 — Design diagrams documented in `docs/diagrams/`.
- Day 3 — Solution scaffold and infrastructure configuration committed.
- Day 4 — Identity registration/login, JWT, refresh-token rotation, role seeding and role policies implemented. See `docs/DAY-4-AUTHENTICATION.md`.
- Day 5 — Subject and study-material CRUD with owner-scoped API queries implemented. See `docs/DAY-5-TO-9.md`.
- Day 6 — Question sets, four question types and server-side validation implemented.
- Day 7 — Persistence/API integration and scoring unit tests added; migration, full build and runtime verification are still pending.
- Day 8 — Test attempts, answer persistence, server-side scoring and result review implemented.
- Day 9 — Strict JSON Schema AI question generation, server-side output validation and unavailable-AI fallback path implemented.
- Verification — Local EF migration, .NET build, tests and endpoint checks remain pending. See `docs/TESTING.md`.

## MVP priorities
The first usable release prioritizes account access, private study subjects/material, generation of practice questions from supplied material, answering and scoring tests, reviewing mistakes, and flashcards. Teacher/class features, SignalR, adaptive progression, and advanced analytics remain optional extensions unless later requirements make them mandatory.
