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
