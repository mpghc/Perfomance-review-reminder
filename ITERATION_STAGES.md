# Performance Review Reminder Bot - Iteration Stages

## Overview

This document outlines the phased approach to building the Performance Review Reminder Bot. Each stage represents a logical milestone that can be completed and verified independently.

## Guiding Principles

- **Incremental Development**: Build features in layers, starting with foundation
- **AI-Driven**: Aim for 90%+ AI-generated code
- **Human Review**: Manual approval between major phases
- **Testing First**: Write tests alongside implementation
- **Quality Over Speed**: Production-ready code from the start

---

## Stage 1: Project Foundation

### Goal
Set up the .NET 8 Blazor Server project with proper structure and configuration.

### Tasks
1. Create .NET 8 Blazor Server application
2. Configure project structure (folders for Data, Services, Pages)
3. Install required NuGet packages:
   - Microsoft.EntityFrameworkCore.Sqlite
   - Microsoft.EntityFrameworkCore.Design
   - Microsoft.EntityFrameworkCore.Tools
4. Set up appsettings.json with connection string
5. Configure Program.cs with dependency injection
6. Add Bootstrap to _Host.cshtml or App.razor
7. Create basic .editorconfig for code style

### Deliverables
- ✅ Working Blazor Server project
- ✅ Proper folder structure
- ✅ Configuration files
- ✅ Dependencies installed

### Verification
- Project builds successfully
- Blazor app runs and displays default page
- No compilation errors

### Estimated Effort
~1 hour

---

## Stage 2: Domain Entities

### Goal
Define the core domain model with all entities and enums.

### Tasks
1. Create `Data/Entities` folder
2. Implement `Employee.cs` entity
3. Implement `PerformanceReview.cs` entity
4. Implement `Feedback.cs` entity
5. Implement `ReminderLog.cs` entity
6. Create `ReviewStatus` enum
7. Add XML documentation to all entities
8. Configure entity relationships

### Deliverables
- ✅ Employee entity with validation attributes
- ✅ PerformanceReview entity with status enum
- ✅ Feedback entity
- ✅ ReminderLog entity
- ✅ Well-documented entities

### Verification
- All entities compile
- Entities follow C# naming conventions
- Relationships are correctly defined
- Code review by human

### Estimated Effort
~2 hours

---

## Stage 3: Data Layer (DbContext & Migrations)

### Goal
Set up EF Core DbContext and create initial database schema.

### Tasks
1. Create `ApplicationDbContext.cs`
2. Configure DbSet properties for all entities
3. Override `OnModelCreating` to configure:
   - Primary keys
   - Foreign keys
   - Indexes
   - Required fields
   - Max lengths
4. Create initial EF Core migration
5. Apply migration to create SQLite database
6. Add seed data (optional)

### Deliverables
- ✅ ApplicationDbContext with all DbSets
- ✅ Model configuration
- ✅ Initial migration
- ✅ SQLite database created

### Verification
- Migration creates database successfully
- Database schema matches entity definitions
- Can query empty tables
- Code review by human

### Estimated Effort
~2 hours

---

## Stage 4: Service Layer - Part 1 (CRUD Services)

### Goal
Implement basic CRUD operations for employees, reviews, and feedback.

### Tasks
1. Create `EmployeeService.cs` with:
   - CreateEmployeeAsync
   - UpdateEmployeeAsync
   - DeleteEmployeeAsync
   - GetEmployeeByIdAsync
   - GetAllEmployeesAsync
   - GetSubordinatesAsync
2. Create `PerformanceReviewService.cs` with:
   - CreateReviewAsync
   - UpdateReviewAsync
   - DeleteReviewAsync
   - GetReviewByIdAsync
   - GetReviewsForEmployeeAsync
   - GetReviewsForCurrentMonthAsync
   - UpdateReviewStatusAsync
3. Create `FeedbackService.cs` with:
   - SubmitFeedbackAsync
   - GetFeedbackForReviewAsync
   - GetFeedbackByEmployeeAsync
4. Register services in Program.cs
5. Add proper error handling
6. Add XML documentation

### Deliverables
- ✅ EmployeeService with all CRUD methods
- ✅ PerformanceReviewService with all CRUD methods
- ✅ FeedbackService with all CRUD methods
- ✅ Services registered in DI container

### Verification
- Services compile successfully
- Services can be injected via DI
- Basic operations work (manual testing)
- Code review by human

### Estimated Effort
~4 hours

---

