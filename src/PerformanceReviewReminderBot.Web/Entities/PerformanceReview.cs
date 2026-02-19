namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Represents a scheduled performance review for an employee.
/// </summary>
public class PerformanceReview
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>FK to the employee being reviewed.</summary>
    public int EmployeeId { get; set; }

    /// <summary>Navigation property to the employee being reviewed.</summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>Scheduled date of the review.</summary>
    public DateOnly ReviewDate { get; set; }

    /// <summary>Current status of the review lifecycle.</summary>
    public ReviewStatus Status { get; set; }

    /// <summary>Timestamp when the review was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Feedback submissions for this review.</summary>
    public ICollection<Feedback> Feedbacks { get; set; } = [];

    /// <summary>Notifications related to this review.</summary>
    public ICollection<Notification> Notifications { get; set; } = [];
}
