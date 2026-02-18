# Performance Review Reminder Bot - Iteration Plan

This document outlines the step-by-step iterations for building the Performance Review Reminder Bot with at least 90% AI-generated code.

## Project Overview

**Technology Stack:**
- .NET 8 (LTS)
- Blazor Server
- EF Core 8
- SQLite
- xUnit for testing
- Bootstrap for UI styling

**Architecture Rules:**
- Simplified layered architecture: Blazor UI → Services → Data (DbContext)
- Domain entities separated from services
- Use EF Core DbContext directly in services
- No CQRS, MediatR, generic repository, or Unit of Work abstraction
- No speculative abstractions (YAGNI)
- Keep code simple, readable, and production-quality
- All multi-entity updates must be transactional

---

## Iteration 1: Project Setup & Infrastructure

**Goal:** Set up the foundational .NET 8 Blazor Server project with proper structure and dependencies.

### Tasks
- [ ] Create .NET 8 Blazor Server solution and project using `dotnet new blazorserver`
- [ ] Set up folder structure following layered architecture
- [ ] Install required NuGet packages:
  - Microsoft.EntityFrameworkCore.Sqlite (8.x)
  - Microsoft.EntityFrameworkCore.Tools (8.x)
  - xUnit (2.x)
  - xUnit.runner.visualstudio (2.x)
  - Coverlet.collector (for code coverage)
- [ ] Configure project settings and dependencies
- [ ] Create test project structure using `dotnet new xunit`
- [ ] Document folder structure in README
- [ ] Verify build and run

### Expected Folder Structure
```
PerformanceReviewReminder/
├── Pages/              # Blazor pages (.razor)
├── Components/         # Reusable Blazor components
├── Layouts/           # MainLayout.razor, AdminLayout.razor
├── Services/          # Business logic services
├── Data/              # ApplicationDbContext and migrations
├── Models/            # Domain entities (Employee, Review, Feedback, etc.)
├── wwwroot/           # Static files, CSS, JS
├── Program.cs
└── appsettings.json

PerformanceReviewReminder.Tests/
├── Services/          # Service layer unit tests
└── Helpers/           # Test helpers and fixtures
```

### Acceptance Criteria
- [ ] Solution builds successfully (`dotnet build`)
- [ ] Blazor app runs without errors (`dotnet run`)
- [ ] All dependencies installed and referenced
- [ ] Folder structure created and documented
- [ ] Test project can be run (`dotnet test`)
- [ ] README updated with setup instructions

### Dependencies
None - This is the first iteration

---

## Iteration 2: Core Domain Entities & Database

**Goal:** Create domain models and set up Entity Framework Core with SQLite database.

### Tasks
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

### Entity Relationships
- Employee has many PerformanceReviews (as reviewee)
- Employee has many PerformanceReviews (as reviewer)
- Employee belongs to one Department
- Department has one Manager (Employee)
- PerformanceReview has many Feedbacks
- PerformanceReview has many ReminderLogs

### Acceptance Criteria
- [ ] All entities created with proper properties and data annotations
- [ ] ApplicationDbContext configured with all DbSets
- [ ] Relationships properly configured using Fluent API
- [ ] Migration created successfully
- [ ] Database can be created and seeded
- [ ] No circular dependencies in entity relationships
- [ ] Entities follow C# naming conventions

### Dependencies
- Iteration 1 must be completed

---

## Iteration 3: Service Layer Implementation

**Goal:** Implement service classes for core business logic with transactional support.

### Tasks
- [ ] Create IEmployeeService interface and EmployeeService implementation
  - GetAllEmployees()
  - GetEmployeeById(int id)
  - CreateEmployee(Employee employee)
  - UpdateEmployee(Employee employee)
  - DeleteEmployee(int id)
- [ ] Create IDepartmentService interface and DepartmentService implementation
  - GetAllDepartments()
  - GetDepartmentById(int id)
  - CreateDepartment(Department department)
  - UpdateDepartment(Department department)
- [ ] Create IReviewService interface and ReviewService implementation
  - GetAllReviews()
  - GetReviewById(int id)
  - GetReviewsByMonth(int year, int month)
  - CreateReview(PerformanceReview review)
  - UpdateReviewStatus(int id, string status)
  - GetCurrentMonthReviews()
- [ ] Create IFeedbackService interface and FeedbackService implementation
  - GetFeedbacksByReviewId(int reviewId)
  - SubmitFeedback(Feedback feedback)
  - GetMissingFeedbackForReview(int reviewId)
