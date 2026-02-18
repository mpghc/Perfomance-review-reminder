# Iteration 9: Testing & Quality Assurance

**Labels:** `iteration`, `testing`, `quality`
**Priority:** Critical
**Estimated Time:** 2-3 sessions
**Depends on:** Iterations 3, 6, 7, 8

## Goal
Implement comprehensive unit tests for service layer and reminder logic with at least 70% code coverage.

## Tasks

### Test Infrastructure
- [ ] Create TestDbContextFactory for in-memory database
- [ ] Create SeedDataHelper for consistent test data
- [ ] Create base test class with common setup/teardown
- [ ] Configure test logging
- [ ] Set up code coverage reporting

### EmployeeService Tests
- [ ] Create EmployeeServiceTests.cs
  - Test GetAllEmployeesAsync() returns all active employees
  - Test GetEmployeeByIdAsync() returns correct employee
  - Test GetEmployeeByIdAsync() returns null for non-existent ID
  - Test CreateEmployeeAsync() creates employee successfully
  - Test CreateEmployeeAsync() throws exception for invalid data
  - Test UpdateEmployeeAsync() updates employee successfully
  - Test DeleteEmployeeAsync() marks employee as inactive
  - Test DeleteEmployeeAsync() returns false for non-existent ID

### DepartmentService Tests
- [ ] Create DepartmentServiceTests.cs
  - Test GetAllDepartmentsAsync() returns all departments
  - Test GetDepartmentByIdAsync() returns correct department
  - Test CreateDepartmentAsync() creates department successfully
  - Test UpdateDepartmentAsync() updates department successfully

### ReviewService Tests
- [ ] Create ReviewServiceTests.cs
  - Test GetAllReviewsAsync() returns all reviews
  - Test GetReviewsByMonthAsync() filters by month correctly
  - Test GetCurrentMonthReviewsAsync() returns only current month
  - Test GetCurrentMonthReviewsAsync() excludes cancelled reviews
  - Test CreateReviewAsync() creates review successfully
  - Test UpdateReviewStatusAsync() updates status correctly
  - Test UpdateReviewStatusAsync() throws exception for invalid status

### FeedbackService Tests
- [ ] Create FeedbackServiceTests.cs
  - Test GetFeedbacksByReviewIdAsync() returns all feedbacks for review
  - Test SubmitFeedbackAsync() creates feedback successfully
  - Test SubmitFeedbackAsync() prevents duplicate submissions
  - Test SubmitFeedbackAsync() throws exception for invalid review
  - Test GetMissingFeedbackProviderIdsAsync() returns correct list
  - Test HasSubmittedFeedbackAsync() returns true when submitted
  - Test HasSubmittedFeedbackAsync() returns false when not submitted

### ReminderService Tests (Critical)
- [ ] Create ReminderServiceTests.cs
  - Test GetCurrentMonthReviewsAsync() with different months
  - Test GetCurrentMonthReviewsAsync() handles year boundaries (Dec-Jan)
  - Test IdentifyMissingFeedbackAsync() with no feedbacks
  - Test IdentifyMissingFeedbackAsync() with some feedbacks
  - Test IdentifyMissingFeedbackAsync() with all feedbacks
  - Test LogReminderAsync() creates reminder log
  - Test ProcessDailyRemindersAsync() processes all current month reviews
  - Test ProcessDailyRemindersAsync() creates correct number of reminders
  - Test ProcessDailyRemindersAsync() is transactional
  - Test ProcessDailyRemindersAsync() with no reviews
  - Test ProcessDailyRemindersAsync() skips completed reviews
  - Test ProcessDailyRemindersAsync() skips cancelled reviews
  - Mock DateTime.Now for month boundary testing

### ReportService Tests
- [ ] Create ReportServiceTests.cs
  - Test GetDashboardSummaryAsync() returns correct counts
  - Test GetMissingFeedbackReportAsync() returns correct items
  - Test GetMissingFeedbackReportAsync() excludes reviews with all feedbacks
  - Test GetReminderLogReportAsync() filters by date correctly
  - Test GetReviewStatusSummaryAsync() calculates percentages correctly

### ReminderBackgroundService Tests (Optional)
- [ ] Create ReminderBackgroundServiceTests.cs
  - Test service starts without errors
  - Test service calls ProcessDailyRemindersAsync
  - Test service handles exceptions gracefully
  - Test service respects cancellation token
  - Mock IServiceProvider and IReminderService

## Test Patterns and Best Practices

### AAA Pattern
```csharp
[Fact]
public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenIdExists()
{
    // Arrange
    var context = CreateInMemoryContext();
    var service = new EmployeeService(context, CreateLogger());
    var employee = new Employee { Name = "John Doe", Email = "john@example.com" };
    context.Employees.Add(employee);
    await context.SaveChangesAsync();

    // Act
    var result = await service.GetEmployeeByIdAsync(employee.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("John Doe", result.Name);
}
```

