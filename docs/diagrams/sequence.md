# Day 2 — Question-generation sequence diagram

```mermaid
sequenceDiagram
    actor Student
    participant Web as StudyForge.Web
    participant API as StudyForge.API
    participant App as Application Services
    participant DB as SQL Server
    participant AI as AI Provider

    Student->>Web: Select material and test settings
    Web->>API: POST /api/question-sets/generate
    API->>App: GenerateQuestionSet(request, userId)
    App->>DB: Verify material ownership and load content
    DB-->>App: Owned study material
    App->>AI: Request structured questions
    alt AI response is valid
        AI-->>App: Structured question JSON
        App->>App: Validate schema, duplicates and source support
    else AI unavailable or invalid
        AI-->>App: Error or malformed output
        App->>App: Use deterministic fallback or fail safely
    end
    App->>DB: Save question set and questions
    DB-->>App: Saved identifiers
    App-->>API: QuestionSetDto
    API-->>Web: 201 Created / response DTO
    Web-->>Student: Display generated practice set
```

## Design rules
- The browser never calls the AI provider directly.
- The API derives the authenticated user identity from the validated authentication context, not from a client-supplied owner ID.
- AI output is untrusted until server-side validation succeeds.
