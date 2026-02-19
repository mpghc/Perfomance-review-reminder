using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class ReviewServiceTests
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

    private static (Employee tm, Employee empA, Employee empB) SeedManagerAndEmployees(AppDbContext context)
    {
        var tm = new Employee { FullName = "Boss", Email = "boss@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(tm);
        context.SaveChanges();

        var empA = new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        var empB = new Employee { FullName = "Bob", Email = "bob@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        context.Employees.AddRange(empA, empB);
        context.SaveChanges();

        return (tm, empA, empB);
    }

    [Fact]
    public async Task ScheduleAsync_ValidFutureDate_CreatesReview()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedManagerAndEmployees(context);
        var service = new ReviewService(context);
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var review = await service.ScheduleAsync(empA.Id, futureDate);

        Assert.True(review.Id > 0);
        Assert.Equal(ReviewStatus.Scheduled, review.Status);
        Assert.Equal(futureDate, review.ReviewDate);
        Assert.Equal(empA.Id, review.EmployeeId);
    }

    [Fact]
    public async Task ScheduleAsync_PastDate_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedManagerAndEmployees(context);
        var service = new ReviewService(context);
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ScheduleAsync(empA.Id, pastDate));

        Assert.Contains("future", ex.Message);
    }

    [Fact]
    public async Task ScheduleAsync_TodayDate_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedManagerAndEmployees(context);
        var service = new ReviewService(context);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ScheduleAsync(empA.Id, today));

        Assert.Contains("future", ex.Message);
    }

    [Fact]
    public async Task ScheduleAsync_NonExistentEmployee_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new ReviewService(context);
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ScheduleAsync(999, futureDate));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task ScheduleAsync_EmployeeWithoutTM_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var orphan = new Employee { FullName = "Orphan", Email = "orphan@test.com", Role = EmployeeRole.Employee };
        context.Employees.Add(orphan);
        context.SaveChanges();
        var service = new ReviewService(context);
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ScheduleAsync(orphan.Id, futureDate));

        Assert.Contains("Talent Manager", ex.Message);
    }

    [Fact]
    public async Task GetByManagerAsync_ReturnsOnlyManagedEmployeeReviews()
    {
        using var context = CreateInMemoryContext();
        var (tm, empA, empB) = SeedManagerAndEmployees(context);
        var otherTm = new Employee { FullName = "OtherBoss", Email = "other@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(otherTm);
        context.SaveChanges();
        var otherEmp = new Employee { FullName = "Charlie", Email = "charlie@test.com", Role = EmployeeRole.Employee, TalentManagerId = otherTm.Id };
        context.Employees.Add(otherEmp);
        context.SaveChanges();

        var service = new ReviewService(context);
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        await service.ScheduleAsync(empA.Id, futureDate);
        await service.ScheduleAsync(otherEmp.Id, futureDate);

        var reviews = await service.GetByManagerAsync(tm.Id);

        Assert.Single(reviews);
        Assert.Equal(empA.Id, reviews[0].EmployeeId);
    }

    [Fact]
    public async Task GetByManagerAsync_NoEmployees_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var lonelyTm = new Employee { FullName = "Lonely", Email = "lonely@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(lonelyTm);
        context.SaveChanges();
        var service = new ReviewService(context);

        var reviews = await service.GetByManagerAsync(lonelyTm.Id);

        Assert.Empty(reviews);
    }

    [Fact]
    public async Task UpdateStatusAsync_ChangesStatusCorrectly()
    {
        using var context = CreateInMemoryContext();
        var (_, empA, _) = SeedManagerAndEmployees(context);
        var service = new ReviewService(context);
        var review = await service.ScheduleAsync(empA.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(7)));

        await service.UpdateStatusAsync(review.Id, ReviewStatus.InProgress);

        var updated = await context.PerformanceReviews.FindAsync(review.Id);
        Assert.Equal(ReviewStatus.InProgress, updated!.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistentReview_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var service = new ReviewService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateStatusAsync(999, ReviewStatus.InProgress));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GetManagedEmployeesAsync_ReturnsOnlyManagedEmployees()
    {
        using var context = CreateInMemoryContext();
        var (tm, empA, empB) = SeedManagerAndEmployees(context);
        var service = new ReviewService(context);

        var managed = await service.GetManagedEmployeesAsync(tm.Id);

        Assert.Equal(2, managed.Count);
        Assert.Contains(managed, e => e.Id == empA.Id);
        Assert.Contains(managed, e => e.Id == empB.Id);
    }
}
