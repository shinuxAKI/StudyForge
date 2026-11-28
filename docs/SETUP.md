# StudyForge — local setup

## Prerequisites
- .NET 10 SDK
- Visual Studio 2026 with the ASP.NET and web development workload, or the .NET CLI
- Git

SQLite is included through the EF Core SQLite provider; a separate SQL Server installation is not required.

## Solution projects
- `StudyForge.API` — ASP.NET Core Web API, Swagger, Identity and JWT bearer authentication.
- `StudyForge.Web` — separate ASP.NET Core MVC frontend.
- `StudyForge.Domain` — domain layer.
- `StudyForge.Application` — use cases and application contracts.
- `StudyForge.Infrastructure` — Identity persistence, EF Core and SQLite.
- `StudyForge.Tests` — automated tests.

## Database configuration
The API reads the connection string named `StudyForgeDb`. The default is a local SQLite file named `studyforge.db` in the API working directory:

```json
"ConnectionStrings": {
  "StudyForgeDb": "Data Source=studyforge.db"
}
```

You can override it with User Secrets or the `ConnectionStrings__StudyForgeDb` environment variable. For example:

```powershell
$env:ConnectionStrings__StudyForgeDb = "Data Source=C:\StudyForgeData\studyforge.db"
```

Create the parent directory before starting the API if you use a custom file path. Back up the database file to back up local StudyForge data.

## JWT signing key
The signing key is deliberately excluded from source control. A local key is already stored in User Secrets if you followed the setup steps. To replace it, use a randomly generated secret of at least 32 UTF-8 bytes:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "replace-this-with-a-random-secret-at-least-32-bytes-long" --project src/StudyForge.API
```

Never commit signing keys or share them in screenshots or logs.

## Restore, build and test
From the repository root:

```powershell
dotnet restore StudyForge.sln
dotnet build StudyForge.sln
dotnet test StudyForge.sln
```

## Database migrations
The current schema includes ASP.NET Core Identity, refresh-token persistence, subjects, study materials, question sets, questions, test attempts, and submitted answers.

If you previously generated migrations with the SQL Server provider, do not reuse those migration files for SQLite. Remove the old un-applied SQL Server migration files from `src/StudyForge.Infrastructure/Persistence/Migrations` (keep a backup first), then generate a fresh SQLite migration:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
dotnet ef migrations add InitialSqliteSchema --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API --output-dir Persistence/Migrations
dotnet ef database update --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
```

If `dotnet-ef` is already installed, use `dotnet tool update --global dotnet-ef --version 10.0.15` instead of the install command. Apply the migration before starting the API. Do not delete an existing database file if it contains data you need.

## Run the API
```powershell
dotnet run --project src/StudyForge.API
```

Open Swagger at the HTTPS URL printed by the API process, followed by `/swagger`.

## Run the web frontend
In a second terminal:

```powershell
dotnet run --project src/StudyForge.Web
```

The API URL used by the web project is configured in `src/StudyForge.Web/appsettings.json`. Update it to match the API HTTPS URL printed by your local run profile.

## Day 4 authentication
See [DAY-4-AUTHENTICATION.md](DAY-4-AUTHENTICATION.md) for endpoint payloads, role behavior, token handling and verification steps.

## Important milestone boundary
Day 4 implements account registration, login, JWT access tokens, rotating refresh tokens and role policies. Subjects, study materials, question generation, tests, scoring and flashcards are later milestones.
