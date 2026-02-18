using Microsoft.EntityFrameworkCore;
using PerformanceReviewBot.Data;
using PerformanceReviewBot.Data.Entities;

namespace PerformanceReviewBot.Services;

public class ReminderService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(AppDbContext context, ILogger<ReminderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessCurrentMonthRemindersAsync()
    {
        _logger.LogInformation("Starting reminder processing for current month reviews...");

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get current month reviews that need attention
        var reviews = await _context.PerformanceReviews
            .Include(pr => pr.Employee)
            .ThenInclude(e => e.Manager)
            .Include(pr => pr.Feedbacks)
            .Where(pr => pr.ReviewDate >= startOfMonth && pr.ReviewDate <= endOfMonth)
            .Where(pr => pr.Status == ReviewStatus.Scheduled || pr.Status == ReviewStatus.InProgress)
            .ToListAsync();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var review in reviews)
            {
                await CheckAndNotifyReviewDueAsync(review);
                await CheckAndNotifyMissingFeedbackAsync(review);
            }

            await transaction.CommitAsync();
            _logger.LogInformation("Reminder processing completed. Processed {Count} reviews.", reviews.Count);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing reminders. Transaction rolled back.");
            throw;
        }
    }

    private async Task CheckAndNotifyReviewDueAsync(PerformanceReview review)
    {
        var daysUntilReview = (review.ReviewDate - DateTime.UtcNow).Days;

        // Send reminder if review is within 7 days
        if (daysUntilReview <= 7 && daysUntilReview >= 0)
        {
            var message = $"Reminder: Performance review scheduled for {review.Employee.FullName} on {review.ReviewDate:yyyy-MM-dd}";
            await SendReminderAsync(review.Employee, review, ReminderType.ReviewDue, message);
            
            // Notify manager
            if (review.Employee.Manager != null)
            {
                await SendReminderAsync(review.Employee.Manager, review, ReminderType.ReviewDue, message);
            }
        }
    }

    private async Task CheckAndNotifyMissingFeedbackAsync(PerformanceReview review)
    {
        // Check if review is in progress or past due
        if (review.Status == ReviewStatus.InProgress || review.ReviewDate < DateTime.UtcNow)
        {
            var hasManagerFeedback = review.Feedbacks.Any(f => f.IsManagerFeedback);
            
            if (!hasManagerFeedback && review.Employee.Manager != null)
            {
                var message = $"Missing manager feedback for {review.Employee.FullName}'s performance review (Due: {review.ReviewDate:yyyy-MM-dd})";
                await SendReminderAsync(review.Employee.Manager, review, ReminderType.FeedbackMissing, message);
            }
        }
    }

    private async Task SendReminderAsync(Employee employee, PerformanceReview review, ReminderType type, string message)
    {
        // Check if reminder already sent today
        var today = DateTime.UtcNow.Date;
        var alreadySent = await _context.ReminderLogs
            .AnyAsync(rl => rl.PerformanceReviewId == review.Id
                && rl.EmployeeId == employee.Id
                && rl.ReminderType == type
                && rl.SentDate.Date == today);

        if (alreadySent)
        {
            return;
        }

        var reminderLog = new ReminderLog
        {
            PerformanceReviewId = review.Id,
            EmployeeId = employee.Id,
            ReminderType = type,
            Message = message,
            SentDate = DateTime.UtcNow
        };

        _context.ReminderLogs.Add(reminderLog);
        await _context.SaveChangesAsync();

        // Simulate sending notification
        _logger.LogInformation("REMINDER SENT - To: {Email}, Type: {Type}, Message: {Message}",
            employee.Email, type, message);
    }

    public async Task<List<ReminderLog>> GetReminderLogsAsync()
    {
        return await _context.ReminderLogs
            .Include(rl => rl.Employee)
            .Include(rl => rl.PerformanceReview)
            .ThenInclude(pr => pr.Employee)
            .OrderByDescending(rl => rl.SentDate)
            .ToListAsync();
    }

    public async Task<List<ReminderLog>> GetReminderLogsByEmployeeAsync(int employeeId)
    {
        return await _context.ReminderLogs
            .Include(rl => rl.PerformanceReview)
            .ThenInclude(pr => pr.Employee)
            .Where(rl => rl.EmployeeId == employeeId)
            .OrderByDescending(rl => rl.SentDate)
            .ToListAsync();
    }
}
