# Day 2 — Learner activity diagram

```mermaid
flowchart TD
    A([Start]) --> B{Authenticated?}
    B -- No --> C[Register or sign in]
    C --> D[Open student dashboard]
    B -- Yes --> D
    D --> E[Select or create subject]
    E --> F[Paste or upload study material]
    F --> G[Preview and save material]
    G --> H[Choose question mode, count and difficulty]
    H --> I[Request question generation]
    I --> J{Valid generated set?}
    J -- No --> K[Reject malformed output or use deterministic fallback]
    K --> L{Fallback available?}
    L -- No --> M[Show safe error and allow retry]
    M --> H
    L -- Yes --> N[Start practice session]
    J -- Yes --> N
    N --> O[Answer questions]
    O --> P[Submit attempt]
    P --> Q[Calculate score and breakdown]
    Q --> R[Show explanations and incorrect answers]
    R --> S{Review mistakes?}
    S -- Yes --> T[Retry missed questions]
    T --> Q
    S -- No --> U[Save result and update progress]
    U --> V([End])
```

## Key validation points
- Material must belong to the signed-in student.
- Question count and mode must be within supported limits.
- Generated questions must pass schema and answer validation before a test can use them.
- A failed AI request must not expose secrets or leave a partially created test.
