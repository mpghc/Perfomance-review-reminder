# Iteration 3: Service Layer Implementation

**Labels:** `iteration`, `services`, `business-logic`
**Priority:** High
**Estimated Time:** 2-3 sessions
**Depends on:** Iteration 2

## Goal
Implement service classes for core business logic with transactional support.

## Tasks

### Employee Service
- [ ] Create IEmployeeService interface
- [ ] Implement EmployeeService with methods:
  - `Task<List<Employee>> GetAllEmployeesAsync()`
  - `Task<Employee?> GetEmployeeByIdAsync(int id)`
  - `Task<Employee> CreateEmployeeAsync(Employee employee)`
  - `Task<Employee> UpdateEmployeeAsync(Employee employee)`
  - `Task<bool> DeleteEmployeeAsync(int id)`

### Department Service
- [ ] Create IDepartmentService interface
- [ ] Implement DepartmentService with methods:
  - `Task<List<Department>> GetAllDepartmentsAsync()`
  - `Task<Department?> GetDepartmentByIdAsync(int id)`
  - `Task<Department> CreateDepartmentAsync(Department department)`
  - `Task<Department> UpdateDepartmentAsync(Department department)`

### Review Service
- [ ] Create IReviewService interface
- [ ] Implement ReviewService with methods:
  - `Task<List<PerformanceReview>> GetAllReviewsAsync()`
  - `Task<PerformanceReview?> GetReviewByIdAsync(int id)`
  - `Task<List<PerformanceReview>> GetReviewsByMonthAsync(int year, int month)`
  - `Task<List<PerformanceReview>> GetCurrentMonthReviewsAsync()`
  - `Task<PerformanceReview> CreateReviewAsync(PerformanceReview review)`
  - `Task<PerformanceReview> UpdateReviewStatusAsync(int id, string status)`

### Feedback Service
- [ ] Create IFeedbackService interface
- [ ] Implement FeedbackService with methods:
  - `Task<List<Feedback>> GetFeedbacksByReviewIdAsync(int reviewId)`
  - `Task<Feedback> SubmitFeedbackAsync(Feedback feedback)`
  - `Task<List<int>> GetMissingFeedbackProviderIdsAsync(int reviewId)`
  - `Task<bool> HasSubmittedFeedbackAsync(int reviewId, int employeeId)`

### Service Registration
- [ ] Register all services in Program.cs with Scoped lifetime
- [ ] Configure dependency injection properly

## Code Quality Requirements
- [ ] All async methods use proper async/await
- [ ] Services use DbContext directly (no repository pattern)
- [ ] Multi-entity updates are wrapped in transactions
- [ ] Proper error handling with try-catch blocks
- [ ] Meaningful exception messages
- [ ] Input validation in service methods
- [ ] Use `ConfigureAwait(false)` where appropriate
- [ ] Follow SOLID principles

## Example Service Pattern
```csharp
public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        try
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.IsActive)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            throw;
        }
    }
    
    // ... other methods
}
```

## Service Registration in Program.cs
```csharp
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
```

## Acceptance Criteria
- [ ] All services implement their interfaces
- [ ] Services use DbContext directly
- [ ] All methods are asynchronous
- [ ] Multi-entity updates use transactions (SaveChangesAsync in single call)
- [ ] Services registered in DI container with Scoped lifetime
- [ ] Error handling implemented consistently
- [ ] Logging added to all service methods
- [ ] Code follows .NET naming conventions
- [ ] No speculative abstractions added
- [ ] Services compile without warnings

## Testing Commands
```bash
# Build to verify compilation
dotnet build

# Check for warnings
dotnet build --no-incremental
```

## Dependencies
- Iteration 2 must be completed

## Notes
- Keep services simple and focused
- Don't add unnecessary abstractions
- Use EF Core's change tracking for updates
- Transaction is implicit when calling SaveChangesAsync once
- Human reviews service interfaces before implementation
