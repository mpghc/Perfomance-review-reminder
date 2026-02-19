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

## Summary of User Decisions

| # | Decision | Chosen Option |
|---|---|---|
| 1 | Blazor hosting model | Blazor Server |
| 2 | Database | SQLite |
| 3 | Email notifications | Fake (log to DB, visible in UI) |
| 4 | Authentication | Seed-based roles, no login |
| 5 | Roles | Merged Admin + Manager → single "Talent Manager" role |
| 6 | Architecture doc | Deferred to next step |
| 7 | Admin page | Dedicated page for employee/team CRUD (not a separate role) |
| 8 | Data model | Removed Team entity; TM→Employee (1:N) + Employee↔Employee teammates (N:N) |
| 9 | Git workflow | New branch per chat session; commit + PR at end |

---

## Session 2 — February 19, 2026

### Prompt 5: Git Branch Workflow + PR

**User prompt (summary):**
> Move all changes to new git branch, commit, and create a PR to main.

**What was done:**
- Created branch `feature/project-documentation`
- Committed all doc files + README changes
- Pushed branch and created PR #10 via GitHub MCP

**User contribution:** Decided to use branch-based workflow for all changes.

---

### Prompt 6: Automate Prompts Log + PR

**User prompt (summary):**
> Can we make something to add chat summary to PromptsLog.md and create PR easier?

**What was done:**
- Discussed options: PowerShell script, VS Code task, Copilot-native workflow
- User asked about Copilot features specifically
- Recommended workflow: ask Copilot to update PromptsLog.md at end of session, then use VS Code Git + GitHub PR extension for commit/push/PR

**User contribution:** Chose to keep it simple — use Copilot chat + VS Code built-in tools, no custom scripts.

---

### Prompt 7: Git Instructions File

**User prompt (summary):**
> Create common GitHub instruction to add and checkout new branch for new chat and make all changes in this branch.

**What was done:**
- Created `.github/instructions/git.instructions.md` with `applyTo: '**'`
- Rules: new branch from main at session start, all changes on branch, commit + push + PR at end

**User contribution:** Decided to formalize the branching workflow as a Copilot instruction.

---

## Session 3 — February 19, 2026

### Prompt 8: Restructure Relationships (TM → Employees, Teammates)

**User prompt (summary):**
> Talent Manager has 1-to-many Employees. Employee has 0-to-many other Employees as teammates.

**What was done:**
- Created branch `feature/business-logic-changes`
- **Removed `Team` entity** entirely from data model
- Added `TalentManagerId` (self-referencing FK) on Employee — TM manages 1:N employees
- Added `EmployeeTeammate` join table — bidirectional N:N teammate relationship between employees
- Updated FR-2: "Team Management" → "Teammate Management"
- Updated FR-3, FR-4: feedback providers are now "teammates" instead of "team members"
- Updated user stories (US-02, US-04) and overview scenario accordingly
- Seed data updated: explicit teammate pairs instead of team membership

**User contribution:** Redesigned the core domain model — replaced team-based structure with direct TM→Employee ownership and explicit teammate relationships.
