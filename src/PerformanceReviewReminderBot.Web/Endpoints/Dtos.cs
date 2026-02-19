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
