using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Pure reminder-generation logic. Finds reviews in the reminder window,
/// identifies teammates who haven't submitted feedback, and creates
/// Reminder / Overdue notifications via <see cref="NotificationService"/>.
/// No background-service infrastructure — that is added in Iteration 14.
/// </summary>
/// <example>
/// <code>
/// await reminderService.ProcessAsync(DateTime.UtcNow);
/// </code>
/// </example>
public class ReminderService(
    AppDbContext context,
    NotificationService notificationService,
    ILogger<ReminderService> logger)
{
    private readonly AppDbContext _context = context;
    private readonly NotificationService _notificationService = notificationService;
    private readonly ILogger<ReminderService> _logger = logger;

    /// <summary>
    /// Window in days before the review date during which reminders are sent.
    /// </summary>
    private const int ReminderWindowDays = 14;

    /// <summary>
    /// Threshold in days before the review date at which overdue alerts are sent to the TM.
    /// </summary>
    private const int OverdueThresholdDays = 3;

    /// <summary>
    /// Processes all active reviews and generates reminder/overdue notifications
    /// for teammates who have not yet submitted feedback.
    /// </summary>
    /// <param name="now">The current date/time, passed explicitly for testability.</param>
    /// <returns>The total number of notifications created during this run.</returns>
    public async Task<int> ProcessAsync(DateTime now)
    {
        var today = DateOnly.FromDateTime(now);
        var windowEnd = today.AddDays(ReminderWindowDays);

        // Find all reviews in the reminder window that are not yet Completed.
        var reviews = await _context.PerformanceReviews
            .Include(r => r.Employee)
                .ThenInclude(e => e.EmployeeTeammates)
                    .ThenInclude(et => et.Teammate)
            .Include(r => r.Feedbacks)
            .Where(r => r.Status != ReviewStatus.Completed
                        && r.ReviewDate >= today
                        && r.ReviewDate <= windowEnd)
            .ToListAsync();

        _logger.LogInformation(
            "ReminderService.ProcessAsync: found {Count} review(s) in the {Days}-day window from {Today}.",
            reviews.Count, ReminderWindowDays, today);

        var notificationsCreated = 0;

        foreach (var review in reviews)
        {
            // Get IDs of teammates who already submitted feedback for this review.
            var submittedAuthorIds = review.Feedbacks
                .Select(f => f.AuthorId)
                .ToHashSet();

            // Teammates of the reviewee who haven't submitted feedback yet.
            var pendingTeammates = review.Employee.EmployeeTeammates
                .Select(et => et.Teammate)
                .Where(t => !submittedAuthorIds.Contains(t.Id))
                .ToList();

            var isOverdue = review.ReviewDate <= today.AddDays(OverdueThresholdDays);

            // Send Reminder to each pending teammate (deduplicated per day).
            foreach (var teammate in pendingTeammates)
            {
                var alreadyNotifiedToday = await HasNotificationTodayAsync(
                    teammate.Id, review.Id, NotificationType.Reminder, now);

                if (!alreadyNotifiedToday)
                {
                    await _notificationService.CreateAsync(
                        teammate.Id,
                        review.Id,
                        NotificationType.Reminder,
                        $"Reminder: please submit feedback for {review.Employee.FullName}'s review on {review.ReviewDate}.",
                        now);

                    notificationsCreated++;

                    _logger.LogInformation(
                        "Created Reminder notification for teammate {TeammateId} ({TeammateName}) " +
                        "regarding review {ReviewId} for {EmployeeName}.",
                        teammate.Id, teammate.FullName, review.Id, review.Employee.FullName);
                }
            }

            // Send Overdue to TM if within 3-day window and there are still pending teammates.
            if (isOverdue && pendingTeammates.Count > 0 && review.Employee.TalentManagerId.HasValue)
            {
                var tmId = review.Employee.TalentManagerId.Value;
                var alreadyOverdueToday = await HasNotificationTodayAsync(
                    tmId, review.Id, NotificationType.Overdue, now);

                if (!alreadyOverdueToday)
                {
                    await _notificationService.CreateAsync(
                        tmId,
                        review.Id,
                        NotificationType.Overdue,
                        $"Overdue: {pendingTeammates.Count} teammate(s) have not submitted feedback for " +
                        $"{review.Employee.FullName}'s review on {review.ReviewDate}.",
                        now);

                    notificationsCreated++;

                    _logger.LogInformation(
                        "Created Overdue notification for TM {TmId} regarding review {ReviewId} " +
                        "for {EmployeeName} — {PendingCount} teammate(s) pending.",
                        tmId, review.Id, review.Employee.FullName, pendingTeammates.Count);
                }
            }
        }

        _logger.LogInformation(
            "ReminderService.ProcessAsync completed: {Created} notification(s) created.",
            notificationsCreated);

        return notificationsCreated;
    }

    /// <summary>
    /// Checks whether a notification of the given type for the given recipient and review
    /// was already created today (deduplication).
    /// </summary>
    private async Task<bool> HasNotificationTodayAsync(
        int recipientId,
        int reviewId,
        NotificationType type,
        DateTime now)
    {
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);

        return await _context.Notifications.AnyAsync(n =>
            n.RecipientId == recipientId
            && n.ReviewId == reviewId
            && n.Type == type
            && n.CreatedAt >= todayStart
            && n.CreatedAt < tomorrowStart);
    }
}
