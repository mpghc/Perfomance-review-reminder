using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Tracks the currently "logged-in" user for the active Blazor circuit.
/// Registered as scoped so each circuit/session has its own instance.
/// Defaults to the first Talent Manager (Bill) on initialization.
/// </summary>
public class CurrentUserService
{
    private readonly AppDbContext _context;

    /// <summary>Id of the currently selected user.</summary>
    public int CurrentUserId { get; private set; }

    /// <summary>Full name of the currently selected user.</summary>
    public string CurrentUserName { get; private set; } = string.Empty;

    /// <summary>Role of the currently selected user.</summary>
    public EmployeeRole CurrentUserRole { get; private set; }

    /// <summary>
    /// Raised when the current user changes so UI components can refresh.
    /// </summary>
    public event Action? OnChange;

    public CurrentUserService(AppDbContext context)
    {
        _context = context;
        InitializeDefault();
    }

    /// <summary>
    /// Switches the current user to the specified employee.
    /// Loads the employee from the database and caches the values.
    /// </summary>
    /// <param name="employeeId">The Id of the employee to switch to.</param>
    public async Task SetCurrentUserAsync(int employeeId)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == employeeId);

        if (employee is null)
        {
            return;
        }

        CurrentUserId = employee.Id;
        CurrentUserName = employee.FullName;
        CurrentUserRole = employee.Role;
        OnChange?.Invoke();
    }

    /// <summary>
    /// Loads the default user (first Talent Manager) on service creation.
    /// </summary>
    private void InitializeDefault()
    {
        var defaultUser = _context.Employees
            .AsNoTracking()
            .FirstOrDefault(e => e.Role == EmployeeRole.TalentManager);

        if (defaultUser is not null)
        {
            CurrentUserId = defaultUser.Id;
            CurrentUserName = defaultUser.FullName;
            CurrentUserRole = defaultUser.Role;
        }
    }
}
