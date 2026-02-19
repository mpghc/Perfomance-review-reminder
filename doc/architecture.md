# Architecture

## Solution Structure

Two projects — one web app, one test project. No extra class libraries; everything lives in the web project organized by folders.

```
PerformanceReviewReminderBot.slnx
├── src/
│   └── PerformanceReviewReminderBot.Web/              ← Blazor Server + Minimal API + EF Core
│       ├── Components/
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor        ← default layout (nav + header + role switcher)
│       │   │   └── AdminLayout.razor       ← admin layout (sidebar with CRUD nav)
│       │   ├── Pages/
│       │   │   ├── Home.razor              ← /
│       │   │   ├── Dashboard.razor         ← /dashboard  (TM: reviews overview)
│       │   │   ├── Admin/
│       │   │   │   ├── Employees.razor     ← /admin/employees
│       │   │   │   ├── EmployeeForm.razor  ← /admin/employees/new, /admin/employees/{id}
│       │   │   │   └── Teammates.razor     ← /admin/teammates
│       │   │   ├── Reviews/
│       │   │   │   ├── ReviewList.razor    ← /reviews
│       │   │   │   └── ScheduleReview.razor← /reviews/schedule/{employeeId}
│       │   │   ├── Feedback/
│       │   │   │   ├── PendingFeedback.razor ← /feedback
│       │   │   │   └── SubmitFeedback.razor  ← /feedback/{reviewId}
│       │   │   └── Notifications/
│       │   │       └── Inbox.razor         ← /notifications
│       │   └── Shared/
│       │       ├── RoleSwitcher.razor       ← dropdown: switch current user
│       │       └── NotificationBadge.razor  ← unread count in header
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── SeedData.cs
│       ├── Entities/
│       │   ├── Employee.cs
│       │   ├── EmployeeTeammate.cs
│       │   ├── PerformanceReview.cs
│       │   ├── Feedback.cs
│       │   ├── Notification.cs
│       │   └── Enums.cs  (EmployeeRole, ReviewStatus, NotificationType)
│       ├── Services/
│       │   ├── EmployeeService.cs
│       │   ├── ReviewService.cs
│       │   ├── FeedbackService.cs
│       │   ├── NotificationService.cs
│       │   ├── CurrentUserService.cs       ← tracks who is "logged in" via role switcher
│       │   └── ReminderBackgroundService.cs← IHostedService, daily reminder engine
│       ├── Endpoints/
│       │   ├── EmployeeEndpoints.cs
│       │   ├── ReviewEndpoints.cs
│       │   ├── FeedbackEndpoints.cs
│       │   └── NotificationEndpoints.cs
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── PerformanceReviewReminderBot.Web.Tests/
        ├── Services/
        │   ├── EmployeeServiceTests.cs
        │   ├── ReviewServiceTests.cs
        │   ├── FeedbackServiceTests.cs
        │   ├── NotificationServiceTests.cs
        │   └── ReminderBackgroundServiceTests.cs
        └── Endpoints/
            ├── EmployeeEndpointsTests.cs
            └── ReviewEndpointsTests.cs
```

**Why this shape:**

- Single web project = simple deployment (NFR-1).
- Services encapsulate business logic; pages call services; endpoints expose them as API.
- No repository pattern — services use `AppDbContext` directly (EF Core *is* the abstraction).
- Background service is a plain `IHostedService` — no external scheduler.

---

## Pages & Routing

| Route | Page | Layout | Accessible By | FR |
|---|---|---|---|---|
| `/` | Home | MainLayout | All | — |
| `/dashboard` | Dashboard | MainLayout | Talent Manager | FR-6 |
| `/admin/employees` | Employee List | AdminLayout | Talent Manager | FR-1 |
| `/admin/employees/new` | Employee Create | AdminLayout | Talent Manager | FR-1 |
| `/admin/employees/{id:int}` | Employee Edit | AdminLayout | Talent Manager | FR-1 |
| `/admin/teammates` | Teammate Management | AdminLayout | Talent Manager | FR-2 |
| `/reviews` | Review List | MainLayout | Talent Manager | FR-3 |
| `/reviews/schedule/{employeeId:int}` | Schedule Review | MainLayout | Talent Manager | FR-3 |
| `/feedback` | Pending Feedback | MainLayout | Employee | FR-5 |
| `/feedback/{reviewId:int}` | Submit Feedback | MainLayout | Employee | FR-5 |
| `/notifications` | Notification Inbox | MainLayout | All | FR-7 |

**Home page** (`/`) shows a role-aware landing: brief welcome + quick links to Dashboard (TM) or Pending Feedback (Employee).

---

## Layouts

### MainLayout

Used by most pages.

