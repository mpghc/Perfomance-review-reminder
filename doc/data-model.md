# Data Model

## Entity Relationship Diagram

```
┌──────────────┐       ┌──────────────┐
│   Employee   │ N───1 │     Team     │
├──────────────┤       ├──────────────┤
│ Id           │       │ Id           │
│ FullName     │       │ Name         │
│ Email        │       │ ManagerId(FK)│──┐
│ Role (enum)  │       └──────────────┘  │
│ TeamId (FK)  │                         │
└──────┬───────┘         1───────────────┘
       │                 (Employee is manager)
       │
       │ 1
       ▼
┌──────────────────┐
│ PerformanceReview│
├──────────────────┤
│ Id               │
│ EmployeeId (FK)  │  ← who is being reviewed
│ ReviewDate       │
│ Status (enum)    │  ← Scheduled / InProgress / Completed
│ CreatedAt        │
└──────┬───────────┘
       │ 1
       │
       │ N
       ▼
┌──────────────────┐
│    Feedback      │
├──────────────────┤
│ Id               │
│ ReviewId (FK)    │  ← which review this is for
│ AuthorId (FK)    │  ← Employee who writes the feedback
│ Content          │  ← text feedback
│ SubmittedAt      │
└──────────────────┘

┌──────────────────┐
│  Notification    │
├──────────────────┤
│ Id               │
│ RecipientId (FK) │  ← Employee who receives it
│ ReviewId (FK)    │  ← related review (nullable)
│ Type (enum)      │  ← Reminder / Overdue
│ Message          │
│ IsRead           │
│ CreatedAt        │
└──────────────────┘
```

## Entities Detail

### Employee

| Column | Type | Notes |
|---|---|---|
| Id | int (PK) | Auto-increment |
| FullName | string | Required, max 200 |
| Email | string | Required, max 200 |
| Role | enum | `TalentManager`, `Employee` |
| TeamId | int? (FK) | Nullable — a Talent Manager may oversee a team without being a member |

### Team

| Column | Type | Notes |
|---|---|---|
| Id | int (PK) | Auto-increment |
| Name | string | Required, max 100 |
| ManagerId | int (FK) | Points to an Employee with Role = TalentManager |

### PerformanceReview

| Column | Type | Notes |
|---|---|---|
| Id | int (PK) | Auto-increment |
| EmployeeId | int (FK) | The employee being reviewed |
| ReviewDate | DateOnly | Scheduled date of the review |
| Status | enum | `Scheduled`, `InProgress`, `Completed` |
| CreatedAt | DateTime | Auto-set on creation |

### Feedback

| Column | Type | Notes |
|---|---|---|
| Id | int (PK) | Auto-increment |
| ReviewId | int (FK) | Which review this feedback belongs to |
| AuthorId | int (FK) | The teammate who submitted the feedback |
| Content | string | Required, feedback text |
| SubmittedAt | DateTime | Auto-set on submission |

**Constraint**: Unique on (ReviewId, AuthorId) — one feedback per person per review.

### Notification

| Column | Type | Notes |
|---|---|---|
| Id | int (PK) | Auto-increment |
| RecipientId | int (FK) | Who receives this notification |
| ReviewId | int? (FK) | Related review (nullable for system messages) |
| Type | enum | `Reminder`, `Overdue` |
| Message | string | Human-readable notification text |
| IsRead | bool | Default: false |
| CreatedAt | DateTime | Auto-set on creation |

## Seed Data (Demo)

The app ships with pre-loaded data so it's usable immediately:

| Entity | Seed Examples |
|---|---|
| Employees | Bill (Talent Manager), Tom, Bob, Carol, Alice (Employees) |
| Teams | "Engineering" — Talent Manager: Bill, Members: Tom, Bob, Carol, Alice |
| Reviews | Tom's review scheduled 14 days from now |
| Notifications | A few sample reminders already created |

This lets a reviewer open the app and immediately explore all pages without manual setup.
