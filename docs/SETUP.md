# Day 3 — Local setup

## Prerequisites
- .NET 10 SDK
- Visual Studio 2026 with the ASP.NET and web development workload, or the .NET CLI
- SQL Server / SQL Server Express / LocalDB
- Git

## Projects
- `StudyForge.API` — ASP.NET Core Web API and Swagger.
- `StudyForge.Web` — separate ASP.NET Core MVC frontend.
- `StudyForge.Domain` — domain layer.
- `StudyForge.Application` — use cases and application contracts.
- `StudyForge.Infrastructure` — EF Core and SQL Server persistence.
- `StudyForge.Tests` — automated tests.

## Configure the database
The API reads the connection string named `StudyForgeDb`. The committed development default uses SQL Server LocalDB. For SQL Server Express or another instance, override it in user secrets or environment variables.

Example environment variable (PowerShell):
```powershell
$env:ConnectionStrings__StudyForgeDb = "Server=localhost;Database=StudyForgeDb;Trusted_Connection=True;TrustServerCertificate=True"
```

Do not commit passwords, production connection strings, JWT signing keys, or AI provider keys.

## Restore and build
From the repository root:
```powershell
dotnet restore StudyForge.sln
dotnet build StudyForge.sln
dotnet test StudyForge.sln
```

## Run
Start the API:
```powershell
dotnet run --project src/StudyForge.API
```

Open the Swagger UI at the HTTPS URL printed by the API process, followed by `/swagger`.

Start the web frontend in a second terminal:
```powershell
dotnet run --project src/StudyForge.Web
```

The API URL used by the web project is configured with `ApiBaseUrl` in `src/StudyForge.Web/appsettings.json`. Update it to match the API HTTPS URL printed by your local run profile.

## EF Core migrations
The Day 3 scaffold registers SQL Server and provides the DbContext. The first migration will be added when the first persistent domain entities are introduced:
```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialCreate --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
dotnet ef database update --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
```

## Current milestone boundary
Day 3 creates the solution structure and infrastructure wiring. Registration/JWT, domain CRUD, AI generation, and test workflows are intentionally scheduled for later days.
