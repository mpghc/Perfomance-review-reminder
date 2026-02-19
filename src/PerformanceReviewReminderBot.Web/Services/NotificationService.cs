using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Provides operations for creating, querying, and managing notifications.
/// Uses AppDbContext directly (no repository abstraction per project rules).
/// </summary>
public class NotificationService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Returns all notifications for the specified recipient, ordered by date descending.
    /// Includes the related Review navigation property when present.
    /// </summary>
    /// <param name="recipientId">The Id of the recipient employee.</param>
    /// <example>
    /// <code>
    /// var notifications = await notificationService.GetByRecipientAsync(currentUser.Id);
    /// </code>
    /// </example>
    public async Task<List<Notification>> GetByRecipientAsync(int recipientId)
    {
        return await _context.Notifications
            .Include(n => n.Review)
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Returns the count of unread notifications for the specified recipient.
    /// </summary>
    /// <param name="recipientId">The Id of the recipient employee.</param>
    public async Task<int> GetUnreadCountAsync(int recipientId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientId == recipientId && !n.IsRead);
    }

    /// <summary>
    /// Marks the specified notification as read.
    /// </summary>
    /// <param name="notificationId">The Id of the notification to mark as read.</param>
    /// <exception cref="InvalidOperationException">Thrown when the notification does not exist.</exception>
    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        if (notification is null)
        {
            throw new InvalidOperationException($"Notification with Id {notificationId} not found.");
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a new notification for the specified recipient.
    /// </summary>
    /// <param name="recipientId">The Id of the recipient employee.</param>
    /// <param name="reviewId">Optional FK to the related performance review.</param>
    /// <param name="type">The type of notification (Reminder or Overdue).</param>
    /// <param name="message">The human-readable notification message.</param>
    /// <returns>The created <see cref="Notification"/> entity.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the recipient does not exist or the message is empty.
    /// </exception>
    public async Task<Notification> CreateAsync(
        int recipientId,
        int? reviewId,
        NotificationType type,
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Notification message is required.");
        }

        var recipientExists = await _context.Employees.AnyAsync(e => e.Id == recipientId);

        if (!recipientExists)
        {
            throw new InvalidOperationException($"Recipient with Id {recipientId} not found.");
        }

        var notification = new Notification
        {
            RecipientId = recipientId,
            ReviewId = reviewId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }
}
