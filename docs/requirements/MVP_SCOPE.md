# Day 1 — Requirements, roles, and MVP scope

## Product statement
StudyForge turns learner-provided study material into structured practice activities. A learner can add material, choose a practice mode and difficulty, complete a session, see a score and explanations, and review mistakes.

## User roles
| Role | Permissions / responsibilities |
|---|---|
| Guest | View the landing page and public product/demo information; register or sign in. |
| Student | Manage their own subjects and study material; generate and take practice sessions; view their own results, progress, and flashcards. |
| Teacher (optional phase) | Create and share class study sets where class-sharing is enabled. Not required for the initial MVP. |
| Administrator | Manage users, reports, categories, moderation, and system statistics. |

## MVP features
1. Account registration and login (implementation scheduled for Day 4).
2. Student-owned subjects and study material (paste/type first; file upload can follow).
3. Question-set generation from supplied material.
4. Supported question modes: multiple choice, true/false, fill-in-the-blank, and short answer.
5. Test configuration: mode, question count, and difficulty.
6. Test attempt, answer submission, scoring, and result review.
7. Explanations and targeted review of incorrect answers.
8. Basic flashcard sets and known/unknown review state.
9. Basic dashboard and test history after core workflows are stable.

## Explicitly deferred / optional
- Teacher classrooms and shared class sets.
- SignalR live events.
- Adaptive difficulty and spaced repetition.
- Advanced analytics, streaks, and notifications.
- Arabic/English bilingual support.
- Sophisticated file parsing; pasted text remains the fallback.

## Functional acceptance criteria
- A guest can reach the registration and login screens.
- An authenticated student can create and list their own subject.
- A student can save study material under a subject they own.
- A student can request a question set using their supplied material and selected configuration.
- Generated questions conform to the selected supported question type and include an answer and concise explanation where applicable.
- Invalid or malformed generated output is rejected or safely replaced by deterministic fallback content.
- A student can submit answers and receive a correct score and question breakdown.
- A student can inspect incorrect answers and retry missed questions.
- A student cannot read or modify another student's private materials or results.
- Admin-only functions reject non-admin users.
- Primary workflows are keyboard accessible and responsive.

## Non-functional acceptance criteria
- Secrets and AI provider credentials remain server-side.
- HTTPS is used outside local development.
- DTOs and request inputs are validated server-side.
- Database access is asynchronous where appropriate.
- Errors and loading states are clear; no sensitive exception details are exposed to end users.
- EF Core migrations are committed and reproducible once the schema is introduced.
- Automated tests cover core validation, ownership, scoring, and authorization rules.

## MVP workflow
Register / sign in → create subject → add material → choose practice settings → generate questions → take test → submit → view score and explanations → review mistakes / practice again.

## Day 1 exit checklist
- [x] Product purpose and target users recorded.
- [x] Roles and access boundaries recorded.
- [x] MVP and deferred features separated.
- [x] Functional and non-functional acceptance criteria recorded.
- [x] End-to-end learner journey recorded.
