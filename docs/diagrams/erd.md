# Day 2 — Initial ERD

This is the logical MVP data blueprint. It is not yet an EF Core migration; entities and constraints will be implemented in later milestones.

```mermaid
erDiagram
    USERS ||--o{ SUBJECTS : owns
    SUBJECTS ||--o{ STUDY_MATERIALS : contains
    SUBJECTS ||--o{ QUESTION_SETS : organizes
    STUDY_MATERIALS ||--o{ QUESTION_SETS : source_for
    QUESTION_SETS ||--|{ QUESTIONS : contains
    QUESTIONS ||--o{ QUESTION_OPTIONS : offers
    USERS ||--o{ TESTS : attempts
    QUESTION_SETS ||--o{ TESTS : used_by
    TESTS ||--o{ TEST_ANSWERS : records
    QUESTIONS ||--o{ TEST_ANSWERS : answered_in
    USERS ||--o{ FLASHCARD_SETS : owns
    SUBJECTS ||--o{ FLASHCARD_SETS : groups
    FLASHCARD_SETS ||--|{ FLASHCARDS : contains
    USERS ||--o{ PROGRESS : has
    USERS ||--o{ NOTIFICATIONS : receives
    USERS ||--o{ AUDIT_LOGS : triggers

    USERS {
      string Id PK
      string Email
      string DisplayName
    }
    SUBJECTS {
      int Id PK
      string OwnerId FK
      string Name
      string Description
    }
    STUDY_MATERIALS {
      int Id PK
      int SubjectId FK
      string Title
      string Content
      datetime CreatedAt
    }
    QUESTION_SETS {
      int Id PK
      int SubjectId FK
      int StudyMaterialId FK
      string Title
      string Difficulty
      datetime CreatedAt
    }
    QUESTIONS {
      int Id PK
      int QuestionSetId FK
      string Type
      string Prompt
      string CorrectAnswer
      string Explanation
    }
    QUESTION_OPTIONS {
      int Id PK
      int QuestionId FK
      string Text
      bool IsCorrect
    }
    TESTS {
      int Id PK
      string UserId FK
      int QuestionSetId FK
      datetime StartedAt
      datetime CompletedAt
      decimal Score
    }
    TEST_ANSWERS {
      int Id PK
      int TestId FK
      int QuestionId FK
      string SubmittedAnswer
      bool IsCorrect
      decimal AwardedMark
    }
    FLASHCARD_SETS {
      int Id PK
      string OwnerId FK
      int SubjectId FK
      string Title
    }
    FLASHCARDS {
      int Id PK
      int FlashcardSetId FK
      string Front
      string Back
    }
    PROGRESS {
      int Id PK
      string UserId FK
      int SubjectId FK
      decimal Mastery
      datetime UpdatedAt
    }
    NOTIFICATIONS {
      int Id PK
      string UserId FK
      string Message
      datetime CreatedAt
    }
    AUDIT_LOGS {
      int Id PK
      string UserId FK
      string Action
      datetime CreatedAt
    }
```

## Notes
- `USERS` is intended to be backed by ASP.NET Core Identity; its key type and profile fields will be finalized with the Day 4 authentication design.
- Question options are used for multiple-choice questions; other question types can have no options.
- Ownership and authorization constraints must be enforced in application queries and API policies, not only by hiding UI elements.
- The schema may be refined when concrete domain entities and migrations are implemented.
