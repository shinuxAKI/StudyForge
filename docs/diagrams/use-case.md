# Day 2 — Use-case diagram

The diagram captures the MVP actors and their principal interactions. Teacher/class functionality is marked optional and is not part of the initial MVP.

```mermaid
flowchart LR
    Guest([Guest])
    Student([Student])
    Admin([Administrator])
    Teacher([Teacher - optional])

    subgraph StudyForge[StudyForge]
      Browse[View landing page / demo]
      Auth[Register / sign in]
      Subject[Manage own subjects]
      Material[Add / edit study material]
      Generate[Generate question set]
      Configure[Choose mode, count, difficulty]
      Take[Take test / answer questions]
      Results[View score and explanations]
      Review[Review mistakes / retry]
      Cards[Review flashcards]
      Progress[View own progress and history]
      Share[Create / share class study sets]
      Manage[Manage users, reports and categories]
    end

    Guest --> Browse
    Guest --> Auth
    Student --> Auth
    Student --> Subject
    Student --> Material
    Student --> Configure
    Configure --> Generate
    Student --> Take
    Student --> Results
    Student --> Review
    Student --> Cards
    Student --> Progress
    Teacher -. optional .-> Share
    Admin --> Manage
```

## Access boundary
Students can access only their own private subjects, materials, tests, answers, and progress. Teacher sharing is an explicit later feature. Administrative functions require an administrator role.
