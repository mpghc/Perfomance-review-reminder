namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Represents feedback submitted by a teammate for a performance review.
/// Each teammate may submit only one feedback per review (unique on ReviewId + AuthorId).
/// </summary>
public class Feedback
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>FK to the performance review this feedback belongs to.</summary>
    public int ReviewId { get; set; }

    /// <summary>Navigation property to the performance review.</summary>
    public PerformanceReview Review { get; set; } = null!;

    /// <summary>FK to the employee who authored this feedback.</summary>
    public int AuthorId { get; set; }

    /// <summary>Navigation property to the feedback author.</summary>
    public Employee Author { get; set; } = null!;

    /// <summary>The text content of the feedback.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Timestamp when the feedback was submitted.</summary>
    public DateTime SubmittedAt { get; set; }
}
