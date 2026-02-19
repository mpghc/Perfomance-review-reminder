using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class ReminderServiceTests
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

    private static ReminderService CreateService(AppDbContext context)
    {
        var notificationService = new NotificationService(context);
        var logger = Substitute.For<ILogger<ReminderService>>();

        return new ReminderService(context, notificationService, logger);
    }

    /// <summary>
    /// Seeds a TM, employee with a teammate, and a review at the given date.
    /// Returns (tm, employee, teammate, review).
    /// </summary>
    private static (Employee tm, Employee employee, Employee teammate, PerformanceReview review) SeedScenario(
        AppDbContext context,
        DateOnly reviewDate,
        ReviewStatus status = ReviewStatus.Scheduled)
    {
        var tm = new Employee { FullName = "Bill", Email = "bill@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(tm);
        context.SaveChanges();

        var employee = new Employee { FullName = "Tom", Email = "tom@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        var teammate = new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        context.Employees.AddRange(employee, teammate);
        context.SaveChanges();

        // Bidirectional teammate relationship
        context.EmployeeTeammates.AddRange(
            new EmployeeTeammate { EmployeeId = employee.Id, TeammateId = teammate.Id },
            new EmployeeTeammate { EmployeeId = teammate.Id, TeammateId = employee.Id });
        context.SaveChanges();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = reviewDate,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        context.PerformanceReviews.Add(review);
        context.SaveChanges();

        return (tm, employee, teammate, review);
    }

    [Fact]
    public async Task ProcessAsync_ReviewIn14DayWindow_CreatesReminderForPendingTeammate()
    {
        // Arrange: review is 10 days from now → inside the 14-day window.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(10);
        var (_, _, teammate, review) = SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: one Reminder notification for Alice
        Assert.Equal(1, created);
        var notifications = await context.Notifications.ToListAsync();
        Assert.Single(notifications);
        Assert.Equal(teammate.Id, notifications[0].RecipientId);
        Assert.Equal(review.Id, notifications[0].ReviewId);
        Assert.Equal(NotificationType.Reminder, notifications[0].Type);
        Assert.Contains("Tom", notifications[0].Message);
    }

    [Fact]
    public async Task ProcessAsync_ReviewOutsideWindow_CreatesNoNotifications()
    {
        // Arrange: review is 20 days from now → outside the 14-day window.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(20);
        SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: no notifications
        Assert.Equal(0, created);
        Assert.Empty(await context.Notifications.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_TeammateAlreadySubmitted_NoReminderForThem()
    {
        // Arrange: review in window, but Alice already submitted feedback.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(10);
        var (_, _, teammate, review) = SeedScenario(context, reviewDate);

        // Alice already submitted feedback
        context.Feedbacks.Add(new Feedback
        {
            ReviewId = review.Id,
            AuthorId = teammate.Id,
            Content = "Great work!",
            SubmittedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: no notifications since feedback already submitted
        Assert.Equal(0, created);
        Assert.Empty(await context.Notifications.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_ReviewWithin3Days_CreatesOverdueForTM()
    {
        // Arrange: review is 2 days from now → within 3-day overdue threshold.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(2);
        var (tm, _, teammate, review) = SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: Reminder for Alice + Overdue for Bill = 2 notifications
        Assert.Equal(2, created);

        var notifications = await context.Notifications.OrderBy(n => n.Type).ToListAsync();
        Assert.Equal(2, notifications.Count);

        var overdue = notifications.First(n => n.Type == NotificationType.Overdue);
        Assert.Equal(tm.Id, overdue.RecipientId);
        Assert.Equal(review.Id, overdue.ReviewId);
        Assert.Contains("Overdue", overdue.Message);

        var reminder = notifications.First(n => n.Type == NotificationType.Reminder);
        Assert.Equal(teammate.Id, reminder.RecipientId);
    }

    [Fact]
    public async Task ProcessAsync_RunTwiceSameDay_DeduplicatesNotifications()
    {
        // Arrange: review in window.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(10);
        SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act: run twice on the same day
        var firstRun = await service.ProcessAsync(now);
        var secondRun = await service.ProcessAsync(now);

        // Assert: first run creates 1, second run creates 0 (deduplication)
        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Equal(1, await context.Notifications.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_CompletedReview_CreatesNoNotifications()
    {
        // Arrange: review is in window but status is Completed.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(5);
        SeedScenario(context, reviewDate, ReviewStatus.Completed);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: no notifications for completed reviews
        Assert.Equal(0, created);
        Assert.Empty(await context.Notifications.ToListAsync());
    }

    [Fact]
    public async Task ProcessAsync_InProgressReview_StillSendsReminders()
    {
        // Arrange: review in window with InProgress status — reminders should still be sent.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(10);
        SeedScenario(context, reviewDate, ReviewStatus.InProgress);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: Reminder notification created
        Assert.Equal(1, created);
    }

    [Fact]
    public async Task ProcessAsync_MultipleTeammates_CreatesReminderForEachPending()
    {
        // Arrange: Tom has two teammates (Alice and Bob), neither has submitted feedback.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(10);
        var (tm, employee, alice, review) = SeedScenario(context, reviewDate);

        // Add a second teammate (Bob)
        var bob = new Employee { FullName = "Bob", Email = "bob@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        context.Employees.Add(bob);
        context.SaveChanges();

        context.EmployeeTeammates.AddRange(
            new EmployeeTeammate { EmployeeId = employee.Id, TeammateId = bob.Id },
            new EmployeeTeammate { EmployeeId = bob.Id, TeammateId = employee.Id });
        context.SaveChanges();

        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: one Reminder each for Alice and Bob = 2
        Assert.Equal(2, created);
        var recipientIds = (await context.Notifications.ToListAsync())
            .Select(n => n.RecipientId)
            .OrderBy(id => id)
            .ToList();
        Assert.Contains(alice.Id, recipientIds);
        Assert.Contains(bob.Id, recipientIds);
    }

    [Fact]
    public async Task ProcessAsync_OverdueDeduplication_SecondRunSkipsOverdue()
    {
        // Arrange: review in 3-day overdue window.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now).AddDays(2);
        SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act
        var firstRun = await service.ProcessAsync(now);
        var secondRun = await service.ProcessAsync(now);

        // Assert: first run = 2 (Reminder + Overdue), second run = 0 (deduplicated)
        Assert.Equal(2, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Equal(2, await context.Notifications.CountAsync());
    }

    [Fact]
    public async Task ProcessAsync_ReviewDateIsToday_StillInWindow()
    {
        // Arrange: review date is today — should still be in window and overdue.
        using var context = CreateInMemoryContext();
        var now = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);
        var reviewDate = DateOnly.FromDateTime(now); // today
        SeedScenario(context, reviewDate);
        var service = CreateService(context);

        // Act
        var created = await service.ProcessAsync(now);

        // Assert: Reminder for teammate + Overdue for TM
        Assert.Equal(2, created);
        Assert.True(await context.Notifications.AnyAsync(n => n.Type == NotificationType.Overdue));
        Assert.True(await context.Notifications.AnyAsync(n => n.Type == NotificationType.Reminder));
    }
}