## Stage 5: Service Layer - Part 2 (Reminder Logic)

### Goal
Implement the core reminder logic and background service.

### Tasks
1. Create `ReminderService.cs` with:
   - CheckAndSendRemindersAsync (main logic)
   - GetReminderLogsAsync
   - GetReminderLogsForReviewAsync
2. Implement reminder logic:
   - Query current month reviews
   - Check for missing feedback
   - Log reminders
   - Save reminder logs
   - Ensure transactional consistency
3. Create `ReminderBackgroundService.cs`:
   - Inherit from BackgroundService
   - Implement daily execution logic
   - Use IServiceScopeFactory
   - Add proper logging
4. Register services in Program.cs
5. Add configuration for reminder settings

### Deliverables
- ✅ ReminderService with complete logic
- ✅ ReminderBackgroundService
- ✅ Transactional reminder operations
- ✅ Comprehensive logging

### Verification
- Reminder logic identifies reviews correctly
- Reminders are logged properly
- Background service runs without errors
- Transactions work correctly
- Code review by human

### Estimated Effort
~3 hours

---

## Stage 6: UI Foundation (Layouts & Components)

### Goal
Create the basic UI structure with layouts and reusable components.

### Tasks
1. Create `Shared/MainLayout.razor`
2. Create `Shared/AdminLayout.razor`
3. Update `Shared/NavMenu.razor` for both layouts
4. Create `Shared/Components/EmployeeCard.razor`
5. Create `Shared/Components/ReviewStatusBadge.razor`
6. Add Bootstrap classes for styling
7. Configure routing in App.razor
8. Create _Imports.razor with common usings

### Deliverables
- ✅ MainLayout for standard users
- ✅ AdminLayout for managers
- ✅ Reusable components
- ✅ Styled navigation menus
- ✅ Responsive Bootstrap UI

### Verification
- Both layouts render correctly
- Navigation works
- Components are reusable
- Bootstrap styling applied
- Manual UI review

### Estimated Effort
~3 hours

---

## Stage 7: Employee Management UI

### Goal
Implement full CRUD UI for employee management.

### Tasks
1. Create `Pages/Employees/Index.razor`:
   - List all employees
   - Search/filter functionality
   - Action buttons (Edit, Delete)
2. Create `Pages/Employees/Create.razor`:
   - Form for new employee
   - Validation
   - Manager dropdown
3. Create `Pages/Employees/Edit.razor`:
   - Pre-populated form
   - Validation
   - Update logic
4. Create `Pages/Employees/Delete.razor`:
   - Confirmation dialog
   - Delete logic
5. Add proper routing
6. Add error handling and user feedback

### Deliverables
- ✅ Employee list page
- ✅ Create employee page
- ✅ Edit employee page
- ✅ Delete employee page
- ✅ Form validation
- ✅ User feedback (success/error messages)

### Verification
- Can create, read, update, delete employees
- Validation works correctly
- UI is user-friendly
- No UI bugs
- Manual UI testing

### Estimated Effort
~4 hours

---

## Stage 8: Performance Review Management UI

### Goal
Implement UI for managing performance reviews.

### Tasks
1. Create `Pages/Reviews/Index.razor`:
   - List all reviews
   - Filter by employee, date, status
   - Action buttons
2. Create `Pages/Reviews/Create.razor`:
   - Form for new review
   - Employee dropdown
   - Date picker
   - Status selection
3. Create `Pages/Reviews/Edit.razor`:
   - Pre-populated form
   - Update review details
4. Create `Pages/Reviews/Details.razor`:
   - Show review information
   - Display associated feedback
   - Allow feedback submission link
5. Add routing and navigation

### Deliverables
- ✅ Review list page
- ✅ Create review page
- ✅ Edit review page
- ✅ Review details page
- ✅ Feedback display

### Verification
- CRUD operations work
- Review details display correctly
- Filtering works
- Manual UI testing

### Estimated Effort
~4 hours

---

## Stage 9: Feedback Submission UI

### Goal
Implement UI for submitting and viewing feedback.

### Tasks
1. Create `Pages/Feedback/Index.razor`:
   - List feedback submitted by current user (simulated)
   - Show associated reviews
2. Create `Pages/Feedback/Submit.razor`:
   - Form for submitting feedback
   - Review selection
   - Text area for content
   - Validation
3. Integrate feedback submission from review details page
4. Add success notifications

### Deliverables
- ✅ Feedback list page
- ✅ Feedback submission form
- ✅ Integration with reviews
- ✅ User feedback notifications

