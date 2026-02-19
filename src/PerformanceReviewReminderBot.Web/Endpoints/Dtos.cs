namespace PerformanceReviewReminderBot.Web.Endpoints;

/// <summary>
/// Request body for creating or updating an employee.
/// </summary>
/// <param name="FullName">Full name of the employee. Required.</param>
/// <param name="Email">Email address of the employee. Required.</param>
/// <param name="Role">Role within the organization (0 = Employee, 1 = TalentManager).</param>
/// <param name="TalentManagerId">
/// FK to the Talent Manager who manages this employee. Null for Talent Managers.
/// </param>
public record EmployeeRequest(
    string FullName,
    string Email,
    int Role,
    int? TalentManagerId);

/// <summary>
/// Response DTO for an employee. Flattens the TalentManager navigation
/// to <see cref="TalentManagerName"/> to avoid leaking EF navigation properties.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="FullName">Full name.</param>
/// <param name="Email">Email address.</param>
/// <param name="Role">Role within the organization.</param>
/// <param name="TalentManagerName">Name of the TM, or null if not assigned.</param>
public record EmployeeResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    string? TalentManagerName);

/// <summary>
/// Lightweight response DTO for a teammate.
/// </summary>
/// <param name="Id">Employee primary key.</param>
/// <param name="FullName">Full name.</param>
/// <param name="Email">Email address.</param>
public record TeammateResponse(
    int Id,
    string FullName,
    string Email);

// ── Review DTOs ──────────────────────────────────────────────

/// <summary>
/// Request body for scheduling a new performance review.
/// </summary>
/// <param name="EmployeeId">The Id of the employee to be reviewed.</param>
/// <param name="ReviewDate">Scheduled review date (yyyy-MM-dd).</param>
public record ReviewRequest(
    int EmployeeId,
    string ReviewDate);

/// <summary>
/// Response DTO for a performance review, including feedback progress.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="EmployeeName">Name of the employee being reviewed.</param>
/// <param name="ReviewDate">Scheduled review date.</param>
/// <param name="Status">Current review status.</param>
/// <param name="FeedbackSubmitted">Number of feedback submissions received.</param>
/// <param name="FeedbackExpected">Total number of teammates expected to submit feedback.</param>
public record ReviewResponse(
    int Id,
    string EmployeeName,
    string ReviewDate,
    string Status,
    int FeedbackSubmitted,
    int FeedbackExpected);

/// <summary>
/// Request body for updating a review's status.
/// </summary>
/// <param name="Status">New status value (Scheduled, InProgress, Completed).</param>
public record StatusUpdateRequest(
    string Status);

// ── Feedback DTOs ────────────────────────────────────────────

/// <summary>
/// Request body for submitting peer feedback.
/// </summary>
/// <param name="AuthorId">The Id of the teammate submitting the feedback.</param>
/// <param name="Content">Text content of the feedback.</param>
public record FeedbackRequest(
    int AuthorId,
    string Content);

/// <summary>
/// Response DTO for a feedback submission.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="AuthorName">Name of the feedback author.</param>
/// <param name="Content">Text content.</param>
/// <param name="SubmittedAt">Timestamp when the feedback was submitted.</param>
public record FeedbackResponse(
    int Id,
    string AuthorName,
    string Content,
    DateTime SubmittedAt);

// ── Notification DTOs ────────────────────────────────────────

/// <summary>
/// Response DTO for a notification.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Message">Human-readable notification message.</param>
/// <param name="Type">Notification type (Reminder or Overdue).</param>
/// <param name="IsRead">Whether the notification has been read.</param>
/// <param name="CreatedAt">Timestamp when the notification was created.</param>
public record NotificationResponse(
    int Id,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt);
