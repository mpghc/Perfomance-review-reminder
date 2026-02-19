namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Defines the role of an employee within the organization.
/// </summary>
public enum EmployeeRole
{
    /// <summary>Employee who participates in reviews and provides feedback.</summary>
    Employee,

    /// <summary>Talent Manager who schedules and oversees performance reviews.</summary>
    TalentManager
}

/// <summary>
/// Represents the lifecycle status of a performance review.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Review is scheduled but has not started yet.</summary>
    Scheduled,

    /// <summary>Feedback collection is in progress.</summary>
    InProgress,

    /// <summary>All feedback collected and review is finalized.</summary>
    Completed
}

/// <summary>
/// Categorizes the type of notification sent to an employee.
/// </summary>
public enum NotificationType
{
    /// <summary>A reminder to submit feedback before the deadline.</summary>
    Reminder,

    /// <summary>A notification that the feedback deadline has passed.</summary>
    Overdue
}
