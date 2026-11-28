# StudyForge verification checklist

This file records checks to execute against a local .NET 10 environment. Do not mark a test as passed until it has actually been run and its result recorded.

## Build checks
- [ ] `dotnet restore StudyForge.sln`
- [ ] `dotnet build StudyForge.sln --no-restore`
- [ ] `dotnet test StudyForge.sln --no-build`

## Day 4 authentication checks
1. Configure the JWT signing key using User Secrets.
2. Generate and apply the Identity/refresh-token EF Core migration.
3. Start the API and open Swagger.
4. Register a student; expect HTTP 201 and the Student role.
5. Register the same email again; expect HTTP 400.
6. Log in with a correct password; expect HTTP 200.
7. Log in with an incorrect password; expect HTTP 401.
8. Use a Student access token on `GET /api/admin/access`; expect HTTP 403.
9. Refresh a valid refresh token; expect HTTP 200 and a replacement token.
10. Attempt to refresh with the old token; expect HTTP 401.
11. Log out with the replacement refresh token; expect HTTP 204.
12. Attempt to refresh with the logged-out token; expect HTTP 401.

## Evidence
Record the date, commit SHA, environment, command or request, actual result, and any failure details here after execution. No test result is assumed by this checklist.

## Days 10–12 API verification
After applying the latest SQLite migration and starting the API, use an authenticated Student token:

1. Create a subject and study material.
2. Create a question set containing multiple-choice, true/false, fill-in-blank, and short-answer questions.
3. Start the test with `POST /api/tests/{questionSetId}/start`; confirm correct answers are not included in the response.
4. Submit exactly one answer per question with `POST /api/tests/{attemptId}/submit`; confirm server-calculated score and answer review.
5. Fetch `GET /api/tests/{attemptId}/results`; confirm only the owning user can access it.
6. Fetch `GET /api/flashcards?dueOnly=true`; confirm only the current user's due cards are returned.
7. Review a card with `POST /api/flashcards/{questionId}/review` and each rating: Again, Hard, Good, Easy. Confirm a future due date is returned.
8. Fetch `GET /api/dashboard`; confirm counts, average score, due cards and recent tests match the data created above.
9. Attempt to access another user's subject, question set, attempt and flashcard; expect no data disclosure (404 or empty own-data response).

These are manual verification steps, not yet recorded as passed.