- [ ] Register all services in Program.cs with appropriate lifetime (Scoped)
- [ ] Implement transactional updates using DbContext.SaveChanges()
- [ ] Add error handling and logging

### Acceptance Criteria
- [ ] All services implement their interfaces
- [ ] Services use DbContext directly (no repository pattern)
- [ ] Multi-entity updates are transactional
- [ ] Services registered in DI container
- [ ] Basic error handling implemented
- [ ] Code follows SOLID principles
- [ ] No speculative abstractions added

### Dependencies
- Iteration 2 must be completed

---

## Iteration 4: Basic Blazor UI & Layouts

**Goal:** Create the foundational UI structure with routing and layouts.

### Tasks
- [ ] Create MainLayout.razor (standard user view)
  - Navigation menu
  - Header with app title
  - Main content area
  - Footer
- [ ] Create AdminLayout.razor (manager view)
  - Admin navigation menu
  - Header with admin title
  - Main content area
  - Different styling/navigation from MainLayout
- [ ] Update _Imports.razor with common using statements
- [ ] Create shared navigation components
  - NavMenu.razor (for MainLayout)
  - AdminNavMenu.razor (for AdminLayout)
- [ ] Set up routing in App.razor
- [ ] Create basic Index.razor page (home page)
- [ ] Style layouts using Bootstrap classes
- [ ] Add responsive design considerations

### Acceptance Criteria
- [ ] MainLayout renders correctly
- [ ] AdminLayout renders correctly with distinct styling
- [ ] Navigation works between pages
- [ ] Layouts are responsive (mobile-friendly)
- [ ] Bootstrap styling applied consistently
- [ ] No console errors in browser
- [ ] Routing configuration is clean and maintainable

### Dependencies
- Iteration 1 must be completed

---

## Iteration 5: Employee & Review Management Pages

**Goal:** Create pages for managing employees and performance reviews.

### Tasks
- [ ] Create Employees.razor page (MainLayout)
  - List all employees in a table
  - Add employee button and form
  - Edit employee functionality
  - Delete employee with confirmation
  - Search/filter employees
- [ ] Create EmployeeDetails.razor page
  - Show employee information
  - List employee's reviews
  - Navigation to review details
- [ ] Create Reviews.razor page (MainLayout)
  - List all reviews in a table
  - Filter by status, date, employee
  - Create new review button and form
  - Update review status
- [ ] Create ReviewDetails.razor page
  - Show review information
  - List feedbacks for the review
  - Link to submit feedback
- [ ] Create reusable components:
  - EmployeeForm.razor
  - ReviewForm.razor
  - ConfirmDialog.razor
- [ ] Implement form validation
- [ ] Add loading indicators
- [ ] Handle errors gracefully with user-friendly messages

### Acceptance Criteria
- [ ] All pages render correctly
- [ ] CRUD operations work for employees
- [ ] CRUD operations work for reviews
- [ ] Forms have proper validation
- [ ] User feedback for successful/failed operations
- [ ] Tables are sortable and filterable
- [ ] UI is responsive and user-friendly
- [ ] Services are properly injected and used

### Dependencies
- Iteration 3 must be completed
- Iteration 4 must be completed

---

## Iteration 6: Feedback Submission & Tracking

**Goal:** Implement feedback submission functionality and tracking.

### Tasks
- [ ] Create FeedbackSubmission.razor page
  - Form to submit feedback for a review
  - Rich text editor or textarea for feedback content
  - Select reviewee from dropdown
  - Submit and cancel buttons
  - Validation (required fields, max length)
- [ ] Create FeedbackList.razor component
  - Display list of feedbacks for a review
  - Show submission date and author
  - Indicate missing feedbacks
