namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Represents a notification sent to an employee, optionally linked to a performance review.
/// </summary>
public class Notification
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>FK to the employee who receives this notification.</summary>
    public int RecipientId { get; set; }

    /// <summary>Navigation property to the recipient employee.</summary>
    public Employee Recipient { get; set; } = null!;

    /// <summary>FK to the related performance review. Nullable for system messages.</summary>
    public int? ReviewId { get; set; }

    /// <summary>Navigation property to the related performance review.</summary>
    public PerformanceReview? Review { get; set; }

    /// <summary>The type of notification (Reminder or Overdue).</summary>
    public NotificationType Type { get; set; }

    /// <summary>Human-readable notification message text.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Whether the notification has been read by the recipient.</summary>
    public bool IsRead { get; set; }

    /// <summary>Timestamp when the notification was created.</summary>
    public DateTime CreatedAt { get; set; }
}
