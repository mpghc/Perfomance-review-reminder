# Implementation Plan

Step-by-step iteration stages to build the Performance Review Reminder Bot from zero to a runnable application.

Each iteration produces a **working, committable state** — the app compiles and runs after every stage. Every iteration ends with a verification checklist to confirm correctness before moving on.

---

## Execution Safety Principles

1. **One architectural concept per iteration.** Never introduce a new data layer, a new UI pattern, and a new service pattern in the same iteration.
2. **Compile-first, run-second.** When adding entities or DbContext changes, first verify `dotnet build`. Only then add migrations and verify at runtime.
3. **Test before moving on.** Every service iteration includes unit tests. Run `dotnet test` before committing.
4. **Incremental migrations.** Each schema change gets its own migration. Never pile multiple unrelated schema changes into one migration.
5. **Seed data is idempotent.** `SeedData.Initialize` must be safe to call on a non-empty database (guard with `if (!context.Employees.Any())`).

---

## EF Core Troubleshooting Quick Reference

Use this section when you hit errors during entity/migration iterations.

| Symptom | Likely Cause | Fix |
|---|---|---|
| `Unable to determine the relationship represented by navigation` | Missing `HasOne`/`HasMany` config or ambiguous FK | Add explicit `.HasForeignKey()` in `OnModelCreating` |
| `The entity type requires a primary key to be defined` | Join table missing composite PK config | Add `builder.HasKey(e => new { e.EmployeeId, e.TeammateId })` |
| `Cannot use table 'X' for entity type 'Y' since it is being used for entity type 'Z'` | Two entities mapped to the same table name | Rename one with `.ToTable("ExplicitName")` |
| `FOREIGN KEY constraint failed` (SQLite) | Inserting a row that references a non-existent parent | Ensure parent rows are inserted first; use `SaveChanges()` between dependent inserts |
| `The seed entity for entity type 'X' cannot be added because a non-zero value is required for property 'Id'` | Using `HasData()` without explicit `Id` values | Always assign explicit PK values in `HasData` seed; prefer code-based seeding instead |
| Migration generates unexpected `DropTable` / `DropColumn` | Previous migration was manually deleted or model diverged from snapshot | Delete `Migrations/` folder, recreate from scratch with `dotnet ef migrations add InitialCreate` |
| `No suitable constructor found for entity type` | Entity has constructor parameters EF can't resolve | Add a parameterless constructor (can be `private`) |
| `The MERGE clause is not supported by SQLite` | Using `ExecuteUpdate`/`ExecuteDelete` on older SQLite provider | Use traditional `Remove()` + `SaveChanges()` pattern instead |

---

## Iteration 1 — Project Scaffolding

**Goal:** Empty Blazor Server app that runs, solution file, test project, basic folder structure.

**Steps:**

