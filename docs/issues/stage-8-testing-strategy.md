# Stage 8: Testing Strategy

## Objective
Implement comprehensive unit tests for the service layer to ensure correctness, reliability, and maintainability of business logic.

## Description
Create a thorough test suite using xUnit to validate all service operations, including CRUD operations, business rule enforcement, reminder logic, and transactional behavior. Use SQLite in-memory database for integration-style tests that verify service behavior with a real database context.

## Testing Approach

### Test Infrastructure Setup
- Use SQLite in-memory database for testing
- Create test fixtures or helper methods for DbContext setup
- Use `DbContextOptions` with unique database names per test
- Implement proper test cleanup (dispose contexts)
- Use Moq for mocking external dependencies (if needed)

### Example Test Setup:
```csharp
private ApplicationDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite("DataSource=:memory:")
        .Options;
    
    var context = new ApplicationDbContext(options);
    context.Database.OpenConnection(); // Required for in-memory SQLite
    context.Database.EnsureCreated();
    
    return context;
}
```

## Service Tests to Implement

### 1. EmployeeService Tests
**Test Class:** `EmployeeServiceTests.cs`

**Tests:**
- `GetAllAsync_ReturnsAllEmployees`
- `GetAllAsync_WithIncludeInactive_ReturnsInactiveEmployees`
- `GetAllAsync_WithoutIncludeInactive_ExcludesInactiveEmployees`
- `GetByIdAsync_WithValidId_ReturnsEmployee`
- `GetByIdAsync_WithInvalidId_ReturnsNull`
- `GetByEmailAsync_WithValidEmail_ReturnsEmployee`
- `GetByEmailAsync_WithInvalidEmail_ReturnsNull`
- `CreateAsync_WithValidEmployee_CreatesEmployee`
- `CreateAsync_WithDuplicateEmail_ThrowsException`
- `CreateAsync_WithInvalidData_ThrowsException`
- `UpdateAsync_WithValidEmployee_UpdatesEmployee`
- `UpdateAsync_WithDuplicateEmail_ThrowsException`
- `DeleteAsync_WithValidId_DeletesEmployee`
- `DeleteAsync_WithActiveReviews_ThrowsOrHandlesAppropriately`
- `EmailExistsAsync_WithExistingEmail_ReturnsTrue`
- `EmailExistsAsync_WithNonExistingEmail_ReturnsFalse`

### 2. PerformanceReviewService Tests
**Test Class:** `PerformanceReviewServiceTests.cs`

**Tests:**
- `GetAllAsync_ReturnsAllReviews`
- `GetByIdAsync_WithValidId_ReturnsReview`
- `GetReviewsForCurrentMonthAsync_ReturnsOnlyCurrentMonthReviews`
- `GetReviewsByStatusAsync_ReturnsReviewsWithSpecificStatus`
- `GetReviewsByEmployeeAsync_ReturnsEmployeeReviews`
- `CreateAsync_WithValidReview_CreatesReview`
- `CreateAsync_WithSelfReview_ThrowsException`
- `CreateAsync_WithInvalidDate_ThrowsException`
- `UpdateAsync_WithValidReview_UpdatesReview`
- `UpdateStatusAsync_WithValidStatus_UpdatesStatus`
- `UpdateStatusAsync_WithInvalidTransition_ThrowsException` (if applicable)
- `DeleteAsync_WithValidId_DeletesReview`

### 3. FeedbackService Tests
**Test Class:** `FeedbackServiceTests.cs`

**Tests:**
- `GetAllAsync_ReturnsAllFeedback`
- `GetByReviewIdAsync_ReturnsFeedbackForSpecificReview`
- `GetByIdAsync_WithValidId_ReturnsFeedback`
- `SubmitFeedbackAsync_WithValidFeedback_CreatesFeedback`
- `SubmitFeedbackAsync_WithDuplicateProvider_ThrowsException`
- `SubmitFeedbackAsync_WithInvalidRating_ThrowsException`
- `SubmitFeedbackAsync_ForCompletedReview_ThrowsException`
- `GetReviewsWithMissingFeedbackAsync_ReturnsReviewsWithoutAllFeedback`
- `HasProviderSubmittedFeedbackAsync_WithExistingFeedback_ReturnsTrue`
- `HasProviderSubmittedFeedbackAsync_WithoutFeedback_ReturnsFalse`

### 4. ReminderService Tests
**Test Class:** `ReminderServiceTests.cs`

**Tests:**
- `GetReviewsNeedingRemindersAsync_ReturnsCurrentMonthReviews`
- `GetReviewsNeedingRemindersAsync_ExcludesCompletedReviews`
- `GetReviewsNeedingRemindersAsync_ExcludesCancelledReviews`
- `SendRemindersAsync_WithMissingFeedback_CreatesReminderLogs`
- `SendRemindersAsync_WithNoMissingFeedback_CreatesNoLogs`
- `SendRemindersAsync_WithRecentReminder_AvoidseDuplicates`
- `SendRemindersAsync_WithTransactionFailure_RollsBackChanges`
- `SendRemindersAsync_WithMultipleReviews_CreatesMultipleLogs`
- `LogReminderAsync_WithValidReminder_CreatesLog`
- `GetReminderLogsByReviewAsync_ReturnsLogsForSpecificReview`
- `GetRecentRemindersAsync_ReturnsRemindersWithinDays`

