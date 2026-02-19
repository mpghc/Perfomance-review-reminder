namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Join table for the bidirectional many-to-many teammate relationship between employees.
/// Both (EmployeeId, TeammateId) and (TeammateId, EmployeeId) rows are stored
/// to ensure consistency when querying from either side.
/// </summary>
public class EmployeeTeammate
{
    /// <summary>FK to the employee on one side of the relationship.</summary>
    public int EmployeeId { get; set; }

    /// <summary>Navigation property to the employee.</summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>FK to the teammate on the other side of the relationship.</summary>
    public int TeammateId { get; set; }

    /// <summary>Navigation property to the teammate.</summary>
    public Employee Teammate { get; set; } = null!;
}
