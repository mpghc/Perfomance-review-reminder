# Prompts Log

A record of the AI-assisted documentation process for this project.

---

## Session 1 — February 19, 2026

### Prompt 1: Initial Documentation Request

**User prompt (summary):**
> Create /doc folder with high-level project description. Project is "Performance Review Reminder Bot" — a web app where employees get reminders to submit peer feedback before performance reviews, and the manager gets alerts about missing feedback. Should be a study project with 90%+ AI-generated code. Include pages, components, routing, multiple layouts.

**AI asked clarifying questions:**
- Blazor hosting model → **User chose: Blazor Server**
- Database → **User chose: SQLite**
- Email notifications → **User chose: Fake email (log to DB/UI)**
- Authentication → **User chose: Seed-based roles (no login)**

**What was created:**
- `doc/overview.md` — project purpose, scope, tech stack, roles
- `doc/requirements.md` — 7 functional requirements, 5 NFRs, 10 user stories
- `doc/architecture.md` — project structure, routing plan, design decisions
- `doc/data-model.md` — 5 entities with ER diagram and seed data plan
- `README.md` — updated with summary and doc links

**User contribution:** Provided project idea, description, key scenario (Tom/Bill example), and made all tech-stack decisions.

---

### Prompt 2: Merge Roles & Remove Architecture

**User prompt (summary):**
> 1. Merge Manager and Admin role — keep only Manager.  
> 2. Don't create architecture doc for now — business only.

**What was done:**
- Removed the Admin role entirely; Manager now handles both admin and management tasks
- Deleted `doc/architecture.md`
- Updated all docs (overview, requirements, data-model, README) to reflect single Manager role
- Removed architecture link from README

**User contribution:** Simplified the role model; decided to defer technical architecture to a later step.

---

### Prompt 3: Rename Manager to Talent Manager + Admin Page

**User prompt (summary):**
> Rename Manager to "Talent Manager" — responsible for a pool of employees and their performance reviews. CRUD of employees and teammates should be on an Admin page.

**What was done:**
- Renamed "Manager" → "Talent Manager" across all docs
- Updated role enum to `TalentManager` in data model
- FR-1 (Employee Management) and FR-2 (Team Management) now specify a dedicated **Admin page**
- User stories updated to reference Admin page for CRUD operations
- Seed data updated (Bill is now a Talent Manager)

**User contribution:** Defined the Talent Manager concept; decided Admin page is a page (not a role) for employee/team CRUD.

---

### Prompt 4: Create Prompts Log

**User prompt (summary):**
> Create PromptsLog.md in docs folder with chat summary.

**What was done:**
- Created this file.

---

### Prompt 5: Architecture Document

**User prompt (summary):**
> Propose architecture: solution structure, entities, pages/routing, layouts, API endpoints, testing strategy. Then save as architecture.md.

**What was done:**
- Created `doc/architecture.md` with full technical architecture
- Solution structure: single Blazor Server project + test project, organized by folders (Components, Data, Entities, Services, Endpoints)
- No repository pattern — services use `AppDbContext` directly
- 11 pages across 5 route groups, using two layouts (MainLayout, AdminLayout)
- Minimal API with 16 endpoints under `/api` (Employees, Teammates, Reviews, Feedback, Notifications)
- Testing strategy: xUnit + NSubstitute, 10 key test scenarios covering services and background job
- Updated `README.md` with link to architecture doc

**User contribution:** Reviewed and approved the proposed architecture.

---

### Prompt 6: Implementation Plan

**User prompt (summary):**
> Generate implementation plan with iteration stages (init app, DB, mock data, base pages, etc.). Save to /doc folder.

**What was done:**
- Created `doc/implementation-plan.md` with 14 iteration stages
- Each iteration is self-contained and produces a committable, runnable state
- Stages: scaffolding → entities & DB → seed data → role switcher → layouts → employee CRUD → teammates → reviews → feedback → notifications → reminder engine → dashboard → API endpoints → polish
- Updated `README.md` with link to implementation plan

**User contribution:** Requested iterative approach with clear stages.

---

## Session 2 — February 19, 2026

### Prompt 7: Implementation — Iterations 1–7

**User prompt (summary):**
> Implement the approved Implementation Plan iteration by iteration. Start with Iteration 1. Stop after each iteration for review. Commit after approval.

**Execution rules established by user:**
- Implement only the requested iteration
- Stop and wait for review after each iteration
- Keep changes small and safe
- Commit to git after approval

---

#### Iteration 1 — Project Scaffolding

**What was done:**
- Created branch `feature/iteration-1-project-scaffolding`
- Scaffolded Blazor Server app (`dotnet new blazor --interactivity server --no-https --framework net9.0`)
- Created xUnit test project with project reference
- Added NuGet packages: EF Core SQLite 9.x, Design, NSubstitute, Mvc.Testing
- Created folder structure: `Entities/`, `Data/`, `Services/`, `Endpoints/`, page subfolders, `Components/Shared/`

**AI asked clarifying questions:**
- .NET version → **User chose: .NET 9** (not .NET 10 which was installed as SDK 10.0.103)

**User contribution:** Chose .NET 9 target framework.

---

#### UI Tweaks (between iterations)

**User prompts (summary):**
> 1. Change nav bar background color (chose dark slate/indigo gradient `#1e1e2f → #3b3b5c`)
> 2. Remove Weather and Counter pages
> 3. Remove About link
> 4. Change navbar brand to "ReviewPulse"

