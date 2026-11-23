# StudyForge implementation plan

## Source of truth
This plan implements the StudyForge project proposal. The repository records actual implementation status; a task is complete only after its artifacts exist and any applicable checks pass.

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
10. Implement multiple-choice, true/false, fill-in, and short-answer flows.
11. Implement flashcards, review state, and mistake review.
12. Build dashboard, progress statistics, and test history.
13. Add SignalR/live updates and notifications if required.
14. Week 2 integration gate: generation → test → score → review workflow.
15. Build responsive landing, login, and study-material screens.
16. Build test builder, test player, and question navigation.
17. Build results, flashcards, progress, and admin screens.
18. Security, API, unit/integration, and edge-case testing.
19. Usability, accessibility, mobile layout, and performance pass.
20. Complete documentation, screenshots, ERD/UML, test evidence, and installation guide.
21. Final demo and package source code, database/scripts, PDF, tests, and presentation.

## Current milestone
- Day 1 requirements and MVP scope: documented.
- Day 2 design diagrams: documented in `docs/diagrams/`.
- Day 3 solution scaffold: in progress; see `docs/SETUP.md`.

## MVP priorities
The first usable release prioritizes account access, private study subjects/material, generation of practice questions from supplied material, answering and scoring tests, reviewing mistakes, and flashcards. Teacher/class features, SignalR, adaptive progression, and advanced analytics remain optional extensions unless later requirements make them mandatory.
