namespace PerformanceReviewReminderBot.Web.Entities;

/// <summary>
/// Represents an employee in the organization.
/// Can be either a Talent Manager or a regular Employee.
/// </summary>
public class Employee
{
    /// <summary>Primary key, auto-incremented.</summary>
    public int Id { get; set; }

    /// <summary>Full name of the employee. Required, max 200 characters.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email address of the employee. Required, max 200 characters.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Role within the organization (TalentManager or Employee).</summary>
    public EmployeeRole Role { get; set; }

    /// <summary>
    /// FK to the Talent Manager who manages this employee.
    /// Null for Talent Managers themselves.
    /// </summary>
    public int? TalentManagerId { get; set; }

    /// <summary>Navigation property to the Talent Manager.</summary>
    public Employee? TalentManager { get; set; }

    /// <summary>Employees managed by this Talent Manager.</summary>
    public ICollection<Employee> ManagedEmployees { get; set; } = [];

    /// <summary>Performance reviews for this employee.</summary>
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = [];

    /// <summary>Notifications received by this employee.</summary>
    public ICollection<Notification> Notifications { get; set; } = [];

    /// <summary>Teammate relationships where this employee is the "Employee" side.</summary>
    public ICollection<EmployeeTeammate> EmployeeTeammates { get; set; } = [];

    /// <summary>Teammate relationships where this employee is the "Teammate" side.</summary>
    public ICollection<EmployeeTeammate> TeammateOf { get; set; } = [];
}