### Test Naming Convention
```
MethodName_StateUnderTest_ExpectedBehavior

Examples:
- GetEmployeeByIdAsync_ReturnsNull_WhenIdDoesNotExist
- SubmitFeedbackAsync_ThrowsException_WhenReviewDoesNotExist
- ProcessDailyRemindersAsync_CreatesReminders_ForMissingFeedbacks
```

### In-Memory Database Setup
```csharp
private ApplicationDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    var context = new ApplicationDbContext(options);
    context.Database.EnsureCreated();
    return context;
}
```

### Mock DateTime for Testing
```csharp
// Create interface for time provider
public interface ITimeProvider
{
    DateTime Now { get; }
}

// Use in services that need current date/time
public class ReminderService
{
    private readonly ITimeProvider _timeProvider;
    
    public ReminderService(ApplicationDbContext context, ITimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }
    
    public async Task<List<PerformanceReview>> GetCurrentMonthReviewsAsync()
    {
        var now = _timeProvider.Now;
        // Use 'now' instead of DateTime.Now
    }
}

// In tests, mock ITimeProvider
var mockTimeProvider = new Mock<ITimeProvider>();
mockTimeProvider.Setup(x => x.Now).Returns(new DateTime(2024, 6, 15));
```

## Code Coverage Requirements
- [ ] Overall service layer coverage: >= 70%
- [ ] ReminderService coverage: >= 90% (critical component)
- [ ] FeedbackService coverage: >= 80%
- [ ] ReviewService coverage: >= 80%
- [ ] EmployeeService coverage: >= 70%
- [ ] Run code coverage report: `dotnet test --collect:"XPlat Code Coverage"`

## Test Data Helpers

### SeedDataHelper.cs
```csharp
public static class SeedDataHelper
{
    public static Department CreateDepartment(string name = "Engineering")
    {
        return new Department { Name = name };
    }

    public static Employee CreateEmployee(string name = "Test Employee", 
        string email = null, int? departmentId = null)
    {
        return new Employee
        {
            Name = name,
            Email = email ?? $"{name.Replace(" ", "").ToLower()}@example.com",
            DepartmentId = departmentId,
            HireDate = DateTime.Now.AddYears(-1),
            IsActive = true
        };
    }

    public static PerformanceReview CreateReview(int employeeId, int reviewerId,
        DateTime? scheduledDate = null, string status = "Scheduled")
    {
        return new PerformanceReview
        {
            EmployeeId = employeeId,
            ReviewerId = reviewerId,
            ScheduledDate = scheduledDate ?? DateTime.Now.AddDays(7),
            Status = status,
            CreatedAt = DateTime.Now
        };
    }

    public static Feedback CreateFeedback(int reviewId, int providedById,
        string content = "Test feedback content")
    {
        return new Feedback
        {
            ReviewId = reviewId,
            ProvidedById = providedById,
            Content = content,
            SubmittedAt = DateTime.Now
        };
    }
}
```

## Acceptance Criteria
- [ ] All service methods have unit tests
- [ ] All tests pass consistently
- [ ] Tests are isolated (use separate in-memory DB for each test)
- [ ] Tests don't depend on each other (can run in any order)
- [ ] Code coverage meets minimum thresholds
- [ ] Critical reminder logic thoroughly tested
- [ ] Tests follow AAA pattern consistently
- [ ] Test names are descriptive
- [ ] No tests marked as skipped
- [ ] All async methods tested with async tests
- [ ] Edge cases covered (null inputs, empty lists, etc.)

## Testing Commands
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ReminderServiceTests"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML coverage report (requires ReportGenerator)
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Bug Fixes
- [ ] Fix any bugs discovered during testing
- [ ] Add regression tests for fixed bugs
- [ ] Document known issues if any cannot be fixed immediately

## Integration Tests (Optional)
- [ ] Create basic smoke tests
  - Application starts without errors
  - Database migrations apply successfully
  - Services can be resolved from DI container
  - Background service starts without errors

## Acceptance Checklist
- [ ] All unit tests written and passing
- [ ] Code coverage reports generated
- [ ] Coverage thresholds met
- [ ] No failing tests
- [ ] Test names are clear and descriptive
- [ ] Tests are maintainable
- [ ] Edge cases covered
- [ ] Mock objects used appropriately
- [ ] Test data helpers created and used
- [ ] Critical paths have high coverage

## Dependencies
- Iteration 3 must be completed (Services)
- Iteration 6 must be completed (Feedback logic)
- Iteration 7 must be completed (Reminder logic)
- Iteration 8 must be completed (Report service)

## Notes
- Use xUnit's IClassFixture for shared test context if needed
- Consider using FluentAssertions for more readable assertions
- Keep tests simple and focused on one thing
- Don't test framework code (EF Core, ASP.NET)
- Focus on business logic and edge cases
- Human reviews test coverage report before moving to next iteration