**What was done:**
- Applied gradient to `MainLayout.razor.css` sidebar
- Deleted `Counter.razor` and `Weather.razor`
- Removed About link from `MainLayout.razor`
- Changed brand text from "PerformanceReviewReminderBot.Web" to "ReviewPulse" in `NavMenu.razor`

**User contribution:** Made all visual/branding decisions.

---

#### Iteration 2 — Entities & DbContext (Compile-Only)

**What was done:**
- Created enums: `EmployeeRole`, `ReviewStatus`, `NotificationType`
- Created 5 entity classes: `Employee`, `EmployeeTeammate`, `PerformanceReview`, `Feedback`, `Notification`
- Created `AppDbContext` with full `OnModelCreating` configuration
- Key design decisions: `DeleteBehavior.Restrict` on TalentManager FK, `Cascade` on EmployeeTeammate, `SetNull` on Notification.Review FK

---

#### README Update (between iterations)

**User prompt (summary):**
> Add detected required dependencies to README.md.

**What was done:**
- Added Prerequisites section (.NET 9 SDK, EF Core CLI)
- Added NuGet Dependencies tables for both web and test projects
- Updated Quick Start with actual URL (`localhost:5064`)

---

#### Iteration 3 — Database Initialization & Migration

**What was done:**
- Added SQLite connection string to `appsettings.json`
- Registered `AppDbContext` in `Program.cs` with SQLite provider
- Added auto-migration on startup
- Created `InitialCreate` migration (5 tables)
- Added `*.db` patterns to `.gitignore`
- Updated `dotnet-ef` tool from 7.0.10 to 10.0.3

---

#### Iteration 4 — Seed Data

**What was done:**
- Created `Data/SeedData.cs` — idempotent seed: 5 employees (Bill as TM, Tom/Alice/Bob/Carol as Employees), 12 teammate rows, 1 review, 2 notifications
- Added `SeedData.Initialize` call in `Program.cs`
- Created `SeedDataTests.cs` with 7 tests (all passing)

---

#### Iteration 5 — CurrentUserService & Role Switcher

**What was done:**
- Created `Services/CurrentUserService.cs` — scoped service, defaults to first TalentManager (Bill), `OnChange` event for UI refresh
- Created `Components/Shared/RoleSwitcher.razor` — dropdown of all employees + role badge
- Created `Components/Shared/NotificationBadge.razor` — placeholder bell icon with "0" count
- Added both components to `NavMenu.razor`
- Registered `CurrentUserService` as scoped in `Program.cs`

---

#### Iteration 6 — Layouts & Navigation Shell

**What was done:**
- Updated `NavMenu.razor` with role-aware nav links: Home (all), Dashboard/Reviews/Admin (TM only), Feedback (Employee only), Inbox (all). Subscribes to `CurrentUser.OnChange` for reactive updates.
- Created `AdminLayout.razor` + `AdminLayout.razor.css` + `AdminNavMenu.razor` — admin sidebar with Employees, Teammates, "← Back to app"
- Added SVG icon CSS classes for all nav items in `NavMenu.razor.css`
- Created 9 placeholder pages: Dashboard, ReviewList, ScheduleReview, PendingFeedback, SubmitFeedback, Inbox, Employees, EmployeeForm, Teammates
- Updated Home.razor with ReviewPulse branding
- Admin pages use `@layout AdminLayout`; all others use default `MainLayout`

---

#### Iteration 7 — Employee Service & Admin CRUD Pages

**What was done:**
- Created `Services/EmployeeService.cs` with methods: `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `GetTalentManagersAsync`
- Validations: required FullName/Email, can't delete employee with reviews, can't delete TM with assigned employees
- Registered `EmployeeService` as scoped in `Program.cs`
- Implemented `Pages/Admin/Employees.razor` — table with Name/Email/Role/TM columns, Add/Edit/Delete with confirmation prompt, success/error alerts
- Implemented `Pages/Admin/EmployeeForm.razor` — create/edit form with DataAnnotations validation, role dropdown, conditional TalentManager dropdown
- Created `EmployeeServiceTests.cs` with 13 unit tests (all passing)

**Test summary at end of session:** 21 total tests, 0 failures.

---

#### Git Commit

**User prompt:**
> Commit changes with git after Iteration approval.

**What was done:**
- Staged all changes and committed on branch `feature/iteration-1-project-scaffolding`
- Commit message: `feat: implement iterations 1-6`
- Iterations 1–6 committed; Iteration 7 pending commit

---

## Summary of User Decisions

| # | Decision | Chosen Option |
|---|---|---|
| 1 | Blazor hosting model | Blazor Server |
| 2 | Database | SQLite |
| 3 | Email notifications | Fake (log to DB, visible in UI) |
| 4 | Authentication | Seed-based roles, no login |
| 5 | Roles | Merged Admin + Manager → single "Talent Manager" role |
| 6 | Architecture doc | Created in Prompt 5 |
| 7 | Admin page | Dedicated page for employee/team CRUD (not a separate role) |
| 8 | Repository pattern | Not used — services call DbContext directly |
| 9 | API layer | Minimal API endpoints alongside Blazor Server pages |
| 10 | Test framework | xUnit + NSubstitute + in-memory SQLite |
| 11 | Implementation approach | 14 iterative stages, each committable |
| 12 | .NET version | .NET 9 (not .NET 10) |
| 13 | Sidebar color | Dark slate/indigo gradient (`#1e1e2f → #3b3b5c`) |
| 14 | App branding | "ReviewPulse" |
