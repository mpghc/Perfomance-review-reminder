using Microsoft.EntityFrameworkCore;
using PerformanceReviewReminderBot.Web.Data;
using PerformanceReviewReminderBot.Web.Entities;

namespace PerformanceReviewReminderBot.Web.Services;

/// <summary>
/// Manages bidirectional teammate relationships between employees.
/// Always inserts/removes both directions in a single SaveChanges call
/// to maintain data consistency.
/// </summary>
public class TeammateService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Returns the list of teammates for the specified employee.
    /// Queries from the EmployeeTeammate table where EmployeeId matches,
    /// returning the Teammate navigation property.
    /// </summary>
    /// <param name="employeeId">The employee whose teammates to retrieve.</param>
    public async Task<List<Employee>> GetTeammatesAsync(int employeeId)
    {
        return await _context.EmployeeTeammates
            .Where(et => et.EmployeeId == employeeId)
            .Include(et => et.Teammate)
            .Select(et => et.Teammate)
            .OrderBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Adds a bidirectional teammate relationship between two employees.
    /// Inserts both (employeeId, teammateId) and (teammateId, employeeId) rows
    /// in a single SaveChanges call.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when: adding self as teammate, duplicate relationship exists,
    /// either employee does not exist, or either employee is not role Employee.
    /// </exception>
    public async Task AddTeammateAsync(int employeeId, int teammateId)
    {
        if (employeeId == teammateId)
        {
            throw new InvalidOperationException("An employee cannot be their own teammate.");
        }

        var employee = await _context.Employees.FindAsync(employeeId);

        if (employee is null)
        {
            throw new InvalidOperationException($"Employee with Id {employeeId} not found.");
        }

        var teammate = await _context.Employees.FindAsync(teammateId);

        if (teammate is null)
        {
            throw new InvalidOperationException($"Employee with Id {teammateId} not found.");
        }

        if (employee.Role != EmployeeRole.Employee)
        {
            throw new InvalidOperationException(
                $"'{employee.FullName}' is a Talent Manager. Only employees can have teammates.");
        }

        if (teammate.Role != EmployeeRole.Employee)
        {
            throw new InvalidOperationException(
                $"'{teammate.FullName}' is a Talent Manager. Only employees can be added as teammates.");
        }

        var exists = await _context.EmployeeTeammates
            .AnyAsync(et => et.EmployeeId == employeeId && et.TeammateId == teammateId);

        if (exists)
        {
            throw new InvalidOperationException(
                $"'{teammate.FullName}' is already a teammate of '{employee.FullName}'.");
        }

        // Insert both directions in a single SaveChanges call for consistency.
        _context.EmployeeTeammates.Add(new EmployeeTeammate
        {
            EmployeeId = employeeId,
            TeammateId = teammateId
        });
        _context.EmployeeTeammates.Add(new EmployeeTeammate
        {
            EmployeeId = teammateId,
            TeammateId = employeeId
        });

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a bidirectional teammate relationship between two employees.
    /// Removes both (employeeId, teammateId) and (teammateId, employeeId) rows
    /// in a single SaveChanges call.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the relationship does not exist.
    /// </exception>
    public async Task RemoveTeammateAsync(int employeeId, int teammateId)
    {
        var forward = await _context.EmployeeTeammates
            .FirstOrDefaultAsync(et => et.EmployeeId == employeeId && et.TeammateId == teammateId);

        var reverse = await _context.EmployeeTeammates
            .FirstOrDefaultAsync(et => et.EmployeeId == teammateId && et.TeammateId == employeeId);

        if (forward is null && reverse is null)
        {
            throw new InvalidOperationException(
                $"No teammate relationship found between employees {employeeId} and {teammateId}.");
        }

        // Remove both directions in a single SaveChanges call for consistency.
        if (forward is not null)
        {
            _context.EmployeeTeammates.Remove(forward);
        }

        if (reverse is not null)
        {
            _context.EmployeeTeammates.Remove(reverse);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Returns employees eligible to be added as teammates of the specified employee:
    /// role = Employee, not self, and not already a teammate.
    /// </summary>
    public async Task<List<Employee>> GetEligibleTeammatesAsync(int employeeId)
    {
        var currentTeammateIds = await _context.EmployeeTeammates
            .Where(et => et.EmployeeId == employeeId)
            .Select(et => et.TeammateId)
            .ToListAsync();

        return await _context.Employees
            .Where(e => e.Role == EmployeeRole.Employee
                        && e.Id != employeeId
                        && !currentTeammateIds.Contains(e.Id))
            .OrderBy(e => e.FullName)
            .AsNoTracking()
            .ToListAsync();
    }
}