### 5. Background Service Tests
**Test Class:** `ReminderBackgroundServiceTests.cs`

**Tests:**
- `ExecuteAsync_CallsReminderService`
- `ExecuteAsync_HandlesExceptionsGracefully`
- `ExecuteAsync_RespectsancellationToken`
- `ExecuteAsync_CreatesServiceScope`
- `ExecuteAsync_LogsExecution`

## Test Organization

### File Structure:
```
PerformanceReviewReminder.Tests/
├── Services/
│   ├── EmployeeServiceTests.cs
│   ├── PerformanceReviewServiceTests.cs
│   ├── FeedbackServiceTests.cs
│   ├── ReminderServiceTests.cs
│   └── ReminderBackgroundServiceTests.cs
├── Helpers/
│   ├── TestDbContextFactory.cs
│   └── TestDataBuilder.cs
└── Fixtures/
    └── DatabaseFixture.cs (if using shared fixtures)
```

### Test Pattern (Arrange-Act-Assert):
```csharp
[Fact]
public async Task CreateAsync_WithValidEmployee_CreatesEmployee()
{
    // Arrange
    using var context = CreateInMemoryContext();
    var service = new EmployeeService(context, Mock.Of<ILogger<EmployeeService>>());
    var employee = new Employee
    {
        Name = "John Doe",
        Email = "john@example.com",
        Department = "IT",
        Role = "Developer",
        IsActive = true
    };
    
    // Act
    var result = await service.CreateAsync(employee);
    
    // Assert
    Assert.NotNull(result);
    Assert.True(result.Id > 0);
    Assert.Equal("John Doe", result.Name);
    
    // Verify in database
    var savedEmployee = await context.Employees.FindAsync(result.Id);
    Assert.NotNull(savedEmployee);
}
```

## Tasks
- [ ] Set up test project structure and folders
- [ ] Create test helper classes:
  - `TestDbContextFactory` for creating in-memory contexts
  - `TestDataBuilder` for generating test data
- [ ] Write all EmployeeService tests (14 tests)
- [ ] Write all PerformanceReviewService tests (12 tests)
- [ ] Write all FeedbackService tests (10 tests)
- [ ] Write all ReminderService tests (11 tests)
- [ ] Write Background Service tests (5 tests)
- [ ] Test transactional behavior explicitly
  - Test successful transaction commit
  - Test transaction rollback on exception
- [ ] Add tests for edge cases:
  - Empty database
  - Null parameters
  - Concurrent operations (if applicable)
- [ ] Ensure all tests are independent (no shared state)
- [ ] Implement proper cleanup (dispose contexts)
- [ ] Run all tests and ensure they pass
- [ ] Measure code coverage (aim for >80% service layer coverage)
- [ ] Add XML documentation to test classes explaining what they test

## Acceptance Criteria
- All service methods have corresponding unit tests
- Tests use in-memory SQLite database
- Tests are independent and can run in any order
- Tests follow Arrange-Act-Assert pattern
- Test names clearly describe what they test
- All tests pass successfully
- Code coverage for service layer is >80%
- Transactional behavior is validated
- Edge cases are covered
- Tests run quickly (< 30 seconds for full suite)
- No test data pollution (proper cleanup)
- Mocking is used appropriately (only for external dependencies)

## Technical Notes
- Use `[Fact]` for simple tests, `[Theory]` with `[InlineData]` for parameterized tests
- Dispose contexts properly (use `using` statements)
- Use `Assert.Throws` or `Assert.ThrowsAsync` for exception testing
- Use `Mock.Of<T>()` for simple mocks, `new Mock<T>()` for complex setups
- Consider using `FluentAssertions` for more readable assertions (optional)
- Group related tests in the same file
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`

## Testing Best Practices
- **Isolated:** Each test should be independent
- **Fast:** Tests should run quickly
- **Repeatable:** Tests should produce same results every time
- **Self-validating:** Tests should have clear pass/fail outcome
- **Timely:** Write tests as you write code (or shortly after)

## Dependencies
- Stage 1 (Solution Structure) must be complete
- Stage 2 (Folder Organization) must be complete
- Stage 3 (Entities and Relationships) must be complete
- Stage 4 (Services Definition) must be complete
- Stage 7 (Reminder Flow) must be complete

## Estimated Effort
Medium-Large - 4-6 hours

## Notes
- Integration tests for pages/components are out of scope (unit tests for services only)
- Consider adding integration tests later if needed
- E2E tests with Blazor testing library are optional for future enhancement