```
┌──────────────────────────────────────────┐
│  Header: App Title | [RoleSwitcher ▼] | 🔔 3 │
├────────────┬─────────────────────────────┤
│  Sidebar   │                             │
│            │        @Body               │
│  • Home    │                             │
│  • Dashboard│                            │
│  • Reviews │                             │
│  • Feedback│                             │
│  • Inbox   │                             │
│  • Admin ▸ │                             │
│            │                             │
└────────────┴─────────────────────────────┘
```

- Sidebar items are role-filtered (Employee doesn't see Dashboard, Admin, Reviews).
- `RoleSwitcher` dropdown in header — switches `CurrentUserService`.
- `NotificationBadge` shows unread count.

### AdminLayout

Used by `/admin/*` pages.

```
┌──────────────────────────────────────────┐
│  Header: Admin | [RoleSwitcher ▼] | 🔔 3    │
├────────────┬─────────────────────────────┤
│  Admin Nav │                             │
│            │        @Body               │
│  • Employees│                            │
│  • Teammates│                            │
│            │                             │
│  ← Back    │                             │
└────────────┴─────────────────────────────┘
```

- Simplified sidebar with only admin-related navigation.
- "Back to app" link returns to MainLayout pages.

---

## API Endpoints (Minimal API)

All endpoints under `/api`. Blazor pages use services directly; the API exists as a separate access layer (course requirement).

### Employees

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/employees` | List all (optional `?role=TalentManager`) |
| `GET` | `/api/employees/{id}` | Get by id |
| `POST` | `/api/employees` | Create |
| `PUT` | `/api/employees/{id}` | Update |
| `DELETE` | `/api/employees/{id}` | Delete |

### Teammates

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/employees/{id}/teammates` | List teammates |
| `POST` | `/api/employees/{id}/teammates/{teammateId}` | Add teammate (bidirectional) |
| `DELETE` | `/api/employees/{id}/teammates/{teammateId}` | Remove teammate (bidirectional) |

### Reviews

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/reviews?managerId={id}` | List reviews (filter by TM) |
| `GET` | `/api/reviews/{id}` | Get review with feedback status |
| `POST` | `/api/reviews` | Schedule new review |
| `PATCH` | `/api/reviews/{id}/status` | Update status |

### Feedback

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/reviews/{reviewId}/feedback` | List feedback for a review |
| `POST` | `/api/reviews/{reviewId}/feedback` | Submit feedback |

### Notifications

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/notifications?recipientId={id}` | List notifications |
| `PATCH` | `/api/notifications/{id}/read` | Mark as read |

---

## Testing Strategy

**Framework:** xUnit + NSubstitute.

| Layer | What to Test | How |
|---|---|---|
| **Services** | Business logic (CRUD validation, reminder logic, bidirectional teammate sync, feedback dedup) | Unit tests with in-memory SQLite `AppDbContext` |
| **Background Service** | Reminder engine creates correct notifications | Unit test with seeded data + time manipulation |
| **API Endpoints** | Request → response correctness, status codes | Integration tests via `WebApplicationFactory<Program>` |

### Key Test Scenarios

| # | Test | Service |
|---|---|---|
| 1 | Create/update/delete employee | EmployeeService |
| 2 | Add teammate creates bidirectional link | EmployeeService |
| 3 | Remove teammate removes both directions | EmployeeService |
| 4 | Schedule review sets status to Scheduled | ReviewService |
| 5 | Submit feedback marks author as done | FeedbackService |
| 6 | Duplicate feedback is rejected | FeedbackService |
| 7 | Reminder engine creates notifications for teammates without feedback | ReminderBackgroundService |
| 8 | Reminder engine skips teammates who already submitted | ReminderBackgroundService |
| 9 | Overdue notification sent to TM within 3-day window | ReminderBackgroundService |
| 10 | Mark notification as read | NotificationService |

**Not tested** (out of scope for course project): Blazor component rendering, CSS, navigation. Focus is on logic and API correctness.

---

## Design Decisions

| Decision | Choice | Rationale |
|---|---|---|
| No repository pattern | Services use `DbContext` directly | EF Core is already an abstraction; extra layer adds no value here |
| No DTOs for Blazor pages | Pages bind to entities | Simple project; avoids mapping boilerplate |
| DTOs for API responses | Separate request/response records | API should not leak EF navigation properties |
| In-memory SQLite for tests | SQLite in-memory provider | Fast, no file cleanup, real SQL behavior |
| Scoped `CurrentUserService` | Tracks "logged in" user per circuit | Simple role switching without auth infrastructure |
| Single background service | `IHostedService` with `PeriodicTimer` | No need for Hangfire/Quartz for one job |
