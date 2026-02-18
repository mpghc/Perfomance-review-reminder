using Microsoft.Extensions.Logging;
using Moq;
using PerformanceReviewBot.Data.Entities;
using PerformanceReviewBot.Services;
using PerformanceReviewBot.Tests.Helpers;
using Xunit;

namespace PerformanceReviewBot.Tests.Services;

public class ReminderServiceTests : IDisposable
{
    private readonly Data.AppDbContext _context;
    private readonly ReminderService _service;
    private readonly Mock<ILogger<ReminderService>> _mockLogger;

    public ReminderServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _mockLogger = new Mock<ILogger<ReminderService>>();
        _service = new ReminderService(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        TestDbContextFactory.DisposeContext(_context);
    }

    [Fact]
    public async Task ProcessCurrentMonthRemindersAsync_ShouldCreateReminderLogs()
    {
        // Arrange
        var manager = new Employee 
        { 
            FirstName = "Manager", 
            LastName = "Test", 
            Email = "manager@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.Add(manager);
        await _context.SaveChangesAsync();

        var employee = new Employee 
        { 
            FirstName = "Employee", 
            LastName = "Test", 
            Email = "employee@test.com", 
            Department = "IT", 
            IsManager = false,
            ManagerId = manager.Id
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow.AddDays(5), // Within 7 days
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        await _service.ProcessCurrentMonthRemindersAsync();

        // Assert
        var logs = await _service.GetReminderLogsAsync();
        Assert.NotEmpty(logs);
        Assert.Contains(logs, l => l.ReminderType == ReminderType.ReviewDue);
    }

    [Fact]
    public async Task ProcessCurrentMonthRemindersAsync_ShouldNotDuplicateReminders()
    {
        // Arrange
        var manager = new Employee 
        { 
            FirstName = "Manager", 
            LastName = "Test", 
            Email = "manager@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.Add(manager);
        await _context.SaveChangesAsync();

        var employee = new Employee 
        { 
            FirstName = "Employee", 
            LastName = "Test", 
            Email = "employee@test.com", 
            Department = "IT", 
            IsManager = false,
            ManagerId = manager.Id
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow.AddDays(5),
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Act - Process twice
        await _service.ProcessCurrentMonthRemindersAsync();
        var logsAfterFirst = await _service.GetReminderLogsAsync();
        var countAfterFirst = logsAfterFirst.Count;

        await _service.ProcessCurrentMonthRemindersAsync();
        var logsAfterSecond = await _service.GetReminderLogsAsync();
        var countAfterSecond = logsAfterSecond.Count;

        // Assert - Should not create duplicate reminders on the same day
        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public async Task ProcessCurrentMonthRemindersAsync_ShouldDetectMissingManagerFeedback()
    {
        // Arrange
        var manager = new Employee 
        { 
            FirstName = "Manager", 
            LastName = "Test", 
            Email = "manager@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.Add(manager);
        await _context.SaveChangesAsync();

        var employee = new Employee 
        { 
            FirstName = "Employee", 
            LastName = "Test", 
            Email = "employee@test.com", 
            Department = "IT", 
            IsManager = false,
            ManagerId = manager.Id
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow.AddDays(-1), // Past due
            Status = ReviewStatus.InProgress
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        await _service.ProcessCurrentMonthRemindersAsync();

        // Assert
        var logs = await _service.GetReminderLogsAsync();
        Assert.Contains(logs, l => l.ReminderType == ReminderType.FeedbackMissing);
    }

    [Fact]
    public async Task GetReminderLogsByEmployeeAsync_ShouldReturnOnlyEmployeeLogs()
    {
        // Arrange
        var employee1 = new Employee 
        { 
            FirstName = "Employee1", 
            LastName = "Test", 
            Email = "emp1@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var employee2 = new Employee 
        { 
            FirstName = "Employee2", 
            LastName = "Test", 
            Email = "emp2@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        _context.Employees.AddRange(employee1, employee2);
        await _context.SaveChangesAsync();

        var review1 = new PerformanceReview 
        { 
            EmployeeId = employee1.Id, 
            ReviewDate = DateTime.UtcNow, 
            Status = ReviewStatus.Scheduled 
        };
        var review2 = new PerformanceReview 
        { 
            EmployeeId = employee2.Id, 
            ReviewDate = DateTime.UtcNow, 
            Status = ReviewStatus.Scheduled 
        };
        _context.PerformanceReviews.AddRange(review1, review2);
        await _context.SaveChangesAsync();

        var log1 = new ReminderLog 
        { 
            EmployeeId = employee1.Id, 
            PerformanceReviewId = review1.Id, 
            ReminderType = ReminderType.ReviewDue, 
            Message = "Test message 1" 
        };
        var log2 = new ReminderLog 
        { 
            EmployeeId = employee2.Id, 
            PerformanceReviewId = review2.Id, 
            ReminderType = ReminderType.ReviewDue, 
            Message = "Test message 2" 
        };
        _context.ReminderLogs.AddRange(log1, log2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetReminderLogsByEmployeeAsync(employee1.Id);

        // Assert
        Assert.Single(result);
        Assert.All(result, l => Assert.Equal(employee1.Id, l.EmployeeId));
    }
}
