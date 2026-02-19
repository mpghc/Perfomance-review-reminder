using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Provides operations for submitting and querying peer feedback on performance reviews.
/// Uses AppDbContext directly (no repository abstraction per project rules).
/// </summary>
public class FeedbackService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Returns reviews where the specified employee is a teammate of the reviewee,
    /// the review is Scheduled or InProgress, and the employee hasn't submitted feedback yet.
    /// </summary>
    /// <param name="employeeId">The Id of the employee whose pending reviews to retrieve.</param>
    /// <example>
    /// <code>
    /// var pending = await feedbackService.GetPendingForUserAsync(currentUser.Id);
    /// </code>
    /// </example>
    public async Task<List<PerformanceReview>> GetPendingForUserAsync(int employeeId)
    {
        // Find reviews where:
        // 1. The employee is a teammate of the reviewee (EmployeeTeammate row exists).
        // 2. The review is not Completed.
        // 3. The employee has not already submitted feedback for this review.
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Where(r => r.Status != ReviewStatus.Completed)
            .Where(r => _context.EmployeeTeammates
                .Any(et => et.EmployeeId == r.EmployeeId && et.TeammateId == employeeId))
            .Where(r => !r.Feedbacks.Any(f => f.AuthorId == employeeId))
            .OrderBy(r => r.ReviewDate)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Returns all feedback for the specified review, including author information.
    /// </summary>
    /// <param name="reviewId">The Id of the review.</param>
    public async Task<List<Feedback>> GetByReviewAsync(int reviewId)
    {
        return await _context.Feedbacks
            .Include(f => f.Author)
            .Where(f => f.ReviewId == reviewId)
            .OrderBy(f => f.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Maximum allowed length (in characters) for feedback content.
    /// Prevents excessively large payloads from being stored.
    /// </summary>
    private const int MaxContentLength = 4000;

    /// <summary>
    /// Submits peer feedback for a performance review.
    /// </summary>
    /// <param name="reviewId">The Id of the review.</param>
    /// <param name="authorId">The Id of the teammate submitting feedback.</param>
    /// <param name="content">The feedback text content.</param>
    /// <returns>The created <see cref="Feedback"/> entity.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when: content is empty or exceeds 4000 characters, review does not exist, review is Completed,
    /// author is not a teammate of the reviewee, or duplicate feedback exists.
    /// </exception>
    public async Task<Feedback> SubmitAsync(int reviewId, int authorId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Feedback content is required.");
        }

        if (content.Length > MaxContentLength)
        {
            throw new InvalidOperationException($"Feedback content must not exceed {MaxContentLength} characters.");
        }

        var review = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null)
        {
            throw new InvalidOperationException($"Review with Id {reviewId} not found.");
        }

        if (review.Status == ReviewStatus.Completed)
        {
            throw new InvalidOperationException("Cannot submit feedback for a completed review.");
        }

        var isTeammate = await _context.EmployeeTeammates
            .AnyAsync(et => et.EmployeeId == review.EmployeeId && et.TeammateId == authorId);

        if (!isTeammate)
        {
            throw new InvalidOperationException(
                "You are not a teammate of the employee being reviewed.");
        }

        var alreadySubmitted = await _context.Feedbacks
            .AnyAsync(f => f.ReviewId == reviewId && f.AuthorId == authorId);

        if (alreadySubmitted)
        {
            throw new InvalidOperationException(
                "You have already submitted feedback for this review.");
        }

        var feedback = new Feedback
        {
            ReviewId = reviewId,
            AuthorId = authorId,
            Content = content,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        return feedback;
    }
}
