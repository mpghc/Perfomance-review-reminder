using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Provides operations for scheduling and managing performance reviews.
/// Uses AppDbContext directly (no repository abstraction per project rules).
/// </summary>
public class ReviewService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Returns all reviews for employees managed by the specified Talent Manager.
    /// Includes Employee navigation and Feedbacks for progress tracking.
    /// </summary>
    /// <param name="managerId">The Id of the Talent Manager.</param>
    /// <example>
    /// <code>
    /// var reviews = await reviewService.GetByManagerAsync(currentUser.Id);
    /// </code>
    /// </example>
    public async Task<List<PerformanceReview>> GetByManagerAsync(int managerId)
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e.EmployeeTeammates)
            .Include(r => r.Feedbacks)
            .Where(r => r.Employee.TalentManagerId == managerId)
            .OrderByDescending(r => r.ReviewDate)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Returns a single review by Id, including employee info and feedback details.
    /// Returns <c>null</c> if not found.
    /// </summary>
    /// <param name="reviewId">The review Id.</param>
    public async Task<PerformanceReview?> GetByIdAsync(int reviewId)
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e.EmployeeTeammates)
            .Include(r => r.Feedbacks)
                .ThenInclude(f => f.Author)
            .FirstOrDefaultAsync(r => r.Id == reviewId);
    }

    /// <summary>
    /// Schedules a new performance review for the specified employee on the given date.
    /// </summary>
    /// <param name="employeeId">The Id of the employee to be reviewed.</param>
    /// <param name="reviewDate">The scheduled review date (must be in the future).</param>
    /// <returns>The created <see cref="PerformanceReview"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the date is in the past, the employee does not exist,
    /// or the employee has no Talent Manager assigned.
    /// </exception>
    public async Task<PerformanceReview> ScheduleAsync(int employeeId, DateOnly reviewDate)
    {
        if (reviewDate <= DateOnly.FromDateTime(DateTime.Today))
        {
            throw new InvalidOperationException("Review date must be in the future.");
        }

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee is null)
        {
            throw new InvalidOperationException($"Employee with Id {employeeId} not found.");
        }

        if (employee.TalentManagerId is null)
        {
            throw new InvalidOperationException(
                $"Employee '{employee.FullName}' does not have a Talent Manager assigned.");
        }

        var review = new PerformanceReview
        {
            EmployeeId = employeeId,
            ReviewDate = reviewDate,
            Status = ReviewStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        return review;
    }

    /// <summary>
    /// Updates the status of an existing review.
    /// </summary>
    /// <param name="reviewId">The Id of the review to update.</param>
    /// <param name="status">The new status value.</param>
    /// <exception cref="InvalidOperationException">Thrown when the review does not exist.</exception>
    public async Task UpdateStatusAsync(int reviewId, ReviewStatus status)
    {
        var review = await _context.PerformanceReviews.FindAsync(reviewId);

        if (review is null)
        {
            throw new InvalidOperationException($"Review with Id {reviewId} not found.");
        }

        review.Status = status;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Returns employees managed by the specified Talent Manager,
    /// for use in the "Schedule Review" employee dropdown.
    /// </summary>
    /// <param name="managerId">The Id of the Talent Manager.</param>
    public async Task<List<Employee>> GetManagedEmployeesAsync(int managerId)
    {
        return await _context.Employees
            .Where(e => e.TalentManagerId == managerId)
            .OrderBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync();
    }
}
