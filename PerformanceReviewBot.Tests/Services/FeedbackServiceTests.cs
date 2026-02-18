using PerformanceReviewBot.Data.Entities;
using PerformanceReviewBot.Services;
using PerformanceReviewBot.Tests.Helpers;
using Xunit;

namespace PerformanceReviewBot.Tests.Services;

public class FeedbackServiceTests : IDisposable
{
    private readonly Data.AppDbContext _context;
    private readonly FeedbackService _service;

    public FeedbackServiceTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        _service = new FeedbackService(_context);
    }

    public void Dispose()
    {
        TestDbContextFactory.DisposeContext(_context);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldAddFeedback()
    {
        // Arrange
        var employee = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var reviewer = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Reviewer", 
            Email = "reviewer@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.AddRange(employee, reviewer);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow,
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        var feedback = new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer.Id,
            Comments = "Great work!",
            Rating = 5,
            IsManagerFeedback = true
        };

        // Act
        var result = await _service.SubmitFeedbackAsync(feedback);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Great work!", result.Comments);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_ShouldPreventDuplicateFeedback()
    {
        // Arrange
        var employee = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var reviewer = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Reviewer", 
            Email = "reviewer@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.AddRange(employee, reviewer);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow,
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        var feedback1 = new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer.Id,
            Comments = "First feedback",
            Rating = 5,
            IsManagerFeedback = true
        };

        await _service.SubmitFeedbackAsync(feedback1);

        var feedback2 = new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer.Id,
            Comments = "Duplicate feedback",
            Rating = 4,
            IsManagerFeedback = true
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SubmitFeedbackAsync(feedback2)
        );
    }

    [Fact]
    public async Task GetFeedbackByReviewIdAsync_ShouldReturnAllFeedbackForReview()
    {
        // Arrange
        var employee = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var reviewer1 = new Employee 
        { 
            FirstName = "Reviewer", 
            LastName = "One", 
            Email = "reviewer1@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        var reviewer2 = new Employee 
        { 
            FirstName = "Reviewer", 
            LastName = "Two", 
            Email = "reviewer2@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        _context.Employees.AddRange(employee, reviewer1, reviewer2);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow,
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        await _service.SubmitFeedbackAsync(new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer1.Id,
            Comments = "Feedback 1",
            Rating = 5,
            IsManagerFeedback = true
        });

        await _service.SubmitFeedbackAsync(new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer2.Id,
            Comments = "Feedback 2",
            Rating = 4,
            IsManagerFeedback = false
        });

        // Act
        var result = await _service.GetFeedbackByReviewIdAsync(review.Id);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HasSubmittedFeedbackAsync_ShouldReturnTrueWhenFeedbackExists()
    {
        // Arrange
        var employee = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var reviewer = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Reviewer", 
            Email = "reviewer@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.AddRange(employee, reviewer);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow,
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        await _service.SubmitFeedbackAsync(new Feedback
        {
            PerformanceReviewId = review.Id,
            ReviewerId = reviewer.Id,
            Comments = "Test feedback",
            Rating = 5,
            IsManagerFeedback = true
        });

        // Act
        var result = await _service.HasSubmittedFeedbackAsync(review.Id, reviewer.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasSubmittedFeedbackAsync_ShouldReturnFalseWhenNoFeedback()
    {
        // Arrange
        var employee = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Employee", 
            Email = "test@test.com", 
            Department = "IT", 
            IsManager = false 
        };
        var reviewer = new Employee 
        { 
            FirstName = "Test", 
            LastName = "Reviewer", 
            Email = "reviewer@test.com", 
            Department = "IT", 
            IsManager = true 
        };
        _context.Employees.AddRange(employee, reviewer);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            EmployeeId = employee.Id,
            ReviewDate = DateTime.UtcNow,
            Status = ReviewStatus.Scheduled
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HasSubmittedFeedbackAsync(review.Id, reviewer.Id);

        // Assert
        Assert.False(result);
    }
}
