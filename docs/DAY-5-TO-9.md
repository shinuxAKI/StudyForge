# Days 5–9 — Study content, questions, assessment and AI

## Day 5 — Subjects, study-material CRUD and ownership
Authenticated users can create/list/edit/delete their own subjects and materials. Reads and writes are scoped to the authenticated user; foreign-owned IDs return 404. Subject name max 100, description max 500, material title max 160 and content max 100,000 characters.
Routes: GET/POST /api/subjects; PUT/DELETE /api/subjects/{id}; GET/POST /api/subjects/{id}/materials; PUT/DELETE /api/materials/{id}.

## Day 6 — Question sets and validation
Question sets belong to a user and may reference only that user's material. Supports MultipleChoice, TrueFalse, FillInBlank and ShortAnswer. Validates prompts, answers, difficulty, option count and exactly one correct option for choice questions.
Routes: GET/POST /api/question-sets; GET/DELETE /api/question-sets/{id}.

## Day 7 — Integration gate
The persistence model and API workflows are connected to the SQL Server EF Core context. This gate is NOT verified until an EF migration, build and automated tests have actually passed.

## Day 8 — Test engine, scoring and answer persistence
Students start tests from their own question sets. Start returns prompts/options without answer keys. Submission requires exactly one answer per question and is single-use. Correctness and percentage are calculated server-side; results are persisted and reviewable.
Routes: POST /api/tests/{questionSetId}/start; POST /api/tests/{attemptId}/submit; GET /api/tests/{attemptId}/results.

## Day 9 — AI question generation
The API calls a server-side OpenAI-compatible chat-completions endpoint using strict JSON Schema output. The server validates count, uniqueness, types, difficulty, option rules and answer consistency again before saving. API keys stay in User Secrets/environment variables.
Route: POST /api/ai/questions/generate with studyMaterialId, count (1–20), and optional title.
If AI is missing/unavailable or output is invalid, generation returns 503 and saves nothing. Previously saved question sets remain available as the fallback study path.

## AI configuration
PowerShell from repository root:
dotnet user-secrets set "AI:Endpoint" "https://provider.example/v1/chat/completions" --project src/StudyForge.API
dotnet user-secrets set "AI:ApiKey" "YOUR_SECRET_KEY" --project src/StudyForge.API
dotnet user-secrets set "AI:Model" "YOUR_SCHEMA_CAPABLE_MODEL" --project src/StudyForge.API
Use a provider/model that supports strict JSON Schema response formats. Never commit provider credentials.

## Migration and verification
From repository root:
dotnet tool update --global dotnet-ef --version 10.0.15
dotnet ef migrations add AddStudyContentQuestionsAndTests --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API --output-dir Persistence/Migrations
dotnet ef database update --project src/StudyForge.Infrastructure --startup-project src/StudyForge.API
dotnet build StudyForge.sln
dotnet test StudyForge.sln

Point ConnectionStrings:StudyForgeDb to your SQL Server before applying the migration. The design-time factory defaults to LocalDB; set ConnectionStrings__StudyForgeDb for another SQL Server. No local migration, build, integration test or live AI provider call has been run in this environment; do not mark those checks passed. AI-generated content can still be wrong and should be reviewed by the learner.
