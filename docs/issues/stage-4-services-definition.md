# Stage 4: Services Definition

## Objective
Implement the business logic layer with clear service interfaces and implementations for all domain operations.

## Description
Create service layer that encapsulates business logic and data access operations. Each service will handle a specific domain area (Employees, Performance Reviews, Feedback, Reminders) and will use the DbContext directly for data operations. All services will be registered for dependency injection.

## Services to Create

### 1. IEmployeeService / EmployeeService
**Methods:**
- `Task<IEnumerable<Employee>> GetAllAsync(bool includeInactive = false)`
- `Task<Employee?> GetByIdAsync(int id)`
- `Task<Employee?> GetByEmailAsync(string email)`
- `Task<Employee> CreateAsync(Employee employee)`
- `Task<Employee> UpdateAsync(Employee employee)`
- `Task DeleteAsync(int id)`
- `Task<bool> EmailExistsAsync(string email, int? excludeId = null)`

**Business Rules:**
- Validate email uniqueness before create/update
- Validate required fields
- Check if employee has active reviews before deletion

### 2. IPerformanceReviewService / PerformanceReviewService
**Methods:**
- `Task<IEnumerable<PerformanceReview>> GetAllAsync()`
- `Task<PerformanceReview?> GetByIdAsync(int id)`
- `Task<IEnumerable<PerformanceReview>> GetReviewsForCurrentMonthAsync()`
- `Task<IEnumerable<PerformanceReview>> GetReviewsByStatusAsync(string status)`
- `Task<IEnumerable<PerformanceReview>> GetReviewsByEmployeeAsync(int employeeId)`
- `Task<PerformanceReview> CreateAsync(PerformanceReview review)`
- `Task<PerformanceReview> UpdateAsync(PerformanceReview review)`
- `Task UpdateStatusAsync(int id, string status)`
- `Task DeleteAsync(int id)`

**Business Rules:**
- Ensure reviewer ≠ reviewee (no self-reviews)
- Validate status transitions
- Validate date ranges (scheduled date in future for new reviews)
- Load related entities (Employee, Reviewer) when needed

### 3. IFeedbackService / FeedbackService
**Methods:**
- `Task<IEnumerable<Feedback>> GetAllAsync()`
- `Task<IEnumerable<Feedback>> GetByReviewIdAsync(int reviewId)`
- `Task<Feedback?> GetByIdAsync(int id)`
- `Task<Feedback> SubmitFeedbackAsync(Feedback feedback)`
- `Task<IEnumerable<PerformanceReview>> GetReviewsWithMissingFeedbackAsync()`
- `Task<bool> HasProviderSubmittedFeedbackAsync(int reviewId, int providerId)`

**Business Rules:**
- Prevent duplicate feedback from same provider for same review
- Validate feedback content is not empty
- Validate rating is in range (1-5) if provided
- Ensure review exists and is not completed/cancelled

### 4. IReminderService / ReminderService
**Methods:**
- `Task SendRemindersAsync()`
- `Task<IEnumerable<PerformanceReview>> GetReviewsNeedingRemindersAsync()`
- `Task LogReminderAsync(ReminderLog reminderLog)`
- `Task<IEnumerable<ReminderLog>> GetReminderLogsByReviewAsync(int reviewId)`
- `Task<IEnumerable<ReminderLog>> GetRecentRemindersAsync(int days = 30)`

**Business Rules:**
- Check reviews scheduled for current month
- Identify reviews with missing feedback
- Create simulated reminder (log instead of actual email)
- Ensure transactional consistency (review check + log creation)
- Avoid sending duplicate reminders (check last sent date)

### 5. ReminderBackgroundService
**Inherits:** `BackgroundService`

**Methods:**
- `protected override async Task ExecuteAsync(CancellationToken stoppingToken)`

**Behavior:**
- Run daily at configured time (simulated with short interval for demo)
- Call `IReminderService.SendRemindersAsync()`
- Log execution start/completion
- Handle exceptions gracefully
- Respect cancellation token

## Tasks
- [ ] Create `IEmployeeService.cs` interface in `Services/Interfaces/`
- [ ] Create `EmployeeService.cs` implementation in `Services/`
- [ ] Create `IPerformanceReviewService.cs` interface in `Services/Interfaces/`
- [ ] Create `PerformanceReviewService.cs` implementation in `Services/`
- [ ] Create `IFeedbackService.cs` interface in `Services/Interfaces/`
- [ ] Create `FeedbackService.cs` implementation in `Services/`
- [ ] Create `IReminderService.cs` interface in `Services/Interfaces/`
- [ ] Create `ReminderService.cs` implementation in `Services/`
- [ ] Create `ReminderBackgroundService.cs` in `Services/`
- [ ] Implement error handling and logging in all services
- [ ] Register services in DI container (`Program.cs`):
  - Register all service interfaces as Scoped
  - Register ReminderBackgroundService as HostedService
- [ ] Add XML documentation comments to all public methods
- [ ] Implement async/await properly throughout

## Acceptance Criteria
- All service interfaces are defined with clear method signatures
- All service implementations follow single responsibility principle
- Services use DbContext directly (no repository pattern)
- Proper exception handling with meaningful error messages
- All database operations are async
- Transactional operations use explicit transactions where needed
- Services are properly registered in DI container
- Background service runs successfully and calls reminder service
- Code follows C# 14 conventions and best practices
- XML documentation exists for all public APIs

## Technical Notes
- Use `ILogger<T>` for logging in all services
- Inject `ApplicationDbContext` in service constructors
- Use transactions for multi-step operations
- Consider using DTOs if needed (but keep simple)
- Background service should use a scoped service provider

## Architecture Principles
- Services own business logic, not just CRUD
- Keep services focused and cohesive
- No generic repository - use DbContext directly
- Explicit transactions when needed
- Simple, readable, maintainable code

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete
- Stage 3 (Entities and Relationships) must be complete

## Estimated Effort
Large - 4-6 hours
