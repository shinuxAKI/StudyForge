# StudyForge — local setup

## Prerequisites
- .NET 10 SDK
- Visual Studio 2026 with the ASP.NET and web development workload, or the .NET CLI
- SQL Server / SQL Server Express / LocalDB
- Git

## Solution projects
- `StudyForge.API` — ASP.NET Core Web API, Swagger, Identity and JWT bearer authentication.
- `StudyForge.Web` — separate ASP.NET Core MVC frontend.
- `StudyForge.Domain` — domain layer.
- `StudyForge.Application` — use cases and application contracts.
- `StudyForge.Infrastructure` — Identity persistence, EF Core and SQL Server.
- `StudyForge.Tests` — automated tests.

## Database configuration
The API reads the connection string named `StudyForgeDb`. The committed development default uses SQL Server LocalDB. Override it in User Secrets or with an environment variable if using SQL Server Express or another instance.

PowerShell example:
```powershell
$env:ConnectionStrings__StudyForgeDb = "Server=localhost;Database=StudyForgeDb;Trusted_Connection=True;TrustServerCertificate=True"
```

## JWT signing key
The signing key is deliberately excluded from source control. Set a local key using User Secrets:

```powershell
dotnet user-secrets set "Jwt:SigningKey" "replace-this-with-a-random-secret-at-least-32-bytes-long" --project src/StudyForge.API
```

Use a randomly generated secret of at least 32 UTF-8 bytes. Never commit signing keys or production connection strings.

## Restore, build and test
From the repository root:

```powershell
dotnet restore StudyForge.sln
dotnet build StudyForge.sln
dotnet test StudyForge.sln
```

## Database migrations
The current schema includes ASP.NET Core Identity tables and refresh-token persistence. Generate and apply the initial migration:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
dotnet ef migrations add AddIdentityAndRefreshTokens --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API --output-dir Persistence/Migrations
dotnet ef database update --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
```

Apply the migration before starting the API.

## Run the API
```powershell
dotnet run --project src/StudyForge.API
```

Open the Swagger UI at the HTTPS URL printed by the API process, followed by `/swagger`.

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
