# Requirements & User Stories

## Functional Requirements

### FR-1: Employee Management (Admin Page)

- Talent Manager can list, create, edit, and delete employees.
- Each employee has: Name, Email, Role (Employee / Talent Manager), Team assignment.
- This functionality lives on a dedicated **Admin page**.

### FR-2: Team Management (Admin Page)

- Talent Manager can create and edit teams.
- Each team has a name and an assigned Talent Manager (one per team).
- Employees are assigned to exactly one team.
- This functionality lives on the **Admin page**.

### FR-3: Performance Review Scheduling (Talent Manager)

- Talent Manager can schedule a performance review for an employee by setting a **review date**.
- A review has a status: **Scheduled → In Progress → Completed**.
- When a review is scheduled, the system knows who needs to provide feedback (= all team members of the reviewee, excluding the reviewee themselves).

### FR-4: Notification Engine

- The system runs a **background job** (e.g., daily) that checks all upcoming reviews.
- **Reminder window**: starts `N` days before the review date (default: 14 days).
- For each review in the reminder window, the system creates a notification for every team member who hasn't submitted feedback yet.
- Notifications are stored in the database and displayed in the app UI (fake email).

### FR-5: Feedback Submission (Employee)

- An employee can see a list of peers whose reviews are upcoming and require their feedback.
- An employee can submit text feedback for a peer.
- Once submitted, feedback is marked as done — no more reminders for that review.

### FR-6: Talent Manager Dashboard

- A Talent Manager sees all reviews for their team members.
- For each review, the Talent Manager sees: who has submitted feedback and who hasn't.
- If the deadline is within **3 days** and feedback is still missing, the item is highlighted as **overdue**.
- The Talent Manager also receives notifications about overdue feedback.

### FR-7: Notification Inbox (All Roles)

- Every user has a notification inbox page.
- Notifications are marked as read/unread.
- Notifications include: feedback reminders (for employees), overdue alerts (for Talent Managers).

---

## Non-Functional Requirements

| # | Requirement |
|---|---|
| NFR-1 | The app must be a single Blazor Server project (simple deployment). |
| NFR-2 | SQLite database, auto-created on first run via EF Core migrations. |
| NFR-3 | Seed data is loaded on startup for demo purposes. |
| NFR-4 | No external dependencies beyond .NET SDK. |
| NFR-5 | At least 90% of code is AI-generated. |

---

## User Stories

### Talent Manager

| ID | Story |
|---|---|
| US-01 | As a Talent Manager, I want to manage employees on the Admin page so I can keep the directory up to date. |
| US-02 | As a Talent Manager, I want to manage teams and assign employees on the Admin page. |
| US-03 | As a Talent Manager, I want to schedule performance reviews for employees. |
| US-04 | As a Talent Manager, I want to see a dashboard of upcoming reviews for my team. |
| US-05 | As a Talent Manager, I want to see who has/hasn't submitted feedback for each review. |
| US-06 | As a Talent Manager, I want to be notified when feedback is overdue. |

### Employee

| ID | Story |
|---|---|
| US-07 | As an Employee, I want to see which peers need my feedback. |
| US-08 | As an Employee, I want to submit feedback for a peer's review. |
| US-09 | As an Employee, I want to see my notifications (reminders). |

### All Users

| ID | Story |
|---|---|
| US-10 | As any user, I want to switch my role/identity via a dropdown so I can demo different views. |