1. Create solution file `PerformanceReviewReminderBot.slnx` at repo root.
2. Scaffold `src/PerformanceReviewReminderBot.Web` using `dotnet new blazor` (Blazor Server, .NET 9).
3. Scaffold `tests/PerformanceReviewReminderBot.Web.Tests` using `dotnet new xunit`.
4. Add project references: test project references web project.
5. Add NuGet packages:
   - Web: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`
   - Tests: `NSubstitute`, `Microsoft.EntityFrameworkCore.Sqlite` (in-memory), `Microsoft.AspNetCore.Mvc.Testing`
6. Create empty folder structure inside `PerformanceReviewReminderBot.Web`:
   - `Entities/`, `Data/`, `Services/`, `Endpoints/`
   - `Components/Pages/Admin/`, `Components/Pages/Reviews/`, `Components/Pages/Feedback/`, `Components/Pages/Notifications/`
   - `Components/Shared/`
7. Enable nullable reference types in both `.csproj` files.

**Verification checklist:**

- [ ] `dotnet build` succeeds with zero warnings.
- [ ] `dotnet run --project src/PerformanceReviewReminderBot.Web` shows default Blazor page in browser.
- [ ] `dotnet test` runs (passes trivially — no tests yet).
- [ ] All folders exist in the project tree.

**Deliverable:** Buildable solution with correct folder layout.

---

## Iteration 2 — Entities & DbContext (Compile-Only)

**Goal:** All entity classes, enums, and `AppDbContext` with full relationship configuration — verified at compile time only. No database, no migration, no runtime changes.

> **Why split from database initialization:** This lets you focus on getting the C# model and `OnModelCreating` configuration correct without also debugging SQLite connection strings or migration tooling. One class of errors at a time.

**Steps:**

1. Create enums in `Entities/Enums.cs`: `EmployeeRole`, `ReviewStatus`, `NotificationType`.
2. Create entity classes in order of dependency:
   - `Entities/Employee.cs` — with self-referencing FK (`TalentManagerId`), navigation properties for managed employees.
   - `Entities/EmployeeTeammate.cs` — composite PK join table. Navigation properties to `Employee` on both sides.
   - `Entities/PerformanceReview.cs` — FK to Employee, `DateOnly` for ReviewDate.
   - `Entities/Feedback.cs` — FK to Review and Author (Employee).
   - `Entities/Notification.cs` — FK to Recipient (Employee) and optional Review.
3. Create `Data/AppDbContext.cs`:
   - Declare `DbSet<T>` for all five entities.
   - Configure in `OnModelCreating`:
     - `Employee`: self-referencing `TalentManagerId` FK with `OnDelete(DeleteBehavior.Restrict)`. Index on `Email`.
     - `EmployeeTeammate`: composite PK on `(EmployeeId, TeammateId)`. Two FKs to `Employee` with `OnDelete(DeleteBehavior.Cascade)`.
     - `PerformanceReview`: FK to `Employee`, index on `(EmployeeId, ReviewDate)`.
     - `Feedback`: FK to `PerformanceReview` and FK to `Employee` (Author). Unique index on `(ReviewId, AuthorId)`.
     - `Notification`: FK to `Employee` (Recipient), optional FK to `PerformanceReview`. Index on `(RecipientId, IsRead)`.
4. **Do NOT** register `AppDbContext` in `Program.cs` yet. Do NOT add connection strings, migrations, or startup code.

**⚠️ Common pitfalls:**

- The `Employee` entity has **three** relationships to itself: TalentManager (1:N), and two sides of the teammate many-to-many. EF Core cannot auto-discover these — you **must** configure them explicitly in `OnModelCreating`.
- Use `DeleteBehavior.Restrict` on the TalentManager FK to prevent cascade-delete cycles in SQLite.
- For `EmployeeTeammate`, configure **two separate** `HasOne(e => e.Employee)` / `HasOne(e => e.Teammate)` relationships, each with their own FK.

**Verification checklist:**

- [ ] `dotnet build` succeeds with zero errors.
- [ ] All five entity classes are in `Entities/`.
- [ ] `AppDbContext` has five `DbSet<T>` properties.
- [ ] `OnModelCreating` explicitly configures all relationships, composite keys, and indexes.
- [ ] No changes to `Program.cs` or `appsettings.json`.

**Deliverable:** Complete domain model that compiles. No runtime behavior yet.

---

## Iteration 3 — Database Initialization & Migration

**Goal:** SQLite database is created on startup via EF Core migration. All tables exist and match the entity model.

> **Why separate from entities:** Migrations depend on a correct model. By confirming compilation first (Iteration 2), you know any errors in this iteration are purely infrastructure (connection string, migration tooling, startup order) — not model bugs.

**Steps:**

1. Add connection string to `appsettings.json`:
   ```json
   "ConnectionStrings": { "Default": "Data Source=performancereviewreminderbot.db" }
   ```
2. Register `AppDbContext` in `Program.cs` with the SQLite provider and the connection string.
3. Add startup migration code in `Program.cs`:
   ```csharp
   using var scope = app.Services.CreateScope();
   var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
   db.Database.Migrate();
   ```
4. Create the initial EF Core migration:
   ```bash
   dotnet ef migrations add InitialCreate --project src/PerformanceReviewReminderBot.Web
   ```
5. Inspect the generated migration file — verify it creates all expected tables with correct columns, FKs, and indexes.
6. Run the app and confirm the database file appears.

**⚠️ Common pitfalls:**

- If `dotnet ef migrations add` fails with relationship errors, go back to Iteration 2 and fix `OnModelCreating` first. Do not attempt to fix the migration file manually.
- SQLite does not enforce FK constraints by default. The EF Core SQLite provider handles this, but if you query the DB directly, run `PRAGMA foreign_keys = ON;` first.
- Ensure `Microsoft.EntityFrameworkCore.Design` is in the web project (not just the test project) — it's needed for migration tooling.

**Verification checklist:**

- [ ] `dotnet ef migrations add` generates a clean migration (no warnings about ambiguous relationships).
- [ ] `dotnet run` starts without errors.
- [ ] `performancereviewreminderbot.db` file is created in the project directory.
- [ ] Open the DB (e.g., with DB Browser for SQLite or `sqlite3` CLI) and verify: `Employees`, `EmployeeTeammates`, `PerformanceReviews`, `Feedbacks`, `Notifications` tables exist.
- [ ] Each table has the expected columns, PKs, and FK constraints.

**Deliverable:** Running app with SQLite database and all tables matching the data model.

---

## Iteration 4 — Seed Data

**Goal:** Demo data loaded on first run so every feature is immediately testable.

**Steps:**

1. Create `Data/SeedData.cs` with a static method `Initialize(AppDbContext context)`.
2. Guard: `if (context.Employees.Any()) return;` — idempotent.
3. Seed employees (assign explicit IDs for deterministic FK references):
   - Bill (Id=1, Talent Manager, TalentManagerId=null)
   - Tom (Id=2), Alice (Id=3), Bob (Id=4), Carol (Id=5) — Employees, all managed by Bill.
4. Call `context.SaveChanges()` after employees before seeding relationships (ensures FK targets exist).
5. Seed teammate relationships (bidirectional — insert **both** directions for each pair):
   - Tom ↔ Alice, Tom ↔ Bob, Tom ↔ Carol, Alice ↔ Bob, Alice ↔ Carol, Bob ↔ Carol.
   - That's 6 pairs = 12 `EmployeeTeammate` rows.
6. Seed one performance review: Tom's review, scheduled 14 days from now, status = Scheduled.
7. Seed sample notifications: a couple of Reminder notifications for Alice and Bob regarding Tom's review.
8. Call `context.SaveChanges()`.
9. Call `SeedData.Initialize` from `Program.cs` after `Database.Migrate()`.

**⚠️ Common pitfalls:**

- Insert employees **before** teammate rows — FK constraint on `EmployeeTeammate` requires both employees to exist.
- When using explicit IDs with SQLite, you may need to use `context.Database.ExecuteSqlRaw("UPDATE sqlite_sequence SET seq = 5 WHERE name = 'Employees'")` or simply let EF handle identity insert. Prefer assigning IDs via entity properties with `ValueGeneratedNever()` if using explicit IDs, or just set properties and let auto-increment work (then read back the IDs).
- Simpler approach: don't set IDs explicitly. Create employees, call `SaveChanges()`, then use the generated `employee.Id` values for FK references.

**Verification checklist:**

- [ ] App starts without errors on a fresh run (delete `performancereviewreminderbot.db` first).
- [ ] Query `Employees` table: 5 rows (1 TM + 4 Employees).
- [ ] Query `EmployeeTeammates` table: 12 rows (6 pairs × 2 directions).
- [ ] Query `PerformanceReviews` table: 1 row with correct EmployeeId and future date.
- [ ] Query `Notifications` table: ≥2 rows.
- [ ] Run app a second time (without deleting DB) — no duplicate data inserted.

**Deliverable:** App starts with realistic demo data, safe for repeated runs.

---

## Iteration 5 — CurrentUserService & Role Switcher

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

**Verification checklist:**

- [ ] App starts; dropdown appears in the default layout.
- [ ] Dropdown lists all 5 seeded employees.
- [ ] Selecting a different user updates displayed name and role badge.
- [ ] Refreshing the page resets to default user (Bill) — expected for scoped service.

**Deliverable:** Working role switcher, shared across all pages.

---

## Iteration 6 — Layouts & Navigation Shell

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
   - `Pages/Home.razor` (`/`) — uses MainLayout.
   - `Pages/Dashboard.razor` (`/dashboard`) — uses MainLayout.
   - `Pages/Admin/Employees.razor` (`/admin/employees`) — uses AdminLayout.
   - `Pages/Admin/EmployeeForm.razor` (`/admin/employees/new`, `/admin/employees/{id:int}`) — uses AdminLayout.
   - `Pages/Admin/Teammates.razor` (`/admin/teammates`) — uses AdminLayout.
   - `Pages/Reviews/ReviewList.razor` (`/reviews`) — uses MainLayout.
   - `Pages/Reviews/ScheduleReview.razor` (`/reviews/schedule/{employeeId:int}`) — uses MainLayout.
   - `Pages/Feedback/PendingFeedback.razor` (`/feedback`) — uses MainLayout.
   - `Pages/Feedback/SubmitFeedback.razor` (`/feedback/{reviewId:int}`) — uses MainLayout.
   - `Pages/Notifications/Inbox.razor` (`/notifications`) — uses MainLayout.

**Verification checklist:**

- [ ] All routes resolve without 404 in the browser.
- [ ] MainLayout renders for non-admin pages; AdminLayout renders for `/admin/*` pages.
- [ ] Nav links navigate correctly between pages.
- [ ] Switch to Employee role → TM-only links (Dashboard, Reviews, Admin) are hidden.
- [ ] Switch to TM role → Employee-only links (Feedback) are hidden.
- [ ] AdminLayout shows "← Back to app" link that returns to MainLayout pages.

**Deliverable:** Full navigation shell — every page reachable, correct layout applied.

---

## Iteration 7 — Employee Service & Admin CRUD Pages

**Goal:** Full CRUD for employees on Admin pages (FR-1). First real business logic.

**Steps:**

1. Create `Services/EmployeeService.cs`:
   - `GetAllAsync()` — returns all employees, includes TalentManager navigation property.
   - `GetByIdAsync(int id)` — single employee or null.
   - `CreateAsync(Employee)` — validates name/email required, returns created entity.
   - `UpdateAsync(Employee)` — validates name/email required, returns updated entity.
   - `DeleteAsync(int id)` — validates: can't delete employee with reviews, can't delete TM with assigned employees. Throws descriptive exception on violation.
2. Register as scoped service.
3. Implement `Pages/Admin/Employees.razor`:
   - Table listing all employees (Name, Email, Role, Talent Manager).
   - "Add Employee" button → navigates to `/admin/employees/new`.
   - Edit/Delete buttons per row.
   - Delete shows confirmation prompt.
4. Implement `Pages/Admin/EmployeeForm.razor`:
   - Form with fields: FullName, Email, Role (dropdown), TalentManagerId (dropdown, shown only for Employee role).
   - DataAnnotations validation.
   - On save → redirect to `/admin/employees`.
   - Handles both create (`/admin/employees/new`) and edit (`/admin/employees/{id:int}`) routes.
5. Write unit tests in `Services/EmployeeServiceTests.cs` (use in-memory SQLite):
   - Create employee succeeds with valid data.
   - Create employee fails with empty name.
   - Update employee succeeds.
   - Delete employee succeeds (no dependencies).
   - Delete employee with reviews throws.
   - Delete TM with assigned employees throws.

**Verification checklist:**

- [ ] `dotnet test` — all EmployeeService tests pass.
- [ ] UI: navigate to `/admin/employees` — see seeded employees in table.
- [ ] UI: click "Add Employee" → fill form → save → new employee appears in table.
- [ ] UI: click "Edit" on an employee → form pre-filled → save → changes reflected.
- [ ] UI: click "Delete" on an employee with no reviews → employee removed.
- [ ] UI: click "Delete" on Tom (has a review) → error message, not deleted.
- [ ] DB: verify row counts match expectations after CRUD operations.

**Deliverable:** Working employee management on Admin page with validated service layer.

---

## Iteration 8 — Teammate Service & Admin Page

**Goal:** Manage bidirectional teammate relationships (FR-2). Introduced **after** Employee CRUD is stable.

> **Why after Employee CRUD:** Teammate logic depends on employees existing and being correctly queryable. By completing Employee CRUD first, you have a tested service layer and a working UI to verify employee data — reducing the variables when debugging teammate relationship issues.

**Steps:**

1. Add teammate methods to `Services/EmployeeService.cs` (or create `Services/TeammateService.cs` if the file is getting large):
   - `GetTeammatesAsync(int employeeId)` — returns list of teammate employees.
   - `AddTeammateAsync(int employeeId, int teammateId)` — inserts **both** directions in a single `SaveChanges` call.
   - `RemoveTeammateAsync(int employeeId, int teammateId)` — removes **both** directions in a single `SaveChanges` call.
   - Validation (each with a descriptive error message):
     - Can't add self as teammate.
     - Can't add duplicate (check before insert).
     - Both employees must exist.
     - Both employees must have role `Employee` (TMs are not teammates).
2. Implement `Pages/Admin/Teammates.razor`:
   - Select an employee from a dropdown (filtered to role = Employee).
   - Show their current teammates as a list.
   - "Add Teammate" — dropdown of eligible employees (not self, not already teammate) + Add button.
   - Remove button per teammate.
3. Write unit tests:
   - `AddTeammateAsync` creates exactly 2 rows in `EmployeeTeammates`.
   - `RemoveTeammateAsync` removes exactly 2 rows.
   - Adding self is rejected with `InvalidOperationException`.
   - Adding duplicate is rejected.
   - Adding non-existent employee is rejected.
   - After adding A↔B, querying teammates of A includes B and vice versa.

**⚠️ Common pitfalls:**

- Always insert/remove **both** directions in the same `SaveChanges()` call to maintain consistency. If one insert succeeds and the other fails, you have a broken half-relationship.
- When querying teammates, use either direction: `WHERE EmployeeId = X` gives you X's teammates via `TeammateId`. Don't query both directions and deduplicate — the data model guarantees both rows exist.
- SQLite composite PK on `(EmployeeId, TeammateId)` means `(1,2)` and `(2,1)` are different rows — this is correct and expected.

**Verification checklist:**

- [ ] `dotnet test` — all teammate tests pass.
- [ ] UI: navigate to `/admin/teammates`, select Tom — see Alice, Bob, Carol as teammates.
- [ ] UI: remove Alice as Tom's teammate → Alice disappears from Tom's list.
- [ ] UI: select Alice → Tom is also no longer her teammate (bidirectional removal confirmed).
- [ ] UI: re-add Alice as Tom's teammate → both directions restored.
- [ ] DB: verify `EmployeeTeammates` table always has matching pairs.

**Deliverable:** Working teammate management with enforced bidirectional consistency.

---

## Iteration 9 — Employee & Teammate API Endpoints

**Goal:** REST API for employee CRUD and teammate management. First batch of Minimal API endpoints.

> **Why spread APIs:** Introducing all 16 endpoints in one iteration creates a large, hard-to-debug changeset. By adding endpoints alongside the services they expose, each batch is small, focused, and testable.

**Steps:**

1. Create request/response DTO records in `Endpoints/` (or a `Dtos/` subfolder):
   - `EmployeeRequest` (FullName, Email, Role, TalentManagerId).
   - `EmployeeResponse` (Id, FullName, Email, Role, TalentManagerName).
   - `TeammateResponse` (Id, FullName, Email).
2. Create `Endpoints/EmployeeEndpoints.cs`:
   - `GET /api/employees` — list all (optional `?role=` filter).
   - `GET /api/employees/{id}` — get by id.
   - `POST /api/employees` — create.
   - `PUT /api/employees/{id}` — update.
   - `DELETE /api/employees/{id}` — delete.
   - `GET /api/employees/{id}/teammates` — list teammates.
   - `POST /api/employees/{id}/teammates/{teammateId}` — add teammate.
   - `DELETE /api/employees/{id}/teammates/{teammateId}` — remove teammate.
3. Map endpoint group in `Program.cs`.
4. Write integration tests using `WebApplicationFactory<Program>`:
   - `Endpoints/EmployeeEndpointsTests.cs`:
     - GET returns seeded employees.
     - POST creates employee, returns 201.
     - PUT updates employee, returns 200.
     - DELETE removes employee, returns 204.
     - DELETE employee with reviews returns 400/409.
     - Teammate add/remove/list happy paths.

**Verification checklist:**

- [ ] `dotnet test` — all endpoint integration tests pass.
- [ ] Manual: `curl GET /api/employees` returns JSON array of employees.
- [ ] Manual: `curl POST /api/employees` with valid body → 201 + created employee.
- [ ] Manual: `curl DELETE /api/employees/{id}` for employee with reviews → error response with message.
- [ ] Manual: `curl GET /api/employees/{id}/teammates` returns teammate list.
- [ ] No EF navigation properties leaked in JSON responses (only DTOs).

**Deliverable:** 8 working employee/teammate API endpoints with integration tests.

---

## Iteration 10 — Review Service & Pages

**Goal:** Schedule and view performance reviews (FR-3, FR-6 partially).

**Steps:**

1. Create `Services/ReviewService.cs`:
   - `GetByManagerAsync(int managerId)` — reviews for all employees managed by this TM. Include Employee navigation property.
   - `GetByIdAsync(int reviewId)` — includes employee info and feedback status (who submitted, who hasn't).
   - `ScheduleAsync(int employeeId, DateOnly reviewDate)` — creates review with status Scheduled.
   - `UpdateStatusAsync(int reviewId, ReviewStatus status)`.
   - Validation:
     - Review date must be in the future.
     - Employee must exist.
     - Employee must have a TalentManager assigned.
2. Register as scoped service.
3. Implement `Pages/Reviews/ReviewList.razor`:
   - Table of reviews for current TM's employees.
   - Columns: Employee Name, Review Date, Status, Feedback progress (e.g., "2/4 submitted").
   - "Schedule Review" button → navigates to `/reviews/schedule/{employeeId}`.
4. Implement `Pages/Reviews/ScheduleReview.razor`:
   - Select employee (from TM's managed employees), pick date.
   - On save → redirect to `/reviews`.
5. Write unit tests in `Services/ReviewServiceTests.cs`:
   - Schedule review succeeds with valid future date.
   - Schedule review rejects past date.
   - `GetByManagerAsync` returns only that TM's employees' reviews.
   - `GetByManagerAsync` returns empty list for TM with no employees.
   - `UpdateStatusAsync` changes status correctly.

**Verification checklist:**

- [ ] `dotnet test` — all ReviewService tests pass.
- [ ] UI: navigate to `/reviews` as Bill (TM) — see Tom's seeded review.
- [ ] UI: schedule a new review for Alice → it appears in the list.
- [ ] UI: verify feedback progress shows correct count (e.g., "0/3 submitted" for a new review with 3 teammates).
- [ ] DB: `PerformanceReviews` table has correct rows with proper FKs.

**Deliverable:** Working review scheduling and listing.

---

## Iteration 11 — Feedback Service & Pages

**Goal:** Employees submit peer feedback (FR-5).

**Steps:**

1. Create `Services/FeedbackService.cs`:
   - `GetPendingForUserAsync(int employeeId)` — reviews where this employee is a teammate of the reviewee, the review is Scheduled or InProgress, and the employee hasn't submitted feedback yet.
   - `GetByReviewAsync(int reviewId)` — all feedback for a review with author info.
   - `SubmitAsync(int reviewId, int authorId, string content)`.
   - Validation:
     - Content is required (non-empty).
     - Author must be a teammate of the reviewee.
     - Can't submit twice (unique constraint on ReviewId + AuthorId).
     - Review must exist and not be Completed.
2. Register as scoped service.
3. Implement `Pages/Feedback/PendingFeedback.razor`:
   - List of reviews awaiting feedback from current user.
   - Shows: Employee Name, Review Date, "Submit Feedback" link.
   - Empty state message if no pending feedback.
4. Implement `Pages/Feedback/SubmitFeedback.razor`:
   - Shows review details (who is being reviewed, date).
   - Textarea for feedback content.
   - Submit button → redirect to `/feedback` with success message.
5. Write unit tests in `Services/FeedbackServiceTests.cs`:
   - Submit feedback succeeds for valid teammate.
   - Duplicate feedback is rejected (throws or returns error).
   - `GetPendingForUserAsync` excludes reviews where feedback already submitted.
   - `GetPendingForUserAsync` excludes Completed reviews.
   - Non-teammate is rejected.
   - Empty content is rejected.

**Verification checklist:**

- [ ] `dotnet test` — all FeedbackService tests pass.
- [ ] UI: switch to Alice → navigate to `/feedback` → see Tom's review as pending.
- [ ] UI: submit feedback for Tom's review → redirected to `/feedback`, Tom's review no longer listed.
- [ ] UI: navigate to `/feedback` again → Tom's review is gone (already submitted).
- [ ] UI: switch to Bob → Tom's review still listed (Bob hasn't submitted yet).
- [ ] DB: `Feedbacks` table has 1 row with correct ReviewId, AuthorId, Content.

**Deliverable:** Working feedback submission flow.

---

## Iteration 12 — Notification Service & Inbox Page

**Goal:** Notification inbox with read/unread (FR-7).

**Steps:**

1. Create `Services/NotificationService.cs`:
   - `GetByRecipientAsync(int recipientId)` — ordered by date descending.
   - `GetUnreadCountAsync(int recipientId)`.
   - `MarkAsReadAsync(int notificationId)`.
   - `CreateAsync(int recipientId, int? reviewId, NotificationType type, string message)`.
   - Validation: recipient must exist, message must be non-empty.
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
   - Create notification succeeds.
   - `MarkAsReadAsync` sets `IsRead = true`.
   - `GetUnreadCountAsync` returns correct count (decreases after marking read).
   - `GetByRecipientAsync` returns only that recipient's notifications, ordered by date.

**Verification checklist:**

- [ ] `dotnet test` — all NotificationService tests pass.
- [ ] UI: switch to Alice — see seeded Reminder notification in Inbox.
- [ ] UI: NotificationBadge shows correct unread count.
- [ ] UI: click "Mark as read" → notification styled as read, badge count decrements.
- [ ] UI: switch to Bill (TM) — see different notifications (or none if only employee reminders were seeded).
- [ ] DB: `Notifications` table `IsRead` column updates correctly.

**Deliverable:** Working notification inbox with live badge.

---

## Iteration 13 — Reminder Service (Pure Logic)

**Goal:** Implement the reminder/notification generation logic as a pure, testable service method — fully tested before introducing any background service infrastructure.

> **Why pure first:** The reminder logic involves complex queries (reviews in reminder window, teammates without feedback, deduplication). Testing this in isolation against an in-memory DB is fast and reliable. Introducing `IHostedService`, `PeriodicTimer`, and scoped service resolution at the same time would create multiple failure points.

**Steps:**

1. Create `Services/ReminderService.cs` (scoped):
   - Single public method: `ProcessAsync(DateTime now)`.
   - Logic:
     a. Find all reviews with status Scheduled or InProgress where `ReviewDate` is within 14 days of `now`.
     b. For each review, find teammates of the reviewee who haven't submitted feedback.
     c. For each such teammate, check if a Reminder notification for this review was already created today — if not, create one.
     d. If `ReviewDate` is within 3 days of `now` and feedback is still missing, check if an Overdue notification was already created today for the Talent Manager — if not, create one.
   - All notification creation goes through `NotificationService.CreateAsync` (reuse, don't duplicate).
2. Register as scoped service.
3. Transaction boundary: wrap the entire `ProcessAsync` in a single `SaveChanges()` call at the end (or use explicit transaction if multiple save points are needed for large datasets).
4. Write unit tests in `Services/ReminderServiceTests.cs` (use in-memory SQLite, seed specific scenarios):
   - **Scenario: 14-day window** — review 10 days away, teammate without feedback → Reminder notification created.
   - **Scenario: outside window** — review 20 days away → no notification created.
   - **Scenario: already submitted** — teammate has submitted feedback → no notification for them.
   - **Scenario: 3-day overdue** — review 2 days away, missing feedback → Overdue notification sent to TM.
   - **Scenario: deduplication** — run `ProcessAsync` twice on the same day → only one notification per recipient per review per day.
   - **Scenario: completed review** — review status is Completed → no notifications.

**Verification checklist:**

- [ ] `dotnet test` — all 6+ ReminderService tests pass.
- [ ] Each test seeds its own data scenario (no test depends on shared state).
- [ ] Deduplication test confirms: second run produces zero new notifications.
- [ ] No `IHostedService` or background infrastructure introduced yet.
- [ ] `ReminderService` depends only on `AppDbContext` and `NotificationService` — no timer, no `IServiceProvider` scope management.

**Deliverable:** Fully tested reminder logic with zero infrastructure coupling.

---

## Iteration 14 — Reminder Background Service (Thin Wrapper)

**Goal:** Wrap the tested `ReminderService` in an `IHostedService` that runs on a timer (FR-4).

> **Why separate from logic:** The background service is a thin shell that creates scopes and calls `ReminderService.ProcessAsync`. This iteration introduces only infrastructure concerns (singleton service, scoped service resolution, timer). The logic is already tested.

**Steps:**

1. Create `Services/ReminderBackgroundService.cs` implementing `IHostedService`:
   - Uses `PeriodicTimer` with a configurable interval (from `IConfiguration`).
   - Default: runs once immediately on startup (for demo), then every 24 hours.
   - On each tick:
     a. Create a new `IServiceScope`.
     b. Resolve `ReminderService` from the scope.
     c. Call `ReminderService.ProcessAsync(DateTime.UtcNow)`.
     d. Dispose the scope.
   - Catch and log exceptions (don't let the background service crash).
2. Register in `Program.cs` via `builder.Services.AddHostedService<ReminderBackgroundService>()`.
3. Add configuration to `appsettings.json`:
   ```json
   "Reminders": { "IntervalMinutes": 1440 }
   ```

**⚠️ Common pitfalls:**

- The background service is a **singleton**, but `AppDbContext` and `ReminderService` are **scoped**. You must create a scope on every tick — do NOT inject `ReminderService` via constructor.
- Always wrap the tick body in `try/catch`. An unhandled exception in a background service silently kills it.
- For demo purposes, trigger the first run immediately (don't wait for the first interval to elapse).

**Verification checklist:**

- [ ] App starts without errors.
- [ ] Within seconds of startup, check Alice's and Bob's notification inboxes — new Reminder notifications appear for Tom's review (seeded 14 days from now).
- [ ] If Tom's review date is within 3 days (adjust seed data or `now` to test), Bill receives an Overdue notification.
- [ ] Check application logs — the background service logs each processing run.
- [ ] Stop and restart the app — no duplicate notifications (deduplication from Iteration 13 logic).

**Deliverable:** Automated reminder engine running as a background service.

---

## Iteration 15 — Dashboard & Home Page

**Goal:** Talent Manager overview with feedback status and overdue highlighting (FR-6). Role-aware home page.

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

**Verification checklist:**

- [ ] UI: log in as Bill (TM) → Dashboard shows Tom's review with feedback progress.
- [ ] UI: expandable section shows which teammates submitted and which haven't.
- [ ] UI: if any review is within 3 days with missing feedback, it's highlighted as overdue.
- [ ] UI: Home page as Bill shows upcoming review count and link to Dashboard.
- [ ] UI: switch to Alice (Employee) → Home page shows pending feedback count and link to `/feedback`.
- [ ] UI: Dashboard is not accessible to Employee role (redirect or "Access Denied").

**Deliverable:** Complete TM dashboard and role-aware home page.

---

## Iteration 16 — Remaining API Endpoints

**Goal:** REST API for reviews, feedback, and notifications. Completes the 16-endpoint requirement.

**Steps:**

1. Create DTOs:
   - `ReviewRequest` (EmployeeId, ReviewDate), `ReviewResponse` (Id, EmployeeName, ReviewDate, Status, FeedbackProgress).
   - `StatusUpdateRequest` (Status).
   - `FeedbackRequest` (AuthorId, Content), `FeedbackResponse` (Id, AuthorName, Content, SubmittedAt).
   - `NotificationResponse` (Id, Message, Type, IsRead, CreatedAt).
2. Create `Endpoints/ReviewEndpoints.cs`:
   - `GET /api/reviews?managerId={id}` — list reviews.
   - `GET /api/reviews/{id}` — get review with feedback status.
   - `POST /api/reviews` — schedule new review.
   - `PATCH /api/reviews/{id}/status` — update status.
3. Create `Endpoints/FeedbackEndpoints.cs`:
   - `GET /api/reviews/{reviewId}/feedback` — list feedback for a review.
   - `POST /api/reviews/{reviewId}/feedback` — submit feedback.
4. Create `Endpoints/NotificationEndpoints.cs`:
   - `GET /api/notifications?recipientId={id}` — list notifications.
   - `PATCH /api/notifications/{id}/read` — mark as read.
5. Map all endpoint groups in `Program.cs`.
6. Write integration tests using `WebApplicationFactory<Program>`:
   - `Endpoints/ReviewEndpointsTests.cs` — schedule + list + status update.
   - Additional tests for feedback and notification endpoints.

**Verification checklist:**

- [ ] `dotnet test` — all new integration tests pass.
- [ ] Manual: `curl POST /api/reviews` with valid body → 201 + review JSON.
- [ ] Manual: `curl GET /api/reviews?managerId=1` → returns reviews for Bill's employees.
- [ ] Manual: `curl POST /api/reviews/{id}/feedback` → 201 + feedback JSON.
- [ ] Manual: `curl PATCH /api/notifications/{id}/read` → 200 + updated notification.
- [ ] All 16 endpoints return proper HTTP status codes and DTO responses.
- [ ] No EF navigation property cycles in JSON responses.

**Deliverable:** Complete REST API (16 endpoints) with integration tests.

---

## Iteration 17 — Polish & Final Verification

**Goal:** Tie up loose ends, final styling, full test run, end-to-end walkthrough.

**Steps:**

1. Review all pages for consistent styling (basic CSS — nothing fancy, but clean).
2. Add `ErrorBoundary` in layouts for graceful error handling.
3. Add basic form validation messages on all forms (DataAnnotations).
4. Ensure role-based visibility: Employee can't navigate to TM pages (redirect or "Access Denied" message).
5. Run full test suite: `dotnet test` — all tests pass.
6. Run the app end-to-end: walk through every user story from requirements:
   - US-01 through US-06 as Talent Manager.
   - US-07 through US-09 as Employee.
   - US-10: role switching works correctly.
7. Verify seed data flow: delete `performancereviewreminderbot.db`, run `dotnet run` → all pages work with seed data, no manual setup.
8. Update `README.md` Quick Start section with actual run instructions.

**Verification checklist:**

- [ ] `dotnet test` — **all** tests pass (unit + integration).
- [ ] Fresh run (delete DB) → app starts, seed data loaded, all pages functional.
- [ ] TM walkthrough: manage employees → manage teammates → schedule review → view dashboard → check inbox.
- [ ] Employee walkthrough: see pending feedback → submit feedback → check inbox → feedback no longer pending.
- [ ] Reminder engine: notifications appear automatically for upcoming reviews.
- [ ] Role switcher: switching users updates all pages correctly.
- [ ] API: hit all 16 endpoints via curl or browser — correct responses.
- [ ] No unhandled exceptions in console/logs during full walkthrough.

**Deliverable:** Production-ready, runnable application meeting all requirements.

---

## Summary

| Iteration | Focus | Key Artifacts |
|---|---|---|
| 1 | Project scaffolding | Solution, projects, folder structure |
| 2 | Entities & DbContext (compile-only) | 5 entities, enums, AppDbContext with OnModelCreating |
| 3 | Database initialization & migration | SQLite connection, initial migration, startup Migrate() |
| 4 | Seed data | SeedData.cs, demo-ready on first run |
| 5 | Role switcher | CurrentUserService, RoleSwitcher component |
| 6 | Layouts & navigation | MainLayout, AdminLayout, placeholder pages |
| 7 | Employee CRUD | EmployeeService, Admin employee pages, unit tests |
| 8 | Teammate management | Bidirectional teammate logic, Admin page, unit tests |
| 9 | Employee & Teammate API | 8 Minimal API endpoints, DTOs, integration tests |
| 10 | Reviews | ReviewService, review pages, unit tests |
| 11 | Feedback | FeedbackService, feedback pages, unit tests |
| 12 | Notifications | NotificationService, inbox page, badge, unit tests |
| 13 | Reminder logic (pure) | ReminderService.ProcessAsync, comprehensive unit tests |
| 14 | Reminder background service | IHostedService thin wrapper, startup trigger |
| 15 | Dashboard & Home | TM dashboard, role-aware home page |
| 16 | Remaining API endpoints | 8 Review/Feedback/Notification endpoints, integration tests |
| 17 | Polish | Styling, validation, error handling, final verification |

---

## Changes from Original Plan

| Change | What | Why |
|---|---|---|
| **Split Iteration 2** | Old "Entities & Database" → New Iteration 2 (Entities & DbContext, compile-only) + Iteration 3 (Database Initialization & Migration) | Separates model correctness (compile-time) from infrastructure concerns (connection strings, migration tooling, startup code). One class of errors at a time. |
| **De-risked Reminder Engine** | Old Iteration 11 (all-in-one) → New Iteration 13 (pure `ReminderService.ProcessAsync` with full tests) + Iteration 14 (thin `IHostedService` wrapper) | The reminder logic is the most complex query in the system. Testing it in isolation against in-memory DB is fast and reliable. The background wrapper is trivial once the logic is proven. |
| **Spread API Endpoints** | Old Iteration 13 (all 16 endpoints at once) → New Iteration 9 (Employee & Teammate endpoints, 8 endpoints) + Iteration 16 (Review/Feedback/Notification endpoints, 8 endpoints) | Smaller, focused API iterations. Employee endpoints introduced right after Employee/Teammate services are stable, providing faster feedback. Remaining endpoints added after all services exist. |
| **Added Verification Checklists** | Every iteration now ends with a concrete checklist: what to test manually, what to verify in the database, what `dotnet test` should show | Makes each iteration self-validating. Reduces risk of proceeding with a broken state. |
| **Added Troubleshooting Section** | New "EF Core Troubleshooting Quick Reference" at the top | EF Core relationship configuration is the highest-risk area. Having a lookup table for common errors saves debugging time. |
| **Added Execution Safety Principles** | New section at the top with 5 ground rules | Codifies the incremental approach: one concept per iteration, compile before run, test before commit. |
| **Added Pitfall Warnings** | Entity, migration, teammate, and background service iterations include `⚠️ Common pitfalls` sections | Preemptive guidance for the most likely errors at each stage. |
| **Iteration count** | 14 → 17 | Three additional iterations from strategic splits. Each iteration is smaller and more focused. No functionality was removed or simplified. |
