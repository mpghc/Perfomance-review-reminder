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
