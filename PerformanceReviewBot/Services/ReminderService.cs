using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Models;

namespace PerformanceReviewBot.Services;

public class ReminderService : IReminderService
{
    private readonly AppDbContext _context;

    public ReminderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PerformanceReview>> GetAllAsync()
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .OrderBy(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<PerformanceReview?> GetByIdAsync(int id)
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task CreateAsync(PerformanceReview review)
    {
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PerformanceReview review)
    {
        _context.PerformanceReviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review != null)
        {
            _context.PerformanceReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PerformanceReview>> GetReviewsThisMonthAsync()
    {
        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Where(r => r.ReviewDate >= startOfMonth && r.ReviewDate <= endOfMonth)
            .OrderBy(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<List<PerformanceReview>> GetOverdueReviewsAsync()
    {
        return await _context.PerformanceReviews
            .Include(r => r.Employee)
            .Where(r => r.Status == ReviewStatus.Overdue)
            .OrderBy(r => r.ReviewDate)
            .ToListAsync();
    }
}