- [ ] Update ReviewDetails.razor to show feedback status
  - List of submitted feedbacks
  - List of missing feedbacks (expected reviewers who haven't submitted)
  - Visual indicators (badges, colors)
- [ ] Implement feedback submission logic in service
  - Validate review exists and is active
  - Ensure no duplicate feedback from same person
  - Update review status if all feedbacks received
- [ ] Add navigation from review list to feedback submission

### Acceptance Criteria
- [ ] Users can submit feedback for reviews
- [ ] Feedback form validates inputs
- [ ] Duplicate submissions are prevented
- [ ] Feedback list shows all submissions
- [ ] Missing feedbacks are clearly identified
- [ ] Review status updates automatically when all feedbacks received
- [ ] UI provides clear feedback on success/failure

### Dependencies
- Iteration 5 must be completed

---

## Iteration 7: Reminder Service & Background Worker

**Goal:** Implement the reminder logic and background service for daily execution.

### Tasks
- [ ] Create IReminderService interface and ReminderService implementation
  - GetCurrentMonthReviews() - get reviews scheduled for current month
  - IdentifyMissingFeedback(reviewId) - identify which reviewers haven't submitted
  - SendReminder(reviewId, recipientType) - log reminder (simulated send)
  - LogReminder(reviewId, sentAt, recipientType) - persist to ReminderLog
  - ProcessDailyReminders() - main entry point for background worker
- [ ] Implement reminder logic:
  - Check reviews scheduled for current month
  - For each review, check if all feedbacks are submitted
  - If feedbacks missing, log reminder to team members
  - If deadline approaching, log reminder to manager
- [ ] Create ReminderBackgroundService inheriting from BackgroundService
  - Simple daily execution simulation (configurable interval for testing)
  - Call ReminderService.ProcessDailyReminders()
  - Handle errors gracefully
  - Log all activities
- [ ] Register services in Program.cs
  - ReminderService as Scoped
  - ReminderBackgroundService as Hosted Service
- [ ] Add configuration in appsettings.json
  - ReminderIntervalHours (default: 24)
  - EnableReminders (true/false)
- [ ] Ensure transactional consistency (reminder logs created within DB transaction)
- [ ] Add logging throughout reminder process

### Acceptance Criteria
- [ ] ReminderService correctly identifies current month reviews
- [ ] Missing feedbacks are accurately identified
- [ ] Reminders are logged to database (ReminderLog table)
- [ ] Background service runs on schedule
- [ ] All operations are transactional
- [ ] Comprehensive logging is in place
- [ ] Service can be enabled/disabled via configuration
- [ ] No complex scheduling frameworks used (simple daily check)

### Dependencies
- Iteration 3 must be completed
- Iteration 6 must be completed

---

## Iteration 8: Admin Reporting & Dashboard

**Goal:** Create admin pages for managers to view reports and missing feedback.

### Tasks
- [ ] Create AdminDashboard.razor page (AdminLayout)
  - Summary cards: total reviews, pending reviews, missing feedbacks
  - Charts or graphs (optional, use simple HTML/CSS if needed)
  - Quick links to reports
- [ ] Create MissingFeedbackReport.razor page (AdminLayout)
  - Table listing employees with missing feedback
  - Columns: Employee Name, Review Date, Expected Feedbacks, Submitted, Missing
  - Filter by department, date range
  - Export functionality (optional - download as CSV)
- [ ] Create ReminderLogReport.razor page (AdminLayout)
  - Table showing all sent reminders
  - Columns: Review, Recipient Type, Sent Date
  - Filter by date range, review status
- [ ] Create IReportService interface and ReportService implementation
  - GetMissingFeedbackReport() - returns list of reviews with missing feedbacks
  - GetReminderLogReport(dateFrom, dateTo) - returns reminder logs in date range
  - GetDashboardSummary() - returns summary statistics
- [ ] Add navigation in AdminLayout to new pages
- [ ] Implement proper authorization (ensure only managers can access)
- [ ] Style admin pages consistently with Bootstrap

### Acceptance Criteria
- [ ] Admin dashboard displays correct summary information
- [ ] Missing feedback report shows accurate data
- [ ] Reminder log report displays all reminders
- [ ] Reports can be filtered as specified
- [ ] Admin layout clearly distinguishes from user layout
- [ ] All admin pages use AdminLayout
- [ ] Navigation between admin pages works smoothly
- [ ] Report queries are efficient (no N+1 problems)

### Dependencies
- Iteration 4 must be completed
- Iteration 7 must be completed

---

## Iteration 9: Testing & Quality Assurance

**Goal:** Implement comprehensive unit tests for service layer and reminder logic.

### Tasks
- [ ] Create unit tests for EmployeeService
  - Test CRUD operations
  - Test error scenarios (null inputs, non-existent IDs)
  - Use in-memory database for isolation
- [ ] Create unit tests for ReviewService
  - Test GetCurrentMonthReviews()
  - Test review status transitions
  - Test GetReviewsByMonth()
- [ ] Create unit tests for FeedbackService
  - Test feedback submission
  - Test duplicate prevention
  - Test missing feedback identification
- [ ] Create unit tests for ReminderService
  - Test ProcessDailyReminders()
  - Test IdentifyMissingFeedback()
  - Test reminder logging
  - Mock time to test current month logic
- [ ] Create unit tests for ReportService
  - Test report generation
  - Test filtering logic
- [ ] Set up test database helpers
  - In-memory DbContext for tests
  - Seed data helpers for consistent test data
- [ ] Ensure test coverage is at least 70% for service layer
- [ ] Fix any bugs discovered during testing
- [ ] Add integration smoke tests (optional)

### Test Patterns
- Use xUnit's IClassFixture for shared test context
- Use in-memory SQLite database for isolation
- Follow AAA pattern (Arrange, Act, Assert)
- Use descriptive test method names
- Mock external dependencies if needed

### Acceptance Criteria
- [ ] All service methods have unit tests
- [ ] All tests pass consistently
- [ ] Tests are isolated and don't depend on each other
- [ ] Test coverage meets minimum threshold (70%)
- [ ] Critical reminder logic is thoroughly tested
- [ ] Tests follow naming conventions
- [ ] Tests are maintainable and readable

### Dependencies
- Iterations 3, 6, 7, and 8 must be completed

---

## Iteration 10: Final Integration & Documentation

**Goal:** Complete the application with final touches, documentation, and deployment readiness.

### Tasks
- [ ] Create comprehensive README.md
  - Project description
  - Technology stack
  - Architecture overview
  - Setup instructions
  - How to run the application
  - How to run tests
  - Configuration options
- [ ] Add XML documentation comments to all public APIs
  - Service interfaces and implementations
  - Models with complex properties
- [ ] Create ARCHITECTURE.md document
  - Explain the layered architecture
  - Describe each layer's responsibility
  - Show dependency flow
  - Explain key design decisions
- [ ] Add seed data script for demo purposes
  - Sample employees
  - Sample departments
  - Sample reviews for current and past months
  - Sample feedbacks
- [ ] Create appsettings.Development.json with development settings
- [ ] Configure logging levels appropriately
- [ ] Test end-to-end workflows:
  - Create employee → Create review → Submit feedback → Reminder runs → Admin report
- [ ] Performance review:
  - Check for N+1 query problems
  - Ensure proper use of async/await
  - Verify database indexes (if needed)
- [ ] Security review:
  - No SQL injection vulnerabilities
  - Proper input validation
  - No sensitive data in logs
- [ ] Create deployment documentation
  - How to publish the application
  - Database migration steps
  - Environment variable configuration
- [ ] Final code review and cleanup
  - Remove commented code
  - Ensure consistent formatting
  - Remove unused using statements
  - Verify all TODOs are resolved

### Acceptance Criteria
- [ ] README is comprehensive and clear
- [ ] All code is documented
- [ ] Seed data script works correctly
- [ ] End-to-end workflows function as expected
- [ ] No performance issues identified
- [ ] No security vulnerabilities found
- [ ] Application is ready for deployment
- [ ] Documentation enables a new developer to understand and run the project
- [ ] At least 90% of code was AI-generated (verified)

### Dependencies
- All previous iterations must be completed

---

## Notes on AI-Assisted Development

### Best Practices for Each Iteration
1. **Show folder structure before generating files** - Always confirm structure before code generation
2. **Generate files step by step** - Don't generate entire iteration at once
3. **Wait for approval between major phases** - Get human review after each major component
4. **Don't regenerate unchanged files** - Only generate new or modified files
5. **Test incrementally** - Run tests after each service/component is created
6. **Review AI-generated code** - Human acts as reviewer, checking for quality and correctness

### Human Responsibilities
- Review all AI-generated code
- Make architectural decisions
- Approve progression to next iteration
- Test the application manually
- Provide feedback on code quality
- Make final decisions on design choices

### AI Responsibilities
- Generate code following specifications
- Follow .NET and C# best practices
- Implement unit tests
- Create documentation
- Follow architecture rules strictly
- Suggest improvements when appropriate

---

## Success Criteria for Entire Project

The project is considered complete when:
- ✅ All 10 iterations are finished
- ✅ At least 90% of code is AI-generated
- ✅ All functional requirements are met
- ✅ All UI requirements are met
- ✅ Reminder logic works correctly
- ✅ Testing requirements are satisfied
- ✅ Application runs without critical bugs
- ✅ Documentation is complete and accurate
- ✅ Code is production-quality (clean, maintainable, well-structured)

---

## Estimated Timeline

- Iteration 1: 1 session
- Iteration 2: 1-2 sessions
- Iteration 3: 2-3 sessions
- Iteration 4: 1-2 sessions
- Iteration 5: 2-3 sessions
- Iteration 6: 1-2 sessions
- Iteration 7: 2-3 sessions
- Iteration 8: 2 sessions
- Iteration 9: 2-3 sessions
- Iteration 10: 1-2 sessions

**Total: ~15-25 sessions** depending on complexity and review time
