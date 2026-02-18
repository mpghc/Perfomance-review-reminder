using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Services;

public class PerformanceReviewService
{
    private readonly AppDbContext _context;

    public PerformanceReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PerformanceReview>> GetAllReviewsAsync()
    {
        return await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .ThenInclude(e => e.Manager)
            .Include(pr => pr.Feedbacks)
            .OrderByDescending(pr => pr.ReviewDate)
            .ToListAsync();
    }

    public async Task<PerformanceReview?> GetReviewByIdAsync(int id)
    {
        return await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .ThenInclude(e => e.Manager)
            .Include(pr => pr.Feedbacks)
            .ThenInclude(f => f.Reviewer)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<List<PerformanceReview>> GetReviewsByEmployeeAsync(int employeeId)
    {
        return await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .Include(pr => pr.Feedbacks)
            .Where(pr => pr.EmployeeId == employeeId)
            .OrderByDescending(pr => pr.ReviewDate)
            .ToListAsync();
    }

    public async Task<List<PerformanceReview>> GetCurrentMonthReviewsAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .ThenInclude(e => e.Manager)
            .Include(pr => pr.Feedbacks)
            .Where(pr => pr.ReviewDate >= startOfMonth && pr.ReviewDate <= endOfMonth)
            .Where(pr => pr.Status == ReviewStatus.Scheduled || pr.Status == ReviewStatus.InProgress)
            .OrderBy(pr => pr.ReviewDate)
            .ToListAsync();
    }

    public async Task<PerformanceReview> ScheduleReviewAsync(PerformanceReview review)
    {
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<PerformanceReview> UpdateReviewStatusAsync(int id, ReviewStatus status)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null)
        {
            throw new InvalidOperationException($"Review with ID {id} not found.");
        }

        review.Status = status;
        if (status == ReviewStatus.Completed)
        {
            review.CompletedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<List<PerformanceReview>> GetReviewsWithMissingFeedbackAsync()
    {
        var currentMonthReviews = await GetCurrentMonthReviewsAsync();
        
        return currentMonthReviews
            .Where(pr => pr.Status == ReviewStatus.InProgress || pr.ReviewDate < DateTime.UtcNow)
            .Where(pr => !pr.Feedbacks.Any(f => f.IsManagerFeedback))
            .ToList();
    }
}
