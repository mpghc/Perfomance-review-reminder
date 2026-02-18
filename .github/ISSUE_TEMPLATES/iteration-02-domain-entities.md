# Iteration 2: Core Domain Entities & Database

**Labels:** `iteration`, `database`, `entities`
**Priority:** High
**Estimated Time:** 1-2 sessions
**Depends on:** Iteration 1

## Goal
Create domain models and set up Entity Framework Core with SQLite database.

## Tasks
- [ ] Create Employee entity (Id, Name, Email, DepartmentId, etc.)
- [ ] Create Department entity (Id, Name, ManagerId)
- [ ] Create PerformanceReview entity (Id, EmployeeId, ReviewerId, ScheduledDate, Status, etc.)
- [ ] Create Feedback entity (Id, ReviewId, ProvidedBy, Content, SubmittedAt, etc.)
- [ ] Create ReminderLog entity (Id, ReviewId, SentAt, RecipientType, etc.)
- [ ] Create ApplicationDbContext inheriting from DbContext
- [ ] Configure entity relationships and constraints
- [ ] Add database connection string to appsettings.json
- [ ] Create initial EF Core migration
- [ ] Create database seed data for testing
- [ ] Add DbContext registration in Program.cs

## Entity Relationships
- Employee has many PerformanceReviews (as reviewee)
- Employee has many PerformanceReviews (as reviewer)
- Employee belongs to one Department
- Department has one Manager (Employee)
- PerformanceReview has many Feedbacks
- PerformanceReview has many ReminderLogs

## Key Entities

### Employee
```csharp
- Id (int, PK)
- Name (string, required)
- Email (string, required, unique)
- DepartmentId (int?, FK)
- HireDate (DateTime)
- IsActive (bool)
```

### Department
```csharp
- Id (int, PK)
- Name (string, required)
- ManagerId (int?, FK to Employee)
```

### PerformanceReview
```csharp
- Id (int, PK)
- EmployeeId (int, FK, required)
- ReviewerId (int, FK, required)
- ScheduledDate (DateTime, required)
- Status (string: Scheduled, InProgress, Completed, Cancelled)
- CreatedAt (DateTime)
- CompletedAt (DateTime?)
```

### Feedback
```csharp
- Id (int, PK)
- ReviewId (int, FK, required)
- ProvidedById (int, FK to Employee, required)
- Content (string, required)
- SubmittedAt (DateTime, required)
```

### ReminderLog
```csharp
- Id (int, PK)
- ReviewId (int, FK, required)
- SentAt (DateTime, required)
- RecipientType (string: Employee, Reviewer, Manager)
- Notes (string?)
```

## Acceptance Criteria
- [ ] All entities created with proper properties and data annotations
- [ ] ApplicationDbContext configured with all DbSets
- [ ] Relationships properly configured using Fluent API
- [ ] Migration created successfully (`dotnet ef migrations add InitialCreate`)
- [ ] Database can be created and seeded (`dotnet ef database update`)
- [ ] No circular dependencies in entity relationships
- [ ] Entities follow C# naming conventions (PascalCase)
- [ ] Proper navigation properties configured

## Commands to Run
```bash
# Add migration
dotnet ef migrations add InitialCreate --project PerformanceReviewReminder

# Update database
dotnet ef database update --project PerformanceReviewReminder

# Verify
dotnet build
```

## Dependencies
- Iteration 1 must be completed

## Notes
- Use Fluent API for complex relationships
- Add indexes for frequently queried fields (Email, ScheduledDate)
- Ensure proper cascade delete behaviors
- Human reviews entity relationships before migration creation
