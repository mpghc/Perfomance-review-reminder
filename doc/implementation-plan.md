# Implementation Plan

Step-by-step iteration stages to build the Performance Review Reminder Bot from zero to a runnable application.

Each iteration produces a **working, committable state** — the app compiles and runs after every stage.

---

## Iteration 1 — Project Scaffolding

**Goal:** Empty Blazor Server app that runs, solution file, test project, basic folder structure.

**Steps:**

1. Create solution file `SkillMiner.slnx` at repo root.
2. Scaffold `src/SkillMiner.Web` using `dotnet new blazor` (Blazor Server, .NET 9).
3. Scaffold `tests/SkillMiner.Web.Tests` using `dotnet new xunit`.
4. Add project references: test project references web project.
5. Add NuGet packages:
   - Web: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`
   - Tests: `NSubstitute`, `Microsoft.EntityFrameworkCore.Sqlite` (in-memory), `Microsoft.AspNetCore.Mvc.Testing`
6. Create empty folder structure inside `SkillMiner.Web`:
   - `Entities/`, `Data/`, `Services/`, `Endpoints/`
   - `Components/Pages/Admin/`, `Components/Pages/Reviews/`, `Components/Pages/Feedback/`, `Components/Pages/Notifications/`
   - `Components/Shared/`
7. Enable nullable reference types in both `.csproj` files.
8. Verify: `dotnet build` succeeds, `dotnet run` shows default Blazor page.

**Deliverable:** Buildable solution with correct folder layout.

---

## Iteration 2 — Entities & Database

**Goal:** EF Core context, all 5 entities, enums, initial migration, SQLite database auto-created on startup.

**Steps:**

1. Create enums in `Entities/Enums.cs`: `EmployeeRole`, `ReviewStatus`, `NotificationType`.
2. Create entity classes:
   - `Entities/Employee.cs` — with self-referencing FK (`TalentManagerId`), navigation properties for managed employees and teammates.
   - `Entities/EmployeeTeammate.cs` — composite PK join table.
   - `Entities/PerformanceReview.cs` — FK to Employee, `DateOnly` for ReviewDate.
   - `Entities/Feedback.cs` — FK to Review and Author, unique constraint on (ReviewId, AuthorId).
   - `Entities/Notification.cs` — FK to Recipient and optional Review.
3. Create `Data/AppDbContext.cs`:
   - Configure all relationships, composite keys, indexes, and constraints in `OnModelCreating`.
   - Configure SQLite-specific settings.
4. Register `AppDbContext` in `Program.cs` with SQLite connection string from `appsettings.json`.
5. Add connection string to `appsettings.json`: `"ConnectionStrings": { "Default": "Data Source=skillminer.db" }`.
6. Create initial EF Core migration: `dotnet ef migrations add InitialCreate`.
7. Apply migration on startup in `Program.cs` using `Database.Migrate()`.
8. Verify: app starts, `skillminer.db` file is created, tables exist.

**Deliverable:** Running app with SQLite database and all tables.

---

## Iteration 3 — Seed Data

**Goal:** Demo data loaded on first run so every feature is immediately testable.

**Steps:**

1. Create `Data/SeedData.cs` with a static method `Initialize(AppDbContext context)`.
2. Seed employees:
   - Bill (Talent Manager)
   - Tom, Alice, Bob, Carol (Employees, all managed by Bill)
3. Seed teammate relationships (bidirectional):
   - Tom ↔ Alice, Tom ↔ Bob, Tom ↔ Carol, Alice ↔ Bob, Alice ↔ Carol, Bob ↔ Carol
4. Seed one performance review: Tom's review, scheduled 14 days from now, status = Scheduled.
5. Seed sample notifications: a couple of Reminder notifications for Alice and Bob regarding Tom's review.
6. Call `SeedData.Initialize` from `Program.cs` after migration (only if database is empty).
7. Verify: run app, query database — all seed data present.

**Deliverable:** App starts with realistic demo data.

---

## Iteration 4 — CurrentUserService & Role Switcher

**Goal:** Mechanism to switch "logged-in" user without authentication. All subsequent pages depend on this.

**Steps:**

1. Create `Services/CurrentUserService.cs` (scoped):
   - Properties: `CurrentUserId`, `CurrentUserName`, `CurrentUserRole`.
   - `SetCurrentUser(int employeeId)` — loads employee from DB and caches.
   - Defaults to Bill (Talent Manager) on first load.
2. Register as scoped service.
3. Create `Components/Shared/RoleSwitcher.razor`:
   - Dropdown listing all employees from DB.
   - On selection change, calls `CurrentUserService.SetCurrentUser`.
   - Displays current user name and role badge.
4. Create `Components/Shared/NotificationBadge.razor` (placeholder — shows hardcoded "0" for now).
5. Verify: dropdown appears in layout, switching user updates displayed name/role.

**Deliverable:** Working role switcher, shared across all pages.

---

## Iteration 5 — Layouts & Navigation Shell

**Goal:** Two layouts (MainLayout, AdminLayout) with role-aware navigation. All pages use correct layout.

**Steps:**

1. Implement `Components/Layout/MainLayout.razor`:
   - Header with app title, RoleSwitcher, NotificationBadge.
   - Sidebar with navigation links: Home, Dashboard, Reviews, Feedback, Inbox, Admin.
   - Role-filtering: hide TM-only links when current user is Employee, hide Employee-only links for TM.
2. Implement `Components/Layout/AdminLayout.razor`:
   - Header with "Admin" title, RoleSwitcher, NotificationBadge.
   - Sidebar with: Employees, Teammates, "← Back to app" link.
3. Create placeholder pages (empty, just a heading + `@page` directive):
   - `Pages/Home.razor` (`/`) — uses MainLayout
   - `Pages/Dashboard.razor` (`/dashboard`) — uses MainLayout
   - `Pages/Admin/Employees.razor` (`/admin/employees`) — uses AdminLayout
   - `Pages/Admin/EmployeeForm.razor` (`/admin/employees/new`, `/admin/employees/{id:int}`) — uses AdminLayout
   - `Pages/Admin/Teammates.razor` (`/admin/teammates`) — uses AdminLayout
   - `Pages/Reviews/ReviewList.razor` (`/reviews`) — uses MainLayout
   - `Pages/Reviews/ScheduleReview.razor` (`/reviews/schedule/{employeeId:int}`) — uses MainLayout
   - `Pages/Feedback/PendingFeedback.razor` (`/feedback`) — uses MainLayout
   - `Pages/Feedback/SubmitFeedback.razor` (`/feedback/{reviewId:int}`) — uses MainLayout
   - `Pages/Notifications/Inbox.razor` (`/notifications`) — uses MainLayout
4. Verify: all routes resolve, correct layout renders, nav links work, role switching hides/shows links.

**Deliverable:** Full navigation shell — every page reachable, correct layout applied.

---

## Iteration 6 — Employee Service & Admin CRUD Pages

**Goal:** Full CRUD for employees on Admin pages (FR-1). First real business logic.

**Steps:**

1. Create `Services/EmployeeService.cs`:
   - `GetAllAsync()`, `GetByIdAsync(int id)`, `CreateAsync(Employee)`, `UpdateAsync(Employee)`, `DeleteAsync(int id)`.
   - Validation: name/email required, can't delete an employee who has reviews or is a TM with assigned employees.
2. Register as scoped service.
3. Implement `Pages/Admin/Employees.razor`:
   - Table listing all employees (Name, Email, Role, Talent Manager).
   - "Add Employee" button → navigates to `/admin/employees/new`.
   - Edit/Delete buttons per row.
   - Delete shows confirmation.
4. Implement `Pages/Admin/EmployeeForm.razor`:
   - Form with fields: FullName, Email, Role (dropdown), TalentManagerId (dropdown, shown only for Employee role).
   - DataAnnotations validation.
   - On save → redirect to `/admin/employees`.
   - Handles both create (`/admin/employees/new`) and edit (`/admin/employees/{id:int}`) routes.
5. Write unit tests in `Services/EmployeeServiceTests.cs`:
   - Create employee succeeds.
   - Update employee succeeds.
   - Delete employee succeeds.
   - Delete employee with reviews fails.
6. Verify: full CRUD flow works via UI; tests pass.

**Deliverable:** Working employee management on Admin page.

---

## Iteration 7 — Teammate Service & Admin Page

**Goal:** Manage bidirectional teammate relationships (FR-2).

**Steps:**

1. Add methods to `Services/EmployeeService.cs` (or create a dedicated section):
   - `GetTeammatesAsync(int employeeId)`.
   - `AddTeammateAsync(int employeeId, int teammateId)` — inserts both directions.
   - `RemoveTeammateAsync(int employeeId, int teammateId)` — removes both directions.
   - Validation: can't add self, can't duplicate, both must exist.
2. Implement `Pages/Admin/Teammates.razor`:
   - Select an employee from a dropdown.
   - Show their current teammates as a list.
   - "Add Teammate" — dropdown of eligible employees (not self, not already teammate) + Add button.
   - Remove button per teammate.
3. Write unit tests:
   - Add teammate creates two rows.
   - Remove teammate removes two rows.
   - Adding self is rejected.
   - Adding duplicate is rejected.
4. Verify: teammate management works via UI; relationships are bidirectional in DB.

**Deliverable:** Working teammate management on Admin page.

---

## Iteration 8 — Review Service & Pages

**Goal:** Schedule and view performance reviews (FR-3, FR-6 partially).

**Steps:**

1. Create `Services/ReviewService.cs`:
   - `GetByManagerAsync(int managerId)` — reviews for all employees under a TM.
   - `GetByIdAsync(int reviewId)` — includes feedback status (who submitted, who hasn't).
   - `ScheduleAsync(int employeeId, DateOnly reviewDate)` — creates review with status Scheduled.
   - `UpdateStatusAsync(int reviewId, ReviewStatus status)`.
   - Validation: review date must be in the future, employee must exist.
2. Register as scoped service.
3. Implement `Pages/Reviews/ReviewList.razor`:
   - Table of reviews for current TM's employees.
   - Columns: Employee Name, Review Date, Status, Feedback progress (e.g., "2/4 submitted").
   - "Schedule Review" button → navigates to `/reviews/schedule/{employeeId}`.
4. Implement `Pages/Reviews/ScheduleReview.razor`:
   - Select employee (from TM's pool), pick date.
   - On save → redirect to `/reviews`.
5. Write unit tests in `Services/ReviewServiceTests.cs`:
   - Schedule review succeeds with valid data.
   - Schedule review rejects past date.
   - GetByManager returns only that TM's employees' reviews.
   - UpdateStatus changes status correctly.
6. Verify: can schedule reviews and see them listed.

**Deliverable:** Working review scheduling and listing.

---

## Iteration 9 — Feedback Service & Pages

**Goal:** Employees submit peer feedback (FR-5).

**Steps:**

1. Create `Services/FeedbackService.cs`:
   - `GetPendingForUserAsync(int employeeId)` — reviews where this employee is a teammate of the reviewee and hasn't submitted feedback yet.
   - `GetByReviewAsync(int reviewId)` — all feedback for a review.
   - `SubmitAsync(int reviewId, int authorId, string content)`.
   - Validation: can't submit twice (unique ReviewId + AuthorId), content required, author must be a teammate.
2. Register as scoped service.
3. Implement `Pages/Feedback/PendingFeedback.razor`:
   - List of reviews awaiting feedback from current user.
   - Shows: Employee Name, Review Date, "Submit Feedback" link.
4. Implement `Pages/Feedback/SubmitFeedback.razor`:
   - Shows review details (who is being reviewed, date).
   - Textarea for feedback content.
   - Submit button → redirect to `/feedback` with success message.
5. Write unit tests in `Services/FeedbackServiceTests.cs`:
   - Submit feedback succeeds.
   - Duplicate feedback is rejected.
   - GetPending excludes already-submitted reviews.
   - Non-teammate is rejected.
6. Verify: employee can see pending feedback list and submit.

**Deliverable:** Working feedback submission flow.

---

## Iteration 10 — Notification Service & Inbox Page

**Goal:** Notification inbox with read/unread (FR-7).

**Steps:**

1. Create `Services/NotificationService.cs`:
   - `GetByRecipientAsync(int recipientId)` — ordered by date descending.
   - `GetUnreadCountAsync(int recipientId)`.
   - `MarkAsReadAsync(int notificationId)`.
   - `CreateAsync(int recipientId, int? reviewId, NotificationType type, string message)`.
2. Register as scoped service.
3. Implement `Pages/Notifications/Inbox.razor`:
   - List of notifications for current user.
   - Unread items visually distinct (bold or highlighted).
   - "Mark as read" button per notification.
   - Shows: message, type badge, date.
4. Update `Components/Shared/NotificationBadge.razor`:
   - Query `NotificationService.GetUnreadCountAsync` for current user.
   - Display count; link to `/notifications`.
5. Write unit tests in `Services/NotificationServiceTests.cs`:
   - Create notification.
   - Mark as read.
   - Unread count is accurate.
6. Verify: notifications visible in inbox, badge updates, mark-as-read works.

**Deliverable:** Working notification inbox with live badge.

---

## Iteration 11 — Reminder Background Service

**Goal:** Automated reminder engine that creates notifications (FR-4).

**Steps:**

1. Create `Services/ReminderBackgroundService.cs` implementing `IHostedService`:
   - Uses `PeriodicTimer` (configurable interval, default = runs once on startup for demo, daily in production).
   - On each tick, calls a `ProcessRemindersAsync` method.
   - Logic:
     a. Find all reviews with status Scheduled or InProgress where ReviewDate is within 14 days.
     b. For each review, find teammates of the reviewee who haven't submitted feedback.
     c. For each such teammate, create a Reminder notification (if one hasn't already been created today).
     d. If ReviewDate is within 3 days and feedback is still missing, create an Overdue notification for the Talent Manager.
   - Uses a scoped `IServiceProvider` to create `AppDbContext` (background service is singleton).
2. Register in `Program.cs` via `builder.Services.AddHostedService<>()`.
3. Extract `ProcessRemindersAsync` logic into a testable service method (e.g., `ReminderService.ProcessAsync(DateTime now)`).
4. Write unit tests in `Services/ReminderBackgroundServiceTests.cs`:
   - Creates reminders for teammates without feedback.
   - Skips teammates who already submitted.
   - Creates overdue notification for TM within 3-day window.
   - Doesn't duplicate notifications created on the same day.
5. Verify: run app, check that notifications appear in inbox for seeded data.

**Deliverable:** Automated reminder engine with full test coverage.

---

## Iteration 12 — Dashboard Page

**Goal:** Talent Manager overview with feedback status and overdue highlighting (FR-6).

**Steps:**

1. Implement `Pages/Dashboard.razor`:
   - Shows all upcoming reviews for the current TM's employees.
   - Per review: Employee Name, Review Date, Status, Feedback progress bar/count.
   - Per review: expandable list of teammates showing submitted ✓ / pending ✗.
   - Items within 3 days of review date with missing feedback are highlighted as **overdue**.
   - Link to review details.
2. Implement `Pages/Home.razor`:
   - Role-aware landing page.
   - Talent Manager: welcome + quick stats (upcoming reviews count, overdue count) + link to Dashboard.
   - Employee: welcome + pending feedback count + link to Pending Feedback.
3. Verify: dashboard shows correct feedback status; Home page adapts to role.

**Deliverable:** Complete TM dashboard and role-aware home page.

---

## Iteration 13 — Minimal API Endpoints

**Goal:** REST API layer exposing all functionality (course requirement).

**Steps:**

1. Create `Endpoints/EmployeeEndpoints.cs`:
   - `GET /api/employees`, `GET /api/employees/{id}`, `POST /api/employees`, `PUT /api/employees/{id}`, `DELETE /api/employees/{id}`.
   - `GET /api/employees/{id}/teammates`, `POST /api/employees/{id}/teammates/{teammateId}`, `DELETE /api/employees/{id}/teammates/{teammateId}`.
2. Create `Endpoints/ReviewEndpoints.cs`:
   - `GET /api/reviews`, `GET /api/reviews/{id}`, `POST /api/reviews`, `PATCH /api/reviews/{id}/status`.
3. Create `Endpoints/FeedbackEndpoints.cs`:
   - `GET /api/reviews/{reviewId}/feedback`, `POST /api/reviews/{reviewId}/feedback`.
4. Create `Endpoints/NotificationEndpoints.cs`:
   - `GET /api/notifications`, `PATCH /api/notifications/{id}/read`.
5. Create request/response DTO records for API (no EF navigation properties leaked).
6. Map all endpoint groups in `Program.cs`.
7. Write integration tests using `WebApplicationFactory<Program>`:
   - `Endpoints/EmployeeEndpointsTests.cs` — CRUD happy path + error cases.
   - `Endpoints/ReviewEndpointsTests.cs` — schedule + status update.
8. Verify: all 16 endpoints respond correctly.

**Deliverable:** Full REST API with integration tests.

---

## Iteration 14 — Polish & Final Verification

**Goal:** Tie up loose ends, final styling, full test run.

**Steps:**

1. Review all pages for consistent styling (basic CSS — nothing fancy, but clean).
2. Add `ErrorBoundary` in layouts for graceful error handling.
3. Add basic form validation messages on all forms (DataAnnotations).
4. Ensure role-based visibility: Employee can't navigate to TM pages (redirect or "Access Denied" message).
5. Run full test suite: `dotnet test` — all tests pass.
6. Run the app end-to-end: walk through every user story from requirements.
7. Verify seed data flow: fresh `dotnet run` → all pages work with seed data, no manual setup.
8. Update `README.md` Quick Start section with actual run instructions.

**Deliverable:** Production-ready, runnable application meeting all requirements.

---

## Summary

| Iteration | Focus | Key Artifacts |
|---|---|---|
| 1 | Project scaffolding | Solution, projects, folder structure |
| 2 | Entities & database | 5 entities, AppDbContext, migration, SQLite |
| 3 | Seed data | SeedData.cs, demo-ready on first run |
| 4 | Role switcher | CurrentUserService, RoleSwitcher component |
| 5 | Layouts & navigation | MainLayout, AdminLayout, placeholder pages |
| 6 | Employee CRUD | EmployeeService, Admin employees pages, tests |
| 7 | Teammate management | Bidirectional teammate logic, Admin page, tests |
| 8 | Reviews | ReviewService, review pages, tests |
| 9 | Feedback | FeedbackService, feedback pages, tests |
| 10 | Notifications | NotificationService, inbox page, badge, tests |
| 11 | Reminder engine | ReminderBackgroundService, automated notifications, tests |
| 12 | Dashboard & Home | TM dashboard, role-aware home page |
| 13 | API endpoints | 16 Minimal API endpoints, DTOs, integration tests |
| 14 | Polish | Styling, validation, error handling, final verification |
