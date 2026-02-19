using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;
using PerformanceReviewReminderBot.Web.Services;

namespace PerformanceReviewReminderBot.Web.Tests;

public class FeedbackServiceTests
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
    /// Seeds a TM, two employees (Alice teammate of Bob), and a Scheduled review for Bob.
    /// Returns (tm, alice, bob, review).
    /// </summary>
    private static (Employee tm, Employee alice, Employee bob, PerformanceReview review) SeedReviewScenario(AppDbContext context)
    {
        var tm = new Employee { FullName = "Boss", Email = "boss@test.com", Role = EmployeeRole.TalentManager };
        context.Employees.Add(tm);
        context.SaveChanges();

        var alice = new Employee { FullName = "Alice", Email = "alice@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        var bob = new Employee { FullName = "Bob", Email = "bob@test.com", Role = EmployeeRole.Employee, TalentManagerId = tm.Id };
        context.Employees.AddRange(alice, bob);
        context.SaveChanges();

        // Alice is a teammate of Bob (bidirectional)
        context.EmployeeTeammates.AddRange(
            new EmployeeTeammate { EmployeeId = bob.Id, TeammateId = alice.Id },
            new EmployeeTeammate { EmployeeId = alice.Id, TeammateId = bob.Id });
        context.SaveChanges();

        var review = new PerformanceReview
        {
            EmployeeId = bob.Id,
            ReviewDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            Status = ReviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };
        context.PerformanceReviews.Add(review);
        context.SaveChanges();

        return (tm, alice, bob, review);
    }

    [Fact]
    public async Task SubmitAsync_ValidTeammate_CreatesFeedback()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);

        var feedback = await service.SubmitAsync(review.Id, alice.Id, "Great work!");

        Assert.True(feedback.Id > 0);
        Assert.Equal(review.Id, feedback.ReviewId);
        Assert.Equal(alice.Id, feedback.AuthorId);
        Assert.Equal("Great work!", feedback.Content);
    }

    [Fact]
    public async Task SubmitAsync_Duplicate_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);
        await service.SubmitAsync(review.Id, alice.Id, "First feedback");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(review.Id, alice.Id, "Second attempt"));

        Assert.Contains("already submitted", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_EmptyContent_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(review.Id, alice.Id, ""));

        Assert.Contains("required", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_NonTeammate_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (tm, _, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);

        // TM is not a teammate of Bob
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(review.Id, tm.Id, "Feedback from TM"));

        Assert.Contains("not a teammate", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_CompletedReview_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        review.Status = ReviewStatus.Completed;
        context.SaveChanges();
        var service = new FeedbackService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(review.Id, alice.Id, "Late feedback"));

        Assert.Contains("completed", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_NonExistentReview_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        SeedReviewScenario(context);
        var service = new FeedbackService(context);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(999, 1, "Feedback"));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task GetPendingForUserAsync_ReturnsPendingReviews()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);

        var pending = await service.GetPendingForUserAsync(alice.Id);

        Assert.Single(pending);
        Assert.Equal(review.Id, pending[0].Id);
    }

    [Fact]
    public async Task GetPendingForUserAsync_ExcludesAlreadySubmitted()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);
        await service.SubmitAsync(review.Id, alice.Id, "Done!");

        var pending = await service.GetPendingForUserAsync(alice.Id);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task GetPendingForUserAsync_ExcludesCompletedReviews()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        review.Status = ReviewStatus.Completed;
        context.SaveChanges();
        var service = new FeedbackService(context);

        var pending = await service.GetPendingForUserAsync(alice.Id);

        Assert.Empty(pending);
    }

    [Fact]
    public async Task SubmitAsync_ContentExceedsMaxLength_ThrowsInvalidOperation()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);
        var oversizedContent = new string('x', 4001);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(review.Id, alice.Id, oversizedContent));

        Assert.Contains("4000", ex.Message);
    }

    [Fact]
    public async Task GetByReviewAsync_ReturnsFeedbackWithAuthor()
    {
        using var context = CreateInMemoryContext();
        var (_, alice, _, review) = SeedReviewScenario(context);
        var service = new FeedbackService(context);
        await service.SubmitAsync(review.Id, alice.Id, "Nice job");

        var feedbacks = await service.GetByReviewAsync(review.Id);

        Assert.Single(feedbacks);
        Assert.Equal("Alice", feedbacks[0].Author.FullName);
        Assert.Equal("Nice job", feedbacks[0].Content);
    }
}