### Verification
- Can submit feedback
- Feedback appears in database
- Feedback displays on review details
- Manual UI testing

### Estimated Effort
~2 hours

---

## Stage 10: Admin Dashboard & Reports

### Goal
Implement admin functionality for managers.

### Tasks
1. Create `Pages/Admin/Dashboard.razor`:
   - System statistics
   - Count of employees
   - Count of scheduled reviews
   - Count of pending feedback
   - Recent activity
2. Create `Pages/Admin/MissingFeedback.razor`:
   - Report of reviews missing feedback
   - Filter by date range
   - Employee information
   - Review details
   - Action buttons (send reminder)
3. Apply AdminLayout to admin pages
4. Add admin navigation menu items

### Deliverables
- ✅ Admin dashboard with stats
- ✅ Missing feedback report
- ✅ AdminLayout applied
- ✅ Professional admin interface

### Verification
- Dashboard displays correct statistics
- Missing feedback report is accurate
- Admin UI is functional
- Manual testing

### Estimated Effort
~3 hours

---

## Stage 11: Unit Tests - Services

### Goal
Implement comprehensive unit tests for the service layer.

### Tasks
1. Create test project: `PerformanceReviewBot.Tests`
2. Add xUnit and test dependencies
3. Create `TestHelpers/InMemoryDbContextFactory.cs`
4. Implement `EmployeeServiceTests.cs`:
   - Test all CRUD operations
   - Test manager relationships
   - Test validation
5. Implement `PerformanceReviewServiceTests.cs`:
   - Test CRUD operations
   - Test status updates
   - Test current month filtering
6. Implement `FeedbackServiceTests.cs`:
   - Test feedback submission
   - Test feedback retrieval
7. Implement `ReminderServiceTests.cs`:
   - Test reminder logic
   - Test missing feedback identification
   - Test reminder logging
   - Test transactions
8. Add Moq for mocking ILogger

### Deliverables
- ✅ Test project configured
- ✅ All service tests implemented
- ✅ Tests use in-memory database
- ✅ High code coverage for services

### Verification
- All tests pass
- Tests are independent
- Tests cover edge cases
- Code review of tests

### Estimated Effort
~5 hours

---

## Stage 12: Integration & Polish

### Goal
Integrate all components, fix bugs, and polish the application.

### Tasks
1. Test end-to-end workflows:
   - Create employee → Create review → Submit feedback
   - Create review → Send reminder → Check logs
   - View admin reports
2. Fix any integration bugs
3. Improve error handling
4. Enhance UI/UX:
   - Loading indicators
   - Better error messages
   - Tooltips and help text
5. Update README.md with:
   - Project description
   - Setup instructions
   - Usage guide
   - Screenshots
6. Add code documentation where missing
7. Clean up unused code
8. Final code review

### Deliverables
- ✅ Fully integrated application
- ✅ All major bugs fixed
- ✅ Polished UI
- ✅ Complete README
- ✅ Documentation

### Verification
- End-to-end testing
- No critical bugs
- Application is production-ready
- Human final review

### Estimated Effort
~4 hours

---

## Stage 13: Final Deployment & Documentation

### Goal
Prepare application for deployment and create final documentation.

### Tasks
1. Create deployment guide
2. Document configuration options
3. Create user manual
4. Add troubleshooting guide
5. Create demo data script
6. Package application for distribution
7. Final quality assurance
8. Handoff preparation

### Deliverables
- ✅ Deployment documentation
- ✅ User manual
- ✅ Demo data
- ✅ Production-ready application

### Verification
- Application can be deployed
- Documentation is complete
- Demo data works
- Final acceptance by stakeholder

### Estimated Effort
~2 hours

---

## Summary

### Total Estimated Effort
Approximately **39 hours** of development time

### Critical Path
1. Foundation (Stage 1)
2. Domain & Data (Stages 2-3)
3. Services (Stages 4-5)
4. UI (Stages 6-10)
5. Testing (Stage 11)
6. Polish (Stages 12-13)

### Success Criteria
- ✅ All functional requirements implemented
- ✅ 90%+ code generated by AI
- ✅ Unit tests with good coverage
- ✅ Clean, maintainable code
- ✅ Production-quality application
- ✅ Complete documentation

### Risk Mitigation
- Manual review after each stage
- Incremental testing throughout
- Early validation of core concepts
- Flexible iteration timeline
- Clear acceptance criteria

---

**Last Updated**: 2026-02-18
**Version**: 1.0
