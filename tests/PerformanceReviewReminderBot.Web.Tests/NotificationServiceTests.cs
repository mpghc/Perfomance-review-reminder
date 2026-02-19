using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class NotificationServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        return context;
    }

    /// <summary>
    /// Seeds a TM and an employee for notification tests.
    /// Returns (tm, employee).
    /// </summary>
    private static (Employee tm, Employee employee) SeedEmployees(AppDbContext context)
    {
        var tm = new Employee { FullName = "Boss", Email = "boss@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(tm);
        context.SaveChanges();

        var employee = new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        context.Employees.Add(employee);
        context.SaveChanges();

        return (tm, employee);
    }

    [Fact]
    public async Task CreateAsync_ValidData_CreatesNotification()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        var notification = await service.CreateAsync(
            employee.Id, null, NotificationType.Reminder, "Please submit feedback.");

        Assert.True(notification.Id > 0);
        Assert.Equal(employee.Id, notification.RecipientId);
        Assert.Equal(NotificationType.Reminder, notification.Type);
        Assert.Equal("Please submit feedback.", notification.Message);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public async Task CreateAsync_EmptyMessage_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(employee.Id, null, NotificationType.Reminder, ""));

        Assert.Contains("message is required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_NonExistentRecipient_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new NotificationService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(999, null, NotificationType.Reminder, "Hello"));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkAsReadAsync_ExistingNotification_SetsIsReadTrue()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        var notification = await service.CreateAsync(
            employee.Id, null, NotificationType.Reminder, "Test");

        Assert.False(notification.IsRead);

        await service.MarkAsReadAsync(notification.Id);

        var updated = await context.Notifications.FindAsync(notification.Id);
        Assert.NotNull(updated);
        Assert.True(updated.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistentId_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new NotificationService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.MarkAsReadAsync(999));
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        await service.CreateAsync(employee.Id, null, NotificationType.Reminder, "One");
        await service.CreateAsync(employee.Id, null, NotificationType.Overdue, "Two");
        await service.CreateAsync(employee.Id, null, NotificationType.Reminder, "Three");

        Assert.Equal(3, await service.GetUnreadCountAsync(employee.Id));

        // Mark one as read — count should decrease
        var all = await context.Notifications.ToListAsync();
        await service.MarkAsReadAsync(all[0].Id);

        Assert.Equal(2, await service.GetUnreadCountAsync(employee.Id));
    }

    [Fact]
    public async Task GetByRecipientAsync_ReturnsOnlyRecipientNotifications_OrderedByDateDesc()
    {
        using var context = CreateInMemoryContext();
        var (tm, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        // Create notifications for employee and for TM
        await service.CreateAsync(employee.Id, null, NotificationType.Reminder, "For Alice 1");
        await Task.Delay(50); // Ensure different timestamps
        await service.CreateAsync(employee.Id, null, NotificationType.Overdue, "For Alice 2");
        await service.CreateAsync(tm.Id, null, NotificationType.Reminder, "For Boss");

        var aliceNotifications = await service.GetByRecipientAsync(employee.Id);

        Assert.Equal(2, aliceNotifications.Count);
        Assert.All(aliceNotifications, n => Assert.Equal(employee.Id, n.RecipientId));

        // Should be ordered by CreatedAt descending (newest first)
        Assert.True(aliceNotifications[0].CreatedAt >= aliceNotifications[1].CreatedAt);
        Assert.Equal("For Alice 2", aliceNotifications[0].Message);
    }

    [Fact]
    public async Task CreateAsync_WithReviewId_SetsReviewFK()
    {
        using var context = CreateInMemoryContext();
        var (tm, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Status = ReviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        context.PerformanceReviews.Add(review);
        await context.SaveChangesAsync();

        var notification = await service.CreateAsync(
            employee.Id, review.Id, NotificationType.Reminder, "Review upcoming");

        Assert.Equal(review.Id, notification.ReviewId);
    }

    [Fact]
    public async Task GetUnreadCountAsync_NoNotifications_ReturnsZero()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        Assert.Equal(0, await service.GetUnreadCountAsync(employee.Id));
    }

    [Fact]
    public async Task GetByRecipientAsync_NoNotifications_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var (_, employee) = SeedEmployees(context);
        var service = new NotificationService(context);

        var result = await service.GetByRecipientAsync(employee.Id);

        Assert.Empty(result);
    }
}
